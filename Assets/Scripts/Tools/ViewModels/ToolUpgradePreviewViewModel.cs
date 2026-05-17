using System.Collections.Generic;
using Tools.Runtime;

namespace Tools.ViewModels
{
    /// <summary>
    /// UI-facing view model for the next upgrade available to an installed tool part.
    /// </summary>
    public sealed class ToolUpgradePreviewViewModel
    {
        /// <summary>
        /// Level before the previewed upgrade.
        /// </summary>
        public int FromLevel { get; }

        /// <summary>
        /// Level after the previewed upgrade.
        /// </summary>
        public int ToLevel { get; }

        /// <summary>
        /// Costs required by the previewed upgrade.
        /// </summary>
        public IReadOnlyList<ToolUpgradeCostViewModel> Costs => costs;

        /// <summary>
        /// Returns true when a valid next upgrade recipe is available.
        /// </summary>
        public bool HasRecipe { get; }

        /// <summary>
        /// Returns true when every cost is fulfilled.
        /// </summary>
        public bool AreCostsFulfilled { get; }

        /// <summary>
        /// Returns true when the upgrade action should be enabled.
        /// </summary>
        public bool CanUpgrade { get; }

        /// <summary>
        /// Machine-readable reason when the upgrade is unavailable.
        /// </summary>
        public ToolUpgradeFailureReason FailureReason { get; }

        /// <summary>
        /// Human-readable reason when the upgrade is unavailable.
        /// </summary>
        public string FailureText { get; }

        private readonly List<ToolUpgradeCostViewModel> costs;

        /// <summary>
        /// Creates an upgrade preview view model.
        /// </summary>
        public ToolUpgradePreviewViewModel(
            int fromLevel,
            int toLevel,
            List<ToolUpgradeCostViewModel> costs,
            bool hasRecipe,
            ToolUpgradeFailureReason failureReason,
            string failureText)
        {
            FromLevel = fromLevel < 0 ? 0 : fromLevel;
            ToLevel = toLevel < 0 ? 0 : toLevel;
            this.costs = costs ?? new List<ToolUpgradeCostViewModel>();
            HasRecipe = hasRecipe;
            AreCostsFulfilled = AreAllCostsFulfilled(this.costs);
            CanUpgrade = HasRecipe && AreCostsFulfilled && failureReason == ToolUpgradeFailureReason.Success;
            FailureReason = failureReason;
            FailureText = failureText ?? string.Empty;
        }

        private static bool AreAllCostsFulfilled(List<ToolUpgradeCostViewModel> costs)
        {
            for (var i = 0; i < costs.Count; i++)
            {
                if (!costs[i].IsFulfilled)
                    return false;
            }

            return true;
        }
    }
}
