using System;

namespace IdleScaper.Persistence.Core
{
    /// <summary>
    /// A single acquired modifier/card.
    /// It references a definition by id and stores player-specific state (level, equipped, etc.).
    /// </summary>
    [Serializable]
    public struct ModifierInstanceSave
    {
        /// <summary>Stable identifier of the modifier/card definition.</summary>
        public int ModifierId;

        /// <summary>Level/rank of this modifier.</summary>
        public int Level;

        /// <summary>Whether the player has unlocked/acquired it.</summary>
        public bool IsUnlocked;

        /// <summary>Whether it is currently active/equipped.</summary>
        public bool IsEquipped;

        /// <summary>Optional: for systems with slots/loadouts.</summary>
        public int SlotIndex;

        public ModifierInstanceSave(int modifierId, int level, bool isUnlocked, bool isEquipped, int slotIndex)
        {
            ModifierId = modifierId;
            Level = level;
            IsUnlocked = isUnlocked;
            IsEquipped = isEquipped;
            SlotIndex = slotIndex;
        }
    }
}