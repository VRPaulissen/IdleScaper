using System;
using System.Collections.Generic;
using System.Reflection;
using Items.Definitions;
using Items.Runtime;
using NUnit.Framework;
using Player;
using Resource.Definitions;
using Resource.Runtime;
using UnityEngine;
using Utilities.Calculations;

namespace Tests.EditMode
{
    /// <summary>
    /// Focused EditMode tests for existing resource gathering and drop rolling behavior.
    /// </summary>
    public sealed class ResourceGatheringLogicTests
    {
        private static readonly ItemId DropItemId = new ItemId("test.resource.drop");
        private static readonly ItemId RareDropItemId = new ItemId("test.resource.rare_drop");

        [Test]
        public void ApplyHit_ReducesDurability()
        {
            var resource = CreateResource(10, 1);
            var state = CreateState(10);
            var interactor = new ResourceInteractor(new SequenceRandomSource(9999, 9999));
            var tool = CreateTool(3);

            var depleted = interactor.ApplyHit(resource, state, tool, out var roll, out var drops);

            Assert.IsFalse(depleted);
            Assert.AreEqual(7, state.DurabilityCurrent);
            Assert.AreEqual(3, roll.FinalDamage);
            Assert.AreEqual(0, drops.Count);
        }

        [Test]
        public void ApplyHit_WhenDamageExceedsDurability_ClampsDurabilityToZero()
        {
            var resource = CreateResource(5, 1);
            var state = CreateState(5);
            var interactor = new ResourceInteractor(new SequenceRandomSource(9999, 9999));

            interactor.ApplyHit(resource, state, CreateTool(20), out _, out _);

            Assert.AreEqual(0, state.DurabilityCurrent);
        }

        [Test]
        public void ApplyHit_WhenDurabilityReachesZero_ReturnsDepleted()
        {
            var resource = CreateResource(5, 1);
            var state = CreateState(5);
            var interactor = new ResourceInteractor(new SequenceRandomSource(9999, 9999));

            var depleted = interactor.ApplyHit(resource, state, CreateTool(5), out _, out _);

            Assert.IsTrue(depleted);
        }

        [Test]
        public void ApplyHit_WhenDepleted_RollsDrops()
        {
            var item = CreateItem(DropItemId);
            var resource = CreateResource(1, 1, CreateDropEntry(item, 1, 1, true, new DropChance(0, 1)));
            var state = CreateState(1);
            var interactor = new ResourceInteractor(new SequenceRandomSource(9999, 9999));

            var depleted = interactor.ApplyHit(resource, state, CreateTool(1), out _, out var drops);

            Assert.IsTrue(depleted);
            Assert.AreEqual(1, drops.Count);
            Assert.AreEqual(DropItemId, drops[0].ItemId);
        }

        [Test]
        public void ApplyHit_WhenNotDepleted_DoesNotRollDrops()
        {
            var item = CreateItem(DropItemId);
            var resource = CreateResource(10, 1, CreateDropEntry(item, 1, 1, true, new DropChance(0, 1)));
            var state = CreateState(10);
            var interactor = new ResourceInteractor(new SequenceRandomSource(9999, 9999));

            var depleted = interactor.ApplyHit(resource, state, CreateTool(1), out _, out var drops);

            Assert.IsFalse(depleted);
            Assert.AreEqual(0, drops.Count);
        }

        [Test]
        public void ApplyHit_WhenToolDamageIsInvalid_UsesResourceBaseDamage()
        {
            var resource = CreateResource(10, 3);
            var state = CreateState(10);
            var interactor = new ResourceInteractor(new SequenceRandomSource(9999, 9999));

            interactor.ApplyHit(resource, state, default, out var roll, out _);

            Assert.AreEqual(7, state.DurabilityCurrent);
            Assert.AreEqual(3, roll.FinalDamage);
            Assert.AreEqual(GatheringHitType.Normal, roll.HitType);
        }

        [Test]
        public void ApplyHit_WhenCritRollSucceeds_AppliesCritMultiplier()
        {
            var resource = CreateResource(20, 1);
            var state = CreateState(20);
            var interactor = new ResourceInteractor(new SequenceRandomSource(9999, 0));
            var tool = new GatheringToolStats(1f, 2, 1f, 3, 0f, 10);

            interactor.ApplyHit(resource, state, tool, out var roll, out _);

            Assert.AreEqual(GatheringHitType.Crit, roll.HitType);
            Assert.AreEqual(6, roll.FinalDamage);
            Assert.AreEqual(14, state.DurabilityCurrent);
        }

        [Test]
        public void ApplyHit_WhenUltraCritRollSucceeds_AppliesUltraCritMultiplier()
        {
            var resource = CreateResource(20, 1);
            var state = CreateState(20);
            var interactor = new ResourceInteractor(new SequenceRandomSource(0));
            var tool = new GatheringToolStats(1f, 2, 1f, 3, 1f, 10);

            interactor.ApplyHit(resource, state, tool, out var roll, out _);

            Assert.AreEqual(GatheringHitType.UltraCrit, roll.HitType);
            Assert.AreEqual(20, roll.FinalDamage);
            Assert.AreEqual(0, state.DurabilityCurrent);
        }

        [Test]
        public void Roll_WithGuaranteedDrop_ReturnsExpectedItemId()
        {
            var item = CreateItem(DropItemId);
            var resource = CreateResource(1, 1, CreateDropEntry(item, 1, 1, true, new DropChance(0, 1)));
            var drops = new List<ItemInstance>();

            resource.Roll(new SequenceRandomSource(), drops);

            Assert.AreEqual(1, drops.Count);
            Assert.AreEqual(DropItemId, drops[0].ItemId);
        }

        [Test]
        public void Roll_WithChanceDropAndRollBelowChance_ReturnsDrop()
        {
            var item = CreateItem(DropItemId);
            var resource = CreateResource(1, 1, CreateDropEntry(item, 1, 1, false, new DropChance(1, 2)));
            var drops = new List<ItemInstance>();

            resource.Roll(new SequenceRandomSource(0), drops);

            Assert.AreEqual(1, drops.Count);
            Assert.AreEqual(DropItemId, drops[0].ItemId);
        }

        [Test]
        public void Roll_WithChanceDropAndRollAboveChance_ReturnsNoDrop()
        {
            var item = CreateItem(DropItemId);
            var resource = CreateResource(1, 1, CreateDropEntry(item, 1, 1, false, new DropChance(1, 2)));
            var drops = new List<ItemInstance>();

            resource.Roll(new SequenceRandomSource(1), drops);

            Assert.AreEqual(0, drops.Count);
        }

        [Test]
        public void Roll_WithQuantityRange_ReturnsQuantityWithinRange()
        {
            var item = CreateItem(DropItemId);
            var resource = CreateResource(1, 1, CreateDropEntry(item, 2, 5, true, new DropChance(0, 1)));
            var drops = new List<ItemInstance>();

            resource.Roll(new SequenceRandomSource(4), drops);

            Assert.AreEqual(1, drops.Count);
            Assert.GreaterOrEqual(drops[0].Quantity, 2);
            Assert.LessOrEqual(drops[0].Quantity, 5);
            Assert.AreEqual(4, drops[0].Quantity);
        }

        [Test]
        public void Roll_WithMultipleGuaranteedEntries_ReturnsMultipleDrops()
        {
            var first = CreateItem(DropItemId);
            var second = CreateItem(RareDropItemId);
            var resource = CreateResource(
                1,
                1,
                CreateDropEntry(first, 1, 1, true, new DropChance(0, 1)),
                CreateDropEntry(second, 2, 2, true, new DropChance(0, 1)));
            var drops = new List<ItemInstance>();

            resource.Roll(new SequenceRandomSource(), drops);

            Assert.AreEqual(2, drops.Count);
            Assert.AreEqual(DropItemId, drops[0].ItemId);
            Assert.AreEqual(RareDropItemId, drops[1].ItemId);
            Assert.AreEqual(2, drops[1].Quantity);
        }

        [Test]
        public void Roll_WithMissingDropItem_SkipsInvalidDrop()
        {
            var resource = CreateResource(1, 1, CreateDropEntry(null, 1, 1, true, new DropChance(0, 1)));
            var drops = new List<ItemInstance>();

            resource.Roll(new SequenceRandomSource(), drops);

            Assert.AreEqual(0, drops.Count);
        }

        private static ResourceDefinition CreateResource(
            int durability,
            int baseDamage,
            params DropEntry[] entries)
        {
            var resource = ScriptableObject.CreateInstance<ResourceDefinition>();
            resource.name = "Test Resource";
            SetPrivateField(resource, "durabilityMax", durability);
            SetPrivateField(resource, "hitIntervalSeconds", 1f);
            SetPrivateField(resource, "baseDamagePerHit", baseDamage);
            SetPrivateField(resource, "entries", new List<DropEntry>(entries));
            return resource;
        }

        private static ResourceRuntimeState CreateState(int durability)
        {
            var state = new ResourceRuntimeState();
            state.SetDurability(durability);
            return state;
        }

        private static GatheringToolStats CreateTool(int damage)
        {
            return new GatheringToolStats(1f, damage, 0f, 3, 0f, 10);
        }

        private static DropEntry CreateDropEntry(
            ItemDefinition item,
            int minQuantity,
            int maxQuantity,
            bool isGuaranteed,
            DropChance chance)
        {
            var entry = new DropEntry();
            SetStructPrivateField(ref entry, "itemDefinition", item);
            SetStructPrivateField(ref entry, "minQuantity", minQuantity);
            SetStructPrivateField(ref entry, "maxQuantity", maxQuantity);
            SetStructPrivateField(ref entry, "isGuaranteed", isGuaranteed);
            SetStructPrivateField(ref entry, "chance", chance);
            return entry;
        }

        private static ItemDefinition CreateItem(ItemId itemId)
        {
            var item = ScriptableObject.CreateInstance<ItemDefinition>();
            SetPrivateField(item, "id", itemId);
            SetPrivateField(item, "displayName", itemId.ToString());
            SetPrivateField(item, "stackable", true);
            SetPrivateField(item, "maxStackSize", 99);
            SetPrivateField(item, "modules", new List<ItemModule>());
            return item;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
                throw new MissingFieldException(target.GetType().Name, fieldName);

            field.SetValue(target, value);
        }

        private static void SetStructPrivateField<T>(ref T target, string fieldName, object value)
        {
            var boxed = (object)target;
            SetPrivateField(boxed, fieldName, value);
            target = (T)boxed;
        }

        private sealed class SequenceRandomSource : IRandomSource
        {
            private readonly Queue<int> values;

            public SequenceRandomSource(params int[] values)
            {
                this.values = new Queue<int>(values);
            }

            public int NextInt(int minInclusive, int maxExclusive)
            {
                if (values.Count == 0)
                    return minInclusive;

                var value = values.Dequeue();
                if (value < minInclusive)
                    return minInclusive;

                return value >= maxExclusive ? maxExclusive - 1 : value;
            }
        }
    }
}
