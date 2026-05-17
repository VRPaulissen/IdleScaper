using System;
using Items.Runtime;
using Items.Runtime.Modules;
using Tools.Runtime;
using UnityEngine;
using ToolLogger = Utilities.Logging.Logger;

namespace Tools.State
{
    /// <summary>
    /// Serializable runtime state for one internal tool part slot.
    /// </summary>
    [Serializable]
    public sealed class ToolPartSlotState
    {
        [SerializeField] private ToolPartSlotId slotId;
        [SerializeField] private ItemId installedPartItemId;
        [SerializeField, Min(0)] private int partLevel;

        /// <summary>
        /// Slot this state belongs to.
        /// </summary>
        public ToolPartSlotId SlotId => slotId;

        /// <summary>
        /// Installed part item id, or invalid if no part is installed.
        /// </summary>
        public ItemId InstalledPartItemId => installedPartItemId;

        /// <summary>
        /// Current level of the installed part.
        /// </summary>
        public int PartLevel => partLevel;

        /// <summary>
        /// Returns true when this slot has an installed part.
        /// </summary>
        public bool HasInstalledPart => installedPartItemId.IsValid;

        /// <summary>
        /// Creates an empty slot state.
        /// </summary>
        public ToolPartSlotState()
        {
        }

        /// <summary>
        /// Creates a slot state for the given slot.
        /// </summary>
        public ToolPartSlotState(ToolPartSlotId slotId)
        {
            this.slotId = slotId;
        }

        /// <summary>
        /// Normalizes this slot after loading save data.
        /// </summary>
        public void Normalize(ItemDatabase itemDatabase = null)
        {
            if (partLevel < 0)
            {
                ToolLogger.LogWarning($"Tool part slot '{slotId}' had a negative level and was reset to 0.");
                partLevel = 0;
            }

            if (!installedPartItemId.IsValid)
            {
                partLevel = 0;
                return;
            }

            if (itemDatabase == null)
                return;

            if (itemDatabase.TryGet(installedPartItemId, out _))
            {
                NormalizeLevelAgainstDefinition(itemDatabase);
                return;
            }

            ToolLogger.LogWarning($"Installed tool part '{installedPartItemId}' no longer exists and was removed from slot '{slotId}'.");
            ClearInstalledPart();
        }

        /// <summary>
        /// Installs a part item and sets its level.
        /// </summary>
        public void SetInstalledPart(ItemId itemId, int level)
        {
            installedPartItemId = itemId;
            partLevel = itemId.IsValid ? Math.Max(0, level) : 0;
        }

        /// <summary>
        /// Updates the installed part level.
        /// </summary>
        public void SetPartLevel(int level)
        {
            partLevel = installedPartItemId.IsValid ? Math.Max(0, level) : 0;
        }

        /// <summary>
        /// Clears the installed part from this slot.
        /// </summary>
        public void ClearInstalledPart()
        {
            installedPartItemId = default;
            partLevel = 0;
        }

        private void NormalizeLevelAgainstDefinition(ItemDatabase itemDatabase)
        {
            if (!itemDatabase.TryGet(installedPartItemId, out var definition) || definition == null)
                return;

            if (!definition.TryGetModule<ToolPartModule>(out var module) || module == null)
                return;

            if (partLevel <= module.MaxLevel)
                return;

            ToolLogger.LogWarning($"Tool part '{installedPartItemId}' level {partLevel} exceeded max {module.MaxLevel} and was clamped.");
            partLevel = module.MaxLevel;
        }
    }
}
