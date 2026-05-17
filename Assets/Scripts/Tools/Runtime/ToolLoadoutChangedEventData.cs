namespace Tools.Runtime
{
    /// <summary>
    /// Event data for any permanent tool loadout change.
    /// </summary>
    public sealed class ToolLoadoutChangedEventData
    {
        /// <summary>
        /// Permanent tool whose loadout changed.
        /// </summary>
        public ToolId ToolId { get; }

        /// <summary>
        /// Internal slot affected by the change.
        /// </summary>
        public ToolPartSlotId SlotId { get; }

        /// <summary>
        /// Creates loadout change event data.
        /// </summary>
        public ToolLoadoutChangedEventData(ToolId toolId, ToolPartSlotId slotId)
        {
            ToolId = toolId;
            SlotId = slotId;
        }
    }
}
