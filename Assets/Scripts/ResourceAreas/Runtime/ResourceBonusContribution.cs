namespace ResourceAreas.Runtime
{
    /// <summary>
    /// Represents one active resource bonus contribution with source metadata.
    /// </summary>
    public readonly struct ResourceBonusContribution
    {
        /// <summary>
        /// Creates a resource bonus contribution.
        /// </summary>
        public ResourceBonusContribution(
            ResourceBonusType type,
            float value,
            string sourceName,
            ResourceBonusSourceType sourceType,
            string sourceId = null)
        {
            Type = type;
            Value = value;
            SourceName = sourceName ?? string.Empty;
            SourceType = sourceType;
            SourceId = sourceId ?? string.Empty;
        }

        /// <summary>
        /// Bonus type contributed by this source.
        /// </summary>
        public ResourceBonusType Type { get; }

        /// <summary>
        /// Bonus value contributed by this source.
        /// </summary>
        public float Value { get; }

        /// <summary>
        /// Human-readable source name.
        /// </summary>
        public string SourceName { get; }

        /// <summary>
        /// Source category for this contribution.
        /// </summary>
        public ResourceBonusSourceType SourceType { get; }

        /// <summary>
        /// Stable source id for this contribution.
        /// </summary>
        public string SourceId { get; }
    }
}
