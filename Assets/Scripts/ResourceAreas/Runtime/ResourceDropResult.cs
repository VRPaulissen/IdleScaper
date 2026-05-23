using Items.Runtime;
using ResourceAreas.Definitions;

namespace ResourceAreas.Runtime
{
    /// <summary>
    /// Represents one successful rolled resource drop.
    /// </summary>
    public readonly struct ResourceDropResult
    {
        /// <summary>
        /// Creates a successful resource drop result.
        /// </summary>
        public ResourceDropResult(ItemId itemId, string displayName, ResourceDropCategory category, int amount, float chanceUsed)
        {
            ItemId = itemId;
            DisplayName = displayName ?? string.Empty;
            Category = category;
            Amount = amount;
            ChanceUsed = chanceUsed;
        }

        /// <summary>
        /// Item id produced by this drop.
        /// </summary>
        public ItemId ItemId { get; }

        /// <summary>
        /// Display name for this drop.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Drop category.
        /// </summary>
        public ResourceDropCategory Category { get; }

        /// <summary>
        /// Rolled amount.
        /// </summary>
        public int Amount { get; }

        /// <summary>
        /// Final chance used for this roll.
        /// </summary>
        public float ChanceUsed { get; }
    }
}
