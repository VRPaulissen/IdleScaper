using System.Collections.Generic;
using Items.Runtime;
using Tools.Runtime;
using UnityEngine;

namespace Tools.Definitions
{
    /// <summary>
    /// Static content definition for upgrading an installed permanent tool part from one level to another.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Tools/Upgrade Recipe", fileName = "ToolUpgrade_")]
    public sealed class ToolUpgradeRecipeDefinition : ScriptableObject
    {
        [Header("Target")]
        [SerializeField] private ToolId toolId;
        [SerializeField] private ToolPartSlotId slotId;
        [SerializeField] private ItemId partItemId;

        [Header("Levels")]
        [SerializeField, Min(0)] private int fromLevel;
        [SerializeField, Min(1)] private int toLevel = 1;

        [Header("Costs")]
        [SerializeField] private List<ToolUpgradeCost> itemCosts = new List<ToolUpgradeCost>();

        /// <summary>
        /// Permanent tool this recipe applies to.
        /// </summary>
        public ToolId ToolId => toolId;

        /// <summary>
        /// Internal part slot this recipe applies to.
        /// </summary>
        public ToolPartSlotId SlotId => slotId;

        /// <summary>
        /// Installed part item this recipe upgrades.
        /// </summary>
        public ItemId PartItemId => partItemId;

        /// <summary>
        /// Required current level before this recipe can be applied.
        /// </summary>
        public int FromLevel => fromLevel;

        /// <summary>
        /// Resulting level after this recipe is applied.
        /// </summary>
        public int ToLevel => toLevel;

        /// <summary>
        /// Inventory item costs required by this recipe.
        /// </summary>
        public IReadOnlyList<ToolUpgradeCost> ItemCosts => itemCosts;

        /// <summary>
        /// Returns true when this recipe targets the given installed part state.
        /// </summary>
        public bool Matches(ToolId toolId, ToolPartSlotId slotId, ItemId partItemId, int currentLevel)
        {
            if (!toolId.IsValid)
                return false;

            if (!slotId.IsValid)
                return false;

            if (!partItemId.IsValid)
                return false;

            return this.toolId == toolId
                   && this.slotId == slotId
                   && this.partItemId == partItemId
                   && fromLevel == currentLevel;
        }

        /// <summary>
        /// Returns true when the recipe has valid ids and level progression.
        /// </summary>
        public bool IsValid()
        {
            if (!toolId.IsValid)
                return false;

            if (!slotId.IsValid)
                return false;

            if (!partItemId.IsValid)
                return false;

            return toLevel > fromLevel;
        }

        private void OnValidate()
        {
            if (fromLevel < 0)
                fromLevel = 0;

            if (toLevel <= fromLevel)
                toLevel = fromLevel + 1;
        }
    }
}
