using Items.Runtime;
using Items.Runtime.Modules;
using Operations;

namespace Equipment
{
    /// <summary>
    /// Result for equipment operations (equip/unequip).
    /// </summary>
    public sealed class EquipmentResult : OperationResult
    {
        /// <summary>
        /// Machine-readable reason code.
        /// </summary>
        public EquipmentResultCode Code { get; }

        /// <summary>
        /// Slot affected by the operation.
        /// </summary>
        public EquipmentSlotId SlotId { get; }

        /// <summary>
        /// Item being equipped/unequipped (if relevant).
        /// </summary>
        public ItemId ItemId { get; }

        /// <summary>
        /// Previous item in the slot (if a swap happened).
        /// </summary>
        public ItemId PreviousItemId { get; }

        private EquipmentResult(
            bool isSuccess,
            EquipmentResultCode code,
            string message,
            EquipmentSlotId slotId,
            ItemId itemId,
            ItemId previousItemId)
            : base(isSuccess, message)
        {
            Code = code;
            SlotId = slotId;
            ItemId = itemId;
            PreviousItemId = previousItemId;
        }

        /// <summary>
        /// Creates a success result.
        /// </summary>
        public static EquipmentResult Success(EquipmentSlotId slotId, ItemId itemId, ItemId previousItemId)
        {
            return new EquipmentResult(
                isSuccess: true,
                code: EquipmentResultCode.Success,
                message: string.Empty,
                slotId: slotId,
                itemId: itemId,
                previousItemId: previousItemId);
        }

        /// <summary>
        /// Creates a failure result.
        /// </summary>
        public static EquipmentResult Failure(EquipmentResultCode code, string message, EquipmentSlotId slotId, ItemId itemId)
        {
            return new EquipmentResult(
                isSuccess: false,
                code: code,
                message: message,
                slotId: slotId,
                itemId: itemId,
                previousItemId: default);
        }
    }

    /// <summary>
    /// Equipment result codes for consistent branching and UI messaging.
    /// </summary>
    public enum EquipmentResultCode
    {
        Success = 0,

        InventoryNull = 1,
        InvalidInventorySlotIndex = 2,
        InventorySlotEmpty = 3,

        ItemNotFoundInDatabase = 4,
        ItemNotEquipable = 5,
        SlotMismatch = 6,

        InventoryNoSpaceForSwap = 7,
        InventoryNoSpaceToUnequip = 8,

        SlotEmpty = 9
    }
}
