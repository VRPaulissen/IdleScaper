using System.Collections.Generic;
using Items.Runtime;
using Tools.Runtime;
using UnityEngine;
using ToolLogger = Utilities.Logging.Logger;

namespace Tools.Definitions
{
    /// <summary>
    /// Asset catalog for looking up permanent tool upgrade recipes.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Tools/Upgrade Recipe Catalog", fileName = "ToolUpgradeRecipeCatalog")]
    public sealed class ToolUpgradeRecipeCatalog : ScriptableObject
    {
        [SerializeField] private List<ToolUpgradeRecipeDefinition> recipes = new List<ToolUpgradeRecipeDefinition>();

        /// <summary>
        /// All registered permanent tool upgrade recipes.
        /// </summary>
        public IReadOnlyList<ToolUpgradeRecipeDefinition> Recipes => recipes;

        /// <summary>
        /// Attempts to find a recipe for the given installed part state.
        /// </summary>
        public bool TryGetRecipe(
            ToolId toolId,
            ToolPartSlotId slotId,
            ItemId partItemId,
            int currentLevel,
            out ToolUpgradeRecipeDefinition recipe)
        {
            recipe = null;

            if (!toolId.IsValid)
                return false;

            if (!slotId.IsValid)
                return false;

            if (!partItemId.IsValid)
                return false;

            for (var i = 0; i < recipes.Count; i++)
            {
                var candidate = recipes[i];
                if (candidate == null)
                    continue;

                if (!candidate.Matches(toolId, slotId, partItemId, currentLevel))
                    continue;

                if (recipe == null)
                {
                    recipe = candidate;
                    continue;
                }

                LogDuplicateRecipe(recipe, candidate);
            }

            return recipe != null;
        }

        /// <summary>
        /// Attempts to find a recipe for the given installed part state and target level.
        /// </summary>
        public bool TryGetRecipe(
            ToolId toolId,
            ToolPartSlotId slotId,
            ItemId partItemId,
            int currentLevel,
            int targetLevel,
            out ToolUpgradeRecipeDefinition recipe)
        {
            recipe = null;

            if (!toolId.IsValid)
                return false;

            if (!slotId.IsValid)
                return false;

            if (!partItemId.IsValid)
                return false;

            for (var i = 0; i < recipes.Count; i++)
            {
                var candidate = recipes[i];
                if (candidate == null)
                    continue;

                if (candidate.ToLevel != targetLevel)
                    continue;

                if (!candidate.Matches(toolId, slotId, partItemId, currentLevel))
                    continue;

                if (recipe == null)
                {
                    recipe = candidate;
                    continue;
                }

                LogDuplicateRecipe(recipe, candidate);
            }

            return recipe != null;
        }

        /// <summary>
        /// Gets all recipes matching the given permanent tool and slot.
        /// </summary>
        public void GetRecipesForSlot(
            ToolId toolId,
            ToolPartSlotId slotId,
            List<ToolUpgradeRecipeDefinition> results)
        {
            if (results == null)
                return;

            results.Clear();

            if (!toolId.IsValid)
                return;

            if (!slotId.IsValid)
                return;

            for (var i = 0; i < recipes.Count; i++)
            {
                var recipe = recipes[i];
                if (recipe == null)
                    continue;

                if (recipe.ToolId != toolId || recipe.SlotId != slotId)
                    continue;

                results.Add(recipe);
            }
        }

        private static void LogDuplicateRecipe(
            ToolUpgradeRecipeDefinition first,
            ToolUpgradeRecipeDefinition duplicate)
        {
            ToolLogger.LogWarning(
                $"Duplicate tool upgrade recipe ignored. " +
                $"Using '{first.name}', ignoring '{duplicate.name}'.");
        }
    }
}
