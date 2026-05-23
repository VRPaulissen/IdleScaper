using System.Collections.Generic;
using ResourceAreas.Runtime;
using UnityEngine;

namespace ResourceAreas.Definitions
{
    /// <summary>
    /// Asset catalog for looking up resource area definitions.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Resources/Resource Area Catalog", fileName = "ResourceAreaCatalog")]
    public sealed class ResourceAreaCatalog : ScriptableObject
    {
        [SerializeField] private List<ResourceAreaDefinition> areas = new List<ResourceAreaDefinition>();

        /// <summary>
        /// All registered resource area definitions.
        /// </summary>
        public IReadOnlyList<ResourceAreaDefinition> Areas => areas;

        /// <summary>
        /// Attempts to resolve a resource area definition by id.
        /// </summary>
        public bool TryGet(ResourceAreaId id, out ResourceAreaDefinition definition)
        {
            definition = null;

            if (!id.IsValid)
                return false;

            foreach (var candidate in areas)
            {
                if (candidate == null)
                    continue;

                if (candidate.Id != id)
                    continue;

                definition = candidate;
                return true;
            }

            return false;
        }
    }
}
