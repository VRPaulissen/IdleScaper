using System;
using Items.Runtime;
using Items.Runtime.Modules;
using UnityEngine;

namespace Equipment
{
    /// <summary>
    /// Serializable mapping of a slot to an equipped item.
    /// </summary>
    [Serializable]
    public class EquipmentSlotData
    {
        [SerializeField] private EquipmentSlotId slot;
        [SerializeField] private ItemId itemId;

        /// <summary>
        /// Slot identifier.
        /// </summary>
        public EquipmentSlotId Slot => slot;

        /// <summary>
        /// Equipped item id, or invalid if empty.
        /// </summary>
        public ItemId ItemId => itemId;

        /// <summary>
        /// Sets the equipped item.
        /// </summary>
        public void Set(ItemId newItemId)
        {
            itemId = newItemId;
        }

        /// <summary>
        /// Clears the slot.
        /// </summary>
        public void Clear()
        {
            itemId = default;
        }
    }
}