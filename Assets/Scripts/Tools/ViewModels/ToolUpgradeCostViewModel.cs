using Items.Runtime;
using UnityEngine;

namespace Tools.ViewModels
{
    /// <summary>
    /// UI-facing view model for one permanent tool upgrade cost.
    /// </summary>
    public sealed class ToolUpgradeCostViewModel
    {
        /// <summary>
        /// Required item id.
        /// </summary>
        public ItemId ItemId { get; }

        /// <summary>
        /// Required item display name.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Required item icon.
        /// </summary>
        public Sprite Icon { get; }

        /// <summary>
        /// Required quantity.
        /// </summary>
        public int RequiredAmount { get; }

        /// <summary>
        /// Quantity currently owned by the player.
        /// </summary>
        public int OwnedAmount { get; }

        /// <summary>
        /// Returns true when the player owns enough of this cost.
        /// </summary>
        public bool IsFulfilled { get; }

        /// <summary>
        /// Creates a cost view model.
        /// </summary>
        public ToolUpgradeCostViewModel(
            ItemId itemId,
            string displayName,
            Sprite icon,
            int requiredAmount,
            int ownedAmount)
        {
            ItemId = itemId;
            DisplayName = displayName ?? string.Empty;
            Icon = icon;
            RequiredAmount = requiredAmount < 0 ? 0 : requiredAmount;
            OwnedAmount = ownedAmount < 0 ? 0 : ownedAmount;
            IsFulfilled = OwnedAmount >= RequiredAmount;
        }
    }
}
