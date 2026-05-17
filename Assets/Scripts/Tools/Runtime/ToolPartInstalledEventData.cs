using Items.Runtime;

namespace Tools.Runtime
{
    /// <summary>
    /// Event data for a permanent tool part installation.
    /// </summary>
    public sealed class ToolPartInstalledEventData
    {
        /// <summary>
        /// Permanent tool that received the part.
        /// </summary>
        public ToolId ToolId { get; }

        /// <summary>
        /// Internal slot that received the part.
        /// </summary>
        public ToolPartSlotId SlotId { get; }

        /// <summary>
        /// Installed part item id.
        /// </summary>
        public ItemId InstalledPartItemId { get; }

        /// <summary>
        /// Previously installed part item id, or invalid if the slot was empty.
        /// </summary>
        public ItemId PreviousPartItemId { get; }

        /// <summary>
        /// Creates installation event data.
        /// </summary>
        public ToolPartInstalledEventData(
            ToolId toolId,
            ToolPartSlotId slotId,
            ItemId installedPartItemId,
            ItemId previousPartItemId)
        {
            ToolId = toolId;
            SlotId = slotId;
            InstalledPartItemId = installedPartItemId;
            PreviousPartItemId = previousPartItemId;
        }
    }
}
