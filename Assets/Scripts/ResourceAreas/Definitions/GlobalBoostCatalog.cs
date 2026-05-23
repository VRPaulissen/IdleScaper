using System.Collections.Generic;
using ResourceAreas.Runtime;
using UnityEngine;

namespace ResourceAreas.Definitions
{
    /// <summary>
    /// Asset catalog for looking up global resource boost definitions.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Resource Areas/Global Boost Catalog", fileName = "GlobalBoostCatalog")]
    public sealed class GlobalBoostCatalog : ScriptableObject
    {
        [SerializeField] private List<GlobalBoostDefinition> boosts = new List<GlobalBoostDefinition>();

        /// <summary>
        /// All registered global resource boost definitions.
        /// </summary>
        public IReadOnlyList<GlobalBoostDefinition> Boosts => boosts;

        /// <summary>
        /// Attempts to resolve a global resource boost definition by id.
        /// </summary>
        public bool TryGet(GlobalBoostId id, out GlobalBoostDefinition definition)
        {
            definition = null;

            if (!id.IsValid)
                return false;

            for (var i = 0; i < boosts.Count; i++)
            {
                var candidate = boosts[i];
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
