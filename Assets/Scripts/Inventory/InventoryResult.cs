using Items.Runtime;
using Operations;

namespace Inventory
{
    /// <summary>
    /// Result for inventory operations (add/remove/move).
    /// </summary>
    public sealed class InventoryResult : OperationResult
    {
        /// <summary>
        /// Machine-readable reason code for the result.
        /// </summary>
        public InventoryResultCode Code { get; }

        /// <summary>
        /// Item associated with the operation.
        /// </summary>
        public ItemId ItemId { get; }

        /// <summary>
        /// Requested quantity for the operation.
        /// </summary>
        public int RequestedQuantity { get; }

        /// <summary>
        /// Quantity that could not be processed (e.g., not added / not removed).
        /// Zero means fully processed.
        /// </summary>
        public int UnprocessedQuantity { get; }

        /// <summary>
        /// From slot index (for moves), otherwise -1.
        /// </summary>
        public int FromSlotIndex { get; }

        /// <summary>
        /// To slot index (for moves), otherwise -1.
        /// </summary>
        public int ToSlotIndex { get; }

        private InventoryResult(
            bool isSuccess,
            InventoryResultCode code,
            string message,
            ItemId itemId,
            int requestedQuantity,
            int unprocessedQuantity,
            int fromSlotIndex,
            int toSlotIndex)
            : base(isSuccess, message)
        {
            Code = code;
            ItemId = itemId;
            RequestedQuantity = requestedQuantity;
            UnprocessedQuantity = unprocessedQuantity;
            FromSlotIndex = fromSlotIndex;
            ToSlotIndex = toSlotIndex;
        }

        /// <summary>
        /// Creates a success result for add/remove.
        /// </summary>
        public static InventoryResult Success(ItemId itemId, int requestedQuantity)
        {
            return new InventoryResult(
                isSuccess: true,
                code: InventoryResultCode.Success,
                message: string.Empty,
                itemId: itemId,
                requestedQuantity: requestedQuantity,
                unprocessedQuantity: 0,
                fromSlotIndex: -1,
                toSlotIndex: -1);
        }

        /// <summary>
        /// Creates a success result for move operations.
        /// </summary>
        public static InventoryResult SuccessMove(int fromSlotIndex, int toSlotIndex)
        {
            return new InventoryResult(
                isSuccess: true,
                code: InventoryResultCode.Success,
                message: string.Empty,
                itemId: default,
                requestedQuantity: 0,
                unprocessedQuantity: 0,
                fromSlotIndex: fromSlotIndex,
                toSlotIndex: toSlotIndex);
        }

        /// <summary>
        /// Creates a failure result for add/remove.
        /// </summary>
        public static InventoryResult Failure(
            InventoryResultCode code,
            string message,
            ItemId itemId,
            int requestedQuantity,
            int unprocessedQuantity)
        {
            return new InventoryResult(
                isSuccess: false,
                code: code,
                message: message,
                itemId: itemId,
                requestedQuantity: requestedQuantity,
                unprocessedQuantity: unprocessedQuantity,
                fromSlotIndex: -1,
                toSlotIndex: -1);
        }

        /// <summary>
        /// Creates a failure result for move operations.
        /// </summary>
        public static InventoryResult FailureMove(
            InventoryResultCode code,
            string message,
            int fromSlotIndex,
            int toSlotIndex)
        {
            return new InventoryResult(
                isSuccess: false,
                code: code,
                message: message,
                itemId: default,
                requestedQuantity: 0,
                unprocessedQuantity: 0,
                fromSlotIndex: fromSlotIndex,
                toSlotIndex: toSlotIndex);
        }
    }

    /// <summary>
    /// Inventory result codes for consistent branching and UI messaging.
    /// </summary>
    public enum InventoryResultCode
    {
        Success = 0,

        InvalidItemId = 1,
        InvalidQuantity = 2,
        ItemNotFoundInDatabase = 3,

        InventoryFull = 4,
        InsufficientItems = 5,

        InvalidSlotIndex = 6,
        SameSlot = 7,
        SourceEmpty = 8,
        NotStackable = 9,
        TargetStackFull = 10
    }
}
