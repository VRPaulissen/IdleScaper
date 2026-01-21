using System;
using Inventory;
using Items.Definitions;
using Items.Runtime;
using Items.Runtime.Modules;

namespace Equipment
{
    /// <summary>
    /// Abstraction for equipment operations.
    /// Used by UI, combat, and gathering systems.
    /// </summary>
    public interface IEquipmentService
    {
        /// <summary>
        /// Gets the item currently equipped in the given slot,
        /// or an invalid <see cref="ItemId"/> if empty.
        /// </summary>
        ItemId GetEquipped(EquipmentSlotId slotId);

        /// <summary>
        /// Attempts to equip an item from the given inventory slot.
        /// Returns false if the item is incompatible or the operation fails.
        /// </summary>
        EquipmentResult TryEquipFromInventory(IInventoryService inventory, int inventorySlotIndex);

        /// <summary>
        /// Attempts to equip an item from an external source.
        /// Returns false if the item is incompatible or the operation fails.
        /// </summary>
        EquipmentResult TryEquipFromExternal(IInventoryService inventory, ItemDefinition item);
        
        /// <summary>
        /// Attempts to unequip an item from the given slot back into the inventory.
        /// Returns false if the inventory has no space.
        /// </summary>
        EquipmentResult TryUnequipToInventory(IInventoryService inventory, EquipmentSlotId slotId);

        /// <summary>
        /// Raised whenever equipment changes.
        /// </summary>
        event Action EquipmentChanged;
    }
}