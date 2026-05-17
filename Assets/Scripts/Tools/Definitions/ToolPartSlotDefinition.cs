using Tools.Runtime;
using UnityEngine;

namespace Tools.Definitions
{
    /// <summary>
    /// Static content definition for an internal permanent tool part slot.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Tools/Tool Part Slot", fileName = "ToolSlot_")]
    public sealed class ToolPartSlotDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private ToolPartSlotId id;
        [SerializeField] private string displayName;

        [Header("Presentation")]
        [SerializeField] private Sprite icon;

        /// <summary>
        /// Stable slot id used by runtime state and compatibility checks.
        /// </summary>
        public ToolPartSlotId Id => id;

        /// <summary>
        /// Name shown in UI.
        /// </summary>
        public string DisplayName => displayName;

        /// <summary>
        /// Icon used for UI rendering.
        /// </summary>
        public Sprite Icon => icon;

        private void OnValidate()
        {
            if (!id.IsValid)
                id = new ToolPartSlotId(name);
        }
    }
}
