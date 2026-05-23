using Tools.Runtime;

namespace ResourceAreas.Runtime
{
    /// <summary>
    /// Describes the current resource gathering context used to resolve bonuses.
    /// </summary>
    public readonly struct ResourceBonusContext
    {
        /// <summary>
        /// Creates a resource bonus context.
        /// </summary>
        public ResourceBonusContext(ResourceAreaId areaId, ResourceId resourceId, ToolId toolId)
        {
            AreaId = areaId;
            ResourceId = resourceId;
            ToolId = toolId;
        }

        /// <summary>
        /// Resource area id for the current context.
        /// </summary>
        public ResourceAreaId AreaId { get; }

        /// <summary>
        /// Resource id for the current context.
        /// </summary>
        public ResourceId ResourceId { get; }

        /// <summary>
        /// Tool id for the current context.
        /// </summary>
        public ToolId ToolId { get; }
    }
}
