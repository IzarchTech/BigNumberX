using System.Globalization;

namespace BigNumberX
{
    /// <summary>
    /// This encapsulates the context settings which describes certain rules for <see cref="DecimalX" /> operations.
    /// </summary>
    public class MathContext
    {
        #region PUBLIC VARIABLES
        
        public uint _precision;

        public RoundingMode _roundingMode;

        /// <summary>
        /// <see cref="CultureInfo" /> used in <see cref="MathContext" /> and <see cref="DecimalX" />.
        /// </summary>
        public static CultureInfo _FCS;

        #endregion

        #region PRIVATE STATIC VARIABLES
        /// <summary>
        /// Temporary Variable to Hold <c>BASIC_DEFAULT </c><see cref="MathContext" />.
        /// </summary>
        private static MathContext BASIC_DEFAULTX;

        /// <summary>
        /// Temporary Variable to Hold <c>Decimal32 </c><see cref="MathContext" />.
        /// </summary>
        private static MathContext Decimal32X;

        /// <summary>
        /// Temporary Variable to Hold <c>Decimal64 </c><see cref="MathContext" />.
        /// </summary>
        private static MathContext Decimal64X;

        /// <summary>
        /// Temporary Variable to Hold <c>Decimal128 </c><see cref="MathContext" />.
        /// </summary>
        private static MathContext Decimal128X;

        /// <summary>
        /// Temporary Variable to Hold <c>Unlimited </c><see cref="MathContext" />.
        /// </summary>
        private static MathContext UnlimitedX;
        #endregion

        #region FIELD & PROPERTIES
        /// <summary>
        /// The number of digits to be used.  (0 = unlimited)
        /// </summary>
        public uint Precision
        {
            get
            {
                return _precision;
            }
        }

        /// <summary>
        /// The rounding algorithm (mode) to be used.
        /// </summary>
        public RoundingMode RoundingMode
        {
            get
            {
                return _roundingMode;
            }
        }

        /// <summary>
        /// A <see cref="MathContext" /> with a precision setting of Precision= 9 digits, RoundingMode= <see  cref="HalfUp" />
        /// </summary>
        public static MathContext BASIC_DEFAULT
        {
            get
            {
                return BASIC_DEFAULTX;
            }
        }

        /// <summary>
        /// A <see cref="MathContext" /> with a precision setting matching the IEEE 754R
        /// Decimal32 format, 7 digits, and a rounding mode of <see cref="HalfEven" /> the IEEE
        /// 754R default.
        /// </summary>
        public static MathContext Decimal32
        {
            get
            {
                return Decimal32X;
            }
        }

        /// <summary>
        /// A <see cref="MathContext" /> with a precision setting matching the IEEE 754R
        /// Decimal64 format, 16 digits, and a rounding mode of <see cref="HalfEven" /> the IEEE
        /// 754R default.
        /// </summary>
        public static MathContext Decimal64
        {
            get
            {
                return Decimal32X;
            }
        }

        /// <summary>
        /// A <see cref="MathContext" /> with a precision setting matching the IEEE 754R
        /// Decimal128 format, 34 digits, and a rounding mode of <see cref="HalfEven" /> the IEEE
        /// 754R default.
        /// </summary>
        public static MathContext Decimal128
        {
            get
            {
                return Decimal128X;
            }
        }

        /// <summary>
        /// A <see cref="MathContext" /> whose settings have the values required for
        /// unlimited precision arithmetic.
        /// The values of the settings are: Precision=0 RoundingMode= <see cref="HalfUp" />
        /// </summary>
        public static MathContext Unlimited
        {
            get
            {
                return UnlimitedX;
            }
        }
        #endregion

        #region CONSTRUCTORS

        public MathContext(uint Precision, RoundingMode mode)
        {
            _precision = Precision;
            _roundingMode = mode;
        }

        public MathContext(uint Precision)
        {
            _precision = Precision;
        }

        #endregion

        #region STATIC CONSTRUCTOR
        static MathContext()
        {
            _FCS = CultureInfo.CurrentCulture;
            BASIC_DEFAULTX = new MathContext(9, RoundingMode.HalfUp);
            Decimal32X = new MathContext(7, RoundingMode.HalfEven);
            Decimal64X = new MathContext(16, RoundingMode.HalfEven);
            Decimal128X = new MathContext(34, RoundingMode.HalfEven);
            UnlimitedX = new MathContext(0, RoundingMode.HalfUp);
        } 
        #endregion

        #region OPERATOR OVERLOADS
        public static bool operator ==(MathContext c1, MathContext c2) => c1.Equals(c2);

        public static bool operator !=(MathContext c1, MathContext c2) => !(c1 == c2); 
        #endregion

        /// <summary>
        /// A custom function to create a <see cref="MathContext" /> by supplying
        /// only a Precision. It uses an already defined RoundingMode= <see cref="HalfEven" />
        /// </summary>
        /// <param name="Precision">
        /// Precision to Use
        /// </param>
        /// <returns>A <see cref="MathContext" /> with specified parameters.</returns>
        public static MathContext ExtendedDefault(uint Precision)
        {
            return new MathContext(Precision, RoundingMode.HalfEven);
        }

        public bool Equals(MathContext other)
        {
            return other._precision == _precision && other._roundingMode == _roundingMode;
        }

        public override string ToString()
        {
            return string.Format(_FCS, "Precision = {0} RoundingMode = {1}", _precision, _roundingMode.ToString());
        }

        public bool RoundingNeeded(IntegerX bi) => true;

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            MathContext mth = obj as MathContext;
            if (mth == null)
                return false;
            return Equals(mth);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
