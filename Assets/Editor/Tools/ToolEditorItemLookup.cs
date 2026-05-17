using System.Collections.Generic;
using Items.Definitions;
using Items.Runtime;
using UnityEditor;

namespace Tools.Editor
{
    /// <summary>
    /// Editor-only lookup cache for resolving item ids to item definition assets.
    /// </summary>
    internal static class ToolEditorItemLookup
    {
        private static readonly Dictionary<ItemId, ItemDefinition> items = new Dictionary<ItemId, ItemDefinition>();
        private static bool built;

        /// <summary>
        /// Attempts to resolve an item definition by stable id.
        /// </summary>
        public static bool TryGet(ItemId itemId, out ItemDefinition itemDefinition)
        {
            EnsureBuilt();
            return items.TryGetValue(itemId, out itemDefinition);
        }

        /// <summary>
        /// Clears and rebuilds the lookup on the next query.
        /// </summary>
        public static void Refresh()
        {
            built = false;
            items.Clear();
        }

        /// <summary>
        /// Draws a compact refresh button for inspectors using this lookup.
        /// </summary>
        public static void DrawRefreshButton()
        {
            if (!EditorGUILayout.LinkButton("Refresh item lookup"))
                return;

            Refresh();
        }

        private static void EnsureBuilt()
        {
            if (built)
                return;

            items.Clear();
            AddDefinitionsFromDatabases();
            AddLooseItemDefinitions();
            built = true;
        }

        private static void AddDefinitionsFromDatabases()
        {
            var guids = AssetDatabase.FindAssets("t:ItemDatabase");
            for (var i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var database = AssetDatabase.LoadAssetAtPath<ItemDatabase>(path);
                if (database == null)
                    continue;

                var definitions = database.Definitions;
                for (var j = 0; j < definitions.Count; j++)
                {
                    AddDefinition(definitions[j]);
                }
            }
        }

        private static void AddLooseItemDefinitions()
        {
            var guids = AssetDatabase.FindAssets("t:ItemDefinition");
            for (var i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                AddDefinition(AssetDatabase.LoadAssetAtPath<ItemDefinition>(path));
            }
        }

        private static void AddDefinition(ItemDefinition definition)
        {
            if (definition == null)
                return;

            if (!definition.Id.IsValid)
                return;

            items[definition.Id] = definition;
        }
    }
}
