using System.Collections.Generic;
using Tools.Runtime;
using UnityEngine;

namespace Tools.Definitions
{
    /// <summary>
    /// Static content definition for a permanent player tool.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Tools/Tool Definition", fileName = "Tool_")]
    public sealed class ToolDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private ToolId id;
        [SerializeField] private string displayName;

        [Header("Presentation")]
        [SerializeField] private Sprite icon;

        [Header("Internal Slots")]
        [SerializeField] private List<ToolPartSlotDefinition> supportedSlots = new List<ToolPartSlotDefinition>(5);

        /// <summary>
        /// Stable tool id used by runtime state and compatibility checks.
        /// </summary>
        public ToolId Id => id;

        /// <summary>
        /// Name shown in UI.
        /// </summary>
        public string DisplayName => displayName;

        /// <summary>
        /// Icon used for UI rendering.
        /// </summary>
        public Sprite Icon => icon;

        /// <summary>
        /// Internal part slots supported by this permanent tool.
        /// </summary>
        public IReadOnlyList<ToolPartSlotDefinition> SupportedSlots => supportedSlots;

        /// <summary>
        /// Returns true if this tool supports the given internal part slot.
        /// </summary>
        public bool SupportsSlot(ToolPartSlotId slotId)
        {
            if (!slotId.IsValid)
                return false;

            foreach (var slot in supportedSlots)
            {
                if (slot == null)
                    continue;

                if (slot.Id == slotId)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Gets a supported internal part slot definition by id.
        /// </summary>
        public ToolPartSlotDefinition GetSlot(ToolPartSlotId slotId)
        {
            if (!slotId.IsValid)
                return null;

            foreach (var slot in supportedSlots)
            {
                if (slot == null)
                    continue;

                if (slot.Id == slotId)
                    return slot;
            }

            return null;
        }

        private void OnValidate()
        {
            if (!id.IsValid)
                id = new ToolId(name);
        }
    }
}
