namespace Tools.Runtime
{
    /// <summary>
    /// Stable identifiers for permanent tool bonus categories.
    /// </summary>
    public enum ToolBonusType
    {
        /// <summary>
        /// Flat mining damage added after multiplier calculations.
        /// </summary>
        MiningDamageFlat = 0,

        /// <summary>
        /// Additive mining damage multiplier bonus.
        /// </summary>
        MiningDamageMultiplier = 1,

        /// <summary>
        /// Additive mining speed multiplier bonus.
        /// </summary>
        MiningSpeedMultiplier = 2,

        /// <summary>
        /// Additive resource yield multiplier bonus.
        /// </summary>
        ResourceYieldMultiplier = 3,

        /// <summary>
        /// Flat rare drop chance bonus.
        /// </summary>
        RareDropChanceFlat = 4
    }
}
