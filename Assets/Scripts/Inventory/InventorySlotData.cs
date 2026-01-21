using System;
using Items.Runtime;
using UnityEngine;

namespace Inventory
{
    /// <summary>
    /// Serializable inventory slot data. Empty slots have an invalid id or quantity 0.
    /// </summary>
    [Serializable]
    public struct InventorySlotData
    {
        [SerializeField] private ItemId itemId;
        [SerializeField, Min(0)] private int quantity;

        /// <summary>
        /// Item id stored in this slot. Invalid means empty.
        /// </summary>
        public ItemId ItemId => itemId;

        /// <summary>
        /// Quantity stored in this slot. 0 means empty.
        /// </summary>
        public int Quantity => quantity;

        /// <summary>
        /// Returns true if this slot contains an item.
        /// </summary>
        public bool HasItem => itemId.IsValid && quantity > 0;

        /// <summary>
        /// Clears this slot.
        /// </summary>
        public void Clear()
        {
            itemId = default;
            quantity = 0;
        }

        /// <summary>
        /// Sets the slot content.
        /// </summary>
        public void Set(ItemId newItemId, int newQuantity)
        {
            itemId = newItemId;
            quantity = Mathf.Max(0, newQuantity);

            if (!itemId.IsValid || quantity <= 0)
                Clear();
        }
    }
}