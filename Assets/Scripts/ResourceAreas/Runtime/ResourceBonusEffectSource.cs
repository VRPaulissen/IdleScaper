namespace ResourceAreas.Runtime
{
    /// <summary>
    /// Carries source metadata used when a resource bonus effect adds contributions.
    /// </summary>
    public readonly struct ResourceBonusEffectSource
    {
        /// <summary>
        /// Creates resource bonus effect source metadata.
        /// </summary>
        public ResourceBonusEffectSource(string sourceName, ResourceBonusSourceType sourceType, string sourceId)
        {
            SourceName = sourceName ?? string.Empty;
            SourceType = sourceType;
            SourceId = sourceId ?? string.Empty;
        }

        /// <summary>
        /// Human-readable source name.
        /// </summary>
        public string SourceName { get; }

        /// <summary>
        /// Source category for contributions added by the effect.
        /// </summary>
        public ResourceBonusSourceType SourceType { get; }

        /// <summary>
        /// Stable source id for contributions added by the effect.
        /// </summary>
        public string SourceId { get; }
    }
}
