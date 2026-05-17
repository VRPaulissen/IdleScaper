using System.Collections.Generic;
using Items.Definitions;
using Items.Runtime.Diagnostics;
using UnityEngine;

namespace Items.Runtime
{
    /// <summary>
    /// Lookup registry for item definitions.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Items/Item Database", fileName = "ItemDatabase")]
    public sealed class ItemDatabase : ScriptableObject
    {
        [SerializeField] private List<ItemDefinition> definitions = new();

        private Dictionary<ItemId, ItemDefinition> map;

        /// <summary>
        /// Attempts to resolve an item definition by id.
        /// </summary>
        public bool TryGet(ItemId id, out ItemDefinition definition)
        {
            return TryGetItem(id, out definition);
        }

        /// <summary>
        /// Attempts to resolve an item definition by stable item id.
        /// </summary>
        public bool TryGetItem(ItemId itemId, out ItemDefinition item)
        {
            item = null;
            if (!itemId.IsValid)
                return false;

            EnsureBuilt();
            return map.TryGetValue(itemId, out item);
        }

        /// <summary>
        /// Returns true when this database contains a valid item for the given id.
        /// </summary>
        public bool Contains(ItemId itemId)
        {
            return TryGetItem(itemId, out _);
        }

        /// <summary>
        /// Returns a snapshot of all registered item definitions.
        /// </summary>
        public IReadOnlyList<ItemDefinition> GetAll()
        {
            if (definitions == null)
                return new List<ItemDefinition>();

            return new List<ItemDefinition>(definitions);
        }

        /// <summary>
        /// Returns all registered item definitions.
        /// </summary>
        public IReadOnlyList<ItemDefinition> Definitions => definitions;

        /// <summary>
        /// Collects and returns non-mutating diagnostics for this item database.
        /// </summary>
        public List<ItemDiagnostic> GetDiagnostics()
        {
            var results = new List<ItemDiagnostic>();
            CollectDiagnostics(results);
            return results;
        }

        /// <summary>
        /// Collects and returns non-mutating diagnostics for this item database.
        /// </summary>
        public List<ItemDiagnostic> Validate()
        {
            return GetDiagnostics();
        }

        /// <summary>
        /// Appends non-mutating diagnostics for this item database and its registered items.
        /// </summary>
        public void CollectDiagnostics(List<ItemDiagnostic> results)
        {
            if (results == null)
                return;

            if (definitions == null)
            {
                results.Add(ItemDiagnostic.Error("ITEM_DATABASE_DEFINITIONS_NULL", $"ItemDatabase '{name}' has a null definitions list.", this));
                return;
            }

            var ids = new Dictionary<ItemId, ItemDefinition>();
            var references = new HashSet<ItemDefinition>();

            for (var i = 0; i < definitions.Count; i++)
            {
                var definition = definitions[i];
                if (definition == null)
                {
                    results.Add(ItemDiagnostic.Error("ITEM_DATABASE_ENTRY_NULL", $"ItemDatabase '{name}' has a null item entry at index {i}.", this));
                    continue;
                }

                if (!references.Add(definition))
                    results.Add(ItemDiagnostic.Warning("ITEM_DATABASE_DUPLICATE_REFERENCE", $"ItemDatabase '{name}' contains item '{definition.name}' more than once.", definition, definition.Id));

                if (!definition.Id.IsValid)
                {
                    results.Add(ItemDiagnostic.Error("ITEM_DATABASE_ENTRY_ID_MISSING", $"ItemDatabase '{name}' contains item '{definition.name}' with no ItemId.", definition, definition.Id));
                }
                else if (ids.TryGetValue(definition.Id, out var first))
                {
                    results.Add(ItemDiagnostic.Error("ITEM_DATABASE_DUPLICATE_ID", $"Duplicate ItemId '{definition.Id}' in '{first.name}' and '{definition.name}'.", definition, definition.Id));
                }
                else
                {
                    ids.Add(definition.Id, definition);
                }

                definition.CollectDiagnostics(results);
            }
        }

        private void OnEnable()
        {
            map = null;
        }

        private void EnsureBuilt()
        {
            if (map != null)
                return;

            var capacity = definitions != null ? definitions.Count : 0;
            map = new Dictionary<ItemId, ItemDefinition>(capacity);
            if (definitions == null)
                return;

            for (var i = 0; i < definitions.Count; i++)
            {
                var def = definitions[i];
                if (def == null || !def.Id.IsValid)
                    continue;

                map[def.Id] = def;
            }
        }
    }
}
