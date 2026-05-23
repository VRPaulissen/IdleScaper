using System;
using System.Collections.Generic;
using Items.Definitions;
using Tools.Definitions;
using UnityEditor;
using UnityEngine;

namespace Tools.Editor
{
    /// <summary>
    /// Custom inspector for permanent tool upgrade recipe catalogs with search, filters, and compact recipe rows.
    /// </summary>
    [CustomEditor(typeof(ToolUpgradeRecipeCatalog))]
    internal sealed class ToolUpgradeRecipeCatalogEditor : UnityEditor.Editor
    {
        private const string AllFilterOption = "All";
        private const string EmptyFilterOption = "Empty";
        private const int CompactButtonWidth = 58;
        private const int RemoveButtonWidth = 68;
        private const int LevelWidth = 76;

        private readonly List<string> validationMessages = new List<string>();
        private readonly List<RecipeRowView> visibleRows = new List<RecipeRowView>();
        private readonly List<RecipeRowView> allRows = new List<RecipeRowView>();
        private readonly HashSet<string> duplicateKeys = new HashSet<string>();
        private readonly Dictionary<string, int> keyCounts = new Dictionary<string, int>();

        private SerializedProperty recipes;
        private ToolRecipeCatalogValidation validation;

        private string searchText = string.Empty;
        private string selectedToolFilter = AllFilterOption;
        private string selectedSlotFilter = AllFilterOption;
        private string selectedPartFilter = AllFilterOption;
        private RecipeIssueFilter issueFilter = RecipeIssueFilter.All;
        private RecipeSortMode sortMode = RecipeSortMode.ToolSlotPartLevel;

        private bool hasValidated;
        private bool showFilteredRecipes = true;
        private bool showRawList;
        private bool showValidationMessages = true;

        private const int ToolColumnWidth = 160;
        private const int SlotColumnWidth = 170;
        private const int LevelColumnWidth = 62;
        private const int CostColumnWidth = 54;
        private const int ActionsColumnWidth = 188;
        private const int RowButtonWidth = 58;

        private void OnEnable()
        {
            recipes = serializedObject.FindProperty("recipes");
        }

        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            BuildRows();

            DrawSummary();
            ToolEditorStyles.Space();
            DrawSearchAndFilters();
            ToolEditorStyles.Space();
            DrawFilteredRecipeList();
            ToolEditorStyles.Space();
            DrawRawRecipeList();
            ToolEditorStyles.Space();
            DrawValidation();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSummary()
        {
            ToolEditorStyles.BeginBox();

            EditorGUILayout.LabelField("Upgrade Recipe Catalog", ToolEditorStyles.Header);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Total Recipes", recipes.arraySize.ToString());
            EditorGUILayout.LabelField("Visible", visibleRows.Count.ToString());
            EditorGUILayout.LabelField("Null Entries", CountNullRows().ToString());

            if (hasValidated)
            {
                EditorGUILayout.LabelField("Duplicate Keys", validation.DuplicateKeyCount.ToString());
                EditorGUILayout.LabelField("Invalid Recipes", validation.InvalidRecipeCount.ToString());
            }

            EditorGUILayout.EndHorizontal();

            ToolEditorStyles.Space();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Validate", GUILayout.Height(24)))
                Validate();

            if (GUILayout.Button("Refresh Item Lookup", GUILayout.Height(24)))
            {
                ToolEditorItemLookup.Refresh();
                BuildRows();
            }

            if (GUILayout.Button("Sort Catalog", GUILayout.Height(24)))
                SortCatalogByKey();

            using (new EditorGUI.DisabledScope(CountNullRows() == 0))
            {
                if (GUILayout.Button("Remove Nulls", GUILayout.Height(24)))
                    RemoveNullRecipes();
            }

            EditorGUILayout.EndHorizontal();

            ToolEditorStyles.EndBox();
        }

        private void DrawSearchAndFilters()
        {
            ToolEditorStyles.BeginBox();

            EditorGUILayout.LabelField("Search & Filters", ToolEditorStyles.Header);

            EditorGUI.BeginChangeCheck();
            searchText = EditorGUILayout.TextField("Search", searchText);
            selectedToolFilter = DrawStringPopup("Tool", selectedToolFilter, BuildToolOptions());
            selectedSlotFilter = DrawStringPopup("Slot", selectedSlotFilter, BuildSlotOptions());
            selectedPartFilter = DrawStringPopup("Part", selectedPartFilter, BuildPartOptions());
            issueFilter = (RecipeIssueFilter)EditorGUILayout.EnumPopup("Issues", issueFilter);
            sortMode = (RecipeSortMode)EditorGUILayout.EnumPopup("Sort View", sortMode);

            if (EditorGUI.EndChangeCheck())
                BuildRows();

            EditorGUILayout.BeginHorizontal();

            using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(searchText)
                                               && selectedToolFilter == AllFilterOption
                                               && selectedSlotFilter == AllFilterOption
                                               && selectedPartFilter == AllFilterOption
                                               && issueFilter == RecipeIssueFilter.All))
            {
                if (GUILayout.Button("Clear Filters"))
                    ClearFilters();
            }

            EditorGUILayout.EndHorizontal();

            ToolEditorStyles.EndBox();
        }

        private void DrawFilteredRecipeList()
        {
            ToolEditorStyles.BeginBox();

            showFilteredRecipes = EditorGUILayout.Foldout(showFilteredRecipes,
                $"Filtered Recipes ({visibleRows.Count}/{recipes.arraySize})", true);
            if (!showFilteredRecipes)
            {
                ToolEditorStyles.EndBox();
                return;
            }

            if (visibleRows.Count == 0)
            {
                EditorGUILayout.HelpBox("No recipes match the current filters.", MessageType.Info);
                ToolEditorStyles.EndBox();
                return;
            }

            DrawRecipeListHeader();

            for (var i = 0; i < visibleRows.Count; i++)
            {
                if (DrawRecipeRow(visibleRows[i]))
                    break;
            }

            ToolEditorStyles.EndBox();
        }

        private void DrawRecipeListHeader()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

            EditorGUILayout.LabelField("Recipe / Part", ToolEditorStyles.SubHeader);
            EditorGUILayout.LabelField("Tool", ToolEditorStyles.SubHeader, GUILayout.Width(ToolColumnWidth));
            EditorGUILayout.LabelField("Slot", ToolEditorStyles.SubHeader, GUILayout.Width(SlotColumnWidth));
            EditorGUILayout.LabelField("Level", ToolEditorStyles.SubHeader, GUILayout.Width(LevelColumnWidth));
            EditorGUILayout.LabelField("Costs", ToolEditorStyles.SubHeader, GUILayout.Width(CostColumnWidth));
            EditorGUILayout.LabelField("Actions", ToolEditorStyles.SubHeader, GUILayout.Width(ActionsColumnWidth));

            EditorGUILayout.EndHorizontal();
        }

        private bool DrawRecipeRow(RecipeRowView row)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            if (row.Recipe == null)
            {
                var removedNull = DrawNullRecipeRow(row);
                EditorGUILayout.EndVertical();
                return removedNull;
            }

            EditorGUILayout.BeginHorizontal();

            DrawRecipeMainCell(row);
            DrawTextCell(row.ToolId, ToolColumnWidth);
            DrawTextCell(row.SlotId, SlotColumnWidth);
            DrawTextCell($"{row.FromLevel} → {row.ToLevel}", LevelColumnWidth);
            DrawTextCell(row.CostCount.ToString(), CostColumnWidth);

            var removed = DrawRecipeActions(row);

            EditorGUILayout.EndHorizontal();

            DrawRecipeWarnings(row);

            EditorGUILayout.EndVertical();
            return removed;
        }

        private bool DrawNullRecipeRow(RecipeRowView row)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField($"Entry {row.OriginalIndex + 1}", ToolEditorStyles.SubHeader);
            EditorGUILayout.LabelField("Null recipe reference", ToolEditorStyles.Muted);
            EditorGUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Remove", GUILayout.Width(RemoveButtonWidth)))
            {
                RemoveRecipeAt(row.OriginalIndex);
                EditorGUILayout.EndHorizontal();
                return true;
            }

            EditorGUILayout.EndHorizontal();
            return false;
        }

        private void DrawRecipeMainCell(RecipeRowView row)
        {
            var recipeProperty = recipes.GetArrayElementAtIndex(row.OriginalIndex);

            EditorGUILayout.BeginVertical(GUILayout.MinWidth(260));

            EditorGUI.BeginChangeCheck();
            var next = (ToolUpgradeRecipeDefinition)EditorGUILayout.ObjectField(
                GUIContent.none,
                recipeProperty.objectReferenceValue,
                typeof(ToolUpgradeRecipeDefinition),
                false);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Change Tool Upgrade Recipe Reference");
                recipeProperty.objectReferenceValue = next;
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
                BuildRows();
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(row.DisplayName, ToolEditorStyles.SubHeader);

            if (!string.IsNullOrWhiteSpace(row.PartDisplayName))
                EditorGUILayout.LabelField(row.PartDisplayName, ToolEditorStyles.Muted);

            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrWhiteSpace(row.PartItemId))
                EditorGUILayout.LabelField(row.PartItemId, ToolEditorStyles.Muted);

            EditorGUILayout.EndVertical();
        }

        private static void DrawTextCell(string value, int width)
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(width));
            GUILayout.Space(2f);
            EditorGUILayout.LabelField(string.IsNullOrWhiteSpace(value) ? "-" : value, ToolEditorStyles.Muted);
            EditorGUILayout.EndVertical();
        }

        private bool DrawRecipeActions(RecipeRowView row)
        {
            EditorGUILayout.BeginHorizontal(GUILayout.Width(ActionsColumnWidth));

            if (GUILayout.Button("Select", GUILayout.Width(RowButtonWidth)))
                Selection.activeObject = row.Recipe;

            if (GUILayout.Button("Ping", GUILayout.Width(RowButtonWidth)))
                EditorGUIUtility.PingObject(row.Recipe);

            if (GUILayout.Button("Remove", GUILayout.Width(RemoveButtonWidth)))
            {
                RemoveRecipeAt(row.OriginalIndex);
                EditorGUILayout.EndHorizontal();
                return true;
            }

            EditorGUILayout.EndHorizontal();
            return false;
        }

        private static void DrawRecipeWarnings(RecipeRowView row)
        {
            if (!row.HasIssue)
                return;

            if (row.HasInvalidLevel)
                EditorGUILayout.HelpBox("ToLevel must be greater than FromLevel.", MessageType.Error);

            if (row.HasEmptyToolId)
                EditorGUILayout.HelpBox("ToolId is empty.", MessageType.Warning);

            if (row.HasEmptySlotId)
                EditorGUILayout.HelpBox("SlotId is empty.", MessageType.Warning);

            if (row.HasEmptyPartItemId)
                EditorGUILayout.HelpBox("Part ItemId is empty.", MessageType.Warning);

            if (row.HasMissingPartItem)
                EditorGUILayout.HelpBox("Part item was not found in project item assets.", MessageType.Warning);

            if (row.HasNoCosts)
                EditorGUILayout.HelpBox("Recipe has no item costs.", MessageType.Warning);

            if (row.HasDuplicateKey)
                EditorGUILayout.HelpBox("Duplicate recipe key exists in this catalog.", MessageType.Warning);
        }

        private void DrawRawRecipeList()
        {
            ToolEditorStyles.BeginBox();

            showRawList = EditorGUILayout.Foldout(showRawList, "Raw Serialized Recipe List", true);
            if (showRawList)
                EditorGUILayout.PropertyField(recipes, new GUIContent("Recipe References"), true);

            ToolEditorStyles.EndBox();
        }

        private void DrawValidation()
        {
            ToolEditorStyles.BeginBox();
            EditorGUILayout.LabelField("Validation", ToolEditorStyles.Header);

            if (!hasValidated)
            {
                EditorGUILayout.HelpBox("Run validation to check recipe keys, levels, costs, and item references.",
                    MessageType.Info);
                ToolEditorStyles.EndBox();
                return;
            }

            if (validationMessages.Count == 0)
            {
                EditorGUILayout.HelpBox("No validation issues found.", MessageType.Info);
                ToolEditorStyles.EndBox();
                return;
            }

            showValidationMessages =
                EditorGUILayout.Foldout(showValidationMessages, $"Issues ({validationMessages.Count})", true);
            if (showValidationMessages)
            {
                EditorGUI.indentLevel++;
                for (var i = 0; i < validationMessages.Count; i++)
                    EditorGUILayout.HelpBox(validationMessages[i], MessageType.Warning);

                EditorGUI.indentLevel--;
            }

            ToolEditorStyles.EndBox();
        }

        private void BuildRows()
        {
            allRows.Clear();
            visibleRows.Clear();
            BuildDuplicateKeys();

            for (var i = 0; i < recipes.arraySize; i++)
            {
                var row = CreateRow(i);
                allRows.Add(row);

                if (!MatchesFilters(row))
                    continue;

                visibleRows.Add(row);
            }

            SortVisibleRows();
        }

        private void BuildDuplicateKeys()
        {
            duplicateKeys.Clear();
            keyCounts.Clear();

            for (var i = 0; i < recipes.arraySize; i++)
            {
                var recipe = recipes.GetArrayElementAtIndex(i).objectReferenceValue as ToolUpgradeRecipeDefinition;
                if (recipe == null)
                    continue;

                var key = BuildRecipeKey(recipe);
                if (keyCounts.TryGetValue(key, out var count))
                    keyCounts[key] = count + 1;
                else
                    keyCounts.Add(key, 1);
            }

            foreach (var pair in keyCounts)
            {
                if (pair.Value <= 1)
                    continue;

                duplicateKeys.Add(pair.Key);
            }
        }

        private RecipeRowView CreateRow(int index)
        {
            var recipe = recipes.GetArrayElementAtIndex(index).objectReferenceValue as ToolUpgradeRecipeDefinition;
            if (recipe == null)
                return RecipeRowView.CreateNull(index);

            var partDisplayName = string.Empty;
            var hasMissingPartItem = false;

            if (recipe.PartItemId.IsValid)
            {
                if (ToolEditorItemLookup.TryGet(recipe.PartItemId, out var itemDefinition) && itemDefinition != null)
                    partDisplayName = GetItemDisplayName(itemDefinition);
                else
                    hasMissingPartItem = true;
            }

            var key = BuildRecipeKey(recipe);
            return new RecipeRowView
            {
                OriginalIndex = index,
                Recipe = recipe,
                DisplayName = string.IsNullOrWhiteSpace(recipe.name) ? $"Recipe {index + 1}" : recipe.name,
                ToolId = recipe.ToolId.ToString(),
                SlotId = recipe.SlotId.ToString(),
                PartItemId = recipe.PartItemId.ToString(),
                PartDisplayName = string.IsNullOrWhiteSpace(partDisplayName)
                    ? recipe.PartItemId.ToString()
                    : partDisplayName,
                FromLevel = recipe.FromLevel,
                ToLevel = recipe.ToLevel,
                CostCount = recipe.ItemCosts.Count,
                HasEmptyToolId = !recipe.ToolId.IsValid,
                HasEmptySlotId = !recipe.SlotId.IsValid,
                HasEmptyPartItemId = !recipe.PartItemId.IsValid,
                HasMissingPartItem = hasMissingPartItem,
                HasInvalidLevel = recipe.ToLevel <= recipe.FromLevel,
                HasNoCosts = recipe.ItemCosts.Count == 0,
                HasDuplicateKey = duplicateKeys.Contains(key)
            };
        }

        private bool MatchesFilters(RecipeRowView row)
        {
            if (issueFilter == RecipeIssueFilter.OnlyIssues && !row.HasIssue)
                return false;

            if (issueFilter == RecipeIssueFilter.OnlyValid && row.HasIssue)
                return false;

            if (!MatchesStringFilter(selectedToolFilter, row.ToolId))
                return false;

            if (!MatchesStringFilter(selectedSlotFilter, row.SlotId))
                return false;

            if (!MatchesStringFilter(selectedPartFilter, row.PartItemId))
                return false;

            return MatchesSearch(row);
        }

        private bool MatchesSearch(RecipeRowView row)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return true;

            var search = searchText.Trim();

            return Contains(row.DisplayName, search)
                   || Contains(row.ToolId, search)
                   || Contains(row.SlotId, search)
                   || Contains(row.PartItemId, search)
                   || Contains(row.PartDisplayName, search)
                   || Contains($"{row.FromLevel}", search)
                   || Contains($"{row.ToLevel}", search)
                   || Contains($"L{row.FromLevel}", search)
                   || Contains($"L{row.ToLevel}", search)
                   || Contains($"{row.FromLevel}->{row.ToLevel}", search)
                   || Contains($"{row.FromLevel} -> {row.ToLevel}", search);
        }

        private static bool MatchesStringFilter(string filter, string value)
        {
            if (filter == AllFilterOption)
                return true;

            if (filter == EmptyFilterOption)
                return string.IsNullOrWhiteSpace(value);

            return string.Equals(filter, value, StringComparison.Ordinal);
        }

        private static bool Contains(string value, string search)
        {
            return !string.IsNullOrWhiteSpace(value)
                   && value.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void SortVisibleRows()
        {
            visibleRows.Sort(CompareRows);
        }

        private int CompareRows(RecipeRowView left, RecipeRowView right)
        {
            if (left.Recipe == null && right.Recipe == null)
                return left.OriginalIndex.CompareTo(right.OriginalIndex);

            if (left.Recipe == null)
                return 1;

            if (right.Recipe == null)
                return -1;

            switch (sortMode)
            {
                case RecipeSortMode.AssetName:
                    return CompareText(left.DisplayName, right.DisplayName);

                case RecipeSortMode.PartLevel:
                    return ComparePartLevel(left, right);

                case RecipeSortMode.OriginalOrder:
                    return left.OriginalIndex.CompareTo(right.OriginalIndex);

                case RecipeSortMode.ToolSlotPartLevel:
                default:
                    return CompareToolSlotPartLevel(left, right);
            }
        }

        private static int CompareToolSlotPartLevel(RecipeRowView left, RecipeRowView right)
        {
            var tool = CompareText(left.ToolId, right.ToolId);
            if (tool != 0)
                return tool;

            var slot = CompareText(left.SlotId, right.SlotId);
            if (slot != 0)
                return slot;

            return ComparePartLevel(left, right);
        }

        private static int ComparePartLevel(RecipeRowView left, RecipeRowView right)
        {
            var part = CompareText(left.PartDisplayName, right.PartDisplayName);
            if (part != 0)
                return part;

            var from = left.FromLevel.CompareTo(right.FromLevel);
            if (from != 0)
                return from;

            return left.ToLevel.CompareTo(right.ToLevel);
        }

        private static int CompareText(string left, string right)
        {
            return string.Compare(left, right, StringComparison.OrdinalIgnoreCase);
        }

        private string[] BuildToolOptions()
        {
            return BuildOptions(row => row.ToolId);
        }

        private string[] BuildSlotOptions()
        {
            return BuildOptions(row => row.SlotId);
        }

        private string[] BuildPartOptions()
        {
            return BuildOptions(row => row.PartItemId);
        }

        private string[] BuildOptions(Func<RecipeRowView, string> selector)
        {
            var options = new List<string> { AllFilterOption };
            var seen = new HashSet<string>();

            for (var i = 0; i < allRows.Count; i++)
            {
                var value = selector(allRows[i]);
                if (string.IsNullOrWhiteSpace(value))
                    continue;

                if (!seen.Add(value))
                    continue;

                options.Add(value);
            }

            options.Sort(1, options.Count - 1, StringComparer.OrdinalIgnoreCase);

            if (HasEmptyOption(selector))
                options.Add(EmptyFilterOption);

            return options.ToArray();
        }

        private bool HasEmptyOption(Func<RecipeRowView, string> selector)
        {
            for (var i = 0; i < allRows.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(selector(allRows[i])))
                    return true;
            }

            return false;
        }

        private static string DrawStringPopup(string label, string current, string[] options)
        {
            var index = Mathf.Max(0, Array.IndexOf(options, current));
            index = EditorGUILayout.Popup(label, index, options);
            return options[index];
        }

        private void ClearFilters()
        {
            searchText = string.Empty;
            selectedToolFilter = AllFilterOption;
            selectedSlotFilter = AllFilterOption;
            selectedPartFilter = AllFilterOption;
            issueFilter = RecipeIssueFilter.All;
            BuildRows();
        }

        private int CountNullRows()
        {
            var count = 0;

            for (var i = 0; i < recipes.arraySize; i++)
            {
                if (recipes.GetArrayElementAtIndex(i).objectReferenceValue == null)
                    count++;
            }

            return count;
        }

        private void RemoveRecipeAt(int index)
        {
            if (index < 0 || index >= recipes.arraySize)
                return;

            Undo.RecordObject(target, "Remove Tool Upgrade Recipe");
            var element = recipes.GetArrayElementAtIndex(index);

            if (element.objectReferenceValue != null)
                element.objectReferenceValue = null;

            recipes.DeleteArrayElementAtIndex(index);
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
            BuildRows();
        }

        private void RemoveNullRecipes()
        {
            Undo.RecordObject(target, "Remove Null Tool Upgrade Recipes");

            for (var i = recipes.arraySize - 1; i >= 0; i--)
            {
                if (recipes.GetArrayElementAtIndex(i).objectReferenceValue != null)
                    continue;

                recipes.DeleteArrayElementAtIndex(i);
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
            BuildRows();
        }

        private void SortCatalogByKey()
        {
            var sortedRecipes = new List<ToolUpgradeRecipeDefinition>();

            for (var i = 0; i < recipes.arraySize; i++)
                sortedRecipes.Add(
                    recipes.GetArrayElementAtIndex(i).objectReferenceValue as ToolUpgradeRecipeDefinition);

            sortedRecipes.Sort(CompareRecipeAssets);

            Undo.RecordObject(target, "Sort Tool Upgrade Recipe Catalog");

            for (var i = 0; i < sortedRecipes.Count; i++)
                recipes.GetArrayElementAtIndex(i).objectReferenceValue = sortedRecipes[i];

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
            BuildRows();
        }

        private static int CompareRecipeAssets(ToolUpgradeRecipeDefinition left, ToolUpgradeRecipeDefinition right)
        {
            if (left == null && right == null)
                return 0;

            if (left == null)
                return 1;

            if (right == null)
                return -1;

            var tool = CompareText(left.ToolId.ToString(), right.ToolId.ToString());
            if (tool != 0)
                return tool;

            var slot = CompareText(left.SlotId.ToString(), right.SlotId.ToString());
            if (slot != 0)
                return slot;

            var part = CompareText(left.PartItemId.ToString(), right.PartItemId.ToString());
            if (part != 0)
                return part;

            var from = left.FromLevel.CompareTo(right.FromLevel);
            if (from != 0)
                return from;

            return left.ToLevel.CompareTo(right.ToLevel);
        }

        private void Validate()
        {
            var catalog = (ToolUpgradeRecipeCatalog)target;
            validation = ToolEditorValidation.ValidateCatalog(catalog, validationMessages);
            hasValidated = true;
            BuildRows();
        }

        private static string BuildRecipeKey(ToolUpgradeRecipeDefinition recipe)
        {
            return $"{recipe.ToolId}|{recipe.SlotId}|{recipe.PartItemId}|{recipe.FromLevel}|{recipe.ToLevel}";
        }

        private static string GetItemDisplayName(ItemDefinition itemDefinition)
        {
            if (itemDefinition == null)
                return string.Empty;

            return string.IsNullOrWhiteSpace(itemDefinition.DisplayName)
                ? itemDefinition.name
                : itemDefinition.DisplayName;
        }

        private enum RecipeIssueFilter
        {
            All,
            OnlyIssues,
            OnlyValid
        }

        private enum RecipeSortMode
        {
            ToolSlotPartLevel,
            PartLevel,
            AssetName,
            OriginalOrder
        }

        private sealed class RecipeRowView
        {
            public int OriginalIndex;
            public ToolUpgradeRecipeDefinition Recipe;
            public string DisplayName;
            public string ToolId;
            public string SlotId;
            public string PartItemId;
            public string PartDisplayName;
            public int FromLevel;
            public int ToLevel;
            public int CostCount;
            public bool HasEmptyToolId;
            public bool HasEmptySlotId;
            public bool HasEmptyPartItemId;
            public bool HasMissingPartItem;
            public bool HasInvalidLevel;
            public bool HasNoCosts;
            public bool HasDuplicateKey;

            public bool HasIssue =>
                Recipe == null
                || HasEmptyToolId
                || HasEmptySlotId
                || HasEmptyPartItemId
                || HasMissingPartItem
                || HasInvalidLevel
                || HasNoCosts
                || HasDuplicateKey;

            public static RecipeRowView CreateNull(int index)
            {
                return new RecipeRowView
                {
                    OriginalIndex = index,
                    DisplayName = $"Null Entry {index}",
                    ToolId = string.Empty,
                    SlotId = string.Empty,
                    PartItemId = string.Empty,
                    PartDisplayName = string.Empty
                };
            }
        }
    }
}