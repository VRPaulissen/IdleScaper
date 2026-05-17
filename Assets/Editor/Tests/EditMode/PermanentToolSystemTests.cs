using System;
using System.Collections.Generic;
using System.Reflection;
using Inventory;
using Items.Definitions;
using Items.Runtime;
using Items.Runtime.Modules;
using NUnit.Framework;
using Tools.Definitions;
using Tools.Runtime;
using Tools.State;
using UnityEditor;
using UnityEngine;

namespace Tests.EditMode
{
    /// <summary>
    /// Focused EditMode tests for permanent modular tool runtime behavior.
    /// </summary>
    public sealed class PermanentToolSystemTests
    {
        private static readonly ItemId StoneHeadId = new ItemId("test.part.pickaxe.head.stone");
        private static readonly ItemId HandleId = new ItemId("test.part.pickaxe.handle.wood");
        private static readonly ItemId CostId = new ItemId("test.resource.stone");
        private static readonly ItemId PlainItemId = new ItemId("test.item.plain");
        private static readonly ToolId OtherToolId = new ToolId("tool.test.other");
        private static readonly string[] pickaxeModuleAssetPaths =
        {
            "Assets/Data/Items/ToolParts/Pickaxe/Modules/Stone Pickaxe Head Module.asset",
            "Assets/Data/Items/ToolParts/Pickaxe/Modules/Wooden Handle Module.asset",
            "Assets/Data/Items/ToolParts/Pickaxe/Modules/Fiber Rope Module.asset",
            "Assets/Data/Items/ToolParts/Pickaxe/Modules/Cloth Grip Module.asset",
            "Assets/Data/Items/ToolParts/Pickaxe/Modules/Basic Polish Module.asset",
            "Assets/Data/Items/ToolParts/Pickaxe/Modules/Copper Pickaxe Head Module.asset",
            "Assets/Data/Items/ToolParts/Pickaxe/Modules/Oak Handle Module.asset",
            "Assets/Data/Items/ToolParts/Pickaxe/Modules/Leather Binding Module.asset",
            "Assets/Data/Items/ToolParts/Pickaxe/Modules/Leather Grip Module.asset",
            "Assets/Data/Items/ToolParts/Pickaxe/Modules/Reinforced Coating Module.asset"
        };

        [Test]
        public void Normalize_EnsuresPickaxeExistsByDefault()
        {
            var state = new ToolCollectionState();

            state.Normalize();

            Assert.NotNull(state.GetTool(ToolId.Pickaxe));
        }

        [Test]
        public void Normalize_EnsuresPickaxeDefaultPresetContainsAllSlots()
        {
            var state = new ToolCollectionState();

            state.Normalize();

            var preset = state.GetTool(ToolId.Pickaxe).GetPreset(0);
            Assert.NotNull(preset.GetSlot(ToolPartSlotId.Head));
            Assert.NotNull(preset.GetSlot(ToolPartSlotId.Handle));
            Assert.NotNull(preset.GetSlot(ToolPartSlotId.Rope));
            Assert.NotNull(preset.GetSlot(ToolPartSlotId.Grip));
            Assert.NotNull(preset.GetSlot(ToolPartSlotId.Coating));
        }

        [Test]
        public void Normalize_WhenInstalledPartItemIsMissing_RemovesInstalledPart()
        {
            var fixture = ToolTestFixture.Create();
            fixture.HeadSlot.SetInstalledPart(new ItemId("test.part.missing"), 2);

            fixture.Tools.Normalize(fixture.ItemDatabase);

            Assert.IsFalse(fixture.HeadSlot.HasInstalledPart);
            Assert.AreEqual(0, fixture.HeadSlot.PartLevel);
        }

        [Test]
        public void Normalize_WhenInstalledPartLevelExceedsMax_ClampsToMax()
        {
            var fixture = ToolTestFixture.Create();
            fixture.HeadSlot.SetInstalledPart(StoneHeadId, 99);

            fixture.Tools.Normalize(fixture.ItemDatabase);

            Assert.AreEqual(5, fixture.HeadSlot.PartLevel);
        }

        [Test]
        public void TryInstallPart_WithCompatiblePart_Succeeds()
        {
            var fixture = ToolTestFixture.Create();
            fixture.Inventory.SetQuantity(StoneHeadId, 1);

            var result = fixture.PartService.TryInstallPart(ToolId.Pickaxe, ToolPartSlotId.Head, StoneHeadId);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(StoneHeadId, fixture.HeadSlot.InstalledPartItemId);
            Assert.AreEqual(0, fixture.Inventory.GetQuantity(StoneHeadId));
        }

        [Test]
        public void TryInstallPart_WithIncompatiblePart_Fails()
        {
            var fixture = ToolTestFixture.Create();
            fixture.Inventory.SetQuantity(HandleId, 1);

            var result = fixture.PartService.TryInstallPart(ToolId.Pickaxe, ToolPartSlotId.Head, HandleId);

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ToolPartOperationResultCode.IncompatibleSlot, result.Code);
            Assert.IsFalse(fixture.HeadSlot.HasInstalledPart);
            Assert.AreEqual(1, fixture.Inventory.GetQuantity(HandleId));
        }

        [Test]
        public void TryRemovePart_WithInstalledPart_ReturnsItToInventory()
        {
            var fixture = ToolTestFixture.Create();
            fixture.HeadSlot.SetInstalledPart(StoneHeadId, 0);

            var result = fixture.PartService.TryRemovePart(ToolId.Pickaxe, ToolPartSlotId.Head);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsFalse(fixture.HeadSlot.HasInstalledPart);
            Assert.AreEqual(1, fixture.Inventory.GetQuantity(StoneHeadId));
        }

        [Test]
        public void TryUpgradeInstalledPart_WithAvailableCosts_Succeeds()
        {
            var fixture = ToolTestFixture.Create();
            fixture.HeadSlot.SetInstalledPart(StoneHeadId, 0);
            fixture.Inventory.SetQuantity(CostId, 5);

            var result = fixture.UpgradeService.TryUpgradeInstalledPart(ToolId.Pickaxe, ToolPartSlotId.Head);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(1, fixture.HeadSlot.PartLevel);
        }

        [Test]
        public void TryUpgradeInstalledPart_WhenSlotIsEmpty_Fails()
        {
            var fixture = ToolTestFixture.Create();

            var result = fixture.UpgradeService.TryUpgradeInstalledPart(ToolId.Pickaxe, ToolPartSlotId.Head);

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ToolUpgradeFailureReason.EmptySlot, result.Reason);
        }

        [Test]
        public void TryUpgradeInstalledPart_WhenInstalledItemMissingFromDatabase_Fails()
        {
            var fixture = ToolTestFixture.Create();
            fixture.HeadSlot.SetInstalledPart(new ItemId("test.missing.item"), 0);

            var result = fixture.UpgradeService.TryUpgradeInstalledPart(ToolId.Pickaxe, ToolPartSlotId.Head);

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ToolUpgradeFailureReason.ItemDefinitionNotFound, result.Reason);
        }

        [Test]
        public void TryUpgradeInstalledPart_WhenInstalledItemHasNoToolPartModule_Fails()
        {
            var fixture = ToolTestFixture.Create();
            fixture.HeadSlot.SetInstalledPart(PlainItemId, 0);

            var result = fixture.UpgradeService.TryUpgradeInstalledPart(ToolId.Pickaxe, ToolPartSlotId.Head);

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ToolUpgradeFailureReason.NotToolPart, result.Reason);
        }

        [Test]
        public void TryUpgradeInstalledPart_WhenPartIsIncompatibleWithTool_Fails()
        {
            var fixture = ToolTestFixture.Create();
            var otherPartId = new ItemId("test.part.other.head");
            fixture.AddToolPart(otherPartId, OtherToolId, ToolPartSlotId.Head, 5);
            fixture.HeadSlot.SetInstalledPart(otherPartId, 0);

            var result = fixture.UpgradeService.TryUpgradeInstalledPart(ToolId.Pickaxe, ToolPartSlotId.Head);

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ToolUpgradeFailureReason.PartNotCompatibleWithTool, result.Reason);
        }

        [Test]
        public void TryUpgradeInstalledPart_WhenPartIsIncompatibleWithSlot_Fails()
        {
            var fixture = ToolTestFixture.Create();
            fixture.HeadSlot.SetInstalledPart(HandleId, 0);

            var result = fixture.UpgradeService.TryUpgradeInstalledPart(ToolId.Pickaxe, ToolPartSlotId.Head);

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ToolUpgradeFailureReason.PartNotCompatibleWithSlot, result.Reason);
        }

        [Test]
        public void TryUpgradeInstalledPart_WhenPartIsMaxLevel_Fails()
        {
            var fixture = ToolTestFixture.Create();
            fixture.HeadSlot.SetInstalledPart(StoneHeadId, 5);

            var result = fixture.UpgradeService.TryUpgradeInstalledPart(ToolId.Pickaxe, ToolPartSlotId.Head);

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ToolUpgradeFailureReason.MaxLevelReached, result.Reason);
        }

        [Test]
        public void TryUpgradeInstalledPart_WhenRecipeIsMissing_Fails()
        {
            var fixture = ToolTestFixture.Create(includeRecipe: false);
            fixture.HeadSlot.SetInstalledPart(StoneHeadId, 0);

            var result = fixture.UpgradeService.TryUpgradeInstalledPart(ToolId.Pickaxe, ToolPartSlotId.Head);

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ToolUpgradeFailureReason.RecipeNotFound, result.Reason);
        }

        [Test]
        public void TryUpgradeInstalledPart_WhenRequiredItemIsMissing_FailsWithMissingCost()
        {
            var fixture = ToolTestFixture.Create();
            fixture.HeadSlot.SetInstalledPart(StoneHeadId, 0);

            var result = fixture.UpgradeService.TryUpgradeInstalledPart(ToolId.Pickaxe, ToolPartSlotId.Head);

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ToolUpgradeFailureReason.MissingCost, result.Reason);
            Assert.AreEqual(1, result.MissingCosts.Count);
            Assert.AreEqual(CostId, result.MissingCosts[0].ItemId);
        }

        [Test]
        public void FailedUpgrade_DoesNotConsumeInventory()
        {
            var fixture = ToolTestFixture.Create();
            fixture.HeadSlot.SetInstalledPart(StoneHeadId, 0);
            fixture.Inventory.SetQuantity(CostId, 4);

            fixture.UpgradeService.TryUpgradeInstalledPart(ToolId.Pickaxe, ToolPartSlotId.Head);

            Assert.AreEqual(4, fixture.Inventory.GetQuantity(CostId));
        }

        [Test]
        public void FailedUpgrade_DoesNotChangePartLevel()
        {
            var fixture = ToolTestFixture.Create();
            fixture.HeadSlot.SetInstalledPart(StoneHeadId, 0);

            fixture.UpgradeService.TryUpgradeInstalledPart(ToolId.Pickaxe, ToolPartSlotId.Head);

            Assert.AreEqual(0, fixture.HeadSlot.PartLevel);
        }

        [Test]
        public void SuccessfulUpgrade_ConsumesExactItemCosts()
        {
            var fixture = ToolTestFixture.Create();
            fixture.HeadSlot.SetInstalledPart(StoneHeadId, 0);
            fixture.Inventory.SetQuantity(CostId, 7);

            fixture.UpgradeService.TryUpgradeInstalledPart(ToolId.Pickaxe, ToolPartSlotId.Head);

            Assert.AreEqual(2, fixture.Inventory.GetQuantity(CostId));
        }

        [Test]
        public void SuccessfulUpgrade_IncreasesLevelByOne()
        {
            var fixture = ToolTestFixture.Create();
            fixture.HeadSlot.SetInstalledPart(StoneHeadId, 0);
            fixture.Inventory.SetQuantity(CostId, 5);

            fixture.UpgradeService.TryUpgradeInstalledPart(ToolId.Pickaxe, ToolPartSlotId.Head);

            Assert.AreEqual(1, fixture.HeadSlot.PartLevel);
        }

        [Test]
        public void GetActiveBonuses_WithEmptyPickaxe_ReturnsZeroBonuses()
        {
            var fixture = ToolTestFixture.Create();

            var aggregate = fixture.BonusService.GetActiveBonuses(ToolId.Pickaxe);

            Assert.AreEqual(0f, aggregate.GetFlat(ToolBonusType.MiningDamageFlat));
            Assert.AreEqual(0f, aggregate.GetMultiplier(ToolBonusType.MiningDamageMultiplier));
        }

        [Test]
        public void GetActiveBonuses_WithCompatiblePart_ContributesBaseBonus()
        {
            var fixture = ToolTestFixture.Create();
            fixture.AddToolPart(
                StoneHeadId,
                ToolId.Pickaxe,
                ToolPartSlotId.Head,
                5,
                new ToolBonusValue(ToolBonusType.MiningDamageFlat, 2f, 0f));
            fixture.HeadSlot.SetInstalledPart(StoneHeadId, 0);

            var aggregate = fixture.BonusService.GetActiveBonuses(ToolId.Pickaxe);

            Assert.AreEqual(2f, aggregate.GetFlat(ToolBonusType.MiningDamageFlat));
        }

        [Test]
        public void GetActiveBonuses_WithPartLevel_ContributesBasePlusPerLevelBonus()
        {
            var fixture = ToolTestFixture.Create();
            fixture.AddToolPart(
                StoneHeadId,
                ToolId.Pickaxe,
                ToolPartSlotId.Head,
                5,
                new ToolBonusValue(ToolBonusType.MiningDamageFlat, 2f, 1f));
            fixture.HeadSlot.SetInstalledPart(StoneHeadId, 3);

            var aggregate = fixture.BonusService.GetActiveBonuses(ToolId.Pickaxe);

            Assert.AreEqual(5f, aggregate.GetFlat(ToolBonusType.MiningDamageFlat));
        }

        [Test]
        public void GetActiveBonuses_WithMultipleParts_AggregatesSameBonusType()
        {
            var fixture = ToolTestFixture.Create();
            fixture.AddToolPart(
                StoneHeadId,
                ToolId.Pickaxe,
                ToolPartSlotId.Head,
                5,
                new ToolBonusValue(ToolBonusType.MiningDamageFlat, 2f, 1f));
            fixture.AddToolPart(
                HandleId,
                ToolId.Pickaxe,
                ToolPartSlotId.Handle,
                5,
                new ToolBonusValue(ToolBonusType.MiningDamageFlat, 4f, 0.5f));

            fixture.HeadSlot.SetInstalledPart(StoneHeadId, 2);
            fixture.HandleSlot.SetInstalledPart(HandleId, 2);

            var aggregate = fixture.BonusService.GetActiveBonuses(ToolId.Pickaxe);

            Assert.AreEqual(9f, aggregate.GetFlat(ToolBonusType.MiningDamageFlat));
        }

        [Test]
        public void GetActiveBonuses_WithIncompatiblePart_ContributesNothing()
        {
            var fixture = ToolTestFixture.Create();
            fixture.AddToolPart(
                HandleId,
                ToolId.Pickaxe,
                ToolPartSlotId.Handle,
                5,
                new ToolBonusValue(ToolBonusType.MiningDamageFlat, 4f, 1f));
            fixture.HeadSlot.SetInstalledPart(HandleId, 3);

            var aggregate = fixture.BonusService.GetActiveBonuses(ToolId.Pickaxe);

            Assert.AreEqual(0f, aggregate.GetFlat(ToolBonusType.MiningDamageFlat));
        }

        [Test]
        public void GetActiveBonuses_WhenInstalledItemMissingFromDatabase_ContributesNothing()
        {
            var fixture = ToolTestFixture.Create();
            fixture.HeadSlot.SetInstalledPart(new ItemId("test.part.missing"), 3);

            var aggregate = fixture.BonusService.GetActiveBonuses(ToolId.Pickaxe);

            Assert.AreEqual(0f, aggregate.GetFlat(ToolBonusType.MiningDamageFlat));
        }

        [Test]
        public void GetActiveBonuses_WhenInstalledItemHasNoToolPartModule_ContributesNothing()
        {
            var fixture = ToolTestFixture.Create();
            fixture.HeadSlot.SetInstalledPart(PlainItemId, 3);

            var aggregate = fixture.BonusService.GetActiveBonuses(ToolId.Pickaxe);

            Assert.AreEqual(0f, aggregate.GetFlat(ToolBonusType.MiningDamageFlat));
        }

        [Test]
        public void GetActiveBonuses_WhenSavedLevelExceedsMax_UsesClampedLevelWithoutMutatingState()
        {
            var fixture = ToolTestFixture.Create();
            fixture.AddToolPart(
                StoneHeadId,
                ToolId.Pickaxe,
                ToolPartSlotId.Head,
                5,
                new ToolBonusValue(ToolBonusType.MiningDamageFlat, 2f, 1f));
            fixture.HeadSlot.SetInstalledPart(StoneHeadId, 99);

            var aggregate = fixture.BonusService.GetActiveBonuses(ToolId.Pickaxe);

            Assert.AreEqual(7f, aggregate.GetFlat(ToolBonusType.MiningDamageFlat));
            Assert.AreEqual(99, fixture.HeadSlot.PartLevel);
        }

        [Test]
        public void GetActiveBonuses_WithMultiplierBonuses_AggregatesAdditively()
        {
            var fixture = ToolTestFixture.Create();
            fixture.AddToolPart(
                StoneHeadId,
                ToolId.Pickaxe,
                ToolPartSlotId.Head,
                5,
                new ToolBonusValue(ToolBonusType.MiningDamageMultiplier, 0.1f, 0.05f));
            fixture.AddToolPart(
                HandleId,
                ToolId.Pickaxe,
                ToolPartSlotId.Handle,
                5,
                new ToolBonusValue(ToolBonusType.MiningDamageMultiplier, 0.25f, 0f));

            fixture.HeadSlot.SetInstalledPart(StoneHeadId, 1);
            fixture.HandleSlot.SetInstalledPart(HandleId, 0);

            var aggregate = fixture.BonusService.GetActiveBonuses(ToolId.Pickaxe);

            Assert.AreEqual(0.4f, aggregate.GetMultiplier(ToolBonusType.MiningDamageMultiplier), 0.0001f);
        }

        [Test]
        public void GetActiveBonuses_DoesNotMutateToolState()
        {
            var fixture = ToolTestFixture.Create();
            fixture.AddToolPart(
                StoneHeadId,
                ToolId.Pickaxe,
                ToolPartSlotId.Head,
                5,
                new ToolBonusValue(ToolBonusType.MiningDamageFlat, 2f, 1f));
            fixture.HeadSlot.SetInstalledPart(StoneHeadId, 2);

            fixture.BonusService.GetActiveBonuses(ToolId.Pickaxe);

            Assert.AreEqual(StoneHeadId, fixture.HeadSlot.InstalledPartItemId);
            Assert.AreEqual(2, fixture.HeadSlot.PartLevel);
        }

        [Test]
        public void GetActiveBonuses_DoesNotMutateInventory()
        {
            var fixture = ToolTestFixture.Create();
            fixture.AddToolPart(
                StoneHeadId,
                ToolId.Pickaxe,
                ToolPartSlotId.Head,
                5,
                new ToolBonusValue(ToolBonusType.MiningDamageFlat, 2f, 1f));
            fixture.HeadSlot.SetInstalledPart(StoneHeadId, 2);
            fixture.Inventory.SetQuantity(CostId, 7);

            fixture.BonusService.GetActiveBonuses(ToolId.Pickaxe);

            Assert.AreEqual(7, fixture.Inventory.GetQuantity(CostId));
        }

        [Test]
        public void PickaxeToolPartAssets_AllContainValidBonusData()
        {
            for (var i = 0; i < pickaxeModuleAssetPaths.Length; i++)
            {
                var path = pickaxeModuleAssetPaths[i];
                var module = AssetDatabase.LoadAssetAtPath<ToolPartModule>(path);
                Assert.NotNull(module, path);
                Assert.Greater(module.Bonuses.Count, 0, path);

                for (var bonusIndex = 0; bonusIndex < module.Bonuses.Count; bonusIndex++)
                {
                    var bonus = module.Bonuses[bonusIndex];
                    Assert.IsTrue(Enum.IsDefined(typeof(ToolBonusType), bonus.Type), path);
                    Assert.GreaterOrEqual(bonus.BaseValue, 0f, path);
                    Assert.GreaterOrEqual(bonus.ValuePerLevel, 0f, path);
                }
            }
        }

        private sealed class ToolTestFixture
        {
            public ToolCollectionState Tools { get; private set; }
            public FakeInventoryService Inventory { get; private set; }
            public ItemDatabase ItemDatabase { get; private set; }
            public PermanentToolPartService PartService { get; private set; }
            public ToolUpgradeService UpgradeService { get; private set; }
            public ToolBonusService BonusService { get; private set; }
            public ToolPartSlotState HeadSlot { get; private set; }
            public ToolPartSlotState HandleSlot { get; private set; }

            private List<ItemDefinition> itemDefinitions;

            public static ToolTestFixture Create(bool includeRecipe = true)
            {
                var fixture = new ToolTestFixture();
                fixture.Initialize(includeRecipe);
                return fixture;
            }

            public void AddToolPart(
                ItemId itemId,
                ToolId toolId,
                ToolPartSlotId slotId,
                int maxLevel,
                params ToolBonusValue[] bonuses)
            {
                itemDefinitions.Add(CreateToolPartItem(itemId, toolId, slotId, maxLevel, bonuses));
                SetPrivateField(ItemDatabase, "definitions", itemDefinitions);
                SetPrivateField(ItemDatabase, "map", null);
            }

            private void Initialize(bool includeRecipe)
            {
                Tools = new ToolCollectionState();
                Tools.Normalize();
                HeadSlot = Tools.GetTool(ToolId.Pickaxe).GetPreset(0).GetSlot(ToolPartSlotId.Head);
                HandleSlot = Tools.GetTool(ToolId.Pickaxe).GetPreset(0).GetSlot(ToolPartSlotId.Handle);

                itemDefinitions = new List<ItemDefinition>
                {
                    CreateToolPartItem(StoneHeadId, ToolId.Pickaxe, ToolPartSlotId.Head, 5, Array.Empty<ToolBonusValue>()),
                    CreateToolPartItem(HandleId, ToolId.Pickaxe, ToolPartSlotId.Handle, 5, Array.Empty<ToolBonusValue>()),
                    CreatePlainItem(CostId),
                    CreatePlainItem(PlainItemId)
                };

                ItemDatabase = ScriptableObject.CreateInstance<ItemDatabase>();
                SetPrivateField(ItemDatabase, "definitions", itemDefinitions);

                Inventory = new FakeInventoryService();
                var recipeCatalog = CreateRecipeCatalog(includeRecipe);
                PartService = new PermanentToolPartService(Tools, ItemDatabase, Inventory);
                UpgradeService = new ToolUpgradeService(Tools, ItemDatabase, Inventory, recipeCatalog);
                BonusService = new ToolBonusService(Tools, ItemDatabase);
            }

            private static ToolUpgradeRecipeCatalog CreateRecipeCatalog(bool includeRecipe)
            {
                var catalog = ScriptableObject.CreateInstance<ToolUpgradeRecipeCatalog>();
                var recipes = new List<ToolUpgradeRecipeDefinition>();
                if (includeRecipe)
                    recipes.Add(CreateRecipe(StoneHeadId, 0, 1, 5));

                SetPrivateField(catalog, "recipes", recipes);
                return catalog;
            }

            private static ToolUpgradeRecipeDefinition CreateRecipe(ItemId partItemId, int fromLevel, int toLevel, int costQuantity)
            {
                var recipe = ScriptableObject.CreateInstance<ToolUpgradeRecipeDefinition>();
                SetPrivateField(recipe, "toolId", ToolId.Pickaxe);
                SetPrivateField(recipe, "slotId", ToolPartSlotId.Head);
                SetPrivateField(recipe, "partItemId", partItemId);
                SetPrivateField(recipe, "fromLevel", fromLevel);
                SetPrivateField(recipe, "toLevel", toLevel);
                SetPrivateField(recipe, "itemCosts", new List<ToolUpgradeCost> { new ToolUpgradeCost(CostId, costQuantity) });
                return recipe;
            }

            private static ItemDefinition CreateToolPartItem(
                ItemId itemId,
                ToolId toolId,
                ToolPartSlotId slotId,
                int maxLevel,
                IReadOnlyList<ToolBonusValue> bonuses)
            {
                var item = CreatePlainItem(itemId);
                var module = ScriptableObject.CreateInstance<ToolPartModule>();
                SetPrivateField(module, "compatibleToolId", toolId);
                SetPrivateField(module, "compatibleSlotId", slotId);
                SetPrivateField(module, "maxLevel", maxLevel);
                SetPrivateField(module, "bonuses", new List<ToolBonusValue>(bonuses));
                SetPrivateField(item, "modules", new List<ItemModule> { module });
                return item;
            }

            private static ItemDefinition CreatePlainItem(ItemId itemId)
            {
                var item = ScriptableObject.CreateInstance<ItemDefinition>();
                SetPrivateField(item, "id", itemId);
                SetPrivateField(item, "displayName", itemId.ToString());
                SetPrivateField(item, "stackable", true);
                SetPrivateField(item, "maxStackSize", 999);
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

        private sealed class FakeInventoryService : IInventoryService
        {
            private readonly Dictionary<ItemId, int> quantities = new Dictionary<ItemId, int>();

            public event Action InventoryChanged;

            public InventoryResult TryAdd(ItemId itemId, int quantity)
            {
                if (!itemId.IsValid)
                    return InventoryResult.Failure(InventoryResultCode.InvalidItemId, "Invalid item id.", itemId, quantity, quantity);

                if (quantity <= 0)
                    return InventoryResult.Failure(InventoryResultCode.InvalidQuantity, "Invalid quantity.", itemId, quantity, quantity);

                SetQuantity(itemId, GetQuantity(itemId) + quantity);
                InventoryChanged?.Invoke();
                return InventoryResult.Success(itemId, quantity);
            }

            public InventoryResult TryRemove(ItemId itemId, int quantity)
            {
                if (!itemId.IsValid)
                    return InventoryResult.Failure(InventoryResultCode.InvalidItemId, "Invalid item id.", itemId, quantity, quantity);

                if (quantity <= 0)
                    return InventoryResult.Failure(InventoryResultCode.InvalidQuantity, "Invalid quantity.", itemId, quantity, quantity);

                var available = GetQuantity(itemId);
                if (available < quantity)
                    return InventoryResult.Failure(InventoryResultCode.InsufficientItems, "Insufficient items.", itemId, quantity, quantity - available);

                SetQuantity(itemId, available - quantity);
                InventoryChanged?.Invoke();
                return InventoryResult.Success(itemId, quantity);
            }

            public int GetQuantity(ItemId itemId)
            {
                if (!itemId.IsValid)
                    return 0;

                return quantities.TryGetValue(itemId, out var quantity) ? quantity : 0;
            }

            public bool CanRemove(ItemId itemId, int quantity)
            {
                if (quantity <= 0)
                    return false;

                return GetQuantity(itemId) >= quantity;
            }

            public InventoryResult TryMove(int fromSlotIndex, int toSlotIndex)
            {
                return InventoryResult.FailureMove(InventoryResultCode.InvalidSlotIndex, "Fake inventory has no slots.", fromSlotIndex, toSlotIndex);
            }

            public InventorySlotData GetSlot(int slotIndex)
            {
                return default;
            }

            public void SetQuantity(ItemId itemId, int quantity)
            {
                if (!itemId.IsValid)
                    return;

                if (quantity <= 0)
                {
                    quantities.Remove(itemId);
                    return;
                }

                quantities[itemId] = quantity;
            }
        }
    }
}
