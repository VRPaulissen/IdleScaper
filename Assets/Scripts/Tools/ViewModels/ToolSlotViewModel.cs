using Tools.Runtime;
using UnityEngine;

namespace Tools.ViewModels
{
    /// <summary>
    /// UI-facing view model for one permanent tool internal slot.
    /// </summary>
    public sealed class ToolSlotViewModel
    {
        /// <summary>
        /// Internal slot id.
        /// </summary>
        public ToolPartSlotId SlotId { get; }

        /// <summary>
        /// Slot display name.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Slot icon.
        /// </summary>
        public Sprite Icon { get; }

        /// <summary>
        /// Returns true when this slot is selected.
        /// </summary>
        public bool IsSelected { get; }

        /// <summary>
        /// Installed part view model.
        /// </summary>
        public ToolPartViewModel InstalledPart { get; }

        /// <summary>
        /// Next upgrade preview for this slot.
        /// </summary>
        public ToolUpgradePreviewViewModel UpgradePreview { get; }

        /// <summary>
        /// Creates a tool slot view model.
        /// </summary>
        public ToolSlotViewModel(
            ToolPartSlotId slotId,
            string displayName,
            Sprite icon,
            bool isSelected,
            ToolPartViewModel installedPart,
            ToolUpgradePreviewViewModel upgradePreview)
        {
            SlotId = slotId;
            DisplayName = displayName ?? string.Empty;
            Icon = icon;
            IsSelected = isSelected;
            InstalledPart = installedPart;
            UpgradePreview = upgradePreview;
        }
    }
}
