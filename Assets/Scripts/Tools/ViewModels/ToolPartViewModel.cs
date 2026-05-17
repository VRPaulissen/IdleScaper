using Items.Runtime;
using UnityEngine;

namespace Tools.ViewModels
{
    /// <summary>
    /// UI-facing view model for an installed permanent tool part.
    /// </summary>
    public sealed class ToolPartViewModel
    {
        /// <summary>
        /// Installed part item id.
        /// </summary>
        public ItemId ItemId { get; }

        /// <summary>
        /// Installed part display name.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Installed part icon.
        /// </summary>
        public Sprite Icon { get; }

        /// <summary>
        /// Current installed part level.
        /// </summary>
        public int CurrentLevel { get; }

        /// <summary>
        /// Maximum installed part level.
        /// </summary>
        public int MaxLevel { get; }

        /// <summary>
        /// Returns true when a part is installed.
        /// </summary>
        public bool IsInstalled { get; }

        /// <summary>
        /// Creates an installed part view model.
        /// </summary>
        public ToolPartViewModel(
            ItemId itemId,
            string displayName,
            Sprite icon,
            int currentLevel,
            int maxLevel,
            bool isInstalled)
        {
            ItemId = itemId;
            DisplayName = displayName ?? string.Empty;
            Icon = icon;
            CurrentLevel = currentLevel < 0 ? 0 : currentLevel;
            MaxLevel = maxLevel < 0 ? 0 : maxLevel;
            IsInstalled = isInstalled;
        }
    }
}
