using System.Collections.Generic;
using Items.Runtime;
using Items.Runtime.Diagnostics;
using Resource.Definitions;
using UnityEditor;
using UnityEngine;

namespace Tools.Editor
{
    /// <summary>
    /// Custom inspector for resource definition validation diagnostics.
    /// </summary>
    [CustomEditor(typeof(ResourceDefinition))]
    internal sealed class ResourceDefinitionEditor : UnityEditor.Editor
    {
        private readonly List<ItemDiagnostic> diagnostics = new List<ItemDiagnostic>();

        private SerializedProperty aliveSprite;
        private SerializedProperty depletedSprite;
        private SerializedProperty durabilityMax;
        private SerializedProperty hitIntervalSeconds;
        private SerializedProperty baseDamagePerHit;
        private SerializedProperty entries;

        private ItemDatabase itemDatabase;
        private bool hasValidated;
        private bool showDiagnostics = true;

        private void OnEnable()
        {
            aliveSprite = serializedObject.FindProperty("aliveSprite");
            depletedSprite = serializedObject.FindProperty("depletedSprite");
            durabilityMax = serializedObject.FindProperty("durabilityMax");
            hitIntervalSeconds = serializedObject.FindProperty("hitIntervalSeconds");
            baseDamagePerHit = serializedObject.FindProperty("baseDamagePerHit");
            entries = serializedObject.FindProperty("entries");
            itemDatabase = FindSingleItemDatabase();
        }

        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawResourceData();
            ToolEditorStyles.Space();
            DrawValidation();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawResourceData()
        {
            ToolEditorStyles.BeginBox();
            EditorGUILayout.LabelField("Resource Definition", ToolEditorStyles.Header);
            EditorGUILayout.PropertyField(aliveSprite);
            EditorGUILayout.PropertyField(depletedSprite);
            EditorGUILayout.PropertyField(durabilityMax);
            EditorGUILayout.PropertyField(hitIntervalSeconds);
            EditorGUILayout.PropertyField(baseDamagePerHit);
            EditorGUILayout.PropertyField(entries, true);
            ToolEditorStyles.EndBox();
        }

        private void DrawValidation()
        {
            ToolEditorStyles.BeginBox();
            EditorGUILayout.LabelField("Validation", ToolEditorStyles.Header);
            itemDatabase = (ItemDatabase)EditorGUILayout.ObjectField("Item Database", itemDatabase, typeof(ItemDatabase), false);

            if (GUILayout.Button("Validate Resource", GUILayout.Height(24)))
                Validate();

            if (!hasValidated)
            {
                EditorGUILayout.HelpBox("Run validation to check resource timing, durability, sprites, and drop data.", MessageType.Info);
                ToolEditorStyles.EndBox();
                return;
            }

            DrawSummary();
            if (diagnostics.Count == 0)
            {
                EditorGUILayout.HelpBox("No resource diagnostics found.", MessageType.Info);
                ToolEditorStyles.EndBox();
                return;
            }

            showDiagnostics = EditorGUILayout.Foldout(showDiagnostics, $"Diagnostics ({diagnostics.Count})", true);
            if (showDiagnostics)
            {
                for (var i = 0; i < diagnostics.Count; i++)
                    DrawDiagnostic(diagnostics[i]);
            }

            ToolEditorStyles.EndBox();
        }

        private void Validate()
        {
            diagnostics.Clear();
            var resource = (ResourceDefinition)target;
            resource.CollectDiagnostics(diagnostics, itemDatabase);
            hasValidated = true;
        }

        private void DrawSummary()
        {
            CountDiagnostics(out var infos, out var warnings, out var errors);
            EditorGUILayout.LabelField("Info", infos.ToString());
            EditorGUILayout.LabelField("Warnings", warnings.ToString());
            EditorGUILayout.LabelField("Errors", errors.ToString());
        }

        private void DrawDiagnostic(ItemDiagnostic diagnostic)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.HelpBox($"{diagnostic.Code}: {diagnostic.Message}", ToMessageType(diagnostic.Severity));
            GUI.enabled = diagnostic.Context != null;
            if (GUILayout.Button("Ping", GUILayout.Width(48), GUILayout.Height(38)))
                EditorGUIUtility.PingObject(diagnostic.Context);

            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
        }

        private void CountDiagnostics(out int infos, out int warnings, out int errors)
        {
            infos = 0;
            warnings = 0;
            errors = 0;

            for (var i = 0; i < diagnostics.Count; i++)
            {
                switch (diagnostics[i].Severity)
                {
                    case ItemDiagnosticSeverity.Info:
                        infos++;
                        break;
                    case ItemDiagnosticSeverity.Warning:
                        warnings++;
                        break;
                    case ItemDiagnosticSeverity.Error:
                        errors++;
                        break;
                }
            }
        }

        private static MessageType ToMessageType(ItemDiagnosticSeverity severity)
        {
            return severity switch
            {
                ItemDiagnosticSeverity.Error => MessageType.Error,
                ItemDiagnosticSeverity.Warning => MessageType.Warning,
                _ => MessageType.Info
            };
        }

        private static ItemDatabase FindSingleItemDatabase()
        {
            var guids = AssetDatabase.FindAssets("t:ItemDatabase");
            if (guids.Length != 1)
                return null;

            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<ItemDatabase>(path);
        }
    }
}
