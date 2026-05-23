using ResourceAreas.Runtime;
using UnityEngine;

namespace ResourceAreas.Definitions
{
    /// <summary>
    /// Resource bonus effect that adds a flat bonus value.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Resource Areas/Effects/Flat Bonus Effect", fileName = "Effect_FlatBonus")]
    public sealed class FlatResourceBonusEffectDefinition : ResourceBonusEffectDefinition
    {
        [SerializeField] private ResourceBonusType bonusType;
        [SerializeField] private float value;

        /// <summary>
        /// Bonus type added by this effect.
        /// </summary>
        public ResourceBonusType BonusType => bonusType;

        /// <summary>
        /// Flat bonus value added by this effect.
        /// </summary>
        public float Value => value;

        /// <inheritdoc />
        public override void AddBonuses(ResourceBonusContext context, ResourceBonusCollection bonuses, ResourceBonusEffectSource source)
        {
            if (bonuses == null)
                return;

            if (float.IsNaN(value) || float.IsInfinity(value))
                return;

            bonuses.Add(bonusType, value, source.SourceName, source.SourceType, source.SourceId);
        }
    }
}
