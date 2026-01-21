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