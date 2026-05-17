using Items.Definitions;
using Items.Runtime.Diagnostics;
using UnityEngine;

namespace Items.Runtime.Modules
{
    /// <summary>
    /// Describes a consumable effect. Execution is delegated to gameplay systems.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Items/Modules/Consumable", fileName = "Mod_Consumable_")]
    public sealed class ConsumableModule : ItemModule
    {
        [SerializeField] private ConsumableEffectId effectId;
        [SerializeField] private int magnitude;

        /// <summary>
        /// Identifies which effect should be applied when consumed.
        /// </summary>
        public ConsumableEffectId EffectId => effectId;

        /// <summary>
        /// Effect strength (meaning defined by the effect).
        /// </summary>
        public int Magnitude => magnitude;

        /// <inheritdoc />
        public override void CollectDiagnostics(ItemDefinition definition, System.Collections.Generic.List<ItemDiagnostic> results)
        {
            if (results == null)
                return;

            if (magnitude < 0)
                results.Add(ItemDiagnostic.Warning("CONSUMABLE_MAGNITUDE_NEGATIVE", $"ConsumableModule '{name}' has a negative magnitude.", this, definition != null ? definition.Id : default));
        }
    }

    /// <summary>
    /// Stable identifiers for consumable behaviors.
    /// </summary>
    public enum ConsumableEffectId
    {
        Heal,
        RestoreStamina,
        BoostSkillTemporary
    }
}
