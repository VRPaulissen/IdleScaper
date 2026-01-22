namespace Player
{
    /// <summary>
    /// Indicates what kind of gathering hit occurred.
    /// </summary>
    public enum GatheringHitType
    {
        /// <summary>
        /// No critical modifiers applied.
        /// </summary>
        Normal = 0,

        /// <summary>
        /// Critical hit modifier applied.
        /// </summary>
        Crit = 1,

        /// <summary>
        /// Ultra critical hit modifier applied.
        /// </summary>
        UltraCrit = 2,
    }
}