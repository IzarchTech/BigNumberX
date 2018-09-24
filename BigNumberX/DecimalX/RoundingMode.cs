namespace BigNumberX
{
    /// <summary>
    /// Indicates the rounding algorithm (behavior) to use for <see cref="DecimalX" /> operations.
    /// </summary>
    /// <remarks> The round-05up algorithm mentioned in GDAS have not been implemented.</remarks>
    public enum RoundingMode
    {
        /// <summary>
        /// Round away from 0.
        /// </summary>
        Up,
        /// <summary>
        /// Truncate (round toward 0).
        /// </summary>
        Down,
        /// <summary>
        /// Round toward positive infinity.
        /// </summary>
        Ceiling,
        /// <summary>
        /// Round toward negative infinity.
        /// </summary>
        Floor,
        /// <summary>
        /// Round to nearest neighbor, round up if equidistant.
        /// </summary>
        HalfUp,
        /// <summary>
        /// Round to nearest neighbor, round down if equidistant.
        /// </summary>
        HalfDown,
        /// <summary>
        /// Round to nearest neighbor, round to even neighbor if equidistant.
        /// </summary>
        HalfEven,
        /// <summary>
        /// Do not do any rounding.
        /// </summary>
        Unnecessary
    }
}
