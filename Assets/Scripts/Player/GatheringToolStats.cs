using System;
using Utilities.Calculations;

namespace Player
{
    /// <summary>
    /// Tool stats used by the resource interaction loop.
    /// </summary>
    public readonly struct GatheringToolStats
    {
        /// <summary>
        /// Time between hits.
        /// </summary>
        public float HitIntervalSeconds { get; }

        /// <summary>
        /// Base damage per hit (before crit rolls).
        /// </summary>
        public int BaseDamagePerHit { get; }

        /// <summary>
        /// Chance [0..1] to crit (applies if ultra crit did not trigger).
        /// </summary>
        public float CritChance { get; }

        /// <summary>
        /// Crit damage multiplier.
        /// </summary>
        public int CritMultiplier { get; }

        /// <summary>
        /// Chance [0..1] to ultra crit (rolled first).
        /// </summary>
        public float UltraCritChance { get; }

        /// <summary>
        /// Ultra crit damage multiplier.
        /// </summary>
        public int UltraCritMultiplier { get; }

        /// <summary>
        /// Computes the rolled damage for this hit (Ultra Crit first, then Crit).
        /// </summary>
        public GatheringDamageRoll RollDamage(IRandomSource randomSource)
        {
            var baseDamage = BaseDamagePerHit;
            if (baseDamage <= 0)
                return GatheringDamageRoll.Normal(0);

            if (randomSource == null)
                return GatheringDamageRoll.Normal(baseDamage);

            if (Probability.Roll(randomSource, UltraCritChance))
                return GatheringDamageRoll.UltraCrit(baseDamage, UltraCritMultiplier);

            if (Probability.Roll(randomSource, CritChance))
                return GatheringDamageRoll.Crit(baseDamage, CritMultiplier);

            return GatheringDamageRoll.Normal(baseDamage);
        }

        /// <summary>
        /// Creates a new stats payload for gathering.
        /// </summary>
        public GatheringToolStats(
            float hitIntervalSeconds,
            int baseDamagePerHit,
            float critChance,
            int critMultiplier,
            float ultraCritChance,
            int ultraCritMultiplier)
        {
            HitIntervalSeconds = hitIntervalSeconds;
            BaseDamagePerHit = baseDamagePerHit;

            CritChance = Clamp01(critChance);
            CritMultiplier = Math.Max(1, critMultiplier);

            UltraCritChance = Clamp01(ultraCritChance);
            UltraCritMultiplier = Math.Max(1, ultraCritMultiplier);
        }
        
        private static float Clamp01(float value)
        {
            return value switch
            {
                < 0f => 0f,
                > 1f => 1f,
                _ => value
            };
        }
    }
}