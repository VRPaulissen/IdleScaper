using System;
using System.Collections.Generic;
using System.Reflection;
using Inventory;
using Items.Runtime;
using NUnit.Framework;
using Player;
using Resource.Runtime;
using UnityEngine;

namespace Tests.EditMode
{
    /// <summary>
    /// Focused EditMode tests for resource node spawner dependency wiring.
    /// </summary>
    public sealed class ResourceNodeSpawnerTests
    {
        [Test]
        public void Initialize_StoresProvidedInventoryService()
        {
            var spawner = CreateInactiveSpawner();
            var inventory = new FakeInventoryService();
            var provider = new FakeResourceToolProvider();

            spawner.Initialize(inventory, provider);

            Assert.AreSame(inventory, GetPrivateField<IInventoryService>(spawner, "inventoryService"));
        }

        [Test]
        public void Initialize_StoresProvidedToolProvider()
        {
            var spawner = CreateInactiveSpawner();
            var inventory = new FakeInventoryService();
            var provider = new FakeResourceToolProvider();

            spawner.Initialize(inventory, provider);

            Assert.AreSame(provider, GetPrivateField<IResourceToolProvider>(spawner, "toolProvider"));
        }

        [Test]
        public void TryResolveDependencies_WithoutInventorySource_FailsWithoutCreatingTemporaryInventory()
        {
            var spawner = CreateInactiveSpawner();

            var resolved = InvokePrivateBool(spawner, "TryResolveDependencies");

            Assert.IsFalse(resolved);
            Assert.IsNull(GetPrivateField<IInventoryService>(spawner, "inventoryService"));
        }

        private static ResourceNodeSpawner CreateInactiveSpawner()
        {
            var go = new GameObject("ResourceNodeSpawner Test");
            go.SetActive(false);
            return go.AddComponent<ResourceNodeSpawner>();
        }

        private static T GetPrivateField<T>(object target, string fieldName)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
                throw new MissingFieldException(target.GetType().Name, fieldName);

            return (T)field.GetValue(target);
        }

        private static bool InvokePrivateBool(object target, string methodName)
        {
            var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
                throw new MissingMethodException(target.GetType().Name, methodName);

            return (bool)method.Invoke(target, Array.Empty<object>());
        }

        private sealed class FakeResourceToolProvider : IResourceToolProvider
        {
            public bool TryGetActiveTool(out GatheringToolStats tool)
            {
                tool = new GatheringToolStats(1f, 1, 0f, 1, 0f, 1);
                return true;
            }
        }

        private sealed class FakeInventoryService : IInventoryService
        {
            public event Action InventoryChanged;

            public InventoryResult TryAdd(ItemId itemId, int quantity)
            {
                InventoryChanged?.Invoke();
                return InventoryResult.Success(itemId, quantity);
            }

            public bool CanAdd(ItemId itemId, int quantity)
            {
                return itemId.IsValid && quantity > 0;
            }

            public bool CanAddAll(IReadOnlyList<ItemInstance> items)
            {
                return true;
            }

            public InventoryResult TryRemove(ItemId itemId, int quantity)
            {
                return InventoryResult.Success(itemId, quantity);
            }

            public int GetQuantity(ItemId itemId)
            {
                return 0;
            }

            public bool CanRemove(ItemId itemId, int quantity)
            {
                return false;
            }

            public InventoryResult TryMove(int fromSlotIndex, int toSlotIndex)
            {
                return InventoryResult.SuccessMove(fromSlotIndex, toSlotIndex);
            }

            public InventorySlotData GetSlot(int slotIndex)
            {
                return default;
            }
        }
    }
}
