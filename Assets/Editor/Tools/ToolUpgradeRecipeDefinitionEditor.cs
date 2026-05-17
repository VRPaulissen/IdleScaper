using System.Collections.Generic;
using Items.Definitions;
using Tools.Definitions;
using UnityEditor;
using UnityEngine;

namespace Tools.Editor
{
    /// <summary>
    /// Custom inspector for permanent tool upgrade recipe assets.
    /// </summary>
    [CustomEditor(typeof(ToolUpgradeRecipeDefinition))]
    internal sealed class ToolUpgradeRecipeDefinitionEditor : UnityEditor.Editor
    {
        private readonly HashSet<string> duplicateCostIds = new HashSet<string>();

        private SerializedProperty toolId;
        private SerializedProperty slotId;
        private SerializedProperty partItemId;
        private SerializedProperty fromLevel;
        private SerializedProperty toLevel;
        private SerializedProperty itemCosts;

        private void OnEnable()
        {
            toolId = serializedObject.FindProperty("toolId");
            slotId = serializedObject.FindProperty("slotId");
            partItemId = serializedObject.FindProperty("partItemId");
            fromLevel = serializedObject.FindProperty("fromLevel");
            toLevel = serializedObject.FindProperty("toLevel");
            itemCosts = serializedObject.FindProperty("itemCosts");
        }

        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawRecipeHeader();
            ToolEditorStyles.Space();
            DrawTarget();
            ToolEditorStyles.Space();
            DrawCosts();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawRecipeHeader()
        {
            ToolEditorStyles.BeginBox();
            EditorGUILayout.LabelField("Upgrade Recipe", ToolEditorStyles.Header);

            var itemId = ToolEditorSerialization.ReadItemId(partItemId);
            if (itemId.IsValid && ToolEditorItemLookup.TryGet(itemId, out var itemDefinition))
                DrawItemSummary(itemDefinition, itemId, ToolEditorStyles.LargeIconSize);
            else
                EditorGUILayout.LabelField("Part", ToolEditorSerialization.ReadIdValue(partItemId));

            EditorGUILayout.LabelField("Levels", $"{fromLevel.intValue} -> {toLevel.intValue}");
            EditorGUILayout.LabelField("Tool / Slot", $"{ToolEditorSerialization.ReadIdValue(toolId)} / {ToolEditorSerialization.ReadIdValue(slotId)}", ToolEditorStyles.Muted);
            DrawLevelWarnings();
            ToolEditorStyles.EndBox();
        }

        private void DrawTarget()
        {
            ToolEditorStyles.BeginBox();
            EditorGUILayout.LabelField("Target", ToolEditorStyles.Header);
            EditorGUILayout.PropertyField(toolId);
            EditorGUILayout.PropertyField(slotId);
            EditorGUILayout.PropertyField(partItemId);
            EditorGUILayout.PropertyField(fromLevel);
            EditorGUILayout.PropertyField(toLevel);
            DrawPartWarnings();
            ToolEditorItemLookup.DrawRefreshButton();
            ToolEditorStyles.EndBox();
        }

        private void DrawPartWarnings()
        {
            var itemId = ToolEditorSerialization.ReadItemId(partItemId);
            if (!itemId.IsValid)
            {
                EditorGUILayout.HelpBox("Part ItemId is empty.", MessageType.Warning);
                return;
            }

            if (!ToolEditorItemLookup.TryGet(itemId, out var itemDefinition) || itemDefinition == null)
            {
                EditorGUILayout.HelpBox($"Part item '{itemId}' was not found in project item assets.", MessageType.Warning);
                return;
            }
        }

        private static void DrawItemSummary(ItemDefinition itemDefinition, object fallbackId, float iconSize)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(
                ToolEditorStyles.GetIconTexture(itemDefinition.Icon),
                GUILayout.Width(iconSize),
                GUILayout.Height(iconSize));
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(GetDisplayName(itemDefinition, fallbackId), ToolEditorStyles.SubHeader);
            EditorGUILayout.LabelField(itemDefinition.Id.ToString(), ToolEditorStyles.Muted);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        private static string GetDisplayName(ItemDefinition itemDefinition, object fallbackId)
        {
            return string.IsNullOrWhiteSpace(itemDefinition.DisplayName) ? fallbackId.ToString() : itemDefinition.DisplayName;
        }

        private void DrawCosts()
        {
            ToolEditorStyles.BeginBox();
            EditorGUILayout.LabelField("Item Costs", ToolEditorStyles.Header);
            DrawCostSummary();

            itemCosts.isExpanded = EditorGUILayout.Foldout(itemCosts.isExpanded, "Cost Rows", true);
            if (itemCosts.isExpanded)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(itemCosts.FindPropertyRelative("Array.size"));
                BuildDuplicateCostSet();
                for (var i = 0; i < itemCosts.arraySize; i++)
                {
                    DrawCostRow(itemCosts.GetArrayElementAtIndex(i), i);
                }

                EditorGUI.indentLevel--;
            }

            ToolEditorStyles.EndBox();
        }

        private void DrawCostRow(SerializedProperty cost, int index)
        {
            var itemIdProperty = cost.FindPropertyRelative("itemId");
            var quantityProperty = cost.FindPropertyRelative("quantity");
            var itemId = ToolEditorSerialization.ReadItemId(itemIdProperty);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            if (itemId.IsValid && ToolEditorItemLookup.TryGet(itemId, out var itemDefinition))
                DrawItemSummary(itemDefinition, itemId, ToolEditorStyles.SmallIconSize);
            else if (itemId.IsValid)
                EditorGUILayout.HelpBox($"Cost item '{itemId}' was not found.", MessageType.Warning);
            else
                EditorGUILayout.LabelField($"Cost {index + 1}", ToolEditorStyles.SubHeader);

            EditorGUILayout.PropertyField(itemIdProperty);
            EditorGUILayout.PropertyField(quantityProperty);
            DrawCostWarnings(itemId, quantityProperty.intValue);
            EditorGUILayout.EndVertical();
        }

        private void DrawCostWarnings(Items.Runtime.ItemId itemId, int quantity)
        {
            if (!itemId.IsValid)
                EditorGUILayout.HelpBox("Cost ItemId is empty.", MessageType.Warning);

            if (quantity <= 0)
                EditorGUILayout.HelpBox("Cost quantity must be greater than zero.", MessageType.Error);

            if (itemId.IsValid && duplicateCostIds.Contains(itemId.ToString()))
                EditorGUILayout.HelpBox("Duplicate cost ItemId in this recipe. Runtime costs are aggregated, but the recipe may be harder to read.", MessageType.Warning);
        }

        private void DrawCostSummary()
        {
            BuildDuplicateCostSet();
            EditorGUILayout.LabelField("Rows", itemCosts.arraySize.ToString());
            if (duplicateCostIds.Count > 0)
                EditorGUILayout.HelpBox("Duplicate ingredient IDs exist. Runtime aggregation handles them, but combining rows is easier to read.", MessageType.Warning);
        }

        private void DrawLevelWarnings()
        {
            if (toLevel.intValue <= fromLevel.intValue)
            {
                EditorGUILayout.HelpBox("ToLevel must be greater than FromLevel.", MessageType.Error);
                return;
            }

            if (toLevel.intValue != fromLevel.intValue + 1)
                EditorGUILayout.HelpBox("This recipe skips one or more levels. That is allowed by data, but unusual for normal upgrades.", MessageType.Info);
        }

        private void BuildDuplicateCostSet()
        {
            duplicateCostIds.Clear();
            var seen = new HashSet<string>();

            for (var i = 0; i < itemCosts.arraySize; i++)
            {
                var cost = itemCosts.GetArrayElementAtIndex(i);
                var itemId = ToolEditorSerialization.ReadIdValue(cost.FindPropertyRelative("itemId"));
                if (string.IsNullOrWhiteSpace(itemId))
                    continue;

                if (!seen.Add(itemId))
                    duplicateCostIds.Add(itemId);
            }
        }
    }
}
