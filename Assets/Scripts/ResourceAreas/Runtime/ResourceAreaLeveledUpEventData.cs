namespace ResourceAreas.Runtime
{
    /// <summary>
    /// Event data raised when a resource area gains a level.
    /// </summary>
    public readonly struct ResourceAreaLeveledUpEventData
    {
        /// <summary>
        /// Creates event data for a resource area level-up.
        /// </summary>
        public ResourceAreaLeveledUpEventData(ResourceAreaId areaId, int previousLevel, int newLevel, float currentXp, float addedXp)
        {
            AreaId = areaId;
            PreviousLevel = previousLevel;
            NewLevel = newLevel;
            CurrentXp = currentXp;
            AddedXp = addedXp;
        }

        /// <summary>
        /// Resource area id that leveled up.
        /// </summary>
        public ResourceAreaId AreaId { get; }

        /// <summary>
        /// Level before the level-up.
        /// </summary>
        public int PreviousLevel { get; }

        /// <summary>
        /// Level after the level-up.
        /// </summary>
        public int NewLevel { get; }

        /// <summary>
        /// Current experience after the level-up.
        /// </summary>
        public float CurrentXp { get; }

        /// <summary>
        /// Experience added by the operation that caused the level-up.
        /// </summary>
        public float AddedXp { get; }
    }
}
