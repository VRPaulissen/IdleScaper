using System.Collections.Generic;
using IdleScaper.Scripts.Items.Definitions;
using UnityEngine;

namespace IdleScaper.Scripts.Items
{
    /// <summary>
    /// Simple inventory that stores item quantities.
    /// </summary>
    public class Inventory : MonoBehaviour
    {
        private readonly Dictionary<ItemDefinition, int> items = new();

        /// <summary>
        /// Adds a specific amount of an item.
        /// </summary>
        public void Add(ItemDefinition item, int amount)
        {
            if (item == null || amount <= 0)
                return;

            if (!items.TryAdd(item, amount))
                items[item] += amount;
        }

        /// <summary>
        /// Attempts to remove a specific amount of an item.
        /// Returns true if successful.
        /// </summary>
        public bool TryRemove(ItemDefinition item, int amount)
        {
            if (item == null || amount <= 0)
                return false;

            if (!items.TryGetValue(item, out var current) || current < amount)
                return false;

            current -= amount;
            if (current <= 0)
                items.Remove(item);
            else
                items[item] = current;

            return true;
        }

        /// <summary>
        /// Checks if the inventory contains at least the given amount.
        /// </summary>
        public bool HasItem(ItemDefinition item, int amount = 1)
        {
            return item != null &&
                   items.TryGetValue(item, out var current) &&
                   current >= amount;
        }

        /// <summary>
        /// Gets the current quantity of an item.
        /// Returns 0 if not present.
        /// </summary>
        public int GetQuantity(ItemDefinition item)
        {
            return item != null && items.TryGetValue(item, out var qty) ? qty : 0;
        }

        /// <summary>
        /// Clears all items from the inventory.
        /// </summary>
        public void Clear() => items.Clear();
    }
}