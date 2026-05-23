using ResourceAreas.Runtime;
using UnityEngine;

namespace ResourceAreas.Definitions
{
    /// <summary>
    /// Resource bonus effect that adds a flat chance bonus for a specific drop key.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Resource Areas/Effects/Specific Drop Chance Bonus Effect", fileName = "Effect_SpecificDropChance")]
    public sealed class SpecificDropChanceBonusEffectDefinition : ResourceBonusEffectDefinition
    {
        [SerializeField] private string dropKey;
        [SerializeField] private float chanceValue;

        /// <summary>
        /// Drop key this chance bonus targets.
        /// </summary>
        public string DropKey => dropKey;

        /// <summary>
        /// Flat chance value added by this effect.
        /// </summary>
        public float ChanceValue => chanceValue;

        /// <inheritdoc />
        public override void AddBonuses(ResourceBonusContext context, ResourceBonusCollection bonuses, ResourceBonusEffectSource source)
        {
            if (bonuses == null)
                return;

            if (float.IsNaN(chanceValue) || float.IsInfinity(chanceValue))
                return;

            bonuses.Add(
                ResourceBonusType.SpecificDropChanceFlat,
                chanceValue,
                source.SourceName,
                source.SourceType,
                GetSourceId(source));
        }

        private string GetSourceId(ResourceBonusEffectSource source)
        {
            if (string.IsNullOrWhiteSpace(dropKey))
                return source.SourceId;

            if (string.IsNullOrWhiteSpace(source.SourceId))
                return dropKey;

            return source.SourceId + ".drop." + dropKey;
        }
    }
}
