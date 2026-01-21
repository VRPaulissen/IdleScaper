using System;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory
{
    /// <summary>
    /// Serializable container for inventory slots.
    /// </summary>
    [Serializable]
    public sealed class InventoryState
    {
        [SerializeField] private List<InventorySlotData> slots = new List<InventorySlotData>(64);

        /// <summary>
        /// Current slots list (index-based).
        /// </summary>
        public List<InventorySlotData> Slots => slots;

        /// <summary>
        /// Ensures the inventory has exactly the requested slot count.
        /// </summary>
        public void EnsureSize(int slotCount)
        {
            slotCount = Mathf.Max(0, slotCount);

            if (slots.Count == slotCount)
                return;

            if (slots.Count < slotCount)
            {
                while (slots.Count < slotCount)
                    slots.Add(default);
            }
            else
            {
                slots.RemoveRange(slotCount, slots.Count - slotCount);
            }
        }
    }
}