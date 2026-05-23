namespace ResourceAreas.Runtime
{
    /// <summary>
    /// Stable identifiers for resource area bonus categories.
    /// </summary>
    public enum ResourceBonusType
    {
        /// <summary>
        /// Additive multiplier bonus for resource yield.
        /// </summary>
        ResourceYieldMultiplier,

        /// <summary>
        /// Additive multiplier bonus for resource experience.
        /// </summary>
        ResourceXpMultiplier,

        /// <summary>
        /// Additive multiplier bonus for resource area experience.
        /// </summary>
        ResourceAreaXpMultiplier,

        /// <summary>
        /// Additive multiplier bonus for mining damage.
        /// </summary>
        MiningDamageMultiplier,

        /// <summary>
        /// Additive multiplier bonus for respawn time.
        /// </summary>
        RespawnTimeMultiplier,

        /// <summary>
        /// Flat bonus for rare drop chance.
        /// </summary>
        RareDropChanceFlat,

        /// <summary>
        /// Flat bonus for gem drop chance.
        /// </summary>
        GemDropChanceFlat,

        /// <summary>
        /// Flat bonus for unique drop chance.
        /// </summary>
        UniqueDropChanceFlat,

        /// <summary>
        /// Flat bonus for a specific drop chance.
        /// </summary>
        SpecificDropChanceFlat,

        /// <summary>
        /// Additive multiplier bonus for resource sell value.
        /// </summary>
        ResourceSellValueMultiplier,

        /// <summary>
        /// Marker bonus indicating a drop unlock key is active.
        /// </summary>
        DropUnlock
    }
}
