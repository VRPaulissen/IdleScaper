using System;
using System.Collections.Generic;
using Items.Definitions;
using Items.Runtime;
using Items.Runtime.Modules;
using Tools.Definitions;
using Tools.Runtime;
using UnityEditor;
using UnityEngine;

namespace Tools.Editor
{
    /// <summary>
    /// Creates permanent tool part and recipe assets for the editor window.
    /// </summary>
    internal static class ToolPartRecipeCreatorAssetFactory
    {
        /// <summary>
        /// Creates all requested assets and registrations.
        /// </summary>
        public static ToolPartRecipeCreatorResult Create(
            ToolPartRecipeCreatorState state,
            ToolPartSlotDefinition slot,
            string itemId,
            string itemPath,
            string modulePath,
            IReadOnlyList<ToolRecipeRow> recipeRows,
            IReadOnlyList<string> recipePaths)
        {
            var result = new ToolPartRecipeCreatorResult();
            try
            {
                if (state.CreationMode != ToolContentCreationMode.RecipesOnly)
                {
                    EnsureFolder(state.PartFolder);
                    EnsureFolder(state.ModuleFolder);
                }

                if (state.CreationMode != ToolContentCreationMode.ToolPartOnly && state.RecipeMode != ToolRecipeCreationMode.NoRecipes)
                    EnsureFolder(state.RecipeFolder);

                if (state.CreationMode != ToolContentCreationMode.RecipesOnly)
                {
                    result.ToolPartModule = CreateModule(state, slot, modulePath);
                    result.ItemDefinition = CreateItemDefinition(state, itemId, result.ToolPartModule, itemPath);
                }

                if (state.CreationMode != ToolContentCreationMode.RecipesOnly && state.RegisterItem)
                    result.UpdatedItemDatabase = RegisterItem(state.ItemDatabase, result.ItemDefinition);

                if (state.CreationMode != ToolContentCreationMode.ToolPartOnly && state.RecipeMode != ToolRecipeCreationMode.NoRecipes)
                    CreateRecipes(state, slot, itemId, recipeRows, recipePaths, result);

                if (state.RegisterRecipes && result.Recipes.Count > 0)
                    result.UpdatedRecipeCatalog = RegisterRecipes(state.RecipeCatalog, result.Recipes);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                ToolEditorItemLookup.Refresh();
                Selection.activeObject = result.ItemDefinition;
                EditorGUIUtility.PingObject(result.ItemDefinition);
            }
            catch (Exception ex)
            {
                result.Messages.Add($"Creation failed after an unexpected exception: {ex.Message}");
                Debug.LogException(ex);
            }

            return result;
        }

        private static ToolPartModule CreateModule(
            ToolPartRecipeCreatorState state,
            ToolPartSlotDefinition slot,
            string path)
        {
            var module = ScriptableObject.CreateInstance<ToolPartModule>();
            module.name = $"{state.PartDisplayName} Module";
            var serialized = new SerializedObject(module);
            SetId(serialized.FindProperty("compatibleToolId"), state.ToolDefinition.Id.ToString());
            SetId(serialized.FindProperty("compatibleSlotId"), slot.Id.ToString());
            serialized.FindProperty("maxLevel").intValue = Mathf.Max(1, state.MaxUpgradeLevel);
            SetBonuses(serialized.FindProperty("bonuses"), state.Bonuses);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.CreateAsset(module, path);
            Undo.RegisterCreatedObjectUndo(module, "Create Tool Part Module");
            return module;
        }

        private static ItemDefinition CreateItemDefinition(
            ToolPartRecipeCreatorState state,
            string itemId,
            ToolPartModule module,
            string path)
        {
            var item = ScriptableObject.CreateInstance<ItemDefinition>();
            item.name = state.PartDisplayName;
            var serialized = new SerializedObject(item);
            SetId(serialized.FindProperty("id"), itemId);
            serialized.FindProperty("displayName").stringValue = state.PartDisplayName;
            serialized.FindProperty("description").stringValue = state.Description ?? string.Empty;
            serialized.FindProperty("icon").objectReferenceValue = state.Icon;
            serialized.FindProperty("stackable").boolValue = state.Stackable;
            serialized.FindProperty("maxStackSize").intValue = Mathf.Max(1, state.MaxStackSize);
            var modules = serialized.FindProperty("modules");
            modules.arraySize = 1;
            modules.GetArrayElementAtIndex(0).objectReferenceValue = module;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.CreateAsset(item, path);
            Undo.RegisterCreatedObjectUndo(item, "Create Tool Part Item");
            return item;
        }

        private static void CreateRecipes(
            ToolPartRecipeCreatorState state,
            ToolPartSlotDefinition slot,
            string itemId,
            IReadOnlyList<ToolRecipeRow> recipeRows,
            IReadOnlyList<string> recipePaths,
            ToolPartRecipeCreatorResult result)
        {
            for (var i = 0; i < recipeRows.Count; i++)
            {
                var recipe = CreateRecipe(state, slot, itemId, recipeRows[i], recipePaths[i]);
                result.Recipes.Add(recipe);
            }
        }

        private static ToolUpgradeRecipeDefinition CreateRecipe(
            ToolPartRecipeCreatorState state,
            ToolPartSlotDefinition slot,
            string itemId,
            ToolRecipeRow row,
            string path)
        {
            var recipe = ScriptableObject.CreateInstance<ToolUpgradeRecipeDefinition>();
            recipe.name = $"{SanitizeAssetName(state.PartDisplayName).Replace(' ', '_')} L{row.FromLevel}-L{row.ToLevel}";
            var serialized = new SerializedObject(recipe);
            SetId(serialized.FindProperty("toolId"), state.ToolDefinition.Id.ToString());
            SetId(serialized.FindProperty("slotId"), slot.Id.ToString());
            SetId(serialized.FindProperty("partItemId"), itemId);
            serialized.FindProperty("fromLevel").intValue = row.FromLevel;
            serialized.FindProperty("toLevel").intValue = row.ToLevel;
            SetCosts(serialized.FindProperty("itemCosts"), row.Costs);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.CreateAsset(recipe, path);
            Undo.RegisterCreatedObjectUndo(recipe, "Create Tool Upgrade Recipe");
            return recipe;
        }

        private static bool RegisterItem(ItemDatabase database, ItemDefinition item)
        {
            Undo.RecordObject(database, "Register Tool Part Item");
            var serialized = new SerializedObject(database);
            var definitions = serialized.FindProperty("definitions");
            definitions.arraySize++;
            definitions.GetArrayElementAtIndex(definitions.arraySize - 1).objectReferenceValue = item;
            serialized.ApplyModifiedProperties();
            EditorUtility.SetDirty(database);
            return true;
        }

        private static bool RegisterRecipes(
            ToolUpgradeRecipeCatalog catalog,
            IReadOnlyList<ToolUpgradeRecipeDefinition> recipes)
        {
            Undo.RecordObject(catalog, "Register Tool Upgrade Recipes");
            var serialized = new SerializedObject(catalog);
            var entries = serialized.FindProperty("recipes");
            for (var i = 0; i < recipes.Count; i++)
            {
                entries.arraySize++;
                entries.GetArrayElementAtIndex(entries.arraySize - 1).objectReferenceValue = recipes[i];
            }

            serialized.ApplyModifiedProperties();
            EditorUtility.SetDirty(catalog);
            return true;
        }

        private static void SetBonuses(SerializedProperty property, IReadOnlyList<ToolBonusValue> bonuses)
        {
            property.arraySize = bonuses.Count;
            for (var i = 0; i < bonuses.Count; i++)
            {
                var bonus = property.GetArrayElementAtIndex(i);
                bonus.FindPropertyRelative("type").enumValueIndex = (int)bonuses[i].Type;
                bonus.FindPropertyRelative("baseValue").floatValue = bonuses[i].BaseValue;
                bonus.FindPropertyRelative("valuePerLevel").floatValue = bonuses[i].ValuePerLevel;
            }
        }

        private static void SetCosts(SerializedProperty property, IReadOnlyList<ToolRecipeCostRow> costs)
        {
            property.arraySize = costs.Count;
            for (var i = 0; i < costs.Count; i++)
            {
                var cost = property.GetArrayElementAtIndex(i);
                SetId(cost.FindPropertyRelative("itemId"), costs[i].Item.Id.ToString());
                cost.FindPropertyRelative("quantity").intValue = Mathf.Max(1, costs[i].Quantity);
            }
        }

        private static void SetId(SerializedProperty property, string value)
        {
            property.FindPropertyRelative("value").stringValue = value;
        }

        private static void EnsureFolder(string folder)
        {
            var normalized = folder.Replace('\\', '/').TrimEnd('/');
            if (AssetDatabase.IsValidFolder(normalized))
                return;

            var parts = normalized.Split('/');
            var current = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);

                current = next;
            }
        }

        /// <summary>
        /// Creates a filesystem-safe asset name.
        /// </summary>
        public static string SanitizeAssetName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "New Tool Part";

            var invalid = System.IO.Path.GetInvalidFileNameChars();
            var chars = value.Trim().ToCharArray();
            for (var i = 0; i < chars.Length; i++)
            {
                for (var j = 0; j < invalid.Length; j++)
                {
                    if (chars[i] != invalid[j])
                        continue;

                    chars[i] = '_';
                    break;
                }
            }

            return new string(chars);
        }
    }
}
