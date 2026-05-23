namespace ResourceAreas.Definitions
{
    /// <summary>
    /// Category for an item drop from a resource.
    /// </summary>
    public enum ResourceDropCategory
    {
        /// <summary>
        /// Primary item produced by the resource.
        /// </summary>
        MainResource,

        /// <summary>
        /// Common secondary drop.
        /// </summary>
        Common,

        /// <summary>
        /// Rare secondary drop.
        /// </summary>
        Rare,

        /// <summary>
        /// Gem drop.
        /// </summary>
        Gem,

        /// <summary>
        /// Unique drop.
        /// </summary>
        Unique,

        /// <summary>
        /// Fragment drop.
        /// </summary>
        Fragment,

        /// <summary>
        /// Currency drop.
        /// </summary>
        Currency
    }
}
