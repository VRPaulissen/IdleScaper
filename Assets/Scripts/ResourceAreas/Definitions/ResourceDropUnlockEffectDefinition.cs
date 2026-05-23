using ResourceAreas.Runtime;
using UnityEngine;

namespace ResourceAreas.Definitions
{
    /// <summary>
    /// Placeholder resource bonus effect that marks a drop unlock key for future drop resolver logic.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Resource Areas/Effects/Drop Unlock Effect", fileName = "Effect_DropUnlock")]
    public sealed class ResourceDropUnlockEffectDefinition : ResourceBonusEffectDefinition
    {
        [SerializeField] private string unlockKey;

        /// <summary>
        /// Drop unlock key represented by this effect.
        /// </summary>
        public string UnlockKey => unlockKey;

        /// <inheritdoc />
        public override void AddBonuses(ResourceBonusContext context, ResourceBonusCollection bonuses, ResourceBonusEffectSource source)
        {
        }
    }
}
