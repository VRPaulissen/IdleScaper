using Items.Runtime;
using ResourceAreas.Definitions;

namespace ResourceAreas.Runtime
{
    /// <summary>
    /// Read model for one possible resolved drop from a resource.
    /// </summary>
    public sealed class ResolvedResourceDrop
    {
        /// <summary>
        /// Creates a resolved resource drop.
        /// </summary>
        public ResolvedResourceDrop(
            ItemId itemId,
            string displayName,
            ResourceDropCategory category,
            float baseChance,
            float bonusChance,
            float finalChance,
            int minAmount,
            int maxAmount,
            string requiredUnlockKey,
            bool isExplicitlyUnlocked,
            string failureReason)
        {
            ItemId = itemId;
            DisplayName = displayName ?? string.Empty;
            Category = category;
            BaseChance = ClampChance(baseChance);
            BonusChance = bonusChance;
            FinalChance = ClampChance(finalChance);
            MinAmount = minAmount;
            MaxAmount = maxAmount;
            RequiredUnlockKey = requiredUnlockKey ?? string.Empty;
            HasExplicitUnlockRequirement = !string.IsNullOrWhiteSpace(RequiredUnlockKey);
            IsExplicitlyUnlocked = isExplicitlyUnlocked;
            FailureReason = failureReason ?? string.Empty;
            CanRoll = CalculateCanRoll();
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
        /// Base drop chance before bonuses.
        /// </summary>
        public float BaseChance { get; }

        /// <summary>
        /// Bonus chance added by resolved bonuses.
        /// </summary>
        public float BonusChance { get; }

        /// <summary>
        /// Final clamped drop chance.
        /// </summary>
        public float FinalChance { get; }

        /// <summary>
        /// Minimum rolled amount.
        /// </summary>
        public int MinAmount { get; }

        /// <summary>
        /// Maximum rolled amount.
        /// </summary>
        public int MaxAmount { get; }

        /// <summary>
        /// Optional explicit unlock key required by this drop.
        /// </summary>
        public string RequiredUnlockKey { get; }

        /// <summary>
        /// Returns true when this drop has an explicit unlock requirement.
        /// </summary>
        public bool HasExplicitUnlockRequirement { get; }

        /// <summary>
        /// Returns true when the explicit unlock requirement is satisfied.
        /// </summary>
        public bool IsExplicitlyUnlocked { get; }

        /// <summary>
        /// Returns true when this drop is valid and can be rolled.
        /// </summary>
        public bool CanRoll { get; }

        /// <summary>
        /// Concise reason this drop cannot roll.
        /// </summary>
        public string FailureReason { get; }

        private bool CalculateCanRoll()
        {
            if (!ItemId.IsValid && string.IsNullOrWhiteSpace(DisplayName))
                return false;

            if (HasExplicitUnlockRequirement && !IsExplicitlyUnlocked)
                return false;

            if (FinalChance <= 0f)
                return false;

            if (MinAmount <= 0 || MaxAmount < MinAmount)
                return false;

            return true;
        }

        private static float ClampChance(float chance)
        {
            if (chance < 0f)
                return 0f;

            if (chance > 1f)
                return 1f;

            return chance;
        }
    }
}
