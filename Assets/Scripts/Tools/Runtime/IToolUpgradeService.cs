using System;

namespace Tools.Runtime
{
    /// <summary>
    /// Service for validating and applying permanent tool part upgrades.
    /// </summary>
    public interface IToolUpgradeService
    {
        /// <summary>
        /// Raised after an installed permanent tool part is upgraded.
        /// </summary>
        event Action<ToolPartUpgradedEventData> ToolPartUpgraded;

        /// <summary>
        /// Attempts to upgrade the installed part in the active preset of a permanent tool.
        /// </summary>
        ToolUpgradeResult TryUpgradeInstalledPart(ToolId toolId, ToolPartSlotId slotId);
    }
}
