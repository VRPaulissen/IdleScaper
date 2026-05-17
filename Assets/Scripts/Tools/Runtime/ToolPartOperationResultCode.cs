namespace Tools.Runtime
{
    /// <summary>
    /// Failure and success codes for permanent tool part operations.
    /// </summary>
    public enum ToolPartOperationResultCode
    {
        Success = 0,

        InventoryNull = 1,
        InvalidToolId = 2,
        InvalidSlotId = 3,
        InvalidItemId = 4,

        ToolNotFound = 5,
        ActivePresetNotFound = 6,
        SlotNotFound = 7,
        SlotEmpty = 8,
        PartAlreadyInstalled = 9,

        ItemNotFoundInDatabase = 10,
        ItemNotToolPart = 11,
        IncompatibleTool = 12,
        IncompatibleSlot = 13,
        InsufficientInventory = 14,

        InventoryRemoveFailed = 15,
        InventoryReturnFailed = 16
    }
}
