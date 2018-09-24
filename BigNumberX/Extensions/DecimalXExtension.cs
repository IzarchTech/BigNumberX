using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;

namespace BigNumberX
{
    public static class DecimalXExtension
    {
        private const string NegativeSquareRoot = "Cannot compute squareroot of negative number";
        private const string SqrtScaleInvalid = "Scale cannot be <= 0.";
        private const string InvalidScale = "Scale cannot be < 0.";
        private const string InvalidScale2 = "Scale cannot be <= 0.";
        private const string NegativeOrZeroNaturalLog = "Cannot compute Natural Log of Negative or Zero Number.";
        private const string NegativeIntRoot = "Cannot compute IntRoot of Negative Number";

        /// <summary>
        /// Compute x^exponent to a given scale. Uses the same algorithm as class
        /// numbercruncher.mathutils.IntPower.
        /// </summary>
        /// <param name="exponent">
        /// the exponent value
        /// </param>
        /// <param name="scale">
        /// the desired <c>scale</c> of the result. (where the <c>scale</c> is
        /// the number of digits to the right of the decimal point.
        /// </param>
        /// <returns>
        /// the result value
        /// </returns>
        /// <exception cref="ArgumentException">
        /// if <c>scale</c> &lt; 0
        /// </exception>
        public static DecimalX IntPower(this DecimalX x, long exponent, int scale)
        {
            DecimalX a, power;

            if (scale < 0)
                throw new ArgumentException(InvalidScale);

            if (exponent < 0)
            {
                a = DecimalX.Create(1);
                return a.CDivide(x.IntPower(-exponent, scale), scale, RoundingMode.HalfEven);
            }

            power = DecimalX.Create(1);

            while (exponent > 0)
            {
                if ((exponent & 1) == 1)
                {
                    power = power.Multiply(x);
                    power = DecimalX.Rescale(power, -scale, RoundingMode.HalfEven);
                }

                x = x.Multiply(x);
                x = DecimalX.Rescale(x, -scale, RoundingMode.HalfEven);
                exponent = exponent >> 1;
            }

            return power;
        }

        /// <summary>
        /// Compute the integral root of x to a given scale, x &gt;= 0 Using
        /// Newton's algorithm.
        /// </summary>
        /// <param name="index">
        /// the integral root value
        /// </param>
        /// <param name="scale">
        /// the desired <c>scale</c> of the result. (where the <c>scale</c> is
        /// the number of digits to the right of the decimal point.
        /// </param>
        /// <returns>
        /// the result value
        /// </returns>
        /// <exception cref="ArgumentException">
        /// if <c>scale</c> &lt; 0.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// if <c>self</c> &lt; 0.
        /// </exception>
        public static DecimalX IntRoot(this DecimalX x, long index, int scale)
        {
            DecimalX n, i, im1, tolerance, xPrev, xToIm1, xToI, numerator, denominator;
            int sp1;

            if (scale < 0)
                throw new ArgumentException(InvalidScale);

            if (x.Signum() < 0)
                throw new ArgumentException(NegativeIntRoot);

            sp1 = scale + 1;
            n = x;
            i = DecimalX.Create(index);
            im1 = DecimalX.Create(index - 1);
            tolerance = DecimalX.Create(5);
            tolerance= tolerance.MovePointLeft(sp1);

            // The initial approximation is x/index.
            x = x.CDivide(i, scale, RoundingMode.HalfEven);

            // Loop until the approximations converge
            // (two successive approximations are equal after rounding).
            do
            {
                // x^(index-1)
                xToIm1 = x.IntPower(index - 1, sp1);

                // x^index
                xToI = x.Multiply(xToIm1);
                xToI = DecimalX.Rescale(xToI, -sp1, RoundingMode.HalfEven);

                // n + (index-1)*(x^index)
                numerator = n.Add(im1.Multiply(xToI));
                numerator = DecimalX.Rescale(numerator, -sp1, RoundingMode.HalfEven);

                // (index*(x^(index-1))
                denominator = i.Multiply(xToIm1);
                denominator = DecimalX.Rescale(denominator, -sp1, RoundingMode.HalfEven);

                // x = (n + (index-1)*(x^index)) / (index*(x^(index-1)))
                xPrev = x;
                x = numerator.CDivide(denominator, sp1, RoundingMode.Down);

            } while (x.Subtract(xPrev).Abs().CompareTo(tolerance) > 0);

            return x;
        }

        /// <summary>
        /// Compute e^x to a given scale. <br />Break x into its whole and
        /// fraction parts and compute (e^(1 + fraction/whole))^whole using
        /// Taylor's formula.
        /// </summary>
        /// <param name="scale">
        /// the desired <c>scale</c> of the result. (where the <c>scale</c> is
        /// the number of digits to the right of the decimal point.
        /// </param>
        /// <returns>
        /// the result value
        /// </returns>
        /// <exception cref="ArgumentException">
        /// if <c>scale</c> &lt;= 0.
        /// </exception>
        public static DecimalX Exp(this DecimalX x, int scale)
        {
            DecimalX a, xWhole, xFraction, z, t, maxLong, tempRes, b;

            if (scale <= 0)
                throw new ArgumentException(InvalidScale2);

            if (x.Signum() == 0)
            {
                return DecimalX.Create(1);
            }
            else if (x.Signum() == -1)
            {
                a = DecimalX.Create(1);
                return a.CDivide(x.Negate().Exp(scale), scale, RoundingMode.HalfEven);
            }

            // Compute the whole part of x.
            xWhole = DecimalX.Rescale(x, 0, RoundingMode.Down);

            // If there isn't a whole part, compute and return e^x.
            if (xWhole.Signum() == 0)
                return expTaylor(x, scale);

            // Compute the fraction part of x.
            xFraction = x.Subtract(xWhole);

            // z = 1 + fraction/whole
            b = DecimalX.Create(1);
            z = b.Add(xFraction.CDivide(xWhole, scale, RoundingMode.HalfEven));

            // t = e^z
            t = expTaylor(z, scale);

            maxLong = DecimalX.Create(long.MaxValue);
            tempRes = DecimalX.Create(1);

            // Compute and return t^whole using IntPower().
            // If whole > Int64.MaxValue, then first compute products
            // of e^Int64.MaxValue.
            while (xWhole.CompareTo(maxLong) >= 0)
            {
                tempRes = tempRes.Multiply(t.IntPower(long.MaxValue, scale));
                tempRes = DecimalX.Rescale(tempRes, -scale, RoundingMode.HalfEven);
                xWhole= xWhole.Subtract(maxLong);
            }

            var result = tempRes.Multiply(t.IntPower(xWhole.ToLong(), scale));
            return DecimalX.Rescale(result, -scale, RoundingMode.HalfEven);
        }

        /// <summary>
        /// Compute the natural logarithm of x to a given scale, x > 0.
        /// </summary>
        public static DecimalX Ln(this DecimalX x, int scale)
        {
            DecimalX root, lnRoot, a;
            int magnitude;
            // Check that scale > 0.
            if (scale <= 0)
                throw new ArgumentException(InvalidScale2);

            // Check that x > 0.
            if (x.Signum() <= 0)
                throw new ArgumentException(NegativeOrZeroNaturalLog);

            // The number of digits to the left of the decimal point.
            magnitude = x.ToString().Length - (-x.Exponent) - 1;

            if (magnitude < 3)
            {
                return lnNewton(x, scale);
            }
            else
            {
                // x^(1/magnitude)
                root = x.IntRoot(magnitude, scale);

                // ln(x^(1/magnitude))
                lnRoot = lnNewton(root, scale);

                // magnitude*ln(x^(1/magnitude))
                a = DecimalX.Create(magnitude);
                var result = a.Multiply(lnRoot);
                return DecimalX.Rescale(result, -scale, RoundingMode.HalfEven);
            }

        }

        /// <summary>
        /// Compute the square root of self to a given scale, Using Newton's
        /// algorithm. x &gt;= 0.
        /// </summary>
        /// <param name="scale">
        /// the desired <c>scale</c> of the result. (where the <c>scale</c> is
        /// the number of digits to the right of the decimal point.
        /// </param>
        /// <returns>
        /// the result value
        /// </returns>
        /// <exception cref="ArgumentException">
        /// if <c>scale</c> is &lt;= 0.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// if <c>self</c> is &lt; 0.
        /// </exception>
        public static DecimalX Sqrt(this DecimalX x, int scale)
        {
            IntegerX n, ix, ixPrev;
            int bits;

            // Check that scale > 0.
            if (scale <= 0)
                throw new ArgumentException(SqrtScaleInvalid);

            // Check that x >= 0.
            if (x.Signum() < 0)
                throw new ArgumentException(NegativeSquareRoot);

            if (x.Signum() == 0)
                return new DecimalX(x.ToIntegerX(), -scale);

            // n = x*(10^(2*scale))
            n = x.MovePointRight(scale << 1).ToIntegerX();

            // The first approximation is the upper half of n.
            bits = (int)(n.BitLength() + 1) >> 1;
            ix = n.RightShift(bits);
            ixPrev = 0;

            // Loop until the approximations converge
            // (two successive approximations are equal after rounding).
            while (ix.CompareTo(ixPrev) != 0)
            {
                ixPrev = ix;

                // x = (x + n/x)/2
                ix = ix.Add(n.Divide(ix)).RightShift(1);
            }

            return new DecimalX(ix, -scale);
        }

        private static DecimalX CDivide(this DecimalX dividend, DecimalX divisor, int scale, RoundingMode roundingMode)
        {
            if (dividend.CheckExponent((long)scale + -divisor.Exponent) > -dividend.Exponent)
            {
                dividend = DecimalX.Rescale(dividend, -scale + divisor.Exponent, RoundingMode.Unnecessary);
            }
            else
            {
                divisor = DecimalX.Rescale(divisor, (dividend.CheckExponent((long)dividend.Exponent - (-scale))), RoundingMode.Unnecessary);
            }

            return new DecimalX(DecimalX.RoundingDivide2(dividend.Coefficient, divisor.Coefficient, roundingMode), -scale);
        }

        private static DecimalX expTaylor(DecimalX x, int scale)
        {
            DecimalX factorial, xPower, sumPrev, sum, term;
            int i;

            factorial = DecimalX.Create(1);
            xPower = x;

            // 1 + x
            sum = x.Add(DecimalX.Create(1));

            // Loop until the sums converge
            // (two successive sums are equal after rounding).
            i = 2;

            do
            {
                // x^i
                xPower = xPower.Multiply(x);
                xPower = DecimalX.Rescale(xPower, -scale, RoundingMode.HalfEven);

                // i!
                factorial = factorial.Multiply(DecimalX.Create(i));

                // x^i/i!
                term = xPower.CDivide(factorial, scale, RoundingMode.HalfEven);

                // sum = sum + x^i/i!
                sumPrev = sum;
                sum = sum.Add(term);

                i++;
            } while (sum.CompareTo(sumPrev) != 0);

            return sum;
        }

        private static DecimalX lnNewton(DecimalX x, int scale)
        {
            DecimalX n, term, tolerance, eToX;
            int sp1;

            sp1 = scale + 1;
            n = x;

            // Convergence tolerance = 5*(10^-(scale+1))
            tolerance = DecimalX.Create(5);
            tolerance = tolerance.MovePointLeft(sp1);

            // Loop until the approximations converge
            // (two successive approximations are within the tolerance).
            do
            {
                // e^x
                eToX = x.Exp(sp1);

                // (e^x - n)/e^x
                term = eToX.Subtract(n).CDivide(eToX, sp1, RoundingMode.Down);

                // x - (e^x - n)/e^x
                x = x.Subtract(term);
            } while (term.CompareTo(tolerance) > 0);

            x = DecimalX.Rescale(x, -scale, RoundingMode.HalfEven);
            return x;
        }
    }
}
