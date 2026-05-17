using System.Collections.Generic;
using Tools.Runtime;
using UnityEngine;

namespace Tools.ViewModels
{
    /// <summary>
    /// UI-facing view model for a permanent tool upgrade screen.
    /// </summary>
    public sealed class ToolUpgradeScreenViewModel
    {
        /// <summary>
        /// Permanent tool id shown by this screen.
        /// </summary>
        public ToolId ToolId { get; }

        /// <summary>
        /// Tool display name.
        /// </summary>
        public string ToolDisplayName { get; }

        /// <summary>
        /// Tool icon.
        /// </summary>
        public Sprite ToolIcon { get; }

        /// <summary>
        /// Selected internal slot id.
        /// </summary>
        public ToolPartSlotId SelectedSlotId { get; }

        /// <summary>
        /// Slot view models for this tool.
        /// </summary>
        public IReadOnlyList<ToolSlotViewModel> Slots => slots;

        /// <summary>
        /// Currently selected slot view model.
        /// </summary>
        public ToolSlotViewModel SelectedSlot { get; }

        /// <summary>
        /// Returns true when the screen has valid tool content and state.
        /// </summary>
        public bool IsAvailable { get; }

        /// <summary>
        /// Machine-readable reason when the screen is unavailable.
        /// </summary>
        public ToolUpgradeFailureReason FailureReason { get; }

        /// <summary>
        /// Human-readable reason when the screen is unavailable.
        /// </summary>
        public string FailureText { get; }

        private readonly List<ToolSlotViewModel> slots;

        /// <summary>
        /// Creates a tool upgrade screen view model.
        /// </summary>
        public ToolUpgradeScreenViewModel(
            ToolId toolId,
            string toolDisplayName,
            Sprite toolIcon,
            ToolPartSlotId selectedSlotId,
            List<ToolSlotViewModel> slots,
            ToolSlotViewModel selectedSlot,
            bool isAvailable,
            ToolUpgradeFailureReason failureReason,
            string failureText)
        {
            ToolId = toolId;
            ToolDisplayName = toolDisplayName ?? string.Empty;
            ToolIcon = toolIcon;
            SelectedSlotId = selectedSlotId;
            this.slots = slots ?? new List<ToolSlotViewModel>();
            SelectedSlot = selectedSlot;
            IsAvailable = isAvailable;
            FailureReason = failureReason;
            FailureText = failureText ?? string.Empty;
        }
    }
}
