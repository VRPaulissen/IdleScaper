using System.Collections.Generic;
using Items.Definitions;
using Items.Runtime;
using Tools.Definitions;
using UnityEditor;
using UnityEngine;

namespace Tools.Editor
{
    /// <summary>
    /// Custom inspector for permanent tool upgrade recipe assets with a clean, flat authoring layout.
    /// </summary>
    [CustomEditor(typeof(ToolUpgradeRecipeDefinition))]
    internal sealed class ToolUpgradeRecipeDefinitionEditor : UnityEditor.Editor
    {
        private const float SectionSpacing = 8f;
        private const float IdLabelWidth = 92f;
        private const float QuantityWidth = 72f;
        private const float SmallButtonWidth = 28f;
        private const float RowIconSize = 24f;

        private readonly HashSet<string> duplicateCostIds = new HashSet<string>();

        private SerializedProperty toolId;
        private SerializedProperty slotId;
        private SerializedProperty partItemId;
        private SerializedProperty fromLevel;
        private SerializedProperty toLevel;
        private SerializedProperty itemCosts;

        private bool showRawData;

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

            BuildDuplicateCostSet();

            DrawHeader();
            DrawTargetSection();
            DrawLevelSection();
            DrawCostSection();
            DrawRawDataSection();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawHeader()
        {
            BeginSection();

            EditorGUILayout.LabelField("Upgrade Recipe", ToolEditorStyles.Header);
            EditorGUILayout.LabelField(target.name, ToolEditorStyles.Muted);

            ToolEditorStyles.Space();

            DrawPartSummary();
            DrawRecipeSummary();

            EndSection();
        }

        private void DrawPartSummary()
        {
            var itemId = ToolEditorSerialization.ReadItemId(partItemId);

            if (itemId.IsValid && ToolEditorItemLookup.TryGet(itemId, out var itemDefinition) && itemDefinition != null)
            {
                DrawItemSummary(itemDefinition, ToolEditorStyles.LargeIconSize);
                return;
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(Texture2D.grayTexture, GUILayout.Width(ToolEditorStyles.LargeIconSize), GUILayout.Height(ToolEditorStyles.LargeIconSize));

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("No valid part item selected", ToolEditorStyles.SubHeader);
            EditorGUILayout.LabelField(ToolEditorSerialization.ReadIdValue(partItemId), ToolEditorStyles.Muted);
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawRecipeSummary()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            DrawSummaryLine("Tool", ToolEditorSerialization.ReadIdValue(toolId));
            DrawSummaryLine("Slot", ToolEditorSerialization.ReadIdValue(slotId));
            DrawSummaryLine("Levels", $"{fromLevel.intValue} → {toLevel.intValue}");
            DrawSummaryLine("Costs", itemCosts.arraySize.ToString());

            EditorGUILayout.EndVertical();

            DrawTargetWarnings();
            DrawLevelWarnings();
        }

        private static void DrawSummaryLine(string label, string value)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, ToolEditorStyles.Muted, GUILayout.Width(IdLabelWidth));
            EditorGUILayout.LabelField(string.IsNullOrWhiteSpace(value) ? "-" : value, ToolEditorStyles.SubHeader);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawTargetSection()
        {
            Space();

            BeginSection();

            EditorGUILayout.LabelField("Target", ToolEditorStyles.Header);
            EditorGUILayout.LabelField("The installed tool part that this recipe upgrades.", EditorStyles.wordWrappedMiniLabel);

            ToolEditorStyles.Space();

            DrawIdField("ToolId", toolId);
            DrawIdField("SlotId", slotId);
            DrawIdField("Part ItemId", partItemId);

            ToolEditorStyles.Space();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Refresh Item Lookup", GUILayout.Height(24f)))
                ToolEditorItemLookup.Refresh();

            EditorGUILayout.EndHorizontal();

            EndSection();
        }

        private static void DrawIdField(string label, SerializedProperty idProperty)
        {
            var valueProperty = idProperty.FindPropertyRelative("value");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(IdLabelWidth));

            if (valueProperty == null)
            {
                EditorGUILayout.HelpBox("Id property does not contain a serialized value field.", MessageType.Error);
                EditorGUILayout.EndHorizontal();
                return;
            }

            valueProperty.stringValue = EditorGUILayout.TextField(valueProperty.stringValue);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawLevelSection()
        {
            Space();

            BeginSection();

            EditorGUILayout.LabelField("Level Transition", ToolEditorStyles.Header);
            EditorGUILayout.LabelField("The required current level and the resulting upgraded level.", EditorStyles.wordWrappedMiniLabel);

            ToolEditorStyles.Space();

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("From", GUILayout.Width(IdLabelWidth));
            fromLevel.intValue = Mathf.Max(0, EditorGUILayout.IntField(fromLevel.intValue));

            EditorGUILayout.LabelField("To", GUILayout.Width(28f));
            toLevel.intValue = Mathf.Max(1, EditorGUILayout.IntField(toLevel.intValue));

            if (GUILayout.Button("Normalize", GUILayout.Width(88f)))
                toLevel.intValue = fromLevel.intValue + 1;

            EditorGUILayout.EndHorizontal();

            DrawLevelWarnings();

            EndSection();
        }

        private void DrawCostSection()
        {
            Space();

            BeginSection();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Costs ({itemCosts.arraySize})", ToolEditorStyles.Header);

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Add Cost", GUILayout.Width(92f)))
                AddCostRow();

            using (new EditorGUI.DisabledScope(itemCosts.arraySize == 0))
            {
                if (GUILayout.Button("Clear", GUILayout.Width(64f)))
                    ClearCosts();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Inventory items required to apply this upgrade.", EditorStyles.wordWrappedMiniLabel);

            ToolEditorStyles.Space();

            if (itemCosts.arraySize == 0)
                EditorGUILayout.HelpBox("This recipe has no costs.", MessageType.Warning);

            if (duplicateCostIds.Count > 0)
                EditorGUILayout.HelpBox("Duplicate cost items are present. Runtime aggregation can handle this, but one row per item is easier to maintain.", MessageType.Warning);

            for (var i = 0; i < itemCosts.arraySize; i++)
                DrawCostRow(i);

            EndSection();
        }

        private void DrawCostRow(int index)
        {
            var cost = itemCosts.GetArrayElementAtIndex(index);
            var itemIdProperty = cost.FindPropertyRelative("itemId");
            var quantityProperty = cost.FindPropertyRelative("quantity");
            var itemId = ToolEditorSerialization.ReadItemId(itemIdProperty);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();

            DrawCostItemPreview(itemId);

            EditorGUILayout.BeginVertical();

            DrawIdField("ItemId", itemIdProperty);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Quantity", GUILayout.Width(IdLabelWidth));
            quantityProperty.intValue = Mathf.Max(1, EditorGUILayout.IntField(quantityProperty.intValue, GUILayout.Width(QuantityWidth)));
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("×", GUILayout.Width(SmallButtonWidth)))
            {
                RemoveCostRow(index);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            DrawCostWarnings(itemId, quantityProperty.intValue);

            EditorGUILayout.EndVertical();
        }

        private static void DrawCostItemPreview(ItemId itemId)
        {
            if (itemId.IsValid && ToolEditorItemLookup.TryGet(itemId, out var itemDefinition) && itemDefinition != null)
            {
                GUILayout.Label(
                    ToolEditorStyles.GetIconTexture(itemDefinition.Icon),
                    GUILayout.Width(RowIconSize),
                    GUILayout.Height(RowIconSize));

                EditorGUILayout.BeginVertical(GUILayout.Width(150f));
                EditorGUILayout.LabelField(GetDisplayName(itemDefinition, itemId), ToolEditorStyles.SubHeader);
                EditorGUILayout.LabelField(itemDefinition.Id.ToString(), ToolEditorStyles.Muted);
                EditorGUILayout.EndVertical();
                return;
            }

            GUILayout.Label(Texture2D.grayTexture, GUILayout.Width(RowIconSize), GUILayout.Height(RowIconSize));

            EditorGUILayout.BeginVertical(GUILayout.Width(150f));
            EditorGUILayout.LabelField("Missing item", ToolEditorStyles.SubHeader);
            EditorGUILayout.LabelField(itemId.ToString(), ToolEditorStyles.Muted);
            EditorGUILayout.EndVertical();
        }

        private void DrawRawDataSection()
        {
            Space();

            BeginSection();

            showRawData = EditorGUILayout.Foldout(showRawData, "Raw Serialized Data", true);
            if (showRawData)
            {
                EditorGUILayout.PropertyField(toolId);
                EditorGUILayout.PropertyField(slotId);
                EditorGUILayout.PropertyField(partItemId);
                EditorGUILayout.PropertyField(fromLevel);
                EditorGUILayout.PropertyField(toLevel);
                EditorGUILayout.PropertyField(itemCosts, true);
            }

            EndSection();
        }

        private void DrawTargetWarnings()
        {
            if (string.IsNullOrWhiteSpace(ToolEditorSerialization.ReadIdValue(toolId)))
                EditorGUILayout.HelpBox("ToolId is empty.", MessageType.Warning);

            if (string.IsNullOrWhiteSpace(ToolEditorSerialization.ReadIdValue(slotId)))
                EditorGUILayout.HelpBox("SlotId is empty.", MessageType.Warning);

            DrawPartWarnings();
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
                EditorGUILayout.HelpBox($"Part item '{itemId}' was not found in project item assets.", MessageType.Warning);
        }

        private void DrawLevelWarnings()
        {
            if (toLevel.intValue <= fromLevel.intValue)
            {
                EditorGUILayout.HelpBox("To Level must be greater than From Level.", MessageType.Error);
                return;
            }

            if (toLevel.intValue != fromLevel.intValue + 1)
                EditorGUILayout.HelpBox("This recipe skips one or more levels. That is allowed, but unusual for normal upgrades.", MessageType.Info);
        }

        private void DrawCostWarnings(ItemId itemId, int quantity)
        {
            if (!itemId.IsValid)
                EditorGUILayout.HelpBox("Cost ItemId is empty.", MessageType.Warning);

            if (itemId.IsValid && !ToolEditorItemLookup.TryGet(itemId, out _))
                EditorGUILayout.HelpBox($"Cost item '{itemId}' was not found in project item assets.", MessageType.Warning);

            if (quantity <= 0)
                EditorGUILayout.HelpBox("Cost quantity must be greater than zero.", MessageType.Error);

            if (itemId.IsValid && duplicateCostIds.Contains(itemId.ToString()))
                EditorGUILayout.HelpBox("Duplicate cost ItemId in this recipe.", MessageType.Warning);
        }

        private void AddCostRow()
        {
            Undo.RecordObject(target, "Add Tool Upgrade Cost");

            itemCosts.arraySize++;
            var cost = itemCosts.GetArrayElementAtIndex(itemCosts.arraySize - 1);
            DrawIdFieldValue(cost.FindPropertyRelative("itemId"), string.Empty);
            cost.FindPropertyRelative("quantity").intValue = 1;

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
            BuildDuplicateCostSet();
        }

        private void RemoveCostRow(int index)
        {
            if (index < 0 || index >= itemCosts.arraySize)
                return;

            Undo.RecordObject(target, "Remove Tool Upgrade Cost");
            itemCosts.DeleteArrayElementAtIndex(index);
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
            BuildDuplicateCostSet();
        }

        private void ClearCosts()
        {
            Undo.RecordObject(target, "Clear Tool Upgrade Costs");
            itemCosts.ClearArray();
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
            BuildDuplicateCostSet();
        }

        private static void DrawIdFieldValue(SerializedProperty idProperty, string value)
        {
            var valueProperty = idProperty.FindPropertyRelative("value");
            if (valueProperty == null)
                return;

            valueProperty.stringValue = value;
        }

        private void BuildDuplicateCostSet()
        {
            duplicateCostIds.Clear();

            var seen = new HashSet<string>();
            for (var i = 0; i < itemCosts.arraySize; i++)
            {
                var cost = itemCosts.GetArrayElementAtIndex(i);
                var itemIdValue = ToolEditorSerialization.ReadIdValue(cost.FindPropertyRelative("itemId"));

                if (string.IsNullOrWhiteSpace(itemIdValue))
                    continue;

                if (!seen.Add(itemIdValue))
                    duplicateCostIds.Add(itemIdValue);
            }
        }

        private static void DrawItemSummary(ItemDefinition itemDefinition, float iconSize)
        {
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(
                ToolEditorStyles.GetIconTexture(itemDefinition.Icon),
                GUILayout.Width(iconSize),
                GUILayout.Height(iconSize));

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(GetDisplayName(itemDefinition, itemDefinition.Id), ToolEditorStyles.SubHeader);
            EditorGUILayout.LabelField(itemDefinition.Id.ToString(), ToolEditorStyles.Muted);
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        private static string GetDisplayName(ItemDefinition itemDefinition, object fallbackId)
        {
            if (itemDefinition == null)
                return fallbackId != null ? fallbackId.ToString() : string.Empty;

            return string.IsNullOrWhiteSpace(itemDefinition.DisplayName)
                ? fallbackId.ToString()
                : itemDefinition.DisplayName;
        }

        private static void BeginSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        }

        private static void EndSection()
        {
            EditorGUILayout.EndVertical();
        }

        private static void Space()
        {
            EditorGUILayout.Space(SectionSpacing);
        }
    }
}