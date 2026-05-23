namespace ResourceAreas.Runtime
{
    /// <summary>
    /// Event data raised when resource experience or level changes.
    /// </summary>
    public readonly struct ResourceProgressChangedEventData
    {
        /// <summary>
        /// Creates event data for a resource progress change.
        /// </summary>
        public ResourceProgressChangedEventData(
            ResourceId resourceId,
            int previousLevel,
            int newLevel,
            float previousXp,
            float currentXp,
            float addedXp)
        {
            ResourceId = resourceId;
            PreviousLevel = previousLevel;
            NewLevel = newLevel;
            PreviousXp = previousXp;
            CurrentXp = currentXp;
            AddedXp = addedXp;
        }

        /// <summary>
        /// Resource id whose progress changed.
        /// </summary>
        public ResourceId ResourceId { get; }

        /// <summary>
        /// Level before the progress change.
        /// </summary>
        public int PreviousLevel { get; }

        /// <summary>
        /// Level after the progress change.
        /// </summary>
        public int NewLevel { get; }

        /// <summary>
        /// Experience before the progress change.
        /// </summary>
        public float PreviousXp { get; }

        /// <summary>
        /// Current experience after the progress change.
        /// </summary>
        public float CurrentXp { get; }

        /// <summary>
        /// Experience added by the progress change.
        /// </summary>
        public float AddedXp { get; }
    }
}
