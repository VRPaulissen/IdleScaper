using System;
using System.Collections.Generic;
using Inventory;
using Items.Definitions;
using Items.Runtime;
using Items.Runtime.Modules;
using Tools.Definitions;
using Tools.Runtime;
using Tools.State;

namespace Tools.ViewModels
{
    /// <summary>
    /// Builds UI-facing view models for permanent tool upgrade screens.
    /// </summary>
    public sealed class ToolUpgradeScreenViewModelBuilder
    {
        private readonly ToolCollectionState tools;
        private readonly ToolDefinitionCatalog toolDefinitions;
        private readonly ItemDatabase itemDatabase;
        private readonly ToolUpgradeRecipeCatalog recipeCatalog;
        private readonly IInventoryService inventory;

        /// <summary>
        /// Creates a permanent tool upgrade screen view model builder.
        /// </summary>
        public ToolUpgradeScreenViewModelBuilder(
            ToolCollectionState tools,
            ToolDefinitionCatalog toolDefinitions,
            ItemDatabase itemDatabase,
            ToolUpgradeRecipeCatalog recipeCatalog,
            IInventoryService inventory)
        {
            this.tools = tools ?? throw new ArgumentNullException(nameof(tools));
            this.toolDefinitions = toolDefinitions ?? throw new ArgumentNullException(nameof(toolDefinitions));
            this.itemDatabase = itemDatabase ?? throw new ArgumentNullException(nameof(itemDatabase));
            this.recipeCatalog = recipeCatalog ?? throw new ArgumentNullException(nameof(recipeCatalog));
            this.inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
        }

        /// <summary>
        /// Builds a view model for the given permanent tool and selected slot.
        /// </summary>
        public ToolUpgradeScreenViewModel Build(ToolId toolId, ToolPartSlotId selectedSlotId)
        {
            if (!toolId.IsValid)
                return BuildUnavailable(toolId, selectedSlotId, ToolUpgradeFailureReason.InvalidTool, "Invalid tool id.");

            if (!toolDefinitions.TryGet(toolId, out var toolDefinition) || toolDefinition == null)
                return BuildUnavailable(toolId, selectedSlotId, ToolUpgradeFailureReason.InvalidTool, "Tool definition was not found.");

            var toolState = tools.GetTool(toolId);
            if (toolState == null)
                return BuildUnavailable(toolId, selectedSlotId, ToolUpgradeFailureReason.InvalidTool, "Tool state was not found.");

            var preset = toolState.GetPreset(toolState.ActivePresetIndex);
            if (preset == null)
                return BuildUnavailable(toolId, selectedSlotId, ToolUpgradeFailureReason.InvalidPreset, "Active tool preset was not found.");

            var effectiveSelectedSlotId = ResolveSelectedSlot(toolDefinition, selectedSlotId);
            var slots = BuildSlots(toolDefinition, preset, effectiveSelectedSlotId);
            var selectedSlot = FindSelectedSlot(slots);

            return new ToolUpgradeScreenViewModel(
                toolId,
                toolDefinition.DisplayName,
                toolDefinition.Icon,
                effectiveSelectedSlotId,
                slots,
                selectedSlot,
                true,
                ToolUpgradeFailureReason.Success,
                string.Empty);
        }

        private ToolUpgradeScreenViewModel BuildUnavailable(
            ToolId toolId,
            ToolPartSlotId selectedSlotId,
            ToolUpgradeFailureReason reason,
            string failureText)
        {
            return new ToolUpgradeScreenViewModel(
                toolId,
                string.Empty,
                null,
                selectedSlotId,
                new List<ToolSlotViewModel>(),
                null,
                false,
                reason,
                failureText);
        }

        private static ToolPartSlotId ResolveSelectedSlot(ToolDefinition toolDefinition, ToolPartSlotId selectedSlotId)
        {
            if (selectedSlotId.IsValid && toolDefinition.SupportsSlot(selectedSlotId))
                return selectedSlotId;

            var slots = toolDefinition.SupportedSlots;
            for (var i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];
                if (slot == null)
                    continue;

                return slot.Id;
            }

            return default;
        }

        private List<ToolSlotViewModel> BuildSlots(
            ToolDefinition toolDefinition,
            ToolPresetState preset,
            ToolPartSlotId selectedSlotId)
        {
            var slots = new List<ToolSlotViewModel>();
            var slotDefinitions = toolDefinition.SupportedSlots;

            for (var i = 0; i < slotDefinitions.Count; i++)
            {
                var slotDefinition = slotDefinitions[i];
                if (slotDefinition == null)
                    continue;

                var slotState = preset.GetSlot(slotDefinition.Id);
                var isSelected = slotDefinition.Id == selectedSlotId;
                var part = BuildPart(slotState);
                var preview = BuildPreview(toolDefinition.Id, slotDefinition.Id, slotState, part);

                slots.Add(new ToolSlotViewModel(
                    slotDefinition.Id,
                    slotDefinition.DisplayName,
                    slotDefinition.Icon,
                    isSelected,
                    part,
                    preview));
            }

            return slots;
        }

        private ToolPartViewModel BuildPart(ToolPartSlotState slotState)
        {
            if (slotState == null || !slotState.HasInstalledPart)
                return new ToolPartViewModel(default, "Empty", null, 0, 0, false);

            var partItemId = slotState.InstalledPartItemId;
            if (!itemDatabase.TryGetItem(partItemId, out var itemDefinition) || itemDefinition == null)
                return new ToolPartViewModel(partItemId, "Missing Item", null, slotState.PartLevel, 0, true);

            var maxLevel = GetMaxLevel(itemDefinition);
            return new ToolPartViewModel(
                partItemId,
                itemDefinition.DisplayName,
                itemDefinition.Icon,
                slotState.PartLevel,
                maxLevel,
                true);
        }

        private ToolUpgradePreviewViewModel BuildPreview(
            ToolId toolId,
            ToolPartSlotId slotId,
            ToolPartSlotState slotState,
            ToolPartViewModel part)
        {
            if (slotState == null)
                return BuildUnavailablePreview(0, 0, ToolUpgradeFailureReason.InvalidSlot, "Slot state was not found.");

            if (!slotState.HasInstalledPart)
                return BuildUnavailablePreview(0, 0, ToolUpgradeFailureReason.EmptySlot, "No part installed.");

            if (!itemDatabase.TryGetItem(slotState.InstalledPartItemId, out var itemDefinition) || itemDefinition == null)
                return BuildUnavailablePreview(slotState.PartLevel, slotState.PartLevel, ToolUpgradeFailureReason.ItemDefinitionNotFound, "Installed item definition was not found.");

            if (!itemDefinition.TryGetModule<ToolPartModule>(out var toolPartModule) || toolPartModule == null)
                return BuildUnavailablePreview(slotState.PartLevel, slotState.PartLevel, ToolUpgradeFailureReason.NotToolPart, "Installed item is not a tool part.");

            if (!toolPartModule.IsCompatibleWith(toolId, slotId))
                return BuildCompatibilityFailure(toolPartModule, toolId, slotId, slotState.PartLevel);

            if (slotState.PartLevel >= toolPartModule.MaxLevel)
                return BuildUnavailablePreview(slotState.PartLevel, slotState.PartLevel, ToolUpgradeFailureReason.MaxLevelReached, "Part is at max level.");

            var targetLevel = slotState.PartLevel + 1;
            if (!recipeCatalog.TryGetRecipe(toolId, slotId, slotState.InstalledPartItemId, slotState.PartLevel, targetLevel, out var recipe) || recipe == null)
                return BuildUnavailablePreview(slotState.PartLevel, targetLevel, ToolUpgradeFailureReason.RecipeNotFound, "No upgrade recipe found.");

            var costs = BuildCosts(recipe);
            var reason = AreCostsFulfilled(costs) ? ToolUpgradeFailureReason.Success : ToolUpgradeFailureReason.MissingCost;
            var text = reason == ToolUpgradeFailureReason.Success ? string.Empty : "Missing required items.";

            return new ToolUpgradePreviewViewModel(
                recipe.FromLevel,
                recipe.ToLevel,
                costs,
                true,
                reason,
                text);
        }

        private static ToolUpgradePreviewViewModel BuildUnavailablePreview(
            int fromLevel,
            int toLevel,
            ToolUpgradeFailureReason reason,
            string failureText)
        {
            return new ToolUpgradePreviewViewModel(
                fromLevel,
                toLevel,
                new List<ToolUpgradeCostViewModel>(),
                false,
                reason,
                failureText);
        }

        private static ToolUpgradePreviewViewModel BuildCompatibilityFailure(
            ToolPartModule toolPartModule,
            ToolId toolId,
            ToolPartSlotId slotId,
            int currentLevel)
        {
            var reason = toolPartModule.CompatibleToolId != toolId
                ? ToolUpgradeFailureReason.PartNotCompatibleWithTool
                : ToolUpgradeFailureReason.PartNotCompatibleWithSlot;

            var text = reason == ToolUpgradeFailureReason.PartNotCompatibleWithTool
                ? "Installed part is not compatible with this tool."
                : "Installed part is not compatible with this slot.";

            return BuildUnavailablePreview(currentLevel, currentLevel, reason, text);
        }

        private List<ToolUpgradeCostViewModel> BuildCosts(ToolUpgradeRecipeDefinition recipe)
        {
            var costs = new List<ToolUpgradeCostViewModel>();
            var itemCosts = recipe.ItemCosts;

            for (var i = 0; i < itemCosts.Count; i++)
            {
                var cost = itemCosts[i];
                if (!cost.IsValid)
                    continue;

                var displayName = cost.ItemId.ToString();
                UnityEngine.Sprite icon = null;
                if (itemDatabase.TryGetItem(cost.ItemId, out var itemDefinition) && itemDefinition != null)
                {
                    displayName = itemDefinition.DisplayName;
                    icon = itemDefinition.Icon;
                }

                costs.Add(new ToolUpgradeCostViewModel(
                    cost.ItemId,
                    displayName,
                    icon,
                    cost.Quantity,
                    inventory.GetQuantity(cost.ItemId)));
            }

            return MergeDuplicateCosts(costs);
        }

        private static List<ToolUpgradeCostViewModel> MergeDuplicateCosts(List<ToolUpgradeCostViewModel> costs)
        {
            var merged = new List<ToolUpgradeCostViewModel>();

            for (var i = 0; i < costs.Count; i++)
            {
                var cost = costs[i];
                var existingIndex = IndexOfCost(merged, cost.ItemId);
                if (existingIndex < 0)
                {
                    merged.Add(cost);
                    continue;
                }

                var existing = merged[existingIndex];
                merged[existingIndex] = new ToolUpgradeCostViewModel(
                    existing.ItemId,
                    existing.DisplayName,
                    existing.Icon,
                    existing.RequiredAmount + cost.RequiredAmount,
                    existing.OwnedAmount);
            }

            return merged;
        }

        private static int IndexOfCost(List<ToolUpgradeCostViewModel> costs, ItemId itemId)
        {
            for (var i = 0; i < costs.Count; i++)
            {
                if (costs[i].ItemId == itemId)
                    return i;
            }

            return -1;
        }

        private static int GetMaxLevel(ItemDefinition itemDefinition)
        {
            if (itemDefinition == null)
                return 0;

            if (!itemDefinition.TryGetModule<ToolPartModule>(out var module) || module == null)
                return 0;

            return module.MaxLevel;
        }

        private static bool AreCostsFulfilled(List<ToolUpgradeCostViewModel> costs)
        {
            for (var i = 0; i < costs.Count; i++)
            {
                if (!costs[i].IsFulfilled)
                    return false;
            }

            return true;
        }

        private static ToolSlotViewModel FindSelectedSlot(List<ToolSlotViewModel> slots)
        {
            for (var i = 0; i < slots.Count; i++)
            {
                if (slots[i].IsSelected)
                    return slots[i];
            }

            return null;
        }
    }
}
