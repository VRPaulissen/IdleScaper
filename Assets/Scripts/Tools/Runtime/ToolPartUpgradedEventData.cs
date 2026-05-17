using Items.Runtime;
using Tools.Definitions;

namespace Tools.Runtime
{
    /// <summary>
    /// Event data for a permanent tool part upgrade.
    /// </summary>
    public sealed class ToolPartUpgradedEventData
    {
        /// <summary>
        /// Permanent tool containing the upgraded part.
        /// </summary>
        public ToolId ToolId { get; }

        /// <summary>
        /// Internal slot containing the upgraded part.
        /// </summary>
        public ToolPartSlotId SlotId { get; }

        /// <summary>
        /// Upgraded installed part item id.
        /// </summary>
        public ItemId PartItemId { get; }

        /// <summary>
        /// Previous part level.
        /// </summary>
        public int FromLevel { get; }

        /// <summary>
        /// New part level.
        /// </summary>
        public int ToLevel { get; }

        /// <summary>
        /// Recipe used for the upgrade.
        /// </summary>
        public ToolUpgradeRecipeDefinition Recipe { get; }

        /// <summary>
        /// Creates upgraded event data.
        /// </summary>
        public ToolPartUpgradedEventData(
            ToolId toolId,
            ToolPartSlotId slotId,
            ItemId partItemId,
            int fromLevel,
            int toLevel,
            ToolUpgradeRecipeDefinition recipe)
        {
            ToolId = toolId;
            SlotId = slotId;
            PartItemId = partItemId;
            FromLevel = fromLevel;
            ToLevel = toLevel;
            Recipe = recipe;
        }
    }
}
