namespace ResourceAreas.Runtime
{
    /// <summary>
    /// Identifies where a resource bonus contribution came from.
    /// </summary>
    public enum ResourceBonusSourceType
    {
        /// <summary>
        /// Bonus applies globally.
        /// </summary>
        Global,

        /// <summary>
        /// Bonus comes from a resource area.
        /// </summary>
        ResourceArea,

        /// <summary>
        /// Bonus comes from a resource.
        /// </summary>
        Resource,

        /// <summary>
        /// Bonus comes from a tool.
        /// </summary>
        Tool,

        /// <summary>
        /// Bonus comes from a temporary effect.
        /// </summary>
        Temporary,

        /// <summary>
        /// Bonus comes from equipment.
        /// </summary>
        Equipment,

        /// <summary>
        /// Bonus comes from an event.
        /// </summary>
        Event
    }
}
