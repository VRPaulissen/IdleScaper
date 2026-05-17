using System;
using Inventory;
using Items.Runtime;
using Items.Runtime.Modules;
using Tools.State;

namespace Tools.Runtime
{
    /// <summary>
    /// Runtime service for installing and removing inventory-backed parts inside permanent player tools.
    /// </summary>
    public sealed class PermanentToolPartService : IPermanentToolPartService
    {
        private readonly ToolCollectionState tools;
        private readonly ItemDatabase itemDatabase;
        private readonly IInventoryService inventory;

        /// <inheritdoc />
        public event Action<ToolPartInstalledEventData> ToolPartInstalled;

        /// <inheritdoc />
        public event Action<ToolPartRemovedEventData> ToolPartRemoved;

        /// <inheritdoc />
        public event Action<ToolLoadoutChangedEventData> ToolLoadoutChanged;

        /// <summary>
        /// Creates a permanent tool part service.
        /// </summary>
        public PermanentToolPartService(
            ToolCollectionState tools,
            ItemDatabase itemDatabase,
            IInventoryService inventory)
        {
            this.tools = tools ?? throw new ArgumentNullException(nameof(tools));
            this.itemDatabase = itemDatabase ?? throw new ArgumentNullException(nameof(itemDatabase));
            this.inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
        }

        /// <inheritdoc />
        public ToolPartOperationResult TryInstallPart(ToolId toolId, ToolPartSlotId slotId, ItemId partItemId)
        {
            var stateResult = TryGetSlot(toolId, slotId, out var slot);
            if (!stateResult.IsSuccess)
                return stateResult;

            var validation = ValidateInstallTarget(toolId, slotId, partItemId);
            if (!validation.IsSuccess)
                return validation;

            var previousPartItemId = slot.InstalledPartItemId;
            if (previousPartItemId == partItemId)
            {
                return ToolPartOperationResult.Failure(
                    ToolPartOperationResultCode.PartAlreadyInstalled,
                    "The requested part is already installed in this slot.",
                    toolId,
                    slotId,
                    partItemId,
                    previousPartItemId);
            }

            if (!inventory.CanRemove(partItemId, 1))
            {
                return ToolPartOperationResult.Failure(
                    ToolPartOperationResultCode.InsufficientInventory,
                    "Inventory does not contain the requested tool part.",
                    toolId,
                    slotId,
                    partItemId,
                    previousPartItemId);
            }

            var removeNewResult = inventory.TryRemove(partItemId, 1);
            if (!removeNewResult.IsSuccess)
            {
                return ToolPartOperationResult.Failure(
                    ToolPartOperationResultCode.InventoryRemoveFailed,
                    $"Failed to remove requested tool part from inventory: {removeNewResult.Code}.",
                    toolId,
                    slotId,
                    partItemId,
                    previousPartItemId);
            }

            if (previousPartItemId.IsValid)
            {
                var returnOldResult = inventory.TryAdd(previousPartItemId, 1);
                if (!returnOldResult.IsSuccess)
                {
                    RollbackNewPartToInventory(partItemId);
                    return ToolPartOperationResult.Failure(
                        ToolPartOperationResultCode.InventoryReturnFailed,
                        $"Failed to return previous tool part to inventory: {returnOldResult.Code}.",
                        toolId,
                        slotId,
                        partItemId,
                        previousPartItemId);
                }
            }

            slot.SetInstalledPart(partItemId, 0);
            RaiseInstalled(toolId, slotId, partItemId, previousPartItemId);

            return ToolPartOperationResult.Success(toolId, slotId, partItemId, previousPartItemId);
        }

        /// <inheritdoc />
        public ToolPartOperationResult TryRemovePart(ToolId toolId, ToolPartSlotId slotId)
        {
            var stateResult = TryGetSlot(toolId, slotId, out var slot);
            if (!stateResult.IsSuccess)
                return stateResult;

            if (!slot.HasInstalledPart)
            {
                return ToolPartOperationResult.Failure(
                    ToolPartOperationResultCode.SlotEmpty,
                    "The requested tool slot has no installed part.",
                    toolId,
                    slotId);
            }

            var removedPartItemId = slot.InstalledPartItemId;
            var removedPartLevel = slot.PartLevel;
            var returnResult = inventory.TryAdd(removedPartItemId, 1);
            if (!returnResult.IsSuccess)
            {
                return ToolPartOperationResult.Failure(
                    ToolPartOperationResultCode.InventoryReturnFailed,
                    $"Failed to return installed tool part to inventory: {returnResult.Code}.",
                    toolId,
                    slotId,
                    removedPartItemId);
            }

            slot.ClearInstalledPart();
            RaiseRemoved(toolId, slotId, removedPartItemId, removedPartLevel);

            return ToolPartOperationResult.Success(toolId, slotId, removedPartItemId);
        }

        private ToolPartOperationResult TryGetSlot(
            ToolId toolId,
            ToolPartSlotId slotId,
            out ToolPartSlotState slot)
        {
            slot = null;

            if (!toolId.IsValid)
            {
                return ToolPartOperationResult.Failure(
                    ToolPartOperationResultCode.InvalidToolId,
                    "Invalid tool id.",
                    toolId,
                    slotId);
            }

            if (!slotId.IsValid)
            {
                return ToolPartOperationResult.Failure(
                    ToolPartOperationResultCode.InvalidSlotId,
                    "Invalid tool part slot id.",
                    toolId,
                    slotId);
            }

            var tool = tools.GetTool(toolId);
            if (tool == null)
            {
                return ToolPartOperationResult.Failure(
                    ToolPartOperationResultCode.ToolNotFound,
                    "Permanent tool state was not found.",
                    toolId,
                    slotId);
            }

            var preset = tool.GetPreset(tool.ActivePresetIndex);
            if (preset == null)
            {
                return ToolPartOperationResult.Failure(
                    ToolPartOperationResultCode.ActivePresetNotFound,
                    "Active permanent tool preset was not found.",
                    toolId,
                    slotId);
            }

            slot = preset.GetSlot(slotId);
            if (slot != null)
                return ToolPartOperationResult.Success(toolId, slotId, default);

            return ToolPartOperationResult.Failure(
                ToolPartOperationResultCode.SlotNotFound,
                "Permanent tool part slot was not found.",
                toolId,
                slotId);
        }

        private ToolPartOperationResult ValidateInstallTarget(
            ToolId toolId,
            ToolPartSlotId slotId,
            ItemId partItemId)
        {
            if (!partItemId.IsValid)
            {
                return ToolPartOperationResult.Failure(
                    ToolPartOperationResultCode.InvalidItemId,
                    "Invalid tool part item id.",
                    toolId,
                    slotId,
                    partItemId);
            }

            if (!itemDatabase.TryGet(partItemId, out var itemDefinition) || itemDefinition == null)
            {
                return ToolPartOperationResult.Failure(
                    ToolPartOperationResultCode.ItemNotFoundInDatabase,
                    "Tool part item was not found in the item database.",
                    toolId,
                    slotId,
                    partItemId);
            }

            if (!itemDefinition.TryGetModule<ToolPartModule>(out var toolPartModule) || toolPartModule == null)
            {
                return ToolPartOperationResult.Failure(
                    ToolPartOperationResultCode.ItemNotToolPart,
                    "Item does not have a ToolPartModule.",
                    toolId,
                    slotId,
                    partItemId);
            }

            if (toolPartModule.CompatibleToolId != toolId)
            {
                return ToolPartOperationResult.Failure(
                    ToolPartOperationResultCode.IncompatibleTool,
                    "Tool part is not compatible with this permanent tool.",
                    toolId,
                    slotId,
                    partItemId);
            }

            if (toolPartModule.CompatibleSlotId == slotId)
                return ToolPartOperationResult.Success(toolId, slotId, partItemId);

            return ToolPartOperationResult.Failure(
                ToolPartOperationResultCode.IncompatibleSlot,
                "Tool part is not compatible with this internal slot.",
                toolId,
                slotId,
                partItemId);
        }

        private void RollbackNewPartToInventory(ItemId partItemId)
        {
            if (!partItemId.IsValid)
                return;

            inventory.TryAdd(partItemId, 1);
        }

        private void RaiseInstalled(
            ToolId toolId,
            ToolPartSlotId slotId,
            ItemId partItemId,
            ItemId previousPartItemId)
        {
            ToolPartInstalled?.Invoke(new ToolPartInstalledEventData(
                toolId,
                slotId,
                partItemId,
                previousPartItemId));

            RaiseLoadoutChanged(toolId, slotId);
        }

        private void RaiseRemoved(
            ToolId toolId,
            ToolPartSlotId slotId,
            ItemId removedPartItemId,
            int removedPartLevel)
        {
            ToolPartRemoved?.Invoke(new ToolPartRemovedEventData(
                toolId,
                slotId,
                removedPartItemId,
                removedPartLevel));

            RaiseLoadoutChanged(toolId, slotId);
        }

        private void RaiseLoadoutChanged(ToolId toolId, ToolPartSlotId slotId)
        {
            ToolLoadoutChanged?.Invoke(new ToolLoadoutChangedEventData(toolId, slotId));
        }
    }
}
