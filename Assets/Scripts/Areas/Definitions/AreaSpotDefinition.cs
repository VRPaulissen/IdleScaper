using IdleScaper.Skills.Actions;
using UnityEngine;

namespace IdleScaper.Areas.Definitions
{
    /// <summary>
    /// Describes a type of spot that can appear in an idle area.
    /// </summary>
    [System.Serializable]
    public struct AreaSpotDefinition
    {
        /// <summary>Action performed at this spot (tree, rock, etc.).</summary>
        public SkillActionDefinition Action;

        /// <summary>Prefab visual for this spot.</summary>
        public GameObject SpotPrefab;

        /// <summary>Relative spawn weight.</summary>
        public float SpawnWeight;

        /// <summary>Maximum instances of this spot.</summary>
        public int MaxInstances;
    }
}