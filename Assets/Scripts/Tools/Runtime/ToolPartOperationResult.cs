using Items.Runtime;
using Operations;

namespace Tools.Runtime
{
    /// <summary>
    /// Result for permanent tool part install and remove operations.
    /// </summary>
    public sealed class ToolPartOperationResult : OperationResult
    {
        /// <summary>
        /// Machine-readable reason code.
        /// </summary>
        public ToolPartOperationResultCode Code { get; }

        /// <summary>
        /// Permanent tool affected by the operation.
        /// </summary>
        public ToolId ToolId { get; }

        /// <summary>
        /// Internal part slot affected by the operation.
        /// </summary>
        public ToolPartSlotId SlotId { get; }

        /// <summary>
        /// Item installed, removed, or requested by the operation.
        /// </summary>
        public ItemId ItemId { get; }

        /// <summary>
        /// Previous installed part item, if a swap occurred.
        /// </summary>
        public ItemId PreviousItemId { get; }

        private ToolPartOperationResult(
            bool isSuccess,
            ToolPartOperationResultCode code,
            string message,
            ToolId toolId,
            ToolPartSlotId slotId,
            ItemId itemId,
            ItemId previousItemId)
            : base(isSuccess, message)
        {
            Code = code;
            ToolId = toolId;
            SlotId = slotId;
            ItemId = itemId;
            PreviousItemId = previousItemId;
        }

        /// <summary>
        /// Creates a successful operation result.
        /// </summary>
        public static ToolPartOperationResult Success(
            ToolId toolId,
            ToolPartSlotId slotId,
            ItemId itemId,
            ItemId previousItemId = default)
        {
            return new ToolPartOperationResult(
                true,
                ToolPartOperationResultCode.Success,
                string.Empty,
                toolId,
                slotId,
                itemId,
                previousItemId);
        }

        /// <summary>
        /// Creates a failed operation result.
        /// </summary>
        public static ToolPartOperationResult Failure(
            ToolPartOperationResultCode code,
            string message,
            ToolId toolId,
            ToolPartSlotId slotId,
            ItemId itemId = default,
            ItemId previousItemId = default)
        {
            return new ToolPartOperationResult(
                false,
                code,
                message,
                toolId,
                slotId,
                itemId,
                previousItemId);
        }
    }
}
