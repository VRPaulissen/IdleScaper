using System;
using System.Collections.Generic;
using System.Reflection;
using Inventory;
using Items.Definitions;
using Items.Runtime;
using NUnit.Framework;
using Resource.Runtime;
using UnityEngine;

namespace Tests.EditMode
{
    /// <summary>
    /// Focused EditMode tests for atomic resource reward awarding.
    /// </summary>
    public sealed class ResourceRewardServiceTests
    {
        private static readonly ItemId StackableItemId = new ItemId("test.reward.stackable");
        private static readonly ItemId OtherStackableItemId = new ItemId("test.reward.other_stackable");
        private static readonly ItemId NonStackableItemId = new ItemId("test.reward.non_stackable");
        private static readonly ItemId OtherNonStackableItemId = new ItemId("test.reward.other_non_stackable");

        [Test]
        public void TryAward_WhenAllDropsFit_AddsAllDrops()
        {
            var fixture = RewardTestFixture.Create(slotCount: 3);

            var result = fixture.Rewards.TryAward(new[]
            {
                new ItemInstance(StackableItemId, 4),
                new ItemInstance(OtherStackableItemId, 2)
            });

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(4, fixture.Inventory.GetQuantity(StackableItemId));
            Assert.AreEqual(2, fixture.Inventory.GetQuantity(OtherStackableItemId));
        }

        [Test]
        public void TryAward_WhenNoDropsFit_AddsNothing()
        {
            var fixture = RewardTestFixture.Create(slotCount: 1);
            fixture.SetSlot(0, OtherStackableItemId, 10);

            var result = fixture.Rewards.TryAward(new[] { new ItemInstance(StackableItemId, 1) });

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ResourceRewardFailureReason.InventoryFull, result.Reason);
            Assert.AreEqual(0, fixture.Inventory.GetQuantity(StackableItemId));
            Assert.AreEqual(10, fixture.Inventory.GetQuantity(OtherStackableItemId));
        }

        [Test]
        public void TryAward_WhenSomeDropsFitButNotAll_AddsNothing()
        {
            var fixture = RewardTestFixture.Create(slotCount: 1);

            var result = fixture.Rewards.TryAward(new[]
            {
                new ItemInstance(NonStackableItemId, 1),
                new ItemInstance(OtherNonStackableItemId, 1)
            });

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ResourceRewardFailureReason.InventoryFull, result.Reason);
            Assert.AreEqual(0, fixture.Inventory.GetQuantity(NonStackableItemId));
            Assert.AreEqual(0, fixture.Inventory.GetQuantity(OtherNonStackableItemId));
            Assert.IsFalse(fixture.Inventory.GetSlot(0).HasItem);
        }

        [Test]
        public void TryAward_WithDuplicateItemIds_AggregatesAndAddsTotalQuantity()
        {
            var fixture = RewardTestFixture.Create(slotCount: 1);

            var result = fixture.Rewards.TryAward(new[]
            {
                new ItemInstance(StackableItemId, 4),
                new ItemInstance(StackableItemId, 6)
            });

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(10, fixture.Inventory.GetQuantity(StackableItemId));
            Assert.AreEqual(1, result.AwardedDrops.Count);
            Assert.AreEqual(10, result.AwardedDrops[0].Quantity);
        }

        [Test]
        public void TryAward_WithInvalidItemId_FailsSafely()
        {
            var fixture = RewardTestFixture.Create(slotCount: 1);

            var result = fixture.Rewards.TryAward(new[] { default(ItemInstance) });

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ResourceRewardFailureReason.InvalidDrop, result.Reason);
            Assert.IsFalse(fixture.Inventory.GetSlot(0).HasItem);
        }

        [Test]
        public void TryAward_WhenPreflightFails_DoesNotMutateInventory()
        {
            var fixture = RewardTestFixture.Create(slotCount: 1);
            fixture.SetSlot(0, StackableItemId, 8);

            fixture.Rewards.TryAward(new[] { new ItemInstance(StackableItemId, 3) });

            Assert.AreEqual(StackableItemId, fixture.Inventory.GetSlot(0).ItemId);
            Assert.AreEqual(8, fixture.Inventory.GetSlot(0).Quantity);
        }

        [Test]
        public void TryAward_WhenPreflightFails_DoesNotRaiseInventoryChanged()
        {
            var fixture = RewardTestFixture.Create(slotCount: 1);
            fixture.SetSlot(0, StackableItemId, 10);
            var eventCount = 0;
            fixture.Inventory.InventoryChanged += () => eventCount++;

            fixture.Rewards.TryAward(new[] { new ItemInstance(StackableItemId, 1) });

            Assert.AreEqual(0, eventCount);
        }

        [Test]
        public void TryAward_WhenSuccessful_RaisesInventoryChangedForAwardedStacks()
        {
            var fixture = RewardTestFixture.Create(slotCount: 1);
            var eventCount = 0;
            fixture.Inventory.InventoryChanged += () => eventCount++;

            fixture.Rewards.TryAward(new[]
            {
                new ItemInstance(StackableItemId, 4),
                new ItemInstance(StackableItemId, 6)
            });

            Assert.AreEqual(1, eventCount);
        }

        private sealed class RewardTestFixture
        {
            public InventoryService Inventory { get; private set; }
            public InventoryState State { get; private set; }
            public ResourceRewardService Rewards { get; private set; }

            public static RewardTestFixture Create(int slotCount)
            {
                var fixture = new RewardTestFixture();
                fixture.Initialize(slotCount);
                return fixture;
            }

            public void SetSlot(int index, ItemId itemId, int quantity)
            {
                var slot = State.Slots[index];
                slot.Set(itemId, quantity);
                State.Slots[index] = slot;
            }

            private void Initialize(int slotCount)
            {
                State = new InventoryState();
                var database = ScriptableObject.CreateInstance<ItemDatabase>();
                SetPrivateField(database, "definitions", new List<ItemDefinition>
                {
                    CreateItem(StackableItemId, true, 10),
                    CreateItem(OtherStackableItemId, true, 10),
                    CreateItem(NonStackableItemId, false, 1),
                    CreateItem(OtherNonStackableItemId, false, 1)
                });

                Inventory = new InventoryService(database, State, slotCount);
                Rewards = new ResourceRewardService(Inventory);
            }

            private static ItemDefinition CreateItem(ItemId itemId, bool stackable, int maxStackSize)
            {
                var item = ScriptableObject.CreateInstance<ItemDefinition>();
                SetPrivateField(item, "id", itemId);
                SetPrivateField(item, "displayName", itemId.ToString());
                SetPrivateField(item, "stackable", stackable);
                SetPrivateField(item, "maxStackSize", maxStackSize);
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
        }
    }
}
