using System.Collections.Generic;
using ResourceAreas.Runtime;

namespace ResourceAreas.ViewModels
{
    /// <summary>
    /// Groups resolved resource bonus contribution rows by source type.
    /// </summary>
    public sealed class ResourceBonusDebugGroup
    {
        private readonly List<ResourceBonusDebugRow> rows = new List<ResourceBonusDebugRow>();

        /// <summary>
        /// Creates a resource bonus debug group.
        /// </summary>
        public ResourceBonusDebugGroup(ResourceBonusSourceType sourceType, string displayName)
        {
            SourceType = sourceType;
            DisplayName = displayName ?? string.Empty;
        }

        /// <summary>
        /// Source type represented by this group.
        /// </summary>
        public ResourceBonusSourceType SourceType { get; }

        /// <summary>
        /// Human-readable group display name.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Contribution rows in this group.
        /// </summary>
        public IReadOnlyList<ResourceBonusDebugRow> Rows => rows;

        /// <summary>
        /// Adds a contribution row to this group.
        /// </summary>
        public void Add(ResourceBonusDebugRow row)
        {
            if (row == null)
                return;

            rows.Add(row);
        }
    }
}
