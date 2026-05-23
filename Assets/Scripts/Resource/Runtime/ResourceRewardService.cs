using System;
using System.Collections.Generic;
using Inventory;
using Items.Runtime;

namespace Resource.Runtime
{
    /// <summary>
    /// Awards rolled resource drops into inventory using an atomic all-or-nothing policy.
    /// </summary>
    public sealed class ResourceRewardService
    {
        private readonly IInventoryService inventory;

        /// <summary>
        /// Creates a resource reward service.
        /// </summary>
        public ResourceRewardService(IInventoryService inventory)
        {
            this.inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
        }

        /// <summary>
        /// Attempts to award all drops. No inventory is mutated unless the full batch can fit.
        /// </summary>
        public ResourceRewardResult TryAward(IReadOnlyList<ItemInstance> drops)
        {
            var aggregatedDrops = AggregateDrops(drops, out var invalidDrop);
            if (invalidDrop.HasValue)
            {
                return ResourceRewardResult.Failure(
                    ResourceRewardFailureReason.InvalidDrop,
                    "A resource drop had an invalid item id or quantity.",
                    new List<ItemInstance> { invalidDrop.Value });
            }

            if (aggregatedDrops.Count == 0)
                return ResourceRewardResult.Success(aggregatedDrops);

            if (!inventory.CanAddAll(aggregatedDrops))
            {
                return ResourceRewardResult.Failure(
                    ResourceRewardFailureReason.InventoryFull,
                    "Inventory cannot fit the full resource reward batch.",
                    aggregatedDrops);
            }

            return AddAggregatedDrops(aggregatedDrops);
        }

        private ResourceRewardResult AddAggregatedDrops(List<ItemInstance> drops)
        {
            for (var i = 0; i < drops.Count; i++)
            {
                var drop = drops[i];
                var result = inventory.TryAdd(drop.ItemId, drop.Quantity);
                if (result.IsSuccess)
                    continue;

                return ResourceRewardResult.Failure(
                    ResourceRewardFailureReason.InventoryAddFailed,
                    $"Inventory add failed after resource reward preflight: {result.Code}.",
                    drops);
            }

            return ResourceRewardResult.Success(drops);
        }

        private static List<ItemInstance> AggregateDrops(
            IReadOnlyList<ItemInstance> drops,
            out ItemInstance? invalidDrop)
        {
            invalidDrop = null;
            var results = new List<ItemInstance>();
            if (drops == null)
                return results;

            for (var i = 0; i < drops.Count; i++)
            {
                var drop = drops[i];
                if (!drop.ItemId.IsValid || drop.Quantity <= 0)
                {
                    invalidDrop = drop;
                    return results;
                }

                AddOrMerge(results, drop);
            }

            return results;
        }

        private static void AddOrMerge(List<ItemInstance> drops, ItemInstance drop)
        {
            for (var i = 0; i < drops.Count; i++)
            {
                var existing = drops[i];
                if (existing.ItemId != drop.ItemId)
                    continue;

                drops[i] = existing.WithQuantity(existing.Quantity + drop.Quantity);
                return;
            }

            drops.Add(drop);
        }
    }
}
