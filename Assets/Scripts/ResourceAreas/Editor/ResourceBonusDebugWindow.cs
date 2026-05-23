#if UNITY_EDITOR
using System.Collections.Generic;
using ResourceAreas.Definitions;
using ResourceAreas.Runtime;
using ResourceAreas.Services;
using ResourceAreas.State;
using ResourceAreas.ViewModels;
using Tools.Definitions;
using Tools.Runtime;
using UnityEditor;
using UnityEngine;

namespace ResourceAreas.Editor
{
    /// <summary>
    /// Editor window for inspecting resolved resource bonus totals and contributions.
    /// </summary>
    public sealed class ResourceBonusDebugWindow : EditorWindow
    {
        private readonly List<IResourceBonusProvider> providers = new List<IResourceBonusProvider>(4);
        private ResourceAreaCatalog areaCatalog;
        private ResourceCatalog resourceCatalog;
        private GlobalBoostCatalog globalBoostCatalog;
        private ToolDefinitionCatalog toolDefinitionCatalog;
        private ResourceAreaDefinition selectedArea;
        private ResourceDefinition selectedResource;
        private ToolDefinition selectedTool;
        private GlobalBoostDefinition selectedGlobalBoost;
        private int areaLevel = 1;
        private float areaXp;
        private int resourceLevel = 1;
        private float resourceXp;
        private bool globalBoostActive;
        private int globalBoostStacks = 1;
        private bool autoRefresh;
        private Vector2 scrollPosition;
        private ResourceCollectionState temporaryState;
        private GlobalBoostCollectionState temporaryGlobalBoostState;
        private ResourceBonusDebugViewModel viewModel;
        private string providerHelpText = string.Empty;

        /// <summary>
        /// Opens the resource bonus debug window.
        /// </summary>
        [MenuItem("Tools/Resource Areas/Bonus Debug Window")]
        public static void Open()
        {
            GetWindow<ResourceBonusDebugWindow>("Resource Bonus Debug");
        }

        private void OnEnable()
        {
            temporaryState ??= new ResourceCollectionState();
            temporaryGlobalBoostState ??= new GlobalBoostCollectionState();
        }

        private void OnGUI()
        {
            temporaryState ??= new ResourceCollectionState();
            temporaryGlobalBoostState ??= new GlobalBoostCollectionState();
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUI.BeginChangeCheck();
            DrawReferenceFields();
            DrawProgressFields();
            var changed = EditorGUI.EndChangeCheck();

            DrawActions();

            if (autoRefresh && changed)
                TryBuildViewModel();

            DrawViewModel();
            EditorGUILayout.EndScrollView();
        }

        private void DrawReferenceFields()
        {
            EditorGUILayout.LabelField("References", EditorStyles.boldLabel);
            areaCatalog = EditorGUILayout.ObjectField("Resource Area Catalog", areaCatalog, typeof(ResourceAreaCatalog), false) as ResourceAreaCatalog;
            resourceCatalog = EditorGUILayout.ObjectField("Resource Catalog", resourceCatalog, typeof(ResourceCatalog), false) as ResourceCatalog;
            globalBoostCatalog = EditorGUILayout.ObjectField("Global Boost Catalog", globalBoostCatalog, typeof(GlobalBoostCatalog), false) as GlobalBoostCatalog;
            toolDefinitionCatalog = EditorGUILayout.ObjectField("Tool Definition Catalog", toolDefinitionCatalog, typeof(ToolDefinitionCatalog), false) as ToolDefinitionCatalog;
            selectedArea = EditorGUILayout.ObjectField("Resource Area", selectedArea, typeof(ResourceAreaDefinition), false) as ResourceAreaDefinition;
            selectedResource = EditorGUILayout.ObjectField("Resource", selectedResource, typeof(ResourceDefinition), false) as ResourceDefinition;
            selectedTool = EditorGUILayout.ObjectField("Tool", selectedTool, typeof(ToolDefinition), false) as ToolDefinition;
            selectedGlobalBoost = EditorGUILayout.ObjectField("Active Global Boost", selectedGlobalBoost, typeof(GlobalBoostDefinition), false) as GlobalBoostDefinition;
            EditorGUILayout.Space();
        }

        private void DrawProgressFields()
        {
            EditorGUILayout.LabelField("Temporary Progress", EditorStyles.boldLabel);
            areaLevel = Mathf.Max(1, EditorGUILayout.IntField("Area Level", areaLevel));
            areaXp = Mathf.Max(0f, EditorGUILayout.FloatField("Area XP", areaXp));
            resourceLevel = Mathf.Max(1, EditorGUILayout.IntField("Resource Level", resourceLevel));
            resourceXp = Mathf.Max(0f, EditorGUILayout.FloatField("Resource XP", resourceXp));
            globalBoostActive = EditorGUILayout.Toggle("Global Boost Active", globalBoostActive);
            globalBoostStacks = Mathf.Max(1, EditorGUILayout.IntField("Global Boost Stacks", globalBoostStacks));
            EditorGUILayout.Space();
        }

        private void DrawActions()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh", GUILayout.Width(120f)))
                TryBuildViewModel();

            autoRefresh = EditorGUILayout.ToggleLeft("Auto Refresh", autoRefresh, GUILayout.Width(140f));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }

        private void DrawViewModel()
        {
            DrawMissingReferenceMessages();

            if (!string.IsNullOrEmpty(providerHelpText))
                EditorGUILayout.HelpBox(providerHelpText, MessageType.Info);

            if (viewModel == null)
            {
                EditorGUILayout.HelpBox("Select references and click Refresh.", MessageType.Info);
                return;
            }

            if (!viewModel.IsValid)
            {
                EditorGUILayout.HelpBox(viewModel.FailureText, MessageType.Warning);
                return;
            }

            DrawContext();
            DrawTotals();
            DrawContributionGroups();
        }

        private void DrawMissingReferenceMessages()
        {
            if (areaCatalog == null)
                EditorGUILayout.HelpBox("Resource Area Catalog is required.", MessageType.Warning);

            if (resourceCatalog == null)
                EditorGUILayout.HelpBox("Resource Catalog is required.", MessageType.Warning);

            if (selectedArea == null)
                EditorGUILayout.HelpBox("Resource Area selection is required.", MessageType.Warning);

            if (selectedResource == null)
                EditorGUILayout.HelpBox("Resource selection is required.", MessageType.Warning);

            if (selectedTool == null)
                EditorGUILayout.HelpBox("Tool selection is required.", MessageType.Warning);
        }

        private void DrawContext()
        {
            EditorGUILayout.LabelField("Context", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Area", viewModel.AreaName + "  Lv. " + viewModel.AreaLevel);
            EditorGUILayout.LabelField("Resource", viewModel.ResourceName + "  Lv. " + viewModel.ResourceLevel);
            EditorGUILayout.LabelField("Tool", viewModel.ToolName);
            EditorGUILayout.Space();
        }

        private void DrawTotals()
        {
            EditorGUILayout.LabelField("Totals", EditorStyles.boldLabel);
            DrawTotalHeader();

            var totals = viewModel.Totals;
            if (totals.Count == 0)
            {
                EditorGUILayout.LabelField("No resolved total bonuses.");
                EditorGUILayout.Space();
                return;
            }

            for (var i = 0; i < totals.Count; i++)
                DrawTotalRow(totals[i]);

            EditorGUILayout.Space();
        }

        private void DrawTotalHeader()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Bonus Type", EditorStyles.miniBoldLabel, GUILayout.Width(220f));
            GUILayout.Label("Total Value", EditorStyles.miniBoldLabel, GUILayout.Width(100f));
            GUILayout.Label("Formatted", EditorStyles.miniBoldLabel, GUILayout.Width(100f));
            EditorGUILayout.EndHorizontal();
        }

        private void DrawTotalRow(ResourceBonusDebugTotalRow row)
        {
            if (row == null)
                return;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(row.BonusType.ToString(), GUILayout.Width(220f));
            GUILayout.Label(row.TotalValue.ToString("0.###"), GUILayout.Width(100f));
            GUILayout.Label(row.FormattedValue, GUILayout.Width(100f));
            EditorGUILayout.EndHorizontal();
        }

        private void DrawContributionGroups()
        {
            EditorGUILayout.LabelField("Contributions", EditorStyles.boldLabel);
            var groups = viewModel.Groups;
            if (groups.Count == 0)
            {
                EditorGUILayout.LabelField("No resolved bonus contributions.");
                return;
            }

            for (var i = 0; i < groups.Count; i++)
                DrawContributionGroup(groups[i]);
        }

        private void DrawContributionGroup(ResourceBonusDebugGroup group)
        {
            if (group == null)
                return;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(group.DisplayName, EditorStyles.boldLabel);
            DrawContributionHeader();

            var rows = group.Rows;
            for (var i = 0; i < rows.Count; i++)
                DrawContributionRow(rows[i]);
        }

        private void DrawContributionHeader()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Bonus Type", EditorStyles.miniBoldLabel, GUILayout.Width(190f));
            GUILayout.Label("Value", EditorStyles.miniBoldLabel, GUILayout.Width(80f));
            GUILayout.Label("Formatted", EditorStyles.miniBoldLabel, GUILayout.Width(90f));
            GUILayout.Label("Source Name", EditorStyles.miniBoldLabel, GUILayout.Width(180f));
            GUILayout.Label("Source Id", EditorStyles.miniBoldLabel, GUILayout.Width(220f));
            EditorGUILayout.EndHorizontal();
        }

        private void DrawContributionRow(ResourceBonusDebugRow row)
        {
            if (row == null)
                return;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(row.BonusType.ToString(), GUILayout.Width(190f));
            GUILayout.Label(row.Value.ToString("0.###"), GUILayout.Width(80f));
            GUILayout.Label(row.FormattedValue, GUILayout.Width(90f));
            GUILayout.Label(row.SourceName, GUILayout.Width(180f));
            GUILayout.Label(row.SourceId, GUILayout.Width(220f));
            EditorGUILayout.EndHorizontal();
        }

        private bool TryBuildViewModel()
        {
            providerHelpText = string.Empty;

            if (!HasRequiredReferences())
            {
                viewModel = null;
                return false;
            }

            ApplyTemporaryProgress(temporaryState);
            ApplyTemporaryGlobalBoost(temporaryGlobalBoostState);

            if (!TryCreateResolver(out var resolver))
            {
                viewModel = null;
                return false;
            }

            var builder = new ResourceBonusDebugViewModelBuilder(
                temporaryState,
                areaCatalog,
                resourceCatalog,
                resolver,
                toolDefinitionCatalog);

            viewModel = builder.Build(selectedArea.Id, selectedResource.Id, selectedTool.Id);
            return viewModel.IsValid;
        }

        private bool HasRequiredReferences()
        {
            if (areaCatalog == null)
                return false;

            if (resourceCatalog == null)
                return false;

            if (selectedArea == null)
                return false;

            if (selectedResource == null)
                return false;

            return selectedTool != null;
        }

        private bool TryCreateResolver(out ResourceBonusResolver resolver)
        {
            resolver = null;
            providers.Clear();

            if (temporaryState == null || areaCatalog == null || resourceCatalog == null)
                return false;

            providers.Add(new ResourceAreaBonusProvider(temporaryState, areaCatalog));
            providers.Add(new ResourceLevelBonusProvider(temporaryState, resourceCatalog));
            AddGlobalProviderIfAvailable();
            providerHelpText = "Tool bonus provider is not connected in this editor window yet.";
            resolver = new ResourceBonusResolver(providers);
            return true;
        }

        private void AddGlobalProviderIfAvailable()
        {
            if (globalBoostCatalog == null || temporaryGlobalBoostState == null || selectedGlobalBoost == null)
                return;

            providers.Add(new GlobalResourceBonusProvider(temporaryGlobalBoostState, globalBoostCatalog));
        }

        private void ApplyTemporaryProgress(ResourceCollectionState state)
        {
            if (state == null)
                return;

            if (selectedArea != null)
            {
                var areaProgress = state.GetOrCreateAreaProgress(selectedArea.Id);
                areaProgress?.SetProgress(areaLevel, areaXp);
            }

            if (selectedResource == null)
                return;

            var resourceProgress = state.GetOrCreateResourceProgress(selectedResource.Id);
            resourceProgress?.SetProgress(resourceLevel, resourceXp);
        }

        private void ApplyTemporaryGlobalBoost(GlobalBoostCollectionState state)
        {
            if (state == null)
                return;

            state.Boosts.Clear();

            if (selectedGlobalBoost == null)
                return;

            var boostState = state.GetOrCreateBoost(selectedGlobalBoost.Id);
            if (boostState == null)
                return;

            boostState.SetActive(globalBoostActive);
            boostState.SetStackCount(globalBoostStacks);
            boostState.ClearTiming();
        }
    }
}
#endif
