using Items.Runtime;

namespace Tools.Runtime
{
    /// <summary>
    /// Describes an inventory item cost that is missing for a permanent tool part upgrade.
    /// </summary>
    public sealed class MissingToolUpgradeCost
    {
        /// <summary>
        /// Required inventory item id.
        /// </summary>
        public ItemId ItemId { get; }

        /// <summary>
        /// Required item quantity.
        /// </summary>
        public int RequiredQuantity { get; }

        /// <summary>
        /// Available item quantity.
        /// </summary>
        public int AvailableQuantity { get; }

        /// <summary>
        /// Creates a missing upgrade cost entry.
        /// </summary>
        public MissingToolUpgradeCost(ItemId itemId, int requiredQuantity, int availableQuantity)
        {
            ItemId = itemId;
            RequiredQuantity = requiredQuantity;
            AvailableQuantity = availableQuantity;
        }
    }
}
