using System.Collections.Generic;
using ResourceAreas.Runtime;
using Tools.Runtime;

namespace ResourceAreas.ViewModels
{
    /// <summary>
    /// Read model for resolved resource drops for one area, resource, and tool context.
    /// </summary>
    public sealed class ResourceDropDebugViewModel
    {
        private readonly List<ResourceDropDebugRow> drops = new List<ResourceDropDebugRow>();

        /// <summary>
        /// Creates a resource drop debug view model.
        /// </summary>
        public ResourceDropDebugViewModel(
            ResourceAreaId areaId,
            string areaName,
            int areaLevel,
            ResourceId resourceId,
            string resourceName,
            int resourceLevel,
            ToolId toolId,
            string toolName,
            bool isValid,
            string failureText)
        {
            AreaId = areaId;
            AreaName = areaName ?? string.Empty;
            AreaLevel = areaLevel;
            ResourceId = resourceId;
            ResourceName = resourceName ?? string.Empty;
            ResourceLevel = resourceLevel;
            ToolId = toolId;
            ToolName = toolName ?? string.Empty;
            IsValid = isValid;
            FailureText = failureText ?? string.Empty;
        }

        /// <summary>
        /// Resource area id used for this debug context.
        /// </summary>
        public ResourceAreaId AreaId { get; }

        /// <summary>
        /// Resource area display name.
        /// </summary>
        public string AreaName { get; }

        /// <summary>
        /// Resource area level used for this debug context.
        /// </summary>
        public int AreaLevel { get; }

        /// <summary>
        /// Resource id used for this debug context.
        /// </summary>
        public ResourceId ResourceId { get; }

        /// <summary>
        /// Resource display name.
        /// </summary>
        public string ResourceName { get; }

        /// <summary>
        /// Resource level used for this debug context.
        /// </summary>
        public int ResourceLevel { get; }

        /// <summary>
        /// Tool id used for this debug context.
        /// </summary>
        public ToolId ToolId { get; }

        /// <summary>
        /// Tool display name.
        /// </summary>
        public string ToolName { get; }

        /// <summary>
        /// Resolved possible drops in drop table order.
        /// </summary>
        public IReadOnlyList<ResourceDropDebugRow> Drops => drops;

        /// <summary>
        /// Returns true when the view model was built from valid required data.
        /// </summary>
        public bool IsValid { get; }

        /// <summary>
        /// Failure text when the view model is invalid.
        /// </summary>
        public string FailureText { get; }

        /// <summary>
        /// Adds a resolved drop row.
        /// </summary>
        public void AddDrop(ResourceDropDebugRow row)
        {
            if (row == null)
                return;

            drops.Add(row);
        }
    }
}
