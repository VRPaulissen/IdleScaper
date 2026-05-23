using ResourceAreas.Runtime;
using UnityEngine;

namespace ResourceAreas.Definitions
{
    /// <summary>
    /// Resource bonus effect that adds an additive multiplier bonus value.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Resource Areas/Effects/Multiplier Bonus Effect", fileName = "Effect_MultiplierBonus")]
    public sealed class MultiplierResourceBonusEffectDefinition : ResourceBonusEffectDefinition
    {
        [SerializeField] private ResourceBonusType bonusType;
        [SerializeField] private float multiplierValue;

        /// <summary>
        /// Bonus type added by this effect.
        /// </summary>
        public ResourceBonusType BonusType => bonusType;

        /// <summary>
        /// Additive multiplier value added by this effect.
        /// </summary>
        public float MultiplierValue => multiplierValue;

        /// <inheritdoc />
        public override void AddBonuses(ResourceBonusContext context, ResourceBonusCollection bonuses, ResourceBonusEffectSource source)
        {
            if (bonuses == null)
                return;

            if (float.IsNaN(multiplierValue) || float.IsInfinity(multiplierValue))
                return;

            bonuses.Add(bonusType, multiplierValue, source.SourceName, source.SourceType, source.SourceId);
        }
    }
}
