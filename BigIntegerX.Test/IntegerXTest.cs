using System;
using NUnit.Framework;

namespace BigNumberX.Test
{
    [TestFixture]
    public class IntegerXTest
    {
        #region TEST UTILITIES
        private bool SameMag(uint[] xs, uint[] ys)
        {
            int i;
            if (xs.Length != ys.Length)
            {
                return false;
            }

            i = 0;
            while (i < xs.Length)
            {
                if (xs[i] != ys[i])
                {
                    return false;
                }
                i++;
            }

            return true;
        }

        private bool SameValue(IntegerX i, int sign, uint[] mag) => SameSign(i.Signum(), sign) && SameMag(i.GetMagnitude(), mag);

        private bool SameSign(int s1, int s2) => s1 == s2;

        #endregion

        #region BASIC ACCESSOR TESTS
        [TestCase]
        public void Signum_is_zero_for_zero()
        {
            IntegerX i = IntegerX.Create(0);
            Assert.IsTrue(i.Signum() == 0);
        }

        [TestCase]
        public void Magnitude_is_same_for_pos_and_neg()
        {
            IntegerX neg, pos;
            neg = IntegerX.Create(-100);
            pos = IntegerX.Create(100);

            Assert.IsTrue(SameMag(neg.GetMagnitude(), pos.GetMagnitude()));
        }

        [TestCase]
        public void Magnitude_is_zero_length_for_zero()
        {
            var i = IntegerX.Create(0);
            Assert.IsTrue(i.GetMagnitude().Length == 0);
        }

        [TestCase]
        public void Signum_is_m1_for_negative()
        {
            var i = IntegerX.Create(-100);
            Assert.IsTrue(i.Signum() == -1);
        }

        [TestCase]
        public void Signum_is_1_for_negative()
        {
            var i = IntegerX.Create(100);
            Assert.IsTrue(i.Signum() == 1);
        }

        [TestCase]
        public void IsPositive_works()
        {
            IntegerX i;

            i = IntegerX.Create(0);
            Assert.IsTrue(i.IsPositive() == false);

            i = IntegerX.Create(100);
            Assert.IsTrue(i.IsPositive());

            i = IntegerX.Create(-100);
            Assert.IsTrue(i.IsPositive() == false);
        }

        [TestCase]
        public void IsNegative_works()
        {
            IntegerX i;

            i = IntegerX.Create(0);
            Assert.IsTrue(i.IsNegative() == false);

            i = IntegerX.Create(-100);
            Assert.IsTrue(i.IsNegative());

            i = IntegerX.Create(100);
            Assert.IsTrue(i.IsNegative() == false);
        }

        [TestCase]
        public void IsZero_works()
        {
            IntegerX i;

            i = IntegerX.Create(0);
            Assert.IsTrue(i.IsZero());

            i = IntegerX.Create(-100);
            Assert.IsTrue(i.IsZero() == false);

            i = IntegerX.Create(100);
            Assert.IsTrue(i.IsZero() == false);
        }
        #endregion

        #region BASIC FACTORY TESTS

        [TestCase]
        public void Create_ulong_various()
        {
            IntegerX i;
            uint[] temp;

            i = IntegerX.Create((ulong)0);
            temp = new uint[0];
            Assert.IsTrue(SameValue(i, 0, temp));

            i = IntegerX.Create((ulong)100);
            Assert.IsTrue(SameValue(i, 1, new uint[] { 100 }));

            i = IntegerX.Create((ulong)0x00FFEEDDCCBBAA99);
            Assert.IsTrue(SameValue(i, 1, new uint[] { 0x00FFEEDD, 0xCCBBAA99 }));
        }

        [TestCase]
        public void Create_uint_various()
        {
            IntegerX i;
            uint[] temp;

            i = IntegerX.Create((uint)0);
            temp = new uint[0];
            Assert.IsTrue(SameValue(i, 0, temp));

            i = IntegerX.Create((uint)100);
            Assert.IsTrue(SameValue(i, 1, new uint[] { 100 }));

            i = IntegerX.Create((ulong)0xFFEEDDCC);
            Assert.IsTrue(SameValue(i, 1, new uint[] { 0xFFEEDDCC }));
        }

        [TestCase]
        public void Create_long_various()
        {
            IntegerX i;
            uint[] temp;

            i = IntegerX.Create((long)0);
            temp = new uint[0];
            Assert.IsTrue(SameValue(i, 0, temp));

            i = IntegerX.Create(long.MinValue);
            Assert.IsTrue(SameValue(i, -1, new uint[] { 0x80000000, 0 }));

            i = IntegerX.Create((long)100);
            Assert.IsTrue(SameValue(i, 1, new uint[] { 100 }));

            i = IntegerX.Create((long)-100);
            Assert.IsTrue(SameValue(i, -1, new uint[] { 100 }));

            i = IntegerX.Create(0x00FFEEDDCCBBAA99);
            Assert.IsTrue(SameValue(i, 1, new uint[] { 0x00FFEEDD, 0xCCBBAA99 }));

            unchecked { i = IntegerX.Create((long)0xFFFFEEDDCCBBAA99); }
            Assert.IsTrue(SameValue(i, -1, new uint[] { 0x00001122, 0x33445567 }));
        }

        [TestCase]
        public void Create_integer_various()
        {
            IntegerX i;
            uint[] temp;

            i = IntegerX.Create(0);
            temp = new uint[0];

            Assert.IsTrue(SameValue(i, 0, temp));

            i = IntegerX.Create(int.MinValue);
            Assert.IsTrue(SameValue(i, -1, new uint[] { 0x80000000 }));

            i = IntegerX.Create(100);
            Assert.IsTrue(SameValue(i, 1, new uint[] { 100 }));

            i = IntegerX.Create(-100);
            Assert.IsTrue(SameValue(i, -1, new uint[] { 100 }));

            i = IntegerX.Create(0x00FFEEDD);
            Assert.IsTrue(SameValue(i, 1, new uint[] { 0x00FFEEDD }));

            unchecked { i = IntegerX.Create((int)0xFFFFEEDD); }
            Assert.IsTrue(SameValue(i, -1, new uint[] { 0x00001123 }));
        }

        [TestCase]
        public void Create_double_various()
        {
            IntegerX i;
            uint[] temp;

            i = IntegerX.Create(0.0);
            temp = new uint[0];

            Assert.IsTrue(SameValue(i, 0, temp));

            i = IntegerX.Create(1.0);
            Assert.IsTrue(SameValue(i, 1, new uint[] { 1 }));

            i = IntegerX.Create(-1.0);
            Assert.IsTrue(SameValue(i, -1, new uint[] { 1 }));

            i = IntegerX.Create(10.0);
            Assert.IsTrue(SameValue(i, 1, new uint[] { 10 }));

            i = IntegerX.Create(12345678.123);
            Assert.IsTrue(SameValue(i, 1, new uint[] { 12345678 }));

            i = IntegerX.Create(4.2949672950000000E+009);
            Assert.IsTrue(SameValue(i, 1, new uint[] { 4294967295 }));

            i = IntegerX.Create(4.2949672960000000E+009);
            Assert.IsTrue(SameValue(i, 1, new uint[] { 0x1, 0x0 }));

            i = IntegerX.Create(-1.2345678901234569E+300);
            Assert.IsTrue(SameValue(i, -1, new uint[] { 0x1D, 0x7EE8BCBB, 0xD3520000, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 }));
        }

        [TestCase]
        public void Create_double_powers_of_two()
        {
            int i;
            IntegerX b;

            i = 0;
            while (i < Math.Log(double.MaxValue, 2))
            {
                b = IntegerX.Create(Math.Pow(2.0, i));
                Assert.IsTrue(b == IntegerX.One << i);
                i++;
            }
        }

        [TestCase]
        public void Create_double_fails_on_pos_infinity()
        {
            Assert.Throws<OverflowException>(delegate { IntegerX.Create(double.PositiveInfinity); });
        }

        [TestCase]
        public void Create_double_fails_on_neg_infinity()
        {
            Assert.Throws<OverflowException>(delegate { IntegerX.Create(double.NegativeInfinity); });
        }

        [TestCase]
        public void Create_double_fails_on_NaN()
        {
            Assert.Throws<OverflowException>(delegate { IntegerX.Create(double.NaN); });
        }
        #endregion

        #region CONSTRUCTOR TESTS

        [TestCase]
        public void I_basic_constructor_handles_zero()
        {
            IntegerX i;
            uint[] temp;

            i = IntegerX.Create(0);
            temp = new uint[0];
            Assert.IsTrue(SameValue(i, 0, temp));
        }

        [TestCase]
        public void I_basic_constructor_handles_basic_data_positive()
        {
            uint[] data;
            IntegerX i;

            data = new uint[] { 0xFFEEDDCC, 0xBBAA9988, 0x77665544 };
            i = new IntegerX(1, data);
            Assert.IsTrue(i.IsPositive());
            Assert.IsTrue(SameValue(i, 1, data));
        }

        [TestCase]
        public void I_basic_constructor_handles_basic_data_negative()
        {
            uint[] data;
            IntegerX i;

            data = new uint[] { 0xFFEEDDCC, 0xBBAA9988, 0x77665544 };
            i = new IntegerX(-1, data);
            Assert.IsTrue(i.IsNegative());
            Assert.IsTrue(SameValue(i, -1, data));
        }

        [TestCase]
        public void I_basic_constructor_fails_on_bad_sign_neg()
        {
            Assert.Throws<ArgumentException>(delegate { new IntegerX(-2, new uint[] { 1 }); });
        }

        [TestCase]
        public void I_basic_constructor_fails_on_bad_sign_pos()
        {
            Assert.Throws<ArgumentException>(delegate { new IntegerX(2, new uint[] { 1 }); });
        }

        [TestCase]
        public void I_basic_constructor_fails_on_zero_sign_on_nonzero_mag()
        {
            Assert.Throws<ArgumentException>(delegate { new IntegerX(0, new uint[] { 1 }); });
        }

        [TestCase]
        public void I_basic_constructor_normalized_magnitude()
        {
            uint[] data, normData;
            IntegerX i;

            data = new uint[] { 0, 0, 1, 0 };
            normData = new uint[] { 1, 0 };
            i = new IntegerX(1, data);
            Assert.IsTrue(SameValue(i, 1, normData));
        }

        [TestCase]
        public void I_basic_constructor_detects_all_zero_mag()
        {
            uint[] data, temp;
            IntegerX i;

            data = new uint[] { 0, 0, 0, 0, 0 };
            i = new IntegerX(1, data);
            temp = new uint[0];
            Assert.IsTrue(SameValue(i, 0, temp));
        }

        [TestCase]
        public void I_copy_constructor_works()
        {
            IntegerX i, c;

            i = new IntegerX(1, new uint[] { 1, 2, 3 });
            c = new IntegerX(i);
            Assert.IsTrue(SameValue(c, i.Signum(), i.GetMagnitude()));
        }

        #endregion

        #region PARSING TESTS

        [TestCase]
        public void Parse_detects_radix_too_small()
        {
            IntegerX i;
            var result = IntegerX.TryParse("0", 1, out i);
            Assert.IsTrue(result == false);
        }

        [TestCase]
        public void Parse_detects_radix_too_large()
        {
            IntegerX i;
            var result = IntegerX.TryParse("0", 37, out i);
            Assert.IsTrue(result == false);
        }

        [TestCase]
        public void Parse_zero()
        {
            IntegerX i;
            bool result;
            uint[] temp;

            result = IntegerX.TryParse("0", 10, out i);
            Assert.IsTrue(result);
            temp = new uint[0];
            Assert.IsTrue(SameValue(i, 0, temp));
        }

        [TestCase]
        public void Parse_negative_zero_just_zero()
        {
            IntegerX i;
            bool result;
            uint[] temp;

            result = IntegerX.TryParse("-0", 10, out i);
            Assert.IsTrue(result);
            temp = new uint[0];
            Assert.IsTrue(SameValue(i, 0, temp));
        }

        [TestCase]
        public void Parse_multiple_zeros_is_zero()
        {
            IntegerX i;
            bool result;
            uint[] temp;

            result = IntegerX.TryParse("00000", 10, out i);
            Assert.IsTrue(result);
            temp = new uint[0];
            Assert.IsTrue(SameValue(i, 0, temp));
        }

        [TestCase]
        public void Parse_multiple_zeros_with_leading_minus_is_zero()
        {
            IntegerX i;
            bool result;
            uint[] temp;

            result = IntegerX.TryParse("-00000", 10, out i);
            Assert.IsTrue(result);
            temp = new uint[0];
            Assert.IsTrue(SameValue(i, 0, temp));
        }

        [TestCase]
        public void Parse_multiple_hyphens_fails()
        {
            IntegerX i;
            bool result;

            result = IntegerX.TryParse("-123-4", 10, out i);
            Assert.IsTrue(result == false);
        }

        [TestCase]
        public void Parse_adjacent_hyphens_fails()
        {
            IntegerX i;
            bool result;

            result = IntegerX.TryParse("--1234", 10, out i);
            Assert.IsTrue(result == false);
        }

        [TestCase]
        public void Parse_just_adjacent_hyphens_fails()
        {
            IntegerX i;
            bool result;

            result = IntegerX.TryParse("--", 10, out i);
            Assert.IsTrue(result == false);
        }

        [TestCase]
        public void Parse_hyphen_only_fails()
        {
            IntegerX i;
            bool result;

            result = IntegerX.TryParse("-", 10, out i);
            Assert.IsTrue(result == false);
        }

        [TestCase]
        public void Parse_fails_on_bogus_char()
        {
            IntegerX i;
            bool result;

            result = IntegerX.TryParse("123.56", 10, out i);
            Assert.IsTrue(result == false);
        }

        [TestCase]
        public void Parse_fails_on_digit_out_of_range_base_2()
        {
            IntegerX i;
            bool result;

            result = IntegerX.TryParse("01010120101", 2, out i);
            Assert.IsTrue(result == false);
        }

        [TestCase]
        public void Parse_fails_on_digit_out_of_range_base_8()
        {
            IntegerX i;
            bool result;

            result = IntegerX.TryParse("01234567875", 8, out i);
            Assert.IsTrue(result == false);
        }

        [TestCase]
        public void Parse_fails_on_digit_out_of_range_base_16()
        {
            IntegerX i;
            bool result;

            result = IntegerX.TryParse("CabBaGe", 16, out i);
            Assert.IsTrue(result == false);
        }

        [TestCase]
        public void Parse_fails_on_digit_out_of_range_in_later_super_digit_base_16()
        {
            IntegerX i;
            bool result;

            result = IntegerX.TryParse("AAAAAAAAAAAAAAAAAAAAAAACabBaGe", 16, out i);
            Assert.IsTrue(result == false);
        }

        [TestCase]
        public void Parse_simple_base_2()
        {
            IntegerX i;
            bool result;

            result = IntegerX.TryParse("100", 2, out i);
            Assert.IsTrue(result);
            Assert.IsTrue(SameValue(i, 1, new uint[] { 4 }));
        }

        [TestCase]
        public void Parse_simple_base_10()
        {
            IntegerX i;
            bool result;

            result = IntegerX.TryParse("100", 10, out i);
            Assert.IsTrue(result);
            Assert.IsTrue(SameValue(i, 1, new uint[] { 100 }));
        }

        [TestCase]
        public void Parse_simple_base_16()
        {
            IntegerX i;
            bool result;

            result = IntegerX.TryParse("100", 16, out i);
            Assert.IsTrue(result);
            Assert.IsTrue(SameValue(i, 1, new uint[] { 0x100 }));
        }

        [TestCase]
        public void Parse_simple_base_36()
        {
            IntegerX i;
            bool result;

            result = IntegerX.TryParse("100", 36, out i);
            Assert.IsTrue(result);
            Assert.IsTrue(SameValue(i, 1, new uint[] { 36 * 36 }));
        }

        [TestCase]
        public void Parse_works_on_long_string_base_16()
        {
            IntegerX i;
            bool result;

            result = IntegerX.TryParse("100000000000000000000", 16, out i);
            Assert.IsTrue(result);
            Assert.IsTrue(SameValue(i, 1, new uint[] { 0x00010000, 0, 0 }));
        }

        [TestCase]
        public void Parse_works_on_long_string_base_10()
        {
            IntegerX i;
            bool result;

            result = IntegerX.TryParse("123456789012345678901234567890", 10, out i);
            Assert.IsTrue(result);
            Assert.IsTrue(SameValue(i, 1, new uint[] { 0x1, 0x8EE90FF6, 0xC373E0EE, 0x4E3F0AD2 }));
        }

        [TestCase]
        public void Parse_works_with_leading_minus_sign()
        {
            IntegerX i;
            bool result;

            result = IntegerX.TryParse("-123456789012345678901234567890", 10, out i);
            Assert.IsTrue(result);
            Assert.IsTrue(SameValue(i, -1, new uint[] { 0x1, 0x8EE90FF6, 0xC373E0EE, 0x4E3F0AD2 }));
        }

        [TestCase]
        public void Parse_works_with_leading_plus_sign()
        {
            IntegerX i;
            bool result;

            result = IntegerX.TryParse("+123456789012345678901234567890", 10, out i);
            Assert.IsTrue(result);
            Assert.IsTrue(SameValue(i, 1, new uint[] { 0x1, 0x8EE90FF6, 0xC373E0EE, 0x4E3F0AD2 }));
        }

        [TestCase]
        public void Parse_works_on_long_string_base_10_2()
        {
            IntegerX i;
            bool result;

            result = IntegerX.TryParse("1024000001024000001024", 10, out i);
            Assert.IsTrue(result);
            Assert.IsTrue(SameValue(i, 1, new uint[] { 0x37, 0x82DACF8B, 0xFB280400 }));
        }
        #endregion

        #region ToString() TESTS

        [TestCase]
        public void ToString_fails_on_radix_too_small()
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate { var i = new IntegerX(0, new uint[0]); i.toString(1); });
        }

        [TestCase]
        public void ToString_detects_radix_too_large()
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate { var i = new IntegerX(0, new uint[0]); i.toString(37); });
        }

        [TestCase]
        public void ToString_on_zero_works_for_all_radixes()
        {
            const int MinRadix = 2;
            const int MaxRadix = 36;

            IntegerX i;
            uint radix;
            uint[] data;

            data = new uint[0];
            i = new IntegerX(0, data);
            radix = MinRadix;
            while (radix <= MaxRadix)
            {
                Assert.IsTrue(i.toString(radix) == "0");
                radix++;
            }
        }

        [TestCase]
        public void ToString_simple_base_2()
        {
            IntegerX i;
            string result;

            i = new IntegerX(1, new uint[] { 4 });
            result = i.toString(2);
            Assert.IsTrue(result == "100");
        }

        [TestCase]
        public void ToString_simple_base_10()
        {
            IntegerX i;
            string result;

            i = new IntegerX(1, new uint[] { 927 });
            result = i.toString(10);
            Assert.IsTrue(result == "927");
        }

        [TestCase]
        public void ToString_simple_base_16()
        {
            IntegerX i;
            string result;

            i = new IntegerX(1, new uint[] { 0xA20F5 });
            result = i.toString(16);
            Assert.IsTrue(result == "A20F5");
        }

        [TestCase]
        public void ToString_simple_base_26()
        {
            IntegerX i;
            string result;

            i = new IntegerX(1, new uint[] { 23 * 26 * 26 + 12 * 26 + 15 });
            result = i.toString(26);
            Assert.IsTrue(result == "NCF");
        }


        [TestCase]
        public void ToString_long_base_16()
        {
            IntegerX i;
            string result;

            i = new IntegerX(-1, new uint[] { 0x00FEDCBA, 0x12345678, 0x87654321 });
            result = i.toString(16);
            Assert.IsTrue(result == "-FEDCBA1234567887654321");
        }

        [TestCase]
        public void ToString_long_base_10()
        {
            IntegerX i;
            string result;

            i = new IntegerX(1, new uint[] { 0x1, 0x8EE90FF6, 0xC373E0EE, 0x4E3F0AD2 });
            result = i.toString(10);
            Assert.IsTrue(result == "123456789012345678901234567890");
        }

        [TestCase]
        public void ToString_long_base_10_2()
        {
            IntegerX i;
            string result;

            i = new IntegerX(1, new uint[] { 0x37, 0x82DACF8B, 0xFB280400 });
            result = i.toString(10);
            Assert.IsTrue(result == "1024000001024000001024");
        }

        #endregion

        #region COMPARISON TESTS

        [TestCase]
        public void Compare_on_zeros_is_0()
        {
            IntegerX x, y;
            uint[] temp;

            temp = new uint[0];
            x = new IntegerX(0, temp);
            y = new IntegerX(0, temp);
            Assert.IsTrue(IntegerX.Compare(x, y) == 0);
        }

        [TestCase]
        public void Compare_neg_pos_is_minus1()
        {
            IntegerX x, y;

            x = new IntegerX(-1, new uint[] { 0xFFFFFFFF, 0xFFFFFFFF });
            y = new IntegerX(1, new uint[] { 0x1 });
            Assert.IsTrue(IntegerX.Compare(x, y) == -1);
        }

        [TestCase]
        public void Compare_pos_neg_is_plus1()
        {
            IntegerX x, y;

            x = new IntegerX(1, new uint[] { 0x1 });
            y = new IntegerX(-1, new uint[] { 0xFFFFFFFF, 0xFFFFFFFF });
            Assert.IsTrue(IntegerX.Compare(x, y) == 1);
        }

        [TestCase]
        public void Compare_negs_smaller_len_first_is_plus1()
        {
            IntegerX x, y;

            x = new IntegerX(-1, new uint[] { 0xFFFFFFFF });
            y = new IntegerX(-1, new uint[] { 0xFFFFFFFF, 0xFFFFFFFF });
            Assert.IsTrue(IntegerX.Compare(x, y) == 1);
        }

        [TestCase]
        public void Compare_negs_larger_len_first_is_minus1()
        {
            IntegerX x, y;

            x = new IntegerX(-1, new uint[] { 0xFFFFFFFF, 0xFFFFFFFF });
            y = new IntegerX(-1, new uint[] { 0xFFFFFFFF });
            Assert.IsTrue(IntegerX.Compare(x, y) == -1);
        }

        [TestCase]
        public void Compare_pos_smaller_len_first_is_minus1()
        {
            IntegerX x, y;

            x = new IntegerX(1, new uint[] { 0xFFFFFFFF });
            y = new IntegerX(1, new uint[] { 0xFFFFFFFF, 0xFFFFFFFF });
            Assert.IsTrue(IntegerX.Compare(x, y) == -1);
        }

        [TestCase]
        public void Compare_pos_larger_len_first_is_plus1()
        {
            IntegerX x, y;

            x = new IntegerX(1, new uint[] { 0xFFFFFFFF, 0xFFFFFFFF });
            y = new IntegerX(1, new uint[] { 0xFFFFFFFF });
            Assert.IsTrue(IntegerX.Compare(x, y) == 1);
        }

        [TestCase]
        public void Compare_same_len_smaller_first_diff_in_MSB_is_minus1()
        {
            IntegerX x, y;

            x = new IntegerX(1, new uint[] { 0xFFFFFFFE, 0x12345678, 0xFFFFFFFF });
            y = new IntegerX(1, new uint[] { 0xFFFFFFFF, 0x12345678, 0xFFFFFFFF });
            Assert.IsTrue(IntegerX.Compare(x, y) == -1);
        }

        [TestCase]
        public void Compare_same_len_smaller_first_diff_in_middle_is_minus1()
        {
            IntegerX x, y;

            x = new IntegerX(1, new uint[] { 0xFFFFFFFF, 0x12345678, 0xFFFFFFFE });
            y = new IntegerX(1, new uint[] { 0xFFFFFFFF, 0x12345678, 0xFFFFFFFF });
            Assert.IsTrue(IntegerX.Compare(x, y) == -1);
        }

        [TestCase]
        public void Compare_same_len_larger_first_diff_in_MSB_is_plus1()
        {
            IntegerX x, y;

            x = new IntegerX(1, new uint[] { 0xFFFFFFFE, 0x12345678, 0xFFFFFFFF });
            y = new IntegerX(1, new uint[] { 0xFFFFFFFF, 0x12345678, 0xFFFFFFFF });
            Assert.IsTrue(IntegerX.Compare(x, y) == -1);
        }

        [TestCase]
        public void Compare_same_len_larger_first_diff_in_LSB_is_plus1()
        {
            IntegerX x, y;

            x = new IntegerX(1, new uint[] { 0xFFFFFFFF, 0x12345678, 0xFFFFFFFE });
            y = new IntegerX(1, new uint[] { 0xFFFFFFFF, 0x12345678, 0xFFFFFFFF });
            Assert.IsTrue(IntegerX.Compare(x, y) == -1);
        }

        [TestCase]
        public void Compare_same_len_larger_first_diff_in_middle_is_plus1()
        {
            IntegerX x, y;

            x = new IntegerX(1, new uint[] { 0xFFFFFFFF, 0x12335678, 0xFFFFFFFF });
            y = new IntegerX(1, new uint[] { 0xFFFFFFFF, 0x12345678, 0xFFFFFFFF });
            Assert.IsTrue(IntegerX.Compare(x, y) == -1);
        }

        [TestCase]
        public void Compare_same_is_0()
        {
            IntegerX x, y;

            x = new IntegerX(1, new uint[] { 0xFFFFFFFF, 0x12345678, 0xFFFFFFFF });
            y = new IntegerX(1, new uint[] { 0xFFFFFFFF, 0x12345678, 0xFFFFFFFF });
            Assert.IsTrue(IntegerX.Compare(x, y) == 0);
        }
        #endregion

        #region ADD/SUBTRACT/NEGATE/ABS TESTS

        [TestCase]
        public void Negate_zero_is_zero()
        {
            IntegerX xn, x;
            uint[] temp = new uint[0];

            x = new IntegerX(0, temp);
            xn = x.Negate();
            Assert.IsTrue(SameValue(xn, 0, temp));
        }

        [TestCase]
        public void Negate_positive_is_same_mag_neg()
        {
            IntegerX xn, x;

            x = new IntegerX(1, new uint[] { 0xFEDCBA98, 0x87654321 });
            xn = x.Negate();
            Assert.IsTrue(SameValue(xn, -1, x.GetMagnitude()));
        }


        [TestCase]
        public void Negate_negative_is_same_mag_pos()
        {
            IntegerX xn, x;

            x = new IntegerX(-1, new uint[] { 0xFEDCBA98, 0x87654321 });
            xn = x.Negate();
            Assert.IsTrue(SameValue(xn, 1, x.GetMagnitude()));
        }

        [TestCase]
        public void Add_pos_same_length_no_carry()
        {
            IntegerX x, y, z;
            x = new IntegerX(1, new uint[] { 0x12345678, 0x12345678 });
            y = new IntegerX(1, new uint[] { 0x23456789, 0x13243546 });
            z = x.Add(y);

            Assert.IsTrue(SameValue(z, 1, new uint[] { 0x3579BE01, 0x25588BBE }));
        }

        [TestCase]
        public void Add_neg_same_length_no_carry()
        {
            IntegerX x, y, z;
            x = new IntegerX(-1, new uint[] { 0x12345678, 0x12345678 });
            y = new IntegerX(-1, new uint[] { 0x23456789, 0x13243546 });
            z = x.Add(y);

            Assert.IsTrue(SameValue(z, -1, new uint[] { 0x3579BE01, 0x25588BBE }));
        }

        [TestCase]
        public void Add_pos_same_length_some_carry()
        {
            IntegerX x, y, z;

            x = new IntegerX(1, new uint[] {0x12345678, 0x12345678, 0xFFFFFFFF});
            y = new IntegerX(1, new uint[] { 0x23456789, 0x13243546, 0x11111111});
            z = x.Add(y);

            Assert.IsTrue(SameValue(z, 1, new uint[] { 0x3579BE01, 0x25588BBF, 0x11111110}));
        }

        [TestCase]
        public void Add_neg_same_length_some_carry()
        {
            IntegerX x, y, z;

            x = new IntegerX(-1, new uint[] { 0x12345678, 0x12345678, 0xFFFFFFFF });
            y = new IntegerX(-1, new uint[] { 0x23456789, 0x13243546, 0x11111111 });
            z = x.Add(y);

            Assert.IsTrue(SameValue(z, -1, new uint[] { 0x3579BE01, 0x25588BBF, 0x11111110 }));
        }

        [TestCase]
        public void Add_pos_first_longer_one_carry()
        {
            IntegerX x, y, z;

            x = new IntegerX(1, new uint[] { 0x12345678, 0x12345678, 0xFFFFFFFF, 0x22222222 });
            y = new IntegerX(1, new uint[] { 0x11111111, 0x11111111 });
            z = x.Add(y);

            Assert.IsTrue(SameValue(z, 1, new uint[] { 0x12345678, 0x12345679, 0x11111110, 0x33333333 }));
        }

        [TestCase]
        public void Add_pos_first_longer_more_carry()
        {
            IntegerX x, y, z;

            x = new IntegerX(1, new uint[] { 0x12345678, 0x12345678, 0xFFFFFFFF, 0xFFFFFFFF, 0x22222222 });
            y = new IntegerX(1, new uint[] { 0x11111111, 0x11111111 });
            z = x.Add(y);

            Assert.IsTrue(SameValue(z, 1, new uint[] { 0x12345678, 0x12345679, 0x00000000, 0x11111110, 0x33333333 }));
        }

        [TestCase]
        public void Add_pos_first_longer_carry_extend()
        {
            IntegerX x, y, z;

            x = new IntegerX(1, new uint[] { 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0x22222222 });
            y = new IntegerX(1, new uint[] { 0x11111111, 0x11111111 });
            z = x.Add(y);

            Assert.IsTrue(SameValue(z, 1, new uint[] { 0x00000001, 0x00000000, 0x00000000, 0x11111110, 0x33333333 }));
        }

        [TestCase]
        public void Add_pos_neg_first_larger_mag()
        {
            IntegerX x, y, z;

            x = new IntegerX(1, new uint[] { 0x5 });
            y = new IntegerX(-1, new uint[] { 0x3 });
            z = x.Add(y);

            Assert.IsTrue(SameValue(z, 1, new uint[] { 0x2 }));
        }

        [TestCase]
        public void Add_pos_neg_second_larger_mag()
        {
            IntegerX x, y, z;

            x = new IntegerX(1, new uint[] { 0x3 });
            y = new IntegerX(-1, new uint[] { 0x5 });
            z = x.Add(y);

            Assert.IsTrue(SameValue(z, -1, new uint[] { 0x2 }));
        }

        [TestCase]
        public void Add_pos_neg_same_mag()
        {
            IntegerX x, y, z;

            x = new IntegerX(1, new uint[] { 0x3 });
            y = new IntegerX(-1, new uint[] { 0x3 });
            z = x.Add(y);

            Assert.IsTrue(z.IsZero());
        }

        [TestCase]
        public void Add_neg_pos_first_larger_mag()
        {
            IntegerX x, y, z;

            x = new IntegerX(-1, new uint[] { 0x5 });
            y = new IntegerX(1, new uint[] { 0x3 });
            z = x.Add(y);

            Assert.IsTrue(SameValue(z, -1, new uint[] { 0x2 }));
        }

        [TestCase]
        public void Add_neg_pos_second_larger_mag()
        {
            IntegerX x, y, z;

            x = new IntegerX(-1, new uint[] { 0x3 });
            y = new IntegerX(1, new uint[] { 0x5 });
            z = x.Add(y);

            Assert.IsTrue(SameValue(z, 1, new uint[] { 0x2 }));
        }

        [TestCase]
        public void Add_neg_pos_same_mag()
        {
            IntegerX x, y, z;

            x = new IntegerX(-1, new uint[] { 0x3 });
            y = new IntegerX(1, new uint[] { 0x3 });
            z = x.Add(y);

            Assert.IsTrue(z.IsZero());
        }

        [TestCase]
        public void Add_zero_to_pos()
        {
            IntegerX x, y, z;
            var temp = new uint[0];

            x = new IntegerX(0, temp);
            y = new IntegerX(1, new uint[] { 0x3 });
            z = x.Add(y);

            Assert.IsTrue(z == y);
        }

        [TestCase]
        public void Subtract_zero_yields_this()
        {
            IntegerX x, y, z;

            x = new IntegerX(1, new uint[] { 0x12345678, 0x12345678, 0xFFFFFFFF, 0xFFFFFFFF, 0x22222222 });
            y = new IntegerX(1, new uint[0]);
            z = x.Subtract(y);

            Assert.IsTrue(z == x);
        }

        [TestCase]
        public void Subtract_from_zero_yields_negation()
        {
            IntegerX x, y, z;
            var temp = new uint[0];

            x = new IntegerX(1, temp );
            y = new IntegerX(1, new uint[] { 0x12345678, 0x12345678, 0xFFFFFFFF, 0xFFFFFFFF, 0x22222222 });
            z = x.Subtract(y);

            Assert.IsTrue(SameValue(z, -1, y.GetMagnitude()));
        }

        [TestCase]
        public void Subtract_opposite_sign_first_pos_is_add()
        {
            IntegerX x, y, z;

            x =  new IntegerX(1, new uint[] { 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0x22222222 });
            y = new IntegerX(-1, new uint[] { 0x11111111, 0x11111111 });
            z = x.Subtract(y);
            Assert.IsTrue(SameValue(z, 1, new uint[] { 0x00000001, 0x00000000, 0x00000000, 0x11111110, 0x33333333 }));
        }

        [TestCase]
        public void Subtract_opposite_sign_first_neg_is_add()
        {
            IntegerX x, y, z;

            x = new IntegerX(-1, new uint[] { 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0x22222222 });
            y = new IntegerX(1, new uint[] { 0x11111111, 0x11111111 });
            z = x.Subtract(y);
            Assert.IsTrue(SameValue(z, -1, new uint[] {0x00000001, 0x00000000,
              0x00000000, 0x11111110, 0x33333333 }));
        }

        [TestCase]
        public void Subtract_equal_pos_is_zero()
        {
            IntegerX x, y, z;

            x = new IntegerX(1, new uint[] { 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0x22222222 });
            y = new IntegerX(1, new uint[] { 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0x22222222 });
            z = x.Subtract(y);
            Assert.IsTrue(z.IsZero());
        }

        [TestCase]
        public void Subtract_equal_neg_is_zero()
        {
            IntegerX x, y, z;

            x = new IntegerX(-1, new uint[] {0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0x22222222 });
            y = new IntegerX(-1, new uint[] {0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0x22222222 });
            z = x.Subtract(y);

            Assert.IsTrue(z.IsZero());
        }

        [TestCase]
        public void Subtract_both_pos_first_larger_no_borrow()
        {
            IntegerX x, y, z;

            x = new IntegerX(1, new uint[] { 0x12345678, 0x12345678, 0xFFFFFFFF, 0x33333333 });
            y = new IntegerX(1, new uint[] { 0x11111111, 0x11111111 });
            z = x.Subtract(y);
            Assert.IsTrue(SameValue(z, 1, new uint[] {0x12345678, 0x12345678, 0xEEEEEEEE, 0x22222222 })); 
        }

        [TestCase]
        public void Subtract_both_pos_first_smaller_no_borrow()
        {
            IntegerX x, y, z;

            x = new IntegerX(1, new uint[] { 0x11111111, 0x11111111 });
            y = new IntegerX(1, new uint[] { 0x12345678, 0x12345678, 0xFFFFFFFF, 0x33333333 });
            z = x.Subtract(y);
            Assert.IsTrue(SameValue(z, -1, new uint[] { 0x12345678, 0x12345678, 0xEEEEEEEE, 0x22222222 }));
        }

        [TestCase]
        public void Subtract_both_neg_first_larger_no_borrow()
        {
            IntegerX x, y, z;

            x = new IntegerX(1, new uint[] {0x12345678, 0x12345678, 0xFFFFFFFF, 0x33333333});
            y = new IntegerX(1, new uint[] { 0x11111111, 0x11111111 });
            z = x.Subtract(y);
            Assert.IsTrue(SameValue(z, 1, new uint[] {0x12345678, 0x12345678, 0xEEEEEEEE, 0x22222222}));
        }

        [TestCase]
        public void Subtract_both_neg_first_smaller_no_borrow()
        {
            IntegerX x, y, z;

            x = new IntegerX(1, new uint[] {0x11111111, 0x11111111 });
            y = new IntegerX(1, new uint[] {0x12345678, 0x12345678, 0xFFFFFFFF, 0x33333333 });
            z = x.Subtract(y);
            Assert.IsTrue(SameValue(z, -1, new uint[] { 0x12345678, 0x12345678, 0xEEEEEEEE, 0x22222222 }));
        }

        [TestCase]
        public void Subtract_both_pos_first_larger_some_borrow()
        {
            IntegerX x, y, z;

            x = new IntegerX(1, new uint[] { 0x12345678, 0x12345678, 0x00000000, 0x03333333 });
            y = new IntegerX(1, new uint[] { 0x11111111, 0x11111111 });
            z = x.Subtract(y);
            Assert.IsTrue(SameValue(z, 1, new uint[] { 0x12345678, 0x12345677, 0xEEEEEEEE, 0xF2222222 }));
        }

        [TestCase]
        public void Subtract_both_pos_first_larger_lose_MSB()
        {
            IntegerX x, y, z;

            x = new IntegerX(1, new uint[] { 0x12345678, 0x12345678, 0x00000000, 0x33333333 });
            y = new IntegerX(1, new uint[] { 0x12345678, 0x12345676, 0x00000000, 0x44444444 });
            z = x.Subtract(y);
            Assert.IsTrue(SameValue(z, 1, new uint[] { 0x1, 0xFFFFFFFF, 0xEEEEEEEF }));
        }

        [TestCase]
        public void Subtract_both_pos_first_larger_lose_several_MSB()
        {
            IntegerX x, y, z;

            x = new IntegerX(1, new uint[] { 0x12345678, 0x12345678, 0x12345678, 0x00000000, 0x33333333 });
            y = new IntegerX(1, new uint[] { 0x12345678, 0x12345678, 0x12345676, 0x00000000, 0x44444444 });
            z = x.Subtract(y);
            Assert.IsTrue(SameValue(z, 1, new uint[] { 0x1, 0xFFFFFFFF, 0xEEEEEEEF }));
        }

        [TestCase]
        public void Abs_zero_is_zero()
        {
            var z = IntegerX.Create(0);
            Assert.IsTrue(z.Abs().IsZero());
        }

        [TestCase]
        public void Abs_pos_is_pos()
        {
            var data = new uint[] { 0x1, 0x2, 0x3 };
            var i = new IntegerX(1, data);
            Assert.IsTrue(SameValue(i.Abs(), 1, data));
        }

        [TestCase]
        public void Abs_neg_is_pos()
        {
            var data = new uint[] { 0x1, 0x2, 0x3 };
            var i = new IntegerX(-1, data);
            Assert.IsTrue(SameValue(i.Abs(), 1, data));
        }
        #endregion

        #region MULTIPLICATION
        [TestCase]
        public void Mult_x_by_zero_is_zero()
        {
            IntegerX x, y, z;
            var data = new uint[0];

            x = new IntegerX(1, new uint[] { 0x12345678 });
            y = new IntegerX(0, data);
            z = x.Multiply(y);
            Assert.IsTrue(z.IsZero());
        }

        [TestCase]
        public void Mult_zero_by_y_is_zero()
        {
            IntegerX x, y, z;
            var data = new uint[0];

            x = new IntegerX(0, data);
            y = new IntegerX(1, new uint[] { 0x12345678 });
            z = x.Multiply(y);
            Assert.IsTrue(z.IsZero());
        }

        [TestCase]
        public void Mult_two_pos_is_pos()
        {
            IntegerX x, y, z;

            x = new IntegerX(1, new uint[] { 0xDEFCBA98 });
            y = new IntegerX(1, new uint[] { 0x12345678 });
            z = x.Multiply(y);
            Assert.IsTrue(z.IsPositive());
        }

        [TestCase]
        public void Mult_two_neg_is_pos()
        {
            IntegerX x, y, z;

            x = new IntegerX(-1, new uint[] { 0xDEFCBA98 });
            y = new IntegerX(-1, new uint[] { 0x12345678 });
            z = x.Multiply(y);
            Assert.IsTrue(z.IsPositive());
        }

        [TestCase]
        public void Mult_pos_neg_is_neg()
        {
            IntegerX x, y, z;

            x = new IntegerX(1, new uint[] { 0xDEFCBA98 });
            y = new IntegerX(-1, new uint[] { 0x12345678 });
            z = x.Multiply(y);
            Assert.IsTrue(z.IsNegative());
        }

        [TestCase]
        public void Mult_neg_pos_is_neg()
        {
            IntegerX x, y, z;

            x = new IntegerX(-1, new uint[] { 0xDEFCBA98 });
            y = new IntegerX(1, new uint[] { 0x12345678 });
            z = x.Multiply(y);
            Assert.IsTrue(z.IsNegative());
        }

        [TestCase]
        public void Mult_1()
        {
            IntegerX x, y, z;

            x = new IntegerX(1, new uint[] { 100 });
            y = new IntegerX(1, new uint[] { 200 });
            z = x.Multiply(y);
            Assert.IsTrue(SameValue(z, 1, new uint[] { 20000 }));
        }

        [TestCase]
        public void Mult_2()
        {
            IntegerX x, y, z;

            x = new IntegerX(1, new uint[] { 0xFFFFFFFF, 0xF0000000 });
            y = new IntegerX(1, new uint[] { 0x2 });
            z = x.Multiply(y);
            Assert.IsTrue(SameValue(z, 1, new uint[] { 0x1, 0xFFFFFFFF, 0xE0000000 }));
        }

        [TestCase]
        public void Mult_3()
        {
            IntegerX x, y, z;

            x = new IntegerX(1, new uint[] { 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF });
            y = new IntegerX(1, new uint[] { 0x1, 0x1 });
            z = x.Multiply(y);
            Assert.IsTrue(SameValue(z, 1, new uint[] { 0x1, 0x0, 0xFFFFFFFF, 0xFFFFFFFE, 0xFFFFFFFF }));
        }
        #endregion

        #region INTERNAL TESTS
        [TestCase]
        public void Normalize_shifts_happens_different_len()
        {
            uint[] x, xn;
            int i, rshift;

            x = new uint[] { 0x8421FEC8, 0xFE62F731 };
            xn = new uint[3];

            IntegerX.Normalize(ref xn, 3, x, 2, 0);
            Assert.IsTrue(xn[2] == x[1]);
            Assert.IsTrue(xn[1] == x[0]);
            Assert.IsTrue(xn[0] == 0);

            i = 1;
            while (i < 32)
            {
                rshift = 32 - i;
                IntegerX.Normalize(ref xn, 3, x, 2, i);
                Assert.IsTrue(xn[2] == x[1] << i);
                Assert.IsTrue(xn[1] == (x[0] << i | x[1] >> rshift));
                Assert.IsTrue(xn[0] == x[0] >> rshift);
                i++;
            }
        }

        [TestCase]
        public void Normalize_shifts_happens_same_len()
        {
            uint[] x, xn;
            int i, rshift;

            x = new uint[] { 0x0421FEC8, 0xFE62F731 };
            xn = new uint[3];

            IntegerX.Normalize(ref xn, 2, x, 2, 0);
            Assert.IsTrue(xn[1] == x[1]);
            Assert.IsTrue(xn[0] == x[0]);

            i = 1;
            while (i < 5)
            {
                rshift = 32 - i;
                IntegerX.Normalize(ref xn, 2, x, 2, i);
                Assert.IsTrue(xn[1] == x[1] << i);
                Assert.IsTrue(xn[0] == (x[0] << i | x[1] >> rshift));
                i++;
            }
        }

        [TestCase]
        public void Normalize_shifts_over_left_end_throws()
        {
            uint[] x, xn;
            x = new uint[] { 0x0421FEC8, 0xFE62F731 };
            xn = new uint[2];

            Assert.Throws<InvalidOperationException>(delegate { IntegerX.Normalize(ref xn, 2, x, 2, 8); });
        }
        #endregion

        #region DIVISION

        [TestCase]
        public void Divide_by_zero_throws()
        {
            IntegerX x, y;
            uint[] temp;

            x = new IntegerX(1, new uint[] { 0xFFFFFFFF });
            temp = new uint[0];

            y = new IntegerX(0, temp);

            Assert.Throws<DivideByZeroException>(delegate { x.Divide(y); });
        }

        [TestCase]
        public void Divide_into_zero_is_zero()
        {
            IntegerX y, q, r;
            uint[] temp, temp2;

            y = new IntegerX(1, new uint[] { 0x1234, 0xABCD });
            temp = new uint[0];
            q = new IntegerX(0, temp);
            temp2 = new uint[0];
            r = new IntegerX(0, temp2);
            TestDivRem(y, q, r);
        }

        [TestCase]
        public void Divide_into_smaller_is_zero_plus_remainder_is_dividend()
        {
            IntegerX x, y, q, r;

            x = new IntegerX(1, new uint[] { 0x12345678, 0xABCDEF23, 0x88776654 });
            y = new IntegerX(1, new uint[] { 0x12345678, 0xABCDEF23, 0x88776655 });
            q = x.DivRem(y, out r);
            Assert.IsTrue(r == x);
            Assert.IsTrue(q.IsZero());
        }

        [TestCase]
        public void Divide_into_smaller_is_zero_plus_remainder_is_dividend_len_difference()
        {
            IntegerX x, y, q, r;

            x = new IntegerX(1, new uint[] { 0x12345678 });
            y = new IntegerX(1, new uint[] { 0x12345678, 0xABCDEF23, 0x88776655 });
            q = x.DivRem(y, out r);
            Assert.IsTrue(r == x);
            Assert.IsTrue(q.IsZero());
        }

        [TestCase]
        public void Divide_same_on_len_1()
        {
            IntegerX y, q, r;

            y = new IntegerX(1, new uint[] { 0x12345678 });
            q = new IntegerX(1, new uint[] { 0x1 });
            var temp = new uint[0];
            r = new IntegerX(0, temp);

            TestDivRem(y, q, r);
        }

        [TestCase]
        public void Divide_same_on_len_3()
        {
            IntegerX y, q, r;
            uint[] temp;

            y = new IntegerX(1, new uint[] { 0x12345678, 0xABCDEF23, 0x88776655 });
            q = new IntegerX(1, new uint[] { 0x1 });
            temp = new uint[0];
            r = new IntegerX(0, temp);

            TestDivRem(y, q, r);
        }

        [TestCase]
        public void Divide_same_except_small_remainder()
        {
            IntegerX y, q, r;

            y = new IntegerX(1, new uint[] { 0x12345678 });
            q = new IntegerX(1, new uint[] { 0x1 });
            r = new IntegerX(1, new uint[] { 0x45 });

            TestDivRem(y, q, r);
        }

        [TestCase]
        public void Divide_same_except_small_remainder_2()
        {
            IntegerX y, q, r;

            y = new IntegerX(1, new uint[] { 0x12345678, 0xABCDEF23, 0x88776655 });
            q = new IntegerX(1, new uint[] { 0x1 });
            r = new IntegerX(1, new uint[] { 0x45 });

            TestDivRem(y, q, r);
        }

        [TestCase]
        public void Divide_two_digits_with_small_remainder_no_shift()
        {
            IntegerX y, q, r;
            
            y = new IntegerX(1, new uint[] { 0xFF000000 });
            q = new IntegerX(1, new uint[] { 0x1, 0x1, 0x1 });
            r = new IntegerX(1, new uint[] { 0xAB });

            TestDivRem(y, q, r);
        }

        [TestCase]
        public void Divide_two_digits_with_small_remainder_no_shift2()
        {
            IntegerX y, q, r;

            y = new IntegerX(1, new uint[] { 0xFF000000, 0x000000AA });
            q = new IntegerX(1, new uint[] { 0x1, 0x1, 0x1 });
            r = new IntegerX(1, new uint[] { 0xAB, 0x45 });

            TestDivRem(y, q, r);
        }

        private void GenerateKnuthExample(int m, int n, out IntegerX bmn, out IntegerX bm, out IntegerX bn)
        {
            uint[] bmnArray, bmArray, bnArray;
            int i;

            if (m >= n)
                throw new InvalidOperationException("m must be less than n");

            bmnArray = new uint[m + n];
            bmArray = new uint[m];
            bnArray = new uint[n];

            i = 0;
            while (i < m)
            {
                bmArray[i] = 0xFFFFFFFF;
                i++;
            }

            i = 0;
            while (i < n)
            {
                bnArray[i] = 0xFFFFFFFF;
                i++;
            }

            i = 0;
            while (i < m - 1)
            {
                bmnArray[i] = 0xFFFFFFFF;
                i++;
            }

            bmnArray[m - 1] = 0xFFFFFFFE;

            i = 0;
            while (i < n - m)
            {
                bmnArray[m + i] = 0xFFFFFFFF;
                i++;
            }

            i = 0;
            while (i < m - 2)
            {
                bmnArray[n + i] = 0;
                i++;
            }

            bmnArray[m + n - 1] = 1;

            bmn = new IntegerX(1, bmnArray);
            bm = new IntegerX(1, bmArray);
            bn = new IntegerX(1, bnArray);
        }

        [TestCase]
        public void TestKnuthExamples()
        {
            IntegerX bm, bn, bmn, add, q, r, x;
            int m, n;

            m = 2;
            while (m < 5)
            {
                n = m + 1;
                while (n < m + 5)
                {
                    GenerateKnuthExample(m, n, out bmn, out bm, out bn);

                    add = bm - new IntegerX(1, new uint[] { 0xABCD });
                    x = bmn + add;
                    q = x.DivRem(bm, out r);
                    Assert.IsTrue(r == add);
                    Assert.IsTrue(q == bn);
                    n++;
                }

                m++;
            }
        }

        private void TestDivRem(IntegerX y, IntegerX mult, IntegerX add)
        {
            IntegerX x, q, r;

            x = y * mult + add;
            q = x.DivRem(y, out r);

            Assert.IsTrue(q == mult);
            Assert.IsTrue(r == add);
        }

        #endregion

        #region POWER, MODPOWER TESTS
        [TestCase]
        public void Power_on_negative_exponent_fails()
        {
            var i = new IntegerX(1, new uint[] { 0x1 });

            Assert.Throws<ArgumentOutOfRangeException>(delegate { i.Power(-2); });
        }

        [TestCase]
        public void Power_with_exponent_0_is_one()
        {
            var i = new IntegerX(1, new uint[] { 0x1 });
            Assert.IsTrue(SameValue(i.Power(0), 1, new uint[] { 1 }));
        }

        [TestCase]
        public void Power_on_zero_is_zero()
        {
            var z = IntegerX.Create(0);
            Assert.IsTrue(z.Power(12).IsZero());
        }

        [TestCase]
        public void Power_on_small_exponent_works()
        {
            IntegerX i, p, e;

            i = IntegerX.Create(3);
            p = i.Power(6);
            e = IntegerX.Create(729);
            Assert.IsTrue(p == e);
        }

        [TestCase]
        public void Power_on_completely_odd_exponent_works()
        {
            IntegerX i, p, p2_to_255;

            i = new IntegerX(1, new uint[] { 0x2 });
            p = i.Power(255);
            p2_to_255 = new IntegerX(1, new uint[] { 0x80000000, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 });
            Assert.IsTrue(p == p2_to_255);
        }

        [TestCase]
        public void Power_on_power_of_two_exponent_works()
        {
            IntegerX i, p, p2_to_256;

            i = new IntegerX(1, new uint[] { 0x2 });
            p = i.Power(256);
            p2_to_256 = new IntegerX(1, new uint[] { 0x1, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 });
            Assert.IsTrue(p == p2_to_256);
        }

        [TestCase]
        public void ModPow_on_negative_exponent_fails()
        {
            var i = new IntegerX(1, new uint[] { 0x1 });
            var m = new IntegerX(1, new uint[] { 0x1 });

            Assert.Throws<ArgumentOutOfRangeException>(delegate { i.ModPow(-2, m); });
        }

        [TestCase]
        public void ModPow_with_exponent_0_is_one()
        {
            var i = new IntegerX(1, new uint[] { 0x1 });
            var m = new IntegerX(1, new uint[] { 0x1 });
            Assert.IsTrue(SameValue(i.ModPow(0, m), 1, new uint[] { 1 }));
        }

        [TestCase]
        public void ModPow_on_zero_is_zero()
        {
            var z = IntegerX.Create(0);
            var m = new IntegerX(1, new uint[] { 0x1 });
            Assert.IsTrue(z.ModPow(12, m).IsZero());
        }

        [TestCase]
        public void ModPow_on_small_exponent_works()
        {
            IntegerX i, m, e, p, a;

            i = IntegerX.Create(3);
            m = IntegerX.Create(100);
            e = IntegerX.Create(6);
            p = i.ModPow(e, m);
            a = IntegerX.Create(29);
            Assert.IsTrue(p == a);
        }

        [TestCase]
        public void ModPow_on_completely_odd_exponent_works()
        {
            IntegerX i, m, e, p, a;
            i = new IntegerX(1, new uint[] { 0x2 });
            m = new IntegerX(1, new uint[] { 0x7 });
            e = IntegerX.Create(255);
            p = i.ModPow(e, m);
            a = new IntegerX(1, new uint[] { 0x1 });
            Assert.IsTrue(p == a);
        }

        [TestCase]
        public void ModPow_on_power_of_two_exponent_works()
        {
            IntegerX i, m, e, p, a;

            i = new IntegerX(1, new uint[] { 2 });
            m = new IntegerX(1, new uint[] { 7 });
            e = new IntegerX(256);
            p = i.ModPow(e, m);
            a = new IntegerX(1, new uint[] { 2 });
            Assert.IsTrue(p == a);
        }
        #endregion

        #region MISC TESTS
        [TestCase]
        public void IsOddWorks()
        {
            IntegerX x1, x2, x3, x4, x5, x6, x7, x8, x9, x10, x11, x12, x13;

            x1 = new IntegerX(0, new uint[] { 0x0 });
            x2 = new IntegerX(1, new uint[] { 0x1 });
            x3 = new IntegerX(-1, new uint[] { 0x1 });
            x4 = new IntegerX(1, new uint[] { 0x2 });
            x5 = new IntegerX(-1, new uint[] { 0x2 });
            x6 = new IntegerX(1, new uint[] { 0xFFFFFFF0 });
            x7 = new IntegerX(-1, new uint[] { 0xFFFFFFF0 });
            x8 = new IntegerX(1, new uint[] { 0xFFFFFFF1 });
            x9 = new IntegerX(-1, new uint[] { 0xFFFFFFF1 });
            x10 = new IntegerX(1, new uint[] { 0x1, 0x2 });
            x11 = new IntegerX(1, new uint[] { 0x2, 0x1 });
            x12 = new IntegerX(1, new uint[] { 0x2, 0x2 });
            x13 = new IntegerX(1, new uint[] { 0x1, 0x1 });

            Assert.IsTrue(x1.IsOdd() == false);
            Assert.IsTrue(x2.IsOdd());
            Assert.IsTrue(x3.IsOdd());
            Assert.IsTrue(x4.IsOdd() == false);
            Assert.IsTrue(x5.IsOdd() == false);
            Assert.IsTrue(x6.IsOdd() == false);
            Assert.IsTrue(x7.IsOdd() == false);
            Assert.IsTrue(x8.IsOdd());
            Assert.IsTrue(x9.IsOdd());
            Assert.IsTrue(x10.IsOdd() == false);
            Assert.IsTrue(x11.IsOdd());
            Assert.IsTrue(x12.IsOdd() == false);
            Assert.IsTrue(x13.IsOdd());
        }
        #endregion

        #region BITWISE OPERATION TESTS -- BOOLEAN OPS

        [TestCase]
        public void BitAnd_pos_pos()
        {
            uint digit1, digit2, d0, d1, d2;
            IntegerX x, y, z, w;

            digit1 = 0xACACACAC;
            digit2 = 0xCACACACA;

            x = new IntegerX(1, new uint[] { digit1, digit2, digit1, digit2 });
            y = new IntegerX(1, new uint[] { digit1, digit2, 0 });

            d0 = digit2 & digit1;
            d1 = digit1 & digit2;
            d2 = 0;

            z = new IntegerX(1, new uint[] { d0, d1, d2 });

            w = x & y;

            Assert.IsTrue(w == z);
        }

        [TestCase]
        public void BitAnd_pos_neg()
        {
            uint digit1, digit2, d0, d1, d2, d3;
            IntegerX x, y, z, w;

            digit1 = 0xACACACAC;
            digit2 = 0xCACACACA;

            x = new IntegerX(1, new uint[] { digit1, digit2,
             digit1, digit2 });
            y= new IntegerX(-1, new uint[] { digit1, digit2, 0 });

            d0 = digit1;
            d1 = digit2 &~digit1;
            d2 = digit1 &(~digit2 + 1);
            d3 = 0;

            z= new IntegerX(1, new uint[] { d0, d1, d2, d3 });
            w= x & y;

            Assert.IsTrue(w == z);
        }

        [TestCase]
        public void BitAnd_neg_pos()
        {
            uint digit1, digit2, d0, d1, d2;
            IntegerX x, y, z, w;

            digit1 = 0xACACACAC;
            digit2 = 0xCACACACA;

            x = new IntegerX(-1, new uint[] { digit1, digit2, digit1, digit2 });
            y = new IntegerX(1, new uint[] { digit1, digit2, 0 });

            d0 = ~digit2 & digit1;
            d1 = ~digit1 & digit2;
            d2 = 0;

            z = new IntegerX(1, new uint[] { d0, d1, d2 });
            w = x & y;

            Assert.IsTrue(w == z);
        }

        [TestCase]
        public void BitAnd_neg_neg()
        {
            uint digit1, digit2, d0, d1, d2, d3;
            IntegerX x, y, z, w;

            digit1=  0xACACACAC;
            digit2=  0xCACACACA;

            x=  new IntegerX(-1, new uint[] { digit1, digit2, digit1, digit2 });
            y=  new IntegerX(-1, new uint[] { digit1, digit2, 0 });

            d0=  digit1;
            d1=  ~(~digit2 & ~digit1);
            d2=  ~(~digit1 & (~digit2 + 1)) + 1;
            d3=  0;

            z=  new IntegerX(-1, new uint[] { d0, d1, d2, d3 });
            w=  x & y;

            Assert.IsTrue(w == z);
        }

        [TestCase]
        public void BitOr_pos_pos()
        {
            uint digit1, digit2, d0, d1, d2, d3;
            IntegerX x, y, z, w;

            digit1 = 0xACACACAC;
            digit2 = 0xCACACACA;

            x = new IntegerX(1, new uint[] { digit1, digit2,
             digit1, digit2 });
            y = new IntegerX(1, new uint[] { digit1, digit2, 0 });

            d0 = digit1;
            d1 = digit2 | digit1;
            d2 = digit1 | digit2;
            d3 = digit2;

            z = new IntegerX(1, new uint[] { d0, d1, d2, d3 });
            w = x | y;

            Assert.IsTrue(w == z);
        }

        [TestCase]
        public void BitOr_pos_neg()
        {
            uint digit1, digit2, d0, d1, d2;
            IntegerX x, y, z, w;

            digit1 = 0xACACACAC;
            digit2 = 0xCACACACA;

            x = new IntegerX(1, new uint[] { digit1, digit2,
             digit1, digit2 });
            y = new IntegerX(-1, new uint[] { digit1, digit2, 0 });

            d0 = ~(digit2 | ~digit1);
            d1 = ~(digit1 | (~digit2 + 1));
            d2 = ~digit2 + 1;

            z = new IntegerX(-1, new uint[] { d0, d1, d2 });
            w = x | y;

            Assert.IsTrue(w == z);
        }

        [TestCase]
        public void BitOr_neg_pos()
        {
            uint digit1, digit2, d0, d1, d2, d3;
            IntegerX x, y, z, w;

            digit1 = 0xACACACAC;
            digit2 = 0xCACACACA;

            x = new IntegerX(-1, new uint[] { digit1, digit2, digit1, digit2 });
            y = new IntegerX(1, new uint[] { digit1, digit2, 0 });

            d0 = digit1;
            d1 = ~(~digit2 | digit1);
            d2 = ~(~digit1 | digit2);
            d3 = ~(~digit2 + 1) + 1;

            z = new IntegerX(-1, new uint[] { d0, d1, d2, d3 });
            w = x | y;

            Assert.IsTrue(w == z);
        }

        [TestCase]
        public void BitOr_neg_neg()
        {
            uint digit1, digit2, d0, d1, d2;
            IntegerX x, y, z, w;

            digit1 = 0xACACACAC;
            digit2 = 0xCACACACA;

            x = new IntegerX(-1, new uint[] { digit1, digit2, digit1, digit2 });
            y = new IntegerX(-1, new uint[] { digit1, digit2, 0 });

            d0 = ~(~digit2 | ~digit1);
            d1 = ~(~digit1 | ~digit2);
            d2 = ~(~digit2 + 1) + 1;

            z = new IntegerX(-1, new uint[] { d0, d1, d2 });
            w = x | y;

            Assert.IsTrue(w == z);
        }

        [TestCase]
        public void BitXor_pos_pos()
        {
            uint digit1, digit2, d0, d1, d2, d3;
            IntegerX x, y, z, w;

            digit1 = 0xACACACAC;
            digit2 = 0xCACACACA;

            x = new IntegerX(1, new uint[] { digit1, digit2, digit1, digit2 });
            y = new IntegerX(1, new uint[] { digit1, digit2, 0 });

            d0 = ~(digit1 ^ 0x0);
            d1 = ~(digit2 ^ digit1);
            d2 = ~(digit1 ^ digit2);
            d3 = ~(digit2 ^ 0x0) + 1;

            z = new IntegerX(-1, new uint[] { d0, d1, d2, d3 });
            w = x ^ y;

            Assert.IsTrue(w == z);
        }

        [TestCase]
        public void BitXor_pos_neg()
        {
            uint digit1, digit2, d0, d1, d2, d3;
            IntegerX x, y, z, w;

            digit1 = 0xACACACAC;
            digit2 = 0xCACACACA;

            x = new IntegerX(1, new uint[] { digit1, digit2, digit1, digit2 });
            y = new IntegerX(-1, new uint[] { digit1, digit2, 0 });

            d0 = digit1 ^~(uint)0x0;
            d1 = digit2 ^~digit1;
            d2 = digit1 ^(~digit2 + 1);
            d3 = digit2 ^ 0x0;

            z = new IntegerX(1, new uint[] { d0, d1, d2, d3 });
            w = x ^ y;

            Assert.IsTrue(w == z);
        }

        [TestCase]
        public void BitXor_neg_pos()
        {
            uint digit1, digit2, d0, d1, d2, d3;
            IntegerX x, y, z, w;

            digit1 = 0xACACACAC;
            digit2 = 0xCACACACA;

            x = new IntegerX(-1, new uint[] { digit1, digit2, digit1, digit2 });
            y = new IntegerX(1, new uint[] { digit1, digit2, 0 });

            d0 = ~digit1 ^ 0x0;
            d1 = ~digit2 ^ digit1;
            d2 = ~digit1 ^ digit2;
            d3 = (~digit2 +1) ^ (uint)0x0;

            z = new IntegerX(1, new uint[] { d0, d1, d2, d3 });
            w = x ^ y;

            Assert.IsTrue(w == z);
        }

        [TestCase]
        public void BitXor_neg_neg()
        {
            uint digit1, digit2, d0, d1, d2, d3;
            IntegerX x, y, z, w;

            digit1 = 0xACACACAC;
            digit2 = 0xCACACACA;

            x = new IntegerX(-1, new uint[] {digit1, digit2,
             digit1, digit2 });
            y = new IntegerX(-1, new uint[] { digit1, digit2, 0 });

            d0 = ~(~digit1 ^ ~(uint)0x0);
            d1 = ~(~digit2 ^ ~digit1);
            d2 = ~(~digit1 ^ (~digit2 + 1));
            d3 = ~((~digit2 + 1) ^ 0x0) + 1;

            z = new IntegerX(-1, new uint[] { d0, d1, d2, d3 });
            w = x ^ y;

            Assert.IsTrue(w == z);
        }

        [TestCase]
        public void BitAndNot_pos_pos()
        {
            uint digit1, digit2, d0, d1, d2, d3;
            IntegerX x, y, z, w;

            digit1 = 0xACACACAC;
            digit2 = 0xCACACACA;

            x = new IntegerX(1, new uint[] { digit1, digit2, digit1, digit2 });
            y = new IntegerX(1, new uint[] { digit1, digit2, 0 });

            d0 = digit1;
            d1 = digit2 & ~digit1;
            d2 = digit1 & ~digit2;
            d3 = digit2;

            z = new IntegerX(1, new uint[] { d0, d1, d2, d3 });
            w = x.BitwiseAndNot(y);

            Assert.IsTrue(w == z);
        }

        [TestCase]
        public void BitAndNot_pos_neg()
        {
            uint digit1, digit2, d0, d1, d2;
            IntegerX x, y, z, w;

            digit1 = 0xACACACAC;
            digit2 = 0xCACACACA;

            x = new IntegerX(1, new uint[] { digit1, digit2, digit1, digit2 });
            y = new IntegerX(-1, new uint[] { digit1, digit2, 0 });

            d0 = digit2 &  digit1;
            d1 = digit1 & ~(~digit2 + 1);
            d2 = digit2;

            z = new IntegerX(1, new uint[] { d0, d1, d2 });
            w = x.BitwiseAndNot(y);

            Assert.IsTrue(w == z);
        }

        [TestCase]
        public void BitAndNot_neg_pos()
        {
            uint digit1, digit2, d0, d1, d2, d3;
            IntegerX x, y, z, w;

            digit1 = 0xACACACAC;
            digit2 = 0xCACACACA;

            x = new IntegerX(-1, new uint[] { digit1, digit2, digit1, digit2 });
            y = new IntegerX(1, new uint[] { digit1, digit2, 0 });

            d0 = digit1;
            d1 = digit2 | digit1;
            d2 = digit1 | digit2;
            d3 = digit2;

            z = new IntegerX(-1, new uint[] { d0, d1, d2, d3 });
            w = x.BitwiseAndNot(y);

            Assert.IsTrue(w == z);
        }

        [TestCase]
        public void BitAndNot_neg_neg()
        {
            uint digit1, digit2, d0, d1, d2;
            IntegerX x, y, z, w;

            digit1 = 0xACACACAC;
            digit2 = 0xCACACACA;

            x = new IntegerX(-1, new uint[] {digit1, digit2, digit1, digit2 });
            y = new IntegerX(-1, new uint[] { digit1, digit2, 0 });

            d0 = ~digit2 & digit1;
            d1 = ~digit1 & ~(~digit2 + 1);
            d2 = ~digit2 +1;

            z = new IntegerX(1, new uint[] { d0, d1, d2 });
            w = x.BitwiseAndNot(y);

            Assert.IsTrue(w == z);
        }

        [TestCase]
        public void BitNot_pos()
        {
            uint digit1, digit2, d0, d1, d2;
            IntegerX x, z, w;

            digit1 = 0xACACACAC;
            digit2 = 0xCACACACA;

            x = new IntegerX(1, new uint[] { digit1, digit2, 0 });

            d0 = digit1;
            d1 = digit2;
            d2 = 1;

            z = new IntegerX(-1, new uint[] { d0, d1, d2 });
            w = x.OnesComplement();

            Assert.IsTrue(w == z);
        }

        [TestCase]
        public void BitNot_neg()
        {
            uint digit1, digit2, d0, d1, d2;
            IntegerX x, z, w;

            digit1 = 0xACACACAC;
            digit2 = 0xCACACACA;

            x = new IntegerX(-1, new uint[] { digit1, digit2, 0 });

            d0 = digit1;
            d1 = ~(~digit2 + 1);
            d2 = 0xFFFFFFFF;

            z = new IntegerX(1, new uint[] { d0, d1, d2 });
            w = x.OnesComplement();

            Assert.IsTrue(w == z);
        }
        #endregion

        #region BITWISE OPERATION TESTS -- SINGLE BIT
        [TestCase]
        public void TestBit_pos_inside()
        {
            var x = new IntegerX(1, new uint[] { 0xAAAAAAAA, 0xAAAAAAAA });
            var i = 0;
            while (i < 64)
            {
                Assert.IsTrue(x.TestBit(i) == (i % 2 != 0));
                i++;
            }
        }

        [TestCase]
        public void TestBit_neg_inside()
        {
            var x = new IntegerX(-1, new uint[] { 0xAAAAAAAA, 0xAAAAAAAA });

            Assert.IsTrue(x.TestBit(0) == false);
            Assert.IsTrue(x.TestBit(1) == true);

            var i = 2;
            while (i < 64)
            {
                Assert.IsTrue(x.TestBit(i) == (i % 2 == 0));
                i++;
            }
        }

        [TestCase]
        public void TestBit_pos_outside()
        {
            var x = new IntegerX(1, new uint[] { 0xAAAAAAAA, 0xAAAAAAAA });

            Assert.IsTrue(x.TestBit(1000) == false);
        }

        [TestCase]
        public void TestBit_neg_outside()
        {
            var x = new IntegerX(-1, new uint[] { 0xAAAAAAAA, 0xAAAAAAAA });

            Assert.IsTrue(x.TestBit(1000));
        }

        [TestCase]
        public void SetBit_pos_inside_initial_set()
        {
            var x = new IntegerX(1, new uint[] { 0xFFFF0000, 0xFFFF0000 });
            var y = x.SetBit(56);

            Assert.IsTrue(y == x);
        }

        [TestCase]
        public void SetBit_pos_inside_initial_clear()
        {
            var x = new IntegerX(1, new uint[] { 0xFFFF0000, 0xFFFF0000 });
            var w = new IntegerX(1, new uint[] { 0xFFFF0080, 0xFFFF0000 });
            var y = x.SetBit(39);

            Assert.IsTrue(y == w);
        }

        [TestCase]
        public void SetBit_pos_outside()
        {
            var x = new IntegerX(1, new uint[] { 0xFFFF0000, 0xFFFF0000 });
            var y = x.SetBit(99);
            Assert.IsTrue(SameValue(y, 1, new uint[] {8, 0, 0xFFFF0000, 0xFFFF0000 }));
        }

        [TestCase]
        public void SetBit_neg_inside_initial_set()
        {
            var x = new IntegerX(-1, new uint[] { 0xFFFF0000, 0xFFFF0000 });
            var y = x.SetBit(39);
            Assert.IsTrue(y == x);
        }

        [TestCase]
        public void SetBit_neg_inside_initial_clear()
        {
            var x = new IntegerX(-1, new uint[] { 0xFFFF0000, 0xFFFF0000 });
            var w = new IntegerX(-1, new uint[] { 0xFEFF0000, 0xFFFF0000 });
            var y = x.SetBit(56);
            Assert.IsTrue(y == w);
        }

        [TestCase]
        public void SetBit_neg_outside()
        {
            var x = new IntegerX(-1, new uint[] { 0xFFFF0000, 0xFFFF0000 });
            var y = x.SetBit(99);
            Assert.IsTrue(SameValue(y, -1, new uint[] { 0xFFFF0000, 0xFFFF0000 }));
        }

        [TestCase]
        public void ClearBit_pos_inside_initial_set()
        {
            var x = new IntegerX(1, new uint[] { 0xFFFF0000, 0xFFFF0000 });
            var w = new IntegerX(1, new uint[] { 0xFEFF0000, 0xFFFF0000 });
            var y = x.ClearBit(56);
            Assert.IsTrue(y == w);
        }

        [TestCase]
        public void ClearBit_pos_inside_initial_clear()
        {
            var x = new IntegerX(1, new uint[] { 0xFFFF0000, 0xFFFF0000 });
            var y = x.ClearBit(39);
            Assert.IsTrue(y == x);
        }

        [TestCase]
        public void ClearBit_pos_outside()
        {
            var x = new IntegerX(1, new uint[] { 0xFFFF0000, 0xFFFF0000 });
            var y = x.ClearBit(99);
            Assert.IsTrue(y == x);
        }

        [TestCase]
        public void ClearBit_neg_inside_initial_set()
        {
            var x = new IntegerX(-1, new uint[] { 0xFFFF0000, 0xFFFF0000 });
            var y = x.ClearBit(39);
            Assert.IsTrue(SameValue(y, -1, new uint[] { 0xFFFF0080, 0xFFFF0000 }));
        }

        [TestCase]
        public void ClearBit_neg_inside_initial_clear()
        {
            var x = new IntegerX(-1, new uint[] { 0xFFFF0000, 0xFFFF0000 });
            var y = x.ClearBit(56);
            Assert.IsTrue(y == x);
        }

        [TestCase]
        public void ClearBit_neg_outside()
        {
            var x = new IntegerX(-1, new uint[] { 0xFFFF0000, 0xFFFF0000 });
            var y = x.ClearBit(99);
            Assert.IsTrue(SameValue(y, -1, new uint[] { 8, 0, 0xFFFF0000, 0xFFFF0000 }));
        }

        [TestCase]
        public void FlipBit_pos_inside_initial_set()
        {
            var x = new IntegerX(1, new uint[] { 0xFFFF0000, 0xFFFF0000 });
            var w = new IntegerX(1, new uint[] { 0xFEFF0000, 0xFFFF0000 });
            var y = x.FlipBit(56);
            Assert.IsTrue(y == w);
        }

        [TestCase]
        public void FlipBit_pos_inside_initial_clear()
        {
            var x = new IntegerX(1, new uint[] { 0xFFFF0000, 0xFFFF0000 });
            var w = new IntegerX(1, new uint[] { 0xFFFF0080, 0xFFFF0000 });
            var y = x.FlipBit(39);
            Assert.IsTrue(y == w);
        }

        [TestCase]
        public void FlipBit_pos_outside()
        {
            var x = new IntegerX(1, new uint[] { 0xFFFF0000, 0xFFFF0000 });
            var y = x.FlipBit(99);
            Assert.IsTrue(SameValue(y, 1, new uint[] { 8, 0, 0xFFFF0000, 0xFFFF0000 }));
        }

        [TestCase]
        public void FlipBit_neg_inside_initial_set()
        {
            var x = new IntegerX(-1, new uint[] { 0xFFFF0000, 0xFFFF0000 });
            var w = new IntegerX(-1, new uint[] { 0xFFFF0080, 0xFFFF0000 });
            var y = x.FlipBit(39);
            Assert.IsTrue(y == w);
        }

        [TestCase]
        public void FlipBit_neg_inside_initial_clear()
        {
            var x = new IntegerX(-1, new uint[] { 0xFFFF0000, 0xFFFF0000 });
            var w = new IntegerX(-1, new uint[] { 0xFEFF0000, 0xFFFF0000 });
            var y = x.FlipBit(56);
            Assert.IsTrue(y == w);
        }

        [TestCase]
        public void FlipBit_neg_outside()
        {
            var x = new IntegerX(-1, new uint[] { 0xFFFF0000, 0xFFFF0000 });
            var y = x.FlipBit(99);
            Assert.IsTrue(SameValue(y, -1, new uint[] { 8, 0, 0xFFFF0000, 0xFFFF0000 }));
        }
        #endregion

        #region BITWISE OPERATION TEST -- SHIFTS

        [TestCase]
        public void LeftShift_zero_is_zero()
        {
            var x = IntegerX.Create(0);
            var y = x.LeftShift(1000);
            Assert.IsTrue(y.IsZero());
        }

        [TestCase]
        public void LeftShift_neg_shift_same_as_right_shift()
        {
            uint digit1 = 0xC1F0F1CD;
            uint digit2 = 0xB38F4F83;
            uint digit3 = 0x1234678;
            var x = new IntegerX(1, new uint[] { digit1, digit2, digit3 });
            var y = x.LeftShift(-40);
            var z = x.RightShift(40);
            Assert.IsTrue(y == z);
        }

        [TestCase]
        public void LeftShift_zero_shift_is_this()
        {
            uint digit1 = 0xC1F0F1CD;
            uint digit2 = 0xB38F4F83;
            uint digit3 = 0x1234678;
            var x = new IntegerX(1, new uint[] { digit1, digit2, digit3 });
            var y = x.LeftShift(0);
            Assert.IsTrue(y == x);
        }

        [TestCase]
        public void LeftShift_pos_whole_digit_shift_adds_zeros_at_end()
        {
            uint digit1 = 0xC1F0F1CD;
            uint digit2 = 0xB38F4F83;
            uint digit3 = 0x1234678;
            var x = new IntegerX(1, new uint[] { digit1, digit2, digit3 });
            var y = x.LeftShift(64);
            var w = new IntegerX(1, new uint[] { digit1, digit2, digit3, 0, 0 });
            Assert.IsTrue(y == w);
        }

        [TestCase]
        public void LeftShift_pos_small_shift()
        {
            uint digit1 = 0xC1F0F1CD;
            uint digit2 = 0xB38F4F83;
            uint digit3 = 0x1234678;
            var x = new IntegerX(1, new uint[] { digit1, digit2, digit3 });
            var y = x.LeftShift(7);
            var w = new IntegerX(1, new uint[] { digit1 >> 25, (digit1 << 7) | (digit2 >> 25), (digit2 << 7) | (digit3 >> 25), digit3 << 7});
            Assert.IsTrue(y == w);
        }

        [TestCase]
        public void LeftShift_neg_small_shift()
        {
            uint digit1 = 0xC1F0F1CD;
            uint digit2 = 0xB38F4F83;
            uint digit3 = 0x1234678;
            var x = new IntegerX(-1, new uint[] { digit1, digit2, digit3 });
            var y = x.LeftShift(7);
            var w = new IntegerX(-1, new uint[] { digit1 >> 25, (digit1 << 7) | (digit2 >> 25), (digit2 << 7) | (digit3 >> 25), digit3 << 7 });
            Assert.IsTrue(y == w);
        }

        [TestCase]
        public void LeftShift_pos_big_shift()
        {
            uint digit1 = 0xC1F0F1CD;
            uint digit2 = 0xB38F4F83;
            uint digit3 = 0x1234678;
            var x = new IntegerX(1, new uint[] { digit1, digit2, digit3 });
            var y = x.LeftShift(7 + 64);
            var w = new IntegerX(1, new uint[] { digit1 >> 25, (digit1 << 7) | (digit2 >> 25), (digit2 << 7) | (digit3 >> 25), digit3 << 7, 0, 0 });
            Assert.IsTrue(y == w);
        }

        [TestCase]
        public void LeftShift_neg_big_shift()
        {
            uint digit1 = 0xC1F0F1CD;
            uint digit2 = 0xB38F4F83;
            uint digit3 = 0x1234678;
            var x = new IntegerX(-1, new uint[] { digit1, digit2, digit3 });
            var y = x.LeftShift(7 + 64);
            var w = new IntegerX(-1, new uint[] { digit1 >> 25, (digit1 << 7) | (digit2 >> 25), (digit2 << 7) | (digit3 >> 25), digit3 << 7, 0, 0 });
            Assert.IsTrue(y == w);
        }

        [TestCase]
        public void LeftShift_pos_big_shift_zero_high_bits()
        {
            uint digit1 = 0x0000F1CD;
            uint digit2 = 0xB38F4F83;
            uint digit3 = 0x1234678;
            var x = new IntegerX(1, new uint[] { digit1, digit2, digit3 });
            var y = x.LeftShift(7 + 64);
            var w = new IntegerX(1, new uint[] { digit1 >> 25, (digit1 << 7) | (digit2 >> 25), (digit2 << 7) | (digit3 >> 25), digit3 << 7, 0, 0 });
            Assert.IsTrue(y == w);
        }

        [TestCase]
        public void RightShift_zero_is_zero()
        {
            var x = IntegerX.Create(0);
            var y = x.RightShift(1000);
            Assert.IsTrue(y.IsZero());
        }

        [TestCase]
        public void RightShift_neg_shift_same_as_left_shift()
        {
            uint digit1 = 0xC1F0F1CD;
            uint digit2 = 0xB38F4F83;
            uint digit3 = 0x1234678;
            var x = new IntegerX(1, new uint[] { digit1, digit2, digit3 });
            var y = x.RightShift(-40);
            var z = x.LeftShift(40);
            Assert.IsTrue(y == z);
        }

        [TestCase]
        public void RightShift_zero_shift_is_this()
        {
            uint digit1 = 0xC1F0F1CD;
            uint digit2 = 0xB38F4F83;
            uint digit3 = 0x1234678;
            var x = new IntegerX(1, new uint[] { digit1, digit2, digit3 });
            var y = x.RightShift(0);
            Assert.IsTrue(y == x);
        }

        [TestCase]
        public void RightShift_pos_whole_digit_shift_loses_whole_digits()
        {
            uint digit1 = 0xC1F0F1CD;
            uint digit2 = 0xB38F4F83;
            uint digit3 = 0x1234678;
            var x = new IntegerX(1, new uint[] { digit1, digit2, digit3 });
            var y = x.RightShift(64);
            var w = new IntegerX(1, new uint[] { digit1 });
            Assert.IsTrue(y == w);
        }

        [TestCase]
        public void RightShift_neg_whole_digit_shift_loses_whole_digits()
        {
            uint digit1 = 0xC1F0F1CD;
            uint digit2 = 0xB38F4F83;
            uint digit3 = 0x1234678;
            var x = new IntegerX(-1, new uint[] { digit1, digit2, digit3 });
            var y = x.RightShift(64);
            var w = new IntegerX(-1, new uint[] { digit1 });
            Assert.IsTrue(y == w);
        }

        [TestCase]
        public void RightShift_pos_small_shift()
        {
            uint digit1 = 0xC1F0F1CD;
            uint digit2 = 0xB38F4F83;
            uint digit3 = 0x1234678;
            var x = new IntegerX(1, new uint[] { digit1, digit2, digit3 });
            var y = x.RightShift(7);
            var w = new IntegerX(1, new uint[] { digit1 >> 7, (digit1 << 25) | (digit2 >> 7), (digit2 << 25) | (digit3 >> 7) });
            Assert.IsTrue(y == w);
        }

        [TestCase]
        public void RightShift_neg_small_shift()
        {
            uint digit1 = 0xC1F0F1CD;
            uint digit2 = 0xB38F4F83;
            uint digit3 = 0x1234678;
            var x = new IntegerX(-1, new uint[] { digit1, digit2, digit3 });
            var y = x.RightShift(7);
            var w = new IntegerX(-1, new uint[] { digit1 >> 7, (digit1 << 25) | (digit2 >> 7), (digit2 << 25) | (digit3 >> 7)});
            Assert.IsTrue(y == w);
        }

        [TestCase]
        public void RightShift_pos_big_shift()
        {
            uint digit1 = 0xC1F0F1CD;
            uint digit2 = 0xB38F4F83;
            uint digit3 = 0x1234678;
            var x = new IntegerX(1, new uint[] { digit1, digit2, digit3 });
            var y = x.RightShift(7 + 32);
            var w = new IntegerX(1, new uint[] { digit1 >> 7, (digit1 << 25) | (digit2 >> 7) });
            Assert.IsTrue(y == w);
        }

        [TestCase]
        public void RightShift_neg_big_shift()
        {
            uint digit1 = 0xC1F0F1CD;
            uint digit2 = 0xB38F4F83;
            uint digit3 = 0x1234678;
            var x = new IntegerX(-1, new uint[] { digit1, digit2, digit3 });
            var y = x.RightShift(7 + 32);
            var w = new IntegerX(-1, new uint[] { digit1 >> 7, (digit1 << 25) | (digit2 >> 7)});
            Assert.IsTrue(y == w);
        }

        [TestCase]
        public void RightShift_pos_big_shift_zero_high_bits()
        {
            uint digit1 = 0x00001D;
            uint digit2 = 0xB38F4F83;
            uint digit3 = 0x1234678;
            var x = new IntegerX(1, new uint[] { digit1, digit2, digit3 });
            var y = x.RightShift(7 + 32);
            var w = new IntegerX(1, new uint[] { digit1 >> 7, (digit1 << 25) | (digit2 >> 7) });
            Assert.IsTrue(y == w);
        }
        #endregion

        #region ATTEMPTED CONVERSION METHODS
        private void AsIntTest(IntegerX i, bool expRet, int expInt)
        {
            int v;
            bool b;

            b = i.AsInt(out v);
            Assert.IsTrue(b == expRet);
            Assert.IsTrue(v == expInt);
        }

        [TestCase]
        public void AsInt_various()
        {
            AsIntTest(IntegerX.Create(0), true, 0);

            AsIntTest(new IntegerX(-1, new uint[] { 0x80000001 }), false, 0);

            AsIntTest(new IntegerX(1, new uint[] { 0x80000000 }), false, 0);

            AsIntTest(new IntegerX(-1, new uint[] { 0x80000000 }), true,
              Int32.MinValue);

            AsIntTest(IntegerX.Create(100), true, 100);

            AsIntTest(IntegerX.Create(-100), true, -100);

            AsIntTest(new IntegerX(1, new uint[] { 1, 0 }), false, 0);
        }

        private void AsLongTest(IntegerX i, bool expRet, long expInt)
        {
            long v;
            bool b;

            b = i.AsLong(out v);
            Assert.IsTrue(b == expRet);
            Assert.IsTrue(v == expInt);
        }

        [TestCase]
        public void AsLong_various()
        {
            AsLongTest(IntegerX.Create(0), true, 0);

            AsLongTest(new IntegerX(-1, new uint[] { 0x80000001 }), true, (long)int.MinValue - 1);

            AsLongTest(new IntegerX(1, new uint[] { 0x80000000 }), true, 0x80000000);

            AsLongTest(new IntegerX(-1, new uint[] { 0x80000000 }), true, (long)int.MinValue);

            AsLongTest(new IntegerX(-1, new uint[] { 0x80000000, 0x00000001 }), false, 0);

            AsLongTest(new IntegerX(1, new uint[] { 0x80000000, 0x0 }), false, 0);

            AsLongTest(new IntegerX(-1, new uint[] { 0x80000000, 0x0 }), true, long.MinValue);

            AsLongTest(IntegerX.Create(100), true, 100);

            AsLongTest(IntegerX.Create(-100), true, -100);

            AsLongTest(IntegerX.Create(123456789123456), true, 123456789123456);

            AsLongTest(IntegerX.Create(-123456789123456), true, -123456789123456);

            AsLongTest(new IntegerX(1, new uint[] { 1, 0, 0 }), false, 0);
        }

        private void AsUIntTest(IntegerX i, bool expRet, uint expInt)
        {
            uint v;
            bool b;

            b = i.AsUInt(out v);
            Assert.IsTrue(b == expRet);
            Assert.IsTrue(v == expInt);
        }

        [TestCase]
        public void AsUInt_various()
        {
            AsUIntTest(IntegerX.Create(-1), false, 0);

            AsUIntTest(IntegerX.Create(0), true, 0);

            AsUIntTest(new IntegerX(1, new uint[] { 0xFFFFFFFF }), true, 0xFFFFFFFF);

            AsUIntTest(new IntegerX(1, new uint[] {0x1, 0x0 }), false, 0);
        }

        private void AsULongTest(IntegerX i, bool expRet, ulong expInt)
        {
            ulong v;
            bool b;

            b = i.AsULong(out v);
            Assert.IsTrue(b == expRet);
            Assert.IsTrue(v == expInt);
        }

        [TestCase]
        public void AsULong_various()
        {
            AsULongTest(IntegerX.Create(-1), false, 0);

            AsULongTest(IntegerX.Create(0), true, 0);

            AsULongTest(new IntegerX(1, new uint[] { 0xFFFFFFFF }), true, 0xFFFFFFFF);

            AsULongTest(new IntegerX(1, new uint[] { 0xFFFFFFFF, 0xFFFFFFFF }), true, 0xFFFFFFFFFFFFFFFF);

            AsLongTest(new IntegerX(1, new uint[] { 0x1, 0x0, 0x0 }), false, 0);
        }
        #endregion

        #region EQUATABLE
        [TestCase]
        public void Equals_I_on_same_is_true()
        {
            var i = new IntegerX(1, new uint[] { 0x1, 0x2, 0x3 });
            var j = new IntegerX(1, new uint[] { 0x1, 0x2, 0x3 });
            Assert.IsTrue(i.Equals(j));
        }

        public void Equals_I_on_different_is_false()
        {
            var i = new IntegerX(1, new uint[] { 0x1, 0x2, 0x3 });
            var j = new IntegerX(1, new uint[] { 0x1, 0x2, 0x4 });
            Assert.IsTrue(i.Equals(j) == false);
        }
        #endregion

        #region PRECISION TESTS
        [TestCase]
        public void PrecisionSingleDigitsIsOne()
        {
            var i = -9;
            while(i <= 9)
            {
                var bi = IntegerX.Create(i);
                Assert.IsTrue(bi.Precision == 1);
                i++;
            }
        }

        [TestCase]
        public void PrecisionTwoDigitsIsTwo()
        {
            var values = new int[] { -99, -50, -11, -10, 10, 11, 50, 99 };

            foreach (var v in values)
            {
                var bi = IntegerX.Create(v);
                Assert.IsTrue(bi.Precision == 2);
            }
        }

        [TestCase]
        public void PrecisionThreeDigitsIsThree()
        {
            var values = new int[] { -999, -509, -101, -100, 100, 101, 500, 999 };

            foreach (var v in values)
            {
                var bi = IntegerX.Create(v);
                Assert.IsTrue(bi.Precision == 3);
            }
        }

        [TestCase]
        public void PrecisionBoundaryCases()
        {
            string nines, tenpow;
            int i;
            IntegerX bi9, bi0;

            nines = "";
            tenpow = "1";

            i = 1;
            while (i < 30)
            {
                nines = nines + "9";
                tenpow = tenpow + "0";
                bi9 = IntegerX.Parse(nines);
                bi0 = IntegerX.Parse(tenpow);
                Assert.IsTrue(bi9.Precision ==(uint)i);
                Assert.IsTrue(bi0.Precision == (uint)(i + 1));
                i++;
            }
        }

        [TestCase]
        public void PrecisionBoundaryCase2()
        {
            var x = new IntegerX(1, new uint[] { 0xFFFFFFFF });
            var y = new IntegerX(1, new uint[] { 0x1, 0x0 });
            Assert.IsTrue(x.Precision == 10);
            Assert.IsTrue(y.Precision == 10);
        }
        #endregion
    }
}
