using System;
using System.Collections.Generic;
using Items.Definitions;
using Items.Runtime;
using Items.Runtime.Diagnostics;
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
        public ItemId ItemId => itemDefinition != null ? itemDefinition.Id : default;

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
        /// Attempts to get the drop item id without requiring callers to dereference the item definition.
        /// </summary>
        public bool TryGetItemId(out ItemId itemId)
        {
            itemId = itemDefinition != null ? itemDefinition.Id : default;
            return itemId.IsValid;
        }

        /// <summary>
        /// Appends non-mutating diagnostics for this drop entry.
        /// </summary>
        public void CollectDiagnostics(
            List<ItemDiagnostic> results,
            UnityEngine.Object context,
            string resourceName,
            int index,
            ItemDatabase itemDatabase = null)
        {
            if (results == null)
                return;

            var label = $"Resource '{resourceName}' drop {index + 1}";
            if (itemDefinition == null)
            {
                results.Add(ItemDiagnostic.Error("RESOURCE_DROP_ITEM_MISSING", $"{label} has no item definition.", context));
            }
            else if (!itemDefinition.Id.IsValid)
            {
                results.Add(ItemDiagnostic.Error("RESOURCE_DROP_ITEM_ID_MISSING", $"{label} item '{itemDefinition.name}' has no ItemId.", itemDefinition, itemDefinition.Id));
            }
            else if (itemDatabase != null && !itemDatabase.Contains(itemDefinition.Id))
            {
                results.Add(ItemDiagnostic.Warning("RESOURCE_DROP_ITEM_NOT_REGISTERED", $"{label} item '{itemDefinition.Id}' is not registered in the selected ItemDatabase.", itemDefinition, itemDefinition.Id));
            }

            if (minQuantity <= 0)
                results.Add(ItemDiagnostic.Error("RESOURCE_DROP_MIN_QUANTITY_INVALID", $"{label} has min quantity <= 0.", context, GetDiagnosticItemId()));

            if (maxQuantity <= 0)
                results.Add(ItemDiagnostic.Error("RESOURCE_DROP_MAX_QUANTITY_INVALID", $"{label} has max quantity <= 0.", context, GetDiagnosticItemId()));

            if (maxQuantity < minQuantity)
                results.Add(ItemDiagnostic.Error("RESOURCE_DROP_QUANTITY_RANGE_INVALID", $"{label} has max quantity lower than min quantity.", context, GetDiagnosticItemId()));

            if (!isGuaranteed)
                chance.CollectDiagnostics(results, context, label, "RESOURCE_DROP_CHANCE");
        }

        /// <summary>
        /// Attempts to roll this entry and returns an item instance if successful.
        /// </summary>
        public bool TryRoll(IRandomSource random, out ItemInstance item)
        {
            item = default;

            if (!TryGetItemId(out var itemId))
                return false;

            if (minQuantity <= 0 || maxQuantity <= 0)
                return false;

            if (maxQuantity < minQuantity)
                return false;

            if (!isGuaranteed && !chance.Roll(random))
                return false;

            var quantity = (minQuantity == maxQuantity)
                ? minQuantity
                : random.NextInt(minQuantity, maxQuantity + 1);

            item = new ItemInstance(itemId, quantity);
            return true;
        }

        private ItemId GetDiagnosticItemId()
        {
            return itemDefinition != null ? itemDefinition.Id : default;
        }
    }
}
