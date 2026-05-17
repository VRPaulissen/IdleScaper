using System.Collections.Generic;
using Tools.Definitions;
using UnityEditor;
using UnityEngine;

namespace Tools.Editor
{
    /// <summary>
    /// Custom inspector for permanent tool definition assets.
    /// </summary>
    [CustomEditor(typeof(ToolDefinition))]
    internal sealed class ToolDefinitionEditor : UnityEditor.Editor
    {
        private readonly HashSet<string> duplicateSlotIds = new HashSet<string>();

        private SerializedProperty id;
        private SerializedProperty displayName;
        private SerializedProperty icon;
        private SerializedProperty supportedSlots;

        private void OnEnable()
        {
            id = serializedObject.FindProperty("id");
            displayName = serializedObject.FindProperty("displayName");
            icon = serializedObject.FindProperty("icon");
            supportedSlots = serializedObject.FindProperty("supportedSlots");
        }

        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawToolHeader();
            ToolEditorStyles.Space();
            DrawIdentity();
            ToolEditorStyles.Space();
            DrawSlots();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawToolHeader()
        {
            ToolEditorStyles.BeginBox();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(
                ToolEditorStyles.GetIconTexture((Sprite)icon.objectReferenceValue),
                GUILayout.Width(ToolEditorStyles.LargeIconSize),
                GUILayout.Height(ToolEditorStyles.LargeIconSize));
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(string.IsNullOrWhiteSpace(displayName.stringValue) ? target.name : displayName.stringValue, ToolEditorStyles.Header);
            EditorGUILayout.LabelField(ToolEditorSerialization.ReadIdValue(id), ToolEditorStyles.Muted);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            ToolEditorStyles.EndBox();
        }

        private void DrawIdentity()
        {
            ToolEditorStyles.BeginBox();
            EditorGUILayout.LabelField("Identity", ToolEditorStyles.Header);
            EditorGUILayout.PropertyField(id);
            EditorGUILayout.PropertyField(displayName);
            EditorGUILayout.PropertyField(icon);

            if (string.IsNullOrWhiteSpace(ToolEditorSerialization.ReadIdValue(id)))
                EditorGUILayout.HelpBox("ToolId is empty.", MessageType.Warning);

            ToolEditorStyles.EndBox();
        }

        private void DrawSlots()
        {
            ToolEditorStyles.BeginBox();
            EditorGUILayout.LabelField("Supported Slots", ToolEditorStyles.Header);
            EditorGUILayout.LabelField("Slot Count", supportedSlots.arraySize.ToString());

            if (supportedSlots.arraySize == 0)
                EditorGUILayout.HelpBox("No supported slots are assigned.", MessageType.Warning);

            BuildDuplicateSlotSet();
            supportedSlots.isExpanded = EditorGUILayout.Foldout(supportedSlots.isExpanded, "Slot Definitions", true);
            if (supportedSlots.isExpanded)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(supportedSlots.FindPropertyRelative("Array.size"));
                for (var i = 0; i < supportedSlots.arraySize; i++)
                {
                    DrawSlotRow(supportedSlots.GetArrayElementAtIndex(i), i);
                }

                EditorGUI.indentLevel--;
            }

            if (duplicateSlotIds.Count > 0)
                EditorGUILayout.HelpBox("Duplicate slot IDs are assigned to this tool.", MessageType.Warning);

            ToolEditorStyles.EndBox();
        }

        private void DrawSlotRow(SerializedProperty slotProperty, int index)
        {
            var slot = (ToolPartSlotDefinition)slotProperty.objectReferenceValue;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            if (slot == null)
            {
                EditorGUILayout.PropertyField(slotProperty, new GUIContent($"Slot {index + 1}"));
                EditorGUILayout.HelpBox("Slot definition is null.", MessageType.Warning);
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(
                ToolEditorStyles.GetIconTexture(slot.Icon),
                GUILayout.Width(ToolEditorStyles.SmallIconSize),
                GUILayout.Height(ToolEditorStyles.SmallIconSize));
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(string.IsNullOrWhiteSpace(slot.DisplayName) ? slot.name : slot.DisplayName, ToolEditorStyles.SubHeader);
            EditorGUILayout.LabelField(slot.Id.ToString(), ToolEditorStyles.Muted);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.PropertyField(slotProperty, GUIContent.none);

            if (!slot.Id.IsValid)
                EditorGUILayout.HelpBox("SlotId is empty.", MessageType.Warning);

            if (duplicateSlotIds.Contains(slot.Id.ToString()))
                EditorGUILayout.HelpBox("Duplicate slot id.", MessageType.Warning);

            EditorGUILayout.EndVertical();
        }

        private void BuildDuplicateSlotSet()
        {
            duplicateSlotIds.Clear();
            var seen = new HashSet<string>();

            for (var i = 0; i < supportedSlots.arraySize; i++)
            {
                var slot = (ToolPartSlotDefinition)supportedSlots.GetArrayElementAtIndex(i).objectReferenceValue;
                if (slot == null || !slot.Id.IsValid)
                    continue;

                var slotId = slot.Id.ToString();
                if (!seen.Add(slotId))
                    duplicateSlotIds.Add(slotId);
            }
        }
    }
}
