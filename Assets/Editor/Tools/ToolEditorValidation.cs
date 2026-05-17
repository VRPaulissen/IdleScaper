using System.Collections.Generic;
using Items.Runtime;
using Tools.Definitions;
using UnityEngine;

namespace Tools.Editor
{
    /// <summary>
    /// Shared non-destructive validation helpers for permanent tool content editors.
    /// </summary>
    internal static class ToolEditorValidation
    {
        /// <summary>
        /// Validates an upgrade recipe catalog and appends readable messages.
        /// </summary>
        public static ToolRecipeCatalogValidation ValidateCatalog(
            ToolUpgradeRecipeCatalog catalog,
            List<string> messages)
        {
            var result = new ToolRecipeCatalogValidation();
            if (messages == null)
                return result;

            messages.Clear();
            if (catalog == null)
            {
                messages.Add("Catalog is missing.");
                result.InvalidRecipeCount++;
                return result;
            }

            var keys = new Dictionary<string, ToolUpgradeRecipeDefinition>();
            var recipes = catalog.Recipes;
            result.RecipeCount = recipes.Count;

            for (var i = 0; i < recipes.Count; i++)
            {
                var recipe = recipes[i];
                if (recipe == null)
                {
                    result.InvalidRecipeCount++;
                    messages.Add($"Recipe entry {i} is null.");
                    continue;
                }

                ValidateRecipe(recipe, messages, ref result);
                var key = BuildRecipeKey(recipe);
                if (!keys.TryGetValue(key, out var first))
                {
                    keys.Add(key, recipe);
                    continue;
                }

                result.DuplicateKeyCount++;
                messages.Add($"Duplicate recipe key: '{first.name}' and '{recipe.name}'.");
            }

            return result;
        }

        private static void ValidateRecipe(
            ToolUpgradeRecipeDefinition recipe,
            List<string> messages,
            ref ToolRecipeCatalogValidation result)
        {
            var invalid = false;
            if (!recipe.ToolId.IsValid)
            {
                invalid = true;
                messages.Add($"Recipe '{recipe.name}' has an empty ToolId.");
            }

            if (!recipe.SlotId.IsValid)
            {
                invalid = true;
                messages.Add($"Recipe '{recipe.name}' has an empty SlotId.");
            }

            if (!recipe.PartItemId.IsValid)
            {
                invalid = true;
                messages.Add($"Recipe '{recipe.name}' has an empty part ItemId.");
            }
            else if (!ToolEditorItemLookup.TryGet(recipe.PartItemId, out _))
            {
                messages.Add($"Recipe '{recipe.name}' part item '{recipe.PartItemId}' was not found.");
            }

            if (recipe.ToLevel <= recipe.FromLevel)
            {
                invalid = true;
                messages.Add($"Recipe '{recipe.name}' has an invalid level transition.");
            }

            if (recipe.ItemCosts.Count == 0)
            {
                invalid = true;
                messages.Add($"Recipe '{recipe.name}' has no item costs.");
            }

            var costIds = new HashSet<ItemId>();
            for (var i = 0; i < recipe.ItemCosts.Count; i++)
            {
                var cost = recipe.ItemCosts[i];
                if (!cost.IsValid)
                {
                    invalid = true;
                    messages.Add($"Recipe '{recipe.name}' has an invalid cost at index {i}: item id is missing or quantity is <= 0.");
                    continue;
                }

                if (!costIds.Add(cost.ItemId))
                    messages.Add($"Recipe '{recipe.name}' has duplicate cost item '{cost.ItemId}'.");

                if (!ToolEditorItemLookup.TryGet(cost.ItemId, out _))
                    messages.Add($"Recipe '{recipe.name}' cost item '{cost.ItemId}' was not found.");
            }

            if (invalid)
                result.InvalidRecipeCount++;
        }

        private static string BuildRecipeKey(ToolUpgradeRecipeDefinition recipe)
        {
            return $"{recipe.ToolId}|{recipe.SlotId}|{recipe.PartItemId}|{recipe.FromLevel}|{recipe.ToLevel}";
        }
    }

    /// <summary>
    /// Summary counts produced by upgrade recipe catalog validation.
    /// </summary>
    internal struct ToolRecipeCatalogValidation
    {
        /// <summary>
        /// Number of recipe references in the catalog.
        /// </summary>
        public int RecipeCount;

        /// <summary>
        /// Number of duplicate recipe keys found.
        /// </summary>
        public int DuplicateKeyCount;

        /// <summary>
        /// Number of invalid recipe entries found.
        /// </summary>
        public int InvalidRecipeCount;
    }
}
