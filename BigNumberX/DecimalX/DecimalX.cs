using System;
using System.Runtime.InteropServices;
using System.Text;

namespace BigNumberX
{
    /// <summary>
    /// Numeric Class which represents Immutable, arbitrary-precision signed decimals.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This Class is inspired by the ( <see href="http://speleotrove.com/decimal/decarith.html">
    /// General Decimal Arithmetic Specification</see>) (PDF: <see href="http://speleotrove.com/decimal/decarith.pdf">
    /// General Decimal Arithmetic Specification.PDF</see>). However, at
    /// the moment, the interface and capabilities comes closer to
    /// java.math.BigDecimal.
    /// </para>
    /// <para>
    /// Because of this, as in j.m.BigDecimal, the implementation is
    /// closest to the X3.274 subset described in Appendix A of the GDAS:
    /// infinite values, NaNs, subnormal values and negative zero are not
    /// represented, and most conditions throw exceptions. Exponent limits
    /// in the context are not implemented, except a limit to the range of
    /// an Integer (Int32).
    /// </para>
    /// <para>
    /// The representation is an arbitrary precision integer (the signed
    /// coefficient, also called the unscaled value) and an exponent. The
    /// exponent is limited to the range of an Integer. The value of a
    /// BigDecimal representation is <c>coefficient * 10^exponent</c>.
    /// </para>
    /// <para>
    /// Note: the representation in the GDAS is [sign,coefficient,exponent]
    /// with sign = 0/1 for (pos/neg) and an unsigned coefficient. This
    /// yields signed zero, which we do not have. We used a <see cref="IntegerX" />
    /// (BigInteger) for the signed coefficient. That class does not have
    /// a representation for signed zero.
    /// </para>
    /// <para>
    /// Note: Compared to j.m.BigDecimal, our coefficient = their <c>
    /// unscaledValue</c> and our exponent is the negation of their <c>
    /// scale</c>.
    /// </para>
    /// <para>
    /// The representation also track the number of significant digits.
    /// This is usually the number of digits in the coefficient, except
    /// when the coefficient is zero. This value is computed lazily and
    /// cached.
    /// </para>
    /// <para>
    /// This is not a clean-room implementation. other code was examined,
    /// especially OpenJDK implementation of java.math.BigDecimal, to look
    /// for special cases and other gotchas.
    /// credit was given in the few places where unthinking translation was done.
    /// However, there are only so many ways to
    /// skim certain cats, so some similarities are unavoidable.
    /// </para>
    /// </remarks>
    public class DecimalX
    {

        #region CONST VARIABLES
        /// <summary>
        /// Min Integer value.
        /// </summary>
        private const int MinIntValue = -2147483648;

        /// <summary>
        /// Max Integer value.
        /// </summary>
        private const int MaxIntValue = 2147483647;

        /// <summary>
        /// Max Long value.
        /// </summary>
        private const long MaxLongValue = 9223372036854775807;

        /// <summary>
        /// Exponent bias in the 64-bit floating point representation.
        /// </summary>
        private const short DoubleExponentBias = 1023;

        /// <summary>
        /// The size in bits of the significand in the 64-bit floating point representation.
        /// </summary>
        private const sbyte DoubleSignificandBitLength = 52;

        /// <summary>
        /// How much to shift to accommodate the exponent and the binary digits of the significand.
        /// </summary>
        private const int DoubleShiftBias = DoubleExponentBias + DoubleSignificandBitLength;

        private const double DoublePositiveInfinity = 1.0 / 0.0;

        private const double DoubleNegativeInfinity = -1.0 / 0.0;
        #endregion

        #region PRIVATE VARIABLES
        /// <summary>
        /// The coefficient of this <see cref="DecimalX" />.
        /// </summary>
        private IntegerX _coeff;

        /// <summary>
        /// The exponent of this <see cref="DecimalX" />.
        /// </summary>
        private int _exp;

        /// <summary>
        /// Get the precision (number of decimal digits) of this <see cref="DecimalX" />.
        /// </summary>
        /// <remarks>The value 0 indicated that the number is not known.</remarks>
        private uint _precision;

        /// <summary>
        /// Temporary Variable to Hold <c>Zero </c><see cref="DecimalX" />.
        /// </summary>
        private static DecimalX ZeroX;

        /// <summary>
        /// Temporary Variable to Hold <c>One </c><see cref="DecimalX" />.
        /// </summary>
        private static DecimalX OneX;

        /// <summary>
        /// Temporary Variable to Hold <c>Ten </c><see cref="DecimalX" />.
        /// </summary>
        private static DecimalX TenX;

        // Source : ClojureCLR BigDecimal Source on GitHub.
        /// <summary>
        /// _biPowersOfTen.
        /// </summary>
        private static IntegerX[] _biPowersOfTen;

        /// <summary>
        /// <c>length </c> of <see cref="_biPowersOfTen" />.
        /// </summary>
        private static int _maxCachedPowerOfTen;

        #endregion

        #region PROPS
        /// <summary>
        /// The coefficient of this <see cref="DecimalX" />.
        /// </summary>
        public IntegerX Coefficient
        {
            get
            {
                return _coeff;
            }
        }

        /// <summary>
        /// The exponent of this <see cref="DecimalX" />.
        /// </summary>
        public int Exponent
        {
            get
            {
                return _exp;
            }
        }

        /// <summary>
        /// Get the (number of decimal digits) of this <see cref="DecimalX" />.  Will trigger computation if not already known.
        /// </summary>
        /// <returns>The precision.</returns>
        public uint Precision
        {
            get
            {
                if (_precision == 0)
                {
                    if (_coeff.IsZero())
                        _precision = 1;
                    else
                        _precision = _coeff.Precision;
                }
                return _precision;
            }
        }

        /// <summary>
        /// A Zero.
        /// </summary>
        public static DecimalX Zero
        {
            get
            {
                return ZeroX;
            }
        }

        /// <summary>
        /// A Positive One.
        /// </summary>
        public static DecimalX One
        {
            get
            {
                return OneX;
            }
        }

        /// <summary>
        /// A Ten.
        /// </summary>
        public static DecimalX Ten
        {
            get
            {
                return TenX;
            }
        }
        #endregion

        #region PRIVATE CONSTRUCTORS
        /// <summary>
        /// Create a <see cref="DecimalX" /> with given coefficient, exponent, and precision.
        /// </summary>
        /// <param name="coeff">The coefficient</param>
        /// <param name="exp">The exponent</param>
        /// <param name="Precision">The precision</param>
        /// <remarks>For internal use only.  We can't trust someone outside to set the precision for us.
        /// Only for use when we know the precision explicitly.</remarks>
        private DecimalX(IntegerX coeff, int exp, uint Precision)
        {
            _coeff = coeff;
            _exp = exp;
            _precision = Precision;
        }

        #endregion

        #region CONSTRUCTORS
        /// <summary>
        /// Create a <see cref="DecimalX" /> with given coefficient and exponent.
        /// </summary>
        /// <param name="coeff">The coefficient</param>
        /// <param name="exp">The exponent</param>
        public DecimalX(IntegerX coeff, int exp) : this(coeff, exp, 0) { }

        /// <summary>
        /// Creates a copy of given <see cref="DecimalX" />.
        /// </summary>
        /// <param name="copy">A copy of the given <see cref="DecimalX" /></param>
        /// <remarks>Really only needed internally.  DecimalX's are immutable, so why copy?
        /// Internally, we sometimes need to copy and modify before releasing into the wild.</remarks>
        public DecimalX(DecimalX copy) : this(copy._coeff, copy._exp, copy._precision) { }
        #endregion

        #region STATIC CONSTRUCTOR
        static DecimalX()
        {
            // Create a Zero DecimalX (a big decimal with value as Zero)
            ZeroX = new DecimalX(IntegerX.Zero, 0, 1);
            // Create a One DecimalX (a big decimal with value as One)
            OneX = new DecimalX(IntegerX.One, 0, 1);
            // Create a Ten DecimalX (a big decimal with value as Ten)
            TenX = new DecimalX(IntegerX.Ten, 0, 2);

            _biPowersOfTen = new IntegerX[] { IntegerX.One,
             IntegerX.Create(10), IntegerX.Create(100), IntegerX.Create(1000), IntegerX.Create(10000), IntegerX.Create(100000), IntegerX.Create(1000000), IntegerX.Create(10000000), IntegerX.Create(100000000), IntegerX.Create(1000000000), IntegerX.Create(10000000000), IntegerX.Create(100000000000) };

            _maxCachedPowerOfTen = _biPowersOfTen.Length;

        }
        #endregion

        #region CREATE OVERLOADS
        /// <summary>
        /// Create a <see cref="DecimalX" /> from a double.
        /// </summary>
        /// <param name="v">The double value</param>
        /// <returns>A <see cref="DecimalX" /> corresponding to the double value.</returns>
        /// <remarks>Watch out!  TDecimalX.Create(0.1) is not the same as TDecimalX.Parse("0.1").
        /// We create exact representations of doubles,
        /// and 1/10 does not have an exact representation as a double. So the double 1.0 is not exactly 1/10.</remarks>
        public static DecimalX Create(double v)
        {
            byte[] dbytes;
            ulong significand;
            int biasedExp, leftShift, expToUse;
            IntegerX coeff;

            if ((double.IsNaN(v)) || (double.IsInfinity(v)))
                throw new ArgumentException("Infinity/NaN not supported in DecimalX (yet)");

            var val = BitConverter.GetBytes(v);
            dbytes = new byte[Marshal.SizeOf(v)];
            Array.Copy(val, dbytes, Marshal.SizeOf(v));

            significand = IntegerX.GetDoubleSignificand(dbytes);
            biasedExp = IntegerX.GetDoubleBiasedExponent(dbytes);
            leftShift = biasedExp - DoubleShiftBias;

            if (significand == 0)
            {
                if (biasedExp == 0)
                {
                    return new DecimalX(IntegerX.Zero, 0, 1);
                }
                if (v < 0.0)
                    coeff = IntegerX.NegativeOne;
                else
                    coeff = IntegerX.One;

                leftShift = biasedExp - DoubleExponentBias;
            }
            else
            {
                significand |= 0x10000000000000;
                coeff = IntegerX.Create(significand);

                if (v < 0.0)
                    coeff *= -1;
            }

            expToUse = 0;
            if (leftShift < 0)
            {
                coeff = coeff.Multiply(IntegerX.Five.Power(-leftShift));
                expToUse = leftShift;
            }
            else if (leftShift > 0)
            {
                coeff <<= leftShift;
            }
            
            return new DecimalX(coeff, expToUse); ;
        }

        /// <summary>
        /// Create a <see cref="DecimalX" /> from a double, rounded as specified.
        /// </summary>
        /// <param name="v">The double value</param>
        /// <param name="c">The rounding context</param>
        /// <returns>A <see cref="DecimalX" /> corresponding to the double value, rounded as specified.</returns>
        /// <remarks>Watch out!  BigDecimal.Create(0.1) is not the same as BigDecimal.Parse("0.1").
        /// We create exact representations of doubles,
        /// and 1/10 does not have an exact representation as a double.  So the double 1.0 is not exactly 1/10.</remarks>
        public static DecimalX Create(double v, MathContext c)
        {
            var d = Create(v);
            d.RoundInPlace(c);
            return d;
        }

        /// <summary>
        /// Create a <see cref="DecimalX" /> with the same value as the given
        /// Integer.
        /// </summary>
        /// <param name="v">The initial value</param>
        /// <returns>A <see cref="DecimalX" /> with the same value.</returns>
        public static DecimalX Create(int v) => new DecimalX(IntegerX.Create(v), 0);

        /// <summary>
        /// Create a <see cref="DecimalX" /> with the same value as the given Integer, rounded appropriately.
        /// </summary>
        /// <param name="v">The initial value</param>
        /// <param name="c">The rounding context</param>
        /// <returns>A <see cref="DecimalX" /> with the same value, appropriately rounded</returns>
        public static DecimalX Create(int v, MathContext c)
        {
            var d = new DecimalX(IntegerX.Create(v), 0);

            d.RoundInPlace(c);
            return d;
        }

        /// <summary>
        /// Create a <see cref="DecimalX" /> with the same value as the given UInt.
        /// </summary>
        /// <param name="v">The initial value</param>
        /// <returns>A <see cref="DecimalX" /> with the same value.</returns>
        public static DecimalX Create(uint v) => new DecimalX(IntegerX.Create(v), 0);

        /// <summary>
        /// Create a <see cref="DecimalX" /> with the same value as the given UInt, rounded appropriately.
        /// </summary>
        /// <param name="v">The initial value</param>
        /// <param name="c">The rounding context</param>
        /// <returns>A <see cref="DecimalX" /> with the same value, appropriately rounded</returns>
        public static DecimalX Create(uint v, MathContext c)
        {
            var d = new DecimalX(IntegerX.Create(v), 0);

            d.RoundInPlace(c);
            return d;
        }

        /// <summary>
        /// Create a <see cref="DecimalX" /> with the same value as the given Long value.
        /// </summary>
        /// <param name="v">The initial value</param>
        /// <returns>A <see cref="DecimalX" /> with the same value.</returns>
        public static DecimalX Create(long v) => new DecimalX(IntegerX.Create(v), 0);

        /// <summary>
        /// Create a <see cref="DecimalX" /> with the same value as the given Long value, rounded appropriately.
        /// </summary>
        /// <param name="v">The initial value</param>
        /// <param name="c">The rounding context</param>
        /// <returns>A <see cref="DecimalX" /> with the same value, appropriately rounded</returns>
        public static DecimalX Create(long v, MathContext c)
        {
            var d = new DecimalX(IntegerX.Create(v), 0);

            d.RoundInPlace(c);
            return d;
        }

        /// <summary>
        /// Create a <see cref="DecimalX" /> with the same value as the given ULong.
        /// </summary>
        /// <param name="v">The initial value</param>
        /// <returns>A <see cref="DecimalX" /> with the same value.</returns>
        public static DecimalX Create(ulong v) => new DecimalX(IntegerX.Create(v), 0);

        /// <summary>
        /// Create a <see cref="DecimalX" /> with the same value as the given ULong, rounded appropriately.
        /// </summary>
        /// <param name="v">The initial value</param>
        /// <param name="c">The rounding context</param>
        /// <returns>A <see cref="DecimalX" /> with the same value, appropriately rounded</returns>
        public static DecimalX Create(ulong v, MathContext c)
        {
            var d = new DecimalX(IntegerX.Create(v), 0);

            d.RoundInPlace(c);
            return d;
        }

        /// <summary>
        /// Create a <see cref="DecimalX" /> with the same value as the given <see cref="IntegerX" />.
        /// </summary>
        /// <param name="v">The initial value</param>
        /// <returns>A <see cref="DecimalX" /> with the same value.</returns>
        public static DecimalX Create(IntegerX v) => new DecimalX(v, 0);

        /// <summary>
        /// Create a <see cref="DecimalX" /> with the same value as the given <see cref="IntegerX" />, rounded appropriately.
        /// </summary>
        /// <param name="v">The initial value</param>
        /// <param name="c">The rounding context</param>
        /// <returns>A <see cref="DecimalX" /> with the same value, appropriately rounded</returns>
        public static DecimalX Create(IntegerX v, MathContext c)
        {
            var d = new DecimalX(v, 0);

            d.RoundInPlace(c);
            return d;
        }

        /// <summary>
        /// Create a <see cref="DecimalX" /> by parsing a string.
        /// </summary>
        /// <param name="v">The initial value</param>
        /// <returns>A <see cref="DecimalX" /> with the same value.</returns>
        public static DecimalX Create(string v) => Parse(v);

        /// <summary>
        /// Create a <see cref="DecimalX" /> by parsing a string.
        /// </summary>
        /// <param name="v">The initial value</param>
        /// <param name="c">The context to use</param>
        /// <returns>A <see cref="TDecimalX" /> treated according to the passed context.</returns>
        public static DecimalX Create(string v, MathContext c) => Parse(v, c);

        /// <summary>
        /// Create a <see cref="DecimalX" /> by parsing a character array.
        /// </summary>
        /// <param name="v">The initial value</param>
        /// <returns>A <see cref="DecimalX" /> with the same value.</returns>
        public static DecimalX Create(char[] v) => Parse(v);

        /// <summary>
        /// Create a <see cref="DecimalX" /> by parsing a segment of character array.
        /// </summary>
        /// <param name="v">The initial value</param>
        /// <param name="offset">offset to start from</param>
        /// <param name="len">length</param>
        /// <returns>A <see cref="DecimalX" /> containing processed value.</returns>
        public static DecimalX Create(char[] v, int offset, int len) => Parse(v, offset, len);

        /// <summary>
        /// Create a <see cref="DecimalX" /> by parsing a segment of character array.
        /// </summary>
        /// <param name="v">The initial value</param>
        /// <param name="offset">offset to start from</param>
        /// <param name="len">length</param>
        /// <param name="c">The context to use</param>
        /// <returns>A <see cref="DecimalX" /> containing processed value according to passed context.</returns>
        public static DecimalX Create(char[] v, int offset, int len, MathContext c) => Parse(v, offset, len, c);

        #endregion

        #region PARSE OVERLOADS & UTILS

        /// <summary>
        /// Create a <see cref="DecimalX" /> from a string representation
        /// </summary>
        /// <param name="S">String to parse into <see cref="DecimalX" /></param>
        /// <returns>A <see cref="DecimalX" /> containing processed value.</returns>
        public static DecimalX Parse(string S)
        {
            DecimalX v;
            DoParse(S.ToCharArray(), 0, S.Length, true, out v);

            return v;
        }

        /// <summary>
        /// Create a <see cref="DecimalX" /> from a string representation, rounded as indicated.
        /// </summary>
        /// <param name="S">String to parse into <see cref="DecimalX" /></param>
        /// <param name="c">The math context to use</param>
        /// <returns>A <see cref="DecimalX" /> containing processed value according to passed context.</returns>
        public static DecimalX Parse(string S, MathContext c)
        {
            DecimalX v;
            DoParse(S.ToCharArray(), 0, S.Length, true, out v);
            v.RoundInPlace(c);

            return v;
        }

        /// <summary>
        /// Create a <see cref="DecimalX" /> from an array of characters.
        /// </summary>
        /// <param name="buf">input char array</param>
        /// <returns>A <see cref="DecimalX" /> containing processed value.</returns>
        public static DecimalX Parse(char[] buf)
        {
            DecimalX v;
            DoParse(buf, 0, buf.Length, true, out v);

            return v;
        }

        /// <summary>
        /// Create a <see cref="DecimalX" /> from an array of characters, rounded as indicated.
        /// </summary>
        /// <param name="buf">input char array</param>
        /// <param name="c">math context to use</param>
        /// <returns>A <see cref="DecimalX" /> containing processed value according to passed context.</returns>
        public static DecimalX Parse(char[] buf, MathContext c)
        {
            DecimalX v;
            DoParse(buf, 0, buf.Length, true, out v);
            v.RoundInPlace(c);

            return v;
        }

        /// <summary>
        /// Create a <see cref="DecimalX" /> corresponding to a sequence of characters from an array.
        /// </summary>
        /// <param name="buf">input char array</param>
        /// <param name="offset">offset to start from</param>
        /// <param name="len">length</param>
        /// <returns>A <see cref="DecimalX" /> containing processed value.</returns>
        public static DecimalX Parse(char[] buf, int offset, int len)
        {
            DecimalX v;
            DoParse(buf, offset, len, true, out v);

            return v;
        }

        /// <summary>
        /// Create a <see cref="DecimalX" /> corresponding to a sequence of characters from an array, rounded as indicated.
        /// </summary>
        /// <param name="buf">input char array</param>
        /// <param name="offset">offset to start from</param>
        /// <param name="len">length</param>
        /// <param name="c">math context to use</param>
        /// <returns>A <see cref="DecimalX" /> containing processed value according to passed context.</returns>
        public static DecimalX Parse(char[] buf, int offset, int len, MathContext c)
        {
            DecimalX v;
            DoParse(buf, offset, len, true, out v);
            v.RoundInPlace(c);

            return v;
        }



        /// <summary>
        /// Try to create a <see cref="DecimalX" /> from a string representation.
        /// </summary>
        /// <param name="S">The string to convert</param>
        /// <param name="v">Set to the <see cref="DecimalX" /> corresponding to the string.</param>
        /// <returns>True if successful, false if there is an error parsing.</returns>
        public static bool TryParse(string S, out DecimalX v) => DoParse(S.ToCharArray(), 0, S.Length, false, out v);

        /// <summary>
        /// Try to create a <see cref="DecimalX" /> from a string representation, rounded as indicated.
        /// </summary>
        /// <param name="S">The string to convert</param>
        /// <param name="c">The rounding math context</param>
        /// <param name="v">Set to the <see cref="DecimalX" /> corresponding to the string.</param>
        /// <returns>True if successful, false if there is an error parsing.</returns>
        public static bool TryParse(string S, MathContext c, out DecimalX v)
        {
            var res = DoParse(S.ToCharArray(), 0, S.Length, false, out v);

            if (res)
                v.RoundInPlace(c);
            return res;
        }

        /// <summary>
        /// Try to create a <see cref="DecimalX" /> from an array of characters, rounded as indicated.
        /// </summary>
        /// <param name="buf">input char array</param>
        /// <param name="c">context to use</param>
        /// <param name="v">out result</param>
        /// <returns>True if successful; false otherwise</returns>
        public static bool TryParse(char[] buf, MathContext c, out DecimalX v)
        {
            var res = DoParse(buf, 0, buf.Length, false, out v);

            if (res)
                v.RoundInPlace(c);
            return res;
        }

        /// <summary>
        /// Try to create a <see cref="DecimalX" /> from an array of characters.
        /// </summary>
        /// <param name="buf">input char array</param>
        /// <param name="v">out result</param>
        /// <returns>True if successful; false otherwise</returns>
        public static bool TryParse(char[] buf, out DecimalX v) => DoParse(buf, 0, buf.Length, false, out v);

        /// <summary>
        /// Try to create a <see cref="DecimalX" /> corresponding to a sequence of characters from an array.
        /// </summary>
        /// <param name="buf">input char array</param>
        /// <param name="offset">offset to start from</param>
        /// <param name="len">length</param>
        /// <param name="v">out result</param>
        /// <returns>True if successful; false otherwise</returns>
        public static bool TryParse(char[] buf, int offset, int len, out DecimalX v) => DoParse(buf, offset, len, false, out v);

        /// <summary>
        /// Try to create a <see cref="DecimalX" /> corresponding to a sequence of characters from an array.
        /// </summary>
        /// <param name="buf">input char array</param>
        /// <param name="offset">offset to start from</param>
        /// <param name="len">length</param>
        /// <param name="c">context to use</param>
        /// <param name="v">out result</param>
        /// <returns>True if successful; false otherwise</returns>
        public static bool TryParse(char[] buf, int offset, int len, MathContext c, out DecimalX v)
        {
            var res = DoParse(buf, offset, len, false, out v);

            if (res)
                v.RoundInPlace(c);

            return res;
        }

        /// <summary>
        /// Parse a substring of a character array as a <see cref="DecimalX" />.
        /// </summary>
        /// <param name="buf">
        /// The character array to parse
        /// </param>
        /// <param name="offset">
        /// Start index for parsing
        /// </param>
        /// <param name="len">
        /// Number of chars to parse.
        /// </param>
        /// <param name="throwOnError">
        /// If true, an error causes an exception to be thrown. If false, false
        /// is returned.
        /// </param>
        /// <param name="v">
        /// The <see cref="DecimalX" /> corresponding to the characters.
        /// </param>
        /// <returns>
        /// True if successful, false if not (or throws if throwOnError is true).
        /// </returns>
        /// <remarks>
        /// Ugly. We could use a RegEx, but trying to avoid unnecessary
        /// allocation, I guess. [+-]?\d*(\.\d*)?([Ee][+-]?\d+)? with additional
        /// constraint that one of the two d* must have at least one char.
        /// </remarks>
        private static bool DoParse(char[] buf, int offset, int len, bool throwOnError, out DecimalX v)
        {
            int mainOffset, signedMainLen, signState, mainLen, fractionOffset, fractionLen, expOffset, expLen, Precision, exp, expToUse, i;
            bool hasSign;
            char c;
            char[] LDigits, expDigits;
            IntegerX val;
            string MyString, expString;

            v = default(DecimalX);

            if (len == 0)
            {
                if (throwOnError)
                    throw new FormatException("Empty string");
                return false;
            }

            // Make sure we're not going past the end of the array
            if ((offset + len) > buf.Length)
            {
                if (throwOnError)
                    throw new FormatException("offset + len past the end of the char array");
                return false;
            }

            mainOffset = offset;

            // optional leading sign
            hasSign = false;
            c = buf[offset];

            if ((c == '-') || (c == '+'))
            {
                hasSign = true;
                offset++;
                len--;
            }

            while ((len > 0) && (char.IsDigit(buf[offset])))
            {
                offset++;
                len--;
            }

            signedMainLen = offset - mainOffset;
            if (hasSign)
                signState = 1;
            else
                signState = 0;

            mainLen = offset - mainOffset - signState;

            // parse the optional fraction
            fractionOffset = offset;
            fractionLen = 0;

            //TODO revise NumberDecimalSeperator
            // using a FormatSettings so that library will be Locale - Aware
            if ((len > 0) && (buf[offset].ToString() == MathContext._FCS.NumberFormat.NumberDecimalSeparator))
            {
                offset++;
                len--;
                fractionOffset = offset;

                while ((len > 0) && (char.IsDigit(buf[offset])))
                {
                    offset++;
                    len--;
                }

                fractionLen = offset - fractionOffset;
            }

            // Parse the optional exponent.
            expOffset = -1;
            expLen = 0;

            if ((len > 0) && ((buf[offset] == 'e') || (buf[offset] == 'E')))
            {
                offset++;
                len--;

                expOffset = offset;
                if (len == 0)
                {
                    if (throwOnError)
                        throw new FormatException("Missing exponent");
                    return false;
                }

                // Parse the optional sign;
                c = buf[offset];
                if ((c == '-') || (c == '+'))
                {
                    offset++;
                    len--;
                }

                if (len == 0)
                {
                    if (throwOnError)
                        throw new FormatException("Missing exponent");
                    return false;
                }

                while ((len > 0) && (char.IsDigit(buf[offset])))
                {
                    offset++;
                    len--;
                }

                expLen = offset - expOffset;
                if (expLen == 0)
                {
                    if (throwOnError)
                        throw new FormatException("Missing exponent");
                    return false;
                }
            }

            if (len != 0)
            {
                if (throwOnError)
                    throw new FormatException("Unused characters at end");
                return false;
            }

            Precision = mainLen + fractionLen;
            if (Precision == 0)
            {
                if (throwOnError)
                    throw new FormatException("No digits in coefficient");
                return false;
            }

            LDigits = new char[signedMainLen + fractionLen];
            Array.Copy(buf, mainOffset, LDigits, 0, signedMainLen);

            if (fractionLen > 0)
                Array.Copy(buf, fractionOffset, LDigits, signedMainLen, fractionLen);

            MyString = new string(LDigits);
            val = IntegerX.Parse(MyString);

            exp = 0;
            if (expLen > 0)
            {
                expDigits = new char[expLen];
                Array.Copy(buf, expOffset, expDigits, 0, expLen);
                expString = new string(expDigits);

                if (throwOnError)
                {
                    exp = int.Parse(expString);
                }
                else
                {
                    if (!int.TryParse(expString, out exp))
                        return false;
                }
            }

            expToUse = mainLen - Precision;

            if (exp != 0)
            {
                try
                {
                    expToUse = CheckExponent(expToUse + exp, val.IsZero());
                }
                catch (ArithmeticException)
                {
                    if (throwOnError)
                        throw;
                    return false;
                }
            }

            if (hasSign)
                i = 1;
            else
                i = 0;

            // Remove leading zeros from precision count.
            while ((i < (signedMainLen + fractionLen)) && (Precision > 1) && (LDigits[i] == '0'))
            {
                i++;
                Precision--;
            }

            v = new DecimalX(val, expToUse, (uint)Precision);

            return true;
        }

        #endregion

        #region EXPLICIT OPERATORS

        public static explicit operator double(DecimalX value)
        {
            // As j.m.BigDecimal puts it: "Somewhat inefficient, but guaranteed to work."
            // However, JVM's double parser goes to +/- Infinity when out of range,
            // while CLR's throws an exception.
            // Hate dealing with that.
            try
            {
                return double.Parse(value.ToString(), MathContext._FCS);
            }
            catch (OverflowException)
            {
                if (value.IsNegative())
                    return DoubleNegativeInfinity;
                else
                    return DoublePositiveInfinity;
            }
        }

        public static explicit operator byte(DecimalX value) => (byte)((IntegerX)value);

        public static explicit operator sbyte(DecimalX value) => (sbyte)((IntegerX)value);

        public static explicit operator short(DecimalX value) => (short)((IntegerX)value);

        public static explicit operator ushort(DecimalX value) => (ushort)((IntegerX)value);

        public static explicit operator int(DecimalX value) => (int)((IntegerX)value);

        public static explicit operator uint(DecimalX value) => (uint)((IntegerX)value);

        public static explicit operator long(DecimalX value) => (long)((IntegerX)value);

        public static explicit operator ulong(DecimalX value) => (ulong)((IntegerX)value);

        public static explicit operator IntegerX(DecimalX value) => Rescale(value, 0, RoundingMode.Down)._coeff;
        #endregion

        #region OPERATOR OVERLOADS

        public static bool operator ==(DecimalX x, DecimalX y) => x.Equals(y);

        public static bool operator !=(DecimalX x, DecimalX y) => !(x == y);

        public static bool operator <(DecimalX x, DecimalX y) => x.CompareTo(y) < 0;

        public static bool operator >(DecimalX x, DecimalX y) => x.CompareTo(y) > 0;

        /// <summary>
        /// Compute <paramref name="x"/> + <paramref name="y"/>.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>The sum</returns>
        public static DecimalX operator +(DecimalX x, DecimalX y) => x.Add(y);

        /// <summary>
        /// Compute <paramref name="x"/> - <paramref name="y"/>.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>The difference</returns>
        public static DecimalX operator -(DecimalX x, DecimalX y) => x.Subtract(y);

        /// <summary>
        /// returns +<paramref name="x"/>.
        /// </summary>
        /// <param name="x"></param>
        /// <returns>This</returns>
        public static DecimalX operator +(DecimalX x)
        {
            if (x.IsNegative())
            {
                return x.Negate();
            }

            return x;
        }

        /// <summary>
        /// Compute the negation of <paramref name="x"/>.
        /// </summary>
        /// <param name="x"></param>
        /// <returns>The negation</returns>
        public static DecimalX operator -(DecimalX x) => x.Negate();

        /// <summary>
        /// Compute <paramref name="x"/> * <paramref name="y"/>.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>The product</returns>
        public static DecimalX operator *(DecimalX x, DecimalX y) => x.Multiply(y);

        /// <summary>
        /// Compute <paramref name="x"/> / <paramref name="y"/>.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>The quotient</returns>
        public static DecimalX operator /(DecimalX x, DecimalX y) => x.Divide(y);

        /// <summary>
        /// Compute <paramref name="x"/> % <paramref name="y"/>.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>The quotient</returns>
        public static DecimalX operator %(DecimalX x, DecimalX y) => x.Modulus(y);

        #endregion

        #region OPERATIONS

        /// <summary>
        /// Compute <paramref name="x"/> + <paramref name="y"/>.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>The sum</returns>
        public static DecimalX Add(DecimalX x, DecimalX y) => x.Add(y);

        /// <summary>
        /// Compute <paramref name="x"/> + <paramref name="y"/> with the result rounded per the context.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="c">context to use</param>
        /// <returns>The sum</returns>
        public static DecimalX Add(DecimalX x, DecimalX y, MathContext c) => x.Add(y, c);

        /// <summary>
        /// Compute <paramref name="x"/> - <paramref name="y"/>.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>The difference</returns>
        public static DecimalX Subtract(DecimalX x, DecimalX y) => x.Subtract(y);

        /// <summary>
        /// Compute <paramref name="x"/> - <paramref name="y"/> with the result rounded per the context.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="c">context to use</param>
        /// <returns>The difference</returns>
        public static DecimalX Subtract(DecimalX x, DecimalX y, MathContext c) => x.Subtract(y, c);

        /// <summary>
        /// Compute the negation of <paramref name="x"/>.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>The negation</returns>
        public static DecimalX Negate(DecimalX x) => x.Negate();

        /// <summary>
        /// Compute the negation of <paramref name="x"/>, with result rounded according to the context
        /// </summary>
        /// <param name="x"></param>
        /// <param name="c">context to use</param>
        /// <returns>The negation</returns>
        public static DecimalX Negate(DecimalX x, MathContext c) => x.Negate(c);

        /// <summary>
        /// Compute <paramref name="x"/> * <paramref name="y"/>.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>The product</returns>
        public static DecimalX Multiply(DecimalX x, DecimalX y) => x.Multiply(y);

        /// <summary>
        /// Compute <paramref name="x"/> * <paramref name="y"/>, with result rounded according to the context.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="c">context to use</param>
        /// <returns>The product</returns>
        public static DecimalX Multiply(DecimalX x, DecimalX y, MathContext c) => x.Multiply(y, c);

        /// <summary>
        /// Compute <paramref name="x"/> / <paramref name="y"/>.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>The quotient</returns>
        public static DecimalX Divide(DecimalX x, DecimalX y) => x.Divide(y);

        /// <summary>
        /// Compute <paramref name="x"/> / <paramref name="y"/>, with result rounded according to the context.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="c">context to use</param>
        /// <returns>The quotient</returns>
        public static DecimalX Divide(DecimalX x, DecimalX y, MathContext c) => x.Divide(y, c);

        /// <summary>
        /// Returns <paramref name="x"/> mod <paramref name="y"/>.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>The modulus</returns>
        public static DecimalX Modulus(DecimalX x, DecimalX y) => x.Modulus(y);

        /// <summary>
        /// Returns <paramref name="x"/> mod <paramref name="y"/>, with result rounded according to the context.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="c">context to use</param>
        /// <returns>The modulus</returns>
        public static DecimalX Modulus(DecimalX x, DecimalX y, MathContext c) => x.Modulus(y, c);

        /// <summary>
        /// Compute the quotient and remainder of dividing one <see cref="TDecimalX"/> by another.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="remainder">Set to the remainder after division</param>
        /// <returns>The quotient</returns>
        public static DecimalX DivRem(DecimalX x, DecimalX y, out DecimalX remainder) => x.DivRem(y, out remainder);

        /// <summary>
        /// Compute the quotient and remainder of dividing one <see cref="TDecimalX"/> by another, with result rounded according to the context.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="c">context to use</param>
        /// <param name="remainder">Set to the remainder after division</param>
        /// <returns>The quotient</returns>
        public static DecimalX DivRem(DecimalX x, DecimalX y, MathContext c, out DecimalX remainder) => x.DivRem(y, c, out remainder);

        /// <summary>
        /// Compute the absolute value.
        /// </summary>
        /// <param name="x"></param>
        /// <returns>The absolute value</returns>
        public static DecimalX Abs(DecimalX x) => x.Abs();

        /// <summary>
        /// Compute the absolute value, with result rounded according to the context.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="c">context to use</param>
        /// <returns>The absolute value</returns>
        public static DecimalX Abs(DecimalX x, MathContext c) => x.Abs(c);

        /// <summary>
        /// Computes a <see cref="IntegerX"/> raised to an integer power.
        /// </summary>
        /// <param name="x">The value to exponentiate</param>
        /// <param name="exp">The exponent</param>
        /// <returns>The exponent</returns>
        public static DecimalX Power(DecimalX x, int exp) => x.Power(exp);

        /// <summary>
        /// Computes a <see cref="IntegerX"/> raised to an integer power, with result rounded according to the context.
        /// </summary>
        /// <param name="x">The value to exponentiate</param>
        /// <param name="exp">The exponent</param>
        /// <param name="c">context to use</param>
        /// <returns>The exponent</returns>
        public static DecimalX Power(DecimalX x, int exp, MathContext c) => x.Power(exp, c);

        /// <summary>
        /// Returns "x".
        /// </summary>
        /// <param name="x"></param>
        /// <returns>"x"</returns>
        public static DecimalX Plus(DecimalX x)
        {
            if (x.IsNegative())
                return x.Negate();
            return x;
        }

        /// <summary>
        /// Returns "x" rounded to context.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="c">context to use</param>
        /// <returns>"x" rounded to context</returns>
        public static DecimalX Plus(DecimalX x, MathContext c) => x.Plus(c);

        /// <summary>
        /// Returns the negation of "x".
        /// </summary>
        /// <param name="x"></param>
        /// <returns>"x" negated</returns>
        public static DecimalX Minus(DecimalX x) => x.Negate();

        /// <summary>
        /// Returns the negation of "x" rounded to context.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="c">context to use</param>
        /// <returns>"x" negated and rounded to context</returns>
        public static DecimalX Minus(DecimalX x, MathContext c) => x.Negate(c);

        /// <summary>
        /// Returns self + y.
        /// </summary>
        /// <param name="y">The augend.</param>
        /// <returns>The sum</returns>
        public DecimalX Add(DecimalX y)
        {
            var x = this;
            Align(ref x, ref y);

            return new DecimalX(x._coeff + y._coeff, x._exp);
        }

        /// <summary>
        /// Returns self + y, with the result rounded according to the context.
        /// </summary>
        /// <param name="y">The augend.</param>
        /// <param name="c">context to use.</param>
        /// <returns>The sum</returns>
        /// <remarks>Translated the Sun Java code pretty directly.</remarks>
        public DecimalX Add(DecimalX y, MathContext c)
        {
            var tempRes = Add(y);

            if ((c.Precision == 0) || (c.RoundingMode == RoundingMode.Unnecessary))
                return tempRes;

            return tempRes.Round(c);
        }

        /// <summary>
        /// Returns self - y
        /// </summary>
        /// <param name="y">The subtrahend</param>
        /// <returns>The difference</returns>
        public DecimalX Subtract(DecimalX y)
        {
            var x = this;
            Align(ref x, ref y);

            return new DecimalX(x._coeff - y._coeff, x._exp);
        }

        /// <summary>
        /// Returns self - y
        /// </summary>
        /// <param name="y">The subtrahend</param>
        /// <param name="c">context to use</param>
        /// <returns>The difference</returns>
        public DecimalX Subtract(DecimalX y, MathContext c)
        {
            var tempRes = Subtract(y);

            if ((c.Precision == 0) || (c.RoundingMode == RoundingMode.Unnecessary))
                return tempRes;

            return tempRes.Round(c);
        }

        /// <summary>
        /// Returns the negation of this value.
        /// </summary>
        /// <returns>The negation</returns>
        public DecimalX Negate()
        {
            if (_coeff.IsZero())
                return this;

            return new DecimalX(-_coeff, _exp, _precision);
        }

        /// <summary>
        /// Returns the negation of this value, with result rounded according to the context.
        /// </summary>
        /// <param name="c">context to use</param>
        /// <returns>The negation rounded to context</returns>
        public DecimalX Negate(MathContext c) => Round(Negate(), c);

        /// <summary>
        /// Returns self * y
        /// </summary>
        /// <param name="y">The multiplicand</param>
        /// <returns>The product</returns>
        public DecimalX Multiply(DecimalX y) => new DecimalX(_coeff * y._coeff, _exp + y._exp);

        /// <summary>
        /// Returns self * y
        /// </summary>
        /// <param name="y">The multiplicand</param>
        /// <param name="c">context to use</param>
        /// <returns>The product</returns>
        public DecimalX Multiply(DecimalX y, MathContext c)
        {
            var d = Multiply(y);
            d.RoundInPlace(c);
            return d;
        }

        /// <summary>
        /// Returns self / divisor.
        /// </summary>
        /// <param name="divisor">The divisor</param>
        /// <returns>The division result</returns>
        /// <exception cref="ArithmeticException">If rounding mode is RoundingMode.UNNECESSARY and we have a repeating fraction"</exception>
        public DecimalX Divide(DecimalX divisor)
        {
            DecimalX quotient;
            int preferredExp, quotientExp;
            MathContext c;

            var dividend = this;
            if (divisor._coeff.IsZero())
            {
                if (dividend._coeff.IsZero())
                    throw new ArithmeticException("Division undefined (0/0)");
                throw new ArithmeticException("Division by zero");
            }

            // Calculate preferred exponent
            preferredExp = (int)(Math.Max(Math.Min((long)dividend._exp - divisor._exp, MaxIntValue), MinIntValue));

            if (dividend._coeff.IsZero())
                return new DecimalX(IntegerX.Zero, preferredExp);

            /*  OpenJDK says:
            * If the quotient self/divisor has a terminating decimal
            * expansion, the expansion can have no more than
            * (a.precision() + Math.Ceiling(10*b.precision)/3) digits.
            * Therefore, create a Context object with this
            * precision and do a divide with the UNNECESSARY rounding
            * mode.
            */

            c = new MathContext((uint)Math.Min(dividend.Precision + (long)(Math.Ceiling(10.0 * divisor.Precision / 3.0)), MaxIntValue), RoundingMode.Unnecessary);

            try
            {
                quotient = dividend.Divide(divisor, c);
            }
            catch (ArithmeticException)
            {
                throw new ArithmeticException("Non-terminating decimal expansion; no exact representable decimal result");
            }

            quotientExp = quotient._exp;

            // divide(DecimalX, c) tries to adjust the quotient to
            // the desired one by removing trailing zeros; since the
            // exact divide method does not have an explicit digit
            // limit, we can add zeros too.

            if (preferredExp < quotientExp)
                return Rescale(quotient, preferredExp, RoundingMode.Unnecessary);

            return quotient;
        }

        /// <summary>
        /// Returns self / rhs.
        /// </summary>
        /// <param name="rhs">right hand side (divisor)</param>
        /// <param name="c">The context</param>
        /// <returns>The division result</returns>
        /// <remarks>
        /// <para>The specification talks about the division algorithm in terms of repeated subtraction.
        /// I'll try to re-analyze this in terms of divisions on integers.</para>
        /// <para>Assume we want to divide one <see cref="DecimalX" /> by another:</para>
        /// <code> [x,a] / [y,b] = [(x/y), a-b]</code>
        /// <para>where [x,a] signifies x is integer, a is exponent so [x,a] has value x * 10^a.
        /// Here, (x/y) indicates a result rounded to the desired precision p. For the moment, assume x, y non-negative.</para>
        /// <para>We want to compute (x/y) using integer-only arithmetic, yielding a quotient+remainder q+r
        /// where q has up to p precision and r is used to compute the rounding.  So actually, the result will be [q, a-b+c],
        /// where c is some adjustment factor to make q be in the range [0,10^0).</para>
        /// <para>We will need to adjust either x or y to make sure we can compute x/y and make q be in this range.</para>
        /// <para>Let px be the precision of x (number of digits), let py be the precision of y. Then </para>
        /// <code>
        /// x = x' * 10^px
        /// y = y' * 10^py
        /// </code>
        /// <para>where x' and y' are in the range [.1,1).  However, we'd really like to have:</para>
        /// <code>
        /// (a) x' in [.1,1)
        /// (b) y' in [x',10*x')
        /// </code>
        /// <para>So that  x'/y' is in the range (.1,1].
        /// We can use y' as defined above if y' meets (b), else multiply y' by 10 (and decrease py by 1).
        /// Having done this, we now have</para>
        /// <code>
        /// x/y = (x'/y') * 10^(px-py)
        /// </code>
        /// <para>
        /// This gives us
        /// <code>
        /// 10^(px-py-1) &lt; x/y &lt; 10^(px-py)
        /// </code>
        /// We'd like q to have p digits of precision.  So,
        /// </para>
        /// <code>
        /// if px-py = p, ok.
        /// if px-py &lt; p, multiply x by 10^(p - (px-py)).
        /// if px-py &gt; p, multiply y by 10^(px-py-p).
        /// </code>
        /// <para>Using these adjusted values of x and y, divide to get q and r, round using those, then adjust the exponent.</para>
        /// </remarks>
        public DecimalX Divide(DecimalX rhs, MathContext c)
        {
            DecimalX lhs, tempRes;
            IntegerX x, y, xtest, ytest, roundedInt;
            long preferredExp;
            int xprec, yprec, adjust, delta, exp;

            if (c.Precision == 0)
                return Divide(rhs);

            lhs = this;
            preferredExp = (long)lhs._exp - rhs._exp;

            // Deal with x or y being zero.

            if (rhs._coeff.IsZero())
            {
                if (lhs._coeff.IsZero())
                    throw new ArithmeticException("Division undefined (0/0)");
                throw new ArithmeticException("Division by zero");
            }

            if (lhs._coeff.IsZero())
                return new DecimalX(IntegerX.Zero, (int)(Math.Max(Math.Min(preferredExp, MaxIntValue), MinIntValue)));

            xprec = (int)lhs.Precision;
            yprec = (int)rhs.Precision;

            // Determine if we need to make an adjustment to get x', y' into relation (b).
            x = lhs._coeff;
            y = rhs._coeff;

            xtest = IntegerX.Abs(x);
            ytest = IntegerX.Abs(y);

            if (xprec < yprec)
                xtest = x * BIPowerOfTen(yprec - xprec);
            else if (xprec > yprec)
                ytest = y * BIPowerOfTen(xprec - yprec);

            adjust = 0;
            if (ytest < xtest)
            {
                y *= IntegerX.Ten;
                adjust = 1;
            }

            // Now make sure x and y themselves are in the proper range.

            delta = (int)c.Precision - (xprec - yprec);
            if (delta > 0)
                x *= BIPowerOfTen(delta);
            else if (delta < 0)
                y *= BIPowerOfTen(-delta);

            roundedInt = RoundingDivide2(x, y, c.RoundingMode);

            exp = CheckExponent(preferredExp - delta + adjust, roundedInt.IsZero());

            tempRes = new DecimalX(roundedInt, exp);

            tempRes.RoundInPlace(c);

            if (tempRes.Multiply(rhs).CompareTo(this) == 0)
                return tempRes.StripZerosToMatchExponent(preferredExp);
            else
                return tempRes;
        }

        /// <summary>
        /// Returns self mod y
        /// </summary>
        /// <param name="y">The divisor</param>
        /// <returns>The modulus</returns>
        public DecimalX Modulus(DecimalX y)
        {
            DecimalX r;

            DivRem(y, out r);
            return r;
        }

        /// <summary>
        /// Returns self mod y
        /// </summary>
        /// <param name="y">The divisor</param>
        /// <param name="c">The context</param>
        /// <returns>The modulus</returns>
        public DecimalX Modulus(DecimalX y, MathContext c)
        {
            DecimalX r;

            DivRem(y, c, out r);
            return r;
        }

        /// <summary>
        /// Returns the quotient and remainder of self divided by another.
        /// </summary>
        /// <param name="y">The divisor</param>
        /// <param name="remainder">The remainder</param>
        /// <returns>The quotient</returns>
        public DecimalX DivRem(DecimalX y, out DecimalX remainder)
        {
            var q = DivideInteger(y);
            remainder = this - q * y;
            return q;
        }

        /// <summary>
        /// Returns the quotient and remainder of self divided by another.
        /// </summary>
        /// <param name="y">The divisor</param>
        /// <param name="c">The context</param>
        /// <param name="remainder">The remainder</param>
        /// <returns>The quotient</returns>
        public DecimalX DivRem(DecimalX y, MathContext c, out DecimalX remainder)
        {
            if (c.RoundingMode == RoundingMode.Unnecessary)
                return DivRem(y, out remainder);

            var q = DivideInteger(y, c);
            remainder = this - q * y;
            return q;
        }

        /// <summary>
        /// Returns the integer part of self / y.
        /// </summary>
        /// <param name="y"></param>
        /// <param name="c"></param>
        /// <returns>Returns the integer part of self / y.</returns>
        /// <remarks>I am indebted to the OpenJDK implementation for the algorithm.
        /// <para>However, the spec I'm working from specifies an exponent of zero always!
        /// The OpenJDK implementation does otherwise.  So I've modified it to yield a zero exponent.</para>
        /// </remarks>
        public DecimalX DivideInteger(DecimalX y, MathContext c)
        {
            int preferredExp, tempResExp;
            DecimalX tempRes, product;

            if ((c.Precision == 0) || /* exact result */ (Abs().CompareTo(y.Abs()) < 0))
                return DivideInteger(y);

            preferredExp = 0;

            tempRes = Divide(y, new MathContext(c.Precision, RoundingMode.Down));
            tempResExp = tempRes._exp;

            if (tempResExp > 0)
            {
                product = tempRes.Multiply(y);
                if (Subtract(product).Abs().CompareTo(y.Abs()) >= 0)
                    throw new ArithmeticException("Division impossible");
            }
            else if (tempResExp < 0)
            {
                tempRes = Rescale(tempRes, 0, RoundingMode.Down);
            }

            if ((preferredExp < tempResExp) && ((int)(c.Precision - tempRes.Precision) > 0))
                return Rescale(tempRes, 0, RoundingMode.Unnecessary);
            else
                return tempRes.StripZerosToMatchExponent(preferredExp);
        }

        /// <summary>
        /// Returns the integer part of self / y.
        /// </summary>
        /// <param name="y"></param>
        /// <returns>Returns the integer part of self / y.</returns>
        /// <remarks>I am indebted to the OpenJDK implementation for the algorithm.
        /// <para>However, the spec I'm working from specifies an exponent of zero always!
        /// The OpenJDK implementation does otherwise.  So I've modified it to yield a zero exponent.</para>
        /// </remarks>
        public DecimalX DivideInteger(DecimalX y)
        {
            int preferredExp, maxDigits;
            DecimalX quotient;

            preferredExp = 0;

            if (Abs().CompareTo(y.Abs()) < 0)
                return new DecimalX(IntegerX.Zero, preferredExp);

            if ((_coeff.IsZero()) && (!y._coeff.IsZero()))
                return Rescale(this, preferredExp, RoundingMode.Unnecessary);

            maxDigits = (int)(Math.Min(Precision + (long)(Math.Ceiling(10.0 * y.Precision / 3.0)) + Math.Abs((long)_exp - y._exp), MaxIntValue));

            quotient = Divide(y, new MathContext((uint)maxDigits, RoundingMode.Down));

            if (y._exp < 0)
                quotient = Rescale(quotient, 0, RoundingMode.Down).StripZerosToMatchExponent(preferredExp);

            if (quotient._exp > preferredExp)
                quotient = Rescale(quotient, preferredExp, RoundingMode.Unnecessary);

            return quotient;
        }

        /// <summary>
        /// Returns the absolute value of this instance.
        /// </summary>
        /// <returns>The absolute value</returns>
        public DecimalX Abs()
        {
            if (_coeff.IsNegative())
            {
                return Negate();
            }

            return this;
        }

        /// <summary>
        /// Returns the absolute value of this instance.
        /// <param name="c">context to use</param>
        /// </summary>
        /// <returns>The absolute value</returns>
        public DecimalX Abs(MathContext c)
        {
            if (_coeff.IsNegative())
                return Negate(c);

            return Round(this, c);
        }

        /// <summary>
        /// Returns the value of this instance raised to an integral power.
        /// </summary>
        /// <param name="n">The exponent</param>
        /// <returns>The exponentiated value</returns>
        /// <exception cref="ArithmeticException">Thrown if the exponent is negative or exceeds a certain range.</exception>
        public DecimalX Power(int n)
        {
            if ((n < 0) || (n > 999999999))
                throw new ArithmeticException("Invalid operation");

            var exp = CheckExponent((long)_exp * n);
            return new DecimalX(IntegerX.Power(_coeff, n), exp);
        }

        /// <summary>
        /// Returns the value of this instance raised to an integral power.
        /// </summary>
        /// <param name="n">The exponent</param>
        /// <param name="c">context to use</param>
        /// <returns>The exponentiated value</returns>
        /// <remarks>
        /// <para>Follows the OpenJDK implementation.  This is an implementation of the X3.274-1996 algorithm:</para>
        /// <list>
        /// <item> An ArithmeticException exception is thrown if
        /// <list>
        /// <item>Abs(n) > 999999999</item>
        /// <item>c.precision = 0 and code n &lt; 0</item>
        /// <item>c.precision > 0 and n has more than c.precision decimal digits</item>
        /// </list>
        /// </item>
        /// <item>if n is zero, ONE is returned even if this is zero, otherwise
        /// <list>
        /// <item>if n is positive, the result is calculated via
        /// the repeated squaring technique into a single accumulator.
        /// The individual multiplications with the accumulator use the
        /// same context settings as in c except for a
        /// precision increased to c.precision + elength + 1
        /// where elength is the number of decimal digits in n.
        /// </item>
        /// <item>if n is negative, the result is calculated as if
        /// n were positive; this value is then divided into one
        /// using the working precision specified above.
        /// </item>
        /// <item>The final value from either the positive or negative case
        /// is then rounded to the destination precision.
        /// </item>
        /// </list>
        /// </list>
        /// </remarks>
        public DecimalX Power(int n, MathContext c)
        {
            DecimalX lhs, acc;
            MathContext workc;
            int mag, elength, i;
            bool seenbit;

            if (c.Precision == 0)
                return Power(n);

            if ((n < -999999999) || (n > 999999999))
                throw new ArithmeticException("Invalid operation");

            if (n == 0)
                return One;

            lhs = this;
            workc = c;
            mag = Math.Abs(n);
            if (c.Precision > 0)
            {
                elength = (int)(IntegerX.UIntPrecision((uint)mag));
                if ((uint)elength > c.Precision)
                    throw new ArithmeticException("Invalid operation");

                workc = new MathContext(c.Precision + (uint)(elength + 1), c.RoundingMode);
            }

            acc = One;
            seenbit = false;

            i = 1;
            while (true)
            {
                mag += mag;
                if (mag < 0)
                {
                    seenbit = true;
                    acc = acc.Multiply(lhs, workc);
                }

                if (i == 31)
                    break;

                if (seenbit)
                    acc = acc.Multiply(acc, workc);

                i++;
            }

            if (n < 0)
                acc = One.Divide(acc, workc);

            return acc.Round(c);
        }

        public DecimalX Plus() => this;

        public DecimalX Plus(MathContext c)
        {
            if (c.Precision == 0)
                return this;

            return Round(c);
        }

        public DecimalX Minus() => Negate();

        public DecimalX Minus(MathContext c) => Negate(c);

        #endregion

        #region TYPE CONVERSION

        public double ToDouble() => (double)this;

        public byte ToByte() => (byte)this;

        public sbyte ToSByte() => (sbyte)this;

        public short ToShort() => (short)this;

        public ushort ToUshort() => (ushort)this;

        public int ToInt() => (int)this;

        public long ToLong() => (long)this;

        public uint ToUInt() => (uint)this;

        public ulong ToULong() => (ulong)this;

        public IntegerX ToIntegerX() => (IntegerX)this;

        #endregion

        /// <summary>
        /// Create the canonical string representation for a <see cref="DecimalX" />.
        /// </summary>
        /// <returns>string representation of <see cref="DecimalX" />.</returns>
        public string ToScientificString()
        {
            StringBuilder sb;
            int coeffLen, negOffset, numDec, numZeros;
            long adjustedExp;

            sb = new StringBuilder(_coeff.ToString());
            coeffLen = sb.Length;
            negOffset = 0;

            if (_coeff.IsNegative())
            {
                coeffLen--;
                negOffset = 1;
            }

            adjustedExp = (long)_exp + (coeffLen - 1);
            if ((_exp <= 0) && (adjustedExp >= -6))
            {
                // not using exponential notation
                if (_exp != 0)
                {
                    //We do need a decimal point.
                    numDec = -_exp;
                    if (numDec < coeffLen)
                        sb.Insert(coeffLen - numDec + negOffset, MathContext._FCS.NumberFormat.NumberDecimalSeparator);
                    else if (numDec == coeffLen)
                        sb.Insert(negOffset, '0' + MathContext._FCS.NumberFormat.NumberDecimalSeparator);
                    else
                    {
                        numZeros = numDec - coeffLen;
                        sb.Insert(negOffset, "0", numZeros);
                        sb.Insert(negOffset, '0' + MathContext._FCS.NumberFormat.NumberDecimalSeparator);
                    }
                }
            }
            else
            {
                // using exponential notation
                if (coeffLen > 1)
                    sb.Insert(negOffset + 1, MathContext._FCS.NumberFormat.NumberDecimalSeparator);
                sb.Append('E');
                if (adjustedExp >= 0)
                    sb.Append('+');
                sb.Append(string.Format(MathContext._FCS, "{0}", adjustedExp));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Return a string representing the <see cref="DecimalX" /> value.
        /// </summary>
        /// <returns>string representation of <see cref="DecimalX" />.</returns>
        public override string ToString() => ToScientificString();

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            DecimalX dX = obj as DecimalX;
            if (dX == null)
                return false;
            return Equals(dX);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        private DecimalX StripZerosToMatchExponent(long preferredExp)
        {
            IntegerX rem, quo;

            while ((IntegerX.Abs(_coeff).CompareTo(IntegerX.Ten) >= 0) && (_exp < preferredExp))
            {
                if (_coeff.IsOdd())
                    break;

                quo = IntegerX.DivRem(_coeff, IntegerX.Ten, out rem);
                if (!rem.IsZero())
                    break;

                _coeff = quo;
                _exp = CheckExponent((long)_exp + 1); // could overflow
                if (_precision > 0)// adjust precision if known
                    _precision = _precision - 1;
            }

            return this;
        }

        public void RoundInPlace(MathContext c)
        {
            var v = Round(this, c);
            if (v != this)
            {
                _coeff = v._coeff;
                _exp = v._exp;
                _precision = v._precision;
            }
        }

        public DecimalX Round(MathContext c) => Round(this, c);

        /// <summary>
        ///
        /// </summary>
        /// <param name="v"><see cref="DecimalX" /> to process</param>
        /// <param name="c"><see cref="Context" /> to use</param>
        /// <returns>processed <see cref="DecimalX" /></returns>
        /// <remarks>The OpenJDK implementation has an efficiency hack to only compute the precision
        /// (call to .GetPrecision) if the value is outside the range of the context's precision
        /// (-10^precision to 10^precision), with those bounds being cached on the Context.
        /// TODO: See if it is worth implementing the hack.
        /// </remarks>
        public static DecimalX Round(DecimalX v, MathContext c)
        {
            int drop, exp;
            IntegerX divisor, roundedInteger;
            DecimalX tempRes;

            if (v.Precision < c.Precision)
                return v;

            drop = (int)(v._precision - c.Precision);
            if (drop <= 0)
                return v;

            divisor = BIPowerOfTen(drop);

            roundedInteger = RoundingDivide2(v._coeff, divisor, c.RoundingMode);

            exp = CheckExponent((long)v._exp + drop, roundedInteger.IsZero());

            tempRes = new DecimalX(roundedInteger, exp);

            if (c.Precision > 0)
                tempRes.RoundInPlace(c);

            return tempRes;
        }

        /// <summary>
        /// Reduce exponent to Integer.  Throw error if out of range.
        /// </summary>
        /// <param name="candidate">The value resulting from exponent arithmetic.</param>
        /// <param name="isZero">Are we computing an exponent for a zero coefficient?</param>
        /// <returns>The exponent to use</returns>
        public static int CheckExponent(long candidate, bool isZero)
        {
            bool tempRes;
            int Exponent;

            tempRes = CheckExponent(candidate, isZero, out Exponent);

            if (tempRes)
                return Exponent;

            // Report error condition
            if (candidate > MaxIntValue)
                throw new ArithmeticException("Overflow in scale");
            else
                throw new ArithmeticException("Overflow in scale");
        }

        /// <summary>
        /// Check to see if the result of exponent arithmetic is valid.
        /// </summary>
        /// <param name="candidate">The value resulting from exponent arithmetic.</param>
        /// <param name="IsZero">Are we computing an exponent for a zero coefficient?</param>
        /// <param name="Exponent">The exponent to use</param>
        /// <returns>True if the candidate is valid, false otherwise.</returns>
        /// <remarks>
        /// <para>Exponent arithmetic during various operations may result in values
        /// that are out of range of an Integer.  We can do the computation as a long,
        /// then use this to make sure the result is okay to use.</para>
        /// <para>If the exponent is out of range, but the coefficient is zero,
        /// the exponent in some sense is not that relevant, so we just clamp to
        /// the appropriate (pos/neg) extreme value for Integer.  (This handling inspired by
        /// the OpenJDK implementation.)</para>
        /// </remarks>
        public static bool CheckExponent(long candidate, bool IsZero, out int Exponent)
        {
            Exponent = (int)candidate;
            if (Exponent == candidate)
                return true;

            // We have underflow/overflow.
            // If Zero, use the max value of the appropriate sign.
            if (IsZero)
            {
                if (candidate > MaxIntValue)
                    Exponent = MaxIntValue;
                else
                    Exponent = MinIntValue;

                return true;
            }

            return false;
        }

        public int CheckExponent(long candidate) => CheckExponent(candidate, _coeff.IsZero());

        /// <summary>
        /// Assuming
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static IntegerX RoundingDivide2(IntegerX x, IntegerX y, RoundingMode mode)
        {
            IntegerX r, q;
            bool increment, isNeg;
            int cmp;

            q = x.DivRem(y, out r);

            increment = false;
            if (!r.IsZero()) // we need to pay attention
            {
                isNeg = q.IsNegative();

                switch (mode)
                {
                    case RoundingMode.Unnecessary:
                        throw new ArithmeticException("Rounding is prohibited");
                    case RoundingMode.Ceiling:
                        increment = !isNeg;
                        break;
                    case RoundingMode.Floor:
                        increment = isNeg;
                        break;
                    case RoundingMode.Down:
                        increment = false;
                        break;
                    case RoundingMode.Up:
                        increment = true;
                        break;
                    default:
                        cmp = (r + r).Abs().CompareTo(y);
                        switch (mode)
                        {
                            case RoundingMode.HalfDown:
                                increment = cmp > 0;
                                break;
                            case RoundingMode.HalfUp:
                                increment = cmp >= 0;
                                break;
                            case RoundingMode.HalfEven:
                                increment = (cmp > 0) || ((cmp == 0) && (q.TestBit(0)));
                                break;
                        }
                        break;
                }

                if (increment)
                {
                    if ((q.IsNegative()) || ((q.IsZero()) && (x.IsNegative())))
                        q = q - IntegerX.One;
                    else
                        q = q + IntegerX.One;
                }
            }

            return q;
        }

        public static IntegerX BIPowerOfTen(int n)
        {
            char[] buf;
            int i;
            string tempStr;

            if (n < 0)
                throw new ArgumentException("Power of ten must be non-negative");

            if (n < _maxCachedPowerOfTen)
                return _biPowersOfTen[n];

            buf = new char[n + 1];
            buf[0] = '1';
            i = 1;
            while (i <= n)
            {
                buf[i] = '0';
                i++;
            }

            tempStr = new string(buf);

            return IntegerX.Parse(tempStr);
        }

        /// <summary>
        /// Does this <see cref="DecimalX" /> have a zero value?
        /// </summary>
        public bool IsZero() => _coeff.IsZero();

        /// <summary>
        /// Does this <see cref="DecimalX" /> represent a positive value?
        /// </summary>
        public bool IsPositive() => _coeff.IsPositive();

        /// <summary>
        /// Does this <see cref="DecimalX" /> represent a negative value?
        /// </summary>
        public bool IsNegative() => _coeff.IsNegative();

        /// <summary>
        /// Returns the sign (-1, 0, +1) of this <see cref="DecimalX" />.
        /// </summary>
        public int Signum() => _coeff.Signum();

        public DecimalX MovePointRight(int n)
        {
            var newExp = CheckExponent((long)_exp + n);
            var d = new DecimalX(_coeff, newExp);
            return d;
        }

        public DecimalX MovePointLeft(int n)
        {
            var newExp = CheckExponent((long)_exp - n);
            var d = new DecimalX(_coeff, newExp);
            return d;
        }

        /// <summary>
        /// Returns a <see cref="DecimalX" /> numerically equal to this one, but with
        /// any trailing zeros removed.
        /// </summary>
        /// <returns>processed value</returns>
        /// <remarks>Ended up needing this in ClojureCLR, grabbed from OpenJDK.</remarks>
        public DecimalX StripTrailingZeros()
        {
            var tempRes = new DecimalX(_coeff, _exp);
            tempRes.StripZerosToMatchExponent(MaxLongValue);
            return tempRes;
        }

        public int CompareTo(DecimalX other)
        {
            var d1 = this;
            var d2 = other;

            Align(ref d1, ref d2);
            return d1._coeff.CompareTo(d2._coeff);
        }

        public bool Equals(DecimalX other)
        {
            if (_exp != other._exp)
                return false;

            return _coeff.Equals(other._coeff);
        }

        /// <summary>
        /// Change either x or y by a power of 10 in order to align them.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static void Align(ref DecimalX x, ref DecimalX y)
        {
            if (y._exp > x._exp)
                y = ComputeAlign(y, x);
            else if (x._exp > y._exp)
                x = ComputeAlign(x, y);
        }

        /// <summary>
        /// Modify a larger <see cref="DecimalX" /> to have the same exponent as a smaller one by multiplying the coefficient by a power of 10.
        /// </summary>
        /// <param name="big"></param>
        /// <param name="small"></param>
        /// <returns>computed value</returns>
        public static DecimalX ComputeAlign(DecimalX big, DecimalX small)
        {
            var deltaExp = big._exp - small._exp;

            var result = new DecimalX(big._coeff * BIPowerOfTen(deltaExp), small._exp);
            var p = result._precision;
            return result;
        }

        public static DecimalX Rescale(DecimalX lhs, int newExponent, RoundingMode mode)
        {
            int decrease;
            uint p, newPrecision, newPrec;
            DecimalX r;
            IntegerX newCoeff;

            var delta = CheckExponent((long)lhs._exp - newExponent, false);

            if (delta == 0)
                return lhs;

            if (lhs._coeff.IsZero())
                return new DecimalX(IntegerX.Zero, newExponent);

            if (delta < 0)
            {
                decrease = -delta;
                p = lhs.Precision;

                if (p < (uint)decrease)
                    return new DecimalX(IntegerX.Zero, newExponent);

                newPrecision = p - (uint)decrease;
                r = lhs.Round(new MathContext(newPrecision, mode));
                if (r._exp == newExponent)
                    return r;
                else
                    return Rescale(r, newExponent, mode);
            }

            // decreasing the exponent (delta is positive)
            // multiply by an appropriate power of 10
            // Make sure we don't underflow
            newCoeff = lhs._coeff * BIPowerOfTen(delta);
            newPrec = lhs._precision;
            if (newPrec != 0)
                newPrec = newPrec + (uint)delta;

            return new DecimalX(newCoeff, newExponent, newPrec);

        }

        public static DecimalX Quantize(DecimalX lhs, DecimalX rhs, RoundingMode mode) => Rescale(lhs, rhs._exp, mode);

        public DecimalX Quantize(DecimalX v, RoundingMode mode) => Quantize(this, v, mode);
    }
}
