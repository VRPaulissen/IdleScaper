using System;
using Equipment;
using Inventory;
using Items.Definitions;
using Items.Runtime;
using Items.Runtime.Modules;
using Player;
using Resource.Runtime;
using UnityEngine;
using Logger = Utilities.Logging.Logger;

namespace Utilities
{
    /// <summary>
    /// Debug bootstrapper that initializes all <see cref="ResourceNode"/> instances in the scene.
    /// Intended for fast iteration before a proper composition root/DI setup exists.
    /// </summary>
    public class DebugBootstrapper : MonoBehaviour
    {
        [Header("Dependencies")] 
        [SerializeField] private ItemDatabase itemDatabase;
        [SerializeField] private ItemDefinition axeDefinition;

        [Header("Options")] 
        [SerializeField] private bool includeInactive = true;
        [SerializeField] private bool initializeOnAwake = true;

        private IInventoryService inventoryService;
        private IEquipmentService equipmentService;
        private IResourceToolProvider toolProvider;

        private void Awake()
        {
            if (!initializeOnAwake)
                return;

            Initialize();
        }

        /// <summary>
        /// Initializes all resource nodes currently loaded in the scene.
        /// </summary>
        private void Initialize()
        {
            if (!TryResolveDependencies())
            {
                enabled = false;
                return;
            }

            var nodes = FindObjectsByType<ResourceNode>(includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            var initializedCount = 0;

            foreach (var node in nodes)
            {
                if (node == null)
                    continue;

                node.Initialize(inventoryService, toolProvider);
                initializedCount++;
            }

            Logger.Log($"Initialized {initializedCount} ResourceNodes.");
        }

        private bool TryResolveDependencies()
        {
            try
            {
                inventoryService = new InventoryService(itemDatabase, new InventoryState(), 100);
                equipmentService = new EquipmentService(itemDatabase, new EquipmentState());

                var equip = equipmentService.TryEquipFromExternal(inventoryService, axeDefinition);
                if (!equip.IsSuccess)
                {
                    Logger.LogError(
                        $"Failed to equip axe. " +
                        $"Code={equip.Code} Slot={equip.SlotId} Item={equip.ItemId} Message='{equip.Message}'");
                }

                toolProvider = new SlotToolProvider(equipmentService, itemDatabase, EquipmentSlotId.Axe);

                // Confirm provider can resolve an active tool.
                if (!toolProvider.TryGetActiveTool(out var tool))
                {
                    Logger.LogWarning(
                        $"ToolProvider could not resolve an active Axe tool. " +
                        $"Ensure the equipped Axe item has both EquipmentModule(slot=Axe) and GatheringToolModule.");
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Exception while resolving dependencies: {ex}");
                return false;
            }
        }
    }
}

