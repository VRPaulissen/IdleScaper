using System;
using System.Collections.Generic;
using System.Reflection;
using Items.Definitions;
using Items.Runtime;
using Items.Runtime.Diagnostics;
using Items.Runtime.Modules;
using NUnit.Framework;
using Tools.Runtime;
using UnityEngine;

namespace Tests.EditMode
{
    /// <summary>
    /// Focused EditMode tests for item database helper APIs and diagnostics.
    /// </summary>
    public sealed class ItemDatabaseTests
    {
        private static readonly ItemId ValidItemId = new ItemId("test.item.valid");
        private static readonly ItemId MissingItemId = new ItemId("test.item.missing");

        [Test]
        public void TryGetItem_WithRegisteredItem_ReturnsTrue()
        {
            var item = CreateItem(ValidItemId, "Valid Item");
            var database = CreateDatabase(item);

            var found = database.TryGetItem(ValidItemId, out var result);

            Assert.IsTrue(found);
            Assert.AreSame(item, result);
        }

        [Test]
        public void TryGetItem_WithMissingId_ReturnsFalse()
        {
            var database = CreateDatabase(CreateItem(ValidItemId, "Valid Item"));

            var found = database.TryGetItem(MissingItemId, out var result);

            Assert.IsFalse(found);
            Assert.IsNull(result);
        }

        [Test]
        public void TryGetItem_WithInvalidId_ReturnsFalse()
        {
            var database = CreateDatabase(CreateItem(ValidItemId, "Valid Item"));

            var found = database.TryGetItem(default, out var result);

            Assert.IsFalse(found);
            Assert.IsNull(result);
        }

        [Test]
        public void Contains_WithRegisteredItem_ReturnsTrue()
        {
            var database = CreateDatabase(CreateItem(ValidItemId, "Valid Item"));

            Assert.IsTrue(database.Contains(ValidItemId));
        }

        [Test]
        public void Contains_WithMissingId_ReturnsFalse()
        {
            var database = CreateDatabase(CreateItem(ValidItemId, "Valid Item"));

            Assert.IsFalse(database.Contains(MissingItemId));
        }

        [Test]
        public void GetAll_DoesNotExposeMutableInternalList()
        {
            var database = CreateDatabase(CreateItem(ValidItemId, "Valid Item"));
            var snapshot = database.GetAll();

            if (snapshot is IList<ItemDefinition> mutable)
                mutable.Clear();

            Assert.IsTrue(database.Contains(ValidItemId));
            Assert.AreEqual(1, database.GetAll().Count);
        }

        [Test]
        public void GetDiagnostics_WhenDuplicateItemIdExists_ReportsDuplicateId()
        {
            var first = CreateItem(ValidItemId, "First");
            var second = CreateItem(ValidItemId, "Second");
            var database = CreateDatabase(first, second);

            var diagnostics = database.GetDiagnostics();

            AssertHasDiagnostic(diagnostics, "ITEM_DATABASE_DUPLICATE_ID");
        }

        [Test]
        public void GetDiagnostics_WhenNullEntryExists_ReportsNullEntry()
        {
            var database = CreateDatabase(null);

            var diagnostics = database.GetDiagnostics();

            AssertHasDiagnostic(diagnostics, "ITEM_DATABASE_ENTRY_NULL");
        }

        [Test]
        public void GetDiagnostics_WhenItemIdIsMissing_ReportsMissingId()
        {
            var database = CreateDatabase(CreateItem(default, "Missing Id"));

            var diagnostics = database.GetDiagnostics();

            AssertHasDiagnostic(diagnostics, "ITEM_DATABASE_ENTRY_ID_MISSING");
            AssertHasDiagnostic(diagnostics, "ITEM_ID_MISSING");
        }

        [Test]
        public void GetDiagnostics_WhenStackSettingsAreInvalid_ReportsInvalidStackSize()
        {
            var item = CreateItem(ValidItemId, "Invalid Stack", true, 0);
            var database = CreateDatabase(item);

            var diagnostics = database.GetDiagnostics();

            AssertHasDiagnostic(diagnostics, "ITEM_STACK_SIZE_INVALID");
        }

        [Test]
        public void GetDiagnostics_WhenNullModuleExists_ReportsNullModule()
        {
            var item = CreateItem(ValidItemId, "Null Module");
            SetPrivateField(item, "modules", new List<ItemModule> { null });
            var database = CreateDatabase(item);

            var diagnostics = database.GetDiagnostics();

            AssertHasDiagnostic(diagnostics, "ITEM_MODULE_NULL");
        }

        [Test]
        public void GetDiagnostics_WhenDuplicateModuleTypeExists_ReportsDuplicateModuleType()
        {
            var item = CreateItem(ValidItemId, "Duplicate Module");
            SetPrivateField(item, "modules", new List<ItemModule>
            {
                ScriptableObject.CreateInstance<RequirementsModule>(),
                ScriptableObject.CreateInstance<RequirementsModule>()
            });
            var database = CreateDatabase(item);

            var diagnostics = database.GetDiagnostics();

            AssertHasDiagnostic(diagnostics, "ITEM_MODULE_DUPLICATE_TYPE");
        }

        [Test]
        public void GetDiagnostics_IncludesModuleLevelDiagnostics()
        {
            var item = CreateItem(ValidItemId, "Invalid Tool Part");
            var module = ScriptableObject.CreateInstance<ToolPartModule>();
            SetPrivateField(module, "compatibleToolId", default(ToolId));
            SetPrivateField(module, "compatibleSlotId", default(ToolPartSlotId));
            SetPrivateField(module, "maxLevel", 0);
            SetPrivateField(item, "modules", new List<ItemModule> { module });
            var database = CreateDatabase(item);

            var diagnostics = database.GetDiagnostics();

            AssertHasDiagnostic(diagnostics, "TOOL_PART_TOOL_ID_MISSING");
            AssertHasDiagnostic(diagnostics, "TOOL_PART_SLOT_ID_MISSING");
            AssertHasDiagnostic(diagnostics, "TOOL_PART_MAX_LEVEL_INVALID");
        }

        private static ItemDatabase CreateDatabase(params ItemDefinition[] items)
        {
            var database = ScriptableObject.CreateInstance<ItemDatabase>();
            SetPrivateField(database, "definitions", new List<ItemDefinition>(items));
            return database;
        }

        private static ItemDefinition CreateItem(
            ItemId itemId,
            string displayName,
            bool stackable = true,
            int maxStackSize = 99)
        {
            var item = ScriptableObject.CreateInstance<ItemDefinition>();
            SetPrivateField(item, "id", itemId);
            SetPrivateField(item, "displayName", displayName);
            SetPrivateField(item, "stackable", stackable);
            SetPrivateField(item, "maxStackSize", maxStackSize);
            SetPrivateField(item, "modules", new List<ItemModule>());
            return item;
        }

        private static void AssertHasDiagnostic(List<ItemDiagnostic> diagnostics, string code)
        {
            for (var i = 0; i < diagnostics.Count; i++)
            {
                if (diagnostics[i].Code == code)
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
    }
}
