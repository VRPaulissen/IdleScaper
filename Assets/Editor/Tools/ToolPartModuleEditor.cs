using System.Collections.Generic;
using Items.Runtime.Modules;
using Tools.Runtime;
using UnityEditor;
using UnityEngine;

namespace Tools.Editor
{
    /// <summary>
    /// Custom inspector for permanent tool part item modules.
    /// </summary>
    [CustomEditor(typeof(ToolPartModule))]
    internal sealed class ToolPartModuleEditor : UnityEditor.Editor
    {
        private readonly HashSet<ToolBonusType> duplicateBonusTypes = new HashSet<ToolBonusType>();

        private SerializedProperty compatibleToolId;
        private SerializedProperty compatibleSlotId;
        private SerializedProperty maxLevel;
        private SerializedProperty bonuses;

        private void OnEnable()
        {
            compatibleToolId = serializedObject.FindProperty("compatibleToolId");
            compatibleSlotId = serializedObject.FindProperty("compatibleSlotId");
            maxLevel = serializedObject.FindProperty("maxLevel");
            bonuses = serializedObject.FindProperty("bonuses");
        }

        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawCompatibility();
            ToolEditorStyles.Space();
            DrawProgression();
            ToolEditorStyles.Space();
            DrawBonuses();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawCompatibility()
        {
            ToolEditorStyles.BeginBox();
            EditorGUILayout.LabelField("Compatibility", ToolEditorStyles.Header);
            EditorGUILayout.LabelField("Tool", ToolEditorSerialization.ReadIdValue(compatibleToolId));
            EditorGUILayout.LabelField("Slot", ToolEditorSerialization.ReadIdValue(compatibleSlotId));
            EditorGUILayout.PropertyField(compatibleToolId, new GUIContent("Compatible Tool"));
            EditorGUILayout.PropertyField(compatibleSlotId, new GUIContent("Compatible Slot"));

            if (string.IsNullOrWhiteSpace(ToolEditorSerialization.ReadIdValue(compatibleToolId)))
                EditorGUILayout.HelpBox("Compatible ToolId is empty.", MessageType.Warning);

            if (string.IsNullOrWhiteSpace(ToolEditorSerialization.ReadIdValue(compatibleSlotId)))
                EditorGUILayout.HelpBox("Compatible ToolPartSlotId is empty.", MessageType.Warning);

            ToolEditorStyles.EndBox();
        }

        private void DrawProgression()
        {
            ToolEditorStyles.BeginBox();
            EditorGUILayout.LabelField("Progression", ToolEditorStyles.Header);
            EditorGUILayout.PropertyField(maxLevel);

            if (maxLevel.intValue < 1)
                EditorGUILayout.HelpBox("MaxLevel should be at least 1.", MessageType.Warning);

            ToolEditorStyles.EndBox();
        }

        private void DrawBonuses()
        {
            ToolEditorStyles.BeginBox();
            EditorGUILayout.LabelField("Bonuses", ToolEditorStyles.Header);
            EditorGUILayout.LabelField("Bonus Count", bonuses.arraySize.ToString());
            DrawBonusListWarnings();

            bonuses.isExpanded = EditorGUILayout.Foldout(bonuses.isExpanded, "Bonus Definitions", true);
            if (bonuses.isExpanded)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(bonuses.FindPropertyRelative("Array.size"));
                BuildDuplicateBonusSet();
                for (var i = 0; i < bonuses.arraySize; i++)
                {
                    DrawBonusRow(bonuses.GetArrayElementAtIndex(i), i);
                }

                EditorGUI.indentLevel--;
            }

            if (bonuses.arraySize == 0)
                EditorGUILayout.HelpBox("This part currently contributes no bonuses.", MessageType.Info);

            ToolEditorStyles.EndBox();
        }

        private void DrawBonusRow(SerializedProperty bonus, int index)
        {
            var type = bonus.FindPropertyRelative("type");
            var baseValue = bonus.FindPropertyRelative("baseValue");
            var valuePerLevel = bonus.FindPropertyRelative("valuePerLevel");
            var bonusType = (ToolBonusType)type.enumValueIndex;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(type.enumDisplayNames[type.enumValueIndex], ToolEditorStyles.SubHeader);
            EditorGUILayout.PropertyField(type);
            EditorGUILayout.PropertyField(baseValue);
            EditorGUILayout.PropertyField(valuePerLevel);
            DrawBonusPreviewRow(bonusType, baseValue.floatValue, valuePerLevel.floatValue);
            DrawBonusWarnings(bonusType, baseValue.floatValue, valuePerLevel.floatValue);
            EditorGUILayout.EndVertical();
        }

        private void DrawBonusPreviewRow(ToolBonusType type, float baseValue, float valuePerLevel)
        {
            var levelZero = baseValue;
            var max = baseValue + Mathf.Max(0, maxLevel.intValue) * valuePerLevel;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Preview", GUILayout.Width(EditorGUIUtility.labelWidth));
            EditorGUILayout.LabelField($"L0 {FormatBonus(type, levelZero)}", ToolEditorStyles.Muted);
            EditorGUILayout.LabelField($"L{Mathf.Max(0, maxLevel.intValue)} {FormatBonus(type, max)}", ToolEditorStyles.Muted);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawBonusWarnings(ToolBonusType type, float baseValue, float valuePerLevel)
        {
            if (baseValue < 0f || valuePerLevel < 0f)
                EditorGUILayout.HelpBox("Negative bonus values are not currently expected for tool parts.", MessageType.Warning);

            if (duplicateBonusTypes.Contains(type))
                EditorGUILayout.HelpBox("Duplicate bonus type on this module. Values will stack additively.", MessageType.Info);
        }

        private void DrawBonusListWarnings()
        {
            BuildDuplicateBonusSet();
            if (duplicateBonusTypes.Count > 0)
                EditorGUILayout.HelpBox("Duplicate bonus types are present. They stack additively.", MessageType.Info);
        }

        private static string FormatBonus(ToolBonusType type, float value)
        {
            if (IsPercentLike(type))
                return $"{value:0.####} raw ({value * 100f:+0.##;-0.##;0}%)";

            return value.ToString("0.####");
        }

        private static bool IsPercentLike(ToolBonusType type)
        {
            return type == ToolBonusType.MiningDamageMultiplier
                   || type == ToolBonusType.MiningSpeedMultiplier
                   || type == ToolBonusType.ResourceYieldMultiplier
                   || type == ToolBonusType.RareDropChanceFlat;
        }

        private void BuildDuplicateBonusSet()
        {
            duplicateBonusTypes.Clear();
            var seen = new HashSet<ToolBonusType>();

            for (var i = 0; i < bonuses.arraySize; i++)
            {
                var bonus = bonuses.GetArrayElementAtIndex(i);
                var type = (ToolBonusType)bonus.FindPropertyRelative("type").enumValueIndex;
                if (!seen.Add(type))
                    duplicateBonusTypes.Add(type);
            }
        }
    }
}
