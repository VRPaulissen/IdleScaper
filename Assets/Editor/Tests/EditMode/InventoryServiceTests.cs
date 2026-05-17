using System;
using System.Collections.Generic;
using System.Reflection;
using Equipment;
using Inventory;
using Items.Definitions;
using Items.Runtime;
using Items.Runtime.Modules;
using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode
{
    /// <summary>
    /// Focused EditMode tests for inventory service behavior.
    /// </summary>
    public sealed class InventoryServiceTests
    {
        private static readonly ItemId TestItemId = new ItemId("test.item.stackable");
        private static readonly ItemId OtherItemId = new ItemId("test.item.other");
        private static readonly ItemId NonStackableItemId = new ItemId("test.item.non_stackable");
        private static readonly ItemId OldEquipmentId = new ItemId("test.equipment.old");
        private static readonly ItemId NewEquipmentId = new ItemId("test.equipment.new");

        [Test]
        public void TryRemove_WhenOneStackHasEnoughQuantity_Succeeds()
        {
            var fixture = InventoryTestFixture.Create();
            fixture.SetSlot(0, TestItemId, 5);

            var result = fixture.Inventory.TryRemove(TestItemId, 3);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(2, fixture.Inventory.GetSlot(0).Quantity);
        }

        [Test]
        public void TryRemove_WhenMultipleStacksHaveEnoughQuantity_SucceedsAcrossStacks()
        {
            var fixture = InventoryTestFixture.Create();
            fixture.SetSlot(0, TestItemId, 2);
            fixture.SetSlot(1, TestItemId, 4);

            var result = fixture.Inventory.TryRemove(TestItemId, 5);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsFalse(fixture.Inventory.GetSlot(0).HasItem);
            Assert.AreEqual(1, fixture.Inventory.GetSlot(1).Quantity);
        }

        [Test]
        public void TryRemove_WhenStackReachesZero_ClearsSlot()
        {
            var fixture = InventoryTestFixture.Create();
            fixture.SetSlot(0, TestItemId, 3);

            fixture.Inventory.TryRemove(TestItemId, 3);

            Assert.IsFalse(fixture.Inventory.GetSlot(0).HasItem);
        }

        [Test]
        public void TryRemove_WhenTotalQuantityIsInsufficient_Fails()
        {
            var fixture = InventoryTestFixture.Create();
            fixture.SetSlot(0, TestItemId, 2);

            var result = fixture.Inventory.TryRemove(TestItemId, 3);

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(InventoryResultCode.InsufficientItems, result.Code);
            Assert.AreEqual(1, result.UnprocessedQuantity);
        }

        [Test]
        public void TryRemove_WhenTotalQuantityIsInsufficient_DoesNotChangeSlots()
        {
            var fixture = InventoryTestFixture.Create();
            fixture.SetSlot(0, TestItemId, 2);
            fixture.SetSlot(1, TestItemId, 1);
            fixture.SetSlot(2, OtherItemId, 4);

            fixture.Inventory.TryRemove(TestItemId, 4);

            Assert.AreEqual(TestItemId, fixture.Inventory.GetSlot(0).ItemId);
            Assert.AreEqual(2, fixture.Inventory.GetSlot(0).Quantity);
            Assert.AreEqual(TestItemId, fixture.Inventory.GetSlot(1).ItemId);
            Assert.AreEqual(1, fixture.Inventory.GetSlot(1).Quantity);
            Assert.AreEqual(OtherItemId, fixture.Inventory.GetSlot(2).ItemId);
            Assert.AreEqual(4, fixture.Inventory.GetSlot(2).Quantity);
        }

        [Test]
        public void TryRemove_WhenTotalQuantityIsInsufficient_DoesNotClearPartialStacks()
        {
            var fixture = InventoryTestFixture.Create();
            fixture.SetSlot(0, TestItemId, 1);

            fixture.Inventory.TryRemove(TestItemId, 2);

            Assert.IsTrue(fixture.Inventory.GetSlot(0).HasItem);
            Assert.AreEqual(1, fixture.Inventory.GetSlot(0).Quantity);
        }

        [Test]
        public void TryRemove_WhenTotalQuantityIsInsufficient_DoesNotRaiseInventoryChanged()
        {
            var fixture = InventoryTestFixture.Create();
            fixture.SetSlot(0, TestItemId, 1);
            var eventCount = 0;
            fixture.Inventory.InventoryChanged += () => eventCount++;

            fixture.Inventory.TryRemove(TestItemId, 2);

            Assert.AreEqual(0, eventCount);
        }

        [Test]
        public void TryRemove_WithExactTotalQuantity_RemovesAllMatchingStacks()
        {
            var fixture = InventoryTestFixture.Create();
            fixture.SetSlot(0, TestItemId, 2);
            fixture.SetSlot(1, TestItemId, 3);

            var result = fixture.Inventory.TryRemove(TestItemId, 5);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsFalse(fixture.Inventory.GetSlot(0).HasItem);
            Assert.IsFalse(fixture.Inventory.GetSlot(1).HasItem);
        }

        [Test]
        public void TryRemove_WithInvalidQuantity_FailsWithoutMutation()
        {
            var fixture = InventoryTestFixture.Create();
            fixture.SetSlot(0, TestItemId, 2);

            var result = fixture.Inventory.TryRemove(TestItemId, 0);

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(InventoryResultCode.InvalidQuantity, result.Code);
            Assert.AreEqual(2, fixture.Inventory.GetSlot(0).Quantity);
        }

        [Test]
        public void TryRemove_WithMissingItemId_FailsWithoutMutation()
        {
            var fixture = InventoryTestFixture.Create();
            fixture.SetSlot(0, TestItemId, 2);

            var result = fixture.Inventory.TryRemove(default, 1);

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(InventoryResultCode.InvalidItemId, result.Code);
            Assert.AreEqual(2, fixture.Inventory.GetSlot(0).Quantity);
        }

        [Test]
        public void TryRemove_WhenSuccessful_RaisesInventoryChangedOnce()
        {
            var fixture = InventoryTestFixture.Create();
            fixture.SetSlot(0, TestItemId, 2);
            var eventCount = 0;
            fixture.Inventory.InventoryChanged += () => eventCount++;

            fixture.Inventory.TryRemove(TestItemId, 1);

            Assert.AreEqual(1, eventCount);
        }

        [Test]
        public void CanAdd_WithEmptyInventoryAndStackableItem_ReturnsTrue()
        {
            var fixture = InventoryTestFixture.Create();

            Assert.IsTrue(fixture.Inventory.CanAdd(TestItemId, 5));
        }

        [Test]
        public void CanAdd_WhenExistingPartialStackHasEnoughSpace_ReturnsTrue()
        {
            var fixture = InventoryTestFixture.Create();
            fixture.SetSlot(0, TestItemId, 90);

            Assert.IsTrue(fixture.Inventory.CanAdd(TestItemId, 9));
        }

        [Test]
        public void CanAdd_WhenMultipleStacksAndEmptySlotsCanFitQuantity_ReturnsTrue()
        {
            var fixture = InventoryTestFixture.Create();
            fixture.SetSlot(0, TestItemId, 95);
            fixture.SetSlot(1, OtherItemId, 99);

            Assert.IsTrue(fixture.Inventory.CanAdd(TestItemId, 103));
        }

        [Test]
        public void CanAdd_WhenInventoryHasNoCapacity_ReturnsFalse()
        {
            var fixture = InventoryTestFixture.Create(slotCount: 2);
            fixture.SetSlot(0, OtherItemId, 99);
            fixture.SetSlot(1, TestItemId, 99);

            Assert.IsFalse(fixture.Inventory.CanAdd(TestItemId, 1));
        }

        [Test]
        public void CanAdd_WithInvalidQuantity_ReturnsFalse()
        {
            var fixture = InventoryTestFixture.Create();

            Assert.IsFalse(fixture.Inventory.CanAdd(TestItemId, 0));
        }

        [Test]
        public void CanAdd_WithMissingItemId_ReturnsFalse()
        {
            var fixture = InventoryTestFixture.Create();

            Assert.IsFalse(fixture.Inventory.CanAdd(default, 1));
        }

        [Test]
        public void CanAdd_DoesNotMutateInventory()
        {
            var fixture = InventoryTestFixture.Create();
            fixture.SetSlot(0, TestItemId, 95);
            fixture.SetSlot(1, OtherItemId, 10);

            fixture.Inventory.CanAdd(TestItemId, 5);

            Assert.AreEqual(TestItemId, fixture.Inventory.GetSlot(0).ItemId);
            Assert.AreEqual(95, fixture.Inventory.GetSlot(0).Quantity);
            Assert.AreEqual(OtherItemId, fixture.Inventory.GetSlot(1).ItemId);
            Assert.AreEqual(10, fixture.Inventory.GetSlot(1).Quantity);
        }

        [Test]
        public void CanAdd_DoesNotRaiseInventoryChanged()
        {
            var fixture = InventoryTestFixture.Create();
            var eventCount = 0;
            fixture.Inventory.InventoryChanged += () => eventCount++;

            fixture.Inventory.CanAdd(TestItemId, 1);

            Assert.AreEqual(0, eventCount);
        }

        [Test]
        public void CanAdd_RespectsMaxStackSize()
        {
            var fixture = InventoryTestFixture.Create(slotCount: 1);
            fixture.SetSlot(0, TestItemId, 98);

            Assert.IsTrue(fixture.Inventory.CanAdd(TestItemId, 1));
            Assert.IsFalse(fixture.Inventory.CanAdd(TestItemId, 2));
        }

        [Test]
        public void CanAdd_WithNonStackableItem_UsesOneSlotPerItem()
        {
            var fixture = InventoryTestFixture.Create(slotCount: 2);

            Assert.IsTrue(fixture.Inventory.CanAdd(NonStackableItemId, 2));
            Assert.IsFalse(fixture.Inventory.CanAdd(NonStackableItemId, 3));
        }

        [Test]
        public void TryEquipFromExternal_WhenPreviousItemCannotFitInventory_FailsWithoutChangingEquipment()
        {
            var fixture = InventoryTestFixture.Create(slotCount: 1);
            var equipment = fixture.CreateEquipmentService();
            equipment.TryEquipFromExternal(fixture.Inventory, fixture.GetItem(OldEquipmentId));
            fixture.SetSlot(0, OtherItemId, 99);

            var result = equipment.TryEquipFromExternal(fixture.Inventory, fixture.GetItem(NewEquipmentId));

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(EquipmentResultCode.InventoryNoSpaceForSwap, result.Code);
            Assert.AreEqual(OldEquipmentId, equipment.GetEquipped(EquipmentSlotId.MainHand));
            Assert.AreEqual(OtherItemId, fixture.Inventory.GetSlot(0).ItemId);
            Assert.AreEqual(99, fixture.Inventory.GetSlot(0).Quantity);
        }

        [Test]
        public void TryEquipFromExternal_WhenPreviousItemFitsInventory_SucceedsAndReturnsPreviousItem()
        {
            var fixture = InventoryTestFixture.Create(slotCount: 1);
            var equipment = fixture.CreateEquipmentService();
            equipment.TryEquipFromExternal(fixture.Inventory, fixture.GetItem(OldEquipmentId));

            var result = equipment.TryEquipFromExternal(fixture.Inventory, fixture.GetItem(NewEquipmentId));

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(NewEquipmentId, equipment.GetEquipped(EquipmentSlotId.MainHand));
            Assert.AreEqual(OldEquipmentId, fixture.Inventory.GetSlot(0).ItemId);
            Assert.AreEqual(1, fixture.Inventory.GetSlot(0).Quantity);
        }

        private sealed class InventoryTestFixture
        {
            public InventoryService Inventory { get; private set; }
            public InventoryState State { get; private set; }
            public ItemDatabase Database { get; private set; }

            public static InventoryTestFixture Create(int slotCount = 5)
            {
                var fixture = new InventoryTestFixture();
                fixture.Initialize(slotCount);
                return fixture;
            }

            public void SetSlot(int index, ItemId itemId, int quantity)
            {
                var slot = State.Slots[index];
                slot.Set(itemId, quantity);
                State.Slots[index] = slot;
            }

            public ItemDefinition GetItem(ItemId itemId)
            {
                if (Database.TryGetItem(itemId, out var definition))
                    return definition;

                return null;
            }

            public EquipmentService CreateEquipmentService()
            {
                return new EquipmentService(Database, new EquipmentState());
            }

            private void Initialize(int slotCount)
            {
                State = new InventoryState();
                Database = ScriptableObject.CreateInstance<ItemDatabase>();
                SetPrivateField(Database, "definitions", new List<ItemDefinition>
                {
                    CreateItem(TestItemId),
                    CreateItem(OtherItemId),
                    CreateItem(NonStackableItemId, false, 1),
                    CreateEquipmentItem(OldEquipmentId),
                    CreateEquipmentItem(NewEquipmentId)
                });

                Inventory = new InventoryService(Database, State, slotCount);
            }

            private static ItemDefinition CreateItem(ItemId itemId, bool stackable = true, int maxStackSize = 99)
            {
                var item = ScriptableObject.CreateInstance<ItemDefinition>();
                SetPrivateField(item, "id", itemId);
                SetPrivateField(item, "displayName", itemId.ToString());
                SetPrivateField(item, "stackable", stackable);
                SetPrivateField(item, "maxStackSize", maxStackSize);
                SetPrivateField(item, "modules", new List<ItemModule>());
                return item;
            }

            private static ItemDefinition CreateEquipmentItem(ItemId itemId)
            {
                var item = CreateItem(itemId, false, 1);
                var module = ScriptableObject.CreateInstance<EquipmentModule>();
                SetPrivateField(module, "slot", EquipmentSlotId.MainHand);
                SetPrivateField(item, "modules", new List<ItemModule> { module });
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
