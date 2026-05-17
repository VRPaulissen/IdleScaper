using System;
using Items.Runtime;

namespace Tools.Runtime
{
    /// <summary>
    /// Service for installing and removing inventory-backed parts inside permanent player tools.
    /// </summary>
    public interface IPermanentToolPartService
    {
        /// <summary>
        /// Raised after a part is installed into a permanent tool slot.
        /// </summary>
        event Action<ToolPartInstalledEventData> ToolPartInstalled;

        /// <summary>
        /// Raised after a part is removed from a permanent tool slot.
        /// </summary>
        event Action<ToolPartRemovedEventData> ToolPartRemoved;

        /// <summary>
        /// Raised after a permanent tool loadout changes.
        /// </summary>
        event Action<ToolLoadoutChangedEventData> ToolLoadoutChanged;

        /// <summary>
        /// Attempts to install an inventory item as a part in the active preset of a permanent tool.
        /// </summary>
        ToolPartOperationResult TryInstallPart(ToolId toolId, ToolPartSlotId slotId, ItemId partItemId);

        /// <summary>
        /// Attempts to remove an installed part from the active preset of a permanent tool.
        /// </summary>
        ToolPartOperationResult TryRemovePart(ToolId toolId, ToolPartSlotId slotId);
    }
}
