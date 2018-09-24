/*
 * Copyright (c) 2018 Udezue Chukwunwike izarchtechnologies@hotmail.com
 * This Source Code Form is subject to the terms of the Mozilla Public License
 * v. 2.0. If a copy of the MPL was not distributed with this file, You can
 * obtain one at http://mozilla.org/MPL/2.0/
 * Neither the name of Udezue Chukwunwike nor the names of its contributors may
 * be used to endorse or promote products derived from this software without
 * specific prior written permission.
 */


using System.Text;
using System.Globalization;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace BigNumberX
{
    /// <summary>
    /// Extended Precision Integer.
    /// </summary>
    /// <remarks>
    /// <para>Inspired by the Microsoft.Scripting.Math.BigInteger code,
    /// the java.math.BigInteger code, but mostly by Don Knuth's Art of Computer Programming, Volume 2.</para>
    /// <para>The same as most other BigInteger representations, this implementation uses a sign/magnitude representation.</para>
    /// <para>The magnitude is represented by an array of UInt in big-endian order.
    /// <para>IntegerX's are immutable.</para>
    /// </remarks>
    public class IntegerX
    {
        #region PRIVATE VARIABLES
        /// <summary>
        /// <see cref="CultureInfo" /> used in <see cref="IntegerX" />.
        /// </summary>
        private static CultureInfo _FS;

        /// <summary>
        /// Temporary Variable to Hold <c>Zero </c><see cref="IntegerX" />.
        /// </summary>
        private static IntegerX ZeroX;

        /// <summary>
        /// Temporary Variable to Hold <c>One </c><see cref="IntegerX" />.
        /// </summary>
        private static IntegerX OneX;

        /// <summary>
        /// Temporary Variable to Hold <c>Two </c><see cref="IntegerX" />.
        /// </summary>
        private static IntegerX TwoX;

        /// <summary>
        /// Temporary Variable to Hold <c>Five </c><see cref="IntegerX" />.
        /// </summary>
        private static IntegerX FiveX;

        /// <summary>
        /// Temporary Variable to Hold <c>Ten </c><see cref="IntegerX" />.
        /// </summary>
        private static IntegerX TenX;

        /// <summary>
        /// Temporary Variable to Hold <c>NegativeOne </c><see cref="IntegerX" />.
        /// </summary>
        private static IntegerX NegativeOneX;

        /// <summary>
        /// The sign of the integer.  Must be -1, 0, +1.
        /// </summary>
        private short _sign;

        /// <summary>
        /// The magnitude of the integer (big-endian).
        /// </summary>
        /// <remarks>
        /// <para>Big-endian = _data[0] is the most significant digit.</para>
        /// <para> Some invariants:</para>
        /// <list>
        /// <item>If the integer is zero, then _data must be length zero array and _sign must be zero.</item>
        /// <item>No leading zero UInt.</item>
        /// <item>Must be non-null.  For zero, a zero-length array is used.</item>
        /// </list>
        /// These invariants imply a unique representation for every value.
        /// They also force us to get rid of leading zeros after every operation that might create some.
        /// </remarks>
        private uint[] _data;

        #endregion

        #region CONST VARIABLES
        /// <summary>
        /// The number of bits in one 'digit' of the magnitude.
        /// </summary>
        private const uint BitsPerDigit = 32; // UInt implementation

        /// <summary>
        /// Exponent bias in the 64-bit floating point representation.
        /// </summary>
        private const int DoubleExponentBias = 1023;

        /// <summary>
        /// The size in bits of the significand in the 64-bit floating point representation.
        /// </summary>
        private const int DoubleSignificandBitLength = 52;

        /// <summary>
        /// How much to shift to accommodate the exponent and the binary digits of the significand.
        /// </summary>
        private const int DoubleShiftBias = DoubleExponentBias + DoubleSignificandBitLength;

        /// <summary>
        /// The minimum radix allowed in parsing.
        /// </summary>
        private const int MinRadix = 2;

        /// <summary>
        /// The maximum radix allowed in parsing.
        /// </summary>
        private const int MaxRadix = 36;

        /// <summary>
        /// Max uint value.
        /// </summary>
        private const uint MaxUIntValue = 4294967295;

        /// <summary>
        /// Min short value.
        /// </summary>
        private const short MinShortValue = -32768;

        /// <summary>
        /// Max short value.
        /// </summary>
        private const short MaxShortValue = 32767;

        /// <summary>
        /// Min sbyte value.
        /// </summary>
        private const sbyte MinSByteValue = -128;

        /// <summary>
        /// Max sbyte value.
        /// </summary>
        private const sbyte MaxSByteValue = 127;

        /// <summary>
        /// Max ushort value.
        /// </summary>
        private const ushort MaxUShortValue = 65535;

        #endregion

        #region PRIVATE STATIC VARIABLES
        /// <summary>
        /// UIntLogTable
        /// </summary>
        private static uint[] UIntLogTable;

        /// <summary>
        /// The maximum number of digits in radix[i] that will fit into a UInt.
        /// </summary>
        /// <remarks>
        /// <para>RadixDigitsPerDigit[i] = floor(log_i (2^32 - 1))</para>
        /// </remarks>
        private static int[] RadixDigitsPerDigit;

        /// <summary>
        /// The super radix (power of given radix) that fits into a UInt.
        /// </summary>
        /// <remarks>
        /// <para>SuperRadix[i] = 2 ^ RadixDigitsPerDigit[i]</para>
        /// </remarks>
        private static uint[] SuperRadix;

        /// <summary>
        /// The number of bits in one digit of radix[i] times 1024.
        /// </summary>
        /// <remarks>
        /// <para>BitsPerRadixDigit[i] = ceiling(1024*log_2(i))</para>
        /// <para>The value is multiplied by 1024 to avoid fractions.  Users will need to divide by 1024.</para>
        /// </remarks>
        private static int[] BitsPerRadixDigit;

        /// <summary>
        /// The value at index i is the number of trailing zero bits in the value i.
        /// </summary>
        private static byte[] TrailingZerosTable;

        #endregion

        #region PROPS
        /// <summary>
        /// Support for DecimalX, to compute precision
        /// </summary>
        public uint Precision
        {
            get
            {
                uint digits;
                uint[] work;
                int index;

                if (IsZero())
                {
                    return 1;
                }

                digits = 0;
                work = GetMagnitude();
                index = 0;

                while (index < work.Length - 1)
                {
                    InPlaceDivRem(ref work, ref index, 1000000000);
                    digits += 9;
                }

                if (index == work.Length - 1)
                {
                    digits += UIntPrecision(work[index]);
                }

                return digits;
            }
        }

        /// <summary>
        /// A Zero.
        /// </summary>
        public static IntegerX Zero
        {
            get
            {
                return ZeroX;
            }
        }

        /// <summary>
        /// A Positive One.
        /// </summary>
        public static IntegerX One {
             get
            {
                return OneX;
            }
        }

        /// <summary>
        /// A Two.
        /// </summary>
        public static IntegerX Two
        {
            get
            {
                return TwoX;
            }
        }

        /// <summary>
        /// A Five.
        /// </summary>
        public static IntegerX Five
        {
            get
            {
                return FiveX;
            }
        }

        /// <summary>
        /// A Ten.
        /// </summary>
        public static IntegerX Ten
        {
            get
            {
                return TenX;
            }
        }

        /// <summary>
        /// A Negative One.
        /// </summary>
        public static IntegerX NegativeOne
        {
            get
            {
                return NegativeOneX;
            }
        }
        #endregion

        #region STATIC CONSTRUCTOR
        static IntegerX()
        {
            // Create a Zero IntegerX (a big integer with value as Zero)
            ZeroX = new IntegerX(0, new uint[0]);

            // Create a One IntegerX (a big integer with value as One)
            OneX = new IntegerX(1, new uint[] { 1 });

            // Create a Two IntegerX (a big integer with value as Two)
            TwoX = new IntegerX(1, new uint[] { 2 });

            // Create a Five IntegerX (a big integer with value as Five)
            FiveX = new IntegerX(1, new uint[] { 5 });

            // Create a Ten IntegerX (a big integer with value as Ten)
            TenX = new IntegerX(1, new uint[] { 10 });

            // Create a NegativeOne IntegerX (a big integer with value as NegativeOne)
            NegativeOneX = new IntegerX(-1, new uint[] { 1 });

            _FS = CultureInfo.CurrentCulture;

            UIntLogTable = new uint[] { 0, 9, 99, 999, 9999, 99999, 999999, 9999999, 99999999, 999999999, MaxUIntValue };

            RadixDigitsPerDigit = new int[] { 0, 0, 31, 20, 15, 13, 12, 11, 10, 10, 9, 9, 8, 8, 8, 8, 7, 7, 7, 7, 7, 7,
                7, 7, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6 };

            SuperRadix = new uint[] { 0, 0, 0x80000000, 0xCFD41B91, 0x40000000, 0x48C27395, 0x81BF1000, 0x75DB9C97,
                0x40000000, 0xCFD41B91, 0x3B9ACA00, 0x8C8B6D2B, 0x19A10000, 0x309F1021, 0x57F6C100, 0x98C29B81,
                0x10000000, 0x18754571, 0x247DBC80, 0x3547667B, 0x4C4B4000, 0x6B5A6E1D, 0x94ACE180, 0xCAF18367,
                0xB640000, 0xE8D4A51, 0x1269AE40, 0x17179149, 0x1CB91000, 0x23744899, 0x2B73A840, 0x34E63B41, 0x40000000,
                0x4CFA3CC1, 0x5C13D840, 0x6D91B519, 0x81BF1000};

            BitsPerRadixDigit = new int[] { 0, 0, 1024, 1624, 2048, 2378, 2648, 2875, 3072, 3247, 3402, 3543, 3672, 3790,
                3899, 4001, 4096, 4186, 4271, 4350, 4426, 4498, 4567, 4633, 4696, 4756, 4814, 4870, 4923, 4975, 5025, 5074,
                5120, 5166, 5210, 5253, 5295 };

            TrailingZerosTable = new byte[] { 0, 0, 1, 0, 2, 0, 1, 0, 3, 0, 1, 0, 2, 0, 1, 0, 4, 0, 1, 0, 2, 0, 1, 0, 3, 0, 1, 0, 2, 0, 1, 0, 5, 0, 1, 0, 2,
                0, 1, 0, 3, 0, 1, 0, 2, 0, 1, 0, 4, 0, 1, 0, 2, 0, 1, 0, 3, 0, 1, 0, 2, 0, 1, 0, 6, 0, 1, 0, 2, 0, 1, 0, 3, 0, 1, 0, 2, 0, 1, 0, 4, 0, 1, 0, 2, 0, 1,
                0, 3, 0, 1, 0, 2, 0, 1, 0, 5, 0, 1, 0, 2, 0, 1, 0, 3, 0, 1, 0, 2, 0, 1, 0, 4, 0, 1, 0, 2, 0, 1, 0, 3, 0, 1, 0, 2, 0, 1, 0, 7, 0, 1, 0, 2, 0, 1, 0, 3,
                0, 1, 0, 2, 0, 1, 0, 4, 0, 1, 0, 2, 0, 1, 0, 3, 0, 1, 0, 2, 0, 1, 0, 5, 0, 1, 0, 2, 0, 1, 0, 3, 0, 1, 0, 2, 0, 1, 0, 4, 0, 1, 0, 2, 0, 1, 0, 3, 0, 1,
                0, 2, 0, 1, 0, 6, 0, 1, 0, 2, 0, 1, 0, 3, 0, 1, 0, 2, 0, 1, 0, 4, 0, 1, 0, 2, 0, 1, 0, 3, 0, 1, 0, 2, 0, 1, 0, 5, 0, 1, 0, 2, 0, 1, 0, 3, 0, 1, 0, 2,
                0, 1, 0, 4, 0, 1, 0, 2, 0, 1, 0, 3, 0, 1, 0, 2, 0, 1, 0 };


        }
        #endregion

        #region PUBLIC CONSTRUCTORS
        /// <summary>
        /// Creates a copy of a <see cref="IntegerX"/>.
        /// </summary>
        /// <param name="copy">The <see cref="IntegerX"/> to copy.</param>
        public IntegerX(IntegerX copy)
        {
            _sign = copy._sign;
            _data = copy._data;
        }

        public IntegerX(byte[] val)
        {
            if (val.Length == 0)
            {
                throw new Exception("Zero length BigInteger");
            }

            if ((sbyte)val[0] < 0)
            {
                _data = makePositive(val);
                _sign = -1;
            }
            else
            {
                _data = StripLeadingZeroBytes(val);
                if (_data.Length == 0)
                {
                    _sign = 0;
                }
                else
                {
                    _sign = 1;
                }
            }
        }

        /// <summary>
        /// Creates a <see cref="IntegerX"/> from sign/magnitude data.
        /// </summary>
        /// <param name="sign">The sign (-1, 0, +1)</param>
        /// <param name="data">The magnitude (big-endian)</param>
        /// <exception cref="ArgumentException">Thrown when the sign is not one of -1, 0, +1,
        /// or if a zero sign is given on a non-empty magnitude.</exception>
        /// <remarks>
        /// <para>Leading zero (UInt) digits will be removed.</para>
        /// <para>The sign will be set to zero if a zero-length array is passed.</para>
        /// </remarks>
        public IntegerX(int sign, uint[] data)
        {
            if ((sign < -1) || (sign > 1))
            {
                throw new ArgumentException("Sign must be -1, 0, or +1'");
            }

            data = RemoveLeadingZeros(data);
            if (data.Length == 0)
            {
                sign = 0;
            }
            else if (sign == 0)
            {
                throw new ArgumentException("Zero sign on non-zero data");
            }

            _sign = (short)sign;
            _data = data;
        }

        #endregion

        #region CREATE OVERLOADS

        /// <summary>
        /// Create a <see cref="IntegerX"/> from an unsigned long value.
        /// </summary>
        /// <param name="v">The value</param>
        /// <returns>A <see cref="IntegerX"/>.</returns>
        public static IntegerX Create(ulong v)
        {
            uint most;

            if (v == 0)
                return Zero;

            most = (uint)(v >> (int)BitsPerDigit);
            if (most == 0)
            {
                return new IntegerX(1, new uint[]{ (uint)v });
            }
            else
            {
                return new IntegerX(1, new uint[] { most, (uint)v });
            }

        }

        /// <summary>
        /// Create a <see cref="IntegerX"/> from an unsigned integer value.
        /// </summary>
        /// <param name="v">The value</param>
        /// <returns>A <see cref="IntegerX"/>.</returns>
        public static IntegerX Create(uint v)
        {
            if (v == 0)
                return Zero;
            else
                return new IntegerX(1, new uint[] { v });
        }

        /// <summary>
        /// Create a <see cref="IntegerX"/> from an (signed) Long value.
        /// </summary>
        /// <param name="v">The value</param>
        /// <returns>A <see cref="IntegerX"/>.</returns>
        public static IntegerX Create(long v)
        {
            uint most;
            short Sign;

            if (v == 0)
            {
                return Zero;
            }
            else
            {
                Sign = 1;
                if (v < 0)
                {
                    Sign = -1;
                    v = -v;
                }
                
                most = (uint)(v >> (int)BitsPerDigit);
                if (most == 0)
                {
                    return new IntegerX(Sign, new uint[] { (uint)v });
                }
                else
                {
                    return new IntegerX(Sign, new uint[] { most, (uint)v });
                }
            }

        }

        /// <summary>
        /// Create a <see cref="IntegerX"/> from a (signed) Integer value.
        /// </summary>
        /// <param name="v">The value</param>
        /// <returns>A <see cref="IntegerX"/>.</returns>
        public static IntegerX Create(int v) => Create((long)v);

        /// <summary>
        /// Create a <see cref="IntegerX"/> from a double value.
        /// </summary>
        /// <param name="v">The value</param>
        /// <returns>A <see cref="IntegerX"/>.</returns>
        public static IntegerX Create(double v)
        {
            byte[] dbytes;
            ulong significand;
            int exp;

            var val = BitConverter.GetBytes(v);

            IntegerX tempRes, res;

            if ((double.IsNaN(v)) || (double.IsInfinity(v)))
            {
                throw new OverflowException("Infinity/NaN not supported in IntegerX (yet)");
            }

            dbytes = new byte[Marshal.SizeOf(v)];

            Array.Copy(val, dbytes, Marshal.SizeOf(v));
            significand = GetDoubleSignificand(dbytes);
            exp = GetDoubleBiasedExponent(dbytes);

            if (significand == 0)
            {
                if (exp == 0)
                {
                    return Zero;
                }

                if (v < 0.0)
                {
                    tempRes = NegativeOne;
                }
                else
                {
                    tempRes = One;
                }

                tempRes = tempRes.LeftShift(exp - DoubleExponentBias);
                return tempRes;
            }
            else
            {
                significand |= 0x10000000000000;
                res = Create(significand);
                if (exp > 1075)
                {
                    res <<= (exp - DoubleShiftBias);
                }
                else
                {
                    res >>= (DoubleShiftBias - exp);
                }

                if (v < 0.0)
                {
                    return res * -1;
                }
                else
                {
                    return res;
                }
            }
            
        }

        /// <summary>
        /// Create a <see cref="IntegerX"/> from a string.
        /// </summary>
        /// <param name="v">The value</param>
        /// <returns>A <see cref="IntegerX"/>.</returns>
        public static IntegerX Create(string v) => Parse(v);

        #endregion

        /// <summary>
        /// Append a sequence of digits representing  <param name="rem"/> to the <see cref="StringBuilder"/>,
        /// possibly adding leading null chars if specified.
        /// </summary>
        /// <param name="sb">The <see cref="StringBuilder"/> to append characters to</param>
        /// <param name="rem">The 'super digit' value to be converted to its string representation</param>
        /// <param name="radix">The radix for the conversion</param>
        /// <param name="charBuf">A character buffer used for temporary storage, big enough to hold the string
        /// representation of <paramref name="rem"/></param>
        /// <param name="leadingZeros">Whether or not to pad with the leading zeros if the value is not large enough to fill the buffer</param>
        /// <remarks>Pretty much identical to DLR BigInteger.AppendRadix</remarks>
        private static void AppendDigit(ref StringBuilder sb, uint rem, uint radix, char[] charBuff, bool leadingZeros)
        {
            const string symbols = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
             
            int i, bufLen;
            uint digit;

            bufLen = charBuff.Length;
            i = bufLen - 1;

            while ((i >= 0) && (rem != 0))
            {
                digit = rem % radix;
                rem /= radix;
                charBuff[i] = symbols[(int)digit];
                i--;
            }

            if (leadingZeros)
            {
                while (i >= 0)
                {
                    charBuff[i] = '0';
                    i--;
                }
                sb.Append(charBuff);
            }
            else
            {
                sb.Append(charBuff, i + 1, bufLen - i - 1);
            }
        }

        /// <summary>
        /// Convert an (extended) digit to its value in the given radix.
        /// </summary>
        /// <param name="c">The character to convert</param>
        /// <param name="radix">The radix to interpret the character in</param>
        /// <param name="v">Set to the converted value</param>
        /// <returns> true if the conversion is successful; false otherwise</returns>
        private static bool TryComputeDigitVal(char c, int radix, out uint v)
        {
            v = MaxUIntValue;

            if (('0' <= c) && (c <= '9'))
            {
                v = (uint)(c - '0');
            }
            else if (('a' <= c) && (c <= 'z'))
            {
                v = (uint)(10 + c - 'a');
            }
            else if (('A' <= c) && (c <= 'Z'))
            {
                v = (uint)(10 + c - 'A');
            }

            return v < (uint)radix;
        }

        /// <summary>
        /// Return an indication of the relative values of two UInt arrays treated as unsigned big-endian values.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns><value>-1</value> if the first is less than second; <value>0</value> if equal; <value>+1</value> if greater</returns>
        private static short Compare(uint[] x, uint[] y)
        {
            int i;
            var xlen = x.Length;
            var ylen = y.Length;

            if (xlen < ylen)
            {
                return -1;
            }

            if (xlen > ylen)
            {
                return 1;
            }

            i = 0;

            while (i < xlen)
            {
                if (x[i] < y[i])
                {
                    return -1;
                }
                if (x[i] > y[i])
                {
                    return 1;
                }
                i++;
            }

            return 0;
        }

        /// <summary>
        /// Compute the greatest common divisor of two <see cref="IntegerX"/> values.
        /// </summary>
        /// <param name="a">The first value</param>
        /// <param name="b">The second value</param>
        /// <returns>The greatest common divisor</returns>
        /// <remarks>Does the standard Euclidean algorithm until the two values are approximately
        /// the same length, then switches to a binary gcd algorithm.</remarks>
        private static IntegerX HybridGcd(IntegerX a, IntegerX b)
        {
            IntegerX r;
            while (b._data.Length != 0)
            {
                if (Math.Abs(a._data.Length - b._data.Length) < 2)
                {
                    return BinaryGcd(a, b);
                }

                a.DivRem(b, out r);
                a = b;
                b = r;
            }

            return a;
        }

        /// <summary>
        /// Return the greatest common divisor of two uint values.
        /// </summary>
        /// <param name="a">The first value</param>
        /// <param name="b">The second value</param>
        /// <returns>The greatest common divisor</returns>
        private static uint BinaryGcd(uint a, uint b)
        {
            int y, aZeros, bZeros, t;
            uint x;

            //From Knuth, 4.5.5, Algorithm B
            if (b == 0)
            {
                return a;
            }

            if (a == 0)
            {
                return b;
            }

            aZeros = 0;
            x = a & 0xFF;
            while (x == 0)
            {
                a >>= 8;
                aZeros += 8;
                x = a & 0xFF;
            }

            y = TrailingZerosTable[x];
            aZeros += y;
            a >>= y;

            bZeros = 0;
            x = b & 0xFF;
            while (x == 0)
            {
                b >>= 8;
                bZeros += 8;
                x = b & 0xFF;
            }

            y = TrailingZerosTable[x];
            bZeros += y;
            b >>= y;
            if (aZeros < bZeros)
            {
                t = aZeros;
            }
            else
            {
                t = bZeros;
            }

            while (a != b)
            {
                if (a > b)
                {
                    a -= b;
                    x = a & 0xFF;
                    while (x == 0)
                    {
                        a >>= 8;
                        x = a & 0xFF;
                    }

                    a >>= TrailingZerosTable[x];
                }
                else
                {
                    b -= a;
                    x = b & 0xFF;
                    while (x == 0)
                    {
                        b >>= 8;
                        x = b & 0xFF;
                    }
                    b >>= TrailingZerosTable[x];
                }
            }

            return a << t;
        }

        /// <summary>
        /// Compute the greatest common divisor of two <see cref="IntegerX"/> values.
        /// </summary>
        /// <param name="a">The first value</param>
        /// <param name="b">The second value</param>
        /// <returns>The greatest common divisor</returns>
        /// <remarks>Uses Knuth, 4.5.5, Algorithm B, highly optimized for getting rid of powers of 2.
        /// </remarks>
        private static IntegerX BinaryGcd(IntegerX a, IntegerX b)
        {
            int tsign, lb;
            IntegerX t;
            uint x, y;

            //From Knuth, 4.5.5, Algorithm B
            //Step B1: Find power of 2
            var s1 = a.GetLowestSetBit();
            var s2 = a.GetLowestSetBit();
            var k = Math.Min(s1, s2);
            if (k != 0)
            {
                a = a.RightShift(k);
                b = b.RightShift(k);
            }

            //Step B2: Initialize
            if (k == s1)
            {
                t = b;
                tsign = -1;
            }
            else
            {
                t = a;
                tsign = 1;
            }

            lb = t.GetLowestSetBit();
            while (lb >= 0)
            {
                //Steps B3 and B4 halve t until not even
                t = t.RightShift(lb);
                //Steps B5: reset max(u, v)
                if(tsign > 0)
                {
                    a = t;
                }
                else
                {
                    b = t;
                }

                if((a.AsUInt(out x)) && (b.AsUInt(out y)))
                {
                    x = BinaryGcd(x, y);
                    t = Create(x);
                    if(k > 0)
                    {
                        t = t.LeftShift(k);
                        return t;
                    }
                }

                //Step B6: Subtract
                t = a - b;
                if (t.IsZero())
                {
                    break;
                }

                if (t.IsPositive())
                {
                    tsign = 1;
                }
                else
                {
                    tsign = -1;
                    t = t.Abs();
                }
                lb = t.GetLowestSetBit();
            }

            if (k > 0)
            {
                a = a.LeftShift(k);
            }
            return a;
        }

        /// <summary>
        /// Returns the number of trailing zero bits in a UInt value.
        /// </summary>
        /// <param name="val">The value</param>
        /// <returns>The number of trailing zero bits </returns>
        private static int TrailingZerosCount(uint val)
        {
            uint byteVal = val & 0xFF;
            if (byteVal != 0)
            {
                return TrailingZerosTable[byteVal];
            }

            byteVal = (val >> 8) & 0xFF;
            if (byteVal != 0)
            {
                return TrailingZerosTable[byteVal] + 8;
            }

            byteVal = (val >> 16) & 0xFF;
            if (byteVal != 0)
            {
                return TrailingZerosTable[byteVal] + 16;
            }

            byteVal = (val >> 16) & 0xFF;
            return TrailingZerosTable[byteVal] + 24;
        }

        private static uint BitLengthForUInt(uint x) => 32 - LeadingZeroCount(x);

        /// <summary>
        /// Counts Leading zero bits.
        /// </summary>
        /// <param name="x">value to count leading zero bits on.</param>
        /// <returns>leading zero bit count.</returns>
        private static uint LeadingZeroCount(uint x)
        {
            x |= (x >> 1);
            x |= (x >> 2);
            x |= (x >> 4);
            x |= (x >> 8);
            x |= (x >> 16);

            return 32 - BitCount(x);
        }

        /// <summary>
        /// This algo is in a lot of places.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        private static uint BitCount(uint x)
        {
            x -= ((x >> 1) & 0x55555555);
            x = ((x >> 2) & 0x33333333) + (x & 0x33333333);
            x = ((x >> 4) + x) & 0x0F0F0F0F;
            x += (x >> 8);
            x += (x >> 16);
            return x & 0x0000003F;
        }

        /// <summary>
        /// Returns the index of the lowest set bit in this instance's magnitude.
        /// </summary>
        /// <returns>The index of the lowest set bit</returns>
        private int GetLowestSetBit()
        {
            int j;

            if (_sign == 0)
            {
                return -1;
            }

            j = _data.Length - 1;
            while ((j > 0) && (_data[j] == 0))
            {
                j--;
            }

            return ((_data.Length - j - 1) << 5) + TrailingZerosCount(_data[j]);
        }

        /// <summary>
        /// Returns the specified uint-digit pretending the number
        /// is a little-endian two's complement representation.
        /// </summary>
        /// <param name="n">The index of the digit to retrieve</param>
        /// <returns>The uint at the given index.</returns>
        private uint Get2CDigit(int n)
        {
            uint digit;

            if (n < 0)
            {
                return 0;
            }

            if (n >= _data.Length)
            {
                return Get2CSignExtensionDigit();
            }

            digit = _data[_data.Length - n - 1];

            if (_sign >= 0)
            {
                return digit;
            }

            if (n <= FirstNonzero2CDigitIndex())
            {
                return ~digit + 1;
            }

            return ~digit;
        }

        /// <summary>
        /// Returns the specified uint-digit pretending the number
        /// is a little-endian two's complement representation.
        /// </summary>
        /// <param name="n">The index of the digit to retrieve</param>
        /// <param name="seenNonZero">Set to true if a nonZero byte is seen</param>
        /// <returns>The UInt at the given index.</returns>
        private uint Get2CDigit(int n, ref bool seenNoneZero)
        {
            uint digit;

            if (n < 0)
            {
                return 0;
            }

            if (n >= _data.Length)
            {
                return Get2CSignExtensionDigit();
            }

            digit = _data[_data.Length - n - 1];
            if (_sign >= 0)
            {
                return digit;
            }

            if (seenNoneZero)
            {
                return ~digit;
            }
            else
            {
                if (digit == 0)
                {
                    return 0;
                }
                else
                {
                    seenNoneZero = true;
                    return ~digit + 1;
                }
            }
        }

        /// <summary>
        /// Returns an UInt of all zeros or all ones depending on the sign (pos, neg).
        /// </summary>
        /// <returns>The UInt corresponding to the sign</returns>
        private uint Get2CSignExtensionDigit()
        {
            if (_sign < 0)
            {
                return MaxUIntValue;
            }
            return 0;
        }

        /// <summary>
        /// Returns the index of the first nonzero digit (there must be one), pretending the value is little-endian.
        /// </summary>
        /// <returns></returns>
        private int FirstNonzero2CDigitIndex()
        {
            var i = _data.Length - 1;
            while ((i >= 0) && (_data[i] == 0))
            {
                i--;
            }

            return _data.Length - i - 1;
        }

        /// <summary>
        /// Return the twos-complement of the integer represented by the UInt array.
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        private static uint[] MakeTwosComplement(uint[] a)
        {
            int i;
            uint digit;

            i = a.Length - 1;
            digit = 0;
            while ((i >= 0) && (digit == 0))
            {
                digit = ~a[i] + 1;
                a[i] = digit;
                i--;
            }

            while (i >= 0)
            {
                a[i] = ~a[i];
                i--;
            }

            return a;
        }

        /// <summary>
        /// Add two UInt arrays (big-endian).
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private static uint[] Add(uint[] x, uint[] y)
        {
            uint[] temp, tempRes;
            int xi, yi;
            ulong sum;

            if (x.Length < y.Length)
            {
                temp = x;
                x = y;
                y = temp;
            }

            xi = x.Length;
            yi = y.Length;

            tempRes = new uint[xi];
            sum = 0;

            while (yi > 0)
            {
                xi--;
                yi--;
                sum = (sum >> (int)BitsPerDigit) + x[xi] + y[yi];
                tempRes[xi] = (uint)sum;
            }

            sum >>= (int)BitsPerDigit;
            while ((xi > 0) && (sum != 0))
            {
                xi--;
                sum = (ulong)x[xi] + 1;
                tempRes[xi] = (uint)sum;
                sum >>= (int)BitsPerDigit;
            }

            while (xi > 0)
            {
                xi--;
                tempRes[xi] = x[xi];
            }

            if (sum != 0)
            {
                tempRes = AddSignificantDigit(tempRes, (uint)sum);
            }

            return tempRes;
        }

        /// <summary>
        /// Add one digit.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="newDigit"></param>
        /// <returns></returns>
        private static uint[] AddSignificantDigit(uint[] x, uint newDigit)
        {
            uint[] tempRes;
            int i;

            tempRes = new uint[x.Length + 1];
            tempRes[0] = newDigit;
            i = 0;
            while (i < x.Length)
            {
                tempRes[i + 1] = x[i];
                i++;
            }

            return tempRes;
        }

        /// <summary>
        /// Subtract one instance from another (larger first).
        /// </summary>
        /// <param name="xs"></param>
        /// <param name="ys"></param>
        /// <returns></returns>
        private static uint[] Subtract(uint[] xs, uint[] ys)
        {
            int xlen, ylen, ix, iy;
            uint[] tempRes;
            bool borrow;
            uint x, y;

            // Assume xs > ys
            xlen = xs.Length;
            ylen = ys.Length;
            tempRes = new uint[xlen];

            borrow = false;
            ix = xlen - 1;
            iy = ylen - 1;

            while (iy >= 0)
            {
                x = xs[ix];
                y = ys[iy];
                if (borrow)
                {
                    if (x == 0)
                    {
                        x = 0xFFFFFFFF;
                        borrow = true;
                    }
                    else
                    {
                        x -= 1;
                        borrow = false;
                    }
                }

                borrow = borrow || (y > x);
                tempRes[ix] = x - y;
                iy--;
                ix--;
            }

            while ((borrow) && (ix >= 0))
            {
                tempRes[ix] = xs[ix] - 1;
                borrow = tempRes[ix] == 0xFFFFFFFF;
                ix--;
            }

            while (ix >= 0)
            {
                tempRes[ix] = xs[ix];
                ix--;
            }

            return RemoveLeadingZeros(tempRes);
        }

        /// <summary>
        /// Multiply two big-endian UInt arrays.
        /// </summary>
        /// <param name="xs"></param>
        /// <param name="ys"></param>
        /// <returns></returns>
        private static uint[] Multiply(uint[] xs, uint[] ys)
        {
            int xlen, ylen, xi, zi, yi;
            uint[] zs;
            ulong x, product;

            xlen = xs.Length;
            ylen = ys.Length;
            zs = new uint[xlen + ylen];
            xi = xlen - 1;

            while (xi >= 0)
            {
                x = xs[xi];
                zi = xi + ylen;
                product = 0;
                yi = ylen - 1;

                while (yi >= 0)
                {
                    product = product + x * ys[yi] + zs[zi];
                    zs[zi] = (uint)product;
                    product >>= (int)BitsPerDigit;

                    yi--;
                    zi--;
                }

                while (product != 0)
                {
                    product += zs[zi];
                    zs[zi] = (uint)product;
                    zi++;
                    product >>= (int)BitsPerDigit;
                }

                xi--;
            }

            return RemoveLeadingZeros(zs);
        }

        /// <summary>
        /// Return the quotient and remainder of dividing one <see cref="TIntegerX"/> by another.
        /// </summary>
        /// <param name="x">The dividend</param>
        /// <param name="y">The divisor</param>
        /// <param name="q">Set to the quotient</param>
        /// <param name="r">Set to the remainder</param>
        /// <remarks>Algorithm D in Knuth 4.3.1.</remarks>
        private static void DivMod(uint[] x, uint[] y, out uint[] q, out uint[] r)
        {
            const ulong SuperB = 0x100000000;

            int ylen, xlen, cmp, shift, j, k, i;
            uint rem;
            ulong toptwo, qhat, rhat, val, carry;
            long borrow, temp;
            uint[] xnorm, ynorm;

            ylen = y.Length;

            if (ylen == 0)
            {
                throw new DivideByZeroException();
            }

            xlen = x.Length;

            if (xlen == 0)
            {
                q = new uint[0];
                r = new uint[0];
                return;
            }

            cmp = Compare(x, y);

            if (cmp == 0)
            {
                q = new uint[] { 1 };
                r = new uint[] { 0 };
                return;
            }

            if (cmp < 0)
            {
                q = new uint[0];
                r = new uint[x.Length];
                Array.Copy(x, 0, r, 0, x.Length);
                return;
            }

            if (ylen == 1)
            {
                rem = CopyDivRem(x, y[0], out q);
                r = new uint[] { rem };
                return;
            }

            shift = (int)LeadingZeroCount(y[0]);
            xnorm = new uint[xlen + 1];
            ynorm = new uint[ylen];

            Normalize(ref xnorm, xlen + 1, x, xlen, shift);
            Normalize(ref ynorm, ylen, y, ylen, shift);
            q = new uint[xlen - ylen + 1];
            r = null;

            j = 0;
            while (j <= (xlen - ylen))
            {
                toptwo = xnorm[j] * SuperB + xnorm[j + 1];
                qhat = toptwo / ynorm[0];
                rhat = toptwo % ynorm[0];

                while (true)
                {
                    if ((qhat < SuperB) && ((qhat * ynorm[1]) <= (SuperB * rhat + xnorm[j + 2])))
                    {
                        break;
                    }

                    qhat--;
                    rhat += (ulong)ynorm[0];

                    if (rhat >= SuperB)
                    {
                        break;
                    }
                }

                borrow = 0;
                k = ylen - 1;

                while (k >= 0)
                {
                    i = j + k + 1;
                    val = ynorm[k] * qhat;
                    temp = (long)(xnorm[i] - (long)(uint)val - borrow);
                    xnorm[i] = (uint)temp;
                    val >>= (int)BitsPerDigit;
                    temp = temp >> (int)BitsPerDigit;
                    borrow = (long)val - temp;
                    k--;
                }

                temp = (long)xnorm[j] - borrow;
                xnorm[j] = (uint)temp;

                q[j] = (uint)qhat;

                if (temp < 0)
                {
                    q[j]--;
                    carry = 0;
                    k = ylen - 1;

                    while (k >= 0)
                    {
                        i = j + k + 1;
                        carry = (ulong)ynorm[k] + xnorm[i] + carry;
                        xnorm[i] = (uint)carry;
                        carry >>= (int)BitsPerDigit;
                        k--;
                    }

                    carry += (ulong)xnorm[j];
                    xnorm[j] = (uint)carry;
                }
                j++;
            }
            Unnormalize(xnorm, out r, shift);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="xnorm"></param>
        /// <param name="r"></param>
        /// <param name="shift"></param>
        private static void Unnormalize(uint[] xnorm, out uint[] r, int shift)
        {
            int len, lShift, i;
            uint carry, lval;

            len = xnorm.Length;
            r = new uint[len];

            if (shift == 0)
            {
                i = 0;
                while (i < len)
                {
                    r[i] = xnorm[i];
                    i++;
                }
            }
            else
            {
                lShift = (int)BitsPerDigit - shift;
                carry = 0;
                i = 0;

                while (i < len)
                {
                    lval = xnorm[i];
                    r[i] = (lval >> shift) | carry;
                    carry = lval << lShift;
                    i++;
                }
            }

            r = RemoveLeadingZeros(r);
        }

        /// <summary>
        /// Do a multiplication and addition in place.
        /// </summary>
        /// <param name="data">The subject of the operation, receives the result</param>
        /// <param name="mult">The value to multiply by</param>
        /// <param name="addend">The value to add in</param>
        private static void InPlaceMulAdd(ref uint[] data, uint mult, uint addend)
        {
            int len, i;
            ulong sum, product, carry;

            len = data.Length;
            carry = 0;
            i = len - 1;

            while (i >= 0)
            {
                product = (ulong)data[i] * mult + carry;
                data[i] = (uint)product;
                carry = product >> (int)BitsPerDigit;
                i--;
            }

            sum = (ulong)data[len - 1] + addend;
            data[len - 1] = (uint)sum;
            carry = sum >> (int)BitsPerDigit;
            i = len - 2;

            while ((i >= 0) && (carry > 0))
            {
                sum = data[i] + carry;
                data[i] = (uint)sum;
                carry = sum >> (int)BitsPerDigit;
                i--;
            }
        }

        /// <summary>
        /// Return a (possibly new) UInt array with leading zero uints removed.
        /// </summary>
        /// <param name="data">The UInt array to prune</param>
        /// <returns>A (possibly new) UInt array with leading zero UInt's removed.</returns>
        private static uint[] RemoveLeadingZeros(uint[] data)
        {
            int len, index, i;
            uint[] tempRes;

            len = data.Length;
            index = 0;
            while ((index < len) && (data[index] == 0))
            {
                index++;
            }

            if (index == 0)
            {
                return data;
            }

            tempRes = new uint[len - index];
            i = 0;
            while (i < (len - index))
            {
                tempRes[i] = data[index + i];
                i++;
            }

            return tempRes;
        }

        private static uint[] StripLeadingZeroBytes(byte[] a)
        {
            int byteLength, keep, intLength, b, i, bytesRemaining, bytesToTransfer, j;
            uint[] result;
            byteLength = a.Length;
            keep = 0;

            while ((keep < byteLength) && (a[keep] == 0))
            {
                keep++;
            }

            intLength = ((byteLength - keep) + 3) >> 2;
            result = new uint[intLength];
            b = byteLength - 1;

            i = intLength - 1;
            while (i >= 0)
            {
                result[i] = (uint)(a[b] & 0xFF);
                b--;
                bytesRemaining = b - keep + 1;
                bytesToTransfer = Math.Min(3, bytesRemaining);
                j = 8;

                while (j <= (bytesToTransfer << 3))
                {
                    result[i] |= (uint)((a[b] & 0xFF) << j);
                    b--;
                    j += 8;
                }

                i--;
            }

            return result;
        }

        private static uint[] makePositive(byte[] a)
        {
            int keep, k, byteLength, extraByte, intLength, b, i, numBytesToTransfer, j, mask;
            uint[] result;
            byteLength = a.Length;
            keep = 0;

            while ((keep < byteLength) && ((sbyte)a[keep] == -1))
            {
                keep++;
            }

            k = keep;
            while ((k < byteLength) && ((sbyte)a[k] == 0))
            {
                k++;
            }

            if(k == byteLength)
            {
                extraByte = 1;
            }
            else
            {
                extraByte = 0;
            }

            intLength = ((byteLength - keep + extraByte) + 3) / 4;
            result = new uint[intLength];

            b = byteLength - 1;
            i = intLength - 1;

            while(i >= 0)
            {
                result[i] = (uint)(a[b] & 0xFF);
                b--;
                numBytesToTransfer = Math.Min(3, b - keep + 1);

                if(numBytesToTransfer < 0)
                {
                    numBytesToTransfer = 0;
                }

                j = 8;
                while(j <= 8 * numBytesToTransfer)
                {
                    result[i] |= (uint)((a[b] & 0xFF) << j);
                    b--;
                    j += 8;
                }

                mask = -1 >> (8 * (3 - numBytesToTransfer));
                result[i] = (~result[i]) & (uint)mask;
                i--;
            }

            i = result.Length - 1;
            while(i >= 0)
            {
                result[i] = (result[i] & 0xFFFFFFFF) + 1;
                if(result[i] != 0)
                {
                    break;
                }

                i--;
            }

            return result;
        }

        /// <summary>
        /// Do a division in place and return the remainder.
        /// </summary>
        /// <param name="data">The value to divide into, and where the result appears</param>
        /// <param name="index">Starting index in <paramref name="data"/> for the operation</param>
        /// <param name="divisor">The value to dif</param>
        /// <returns>The remainder</returns>
        /// <remarks>Pretty much identical to DLR BigInteger.div, except DLR's is little-endian
        /// and this is big-endian.</remarks>
        private static uint InPlaceDivRem(ref uint[] data, ref int index, uint divisor)
        {
            ulong rem;
            bool seenNonZero;
            int len, i;
            uint q;

            rem = 0;
            seenNonZero = false;
            len = data.Length;
            i = index;

            while(i < len)
            {
                rem = rem << (int)BitsPerDigit;
                rem = rem | data[i];
                q = (uint)(rem / divisor);
                data[i] = q;
                if(q  == 0)
                {
                    if (!seenNonZero)
                    {
                        index++;
                    }
                }
                else
                {
                    seenNonZero = true;
                }

                rem = rem % divisor;
                i++;
            }

            return (uint)rem;
        }

        /// <summary>
        /// Divide a big-endian UInt array by a UInt divisor, returning the quotient and remainder.
        /// </summary>
        /// <param name="data">A big-endian UInt array</param>
        /// <param name="divisor">The value to divide by</param>
        /// <param name="quotient">Set to the quotient (newly allocated)</param>
        /// <returns>The remainder</returns>
        private static uint CopyDivRem(uint[] data, uint divisor, out uint[] quotient)
        {
            quotient = new uint[data.Length];
            uint q;
            var rem = (ulong)0;
            var len = data.Length;
            var i = 0;

            while (i < len)
            {
                rem <<= (int)BitsPerDigit;
                rem |= data[i];
                q = (uint)(rem / divisor);
                quotient[i] = q;
                rem %= divisor;
                i++;
            }

            quotient = RemoveLeadingZeros(quotient);
            return (uint)rem;
        }

        private int signInt()
        {
            if(Signum() < 0)
            {
                return -1;
            }
            return 0;
        }

        private int getInt(int n)
        {
            int magInt;

            if(n < 0)
            {
                return 0;
            }

            if(n >= _data.Length)
            {
                return signInt();
            }

            magInt = (int) _data[_data.Length - n - 1];

            if(Signum() >= 0)
            {
                return magInt;
            }
            else
            {
                if(n <= firstNonzeroIntNum())
                {
                    return -magInt;
                }
                else
                {
                    return ~magInt;
                }
            }
        }

        private int firstNonzeroIntNum()
        {
            int fn;

            var mlen = _data.Length;
            var i = mlen - 1;

            while ((i >= 0) && (_data[i] == 0))
            {
                i--;
            }

            fn = mlen - i - 1;

            return fn;
        }

        /// <summary>
        /// Create a <see cref="IntegerX"/> from a string representation (radix 10).
        /// </summary>
        /// <param name="x">The string to convert</param>
        /// <returns>A <see cref="IntegerX"/></returns>
        /// <exception cref="Exception">Thrown if there is a bad minus sign (more than one or not leading)
        /// or if one of the digits in the string is not valid for the given radix.</exception>
        public static IntegerX Parse(string x) => Parse(x, 10);
        

        /// <summary>
        /// Create a <see cref="IntegerX"/> from a string representation in the given radix.
        /// </summary>
        /// <param name="s">The string to convert</param>
        /// <param name="radix">The radix of the numeric representation</param>
        /// <returns>A <see cref="IntegerX"/></returns>
        /// <exception cref="Exception">Thrown if there is a bad minus sign (more than one or not leading)
        /// or if one of the digits in the string is not valid for the given radix.</exception>
        public static IntegerX Parse(string s, int radix)
        {
            IntegerX v;

            if (TryParse(s, radix, out v))
            {
                return v;
            }
            else
            {
                throw new Exception("Invalid input format");
            }
            
        }

        /// <summary>
        /// Try to create a <see cref="IntegerX"/> from a string representation (radix 10)
        /// </summary>
        /// <param name="s">The string to convert</param>
        /// <param name="v">Set to the  <see cref="IntegerX"/> corresponding to the string, if possible; set to null otherwise</param>
        /// <returns><c>True</c> if the string is parsed successfully; <c>false</c> otherwise</returns>
        public static bool TryParse(string s, out IntegerX v) => TryParse(s, 10, out v);

        /// <summary>
        /// Try to create a <see cref="IntegerX"/> from a string representation in the given radix)
        /// </summary>
        /// <param name="s">The string to convert</param>
        /// <param name="radix">The radix of the numeric representation</param>
        /// <param name="v">Set to the <see cref="IntegerX"/> corresponding to the string, if possible; set to null otherwise</param>
        /// <returns><c>True</c> if the string is parsed successfully; <c>false</c> otherwise</returns>
        /// <remarks>
        /// <para>This is pretty much the same algorithm as in the Java implementation.
        /// That's pretty much what is in Knuth ACPv2ed3, Sec. 4.4, Method 1b.
        /// That's pretty much what you'd do by hand.</para>
        /// <para>The only enhancement is that instead of doing one digit at a time, you translate a group of contiguous
        /// digits into a UInt, then do a multiply by the radix and add of the UInt.
        /// The size of each group of digits is the maximum number of digits in the radix
        /// that will fit into a UInt.</para>
        /// <para>Once you've decided to make that enhancement to Knuth's algorithm, you pretty much
        /// end up with the Java version's code.</para>
        /// </remarks>
        public static bool TryParse(string s, int radix, out IntegerX v)
        {
            short Sign;
            int len, minusIndex, plusIndex, index, numDigits, numBits, numUints, groupSize, firstGroupLen;
            uint mult, u;
            uint[] data;

            var chArray = s.ToCharArray();

            if ((radix < MinRadix) || (radix > MaxRadix))
            {
                v = default(IntegerX);
                return false;
            }

            Sign = 1;
            len = s.Length;

            minusIndex = s.LastIndexOf('-');
            plusIndex = s.LastIndexOf('+');
            if ((len == 0) || ((minusIndex == 0) && (len == 1)) || ((plusIndex == 0) && (len == 1)) || (minusIndex > 0) || (plusIndex > 0))
            {
                v = default(IntegerX);
                return false;
            }

            index = 0;
            if (plusIndex != -1)
            {
                index = 1;
            }
            else if (minusIndex != -1)
            {
                Sign = -1;
                index = 1;
            }

            while ((index < len) && (s[index] == '0'))
            {
                index++;
            }

            if (index == len)
            {
                v = new IntegerX(Zero);
                return true;
            }

            numDigits = len - index;
            numBits = ((numDigits * BitsPerRadixDigit[radix]) >> 10) + 1;
            numUints = (int)((numBits + BitsPerDigit - 1) / BitsPerDigit);

            data = new uint[numUints];

            groupSize = RadixDigitsPerDigit[radix];
            firstGroupLen = numDigits % groupSize;

            if (firstGroupLen == 0)
            {
                firstGroupLen = groupSize;
            }
            if (!TryParseUInt(s, index, firstGroupLen, radix, out data[data.Length - 1]))
            {
                v = default(IntegerX);
                return false;
            }

            index += firstGroupLen;
            mult = SuperRadix[radix];

            while (index < len)
            {
                if (!TryParseUInt(s, index, groupSize, radix, out u))
                {
                    v = default(IntegerX);
                    return false;
                }

                InPlaceMulAdd(ref data, mult, u);
                index = index + groupSize;
            }

            v = new IntegerX(Sign, RemoveLeadingZeros(data));
            return true;
        }

        /// <summary>
        /// Convert a substring in a given radix to its equivalent numeric value as a UInt.
        /// </summary>
        /// <param name="val">The string containing the substring to convert</param>
        /// <param name="startIndex">The start index of the substring</param>
        /// <param name="len">The length of the substring</param>
        /// <param name="radix">The radix</param>
        /// <param name="u">Set to the converted value, or 0 if the conversion is unsuccessful</param>
        /// <returns><value>true</value> if successful, <value>false</value> otherwise</returns>
        /// <remarks>The length of the substring must be small enough that the converted value is guaranteed to fit
        /// into a UInt.</remarks>
        public static bool TryParseUInt(string val, int startIndex, int len, int radix, out uint u)
        {
            ulong tempRes = 0;
            int i = 0;
            uint v;

            u = 0;
            while (i < len)
            {
                if (!TryComputeDigitVal(val[startIndex + i], radix, out v))
                {
                    return false;
                }

                tempRes = (tempRes * (uint)radix) + v;
                if (tempRes > MaxUIntValue)
                {
                    return false;
                }

                i++;
            }

            u = (uint)tempRes;
            return true;
        }

        /// <summary>
        /// Extract the sign bit from a byte-array representaition of a double.
        /// </summary>
        /// <param name="v">A byte-array representation of a double</param>
        /// <returns>The sign bit, either 0 (positive) or 1 (negative)</returns>
        public static int GetDoubleSign(byte[] v)
        {
            return v[7] & 0x80;
        }

        /// <summary>
        /// Extract the significand (AKA mantissa, coefficient) from a byte-array representation of a double.
        /// </summary>
        /// <param name="v">A byte-array representation of a double</param>
        /// <returns>The significand</returns>
        public static ulong GetDoubleSignificand(byte[] v)
        {
            var i1 = v[0] | (uint)(v[1] << 8) | (uint)(v[2] << 16) | (uint)(v[3] << 24);
            var i2 = v[4] | (uint)(v[5] << 8) | (uint)((v[6] & 0xF) << 16);

            return (i1 | ((ulong)i2 << 32));
        }

        /// <summary>
        /// Extract the exponent from a byte-array representation of a double.
        /// </summary>
        /// <param name="v">A byte-array representation of a double</param>
        /// <returns>The exponent</returns>
        public static ushort GetDoubleBiasedExponent(byte[] v)
        {
            return (ushort)(((ushort)(v[7] & 0x7F) << 4) | ((ushort)(v[6] & 0xF0) >> 4));
        }

        /// <summary>
        /// Algorithm from Hacker's Delight, section 11-4. for internal use only
        /// </summary>
        /// <param name="v">value to use.</param>
        /// <returns>The Precision</returns>
        public static uint UIntPrecision(uint v)
        {
            uint i;

            i = 1;
            while (true)
            {
                if (v <= UIntLogTable[i])
                {
                    return i;
                }
                i++;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="xnorm"></param>
        /// <param name="xnlen"></param>
        /// <param name="x"></param>
        /// <param name="xlen"></param>
        /// <param name="shift"></param>
        /// <remarks>
        /// <para>Assume Length(xnorm) := xlen + 1 or xlen;</para>
        /// <para>Assume shift in [0,31]</para>
        /// <para>This should be private, but I wanted to test it.</para>
        /// </remarks>
        public static void Normalize(ref uint[] xnorm, int xnlen, uint[] x, int xlen, int shift)
        {
            bool sameLen;
            int offset, rShift, i;
            uint carry, xi;

            sameLen = xnlen == xlen;
            if (sameLen)
            {
                offset = 0;
            }
            else
            {
                offset = 1;
            }

            if (shift == 0)
            {
                if (!sameLen)
                {
                    xnorm[0] = 0;
                }

                i = 0;
                while (i < xlen)
                {
                    xnorm[i + offset] = x[i];
                    i++;
                }
                return;
            }

            rShift = (int)BitsPerDigit - shift;
            carry = 0;
            i = xlen - 1;

            while (i >= 0)
            {
                xi = x[i];
                xnorm[i + offset] = (xi << shift) | carry;
                carry = xi >> rShift;
                i--;
            }

            if (sameLen)
            {
                if (carry != 0)
                {
                    throw new InvalidOperationException("Carry off left end.");
                }
            }
            else
            {
                xnorm[0] = carry;
            }
        }

        #region OPERATORS
        /// <summary>
        /// Implicitly convert from byte to <see cref="IntegerX"/>.
        /// </summary>
        /// <param name="v">The value to convert</param>
        /// <returns>The equivalent <see cref="IntegerX"/></returns>
        public static implicit operator IntegerX(byte v) => Create((uint)v);

        /// <summary>
        /// Implicitly convert from sbyte to <see cref="IntegerX"/>.
        /// </summary>
        /// <param name="v">The value to convert</param>
        /// <returns>The equivalent <see cref="IntegerX"/></returns>
        public static implicit operator IntegerX(sbyte v) => Create(v);

        /// <summary>
        /// Implicitly convert from short to <see cref="IntegerX"/>.
        /// </summary>
        /// <param name="v">The value to convert</param>
        /// <returns>The equivalent <see cref="IntegerX"/></returns>
        public static implicit operator IntegerX(short v) => Create(v);

        /// <summary>
        /// Implicitly convert from ushort to <see cref="IntegerX"/>.
        /// </summary>
        /// <param name="v">The value to convert</param>
        /// <returns>The equivalent <see cref="IntegerX"/></returns>
        public static implicit operator IntegerX(ushort v) => Create((uint)v);

        /// <summary>
        /// Implicitly convert from uint to <see cref="IntegerX"/>.
        /// </summary>
        /// <param name="v">The value to convert</param>
        /// <returns>The equivalent <see cref="IntegerX"/></returns>
        public static implicit operator IntegerX(uint v) => Create(v);

        /// <summary>
        /// Implicitly convert from int to <see cref="IntegerX"/>.
        /// </summary>
        /// <param name="v">The value to convert</param>
        /// <returns>The equivalent <see cref="IntegerX"/></returns>
        public static implicit operator IntegerX(int v) => Create(v);

        /// <summary>
        /// Implicitly convert from ulong to <see cref="IntegerX"/>.
        /// </summary>
        /// <param name="v">The value to convert</param>
        /// <returns>The equivalent <see cref="IntegerX"/></returns>
        public static implicit operator IntegerX(ulong v) => Create(v);

        /// <summary>
        /// Explicitly convert from double to <see cref="IntegerX"/>.
        /// </summary>
        /// <param name="self">The value to convert</param>
        /// <returns>The equivalent <see cref="IntegerX"/></returns>
        public static explicit operator IntegerX(double self) => Create(self);

        /// <summary>
        /// Explicitly convert from string to <see cref="IntegerX"/>.
        /// </summary>
        /// <param name="self">The value to convert</param>
        /// <returns>The equivalent <see cref="IntegerX"/></returns>
        public static explicit operator IntegerX(string self) => Create(self);

        /// <summary>
        /// Explicitly convert from <see cref="IntegerX"/> to double.
        /// </summary>
        /// <param name="i">The <see cref="IntegerX"/> to convert</param>
        /// <returns>The equivalent double</returns>
        public static explicit operator double(IntegerX i) => i.toDouble();

        /// <summary>
        /// Explicitly convert from <see cref="IntegerX"/> to byte.
        /// </summary>
        /// <param name="self">The <see cref="IntegerX"/> to convert</param>
        /// <returns>The equivalent byte</returns>
        public static explicit operator byte(IntegerX self)
        {
            int tmp;

            if (self.AsInt(out tmp))
            {
                return (byte)tmp;
            }
            throw new OverflowException("Value can't fit in specified type");
        }

        /// <summary>
        /// Explicitly convert from <see cref="IntegerX"/> to sbyte.
        /// </summary>
        /// <param name="self">The <see cref="IntegerX"/> to convert</param>
        /// <returns>The equivalent sbyte</returns>
        public static explicit operator sbyte(IntegerX self)
        {
            int tmp;

            if (self.AsInt(out tmp))
            {
                return (sbyte)tmp;
            }
            throw new OverflowException("Value can't fit in specified type");
        }

        /// <summary>
        /// Explicitly convert from <see cref="IntegerX"/> to ushort.
        /// </summary>
        /// <param name="self">The <see cref="IntegerX"/> to convert</param>
        /// <returns>The equivalent ushort</returns>
        public static explicit operator ushort(IntegerX self)
        {
            int tmp;

            if (self.AsInt(out tmp))
            {
                return (ushort)tmp;
            }
            throw new OverflowException("Value can't fit in specified type");

        }

        /// <summary>
        /// Explicitly convert from <see cref="IntegerX"/> to short.
        /// </summary>
        /// <param name="self">The <see cref="IntegerX"/> to convert</param>
        /// <returns>The equivalent short</returns>
        public static explicit operator short(IntegerX self)
        {
            int tmp;

            if (self.AsInt(out tmp))
            {
                return (short)tmp;
            }
            throw new OverflowException("Value can't fit in specified type");

        }

        /// <summary>
        /// Explicitly convert from <see cref="IntegerX"/> to uint.
        /// </summary>
        /// <param name="self">The <see cref="IntegerX"/> to convert</param>
        /// <returns>The equivalent uint</returns>
        public static explicit operator uint(IntegerX self)
        {
            uint tmp;

            if (self.AsUInt(out tmp))
            {
                return tmp;
            }
            throw new OverflowException("Value can't fit in specified type");

        }

        /// <summary>
        /// Explicitly convert from <see cref="IntegerX"/> to int.
        /// </summary>
        /// <param name="self">The <see cref="IntegerX"/> to convert</param>
        /// <returns>The equivalent int</returns>
        public static explicit operator int(IntegerX self)
        {
            int tmp;

            if (self.AsInt(out tmp))
            {
                return tmp;
            }
            throw new OverflowException("Value can't fit in specified type");

        }

        /// <summary>
        /// Explicitly convert from <see cref="IntegerX"/> to long.
        /// </summary>
        /// <param name="self">The <see cref="IntegerX"/> to convert</param>
        /// <returns>The equivalent long</returns>
        public static explicit operator long(IntegerX self)
        {
            long tmp;

            if (self.AsLong(out tmp))
            {
                return tmp;
            }
            throw new OverflowException("Value can't fit in specified type");
        }

        /// <summary>
        /// Explicitly convert from <see cref="IntegerX"/> to ulong.
        /// </summary>
        /// <param name="self">The <see cref="IntegerX"/> to convert</param>
        /// <returns>The equivalent ulong</returns>
        public static explicit operator ulong(IntegerX self)
        {
            ulong tmp;

            if (self.AsULong(out tmp))
            {
                return tmp;
            }
            throw new OverflowException("Value can't fit in specified type");
        }

        /// <summary>
        /// Compare two <see cref="IntegerX"/>'s for equivalent numeric values.
        /// </summary>
        /// <param name="x">First value to compare</param>
        /// <param name="y">Second value to compare</param>
        /// <returns><value>true</value> if equivalent; <value>false</value> otherwise</returns>
        public static bool operator ==(IntegerX x, IntegerX y) => Compare(x, y) == 0;

        /// <summary>
        /// Compare two <see cref="IntegerX"/>'s for non-equivalent numeric values.
        /// </summary>
        /// <param name="x">First value to compare</param>
        /// <param name="y">Second value to compare</param>
        /// <returns><value>true</value> if not equivalent; <value>false</value> otherwise</returns>
        public static bool operator !=(IntegerX x, IntegerX y) => Compare(x, y) != 0;

        public override bool Equals(object obj)
        {
            var temp = obj as IntegerX;
            if (temp == null)
            {
                return false;
            }
            return Equals(temp);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Compare two <see cref="IntegerX"/>'s for &lt;.
        /// </summary>
        /// <param name="x">First value to compare</param>
        /// <param name="y">Second value to compare</param>
        /// <returns><value>true</value> if &lt;; <value>false</value> otherwise</returns>
        public static bool operator <(IntegerX x, IntegerX y) => Compare(x, y) < 0;

        /// <summary>
        /// Compare two <see cref="IntegerX"/>'s for &lt;=.
        /// </summary>
        /// <param name="x">First value to compare</param>
        /// <param name="y">Second value to compare</param>
        /// <returns><value>true</value> if &lt;=; <value>false</value> otherwise</returns>
        public static bool operator <=(IntegerX x, IntegerX y) => Compare(x, y) <= 0;

        /// <summary>
        /// Compare two <see cref="IntegerX"/>'s for &gt;.
        /// </summary>
        /// <param name="x">First value to compare</param>
        /// <param name="y">Second value to compare</param>
        /// <returns><value>true</value> if &gt;; <value>false</value> otherwise</returns>
        public static bool operator >(IntegerX x, IntegerX y) => Compare(x, y) > 0;

        /// <summary>
        /// Compare two <see cref="IntegerX"/>'s for &gt;=.
        /// </summary>
        /// <param name="x">First value to compare</param>
        /// <param name="y">Second value to compare</param>
        /// <returns><value>true</value> if &gt;=; <value>false</value> otherwise</returns>
        public static bool operator >=(IntegerX x, IntegerX y) => Compare(x, y) >= 0;

        /// <summary>
        /// Compute <paramref name="x"/> + <paramref name="y"/>.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>The sum</returns>
        public static IntegerX operator +(IntegerX x, IntegerX y) => x.Add(y);

        /// <summary>
        /// Compute <paramref name="x"/> - <paramref name="y"/>.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>The difference</returns>
        public static IntegerX operator -(IntegerX x, IntegerX y) => x.Subtract(y);

        /// <summary>
        /// Compute the plus of <paramref name="x"/>.
        /// </summary>
        /// <param name="x"></param>
        /// <returns>The positive</returns>
        public static IntegerX operator +(IntegerX x) => x;

        /// <summary>
        /// Compute the negation of <paramref name="x"/>.
        /// </summary>
        /// <param name="x"></param>
        /// <returns>The negation</returns>
        public static IntegerX operator -(IntegerX x) => x.Negate();

        /// <summary>
        /// Compute <paramref name="x"/> * <paramref name="y"/>.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>The product</returns>
        public static IntegerX operator *(IntegerX x, IntegerX y) => x.Multiply(y);

        /// <summary>
        /// Compute <paramref name="x"/> / <paramref name="y"/>.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>The quotient</returns>
        public static IntegerX operator /(IntegerX x, IntegerX y) => x.Divide(y);

        /// <summary>
        /// Compute <paramref name="x"/> % <paramref name="y"/>.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>The modulus</returns>
        public static IntegerX operator %(IntegerX x, IntegerX y) => x.Modulo(y);

        /// <summary>
        /// Returns the bitwise-AND.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static IntegerX operator &(IntegerX x, IntegerX y) => x.BitwiseAnd(y);

        /// <summary>
        /// Returns the bitwise-OR.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static IntegerX operator |(IntegerX x, IntegerX y) => x.BitwiseOr(y);

        /// <summary>
        /// Returns the bitwise-complement.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static IntegerX operator !(IntegerX x) => x.OnesComplement();

        /// <summary>
        /// Returns the bitwise-XOR.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static IntegerX operator ^(IntegerX x, IntegerX y) => x.BitwiseXor(y);

        /// <summary>
        /// Returns the left-shift of a <see cref="IntegerX"/> by an integer shift.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="shift"></param>
        /// <returns></returns>
        public static IntegerX operator <<(IntegerX x, int shift) => x.LeftShift(shift);

        /// <summary>
        /// Returns the right-shift of a <see cref="IntegerX"/> by an integer shift.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="shift"></param>
        /// <returns></returns>
        public static IntegerX operator >>(IntegerX x, int shift) => x.RightShift(shift);

        #endregion

        /// <summary>
        /// Compute <paramref name="x"/> + <paramref name="y"/>.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>The sum</returns>
        public static IntegerX Add(IntegerX x, IntegerX y) => x.Add(y);

        /// <summary>
        /// Compute <paramref name="x"/> - <paramref name="y"/>.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>The difference</returns>
        public static IntegerX Subtract(IntegerX x, IntegerX y) => x.Subtract(y);

        /// <summary>
        /// Compute the negation of <paramref name="x"/>.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>The negation</returns>
        public static IntegerX Negate(IntegerX x) => x.Negate();

        /// <summary>
        /// Compute <paramref name="x"/> * <paramref name="y"/>.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>The product</returns>
        public static IntegerX Multiply(IntegerX x, IntegerX y) => x.Multiply(y);

        /// <summary>
        /// Compute <paramref name="x"/> / <paramref name="y"/>.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>The quotient</returns>
        public static IntegerX Divide(IntegerX x, IntegerX y) => x.Divide(y);

        /// <summary>
        /// Returns <paramref name="x"/> % <paramref name="y"/>.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>The modulus</returns>
        public static IntegerX Modulo(IntegerX x, IntegerX y) => x.Modulo(y);

        /// <summary>
        /// Compute the quotient and remainder of dividing one <see cref="TIntegerX"/> by another.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="remainder">Set to the remainder after division</param>
        /// <returns>The quotient</returns>
        public static IntegerX DivRem(IntegerX x, IntegerX y, out IntegerX remainder) => x.DivRem(y, out remainder);

        /// <summary>
        /// Computes the absolute value.
        /// </summary>
        /// <param name="x"></param>
        /// <returns>The absolute value</returns>
        public static IntegerX Abs(IntegerX x) => x.Abs();

        /// <summary>
        /// Returns a <see cref="IntegerX"/> raised to an int power.
        /// </summary>
        /// <param name="x">The value to exponentiate</param>
        /// <param name="exp">The exponent</param>
        /// <returns>The exponent</returns>
        public static IntegerX Power(IntegerX x, int exp) => x.Power(exp);

        /// <summary>
        /// Returns a <see cref="IntegerX"/> raised to an <see cref="IntegerX"/> power modulo another <see cref="IntegerX"/>
        /// </summary>
        /// <param name="x"></param>
        /// <param name="power"></param>
        /// <param name="modulo"></param>
        /// <returns> x ^ e mod m</returns>
        public static IntegerX ModPow(IntegerX x, IntegerX power, IntegerX modulo) => x.ModPow(power, modulo);

        /// <summary>
        /// Returns the greatest common divisor.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>The greatest common divisor</returns>
        public static IntegerX Gcd(IntegerX x, IntegerX y) => x.Gcd(y);

        /// <summary>
        /// Returns the bitwise-AND.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static IntegerX BitwiseAnd(IntegerX x, IntegerX y) => x.BitwiseAnd(y);

        /// <summary>
        /// Returns the bitwise-OR.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static IntegerX BitwiseOr(IntegerX x, IntegerX y) => x.BitwiseOr(y);

        /// <summary>
        /// Returns the bitwise-XOR.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static IntegerX BitwiseXor(IntegerX x, IntegerX y) => x.BitwiseXor(y);


        /// <summary>
        /// Returns the bitwise complement.
        /// </summary>
        /// <param name="x"></param>
        public static IntegerX BitwiseNot(IntegerX x) => x.OnesComplement();

        /// <summary>
        /// Returns the bitwise x and (not y).
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static IntegerX BitwiseAndNot(IntegerX x, IntegerX y) => x.BitwiseAndNot(y);

        /// <summary>
        /// Returns  <paramref name="x"/> << <paramref name="shift"/>.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static IntegerX LeftShift(IntegerX x, int shift) => x.LeftShift(shift);

        /// <summary>
        /// Returns  <paramref name="x"/> >> <paramref name="shift"/>.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="shift"></param>
        /// <returns></returns>
        public static IntegerX RightShift(IntegerX x, int shift) => x.RightShift(shift);

        /// <summary>
        /// Test if a specified bit is set.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static bool TestBit(IntegerX x, int n) => x.TestBit(n);

        /// <summary>
        /// Set the specified bit.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static IntegerX SetBit(IntegerX x, int n) => x.SetBit(n);

        /// <summary>
        /// Set the specified bit to its negation.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static IntegerX FlipBit(IntegerX x, int n) => x.FlipBit(n);

        /// <summary>
        /// Clear the specified bit.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static IntegerX ClearBit(IntegerX x, int n) => x.ClearBit(n);

        /// <summary>
        /// Returns Self + y.
        /// </summary>
        /// <param name="y">The augend.</param>
        /// <returns>The sum</returns>
        public IntegerX Add(IntegerX y)
        {
            int c;

            if (_sign == 0)
            {
                return y;
            }

            if (_sign == y._sign)
            {
                return new IntegerX(_sign, Add(_data, y._data));
            }
            else
            {
                c = Compare(_data, y._data);

                switch (c)
                {
                    case -1:
                        return new IntegerX(-_sign, Subtract(y._data, _data));
                    case 0:
                        return new IntegerX(Zero);
                    case 1:
                        return new IntegerX(_sign, Subtract(_data, y._data));
                    default:
                        throw new InvalidOperationException("Bogus result from Compare");
                }
            }
        }

        /// <summary>
        /// Returns Self - y
        /// </summary>
        /// <param name="y">The subtrahend</param>
        /// <returns>The difference</returns>
        public IntegerX Subtract(IntegerX y)
        {
            int cmp;
            uint[] mag;

            if (y._sign == 0)
            {
                return this;
            }

            if (_sign == 0)
            {
                return y.Negate();
            }

            if (_sign != y._sign)
            {
                return new IntegerX(_sign, Add(_data, y._data));
            }

            cmp = Compare(_data, y._data);
            if (cmp == 0)
            {
                return Zero;
            }

            if (cmp > 0)
            {
                mag = Subtract(_data, y._data);
            }
            else
            {
                mag = Subtract(y._data, _data);
            }
            
            return new IntegerX(cmp * _sign, mag);
        }

        /// <summary>
        /// Returns the negation of this value.
        /// </summary>
        /// <returns>The negation</returns>
        public IntegerX Negate() => new IntegerX(-_sign, _data);

        /// <summary>
        /// Returns Self * y
        /// </summary>
        /// <param name="y">The multiplicand</param>
        /// <returns>The product</returns>
        public IntegerX Multiply(IntegerX y)
        {
            uint[] mag;

            if (_sign == 0)
            {
                return Zero;
            }

            if (y._sign == 0)
            {
                return Zero;
            }

            mag = Multiply(_data, y._data);
            return new IntegerX(_sign * y._sign, mag);
        }

        /// <summary>
        /// Returns Self / y.
        /// </summary>
        /// <param name="y">The divisor</param>
        /// <returns>The quotient</returns>
        public IntegerX Divide(IntegerX y)
        {
            IntegerX rem;
            return DivRem(y, out rem);
        }

        /// <summary>
        /// Returns Self % y
        /// </summary>
        /// <param name="y">The divisor</param>
        /// <returns>The modulus</returns>
        public IntegerX Modulo(IntegerX y)
        {
            IntegerX rem;
            DivRem(y, out rem);
            return rem;
        }

        /// <summary>
        /// Returns the quotient and remainder of this divided by another.
        /// </summary>
        /// <param name="y">The divisor</param>
        /// <param name="remainder">The remainder</param>
        /// <returns>The quotient</returns>
        public IntegerX DivRem(IntegerX y, out IntegerX remainder)
        {
            uint[] q, r;
            DivMod(_data, y._data, out q, out r);
            remainder = new IntegerX(_sign, r);
            return new IntegerX(_sign * y._sign, q);
        }

        /// <summary>
        /// Returns the absolute value of this instance.
        /// </summary>
        /// <returns>The absolute value</returns>
        public IntegerX Abs()
        {
            if (_sign > -0)
            {
                return this;
            }
            return Negate();
        }

        /// <summary>
        /// Returns the value of this instance raised to an integral power.
        /// </summary>
        /// <param name="exp">The exponent</param>
        /// <returns>The exponetiated value</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the exponent is negative.</exception>
        public IntegerX Power(int exp)
        {
            IntegerX mult, tempRes;
            if (exp < 0)
            {
                throw new ArgumentOutOfRangeException("Exponent must be non-negative");
            }

            if (exp == 0)
            {
                return One;
            }

            if (_sign == 0)
            {
                return this;
            }

            mult = this;
            tempRes = One;
            while (exp != 0)
            {
                if ((exp & 1) != 0)
                {
                    tempRes *= mult;
                }
                if (exp == 1)
                {
                    break;
                }
                mult *= mult;
                exp >>= 1;
            }

            return tempRes;
        }

        /// <summary>
        /// Returns (Self ^ <paramref name="power"/>) % <paramref name="modulus"/>
        /// </summary>
        /// <param name="power">The exponent</param>
        /// <param name="modulus"></param>
        /// <returns></returns>
        public IntegerX ModPow(IntegerX power, IntegerX modulus)
        {
            IntegerX mult, tempRes;

            if (power < 0)
            {
                throw new ArgumentOutOfRangeException("power must be non-negative");
            }

            if (power._sign == 0)
            {
                return One;
            }

            if (_sign == 0)
            {
                return this;
            }

            mult = this;
            tempRes = One;

            while (power != Zero)
            {
                if (power.IsOdd())
                {
                    tempRes *= mult;
                    tempRes %= modulus;
                }

                if (power == One)
                {
                    break;
                }

                mult *= mult;
                mult %= modulus;
                power >>= 1;
            }

            return tempRes;
        }

        /// <summary>
        /// Returns the greatest common divisor of this and another value.
        /// </summary>
        /// <param name="y">The other value</param>
        /// <returns>The greatest common divisor</returns>
        public IntegerX Gcd(IntegerX y)
        {
            if (y._sign == 0)
            {
                Abs();
            }
            else if (_sign == 0)
            {
                return y.Abs();
            }

            return HybridGcd(Abs(), y.Abs());
        }

        /// <summary>
        /// Return the bitwise-AND of this instance and another <see cref="IntegerX"/>
        /// </summary>
        /// <param name="y">The value to AND to this instance.</param>
        /// <returns>The bitwise-AND</returns>
        public IntegerX BitwiseAnd(IntegerX y)
        {
            uint xdigit, ydigit;

            var rlen = Math.Max(_data.Length, y._data.Length);
            var tempRes = new uint[rlen];
            var seenNonZeroX = false;
            var seenNonZeroY = false;
            var i = 0;

            while (i < rlen)
            {
                xdigit = Get2CDigit(i, ref seenNonZeroX);
                ydigit = y.Get2CDigit(i, ref seenNonZeroY);
                tempRes[rlen - i - 1] = xdigit & ydigit;
                i++;
            }

            if ((IsNegative()) && (y.IsNegative()))
            {
                return new IntegerX(-1, RemoveLeadingZeros(MakeTwosComplement(tempRes)));
            }

            return new IntegerX(1, RemoveLeadingZeros(tempRes));
        }

        /// <summary>
        /// Return the bitwise-OR of this instance and another <see cref="IntegerX"/>
        /// </summary>
        /// <param name="y">The value to OR to this instance.</param>
        /// <returns>The bitwise-OR</returns>
        public IntegerX BitwiseOr(IntegerX y)
        {
            int i, rlen;
            uint[] tempRes;
            bool seenNonZeroX, seenNonZeroY;
            uint xdigit, ydigit;

            rlen = Math.Max(_data.Length, y._data.Length);
            tempRes = new uint[rlen];
            seenNonZeroY = false;
            seenNonZeroX = false;

            i = 0;
            while (i < rlen)
            {
                xdigit = Get2CDigit(i, ref seenNonZeroX);
                ydigit = y.Get2CDigit(i, ref seenNonZeroY);
                tempRes[rlen - i - 1] = xdigit | ydigit;
                i++;
            }

            if ((IsNegative()) || (y.IsNegative()))
            {
                return new IntegerX(-1, RemoveLeadingZeros(MakeTwosComplement(tempRes)));
            }

            return new IntegerX(1, RemoveLeadingZeros(tempRes));
        }

        /// <summary>
        /// Return the bitwise-XOR of this instance and another <see cref="IntegerX"/>
        /// </summary>
        /// <param name="y">The value to XOR to this instance.</param>
        /// <returns>The bitwise-XOR</returns>
        public IntegerX BitwiseXor(IntegerX y)
        {
            uint xdigit, ydigit;

            var rlen = Math.Max(_data.Length, y._data.Length);
            var tempRes = new uint[rlen];
            var seenNonZeroX = false;
            var seenNonZeroY = false;
            var i = 0;

            while (i < rlen)
            {
                xdigit = Get2CDigit(i, ref seenNonZeroX);
                ydigit = y.Get2CDigit(i, ref seenNonZeroY);
                tempRes[rlen - i - 1] = xdigit ^ ydigit;
                i++;
            }

            if (Signum() == y.Signum())
            {
                return new IntegerX(-1, MakeTwosComplement(tempRes));
            }

            return new IntegerX(1, RemoveLeadingZeros(tempRes));
        }

        /// <summary>
        /// Returns the bitwise complement of this instance.
        /// </summary>
        /// <returns>The bitwise complement</returns>
        public IntegerX OnesComplement()
        {
            uint xdigit;

            var len = _data.Length;
            var tempRes = new uint[len];
            var seenNonZero = false;
            var i = 0;

            while (i < len)
            {
                xdigit = Get2CDigit(i, ref seenNonZero);
                tempRes[len - i - 1] = ~xdigit;
                i++;
            }

            if (IsNegative())
            {
                return new IntegerX(1, RemoveLeadingZeros(tempRes));
            }

            return new IntegerX(-1, MakeTwosComplement(tempRes));
        }

        /// <summary>
        /// Return the bitwise-AND-NOT of this instance and another <see cref="IntegerX"/>
        /// </summary>
        /// <param name="y">The value to OR to this instance.</param>
        /// <returns>The bitwise-AND-NOT</returns>
        public IntegerX BitwiseAndNot(IntegerX y)
        {
            uint xdigit, ydigit;

            var rlen = Math.Max(_data.Length, y._data.Length);
            var tempRes = new uint[rlen];
            var seenNonZeroX = false;
            var seenNonZeroY = false;
            var i = 0;

            while (i < rlen)
            {
                xdigit = Get2CDigit(i, ref seenNonZeroX);
                ydigit = y.Get2CDigit(i, ref seenNonZeroY);
                tempRes[rlen - i - 1] = xdigit & (~ydigit);
                i++;
            }

            if ((IsNegative()) && (y.IsPositive()))
            {
                return new IntegerX(-1, MakeTwosComplement(tempRes));
            }

            return new IntegerX(1, RemoveLeadingZeros(tempRes));
        }

        /// <summary>
        /// Returns the value of the given bit in this instance.
        /// </summary>
        /// <param name="n">Index of the bit to check</param>
        /// <returns><value>true</value> if the bit is set; <value>false</value> otherwise</returns>
        /// <exception cref="ArithmeticException">Thrown if the index is negative.</exception>
        /// <remarks>The value is treated as if in twos-complement.</remarks>
        public bool TestBit(int n)
        {
            if (n < 0)
            {
                throw new ArithmeticException("Negative bit address");
            }

            return (Get2CDigit(n / 32) & (1 << (n % 32))) != 0;
        }

        /// <summary>
        /// Set the n-th bit.
        /// </summary>
        /// <param name="n">Index of the bit to set</param>
        /// <returns>An instance with the bit set</returns>
        /// <exception cref="ArithmeticException">Thrown if the index is negative.</exception>
        /// <remarks>The value is treated as if in twos-complement.</remarks>
        public IntegerX SetBit(int n)
        {
            int index, i, len;
            uint[] tempRes;
            bool seenNonZero;

            if (TestBit(n))
            {
                return this;
            }

            index = n / 32;
            tempRes = new uint[Math.Max(_data.Length, index + 1)];
            len = tempRes.Length;

            seenNonZero = false;
            i = 0;
            while (i < len)
            {
                tempRes[len - i - 1] = Get2CDigit(i, ref seenNonZero);
                i++;
            }

            tempRes[len - index - 1] |= ((uint)1 << (n % 32));

            if (IsNegative())
            {
                return new IntegerX(-1, RemoveLeadingZeros(MakeTwosComplement(tempRes)));
            }

            return new IntegerX(1, RemoveLeadingZeros(tempRes));
        }

        /// <summary>
        /// Clears the n-th bit.
        /// </summary>
        /// <param name="n">Index of the bit to clear</param>
        /// <returns>An instance with the bit cleared</returns>
        /// <exception cref="ArithmeticException">Thrown if the index is negative.</exception>
        /// <remarks>The value is treated as if in twos-complement.</remarks>
        public IntegerX ClearBit(int n)
        {
            int index, i, len;
            uint[] tempRes;
            bool seenNonZero;

            if (!TestBit(n))
            {
                return this;
            }

            index = n / 32;
            tempRes = new uint[Math.Max(_data.Length, index + 1)];
            len = tempRes.Length;

            seenNonZero = false;
            i = 0;

            while (i < len)
            {
                tempRes[len - i - 1] = Get2CDigit(i, ref seenNonZero);
                i++;
            }

            tempRes[len - index - 1] &= (~((uint)1 << (n % 32)));

            if (IsNegative())
            {
                return new IntegerX(-1, RemoveLeadingZeros(MakeTwosComplement(tempRes)));
            }

            return new IntegerX(1, RemoveLeadingZeros(tempRes));
        }

        /// <summary>
        /// Toggles the n-th bit.
        /// </summary>
        /// <param name="n">Index of the bit to toggle</param>
        /// <returns>An instance with the bit toggled</returns>
        /// <exception cref="ArithmeticException">Thrown if the index is negative.</exception>
        /// <remarks>The value is treated as if in twos-complement.</remarks>
        public IntegerX FlipBit(int n)
        {
            int index, i, len;
            uint[] tempRes;
            bool seenNonZero;

            if (n < 0)
            {
                throw new ArithmeticException("Negative bit address");
            }

            index = n / 32;
            tempRes = new uint[Math.Max(_data.Length, index + 1)];
            len = tempRes.Length;
            seenNonZero = false;
            i = 0;

            while (i < len)
            {
                tempRes[len - i - 1] = Get2CDigit(i, ref seenNonZero);
                i++;
            }

            tempRes[len - index - 1] ^= ((uint)1 << (n % 32));

            if (IsNegative())
            {
                return new IntegerX(-1, RemoveLeadingZeros(MakeTwosComplement(tempRes)));
            }

            return new IntegerX(1, RemoveLeadingZeros(tempRes));
        }

        /// <summary>
        /// Returns the value of this instance left-shifted by the given number of bits.
        /// </summary>
        /// <param name="shift">The number of bits to shift.</param>
        /// <returns>An instance with the magnitude shifted.</returns>
        /// <remarks><para>The value is treated as if in twos-complement.</para>
        /// <para>A negative shift count will be treated as a positive right shift.</para></remarks>
        public IntegerX LeftShift(int shift)
        {
            int digitShift, bitShift, xlen, rShift, i, j;
            uint[] tempRes;
            uint highBits;

            if (shift == 0)
            {
                return this;
            }

            if (_sign == 0)
            {
                return this;
            }

            if (shift < 0)
            {
                return RightShift(-shift);
            }

            digitShift = (int)(shift / BitsPerDigit);
            bitShift = (int)(shift % BitsPerDigit);
            xlen = _data.Length;

            if (bitShift == 0)
            {
                tempRes = new uint[xlen + digitShift];
                Array.Copy(_data, 0, tempRes, 0, _data.Length);
            }
            else
            {
                rShift = (int)(BitsPerDigit - bitShift);
                highBits = _data[0] >> rShift;

                if (highBits == 0)
                {
                    tempRes = new uint[xlen + digitShift];
                    i = 0;
                }
                else
                {
                    tempRes = new uint[xlen + digitShift + 1];
                    tempRes[0] = highBits;
                    i = 1;
                }

                j = 0;
                while (j < xlen - 1)
                {
                    tempRes[i] = (_data[j] << bitShift) | (_data[j + 1] >> rShift);
                    j++;
                    i++;
                }

                tempRes[i] = _data[xlen - 1] << bitShift;
            }

            return new IntegerX(_sign, tempRes);
        }

        /// <summary>
        /// Returns the value of this instance right-shifted by the given number of bits.
        /// </summary>
        /// <param name="shift">The number of bits to shift.</param>
        /// <returns>An instance with the magnitude shifted.</returns>
        /// <remarks><para>The value is treated as if in twos-complement.</para>
        /// <para>A negative shift count will be treated as a positive left shift.</para></remarks>
        public IntegerX RightShift(int shift)
        {
            int digitShift, bitShift, xlen, i, j, rlen, lShift;
            uint[] tempRes;
            uint hightBits;

            if (shift == 0)
            {
                return this;
            }

            if (_sign == 0)
            {
                return this;
            }

            if (shift < 0)
            {
                return LeftShift(-shift);
            }

            digitShift = (int)(shift / BitsPerDigit);
            bitShift = (int)(shift % BitsPerDigit);

            xlen = _data.Length;

            if (digitShift >= xlen)
            {
                if (_sign >= 0)
                {
                    return Zero;
                }
                else
                {
                    return NegativeOne;
                }
            }

            if (bitShift == 0)
            {
                rlen = xlen - digitShift;
                tempRes = new uint[rlen];
                i = 0;

                while (i < rlen)
                {
                    tempRes[i] = _data[i];
                    i++;
                }
            }
            else
            {
                hightBits = _data[0] >> bitShift;

                if (hightBits == 0)
                {
                    rlen = xlen - digitShift - 1;
                    tempRes = new uint[rlen];
                    i = 0;
                }
                else
                {
                    rlen = xlen - digitShift;
                    tempRes = new uint[rlen];
                    tempRes[0] = hightBits;
                    i = 1;
                }

                lShift = (int)(BitsPerDigit - bitShift);
                j = 0;

                while (j < xlen - digitShift - 1)
                {
                    tempRes[i] = (_data[j] << lShift) | (_data[j + 1] >> bitShift);
                    j++;
                    i++;
                }
            }

            return new IntegerX(_sign, tempRes);
        }

        /// <summary>
        /// Try to convert to an Integer.
        /// </summary>
        /// <param name="ret">Set to the converted value</param>
        /// <returns><value>true</value> if successful; <value>false</value> if the value cannot be represented.</returns>
        public bool AsInt(out int ret)
        {
            ret = 0;
            switch (_data.Length)
            {
                case 0:
                    return true;

                case 1:
                    if (_data[0] > (uint)0x80000000)
                    {
                        return false;
                    }

                    if ((_data[0] == (uint)0x80000000) && (_sign == 1))
                    {
                        return false;
                    }
                    ret = (int)_data[0] * _sign;
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Try to convert to long.
        /// </summary>
        /// <param name="ret">Set to the converted value</param>
        /// <returns><value>true</value> if successful; <value>false</value> if the value cannot be represented.</returns>
        public bool AsLong(out long ret)
        {
            ulong tmp;
            ret = 0;

            switch (_data.Length)
            {
                case 0:
                    return true;
                case 1:
                    ret = _sign * (long)_data[0];
                    return true;
                case 2:
                    tmp = ((ulong)_data[0] << 32) | (ulong)_data[1];
                    if (tmp > 0x8000000000000000)
                    {
                        return false;
                    }
                    if ((tmp == 0x8000000000000000) && (_sign == 1))
                    {
                        return false;
                    }
                    ret = (long)tmp * _sign;
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Try to convert to an UInt.
        /// </summary>
        /// <param name="ret">Set to the converted value</param>
        /// <returns><value>true</value> if successful; <value>false</value> if the value cannot be represented.</returns>
        public bool AsUInt(out uint ret)
        {
            ret = 0;
            if (_sign == 0)
            {
                return true;
            }
            if (_sign < 0)
            {
                return false;
            }
            if (_data.Length > 1)
            {
                return false;
            }

            ret = _data[0];
            return true;
        }

        /// <summary>
        /// Try to convert to an ULong.
        /// </summary>
        /// <param name="ret">Set to the converted value</param>
        /// <returns><value>true</value> if successful; <value>false</value> if the value cannot be represented.</returns>
        public bool AsULong(out ulong ret)
        {
            ret = 0;
            if (_sign < 0)
            {
                return false;
            }

            switch (_data.Length)
            {
                case 0:
                    return true;
                case 1:
                    ret = (ulong)_data[0];
                    return true;
                case 2:
                    ret = ((ulong)_data[1]) | ((ulong)_data[0] << 32);
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent byte value
        /// </summary>
        /// <returns>The converted value</returns>
        /// <exception cref="OverflowException">Thrown if the value cannot be represented in a byte.</exception>
        public byte toByte()
        {
            uint ret;
            if ((AsUInt(out ret)) && (ret <= 0xFF))
            {
                return (byte)ret;
            }

            throw new OverflowException("IntegerX value won't fit in Byte");
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent short value
        /// </summary>
        /// <returns>The converted value</returns>
        /// <exception cref="OverflowException">Thrown if the value cannot be represented in a short.</exception>
        public short toShort()
        {
            int ret;
            if ((AsInt(out ret)) && (MinShortValue <= ret) && (ret <= MaxShortValue))
            {
                return (short)ret;
            }
            throw new OverflowException("IntegerX value won't fit in a Short");
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent sbyte value
        /// </summary>
        /// <returns>The converted value</returns>
        /// <exception cref="OverflowException">Thrown if the value cannot be represented in a sbyte.</exception>
        public sbyte toSByte()
        {
            int ret;
            if ((AsInt(out ret)) && (MinSByteValue <= ret) && (ret <= MaxSByteValue))
            {
                return (sbyte)ret;
            }
            throw new OverflowException("IntegerX value won't fit in a SByte");
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent ushort value
        /// </summary>
        /// <returns>The converted value</returns>
        /// <exception cref="OverflowException">Thrown if the value cannot be represented in a ushort.</exception>
        public ushort toUShort()
        {
            uint ret;
            if ((AsUInt(out ret)) && (ret <= MaxUShortValue))
            {
                return (ushort)ret;
            }
            throw new OverflowException("IntegerX value won't fit in a UShort");
        }

        /// <summary>
        /// Convert to an equivalent UInt
        /// </summary>
        /// <returns>The equivalent value</returns>
        /// <exception cref="OverflowException">Thrown if the magnitude is too large for the conversion</exception>
        public uint toUInt()
        {
            uint ret;
            if (AsUInt(out ret))
            {
                return ret;
            }
            throw new OverflowException("IntegerX magnitude too large for UInt");
        }

        /// <summary>
        /// Convert to an equivalent Integer
        /// </summary>
        /// <returns>The equivalent value</returns>
        /// <exception cref="OverflowException">Thrown if the magnitude is too large for the conversion</exception>
        public int toInt()
        {
            int ret;
            if (AsInt(out ret))
            {
                return ret;
            }

            throw new OverflowException("IntegerX magnitude too large for Integer");
        }

        /// <summary>
        /// Convert to an equivalent ULong
        /// </summary>
        /// <returns>The equivalent value</returns>
        /// <exception cref="OverflowException">Thrown if the magnitude is too large for the conversion</exception>
        public ulong toULong()
        {
            ulong ret;
            if (AsULong(out ret))
            {
                return ret;
            }

            throw new OverflowException("IntegerX magnitude too large for ULong");
        }

        /// <summary>
        /// Convert to an equivalent Long
        /// </summary>
        /// <returns>The equivalent value</returns>
        /// <exception cref="OverflowException">Thrown if the magnitude is too large for the conversion</exception>
        public long toLong()
        {
            long ret;
            if (AsLong(out ret))
            {
                return ret;
            }

            throw new OverflowException("IntegerX magnitude too large for Long");
        }

        /// <summary>
        /// Convert to an equivalent Double
        /// </summary>
        /// <returns>The equivalent value</returns>
        /// <exception cref="OverflowException">Thrown if the magnitude is too large for the conversion</exception>
        public double toDouble()
        {
            return Convert.ToSingle(10.ToString(), _FS);
        }

        /// <summary>
        /// Compares this instance to another specified instance and returns an indication of their relative values.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(IntegerX other) => Compare(this, other);

        /// <summary>
        /// Indicates whether this instance is equivalent to another object of the same type.
        /// </summary>
        /// <param name="other">The object to compare this instance against</param>
        /// <returns><value>true</value> if equivalent; <value>false</value> otherwise</returns>
        public bool Equals(IntegerX other) => this == other;

        /// <summary>
        /// Returns an indication of the relative values of the two <see cref="IntegerX"/>'s
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns><value>-1</value> if the first is less than second; <value>0</value> if equal; <value>+1</value> if greater</returns>
        public static int Compare(IntegerX x, IntegerX y)
        {
            if (x._sign == y._sign)
            {
                return x._sign * Compare(x._data, y._data);
            }
            else
            {
                if (x._sign < y._sign)
                {
                    return -1;
                }
                else
                {
                    return 1;
                }
            }
        }

        /// <summary>
        /// Converts the numeric value of this <see cref="IntegerX"/> to its string representation in radix 10.
        /// </summary>
        /// <returns>The string representation in radix 10</returns>
        public override string ToString()
        {
            return toString(10);
        }

        /// <summary>
        /// Converts the numeric value of this <see cref="IntegerX"/> to its string representation in the given radix.
        /// </summary>
        /// <param name="radix">The radix for the conversion</param>
        /// <returns>The string representation in the given radix</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the radix is out of range [2,36].</exception>
        public string toString(uint radix)
        {
            int len, index, i;
            uint LSuperRadix, rem;
            uint[] working;
            List<uint> rems;
            StringBuilder sb;
            char[] charBuf;

            if ((radix < MinRadix) || (radix > MaxRadix))
            {
                throw new ArgumentOutOfRangeException(string.Format(_FS, "Radix {0} is out of range. MinRadix = {1}, MaxRadix = {2}", radix, MinRadix, MaxRadix));
            }

            if (_sign == 0)
            {
                return "0";
            }

            len = _data.Length;
            working = new uint[_data.Length];

            Array.Copy(_data, working, _data.Length);

            LSuperRadix = SuperRadix[radix];

            rems = new List<uint> { };
            index = 0;
            while (index < len)
            {
                rem = InPlaceDivRem(ref working, ref index, LSuperRadix);
                rems.Add(rem);
            }

            sb = new StringBuilder(rems.Count * RadixDigitsPerDigit[radix] + 1);

                
            if (_sign < 0)
            {
                sb.Append('-');
            }

            charBuf = new char[RadixDigitsPerDigit[radix]];

            AppendDigit(ref sb, rems[rems.Count - 1], radix, charBuf, false);
            i = rems.Count - 2;

            while (i >= 0)
            {
                AppendDigit(ref sb, rems[i], radix, charBuf, true);
                i--;
            }
            return sb.ToString();

        }

        /// <summary>
        /// Returns true if this instance is positive.
        /// </summary>
        public bool IsPositive() => _sign > 0;

        /// <summary>
        /// Returns true if this instance is negative.
        /// </summary>
        public bool IsNegative() => _sign < 0;

        /// <summary>
        /// Returns the sign (-1, 0, +1) of this instance.
        /// </summary>
        public int Signum() => _sign;

        /// <summary>
        /// Returns true if this instance has value 0.
        /// </summary>
        public bool IsZero() => _sign == 0;

        /// <summary>
        /// Return true if this instance has an odd value.
        /// </summary>
        public bool IsOdd() => ((_data != null) && (_data.Length > 0) && ((_data[_data.Length - 1] & 1) != 0));

        /// <summary>
        /// Returns the magnitude as a big-endian array of UInt.
        /// </summary>
        /// <returns>The magnitude</returns>
        public uint[] GetMagnitude()
        {
            uint[] res = new uint[_data.Length];

            Array.Copy(_data, res, _data.Length);

            return res;
        }

        public uint BitLength()
        {
            uint[] m;
            uint len, n, magBitLength, i;
            bool pow2;

            m = _data;
            len = (uint)m.Length;
            if (len == 0)
            {
                n = 0;
            }
            else
            {
                magBitLength = ((len - 1) << 5) + BitLengthForUInt(m[0]);
                if (Signum() < 0)
                {
                    pow2 = BitCount(m[0]) == 1;
                    i = 1;
                    while ((i < len) && (pow2))
                    {
                        pow2 = m[i] == 0;
                        i++;
                    }

                    if (pow2)
                    {
                        n = magBitLength - 1;
                    }
                    else
                    {
                        n = magBitLength;
                    }
                }
                else
                {
                    n = magBitLength;
                }
            }

            return n;
        }

        public uint BitCount()
        {
            uint bc, i, len, magTrailingZeroCount, j;
            uint[] m;

            m = _data;
            bc = 0;
            i = 0;
            len = (uint)_data.Length;

            while (i < len)
            {
                bc += BitCount(m[i]);
                i++;
            }

            if (Signum() < 0)
            {
                magTrailingZeroCount = 0;
                j = len - 1;

                while (m[j] == 0)
                {
                    magTrailingZeroCount += 32;
                    j--;
                }

                magTrailingZeroCount += (uint)TrailingZerosCount(m[j]);
                bc += magTrailingZeroCount - 1;
            }

            return bc;
        }

        public byte[] toByteArray()
        {
            int byteLen, i, bytesCopied, nextInt, intIndex;
            byte[] result;

            byteLen = (int)(BitLength() / 8) +1;
            result = new byte[byteLen];
            i = byteLen - 1;
            bytesCopied = 4;
            nextInt = 0;
            intIndex = 0;

            while (i >= 0)
            {
                if (bytesCopied == 4)
                {
                    nextInt = getInt(intIndex);
                    intIndex++;
                    bytesCopied = 1;
                }
                else
                {
                    nextInt >>= 8;
                    bytesCopied++;
                }
                result[i] = (byte)nextInt;
                i--;
            }

            return result;
        }

    }


}
