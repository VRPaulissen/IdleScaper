using System.Collections.Generic;
using Items.Runtime;
using Operations;
using Tools.Definitions;

namespace Tools.Runtime
{
    /// <summary>
    /// Result for a permanent tool part upgrade operation.
    /// </summary>
    public sealed class ToolUpgradeResult : OperationResult
    {
        /// <summary>
        /// Machine-readable failure or success reason.
        /// </summary>
        public ToolUpgradeFailureReason Reason { get; }

        /// <summary>
        /// Permanent tool affected by the upgrade.
        /// </summary>
        public ToolId ToolId { get; }

        /// <summary>
        /// Internal part slot affected by the upgrade.
        /// </summary>
        public ToolPartSlotId SlotId { get; }

        /// <summary>
        /// Installed part item affected by the upgrade.
        /// </summary>
        public ItemId PartItemId { get; }

        /// <summary>
        /// Level before the upgrade attempt.
        /// </summary>
        public int FromLevel { get; }

        /// <summary>
        /// Level after the upgrade attempt when successful.
        /// </summary>
        public int ToLevel { get; }

        /// <summary>
        /// Recipe used for the upgrade when available.
        /// </summary>
        public ToolUpgradeRecipeDefinition Recipe { get; }

        /// <summary>
        /// Missing inventory costs when the operation failed because of cost requirements.
        /// </summary>
        public IReadOnlyList<MissingToolUpgradeCost> MissingCosts => missingCosts;

        private readonly List<MissingToolUpgradeCost> missingCosts;

        private ToolUpgradeResult(
            bool isSuccess,
            ToolUpgradeFailureReason reason,
            string message,
            ToolId toolId,
            ToolPartSlotId slotId,
            ItemId partItemId,
            int fromLevel,
            int toLevel,
            ToolUpgradeRecipeDefinition recipe,
            List<MissingToolUpgradeCost> missingCosts)
            : base(isSuccess, message)
        {
            Reason = reason;
            ToolId = toolId;
            SlotId = slotId;
            PartItemId = partItemId;
            FromLevel = fromLevel;
            ToLevel = toLevel;
            Recipe = recipe;
            this.missingCosts = missingCosts ?? new List<MissingToolUpgradeCost>();
        }

        /// <summary>
        /// Creates a successful upgrade result.
        /// </summary>
        public static ToolUpgradeResult Success(
            ToolId toolId,
            ToolPartSlotId slotId,
            ItemId partItemId,
            int fromLevel,
            int toLevel,
            ToolUpgradeRecipeDefinition recipe)
        {
            return new ToolUpgradeResult(
                true,
                ToolUpgradeFailureReason.Success,
                string.Empty,
                toolId,
                slotId,
                partItemId,
                fromLevel,
                toLevel,
                recipe,
                null);
        }

        /// <summary>
        /// Creates a failed upgrade result.
        /// </summary>
        public static ToolUpgradeResult Failure(
            ToolUpgradeFailureReason reason,
            string message,
            ToolId toolId,
            ToolPartSlotId slotId,
            ItemId partItemId = default,
            int fromLevel = 0,
            int toLevel = 0,
            ToolUpgradeRecipeDefinition recipe = null,
            List<MissingToolUpgradeCost> missingCosts = null)
        {
            return new ToolUpgradeResult(
                false,
                reason,
                message,
                toolId,
                slotId,
                partItemId,
                fromLevel,
                toLevel,
                recipe,
                missingCosts);
        }
    }
}
