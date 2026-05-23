using System;
using System.Collections.Generic;
using Items.Runtime;

namespace Inventory
{
    /// <summary>
    /// Abstraction for inventory operations.
    /// Used by gameplay systems (rewards, crafting, equipment).
    /// </summary>
    public interface IInventoryService
    {
        /// <summary>
        /// Attempts to add the given item and quantity to the inventory.
        /// Returns false if not all items could be added.
        /// </summary>
        InventoryResult TryAdd(ItemId itemId, int quantity);

        /// <summary>
        /// Returns true if the inventory can fit the requested item quantity without mutating state.
        /// </summary>
        bool CanAdd(ItemId itemId, int quantity);

        /// <summary>
        /// Returns true if the inventory can fit every item stack without mutating state.
        /// </summary>
        bool CanAddAll(IReadOnlyList<ItemInstance> items);

        /// <summary>
        /// Attempts to remove the given item and quantity from the inventory.
        /// Returns false if insufficient items are present.
        /// </summary>
        InventoryResult TryRemove(ItemId itemId, int quantity);

        /// <summary>
        /// Returns the total quantity of the given item across all inventory slots.
        /// </summary>
        int GetQuantity(ItemId itemId);

        /// <summary>
        /// Returns true if the inventory contains at least the requested quantity of the given item.
        /// </summary>
        bool CanRemove(ItemId itemId, int quantity);

        /// <summary>
        /// Attempts to move or merge items between two inventory slots.
        /// Intended for UI interactions (drag & drop).
        /// </summary>
        InventoryResult TryMove(int fromSlotIndex, int toSlotIndex);

        /// <summary>
        /// Returns the inventory slot data at the given index.
        /// </summary>
        InventorySlotData GetSlot(int slotIndex);
        
        /// <summary>
        /// Raised whenever the inventory content changes.
        /// </summary>
        event Action InventoryChanged;
    }
}
