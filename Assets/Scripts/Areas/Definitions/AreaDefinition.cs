using IdleScaper.Areas.Core;
using UnityEngine;

namespace IdleScaper.Areas.Definitions
{
    /// <summary>
    /// Defines an area configuration.
    /// </summary>
    [CreateAssetMenu(menuName = "IdleScaper/Areas/Area Definition")]
    public class AreaDefinition : ScriptableObject
    {
        /// <summary>Unique identifier for this area.</summary>
        public string Id;

        /// <summary>Display name shown in UI.</summary>
        public string DisplayName;

        /// <summary>Biome of this area.</summary>
        public Biome Biome;

        /// <summary>Skill requirements to enter this area.</summary>
        public AreaSkillRequirement[] EntryRequirements;

        /// <summary>Possible resource spots that can spawn in this area.</summary>
        public AreaSpotDefinition[] Spots;
    }
}