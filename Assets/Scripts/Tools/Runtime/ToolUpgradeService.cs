using System;
using System.Collections.Generic;
using Inventory;
using Items.Runtime;
using Items.Runtime.Modules;
using Tools.Definitions;
using Tools.State;
using ToolLogger = Utilities.Logging.Logger;

namespace Tools.Runtime
{
    /// <summary>
    /// Runtime service for validating and applying upgrades to installed permanent tool parts.
    /// </summary>
    public sealed class ToolUpgradeService : IToolUpgradeService
    {
        private readonly ToolCollectionState tools;
        private readonly ItemDatabase itemDatabase;
        private readonly IInventoryService inventory;
        private readonly ToolUpgradeRecipeCatalog recipeCatalog;

        /// <inheritdoc />
        public event Action<ToolPartUpgradedEventData> ToolPartUpgraded;

        /// <summary>
        /// Creates a permanent tool upgrade service.
        /// </summary>
        public ToolUpgradeService(
            ToolCollectionState tools,
            ItemDatabase itemDatabase,
            IInventoryService inventory,
            ToolUpgradeRecipeCatalog recipeCatalog)
        {
            this.tools = tools ?? throw new ArgumentNullException(nameof(tools));
            this.itemDatabase = itemDatabase ?? throw new ArgumentNullException(nameof(itemDatabase));
            this.inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            this.recipeCatalog = recipeCatalog ?? throw new ArgumentNullException(nameof(recipeCatalog));
        }

        /// <inheritdoc />
        public ToolUpgradeResult TryUpgradeInstalledPart(ToolId toolId, ToolPartSlotId slotId)
        {
            var slotResult = TryGetSlot(toolId, slotId, out var slot);
            if (!slotResult.IsSuccess)
                return slotResult;

            var partItemId = slot.InstalledPartItemId;
            var currentLevel = slot.PartLevel;
            var validation = ValidateInstalledPart(toolId, slotId, partItemId, currentLevel, out var maxLevel);
            if (!validation.IsSuccess)
                return validation;

            var targetLevel = currentLevel + 1;
            if (!TryGetRecipe(toolId, slotId, partItemId, currentLevel, targetLevel, out var recipe))
            {
                return ToolUpgradeResult.Failure(
                    ToolUpgradeFailureReason.RecipeNotFound,
                    "No matching upgrade recipe was found for the installed part and current level.",
                    toolId,
                    slotId,
                    partItemId,
                    currentLevel,
                    targetLevel);
            }

            if (!recipe.IsValid())
            {
                ToolLogger.LogWarning($"Invalid tool upgrade recipe '{recipe.name}' was rejected.");
                return ToolUpgradeResult.Failure(
                    ToolUpgradeFailureReason.InvalidRecipe,
                    "The matching upgrade recipe is invalid.",
                    toolId,
                    slotId,
                    partItemId,
                    currentLevel,
                    targetLevel,
                    recipe);
            }

            if (recipe.ToLevel > maxLevel)
            {
                return ToolUpgradeResult.Failure(
                    ToolUpgradeFailureReason.MaxLevelReached,
                    "The matching recipe would exceed the part maximum level.",
                    toolId,
                    slotId,
                    partItemId,
                    currentLevel,
                    targetLevel,
                    recipe);
            }

            if (HasInvalidCost(recipe))
            {
                ToolLogger.LogWarning($"Tool upgrade recipe '{recipe.name}' has an invalid item cost.");
                return ToolUpgradeResult.Failure(
                    ToolUpgradeFailureReason.InvalidRecipe,
                    "The matching upgrade recipe has an invalid item cost.",
                    toolId,
                    slotId,
                    partItemId,
                    currentLevel,
                    targetLevel,
                    recipe);
            }

            var requiredCosts = BuildRequiredCosts(recipe);
            var missingCosts = GetMissingCosts(requiredCosts);
            if (missingCosts.Count > 0)
            {
                return ToolUpgradeResult.Failure(
                    ToolUpgradeFailureReason.MissingCost,
                    "Inventory does not contain all required upgrade costs.",
                    toolId,
                    slotId,
                    partItemId,
                    currentLevel,
                    targetLevel,
                    recipe,
                    missingCosts);
            }

            if (!TryConsumeCosts(requiredCosts))
            {
                return ToolUpgradeResult.Failure(
                    ToolUpgradeFailureReason.InventoryConsumeFailed,
                    "Failed to consume upgrade costs. Any consumed costs were rolled back where possible.",
                    toolId,
                    slotId,
                    partItemId,
                    currentLevel,
                    targetLevel,
                    recipe);
            }

            slot.SetPartLevel(recipe.ToLevel);
            ToolPartUpgraded?.Invoke(new ToolPartUpgradedEventData(
                toolId,
                slotId,
                partItemId,
                currentLevel,
                recipe.ToLevel,
                recipe));

            return ToolUpgradeResult.Success(
                toolId,
                slotId,
                partItemId,
                currentLevel,
                recipe.ToLevel,
                recipe);
        }

        private ToolUpgradeResult TryGetSlot(
            ToolId toolId,
            ToolPartSlotId slotId,
            out ToolPartSlotState slot)
        {
            slot = null;

            if (!toolId.IsValid)
            {
                return ToolUpgradeResult.Failure(
                    ToolUpgradeFailureReason.InvalidTool,
                    "Invalid permanent tool id.",
                    toolId,
                    slotId);
            }

            if (!slotId.IsValid)
            {
                return ToolUpgradeResult.Failure(
                    ToolUpgradeFailureReason.InvalidSlot,
                    "Invalid permanent tool part slot id.",
                    toolId,
                    slotId);
            }

            var tool = tools.GetTool(toolId);
            if (tool == null)
            {
                return ToolUpgradeResult.Failure(
                    ToolUpgradeFailureReason.InvalidTool,
                    "Permanent tool state was not found.",
                    toolId,
                    slotId);
            }

            var preset = tool.GetPreset(tool.ActivePresetIndex);
            if (preset == null)
            {
                return ToolUpgradeResult.Failure(
                    ToolUpgradeFailureReason.InvalidPreset,
                    "Active permanent tool preset was not found.",
                    toolId,
                    slotId);
            }

            slot = preset.GetSlot(slotId);
            if (slot != null)
                return ToolUpgradeResult.Success(toolId, slotId, default, 0, 0, null);

            return ToolUpgradeResult.Failure(
                ToolUpgradeFailureReason.InvalidSlot,
                "Permanent tool part slot was not found.",
                toolId,
                slotId);
        }

        private ToolUpgradeResult ValidateInstalledPart(
            ToolId toolId,
            ToolPartSlotId slotId,
            ItemId partItemId,
            int currentLevel,
            out int maxLevel)
        {
            maxLevel = 0;

            if (!partItemId.IsValid)
            {
                return ToolUpgradeResult.Failure(
                    ToolUpgradeFailureReason.EmptySlot,
                    "The requested permanent tool slot has no installed part.",
                    toolId,
                    slotId);
            }

            if (!itemDatabase.TryGet(partItemId, out var itemDefinition) || itemDefinition == null)
            {
                return ToolUpgradeResult.Failure(
                    ToolUpgradeFailureReason.ItemDefinitionNotFound,
                    "Installed part item was not found in the item database.",
                    toolId,
                    slotId,
                    partItemId,
                    currentLevel);
            }

            if (!itemDefinition.TryGetModule<ToolPartModule>(out var toolPartModule) || toolPartModule == null)
            {
                return ToolUpgradeResult.Failure(
                    ToolUpgradeFailureReason.NotToolPart,
                    "Installed item does not have a ToolPartModule.",
                    toolId,
                    slotId,
                    partItemId,
                    currentLevel);
            }

            if (toolPartModule.CompatibleToolId != toolId)
            {
                return ToolUpgradeResult.Failure(
                    ToolUpgradeFailureReason.PartNotCompatibleWithTool,
                    "Installed part is not compatible with this permanent tool.",
                    toolId,
                    slotId,
                    partItemId,
                    currentLevel);
            }

            if (toolPartModule.CompatibleSlotId != slotId)
            {
                return ToolUpgradeResult.Failure(
                    ToolUpgradeFailureReason.PartNotCompatibleWithSlot,
                    "Installed part is not compatible with this internal slot.",
                    toolId,
                    slotId,
                    partItemId,
                    currentLevel);
            }

            maxLevel = toolPartModule.MaxLevel;
            if (currentLevel < maxLevel)
                return ToolUpgradeResult.Success(toolId, slotId, partItemId, currentLevel, currentLevel, null);

            return ToolUpgradeResult.Failure(
                ToolUpgradeFailureReason.MaxLevelReached,
                "Installed part is already at its maximum level.",
                toolId,
                slotId,
                partItemId,
                currentLevel,
                currentLevel);
        }

        private bool TryGetRecipe(
            ToolId toolId,
            ToolPartSlotId slotId,
            ItemId partItemId,
            int currentLevel,
            int targetLevel,
            out ToolUpgradeRecipeDefinition recipe)
        {
            return recipeCatalog.TryGetRecipe(
                toolId,
                slotId,
                partItemId,
                currentLevel,
                targetLevel,
                out recipe);
        }

        private static bool HasInvalidCost(ToolUpgradeRecipeDefinition recipe)
        {
            var costs = recipe.ItemCosts;
            for (var i = 0; i < costs.Count; i++)
            {
                if (!costs[i].IsValid)
                    return true;
            }

            return false;
        }

        private static List<ToolUpgradeCost> BuildRequiredCosts(ToolUpgradeRecipeDefinition recipe)
        {
            var results = new List<ToolUpgradeCost>();
            var costs = recipe.ItemCosts;

            for (var i = 0; i < costs.Count; i++)
            {
                var cost = costs[i];
                AddCost(results, cost);
            }

            return results;
        }

        private static void AddCost(List<ToolUpgradeCost> costs, ToolUpgradeCost cost)
        {
            for (var i = 0; i < costs.Count; i++)
            {
                var existing = costs[i];
                if (existing.ItemId != cost.ItemId)
                    continue;

                costs[i] = new ToolUpgradeCost(existing.ItemId, existing.Quantity + cost.Quantity);
                return;
            }

            costs.Add(cost);
        }

        private List<MissingToolUpgradeCost> GetMissingCosts(List<ToolUpgradeCost> requiredCosts)
        {
            var missing = new List<MissingToolUpgradeCost>();

            for (var i = 0; i < requiredCosts.Count; i++)
            {
                var cost = requiredCosts[i];
                var available = inventory.GetQuantity(cost.ItemId);
                if (available >= cost.Quantity)
                    continue;

                missing.Add(new MissingToolUpgradeCost(cost.ItemId, cost.Quantity, available));
            }

            return missing;
        }

        private bool TryConsumeCosts(List<ToolUpgradeCost> requiredCosts)
        {
            var consumed = new List<ToolUpgradeCost>(requiredCosts.Count);

            for (var i = 0; i < requiredCosts.Count; i++)
            {
                var cost = requiredCosts[i];
                var result = inventory.TryRemove(cost.ItemId, cost.Quantity);
                if (result.IsSuccess)
                {
                    consumed.Add(cost);
                    continue;
                }

                RollbackCosts(consumed);
                return false;
            }

            return true;
        }

        private void RollbackCosts(List<ToolUpgradeCost> consumedCosts)
        {
            for (var i = 0; i < consumedCosts.Count; i++)
            {
                var cost = consumedCosts[i];
                var result = inventory.TryAdd(cost.ItemId, cost.Quantity);
                if (result.IsSuccess)
                    continue;

                ToolLogger.LogWarning($"Failed to roll back consumed upgrade cost '{cost.ItemId}' x{cost.Quantity}: {result.Code}.");
            }
        }
    }
}
