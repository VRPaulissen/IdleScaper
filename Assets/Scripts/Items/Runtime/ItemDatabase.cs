using System.Collections.Generic;
using System.Linq;
using Items.Definitions;
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
            EnsureBuilt();
            return map.TryGetValue(id, out definition);
        }

        /// <summary>
        /// Returns all registered item definitions.
        /// </summary>
        public IReadOnlyList<ItemDefinition> Definitions => definitions;

        private void OnEnable()
        {
            map = null;
        }

        private void EnsureBuilt()
        {
            if (map != null)
                return;

            map = new Dictionary<ItemId, ItemDefinition>(definitions.Count);
            foreach (var def in definitions.Where(def => def != null && def.Id.IsValid))
            {
                map[def.Id] = def;
            }
        }
    }
}