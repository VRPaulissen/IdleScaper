namespace ResourceAreas.Definitions
{
    /// <summary>
    /// Category for content unlocked by leveling a resource area.
    /// </summary>
    public enum ResourceAreaUnlockType
    {
        /// <summary>
        /// Unlocks a resource definition.
        /// </summary>
        Resource,

        /// <summary>
        /// Unlocks a passive bonus.
        /// </summary>
        PassiveBonus,

        /// <summary>
        /// Unlocks a feature.
        /// </summary>
        Feature,

        /// <summary>
        /// Unlocks mastery content.
        /// </summary>
        Mastery
    }
}
