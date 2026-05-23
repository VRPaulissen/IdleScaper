namespace ResourceAreas.Runtime
{
    /// <summary>
    /// Event data raised when resource area experience or level changes.
    /// </summary>
    public readonly struct ResourceAreaProgressChangedEventData
    {
        /// <summary>
        /// Creates event data for a resource area progress change.
        /// </summary>
        public ResourceAreaProgressChangedEventData(
            ResourceAreaId areaId,
            int previousLevel,
            int newLevel,
            float previousXp,
            float currentXp,
            float addedXp)
        {
            AreaId = areaId;
            PreviousLevel = previousLevel;
            NewLevel = newLevel;
            PreviousXp = previousXp;
            CurrentXp = currentXp;
            AddedXp = addedXp;
        }

        /// <summary>
        /// Resource area id whose progress changed.
        /// </summary>
        public ResourceAreaId AreaId { get; }

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
