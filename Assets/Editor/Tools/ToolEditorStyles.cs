using UnityEditor;
using UnityEngine;

namespace Tools.Editor
{
    /// <summary>
    /// Shared IMGUI styles for permanent tool editor inspectors.
    /// </summary>
    internal static class ToolEditorStyles
    {
        public const float LargeIconSize = 40f;
        public const float SmallIconSize = 28f;
        public const float SectionSpacing = 6f;

        private static GUIStyle header;
        private static GUIStyle subHeader;
        private static GUIStyle muted;

        /// <summary>
        /// Bold section header style.
        /// </summary>
        public static GUIStyle Header => header ??= new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 13
        };

        /// <summary>
        /// Compact bold row header style.
        /// </summary>
        public static GUIStyle SubHeader => subHeader ??= new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 11
        };

        /// <summary>
        /// Secondary detail label style.
        /// </summary>
        public static GUIStyle Muted => muted ??= new GUIStyle(EditorStyles.miniLabel)
        {
            wordWrap = false
        };

        /// <summary>
        /// Starts a boxed inspector section.
        /// </summary>
        public static void BeginBox()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        }

        /// <summary>
        /// Ends a boxed inspector section.
        /// </summary>
        public static void EndBox()
        {
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Draws standard vertical spacing between inspector sections.
        /// </summary>
        public static void Space()
        {
            EditorGUILayout.Space(SectionSpacing);
        }

        /// <summary>
        /// Returns a usable texture for optional sprite icons.
        /// </summary>
        public static Texture GetIconTexture(Sprite sprite)
        {
            return sprite != null ? sprite.texture : Texture2D.grayTexture;
        }
    }
}
