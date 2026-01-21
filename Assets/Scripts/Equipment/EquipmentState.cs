using System;
using System.Collections.Generic;
using System.Reflection;
using Items.Runtime.Modules;
using UnityEngine;

namespace Equipment
{
    /// <summary>
    /// Serializable equipment state stored as a list for Unity serialization friendliness.
    /// </summary>
    [Serializable]
    public sealed class EquipmentState
    {
        [SerializeField] private List<EquipmentSlotData> slots = new List<EquipmentSlotData>(16);

        /// <summary>
        /// Backing list of slot entries.
        /// </summary>
        public List<EquipmentSlotData> Slots => slots;

        /// <summary>
        /// Ensures the state contains an entry for each <see cref="EquipmentSlotId"/>.
        /// </summary>
        public void EnsureAllSlots()
        {
            var values = (EquipmentSlotId[])Enum.GetValues(typeof(EquipmentSlotId));

            foreach (var slotId in values)
            {
                if (IndexOf(slotId) >= 0)
                    continue;

                slots.Add(new EquipmentSlotData());
                var entry = slots[^1];
                typeof(EquipmentSlotData)
                    .GetField("slot", BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.SetValueDirect(__makeref(entry), slotId);

                slots[^1] = entry;
            }
        }

        /// <summary>
        /// Tries to get the entry index for a slot.
        /// </summary>
        public int IndexOf(Items.Runtime.Modules.EquipmentSlotId slotId)
        {
            for (var i = 0; i < slots.Count; i++)
            {
                if (slots[i].Slot == slotId)
                    return i;
            }

            return -1;
        }
    }
}