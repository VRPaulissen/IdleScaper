using System;
using System.Collections.Generic;
using System.Reflection;
using Items.Definitions;
using Items.Runtime;
using Items.Runtime.Diagnostics;
using Resource.Definitions;
using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode
{
    /// <summary>
    /// Focused EditMode tests for resource definition diagnostics.
    /// </summary>
    public sealed class ResourceDefinitionDiagnosticsTests
    {
        private static readonly ItemId DropItemId = new ItemId("test.resource.drop");

        [Test]
        public void CollectDiagnostics_WithValidResourceDefinition_HasNoBlockingErrors()
        {
            var item = CreateItem(DropItemId);
            var database = CreateDatabase(item);
            var resource = CreateResource(CreateDropEntry(item, 1, 3, true, new DropChance(0, 1)));

            var diagnostics = Collect(resource, database);

            AssertHasNoErrors(diagnostics);
        }

        [Test]
        public void CollectDiagnostics_WithMissingDropItem_ReportsError()
        {
            var resource = CreateResource(CreateDropEntry(null, 1, 1, true, new DropChance(0, 1)));

            var diagnostics = Collect(resource);

            AssertHasDiagnostic(diagnostics, "RESOURCE_DROP_ITEM_MISSING");
        }

        [Test]
        public void CollectDiagnostics_WithInvalidMinQuantity_ReportsError()
        {
            var item = CreateItem(DropItemId);
            var resource = CreateResource(CreateDropEntry(item, 0, 1, true, new DropChance(0, 1)));

            var diagnostics = Collect(resource);

            AssertHasDiagnostic(diagnostics, "RESOURCE_DROP_MIN_QUANTITY_INVALID");
        }

        [Test]
        public void CollectDiagnostics_WithMaxQuantityBelowMinQuantity_ReportsError()
        {
            var item = CreateItem(DropItemId);
            var resource = CreateResource(CreateDropEntry(item, 5, 2, true, new DropChance(0, 1)));

            var diagnostics = Collect(resource);

            AssertHasDiagnostic(diagnostics, "RESOURCE_DROP_QUANTITY_RANGE_INVALID");
        }

        [Test]
        public void CollectDiagnostics_WithDuplicateDropItemIds_ReportsWarning()
        {
            var item = CreateItem(DropItemId);
            var resource = CreateResource(
                CreateDropEntry(item, 1, 1, true, new DropChance(0, 1)),
                CreateDropEntry(item, 1, 1, true, new DropChance(0, 1)));

            var diagnostics = Collect(resource);

            AssertHasDiagnostic(diagnostics, "RESOURCE_DROP_DUPLICATE_ITEM", ItemDiagnosticSeverity.Warning);
        }

        [Test]
        public void CollectDiagnostics_WithInvalidDurability_ReportsError()
        {
            var item = CreateItem(DropItemId);
            var resource = CreateResource(CreateDropEntry(item, 1, 1, true, new DropChance(0, 1)));
            SetPrivateField(resource, "durabilityMax", 0);

            var diagnostics = Collect(resource);

            AssertHasDiagnostic(diagnostics, "RESOURCE_DURABILITY_INVALID");
        }

        [Test]
        public void CollectDiagnostics_WithInvalidChanceDrop_ReportsError()
        {
            var item = CreateItem(DropItemId);
            var resource = CreateResource(CreateDropEntry(item, 1, 1, false, CreateRawChance(1, 0)));

            var diagnostics = Collect(resource);

            AssertHasDiagnostic(diagnostics, "RESOURCE_DROP_CHANCE_DENOMINATOR_INVALID");
        }

        private static List<ItemDiagnostic> Collect(ResourceDefinition resource, ItemDatabase database = null)
        {
            var diagnostics = new List<ItemDiagnostic>();
            resource.CollectDiagnostics(diagnostics, database);
            return diagnostics;
        }

        private static ResourceDefinition CreateResource(params DropEntry[] entries)
        {
            var resource = ScriptableObject.CreateInstance<ResourceDefinition>();
            resource.name = "Test Resource";
            SetPrivateField(resource, "durabilityMax", 10);
            SetPrivateField(resource, "hitIntervalSeconds", 1f);
            SetPrivateField(resource, "baseDamagePerHit", 1);
            SetPrivateField(resource, "entries", new List<DropEntry>(entries));
            return resource;
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

        private static DropChance CreateRawChance(int numerator, int denominator)
        {
            var chance = new DropChance(1, 1);
            SetStructPrivateField(ref chance, "numerator", numerator);
            SetStructPrivateField(ref chance, "denominator", denominator);
            return chance;
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

        private static ItemDatabase CreateDatabase(params ItemDefinition[] items)
        {
            var database = ScriptableObject.CreateInstance<ItemDatabase>();
            SetPrivateField(database, "definitions", new List<ItemDefinition>(items));
            return database;
        }

        private static void AssertHasNoErrors(List<ItemDiagnostic> diagnostics)
        {
            for (var i = 0; i < diagnostics.Count; i++)
            {
                Assert.AreNotEqual(ItemDiagnosticSeverity.Error, diagnostics[i].Severity, diagnostics[i].Message);
            }
        }

        private static void AssertHasDiagnostic(
            List<ItemDiagnostic> diagnostics,
            string code,
            ItemDiagnosticSeverity? severity = null)
        {
            for (var i = 0; i < diagnostics.Count; i++)
            {
                if (diagnostics[i].Code != code)
                    continue;

                if (severity.HasValue)
                    Assert.AreEqual(severity.Value, diagnostics[i].Severity);

                return;
            }

            Assert.Fail($"Expected diagnostic '{code}' was not found.");
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
    }
}
