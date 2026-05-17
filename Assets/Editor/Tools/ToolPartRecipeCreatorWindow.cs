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
    /// Editor window for creating permanent tool part item assets and upgrade recipes.
    /// </summary>
    public sealed class ToolPartRecipeCreatorWindow : EditorWindow
    {
        private const string MenuPath = "Tools/IdleScaper/Tool Part & Recipe Creator";

        private readonly ToolPartRecipeCreatorState state = new ToolPartRecipeCreatorState();
        private readonly List<ToolRecipeRow> activeRecipeRows = new List<ToolRecipeRow>();
        private readonly List<string> activeRecipePaths = new List<string>();

        private Vector2 scroll;
        private ToolPartRecipeCreatorValidation validation = new ToolPartRecipeCreatorValidation();
        private ToolPartRecipeCreatorResult lastResult;
        private bool showBonuses = true;
        private bool showRecipes = true;
        private bool showPaths = true;

        /// <summary>
        /// Opens the tool part and recipe creator window.
        /// </summary>
        [MenuItem(MenuPath)]
        public static void Open()
        {
            GetWindow<ToolPartRecipeCreatorWindow>("Tool Part & Recipe Creator");
        }

        private void OnEnable()
        {
            ReloadDatabases();
            GenerateDefaultChain();
        }

        private void OnGUI()
        {
            RefreshDerivedData();
            scroll = EditorGUILayout.BeginScrollView(scroll);

            DrawHeader();
            ToolEditorStyles.Space();
            DrawToolContext();
            ToolEditorStyles.Space();
            DrawOutputPaths();
            ToolEditorStyles.Space();
            DrawPartCreation();
            ToolEditorStyles.Space();
            DrawBonusSetup();
            ToolEditorStyles.Space();
            DrawRecipeCreation();
            ToolEditorStyles.Space();
            DrawRegistration();
            ToolEditorStyles.Space();
            DrawValidationSummary();
            ToolEditorStyles.Space();
            DrawCreationSummary();

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            ToolEditorStyles.BeginBox();
            EditorGUILayout.LabelField("Tool Part & Recipe Creator", ToolEditorStyles.Header);
            EditorGUILayout.LabelField("Creates permanent tool part ItemDefinitions, ToolPartModules, and optional upgrade recipes.", EditorStyles.wordWrappedMiniLabel);
            if (GUILayout.Button("Refresh / Reload Databases"))
                ReloadDatabases();

            var slot = GetSelectedSlot();
            if (state.ToolDefinition != null && slot != null)
                EditorGUILayout.LabelField("Context", $"{state.ToolDefinition.DisplayName} / {slot.DisplayName}", ToolEditorStyles.Muted);

            ToolEditorStyles.EndBox();
        }

        private void DrawToolContext()
        {
            ToolEditorStyles.BeginBox();
            EditorGUILayout.LabelField("Tool Context", ToolEditorStyles.Header);
            state.CreationMode = (ToolContentCreationMode)EditorGUILayout.EnumPopup("Creation Flow", state.CreationMode);
            state.ItemDatabase = (ItemDatabase)EditorGUILayout.ObjectField("Item Database", state.ItemDatabase, typeof(ItemDatabase), false);
            EditorGUI.BeginChangeCheck();
            state.ToolDefinition = (ToolDefinition)EditorGUILayout.ObjectField("Tool Definition", state.ToolDefinition, typeof(ToolDefinition), false);
            if (EditorGUI.EndChangeCheck())
            {
                state.SlotIndex = 0;
                AutoFillFolders();
                AutoSelectRecipeCatalog();
            }

            state.RecipeCatalog = (ToolUpgradeRecipeCatalog)EditorGUILayout.ObjectField("Recipe Catalog", state.RecipeCatalog, typeof(ToolUpgradeRecipeCatalog), false);
            DrawSlotPopup();
            EditorGUILayout.LabelField("ToolId", state.ToolDefinition != null ? state.ToolDefinition.Id.ToString() : string.Empty);
            EditorGUILayout.LabelField("SlotId", GetSelectedSlot() != null ? GetSelectedSlot().Id.ToString() : string.Empty);
            ToolEditorStyles.EndBox();
        }

        private void DrawSlotPopup()
        {
            var slots = state.ToolDefinition != null ? state.ToolDefinition.SupportedSlots : null;
            if (slots == null || slots.Count == 0)
            {
                EditorGUILayout.HelpBox("Select a ToolDefinition with supported slots.", MessageType.Info);
                return;
            }

            var names = new string[slots.Count];
            for (var i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];
                names[i] = slot == null ? "<Missing Slot>" : $"{slot.DisplayName} ({slot.Id})";
            }

            state.SlotIndex = Mathf.Clamp(state.SlotIndex, 0, slots.Count - 1);
            state.SlotIndex = EditorGUILayout.Popup("Slot", state.SlotIndex, names);
        }

        private void DrawOutputPaths()
        {
            ToolEditorStyles.BeginBox();
            showPaths = EditorGUILayout.Foldout(showPaths, "Output Paths", true);
            if (showPaths)
            {
                if (state.CreationMode != ToolContentCreationMode.RecipesOnly)
                {
                    state.PartFolder = EditorGUILayout.TextField("Part Folder", state.PartFolder);
                    state.ModuleFolder = EditorGUILayout.TextField("Module Folder", state.ModuleFolder);
                }

                state.RecipeFolder = EditorGUILayout.TextField("Recipe Folder", state.RecipeFolder);
                if (state.CreationMode != ToolContentCreationMode.RecipesOnly)
                {
                    EditorGUILayout.LabelField("Item", GetItemPath(), ToolEditorStyles.Muted);
                    EditorGUILayout.LabelField("Module", GetModulePath(), ToolEditorStyles.Muted);
                }

                for (var i = 0; i < activeRecipePaths.Count; i++)
                    EditorGUILayout.LabelField($"Recipe {i + 1}", activeRecipePaths[i], ToolEditorStyles.Muted);
            }

            ToolEditorStyles.EndBox();
        }

        private void DrawPartCreation()
        {
            if (state.CreationMode == ToolContentCreationMode.RecipesOnly)
                return;

            ToolEditorStyles.BeginBox();
            EditorGUILayout.LabelField("Part Creation", ToolEditorStyles.Header);
            EditorGUI.BeginChangeCheck();
            state.PartDisplayName = EditorGUILayout.TextField("Display Name", state.PartDisplayName);
            if (EditorGUI.EndChangeCheck() && !state.UseManualItemId)
                state.ManualItemId = string.Empty;

            state.UseManualItemId = EditorGUILayout.Toggle("Manual ItemId", state.UseManualItemId);
            if (state.UseManualItemId)
                state.ManualItemId = EditorGUILayout.TextField("ItemId", state.ManualItemId);

            EditorGUILayout.LabelField("Generated ItemId", GetItemId(), ToolEditorStyles.Muted);
            state.Description = EditorGUILayout.TextField("Description", state.Description);
            state.Icon = (Sprite)EditorGUILayout.ObjectField("Icon", state.Icon, typeof(Sprite), false);
            state.Stackable = EditorGUILayout.Toggle("Stackable", state.Stackable);
            state.MaxStackSize = EditorGUILayout.IntField("Max Stack Size", state.MaxStackSize);
            state.MaxUpgradeLevel = EditorGUILayout.IntField("Max Upgrade Level", state.MaxUpgradeLevel);
            state.TemplatePart = (ItemDefinition)EditorGUILayout.ObjectField("Template Part", state.TemplatePart, typeof(ItemDefinition), false);
            if (state.TemplatePart != null && GUILayout.Button("Copy Template Defaults"))
                CopyTemplateDefaults();

            ToolEditorStyles.EndBox();
        }

        private void DrawBonusSetup()
        {
            if (state.CreationMode == ToolContentCreationMode.RecipesOnly)
                return;

            ToolEditorStyles.BeginBox();
            showBonuses = EditorGUILayout.Foldout(showBonuses, $"Bonus Setup ({state.Bonuses.Count})", true);
            if (showBonuses)
            {
                for (var i = 0; i < state.Bonuses.Count; i++)
                    DrawBonusRow(i);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Add Bonus"))
                    state.Bonuses.Add(new ToolBonusValue(ToolBonusType.MiningDamageFlat, 0f, 0f));

                GUI.enabled = state.Bonuses.Count > 0;
                if (GUILayout.Button("Remove Last"))
                    state.Bonuses.RemoveAt(state.Bonuses.Count - 1);

                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();
            }

            ToolEditorStyles.EndBox();
        }

        private void DrawBonusRow(int index)
        {
            var bonus = state.Bonuses[index];
            ToolEditorStyles.BeginBox();
            var type = (ToolBonusType)EditorGUILayout.EnumPopup("Type", bonus.Type);
            var baseValue = EditorGUILayout.FloatField("Base Value", bonus.BaseValue);
            var valuePerLevel = EditorGUILayout.FloatField("Value Per Level", bonus.ValuePerLevel);
            state.Bonuses[index] = new ToolBonusValue(type, baseValue, valuePerLevel);
            EditorGUILayout.LabelField("Preview", $"L0 {FormatBonus(type, baseValue)} / L{state.MaxUpgradeLevel} {FormatBonus(type, baseValue + Mathf.Max(0, state.MaxUpgradeLevel) * valuePerLevel)}", ToolEditorStyles.Muted);
            ToolEditorStyles.EndBox();
        }

        private void DrawRecipeCreation()
        {
            if (state.CreationMode == ToolContentCreationMode.ToolPartOnly)
                return;

            ToolEditorStyles.BeginBox();
            showRecipes = EditorGUILayout.Foldout(showRecipes, "Recipe Creation", true);
            if (showRecipes)
            {
                state.RecipeMode = (ToolRecipeCreationMode)EditorGUILayout.EnumPopup("Mode", state.RecipeMode);
                if (state.RecipeMode == ToolRecipeCreationMode.NoRecipes)
                {
                    ToolEditorStyles.EndBox();
                    return;
                }

                state.ExistingRecipePart = (ItemDefinition)EditorGUILayout.ObjectField("Existing Part", state.ExistingRecipePart, typeof(ItemDefinition), false);
                if (state.RecipeMode == ToolRecipeCreationMode.SingleRecipe)
                    DrawSingleRecipeFields();
                else
                    DrawRecipeChainFields();
            }

            ToolEditorStyles.EndBox();
        }

        private void DrawSingleRecipeFields()
        {
            state.SingleFromLevel = EditorGUILayout.IntField("From Level", state.SingleFromLevel);
            state.SingleToLevel = EditorGUILayout.IntField("To Level", state.SingleToLevel);
            DrawCostList("Costs", state.SharedCosts);
        }

        private void DrawRecipeChainFields()
        {
            state.ChainStartLevel = EditorGUILayout.IntField("Start Level", state.ChainStartLevel);
            state.ChainMaxLevel = EditorGUILayout.IntField("Max Level", state.ChainMaxLevel);
            if (GUILayout.Button("Generate Default Chain"))
                GenerateDefaultChain();

            for (var i = 0; i < state.RecipeRows.Count; i++)
            {
                var row = state.RecipeRows[i];
                ToolEditorStyles.BeginBox();
                EditorGUILayout.LabelField($"Recipe L{row.FromLevel}-L{row.ToLevel}", ToolEditorStyles.SubHeader);
                row.FromLevel = EditorGUILayout.IntField("From", row.FromLevel);
                row.ToLevel = EditorGUILayout.IntField("To", row.ToLevel);
                DrawCostList("Costs", row.Costs);
                ToolEditorStyles.EndBox();
            }
        }

        private void DrawCostList(string label, List<ToolRecipeCostRow> costs)
        {
            EditorGUILayout.LabelField(label, ToolEditorStyles.SubHeader);
            for (var i = 0; i < costs.Count; i++)
            {
                var cost = costs[i];
                EditorGUILayout.BeginHorizontal();
                cost.Item = (ItemDefinition)EditorGUILayout.ObjectField(cost.Item, typeof(ItemDefinition), false);
                cost.Quantity = EditorGUILayout.IntField(cost.Quantity, GUILayout.Width(60));
                DrawCostPreview(cost.Item);
                if (GUILayout.Button("-", GUILayout.Width(24)))
                {
                    costs.RemoveAt(i);
                    i--;
                }

                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button($"Add {label} Row"))
                costs.Add(new ToolRecipeCostRow());
        }

        private static void DrawCostPreview(ItemDefinition item)
        {
            if (item == null)
            {
                EditorGUILayout.LabelField("Missing item", ToolEditorStyles.Muted, GUILayout.Width(140));
                return;
            }

            GUILayout.Label(ToolEditorStyles.GetIconTexture(item.Icon), GUILayout.Width(20), GUILayout.Height(20));
            EditorGUILayout.LabelField($"{item.DisplayName} ({item.Id})", ToolEditorStyles.Muted);
        }

        private void DrawRegistration()
        {
            ToolEditorStyles.BeginBox();
            EditorGUILayout.LabelField("Registration", ToolEditorStyles.Header);
            if (state.CreationMode != ToolContentCreationMode.RecipesOnly)
                state.RegisterItem = EditorGUILayout.Toggle("Register Item", state.RegisterItem);

            if (state.CreationMode != ToolContentCreationMode.ToolPartOnly)
                state.RegisterRecipes = EditorGUILayout.Toggle("Register Recipes", state.RegisterRecipes);

            EditorGUILayout.LabelField("Item Database", state.ItemDatabase != null ? state.ItemDatabase.name : "None", ToolEditorStyles.Muted);
            EditorGUILayout.LabelField("Recipe Catalog", state.RecipeCatalog != null ? state.RecipeCatalog.name : "None", ToolEditorStyles.Muted);
            ToolEditorStyles.EndBox();
        }

        private void DrawValidationSummary()
        {
            ToolEditorStyles.BeginBox();
            EditorGUILayout.LabelField("Validation Summary", ToolEditorStyles.Header);
            EditorGUILayout.LabelField("ItemId", GetItemId(), ToolEditorStyles.Muted);
            DrawMessages(validation.Errors, MessageType.Error);
            DrawMessages(validation.Warnings, MessageType.Warning);
            GUI.enabled = validation.CanCreate;
            if (GUILayout.Button(GetCreateButtonLabel(), GUILayout.Height(28)))
                Create();

            GUI.enabled = true;
            ToolEditorStyles.EndBox();
        }

        private void DrawCreationSummary()
        {
            if (lastResult == null)
                return;

            ToolEditorStyles.BeginBox();
            EditorGUILayout.LabelField("Creation Summary", ToolEditorStyles.Header);
            DrawCreatedObject("Item", lastResult.ItemDefinition);
            DrawCreatedObject("Module", lastResult.ToolPartModule);
            for (var i = 0; i < lastResult.Recipes.Count; i++)
                DrawCreatedObject($"Recipe {i + 1}", lastResult.Recipes[i]);

            if (lastResult.UpdatedItemDatabase)
                EditorGUILayout.LabelField("Updated ItemDatabase", state.ItemDatabase.name);

            if (lastResult.UpdatedRecipeCatalog)
                EditorGUILayout.LabelField("Updated RecipeCatalog", state.RecipeCatalog.name);

            DrawMessages(lastResult.Messages, MessageType.Warning);
            ToolEditorStyles.EndBox();
        }

        private static void DrawCreatedObject(string label, Object value)
        {
            if (value == null)
                return;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.ObjectField(label, value, value.GetType(), false);
            if (GUILayout.Button("Ping", GUILayout.Width(48)))
                EditorGUIUtility.PingObject(value);

            EditorGUILayout.EndHorizontal();
        }

        private static void DrawMessages(IReadOnlyList<string> messages, MessageType type)
        {
            for (var i = 0; i < messages.Count; i++)
                EditorGUILayout.HelpBox(messages[i], type);
        }

        private void Create()
        {
            RefreshDerivedData();
            if (!validation.CanCreate)
                return;

            lastResult = ToolPartRecipeCreatorAssetFactory.Create(
                state,
                GetSelectedSlot(),
                GetItemId(),
                GetItemPath(),
                GetModulePath(),
                activeRecipeRows,
                activeRecipePaths);
        }

        private void RefreshDerivedData()
        {
            if (state.ChainMaxLevel != state.MaxUpgradeLevel && state.RecipeRows.Count == 0)
                state.ChainMaxLevel = state.MaxUpgradeLevel;

            BuildActiveRecipeRows();
            BuildActiveRecipePaths();
            validation = ToolPartRecipeCreatorValidator.Validate(
                state,
                GetSelectedSlot(),
                GetItemId(),
                GetItemPath(),
                GetModulePath(),
                activeRecipeRows,
                activeRecipePaths);
        }

        private void BuildActiveRecipeRows()
        {
            activeRecipeRows.Clear();
            if (state.CreationMode == ToolContentCreationMode.ToolPartOnly)
                return;

            if (state.RecipeMode == ToolRecipeCreationMode.NoRecipes)
                return;

            if (state.RecipeMode == ToolRecipeCreationMode.SingleRecipe)
            {
                var row = new ToolRecipeRow { FromLevel = state.SingleFromLevel, ToLevel = state.SingleToLevel };
                CopyCosts(state.SharedCosts, row.Costs);
                activeRecipeRows.Add(row);
                return;
            }

            for (var i = 0; i < state.RecipeRows.Count; i++)
                activeRecipeRows.Add(state.RecipeRows[i]);
        }

        private void BuildActiveRecipePaths()
        {
            activeRecipePaths.Clear();
            for (var i = 0; i < activeRecipeRows.Count; i++)
            {
                var row = activeRecipeRows[i];
                activeRecipePaths.Add($"{state.RecipeFolder.TrimEnd('/')}/{GetRecipeAssetName(row.FromLevel, row.ToLevel)}.asset");
            }
        }

        private void GenerateDefaultChain()
        {
            state.RecipeRows.Clear();
            var start = Mathf.Max(0, state.ChainStartLevel);
            var max = Mathf.Max(start + 1, state.ChainMaxLevel);
            for (var level = start; level < max; level++)
            {
                var row = new ToolRecipeRow { FromLevel = level, ToLevel = level + 1 };
                CopyCosts(state.SharedCosts, row.Costs);
                state.RecipeRows.Add(row);
            }
        }

        private static void CopyCosts(IReadOnlyList<ToolRecipeCostRow> source, List<ToolRecipeCostRow> target)
        {
            target.Clear();
            for (var i = 0; i < source.Count; i++)
                target.Add(new ToolRecipeCostRow { Item = source[i].Item, Quantity = source[i].Quantity });
        }

        private void CopyTemplateDefaults()
        {
            if (state.TemplatePart == null)
                return;

            state.Description = state.TemplatePart.Description;
            state.Icon = state.TemplatePart.Icon;
            state.Stackable = state.TemplatePart.Stackable;
            state.MaxStackSize = state.TemplatePart.MaxStackSize;
            if (!state.TemplatePart.TryGetModule<ToolPartModule>(out var module) || module == null)
                return;

            state.MaxUpgradeLevel = module.MaxLevel;
            state.Bonuses.Clear();
            for (var i = 0; i < module.Bonuses.Count; i++)
                state.Bonuses.Add(module.Bonuses[i]);
        }

        private void ReloadDatabases()
        {
            ToolEditorItemLookup.Refresh();
            state.ItemDatabase = FindSingleAsset<ItemDatabase>() ?? state.ItemDatabase;
            state.ToolDefinition = FindSingleAsset<ToolDefinition>() ?? state.ToolDefinition;
            state.RecipeCatalog = FindSingleAsset<ToolUpgradeRecipeCatalog>() ?? state.RecipeCatalog;
            AutoFillFolders();
            AutoSelectRecipeCatalog();
        }

        private void AutoSelectRecipeCatalog()
        {
            if (state.ToolDefinition == null)
                return;

            var expectedName = $"{GetToolFolderName()} Upgrade Recipe Catalog";
            var guids = AssetDatabase.FindAssets("t:ToolUpgradeRecipeCatalog");
            for (var i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var catalog = AssetDatabase.LoadAssetAtPath<ToolUpgradeRecipeCatalog>(path);
                if (catalog == null)
                    continue;

                if (catalog.name != expectedName)
                    continue;

                state.RecipeCatalog = catalog;
                return;
            }
        }

        private void AutoFillFolders()
        {
            var toolName = GetToolFolderName();
            state.PartFolder = $"Assets/Data/Items/ToolParts/{toolName}/Parts";
            state.ModuleFolder = $"Assets/Data/Items/ToolParts/{toolName}/Modules";
            state.RecipeFolder = $"Assets/Data/Tools/{toolName}/Recipes";
        }

        private ToolPartSlotDefinition GetSelectedSlot()
        {
            var slots = state.ToolDefinition != null ? state.ToolDefinition.SupportedSlots : null;
            if (slots == null || slots.Count == 0)
                return null;

            state.SlotIndex = Mathf.Clamp(state.SlotIndex, 0, slots.Count - 1);
            return slots[state.SlotIndex];
        }

        private string GetItemId()
        {
            if (state.CreationMode == ToolContentCreationMode.RecipesOnly)
                return state.ExistingRecipePart != null ? state.ExistingRecipePart.Id.ToString() : string.Empty;

            if (state.UseManualItemId)
                return state.ManualItemId?.Trim() ?? string.Empty;

            var toolToken = GetIdToken(state.ToolDefinition != null ? state.ToolDefinition.Id.ToString().Replace("tool.", string.Empty) : "tool");
            var slot = GetSelectedSlot();
            var slotToken = GetIdToken(slot != null ? slot.Id.ToString().Substring(slot.Id.ToString().LastIndexOf('.') + 1) : "slot");
            var partToken = GetIdToken(state.PartDisplayName);
            return $"item.tool_part.{toolToken}.{slotToken}.{partToken}";
        }

        private string GetItemPath()
        {
            return $"{state.PartFolder.TrimEnd('/')}/{ToolPartRecipeCreatorAssetFactory.SanitizeAssetName(state.PartDisplayName)}.asset";
        }

        private string GetModulePath()
        {
            return $"{state.ModuleFolder.TrimEnd('/')}/{ToolPartRecipeCreatorAssetFactory.SanitizeAssetName(state.PartDisplayName)} Module.asset";
        }

        private string GetRecipeAssetName(int fromLevel, int toLevel)
        {
            return $"{ToolPartRecipeCreatorAssetFactory.SanitizeAssetName(GetRecipePartName()).Replace(' ', '_')} L{fromLevel}-L{toLevel}";
        }

        private string GetRecipePartName()
        {
            if (state.CreationMode == ToolContentCreationMode.RecipesOnly && state.ExistingRecipePart != null)
                return state.ExistingRecipePart.DisplayName;

            return state.PartDisplayName;
        }

        private string GetToolFolderName()
        {
            if (state.ToolDefinition == null)
                return "Pickaxe";

            return ToolPartRecipeCreatorAssetFactory.SanitizeAssetName(
                string.IsNullOrWhiteSpace(state.ToolDefinition.DisplayName) ? state.ToolDefinition.name : state.ToolDefinition.DisplayName);
        }

        private static string GetIdToken(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "new";

            var lower = value.Trim().ToLowerInvariant();
            var chars = lower.ToCharArray();
            for (var i = 0; i < chars.Length; i++)
            {
                if ((chars[i] >= 'a' && chars[i] <= 'z') || (chars[i] >= '0' && chars[i] <= '9'))
                    continue;

                chars[i] = '_';
            }

            return new string(chars).Trim('_');
        }

        private static T FindSingleAsset<T>() where T : Object
        {
            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            if (guids.Length != 1)
                return null;

            return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        private static string FormatBonus(ToolBonusType type, float value)
        {
            if (type == ToolBonusType.MiningDamageMultiplier
                || type == ToolBonusType.MiningSpeedMultiplier
                || type == ToolBonusType.ResourceYieldMultiplier
                || type == ToolBonusType.RareDropChanceFlat)
                return $"{value:0.####} ({value * 100f:+0.##;-0.##;0}%)";

            return value.ToString("0.####");
        }

        private string GetCreateButtonLabel()
        {
            return state.RecipeMode switch
            {
                _ when state.CreationMode == ToolContentCreationMode.ToolPartOnly => "Create Tool Part",
                ToolRecipeCreationMode.SingleRecipe when state.CreationMode == ToolContentCreationMode.RecipesOnly => "Create Recipe",
                ToolRecipeCreationMode.FullRecipeChain when state.CreationMode == ToolContentCreationMode.RecipesOnly => "Create Recipe Chain",
                ToolRecipeCreationMode.SingleRecipe => "Create Tool Part + Recipe",
                _ => "Create Tool Part + Recipe Chain"
            };
        }
    }
}
