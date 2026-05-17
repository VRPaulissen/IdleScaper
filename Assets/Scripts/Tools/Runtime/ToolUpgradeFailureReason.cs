namespace Tools.Runtime
{
    /// <summary>
    /// Failure and success reasons for permanent tool part upgrades.
    /// </summary>
    public enum ToolUpgradeFailureReason
    {
        Success = 0,

        InvalidTool = 1,
        InvalidPreset = 2,
        InvalidSlot = 3,
        EmptySlot = 4,

        ItemDefinitionNotFound = 5,
        NotToolPart = 6,
        PartNotCompatibleWithTool = 7,
        PartNotCompatibleWithSlot = 8,

        MaxLevelReached = 9,
        RecipeNotFound = 10,
        InvalidRecipe = 11,
        MissingCost = 12,
        InventoryConsumeFailed = 13
    }
}
