using System;
using System.Collections.Generic;
using Items.Runtime;
using Tools.Runtime;
using UnityEngine;
using ToolLogger = Utilities.Logging.Logger;

namespace Tools.State
{
    /// <summary>
    /// Serializable runtime state for one permanent tool preset.
    /// </summary>
    [Serializable]
    public sealed class ToolPresetState
    {
        [SerializeField, Min(0)] private int presetIndex;
        [SerializeField] private List<ToolPartSlotState> slots = new List<ToolPartSlotState>(5);

        /// <summary>
        /// Index of this preset within its tool.
        /// </summary>
        public int PresetIndex => presetIndex;

        /// <summary>
        /// Installed part slots for this preset.
        /// </summary>
        public List<ToolPartSlotState> Slots => slots;

        /// <summary>
        /// Creates a default preset state.
        /// </summary>
        public ToolPresetState()
        {
        }

        /// <summary>
        /// Creates a preset state with the given index.
        /// </summary>
        public ToolPresetState(int presetIndex)
        {
            this.presetIndex = Math.Max(0, presetIndex);
        }

        /// <summary>
        /// Normalizes this preset after loading save data.
        /// </summary>
        public void Normalize(ItemDatabase itemDatabase = null)
        {
            if (presetIndex < 0)
            {
                ToolLogger.LogWarning("Tool preset had a negative index and was reset to 0.");
                presetIndex = 0;
            }

            slots ??= new List<ToolPartSlotState>(5);

            for (var i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];
                if (slot == null)
                    continue;

                slot.Normalize(itemDatabase);
            }
        }

        /// <summary>
        /// Ensures this preset contains the default Pickaxe part slots.
        /// </summary>
        public void EnsurePickaxeSlots()
        {
            EnsureSlot(ToolPartSlotId.Head);
            EnsureSlot(ToolPartSlotId.Handle);
            EnsureSlot(ToolPartSlotId.Rope);
            EnsureSlot(ToolPartSlotId.Grip);
            EnsureSlot(ToolPartSlotId.Coating);
        }

        /// <summary>
        /// Ensures this preset contains the given slot.
        /// </summary>
        public ToolPartSlotState EnsureSlot(ToolPartSlotId slotId)
        {
            var existing = GetSlot(slotId);
            if (existing != null)
                return existing;

            var slot = new ToolPartSlotState(slotId);
            slots.Add(slot);
            return slot;
        }

        /// <summary>
        /// Gets the slot state for the given slot id.
        /// </summary>
        public ToolPartSlotState GetSlot(ToolPartSlotId slotId)
        {
            if (!slotId.IsValid)
                return null;

            for (var i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];
                if (slot == null)
                    continue;

                if (slot.SlotId == slotId)
                    return slot;
            }

            return null;
        }
    }
}
