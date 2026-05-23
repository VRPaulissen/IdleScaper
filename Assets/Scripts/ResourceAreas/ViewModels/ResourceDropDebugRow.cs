using Items.Runtime;
using ResourceAreas.Definitions;

namespace ResourceAreas.ViewModels
{
    /// <summary>
    /// Read model for one resolved possible resource drop.
    /// </summary>
    public sealed class ResourceDropDebugRow
    {
        /// <summary>
        /// Creates a resolved resource drop debug row.
        /// </summary>
        public ResourceDropDebugRow(
            ItemId itemId,
            string displayName,
            ResourceDropCategory category,
            float baseChance,
            float bonusChance,
            float finalChance,
            string formattedBaseChance,
            string formattedBonusChance,
            string formattedFinalChance,
            int minAmount,
            int maxAmount,
            string amountText,
            bool hasExplicitUnlockRequirement,
            bool isExplicitlyUnlocked,
            bool canRoll,
            string failureReason)
        {
            ItemId = itemId;
            DisplayName = displayName ?? string.Empty;
            Category = category;
            BaseChance = baseChance;
            BonusChance = bonusChance;
            FinalChance = finalChance;
            FormattedBaseChance = formattedBaseChance ?? string.Empty;
            FormattedBonusChance = formattedBonusChance ?? string.Empty;
            FormattedFinalChance = formattedFinalChance ?? string.Empty;
            MinAmount = minAmount;
            MaxAmount = maxAmount;
            AmountText = amountText ?? string.Empty;
            HasExplicitUnlockRequirement = hasExplicitUnlockRequirement;
            IsExplicitlyUnlocked = isExplicitlyUnlocked;
            CanRoll = canRoll;
            FailureReason = failureReason ?? string.Empty;
        }

        /// <summary>
        /// Item id produced by this drop.
        /// </summary>
        public ItemId ItemId { get; }

        /// <summary>
        /// Display name for this drop.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Drop category.
        /// </summary>
        public ResourceDropCategory Category { get; }

        /// <summary>
        /// Base chance before bonuses.
        /// </summary>
        public float BaseChance { get; }

        /// <summary>
        /// Bonus chance from resolved bonuses.
        /// </summary>
        public float BonusChance { get; }

        /// <summary>
        /// Final resolved chance.
        /// </summary>
        public float FinalChance { get; }

        /// <summary>
        /// Formatted base chance.
        /// </summary>
        public string FormattedBaseChance { get; }

        /// <summary>
        /// Formatted bonus chance.
        /// </summary>
        public string FormattedBonusChance { get; }

        /// <summary>
        /// Formatted final chance.
        /// </summary>
        public string FormattedFinalChance { get; }

        /// <summary>
        /// Minimum rolled amount.
        /// </summary>
        public int MinAmount { get; }

        /// <summary>
        /// Maximum rolled amount.
        /// </summary>
        public int MaxAmount { get; }

        /// <summary>
        /// Human-readable amount range.
        /// </summary>
        public string AmountText { get; }

        /// <summary>
        /// Returns true when the drop has an explicit unlock requirement.
        /// </summary>
        public bool HasExplicitUnlockRequirement { get; }

        /// <summary>
        /// Returns true when the explicit unlock requirement is satisfied.
        /// </summary>
        public bool IsExplicitlyUnlocked { get; }

        /// <summary>
        /// Returns true when this drop can currently roll.
        /// </summary>
        public bool CanRoll { get; }

        /// <summary>
        /// Concise reason the drop cannot roll.
        /// </summary>
        public string FailureReason { get; }
    }
}
