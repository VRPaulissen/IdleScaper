using System;
using Items.Runtime;
using UnityEngine;

namespace Inventory
{
/// <summary>
    /// Inventory operations for stack-based items using <see cref="ItemDatabase"/> as the catalog.
    /// </summary>
    public sealed class InventoryService : IInventoryService
    {
        private readonly ItemDatabase itemDatabase;
        private readonly InventoryState state;

        /// <summary>
        /// Raised when any inventory slot changes.
        /// </summary>
        public event Action InventoryChanged;

        /// <summary>
        /// Creates an inventory service.
        /// </summary>
        public InventoryService(ItemDatabase itemDatabase, InventoryState state, int slotCount)
        {
            this.itemDatabase = itemDatabase != null ? itemDatabase : throw new ArgumentNullException(nameof(itemDatabase));
            this.state = state ?? throw new ArgumentNullException(nameof(state));

            this.state.EnsureSize(slotCount);
        }

        /// <summary>
        /// Current inventory state (serializable).
        /// </summary>
        public InventoryState State => state;

        /// <summary>
        /// Attempts to add the given item and quantity. Returns false if not all could be added.
        /// </summary>
        public InventoryResult TryAdd(ItemId itemId, int quantity)
        {
            if (!itemId.IsValid)
                return InventoryResult.Failure(InventoryResultCode.InvalidItemId, "Invalid ItemId.", itemId, quantity, quantity);

            if (quantity <= 0)
                return InventoryResult.Failure(InventoryResultCode.InvalidQuantity, "Quantity must be > 0.", itemId, quantity, quantity);

            if (!itemDatabase.TryGet(itemId, out var def) || def == null)
                return InventoryResult.Failure(InventoryResultCode.ItemNotFoundInDatabase, "Item not found in ItemDatabase.", itemId, quantity, quantity);

            var remaining = quantity;

            if (def.Stackable)
                remaining = AddToExistingStacks(itemId, remaining, def.MaxStackSize);

            remaining = AddToEmptySlots(itemId, remaining, def.Stackable ? def.MaxStackSize : 1);

            if (remaining != quantity)
                InventoryChanged?.Invoke();

            if (remaining == 0)
                return InventoryResult.Success(itemId, quantity);

            return InventoryResult.Failure(
                InventoryResultCode.InventoryFull,
                "Not enough space in inventory.",
                itemId,
                quantity,
                remaining);
        }

        /// <summary>
        /// Attempts to remove the given item and quantity. Returns false if insufficient items exist.
        /// </summary>
        public InventoryResult TryRemove(ItemId itemId, int quantity)
        {
            if (!itemId.IsValid)
                return InventoryResult.Failure(InventoryResultCode.InvalidItemId, "Invalid ItemId.", itemId, quantity, quantity);

            if (quantity <= 0)
                return InventoryResult.Failure(InventoryResultCode.InvalidQuantity, "Quantity must be > 0.", itemId, quantity, quantity);

            var remaining = quantity;

            var slots = state.Slots;
            for (var i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];
                if (!slot.HasItem || slot.ItemId != itemId)
                    continue;

                var take = Mathf.Min(slot.Quantity, remaining);
                remaining -= take;

                var newQty = slot.Quantity - take;
                slot.Set(slot.ItemId, newQty);
                slots[i] = slot;

                if (remaining == 0)
                    break;
            }

            if (remaining != quantity)
                InventoryChanged?.Invoke();

            if (remaining == 0)
                return InventoryResult.Success(itemId, quantity);

            return InventoryResult.Failure(
                InventoryResultCode.InsufficientItems,
                "Not enough items to remove requested quantity.",
                itemId,
                quantity,
                remaining);
        }

        /// <inheritdoc />
        public int GetQuantity(ItemId itemId)
        {
            if (!itemId.IsValid)
                return 0;

            var total = 0;
            var slots = state.Slots;

            for (var i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];
                if (!slot.HasItem || slot.ItemId != itemId)
                    continue;

                total += slot.Quantity;
            }

            return total;
        }

        /// <inheritdoc />
        public bool CanRemove(ItemId itemId, int quantity)
        {
            if (!itemId.IsValid)
                return false;

            if (quantity <= 0)
                return false;

            return GetQuantity(itemId) >= quantity;
        }

               /// <inheritdoc />
        public InventoryResult TryMove(int fromSlotIndex, int toSlotIndex)
        {
            var slots = state.Slots;

            if (!IsValidIndex(fromSlotIndex, slots.Count) || !IsValidIndex(toSlotIndex, slots.Count))
                return InventoryResult.FailureMove(InventoryResultCode.InvalidSlotIndex, "Invalid slot index.", fromSlotIndex, toSlotIndex);

            if (fromSlotIndex == toSlotIndex)
                return InventoryResult.FailureMove(InventoryResultCode.SameSlot, "Source and target slot are the same.", fromSlotIndex, toSlotIndex);

            var from = slots[fromSlotIndex];
            var to = slots[toSlotIndex];

            if (!from.HasItem)
                return InventoryResult.FailureMove(InventoryResultCode.SourceEmpty, "Source slot is empty.", fromSlotIndex, toSlotIndex);

            if (!itemDatabase.TryGet(from.ItemId, out var def) || def == null)
                return InventoryResult.FailureMove(InventoryResultCode.ItemNotFoundInDatabase, "Item not found in ItemDatabase.", fromSlotIndex, toSlotIndex);

            if (!to.HasItem)
            {
                slots[toSlotIndex] = from;
                slots[fromSlotIndex] = default;
                InventoryChanged?.Invoke();
                return InventoryResult.SuccessMove(fromSlotIndex, toSlotIndex);
            }

            if (to.ItemId != from.ItemId)
            {
                slots[toSlotIndex] = from;
                slots[fromSlotIndex] = to;
                InventoryChanged?.Invoke();
                return InventoryResult.SuccessMove(fromSlotIndex, toSlotIndex);
            }

            if (!def.Stackable)
                return InventoryResult.FailureMove(InventoryResultCode.NotStackable, "Item is not stackable.", fromSlotIndex, toSlotIndex);

            var maxStack = def.MaxStackSize;
            if (to.Quantity >= maxStack)
                return InventoryResult.FailureMove(InventoryResultCode.TargetStackFull, "Target stack is already full.", fromSlotIndex, toSlotIndex);

            var space = maxStack - to.Quantity;
            var moved = Mathf.Min(space, from.Quantity);

            to.Set(to.ItemId, to.Quantity + moved);
            from.Set(from.ItemId, from.Quantity - moved);

            slots[toSlotIndex] = to;
            slots[fromSlotIndex] = from.HasItem ? from : default;

            InventoryChanged?.Invoke();
            return InventoryResult.SuccessMove(fromSlotIndex, toSlotIndex);
        }

        /// <summary>
        /// Returns the slot content by index.
        /// </summary>
        public InventorySlotData GetSlot(int index)
        {
            var slots = state.Slots;
            if (!IsValidIndex(index, slots.Count))
                return default;

            return slots[index];
        }

        private int AddToExistingStacks(ItemId itemId, int remaining, int maxStack)
        {
            var slots = state.Slots;

            for (var i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];
                if (!slot.HasItem || slot.ItemId != itemId)
                    continue;

                if (slot.Quantity >= maxStack)
                    continue;

                var space = maxStack - slot.Quantity;
                var add = Mathf.Min(space, remaining);

                slot.Set(slot.ItemId, slot.Quantity + add);
                slots[i] = slot;

                remaining -= add;
                if (remaining == 0)
                    return 0;
            }

            return remaining;
        }

        private int AddToEmptySlots(ItemId itemId, int remaining, int perSlotMax)
        {
            var slots = state.Slots;

            for (var i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];
                if (slot.HasItem)
                    continue;

                var add = Mathf.Min(perSlotMax, remaining);
                slot.Set(itemId, add);
                slots[i] = slot;

                remaining -= add;
                if (remaining == 0)
                    return 0;
            }

            return remaining;
        }

        private static bool IsValidIndex(int index, int count)
        {
            return index >= 0 && index < count;
        }
    }
}
