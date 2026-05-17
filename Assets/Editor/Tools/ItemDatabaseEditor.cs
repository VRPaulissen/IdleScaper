using System.Collections.Generic;
using Items.Runtime;
using Items.Runtime.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace Tools.Editor
{
    /// <summary>
    /// Custom inspector for item database validation diagnostics.
    /// </summary>
    [CustomEditor(typeof(ItemDatabase))]
    internal sealed class ItemDatabaseEditor : UnityEditor.Editor
    {
        private readonly List<ItemDiagnostic> diagnostics = new List<ItemDiagnostic>();

        private SerializedProperty definitions;
        private bool hasValidated;
        private bool showDiagnostics = true;

        private void OnEnable()
        {
            definitions = serializedObject.FindProperty("definitions");
        }

        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            ToolEditorStyles.BeginBox();
            EditorGUILayout.LabelField("Item Database", ToolEditorStyles.Header);
            EditorGUILayout.PropertyField(definitions, true);
            ToolEditorStyles.EndBox();

            ToolEditorStyles.Space();
            DrawValidation();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawValidation()
        {
            ToolEditorStyles.BeginBox();
            EditorGUILayout.LabelField("Validation", ToolEditorStyles.Header);
            if (GUILayout.Button("Validate Database", GUILayout.Height(24)))
                Validate();

            if (!hasValidated)
            {
                EditorGUILayout.HelpBox("Run validation to check item IDs, modules, stack settings, and duplicate database entries.", MessageType.Info);
                ToolEditorStyles.EndBox();
                return;
            }

            DrawSummary();
            if (diagnostics.Count == 0)
            {
                EditorGUILayout.HelpBox("No item diagnostics found.", MessageType.Info);
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
            var database = (ItemDatabase)target;
            diagnostics.AddRange(database.GetDiagnostics());
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
    }
}
