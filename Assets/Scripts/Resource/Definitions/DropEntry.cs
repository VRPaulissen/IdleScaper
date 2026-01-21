using System;
using Items.Definitions;
using Items.Runtime;
using Resource.Runtime;
using UnityEngine;
using Utilities.Calculations;

namespace Resource.Definitions
{
    /// <summary>
    /// Defines a single entry in a drop table.
    /// </summary>
    [Serializable]
    public struct DropEntry
    {
        [SerializeField] private ItemDefinition itemDefinition;
        [SerializeField, Min(1)] private int minQuantity;
        [SerializeField, Min(1)] private int maxQuantity;

        [Header("Chance")]
        [SerializeField] private bool isGuaranteed;
        [SerializeField] private DropChance chance;

        /// <summary>
        /// Item id that will be dropped.
        /// </summary>
        public ItemId ItemId => itemDefinition.Id;

        /// <summary>
        /// Minimum quantity to drop.
        /// </summary>
        public int MinQuantity => minQuantity;

        /// <summary>
        /// Maximum quantity to drop.
        /// </summary>
        public int MaxQuantity => maxQuantity;

        /// <summary>
        /// Whether the drop is always awarded.
        /// </summary>
        public bool IsGuaranteed => isGuaranteed;

        /// <summary>
        /// Chance for the drop when not guaranteed.
        /// </summary>
        public DropChance Chance => chance;

        /// <summary>
        /// Attempts to roll this entry and returns an item instance if successful.
        /// </summary>
        public bool TryRoll(IRandomSource random, out ItemInstance item)
        {
            item = default;

            if (!itemDefinition.Id.IsValid)
                return false;
            
            if (maxQuantity < minQuantity)
                return false;

            if (!isGuaranteed && !chance.Roll(random))
                return false;

            var quantity = (minQuantity == maxQuantity)
                ? minQuantity
                : random.NextInt(minQuantity, maxQuantity + 1);

            item = new ItemInstance(itemDefinition.Id, quantity);
            return true;
        }
    }
}
