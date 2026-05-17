using System;
using Items.Runtime;
using UnityEngine;

namespace Tools.Definitions
{
    /// <summary>
    /// Data-only item cost required by a permanent tool upgrade recipe.
    /// </summary>
    [Serializable]
    public struct ToolUpgradeCost
    {
        [SerializeField] private ItemId itemId;
        [SerializeField, Min(1)] private int quantity;

        /// <summary>
        /// Stable id of the required inventory item.
        /// </summary>
        public ItemId ItemId => itemId;

        /// <summary>
        /// Required quantity of the inventory item.
        /// </summary>
        public int Quantity => Math.Max(1, quantity);

        /// <summary>
        /// Returns true when this cost has a valid item and positive quantity.
        /// </summary>
        public bool IsValid => itemId.IsValid && quantity > 0;

        /// <summary>
        /// Creates an item cost for a permanent tool upgrade recipe.
        /// </summary>
        public ToolUpgradeCost(ItemId itemId, int quantity)
        {
            this.itemId = itemId;
            this.quantity = Math.Max(1, quantity);
        }
    }
}
