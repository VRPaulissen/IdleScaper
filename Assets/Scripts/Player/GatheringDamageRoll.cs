namespace Player
{
/// <summary>
    /// Represents the outcome of a gathering damage roll, including the hit type and final damage.
    /// </summary>
    public readonly struct GatheringDamageRoll
    {
        /// <summary>
        /// Base damage before any modifiers are applied.
        /// </summary>
        public int BaseDamage { get; }

        /// <summary>
        /// Multiplier applied to <see cref="BaseDamage"/> (1 for normal hits).
        /// </summary>
        public int Multiplier { get; }

        /// <summary>
        /// Final damage after applying <see cref="Multiplier"/>.
        /// </summary>
        public int FinalDamage { get; }

        /// <summary>
        /// Type of hit that produced this roll.
        /// </summary>
        public GatheringHitType HitType { get; }

        /// <summary>
        /// True if the hit was <see cref="GatheringHitType.Crit"/> or <see cref="GatheringHitType.UltraCrit"/>.
        /// </summary>
        public bool IsCritical => HitType != GatheringHitType.Normal;

        /// <summary>
        /// Creates a new roll result.
        /// </summary>
        public GatheringDamageRoll(int baseDamage, int multiplier, GatheringHitType hitType)
        {
            BaseDamage = baseDamage;
            Multiplier = multiplier < 1 ? 1 : multiplier;
            HitType = hitType;

            // Avoid negative or nonsensical results.
            var clampedBase = baseDamage < 0 ? 0 : baseDamage;
            FinalDamage = clampedBase * Multiplier;
        }

        /// <summary>
        /// Creates a normal (non-critical) roll.
        /// </summary>
        public static GatheringDamageRoll Normal(int baseDamage)
        {
            return new GatheringDamageRoll(baseDamage, 1, GatheringHitType.Normal);
        }

        /// <summary>
        /// Creates a crit roll.
        /// </summary>
        public static GatheringDamageRoll Crit(int baseDamage, int multiplier)
        {
            return new GatheringDamageRoll(baseDamage, multiplier, GatheringHitType.Crit);
        }

        /// <summary>
        /// Creates an ultra crit roll.
        /// </summary>
        public static GatheringDamageRoll UltraCrit(int baseDamage, int multiplier)
        {
            return new GatheringDamageRoll(baseDamage, multiplier, GatheringHitType.UltraCrit);
        }
    }
}