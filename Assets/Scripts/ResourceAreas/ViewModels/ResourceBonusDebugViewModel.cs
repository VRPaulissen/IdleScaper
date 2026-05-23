using System.Collections.Generic;
using ResourceAreas.Runtime;
using Tools.Runtime;

namespace ResourceAreas.ViewModels
{
    /// <summary>
    /// Read model for debugging resolved resource bonuses for one context.
    /// </summary>
    public sealed class ResourceBonusDebugViewModel
    {
        private readonly List<ResourceBonusDebugTotalRow> totals = new List<ResourceBonusDebugTotalRow>();
        private readonly List<ResourceBonusDebugGroup> groups = new List<ResourceBonusDebugGroup>();
        private readonly List<ResourceBonusDebugRow> contributions = new List<ResourceBonusDebugRow>();

        /// <summary>
        /// Creates a resource bonus debug view model.
        /// </summary>
        public ResourceBonusDebugViewModel(
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
        /// Resolved total rows by bonus type.
        /// </summary>
        public IReadOnlyList<ResourceBonusDebugTotalRow> Totals => totals;

        /// <summary>
        /// Contribution rows grouped by source type.
        /// </summary>
        public IReadOnlyList<ResourceBonusDebugGroup> Groups => groups;

        /// <summary>
        /// All resolved contribution rows.
        /// </summary>
        public IReadOnlyList<ResourceBonusDebugRow> Contributions => contributions;

        /// <summary>
        /// Returns true when the view model was built from valid required data.
        /// </summary>
        public bool IsValid { get; }

        /// <summary>
        /// Failure text when the view model is invalid.
        /// </summary>
        public string FailureText { get; }

        /// <summary>
        /// Adds a total row.
        /// </summary>
        public void AddTotal(ResourceBonusDebugTotalRow row)
        {
            if (row == null)
                return;

            totals.Add(row);
        }

        /// <summary>
        /// Adds a group row.
        /// </summary>
        public void AddGroup(ResourceBonusDebugGroup group)
        {
            if (group == null)
                return;

            groups.Add(group);
        }

        /// <summary>
        /// Adds a contribution row.
        /// </summary>
        public void AddContribution(ResourceBonusDebugRow row)
        {
            if (row == null)
                return;

            contributions.Add(row);
        }
    }
}
