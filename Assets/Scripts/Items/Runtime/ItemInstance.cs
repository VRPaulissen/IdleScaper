using System;
using UnityEngine;

namespace Items.Runtime
{
    /// <summary>
    /// Runtime state for an item reference: quantity and optional per-item state.
    /// </summary>
    [Serializable]
    public struct ItemInstance
    {
        [SerializeField] private ItemId itemId;
        [SerializeField, Min(1)] private int quantity;

        // Future-proofing:
        // - durabilityCurrent
        // - rolledAffixIds
        // - enchantmentIds
        // - customData blobs/keys

        /// <summary>
        /// The referenced item definition id.
        /// </summary>
        public ItemId ItemId => itemId;

        /// <summary>
        /// Quantity in this instance (for non-stackable items, treat as 1).
        /// </summary>
        public int Quantity => quantity;

        /// <summary>
        /// Creates an item instance.
        /// </summary>
        public ItemInstance(ItemId itemId, int quantity)
        {
            this.itemId = itemId;
            this.quantity = Mathf.Max(1, quantity);
        }

        /// <summary>
        /// Returns a copy with a different quantity.
        /// </summary>
        public ItemInstance WithQuantity(int newQuantity) => new ItemInstance(itemId, Mathf.Max(1, newQuantity));
    }
}