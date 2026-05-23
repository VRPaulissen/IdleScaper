using ResourceAreas.Runtime;

namespace ResourceAreas.ViewModels
{
    /// <summary>
    /// Read model for one resolved total by bonus type.
    /// </summary>
    public sealed class ResourceBonusDebugTotalRow
    {
        /// <summary>
        /// Creates a resolved resource bonus total row.
        /// </summary>
        public ResourceBonusDebugTotalRow(ResourceBonusType bonusType, float totalValue, string formattedValue)
        {
            BonusType = bonusType;
            TotalValue = totalValue;
            FormattedValue = formattedValue ?? string.Empty;
        }

        /// <summary>
        /// Bonus type represented by this total.
        /// </summary>
        public ResourceBonusType BonusType { get; }

        /// <summary>
        /// Raw total value.
        /// </summary>
        public float TotalValue { get; }

        /// <summary>
        /// Human-readable total value.
        /// </summary>
        public string FormattedValue { get; }
    }
}
