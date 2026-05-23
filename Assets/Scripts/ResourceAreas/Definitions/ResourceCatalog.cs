using System.Collections.Generic;
using ResourceAreas.Runtime;
using UnityEngine;

namespace ResourceAreas.Definitions
{
    /// <summary>
    /// Asset catalog for looking up resource definitions.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Resources/Resource Catalog", fileName = "ResourceCatalog")]
    public sealed class ResourceCatalog : ScriptableObject
    {
        [SerializeField] private List<ResourceDefinition> resources = new List<ResourceDefinition>();

        /// <summary>
        /// All registered resource definitions.
        /// </summary>
        public IReadOnlyList<ResourceDefinition> Resources => resources;

        /// <summary>
        /// Attempts to resolve a resource definition by id.
        /// </summary>
        public bool TryGet(ResourceId id, out ResourceDefinition definition)
        {
            definition = null;

            if (!id.IsValid)
                return false;

            for (var i = 0; i < resources.Count; i++)
            {
                var candidate = resources[i];
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
