using System.Globalization;

namespace BigNumberX
{
    /// <summary>
    /// This encapsulates the context settings which describes certain rules for <see cref="DecimalX" /> operations.
    /// </summary>
    public class MathContext
    {
        #region PUBLIC VARIABLES

        /// <summary>
        /// <see cref="CultureInfo" /> used in <see cref="MathContext" /> and <see cref="DecimalX" />.
        /// </summary>
        public static readonly CultureInfo _FCS;

        #endregion

        #region PRIVATE STATIC VARIABLES

        /// <summary>
        /// Temporary Variable to Hold <c>Decimal64 </c><see cref="MathContext" />.
        /// </summary>
        private static readonly MathContext Decimal64X;

        #endregion

        #region FIELD & PROPERTIES
        /// <summary>
        /// The number of digits to be used.  (0 = unlimited)
        /// </summary>
        public uint Precision { get; }

        /// <summary>
        /// The rounding algorithm (mode) to be used.
        /// </summary>
        public RoundingMode RoundingMode { get; }

        /// <summary>
        /// A <see cref="MathContext" /> with a precision setting of Precision= 9 digits, RoundingMode= <see  cref="HalfUp" />
        /// </summary>
        private static MathContext BASIC_DEFAULT { get; }

        /// <summary>
        /// A <see cref="MathContext" /> with a precision setting matching the IEEE 754R
        /// Decimal64 format, 16 digits, and a rounding mode of <see cref="HalfEven" /> the IEEE
        /// 754R default.
        /// </summary>
        private static MathContext Decimal64 { get; }

        /// <summary>
        /// A <see cref="MathContext" /> with a precision setting matching the IEEE 754R
        /// Decimal128 format, 34 digits, and a rounding mode of <see cref="HalfEven" /> the IEEE
        /// 754R default.
        /// </summary>
        private static MathContext Decimal128 { get; }

        /// <summary>
        /// A <see cref="MathContext" /> whose settings have the values required for
        /// unlimited precision arithmetic.
        /// The values of the settings are: Precision=0 RoundingMode= <see cref="HalfUp" />
        /// </summary>
        private static MathContext Unlimited { get; }

        #endregion

        #region CONSTRUCTORS

        public MathContext(uint precision, RoundingMode mode)
        {
            Precision = precision;
            RoundingMode = mode;
        }

        public MathContext(uint precision)
        {
            Precision = precision;
        }

        #endregion

        #region STATIC CONSTRUCTOR
        static MathContext()
        {
            _FCS = CultureInfo.CurrentCulture;
            BASIC_DEFAULT = new MathContext(9, RoundingMode.HalfUp);
            Decimal64 = new MathContext(7, RoundingMode.HalfEven);
            Decimal64X = new MathContext(16, RoundingMode.HalfEven);
            Decimal128 = new MathContext(34, RoundingMode.HalfEven);
            Unlimited = new MathContext(0, RoundingMode.HalfUp);
        } 
        #endregion

        #region OPERATOR OVERLOADS
        public static bool operator ==(MathContext c1, MathContext c2) => c1 != null && c1.Equals(c2);

        public static bool operator !=(MathContext c1, MathContext c2) => !(c1 == c2); 
        #endregion

        /// <summary>
        /// A custom function to create a <see cref="MathContext" /> by supplying
        /// only a Precision. It uses an already defined RoundingMode= <see cref="HalfEven" />
        /// </summary>
        /// <param name="precision">
        /// Precision to Use
        /// </param>
        /// <returns>A <see cref="MathContext" /> with specified parameters.</returns>
        public static MathContext ExtendedDefault(uint precision)
        {
            return new MathContext(precision, RoundingMode.HalfEven);
        }

        public bool Equals(MathContext other)
        {
            return other.Precision == Precision && other.RoundingMode == RoundingMode;
        }

        public override string ToString()
        {
            return string.Format(_FCS, "Precision = {0} RoundingMode = {1}", Precision, RoundingMode.ToString());
        }

        public bool RoundingNeeded(IntegerX bi) => true;

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            var mth = obj as MathContext;
            return mth != null && Equals(mth);
        }

        public override int GetHashCode() => base.GetHashCode();
    }
}
