namespace ResourceAreas.Runtime
{
    /// <summary>
    /// Event data raised when a resource gains a level.
    /// </summary>
    public readonly struct ResourceLeveledUpEventData
    {
        /// <summary>
        /// Creates event data for a resource level-up.
        /// </summary>
        public ResourceLeveledUpEventData(ResourceId resourceId, int previousLevel, int newLevel, float currentXp, float addedXp)
        {
            ResourceId = resourceId;
            PreviousLevel = previousLevel;
            NewLevel = newLevel;
            CurrentXp = currentXp;
            AddedXp = addedXp;
        }

        /// <summary>
        /// Resource id that leveled up.
        /// </summary>
        public ResourceId ResourceId { get; }

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
