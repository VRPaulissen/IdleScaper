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
        public readonly float HitIntervalSeconds;

        /// <summary>
        /// Damage per hit.
        /// </summary>
        public readonly int DamagePerHit;

        /// <summary>
        /// Creates tool stats.
        /// </summary>
        public GatheringToolStats(float hitIntervalSeconds, int damagePerHit)
        {
            HitIntervalSeconds = hitIntervalSeconds;
            DamagePerHit = damagePerHit;
        }
    }
}