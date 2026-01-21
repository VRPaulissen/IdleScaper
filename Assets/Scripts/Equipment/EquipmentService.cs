using System;
using Inventory;
using Items.Definitions;
using Items.Runtime;
using Items.Runtime.Modules;

namespace Equipment
{
    /// <summary>
    /// Equipment operations that validate slot compatibility via <see cref="EquipmentModule"/>
    /// and optionally move items to/from an inventory.
    /// </summary>
    public sealed class EquipmentService : IEquipmentService
    {
        private readonly ItemDatabase itemDatabase;
        private readonly EquipmentState state;

        /// <summary>
        /// Raised when equipment changes.
        /// </summary>
        public event Action EquipmentChanged;

        /// <summary>
        /// Creates an equipment service.
        /// </summary>
        public EquipmentService(ItemDatabase itemDatabase, EquipmentState state)
        {
            this.itemDatabase = itemDatabase != null ? itemDatabase : throw new ArgumentNullException(nameof(itemDatabase));
            this.state = state ?? throw new ArgumentNullException(nameof(state));

            this.state.EnsureAllSlots();
        }

        /// <summary>
        /// Current equipment state (serializable).
        /// </summary>
        public EquipmentState State => state;

        /// <inheritdoc />
        public ItemId GetEquipped(EquipmentSlotId slotId)
        {
            var index = state.IndexOf(slotId);
            if (index < 0)
                return default;

            return state.Slots[index].ItemId;
        }

        /// <inheritdoc />
        public EquipmentResult TryEquipFromInventory(IInventoryService inventory, int inventorySlotIndex)
        {
            if (inventory == null)
                return EquipmentResult.Failure(EquipmentResultCode.InventoryNull, "Inventory is null.", default, default);

            var invSlot = inventory.GetSlot(inventorySlotIndex);
            if (!invSlot.HasItem)
                return EquipmentResult.Failure(EquipmentResultCode.InventorySlotEmpty, "Inventory slot is empty.", default, default);

            var itemId = invSlot.ItemId;

            if (!itemDatabase.TryGet(itemId, out var def) || def == null)
                return EquipmentResult.Failure(EquipmentResultCode.ItemNotFoundInDatabase, "Item not found in ItemDatabase.", default, itemId);

            if (!def.TryGetModule<EquipmentModule>(out var equipModule) || equipModule == null)
                return EquipmentResult.Failure(EquipmentResultCode.ItemNotEquipable, "Item has no EquipmentModule.", default, itemId);

            var targetSlot = equipModule.Slot;
            var previous = GetEquipped(targetSlot);

            if (previous.IsValid)
            {
                var preflight = inventory.TryAdd(previous, 1);
                if (!preflight.IsSuccess)
                {
                    return EquipmentResult.Failure(
                        EquipmentResultCode.InventoryNoSpaceForSwap,
                        $"Cannot swap: {preflight.Code}.",
                        targetSlot,
                        itemId);
                }

                // Revert the preflight add; we only wanted to check capacity.
                // If you prefer, add an inventory.CanAdd(...) method to avoid this.
                inventory.TryRemove(previous, 1);
            }

            var removeResult = inventory.TryRemove(itemId, 1);
            if (!removeResult.IsSuccess)
            {
                return EquipmentResult.Failure(
                    EquipmentResultCode.InventorySlotEmpty,
                    $"Cannot remove item from inventory: {removeResult.Code}.",
                    targetSlot,
                    itemId);
            }

            SetEquipped(targetSlot, itemId);

            if (previous.IsValid)
            {
                var addPrev = inventory.TryAdd(previous, 1);
                if (!addPrev.IsSuccess)
                {
                    // Rollback: put new item back into inventory, restore previous equipment.
                    inventory.TryAdd(itemId, 1);
                    SetEquipped(targetSlot, previous);

                    return EquipmentResult.Failure(
                        EquipmentResultCode.InventoryNoSpaceForSwap,
                        $"Swap failed while adding previous item back: {addPrev.Code}.",
                        targetSlot,
                        itemId);
                }
            }

            EquipmentChanged?.Invoke();
            return EquipmentResult.Success(targetSlot, itemId, previous);
        }

        /// <inheritdoc />
        public EquipmentResult TryEquipFromExternal(IInventoryService inventory, ItemDefinition item)
        {
            var itemId = item.Id;

            if (!itemDatabase.TryGet(itemId, out var def) || def == null)
                return EquipmentResult.Failure(EquipmentResultCode.ItemNotFoundInDatabase, "Item not found in ItemDatabase.", default, itemId);

            if (!def.TryGetModule<EquipmentModule>(out var equipModule) || equipModule == null)
                return EquipmentResult.Failure(EquipmentResultCode.ItemNotEquipable, "Item has no EquipmentModule.", default, itemId);

            var targetSlot = equipModule.Slot;
            var previous = GetEquipped(targetSlot);

            if (previous.IsValid)
            {
                var preflight = inventory.TryAdd(previous, 1);
                if (!preflight.IsSuccess)
                {
                    return EquipmentResult.Failure(
                        EquipmentResultCode.InventoryNoSpaceForSwap,
                        $"Cannot swap: {preflight.Code}.",
                        targetSlot,
                        itemId);
                }

                inventory.TryRemove(previous, 1);
            }

            SetEquipped(targetSlot, itemId);

            if (previous.IsValid)
            {
                var addPrev = inventory.TryAdd(previous, 1);
                if (!addPrev.IsSuccess)
                {
                    // Rollback: put new item back into inventory, restore previous equipment.
                    inventory.TryAdd(itemId, 1);
                    SetEquipped(targetSlot, previous);

                    return EquipmentResult.Failure(
                        EquipmentResultCode.InventoryNoSpaceForSwap,
                        $"Swap failed while adding previous item back: {addPrev.Code}.",
                        targetSlot,
                        itemId);
                }
            }

            EquipmentChanged?.Invoke();
            return EquipmentResult.Success(targetSlot, itemId, previous);
        }
        
        /// <inheritdoc />
        public EquipmentResult TryUnequipToInventory(IInventoryService inventory, EquipmentSlotId slotId)
        {
            if (inventory == null)
                return EquipmentResult.Failure(EquipmentResultCode.InventoryNull, "Inventory is null.", slotId, default);

            var equipped = GetEquipped(slotId);
            if (!equipped.IsValid)
                return EquipmentResult.Failure(EquipmentResultCode.SlotEmpty, "Equipment slot is empty.", slotId, default);

            var addResult = inventory.TryAdd(equipped, 1);
            if (!addResult.IsSuccess)
            {
                return EquipmentResult.Failure(
                    EquipmentResultCode.InventoryNoSpaceToUnequip,
                    $"Cannot unequip: {addResult.Code}.",
                    slotId,
                    equipped);
            }

            SetEquipped(slotId, default);
            EquipmentChanged?.Invoke();
            return EquipmentResult.Success(slotId, default, equipped);
        }

        private void SetEquipped(EquipmentSlotId slotId, ItemId itemId)
        {
            var index = state.IndexOf(slotId);
            if (index < 0)
                return;

            var entry = state.Slots[index];

            if (itemId.IsValid)
                entry.Set(itemId);
            else
                entry.Clear();

            state.Slots[index] = entry;
        }
    }
}
