using ResourceAreas.Runtime;
using UnityEngine;

namespace ResourceAreas.Definitions
{
    /// <summary>
    /// Base asset for a resource bonus effect that can add one or more bonus contributions.
    /// </summary>
    public abstract class ResourceBonusEffectDefinition : ScriptableObject
    {
        /// <summary>
        /// Adds bonus contributions for the given context and source metadata.
        /// </summary>
        public abstract void AddBonuses(ResourceBonusContext context, ResourceBonusCollection bonuses, ResourceBonusEffectSource source);
    }
}
