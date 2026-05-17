using System.Collections.Generic;
using Items.Runtime.Modules;
using Items.Runtime;
using Tools.Definitions;
using Tools.Runtime;
using UnityEditor;

namespace Tools.Editor
{
    /// <summary>
    /// Validates pending tool part and recipe creation requests.
    /// </summary>
    internal static class ToolPartRecipeCreatorValidator
    {
        /// <summary>
        /// Validates all inputs for the current creation request.
        /// </summary>
        public static ToolPartRecipeCreatorValidation Validate(
            ToolPartRecipeCreatorState state,
            ToolPartSlotDefinition slot,
            string itemId,
            string itemPath,
            string modulePath,
            IReadOnlyList<ToolRecipeRow> recipeRows,
            IReadOnlyList<string> recipePaths)
        {
            var result = new ToolPartRecipeCreatorValidation();
            ValidateContext(state, slot, result);
            ValidatePart(state, itemId, itemPath, modulePath, result);
            ValidateRecipes(state, slot, itemId, recipeRows, recipePaths, result);
            ValidateWarnings(state, result);
            return result;
        }

        private static void ValidateContext(
            ToolPartRecipeCreatorState state,
            ToolPartSlotDefinition slot,
            ToolPartRecipeCreatorValidation result)
        {
            if (state.ToolDefinition == null)
                result.Errors.Add("ToolDefinition is missing.");

            if (slot == null)
                result.Errors.Add("Selected slot is missing.");

            if (state.ToolDefinition != null && slot != null && !state.ToolDefinition.SupportsSlot(slot.Id))
                result.Errors.Add("Selected slot is not supported by the selected tool.");

            if (state.CreationMode != ToolContentCreationMode.RecipesOnly && state.RegisterItem && state.ItemDatabase == null)
                result.Errors.Add("ItemDatabase is required because item registration is enabled.");

            if (state.CreationMode != ToolContentCreationMode.ToolPartOnly && state.RegisterRecipes && state.RecipeMode != ToolRecipeCreationMode.NoRecipes && state.RecipeCatalog == null)
                result.Errors.Add("ToolUpgradeRecipeCatalog is required because recipe registration is enabled.");

            if (state.CreationMode == ToolContentCreationMode.RecipesOnly && state.ExistingRecipePart == null)
            {
                result.Errors.Add("Existing part is required when creating recipes only.");
                return;
            }

            if (state.CreationMode == ToolContentCreationMode.RecipesOnly)
                ValidateExistingRecipePart(state, slot, result);

            if (state.CreationMode == ToolContentCreationMode.RecipesOnly && state.RecipeMode == ToolRecipeCreationMode.NoRecipes)
                result.Errors.Add("Recipe mode cannot be No Recipes when creating recipes only.");
        }

        private static void ValidateExistingRecipePart(
            ToolPartRecipeCreatorState state,
            ToolPartSlotDefinition slot,
            ToolPartRecipeCreatorValidation result)
        {
            if (state.ExistingRecipePart == null || state.ToolDefinition == null || slot == null)
                return;

            if (!state.ExistingRecipePart.TryGetModule<ToolPartModule>(out var module) || module == null)
            {
                result.Errors.Add("Existing recipe part must have a ToolPartModule.");
                return;
            }

            if (!module.IsCompatibleWith(state.ToolDefinition.Id, slot.Id))
                result.Errors.Add("Existing recipe part is not compatible with the selected tool and slot.");
        }

        private static void ValidatePart(
            ToolPartRecipeCreatorState state,
            string itemId,
            string itemPath,
            string modulePath,
            ToolPartRecipeCreatorValidation result)
        {
            if (state.CreationMode == ToolContentCreationMode.RecipesOnly)
                return;

            if (string.IsNullOrWhiteSpace(state.PartDisplayName))
                result.Errors.Add("Part display name is required.");

            if (string.IsNullOrWhiteSpace(itemId))
                result.Errors.Add("Generated or entered ItemId is empty.");

            if (!IsValidAssetFolder(state.PartFolder))
                result.Errors.Add("Tool part item output folder must be under Assets.");

            if (!IsValidAssetFolder(state.ModuleFolder))
                result.Errors.Add("Tool part module output folder must be under Assets.");

            var newItemId = new ItemId(itemId);
            if (state.ItemDatabase != null && state.ItemDatabase.Contains(newItemId))
                result.Errors.Add($"ItemId '{itemId}' already exists in the selected ItemDatabase.");
            else if (ToolEditorItemLookup.TryGet(newItemId, out _))
                result.Errors.Add($"ItemId '{itemId}' already exists.");

            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(itemPath) != null)
                result.Errors.Add($"ItemDefinition path already exists: {itemPath}");

            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(modulePath) != null)
                result.Errors.Add($"ToolPartModule path already exists: {modulePath}");

            if (!AssetDatabase.IsValidFolder(state.PartFolder))
                result.Warnings.Add($"Item output folder will be created: {state.PartFolder}");

            if (!AssetDatabase.IsValidFolder(state.ModuleFolder))
                result.Warnings.Add($"Module output folder will be created: {state.ModuleFolder}");
        }

        private static void ValidateRecipes(
            ToolPartRecipeCreatorState state,
            ToolPartSlotDefinition slot,
            string itemId,
            IReadOnlyList<ToolRecipeRow> recipeRows,
            IReadOnlyList<string> recipePaths,
            ToolPartRecipeCreatorValidation result)
        {
            if (state.CreationMode == ToolContentCreationMode.ToolPartOnly)
                return;

            if (state.RecipeMode == ToolRecipeCreationMode.NoRecipes)
                return;

            if (!IsValidAssetFolder(state.RecipeFolder))
                result.Errors.Add("Recipe output folder must be under Assets.");

            if (!AssetDatabase.IsValidFolder(state.RecipeFolder))
                result.Warnings.Add($"Recipe output folder will be created: {state.RecipeFolder}");

            if (string.IsNullOrWhiteSpace(itemId))
                result.Errors.Add("Part ItemId is required for recipe creation.");

            if (recipeRows.Count == 0)
                result.Errors.Add("At least one recipe row is required.");

            for (var i = 0; i < recipeRows.Count; i++)
            {
                ValidateRecipeRow(state, slot, itemId, recipeRows[i], recipePaths[i], i, result);
            }
        }

        private static void ValidateRecipeRow(
            ToolPartRecipeCreatorState state,
            ToolPartSlotDefinition slot,
            string itemId,
            ToolRecipeRow row,
            string recipePath,
            int index,
            ToolPartRecipeCreatorValidation result)
        {
            if (row.ToLevel <= row.FromLevel)
                result.Errors.Add($"Recipe row {index + 1} has an invalid level transition.");

            if (row.ToLevel != row.FromLevel + 1)
                result.Warnings.Add($"Recipe row {index + 1} skips one or more levels.");

            if (row.Costs.Count == 0)
                result.Warnings.Add($"Recipe row {index + 1} has no costs.");

            for (var i = 0; i < row.Costs.Count; i++)
            {
                var cost = row.Costs[i];
                if (cost.Item == null)
                    result.Errors.Add($"Recipe row {index + 1}, cost {i + 1}: cost item is missing.");

                if (cost.Quantity <= 0)
                    result.Errors.Add($"Recipe row {index + 1}, cost {i + 1}: quantity must be greater than zero.");
            }

            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(recipePath) != null)
                result.Errors.Add($"Recipe path already exists: {recipePath}");

            if (state.RecipeCatalog != null && slot != null && HasRecipeKey(state.RecipeCatalog, state.ToolDefinition.Id, slot.Id, new ItemId(itemId), row.FromLevel, row.ToLevel))
                result.Errors.Add($"Recipe key already exists for {itemId} L{row.FromLevel}-L{row.ToLevel}.");
        }

        private static void ValidateWarnings(ToolPartRecipeCreatorState state, ToolPartRecipeCreatorValidation result)
        {
            if (state.Icon == null)
                result.Warnings.Add("Icon is missing.");

            if (string.IsNullOrWhiteSpace(state.Description))
                result.Warnings.Add("Description is empty.");

            if (state.Bonuses.Count == 0)
                result.Warnings.Add("No bonuses are configured.");

            var bonusTypes = new HashSet<ToolBonusType>();
            for (var i = 0; i < state.Bonuses.Count; i++)
            {
                var bonus = state.Bonuses[i];
                if (bonus.BaseValue < 0f || bonus.ValuePerLevel < 0f)
                    result.Warnings.Add($"Bonus {i + 1} has a negative value.");

                if (!bonusTypes.Add(bonus.Type))
                    result.Warnings.Add($"Duplicate bonus type: {bonus.Type}.");
            }
        }

        private static bool HasRecipeKey(
            ToolUpgradeRecipeCatalog catalog,
            ToolId toolId,
            ToolPartSlotId slotId,
            ItemId itemId,
            int fromLevel,
            int toLevel)
        {
            var recipes = catalog.Recipes;
            for (var i = 0; i < recipes.Count; i++)
            {
                var recipe = recipes[i];
                if (recipe == null)
                    continue;

                if (recipe.ToolId == toolId
                    && recipe.SlotId == slotId
                    && recipe.PartItemId == itemId
                    && recipe.FromLevel == fromLevel
                    && recipe.ToLevel == toLevel)
                    return true;
            }

            return false;
        }

        private static bool IsValidAssetFolder(string path)
        {
            return !string.IsNullOrWhiteSpace(path) && path.Replace('\\', '/').StartsWith("Assets/");
        }
    }
}
