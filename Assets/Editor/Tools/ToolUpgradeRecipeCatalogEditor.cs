using System.Collections.Generic;
using Tools.Definitions;
using UnityEditor;
using UnityEngine;

namespace Tools.Editor
{
    /// <summary>
    /// Custom inspector for permanent tool upgrade recipe catalogs.
    /// </summary>
    [CustomEditor(typeof(ToolUpgradeRecipeCatalog))]
    internal sealed class ToolUpgradeRecipeCatalogEditor : UnityEditor.Editor
    {
        private readonly List<string> validationMessages = new List<string>();

        private SerializedProperty recipes;
        private ToolRecipeCatalogValidation validation;
        private bool hasValidated;
        private bool showValidationMessages = true;

        private void OnEnable()
        {
            recipes = serializedObject.FindProperty("recipes");
        }

        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawSummary();
            ToolEditorStyles.Space();
            DrawRecipeList();
            ToolEditorStyles.Space();
            DrawValidation();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSummary()
        {
            ToolEditorStyles.BeginBox();
            EditorGUILayout.LabelField("Upgrade Recipe Catalog", ToolEditorStyles.Header);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Recipes", recipes.arraySize.ToString());

            if (hasValidated)
            {
                EditorGUILayout.LabelField("Duplicate Keys", validation.DuplicateKeyCount.ToString());
                EditorGUILayout.LabelField("Invalid Recipes", validation.InvalidRecipeCount.ToString());
            }

            EditorGUILayout.EndHorizontal();
            ToolEditorStyles.EndBox();
        }

        private void DrawRecipeList()
        {
            ToolEditorStyles.BeginBox();
            EditorGUILayout.PropertyField(recipes, new GUIContent("Recipe References"), true);
            ToolEditorStyles.EndBox();
        }

        private void DrawValidation()
        {
            ToolEditorStyles.BeginBox();
            EditorGUILayout.LabelField("Validation", ToolEditorStyles.Header);

            if (GUILayout.Button("Validate Catalog", GUILayout.Height(24)))
                Validate();

            if (GUILayout.Button("Refresh Item Lookup"))
                ToolEditorItemLookup.Refresh();

            if (!hasValidated)
            {
                EditorGUILayout.HelpBox("Run validation to check recipe keys, levels, costs, and item references.", MessageType.Info);
                ToolEditorStyles.EndBox();
                return;
            }

            if (validationMessages.Count == 0)
            {
                EditorGUILayout.HelpBox("No validation issues found.", MessageType.Info);
                ToolEditorStyles.EndBox();
                return;
            }

            showValidationMessages = EditorGUILayout.Foldout(showValidationMessages, $"Issues ({validationMessages.Count})", true);
            if (!showValidationMessages)
            {
                ToolEditorStyles.EndBox();
                return;
            }

            EditorGUI.indentLevel++;
            for (var i = 0; i < validationMessages.Count; i++)
            {
                EditorGUILayout.HelpBox(validationMessages[i], MessageType.Warning);
            }
            EditorGUI.indentLevel--;

            ToolEditorStyles.EndBox();
        }

        private void Validate()
        {
            var catalog = (ToolUpgradeRecipeCatalog)target;
            validation = ToolEditorValidation.ValidateCatalog(catalog, validationMessages);
            hasValidated = true;
        }
    }
}
