using Items.Runtime;

namespace Tools.Runtime
{
    /// <summary>
    /// Event data for a permanent tool part removal.
    /// </summary>
    public sealed class ToolPartRemovedEventData
    {
        /// <summary>
        /// Permanent tool that lost the part.
        /// </summary>
        public ToolId ToolId { get; }

        /// <summary>
        /// Internal slot that lost the part.
        /// </summary>
        public ToolPartSlotId SlotId { get; }

        /// <summary>
        /// Removed part item id.
        /// </summary>
        public ItemId RemovedPartItemId { get; }

        /// <summary>
        /// Level of the removed part before it was cleared.
        /// </summary>
        public int RemovedPartLevel { get; }

        /// <summary>
        /// Creates removal event data.
        /// </summary>
        public ToolPartRemovedEventData(
            ToolId toolId,
            ToolPartSlotId slotId,
            ItemId removedPartItemId,
            int removedPartLevel)
        {
            ToolId = toolId;
            SlotId = slotId;
            RemovedPartItemId = removedPartItemId;
            RemovedPartLevel = removedPartLevel;
        }
    }
}
