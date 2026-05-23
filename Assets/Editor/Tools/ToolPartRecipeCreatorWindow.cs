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
        private const float MinWindowWidth = 620f;
        private const float FooterButtonHeight = 34f;
        private const float InlineButtonWidth = 94f;
        private const float SmallButtonWidth = 28f;
        private const float CostQuantityWidth = 64f;
        private const float IconPreviewSize = 24f;

        private readonly ToolPartRecipeCreatorState state = new ToolPartRecipeCreatorState();
        private readonly List<ToolRecipeRow> activeRecipeRows = new List<ToolRecipeRow>();
        private readonly List<string> activeRecipePaths = new List<string>();

        private Vector2 scroll;
        private ToolPartRecipeCreatorValidation validation = new ToolPartRecipeCreatorValidation();
        private ToolPartRecipeCreatorResult lastResult;

        private bool showRequiredSetup = true;
        private bool showPartDetails = true;
        private bool showBonuses = true;
        private bool showRecipes = true;
        private bool showOutput = false;
        private bool showCreationResult = true;
        private bool showValidationDetails = true;

        /// <summary>
        /// Opens the tool part and recipe creator window.
        /// </summary>
        [MenuItem(MenuPath)]
        public static void Open()
        {
            var window = GetWindow<ToolPartRecipeCreatorWindow>("Tool Creator");
            window.minSize = new Vector2(MinWindowWidth, 520f);
            window.Show();
        }

        private void OnEnable()
        {
            minSize = new Vector2(MinWindowWidth, 520f);
            ReloadDatabases();
            GenerateDefaultChain();
        }

        private void OnGUI()
        {
            RefreshDerivedData();

            DrawHeader();

            scroll = EditorGUILayout.BeginScrollView(scroll);
            DrawRequiredSetup();
            DrawPartDetails();
            DrawBonusSetup();
            DrawRecipeCreation();
            DrawOutputAndRegistration();
            DrawCreationSummary();
            EditorGUILayout.EndScrollView();

            DrawFooter();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Tool Creator", ToolEditorStyles.Header);
            EditorGUILayout.LabelField(GetHeaderDescription(), EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Reload", GUILayout.Width(InlineButtonWidth), GUILayout.Height(24f)))
                ReloadDatabases();

            EditorGUILayout.EndHorizontal();

            ToolEditorStyles.Space();
            DrawStatusStrip();
            EditorGUILayout.EndVertical();
        }

        private string GetHeaderDescription()
        {
            return state.CreationMode switch
            {
                ToolContentCreationMode.ToolPartOnly => "Create a permanent tool part item and its ToolPartModule.",
                ToolContentCreationMode.RecipesOnly => "Create upgrade recipes for an existing compatible tool part.",
                _ => "Create a permanent tool part item, ToolPartModule, and upgrade recipes in one flow."
            };
        }

        private void DrawStatusStrip()
        {
            EditorGUILayout.BeginHorizontal();

            DrawStatusCell("Flow", GetCreationModeLabel(state.CreationMode));
            DrawStatusCell("Tool", state.ToolDefinition != null ? GetToolDisplayName() : "Missing");
            DrawStatusCell("Slot", GetSelectedSlot() != null ? GetSelectedSlot().DisplayName : "Missing");
            DrawStatusCell("Recipes", activeRecipeRows.Count.ToString());
            DrawStatusCell("Errors", validation.Errors.Count.ToString());
            DrawStatusCell("Warnings", validation.Warnings.Count.ToString());

            EditorGUILayout.EndHorizontal();
        }

        private static void DrawStatusCell(string label, string value)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.MinWidth(74f));
            EditorGUILayout.LabelField(label, ToolEditorStyles.Muted);
            EditorGUILayout.LabelField(value, ToolEditorStyles.SubHeader);
            EditorGUILayout.EndVertical();
        }

        private void DrawRequiredSetup()
        {
            BeginSection(ref showRequiredSetup, "1. Setup", "Select what you want to create and where it belongs.");
            if (!showRequiredSetup)
                return;

            EditorGUI.BeginChangeCheck();
            state.CreationMode = (ToolContentCreationMode)EditorGUILayout.EnumPopup("Creation Flow", state.CreationMode);
            if (EditorGUI.EndChangeCheck())
                ClampCreationMode();

            state.ItemDatabase = (ItemDatabase)EditorGUILayout.ObjectField("Item Database", state.ItemDatabase, typeof(ItemDatabase), false);

            EditorGUI.BeginChangeCheck();
            state.ToolDefinition = (ToolDefinition)EditorGUILayout.ObjectField("Tool Definition", state.ToolDefinition, typeof(ToolDefinition), false);
            if (EditorGUI.EndChangeCheck())
            {
                state.SlotIndex = 0;
                AutoFillFolders();
                AutoSelectRecipeCatalog();
                GenerateDefaultChain();
            }

            DrawSlotPopup();

            if (state.CreationMode != ToolContentCreationMode.ToolPartOnly)
                state.RecipeCatalog = (ToolUpgradeRecipeCatalog)EditorGUILayout.ObjectField("Recipe Catalog", state.RecipeCatalog, typeof(ToolUpgradeRecipeCatalog), false);

            DrawContextPreview();
            EndSection();
        }

        private void ClampCreationMode()
        {
            if (state.CreationMode == ToolContentCreationMode.RecipesOnly && state.RecipeMode == ToolRecipeCreationMode.NoRecipes)
                state.RecipeMode = ToolRecipeCreationMode.SingleRecipe;
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

        private void DrawContextPreview()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Selected Context", ToolEditorStyles.SubHeader);
            EditorGUILayout.LabelField("ToolId", state.ToolDefinition != null ? state.ToolDefinition.Id.ToString() : "None", ToolEditorStyles.Muted);
            EditorGUILayout.LabelField("SlotId", GetSelectedSlot() != null ? GetSelectedSlot().Id.ToString() : "None", ToolEditorStyles.Muted);
            EditorGUILayout.EndVertical();
        }

        private void DrawPartDetails()
        {
            if (state.CreationMode == ToolContentCreationMode.RecipesOnly)
                return;

            BeginSection(ref showPartDetails, "2. Tool Part", "Define the created item and module data.");
            if (!showPartDetails)
                return;

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            EditorGUI.BeginChangeCheck();
            state.PartDisplayName = EditorGUILayout.TextField("Display Name", state.PartDisplayName);
            if (EditorGUI.EndChangeCheck() && !state.UseManualItemId)
                state.ManualItemId = string.Empty;

            state.Description = EditorGUILayout.TextField("Description", state.Description);
            state.Icon = (Sprite)EditorGUILayout.ObjectField("Icon", state.Icon, typeof(Sprite), false);
            EditorGUILayout.EndVertical();

            GUILayout.Label(ToolEditorStyles.GetIconTexture(state.Icon), GUILayout.Width(56f), GUILayout.Height(56f));
            EditorGUILayout.EndHorizontal();

            ToolEditorStyles.Space();

            EditorGUILayout.BeginHorizontal();
            state.Stackable = EditorGUILayout.Toggle("Stackable", state.Stackable);
            using (new EditorGUI.DisabledScope(!state.Stackable))
            {
                state.MaxStackSize = Mathf.Max(1, EditorGUILayout.IntField("Max Stack Size", state.MaxStackSize));
            }
            EditorGUILayout.EndHorizontal();

            state.MaxUpgradeLevel = Mathf.Max(1, EditorGUILayout.IntField("Max Upgrade Level", state.MaxUpgradeLevel));

            ToolEditorStyles.Space();
            DrawItemIdFields();
            ToolEditorStyles.Space();
            DrawTemplateActions();

            EndSection();
        }

        private void DrawItemIdFields()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            state.UseManualItemId = EditorGUILayout.Toggle("Manual ItemId", state.UseManualItemId);

            if (state.UseManualItemId)
                state.ManualItemId = EditorGUILayout.TextField("ItemId", state.ManualItemId);

            EditorGUILayout.LabelField("Final ItemId", GetItemId(), ToolEditorStyles.Muted);
            EditorGUILayout.EndVertical();
        }

        private void DrawTemplateActions()
        {
            EditorGUILayout.BeginHorizontal();
            state.TemplatePart = (ItemDefinition)EditorGUILayout.ObjectField("Template Part", state.TemplatePart, typeof(ItemDefinition), false);

            using (new EditorGUI.DisabledScope(state.TemplatePart == null))
            {
                if (GUILayout.Button("Copy Defaults", GUILayout.Width(120f)))
                    CopyTemplateDefaults();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawBonusSetup()
        {
            if (state.CreationMode == ToolContentCreationMode.RecipesOnly)
                return;

            BeginSection(ref showBonuses, $"3. Bonuses ({state.Bonuses.Count})", "Configure how this part affects the tool.");
            if (!showBonuses)
                return;

            if (state.Bonuses.Count == 0)
                EditorGUILayout.HelpBox("No bonuses configured. The part will not affect tool stats.", MessageType.Info);

            for (var i = 0; i < state.Bonuses.Count; i++)
                DrawBonusRow(i);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Add Bonus"))
                state.Bonuses.Add(new ToolBonusValue(ToolBonusType.MiningDamageFlat, 0f, 0f));

            using (new EditorGUI.DisabledScope(state.Bonuses.Count == 0))
            {
                if (GUILayout.Button("Clear Bonuses"))
                    state.Bonuses.Clear();
            }

            EditorGUILayout.EndHorizontal();
            EndSection();
        }

        private void DrawBonusRow(int index)
        {
            var bonus = state.Bonuses[index];

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField($"Bonus {index + 1}", ToolEditorStyles.SubHeader, GUILayout.Width(72f));

            var type = (ToolBonusType)EditorGUILayout.EnumPopup(bonus.Type);
            var baseValue = EditorGUILayout.FloatField(bonus.BaseValue, GUILayout.Width(80f));
            var valuePerLevel = EditorGUILayout.FloatField(bonus.ValuePerLevel, GUILayout.Width(80f));

            if (GUILayout.Button("×", GUILayout.Width(SmallButtonWidth)))
            {
                state.Bonuses.RemoveAt(index);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.EndHorizontal();

            state.Bonuses[index] = new ToolBonusValue(type, baseValue, valuePerLevel);

            var maxLevel = Mathf.Max(1, state.MaxUpgradeLevel);
            var maxValue = baseValue + maxLevel * valuePerLevel;
            EditorGUILayout.LabelField(
                "Preview",
                $"L0 {FormatBonus(type, baseValue)}   →   L{maxLevel} {FormatBonus(type, maxValue)}",
                ToolEditorStyles.Muted);

            EditorGUILayout.EndVertical();
        }

        private void DrawRecipeCreation()
        {
            if (state.CreationMode == ToolContentCreationMode.ToolPartOnly)
                return;

            BeginSection(ref showRecipes, $"4. Recipes ({activeRecipeRows.Count})", "Create a single recipe or a full upgrade chain.");
            if (!showRecipes)
                return;

            state.RecipeMode = (ToolRecipeCreationMode)EditorGUILayout.EnumPopup("Recipe Mode", state.RecipeMode);
            ClampCreationMode();

            if (state.RecipeMode == ToolRecipeCreationMode.NoRecipes)
            {
                EditorGUILayout.HelpBox("No recipe assets will be created.", MessageType.Info);
                EndSection();
                return;
            }

            if (state.CreationMode == ToolContentCreationMode.RecipesOnly)
                state.ExistingRecipePart = (ItemDefinition)EditorGUILayout.ObjectField("Existing Part", state.ExistingRecipePart, typeof(ItemDefinition), false);

            if (state.RecipeMode == ToolRecipeCreationMode.SingleRecipe)
                DrawSingleRecipeFields();
            else
                DrawRecipeChainFields();

            EndSection();
        }

        private void DrawSingleRecipeFields()
        {
            EditorGUILayout.BeginHorizontal();
            state.SingleFromLevel = Mathf.Max(0, EditorGUILayout.IntField("From Level", state.SingleFromLevel));
            state.SingleToLevel = Mathf.Max(1, EditorGUILayout.IntField("To Level", state.SingleToLevel));
            EditorGUILayout.EndHorizontal();

            DrawCostList("Costs", state.SharedCosts);
        }

        private void DrawRecipeChainFields()
        {
            EditorGUILayout.BeginHorizontal();
            state.ChainStartLevel = Mathf.Max(0, EditorGUILayout.IntField("Start Level", state.ChainStartLevel));
            state.ChainMaxLevel = Mathf.Max(state.ChainStartLevel + 1, EditorGUILayout.IntField("Max Level", state.ChainMaxLevel));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Generate Chain"))
                GenerateDefaultChain();

            if (GUILayout.Button("Copy Shared Costs To Chain"))
                CopySharedCostsToRecipeRows();

            EditorGUILayout.EndHorizontal();

            ToolEditorStyles.Space();

            DrawCostList("Shared Costs", state.SharedCosts);

            ToolEditorStyles.Space();

            if (state.RecipeRows.Count == 0)
                EditorGUILayout.HelpBox("No recipe rows generated yet.", MessageType.Info);

            for (var i = 0; i < state.RecipeRows.Count; i++)
                DrawRecipeRow(i);
        }

        private void DrawRecipeRow(int index)
        {
            var row = state.RecipeRows[index];

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField($"L{row.FromLevel} → L{row.ToLevel}", ToolEditorStyles.SubHeader, GUILayout.Width(90f));
            row.FromLevel = Mathf.Max(0, EditorGUILayout.IntField(row.FromLevel, GUILayout.Width(54f)));
            row.ToLevel = Mathf.Max(1, EditorGUILayout.IntField(row.ToLevel, GUILayout.Width(54f)));

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Duplicate", GUILayout.Width(78f)))
            {
                var clone = new ToolRecipeRow
                {
                    FromLevel = row.FromLevel,
                    ToLevel = row.ToLevel
                };

                CopyCosts(row.Costs, clone.Costs);
                state.RecipeRows.Insert(index + 1, clone);
            }

            if (GUILayout.Button("×", GUILayout.Width(SmallButtonWidth)))
            {
                state.RecipeRows.RemoveAt(index);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.EndHorizontal();

            DrawCostList("Costs", row.Costs);
            EditorGUILayout.EndVertical();
        }

        private void DrawCostList(string label, List<ToolRecipeCostRow> costs)
        {
            EditorGUILayout.LabelField(label, ToolEditorStyles.SubHeader);

            if (costs.Count == 0)
                EditorGUILayout.LabelField("No costs configured.", ToolEditorStyles.Muted);

            for (var i = 0; i < costs.Count; i++)
                DrawCostRow(costs, i);

            if (GUILayout.Button($"Add {label} Row"))
                costs.Add(new ToolRecipeCostRow());
        }

        private void DrawCostRow(List<ToolRecipeCostRow> costs, int index)
        {
            var cost = costs[index];

            EditorGUILayout.BeginHorizontal();
            cost.Item = (ItemDefinition)EditorGUILayout.ObjectField(cost.Item, typeof(ItemDefinition), false);
            cost.Quantity = Mathf.Max(1, EditorGUILayout.IntField(cost.Quantity, GUILayout.Width(CostQuantityWidth)));
            DrawCostPreview(cost.Item);

            if (GUILayout.Button("×", GUILayout.Width(SmallButtonWidth)))
            {
                costs.RemoveAt(index);
                EditorGUILayout.EndHorizontal();
                return;
            }

            EditorGUILayout.EndHorizontal();
        }

        private static void DrawCostPreview(ItemDefinition item)
        {
            if (item == null)
            {
                EditorGUILayout.LabelField("Missing item", ToolEditorStyles.Muted, GUILayout.Width(160f));
                return;
            }

            GUILayout.Label(
                ToolEditorStyles.GetIconTexture(item.Icon),
                GUILayout.Width(IconPreviewSize),
                GUILayout.Height(IconPreviewSize));

            EditorGUILayout.LabelField($"{item.DisplayName} ({item.Id})", ToolEditorStyles.Muted);
        }

        private void DrawOutputAndRegistration()
        {
            BeginSection(ref showOutput, "5. Output & Registration", "Review folders, generated paths, and database registration.");
            if (!showOutput)
                return;

            DrawRegistration();

            ToolEditorStyles.Space();

            if (state.CreationMode != ToolContentCreationMode.RecipesOnly)
            {
                DrawFolderField("Part Folder", ref state.PartFolder);
                DrawFolderField("Module Folder", ref state.ModuleFolder);
            }

            if (state.CreationMode != ToolContentCreationMode.ToolPartOnly)
                DrawFolderField("Recipe Folder", ref state.RecipeFolder);

            ToolEditorStyles.Space();
            DrawGeneratedPaths();

            EndSection();
        }

        private void DrawRegistration()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Registration", ToolEditorStyles.SubHeader);

            if (state.CreationMode != ToolContentCreationMode.RecipesOnly)
                state.RegisterItem = EditorGUILayout.Toggle("Register Item In Database", state.RegisterItem);

            if (state.CreationMode != ToolContentCreationMode.ToolPartOnly)
                state.RegisterRecipes = EditorGUILayout.Toggle("Register Recipes In Catalog", state.RegisterRecipes);

            EditorGUILayout.LabelField("Item Database", state.ItemDatabase != null ? state.ItemDatabase.name : "None", ToolEditorStyles.Muted);
            EditorGUILayout.LabelField("Recipe Catalog", state.RecipeCatalog != null ? state.RecipeCatalog.name : "None", ToolEditorStyles.Muted);
            EditorGUILayout.EndVertical();
        }

        private void DrawFolderField(string label, ref string folder)
        {
            EditorGUILayout.BeginHorizontal();
            folder = EditorGUILayout.TextField(label, folder);

            if (GUILayout.Button("Browse", GUILayout.Width(InlineButtonWidth)))
            {
                var selected = EditorUtility.OpenFolderPanel(label, "Assets", string.Empty);
                var assetPath = TryConvertToAssetPath(selected);
                if (!string.IsNullOrWhiteSpace(assetPath))
                    folder = assetPath;
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawGeneratedPaths()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Generated Paths", ToolEditorStyles.SubHeader);

            if (state.CreationMode != ToolContentCreationMode.RecipesOnly)
            {
                EditorGUILayout.LabelField("Item", GetItemPath(), ToolEditorStyles.Muted);
                EditorGUILayout.LabelField("Module", GetModulePath(), ToolEditorStyles.Muted);
            }

            for (var i = 0; i < activeRecipePaths.Count; i++)
                EditorGUILayout.LabelField($"Recipe {i + 1}", activeRecipePaths[i], ToolEditorStyles.Muted);

            EditorGUILayout.EndVertical();
        }

        private void DrawCreationSummary()
        {
            if (lastResult == null)
                return;

            BeginSection(ref showCreationResult, "Created Assets", "Last creation result.");
            if (!showCreationResult)
                return;

            DrawCreatedObject("Item", lastResult.ItemDefinition);
            DrawCreatedObject("Module", lastResult.ToolPartModule);

            for (var i = 0; i < lastResult.Recipes.Count; i++)
                DrawCreatedObject($"Recipe {i + 1}", lastResult.Recipes[i]);

            if (lastResult.UpdatedItemDatabase && state.ItemDatabase != null)
                EditorGUILayout.LabelField("Updated ItemDatabase", state.ItemDatabase.name);

            if (lastResult.UpdatedRecipeCatalog && state.RecipeCatalog != null)
                EditorGUILayout.LabelField("Updated RecipeCatalog", state.RecipeCatalog.name);

            DrawMessages(lastResult.Messages, MessageType.Warning);
            EndSection();
        }

        private void DrawFooter()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            DrawFooterValidationSummary();

            GUILayout.FlexibleSpace();

            using (new EditorGUI.DisabledScope(!validation.CanCreate))
            {
                if (GUILayout.Button(GetCreateButtonLabel(), GUILayout.Width(240f), GUILayout.Height(FooterButtonHeight)))
                    Create();
            }

            EditorGUILayout.EndHorizontal();

            if (showValidationDetails)
                DrawValidationMessages();

            EditorGUILayout.EndVertical();
        }

        private void DrawFooterValidationSummary()
        {
            EditorGUILayout.BeginVertical();

            var status = validation.CanCreate ? "Ready to create" : "Fix required fields before creating";
            EditorGUILayout.LabelField(status, ToolEditorStyles.SubHeader);
            EditorGUILayout.LabelField($"ItemId: {GetItemId()}", ToolEditorStyles.Muted);

            showValidationDetails = EditorGUILayout.Foldout(
                showValidationDetails,
                $"Validation details ({validation.Errors.Count} errors, {validation.Warnings.Count} warnings)",
                true);

            EditorGUILayout.EndVertical();
        }

        private void DrawValidationMessages()
        {
            DrawMessages(validation.Errors, MessageType.Error);
            DrawMessages(validation.Warnings, MessageType.Warning);
        }

        private static void DrawCreatedObject(string label, Object value)
        {
            if (value == null)
                return;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.ObjectField(label, value, value.GetType(), false);

            if (GUILayout.Button("Ping", GUILayout.Width(InlineButtonWidth)))
                EditorGUIUtility.PingObject(value);

            EditorGUILayout.EndHorizontal();
        }

        private static void DrawMessages(IReadOnlyList<string> messages, MessageType type)
        {
            for (var i = 0; i < messages.Count; i++)
                EditorGUILayout.HelpBox(messages[i], type);
        }

        private static void BeginSection(ref bool expanded, string title, string description)
        {
            ToolEditorStyles.Space();
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            expanded = EditorGUILayout.Foldout(expanded, title, true);

            if (!expanded)
            {
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.LabelField(description, EditorStyles.wordWrappedMiniLabel);
            ToolEditorStyles.Space();
        }

        private static void EndSection()
        {
            EditorGUILayout.EndVertical();
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
                var row = new ToolRecipeRow
                {
                    FromLevel = state.SingleFromLevel,
                    ToLevel = state.SingleToLevel
                };

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
                var row = new ToolRecipeRow
                {
                    FromLevel = level,
                    ToLevel = level + 1
                };

                CopyCosts(state.SharedCosts, row.Costs);
                state.RecipeRows.Add(row);
            }
        }

        private void CopySharedCostsToRecipeRows()
        {
            for (var i = 0; i < state.RecipeRows.Count; i++)
                CopyCosts(state.SharedCosts, state.RecipeRows[i].Costs);
        }

        private static void CopyCosts(IReadOnlyList<ToolRecipeCostRow> source, List<ToolRecipeCostRow> target)
        {
            target.Clear();

            for (var i = 0; i < source.Count; i++)
            {
                target.Add(new ToolRecipeCostRow
                {
                    Item = source[i].Item,
                    Quantity = source[i].Quantity
                });
            }
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
            var slotValue = slot != null ? slot.Id.ToString() : "slot";
            var slotToken = GetIdToken(slotValue.Substring(slotValue.LastIndexOf('.') + 1));
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
                string.IsNullOrWhiteSpace(state.ToolDefinition.DisplayName)
                    ? state.ToolDefinition.name
                    : state.ToolDefinition.DisplayName);
        }

        private string GetToolDisplayName()
        {
            if (state.ToolDefinition == null)
                return "None";

            return string.IsNullOrWhiteSpace(state.ToolDefinition.DisplayName)
                ? state.ToolDefinition.name
                : state.ToolDefinition.DisplayName;
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
            {
                return $"{value:0.####} ({value * 100f:+0.##;-0.##;0}%)";
            }

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

        private static string GetCreationModeLabel(ToolContentCreationMode mode)
        {
            return mode switch
            {
                ToolContentCreationMode.ToolPartOnly => "Part Only",
                ToolContentCreationMode.RecipesOnly => "Recipes Only",
                _ => "Part + Recipes"
            };
        }

        private static string TryConvertToAssetPath(string absolutePath)
        {
            if (string.IsNullOrWhiteSpace(absolutePath))
                return string.Empty;

            var normalizedPath = absolutePath.Replace('\\', '/');
            var normalizedProjectPath = Application.dataPath.Replace('\\', '/');

            if (!normalizedPath.StartsWith(normalizedProjectPath))
                return string.Empty;

            return $"Assets{normalizedPath.Substring(normalizedProjectPath.Length)}";
        }
    }
}