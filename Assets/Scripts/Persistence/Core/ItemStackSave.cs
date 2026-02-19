using System;

namespace IdleScaper.Persistence.Core
{
    /// <summary>
    /// A single inventory stack entry.
    /// </summary>
    [Serializable]
    public struct ItemStackSave
    {
        /// <summary>Stable identifier of the item type (from your item registry).</summary>
        public int ItemId;

        /// <summary>Quantity owned.</summary>
        public long Quantity;

        public ItemStackSave(int itemId, long quantity)
        {
            ItemId = itemId;
            Quantity = quantity;
        }
    }
}