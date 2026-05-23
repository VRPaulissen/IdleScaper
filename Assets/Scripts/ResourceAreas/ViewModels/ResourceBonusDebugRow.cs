using ResourceAreas.Runtime;

namespace ResourceAreas.ViewModels
{
    /// <summary>
    /// Read model for one resolved resource bonus contribution.
    /// </summary>
    public sealed class ResourceBonusDebugRow
    {
        /// <summary>
        /// Creates a resolved resource bonus contribution row.
        /// </summary>
        public ResourceBonusDebugRow(
            ResourceBonusType bonusType,
            float value,
            string formattedValue,
            ResourceBonusSourceType sourceType,
            string sourceName,
            string sourceId)
        {
            BonusType = bonusType;
            Value = value;
            FormattedValue = formattedValue ?? string.Empty;
            SourceType = sourceType;
            SourceName = sourceName ?? string.Empty;
            SourceId = sourceId ?? string.Empty;
        }

        /// <summary>
        /// Bonus type contributed by this row.
        /// </summary>
        public ResourceBonusType BonusType { get; }

        /// <summary>
        /// Raw contribution value.
        /// </summary>
        public float Value { get; }

        /// <summary>
        /// Human-readable contribution value.
        /// </summary>
        public string FormattedValue { get; }

        /// <summary>
        /// Source category for the contribution.
        /// </summary>
        public ResourceBonusSourceType SourceType { get; }

        /// <summary>
        /// Human-readable source name.
        /// </summary>
        public string SourceName { get; }

        /// <summary>
        /// Stable source id.
        /// </summary>
        public string SourceId { get; }
    }
}
