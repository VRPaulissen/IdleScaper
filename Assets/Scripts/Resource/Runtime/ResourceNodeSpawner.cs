using System;
using System.Collections.Generic;
using Equipment;
using IdleScaper.Bootstrap;
using Inventory;
using Items.Definitions;
using Items.Runtime;
using Items.Runtime.Modules;
using Player;
using UnityEngine;
using Logger = Utilities.Logging.Logger;
using Random = UnityEngine.Random;

namespace Resource.Runtime
{
    /// <summary>
    /// Spawns resource nodes in random non-overlapping positions and wires them into an interaction coordinator.
    /// </summary>
    public sealed class ResourceNodeSpawner : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private ResourceNode resourceNodePrefab;

        [Header("Dependencies")]
        [SerializeField] private ResourceInteractorCoordinator interactionCoordinator;
        [SerializeField] private SaveBootstrap saveBootstrap;
        [SerializeField] private ItemDatabase itemDatabase;
        [SerializeField, Min(1)] private int inventorySlotCount = 100;

        [Header("Prototype Tool Provider")]
        [SerializeField] private bool usePrototypeAxeProvider;
        [SerializeField] private ItemDefinition prototypeAxeDefinition;
        
        [Header("Spawn")]
        [SerializeField] private RectTransform spawnContainer;
        [SerializeField, Min(1)] private int spawnCount = 5;
        [SerializeField, Min(0.1f)] private float minDistancePixels = 120f;
        [SerializeField, Min(1)] private int maxAttemptsPerNode = 80;
        [SerializeField] private Vector2 paddingPixels = new Vector2(40f, 40f);

        private readonly List<ResourceNode> spawnedNodes = new List<ResourceNode>(64);

        private IInventoryService inventoryService;
        private IEquipmentService equipmentService;
        private IResourceToolProvider toolProvider;

        private void Awake()
        {
            if (!TryResolveDependencies())
            {
                enabled = false;
                return;
            }

            Spawn();
        }

        /// <summary>
        /// Injects runtime services before spawning.
        /// </summary>
        public void Initialize(
            IInventoryService inventory,
            IResourceToolProvider resourceToolProvider,
            ResourceInteractorCoordinator coordinator = null)
        {
            inventoryService = inventory;
            toolProvider = resourceToolProvider;

            if (coordinator != null)
                interactionCoordinator = coordinator;
        }

        /// <summary>
        /// Spawns nodes and wires them to the coordinator.
        /// </summary>
        public void Spawn()
        {
            if (resourceNodePrefab == null)
            {
                Logger.LogError($"{nameof(ResourceNodeSpawner)} missing prefab.");
                return;
            }

            if (interactionCoordinator == null)
            {
                Logger.LogError($"{nameof(ResourceNodeSpawner)} missing {nameof(ResourceInteractorCoordinator)}.");
                return;
            }

            if (spawnContainer == null)
            {
                Logger.LogError($"{nameof(ResourceNodeSpawner)} missing spawnContainer RectTransform.");
                return;
            }

            ClearSpawned();

            var positions = new List<Vector2>(spawnCount);

            for (var i = 0; i < spawnCount; i++)
            {
                if (!TryFindNonOverlappingAnchoredPosition(spawnContainer, positions, out var anchored))
                {
                    Logger.LogError($"{nameof(ResourceNodeSpawner)} could not place node {i + 1}/{spawnCount}. " +
                        $"Increase container size, reduce minDistancePixels, or increase maxAttemptsPerNode.");
                    break;
                }

                positions.Add(anchored);

                // Instantiate as child in UI hierarchy
                var node = Instantiate(resourceNodePrefab, spawnContainer);
                node.name = $"{resourceNodePrefab.name}_{i + 1}";

                // Ensure RectTransform is positioned properly in UI space
                var rect = node.transform as RectTransform;
                if (rect == null)
                {
                    Logger.LogError($"{node.name} is not a UI prefab (missing RectTransform). Use world spawning instead.");
                    Destroy(node.gameObject);
                    continue;
                }

                rect.anchoredPosition = anchored;
                rect.localRotation = Quaternion.identity;
                rect.localScale = Vector3.one;
                
                node.Initialize(inventoryService, toolProvider, interactionCoordinator);
                interactionCoordinator.Register(node);
                spawnedNodes.Add(node);
            }
        }

        /// <summary>
        /// Destroys all spawned nodes (runtime only) and clears tracking.
        /// </summary>
        public void ClearSpawned()
        {
            for (var i = spawnedNodes.Count - 1; i >= 0; i--)
            {
                var node = spawnedNodes[i];
                if (node == null)
                    continue;

                interactionCoordinator.Unregister(node);
                Destroy(node.gameObject);
            }

            spawnedNodes.Clear();
        }

        private bool TryFindNonOverlappingAnchoredPosition(
            RectTransform container,
            List<Vector2> existing,
            out Vector2 anchoredPosition)
        {
            // RectTransform.rect is in the container's local space (anchoredPosition space)
            var rect = container.rect;

            var min = rect.min + paddingPixels;
            var max = rect.max - paddingPixels;

            var minSqr = minDistancePixels * minDistancePixels;

            for (var attempt = 0; attempt < maxAttemptsPerNode; attempt++)
            {
                var x = Random.Range(min.x, max.x);
                var y = Random.Range(min.y, max.y);
                var candidate = new Vector2(x, y);

                var overlaps = false;
                for (var i = 0; i < existing.Count; i++)
                {
                    if ((existing[i] - candidate).sqrMagnitude < minSqr)
                    {
                        overlaps = true;
                        break;
                    }
                }

                if (overlaps)
                    continue;

                anchoredPosition = candidate;
                return true;
            }

            anchoredPosition = default;
            return false;
        }

        private bool TryResolveDependencies()
        {
            try
            {
                if (inventoryService == null && !TryResolveInventoryService())
                    return false;

                if (toolProvider == null && !TryResolveToolProvider())
                    return false;

                if (!toolProvider.TryGetActiveTool(out var tool))
                {
                    Logger.LogWarning(
                        $"ToolProvider could not resolve an active tool. " +
                        $"Ensure a resource tool provider is injected or enable the prototype Axe provider.");
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Exception while resolving dependencies: {ex}");
                return false;
            }
        }

        private bool TryResolveInventoryService()
        {
            if (itemDatabase == null)
            {
                Logger.LogError($"{nameof(ResourceNodeSpawner)} missing ItemDatabase.");
                return false;
            }

            if (saveBootstrap == null)
            {
                Logger.LogError($"{nameof(ResourceNodeSpawner)} missing SaveBootstrap. Resource rewards require the saved player inventory.");
                return false;
            }

            var saveData = saveBootstrap.GetSaveData();
            if (saveData == null)
            {
                Logger.LogError($"{nameof(ResourceNodeSpawner)} could not resolve save data.");
                return false;
            }

            saveData.Inventory ??= new InventoryState();
            saveData.Inventory.EnsureSize(inventorySlotCount);
            inventoryService = new InventoryService(itemDatabase, saveData.Inventory, inventorySlotCount);
            inventoryService.InventoryChanged += saveBootstrap.MarkSaveDirty;
            return true;
        }

        private bool TryResolveToolProvider()
        {
            if (!usePrototypeAxeProvider)
            {
                Logger.LogError($"{nameof(ResourceNodeSpawner)} has no resource tool provider. Inject one or enable the explicit prototype Axe provider.");
                return false;
            }

            if (itemDatabase == null)
            {
                Logger.LogError($"{nameof(ResourceNodeSpawner)} missing ItemDatabase.");
                return false;
            }

            if (prototypeAxeDefinition == null)
            {
                Logger.LogError($"{nameof(ResourceNodeSpawner)} prototype Axe provider is enabled but no Axe definition is assigned.");
                return false;
            }

            equipmentService = new EquipmentService(itemDatabase, new EquipmentState());
            var equip = equipmentService.TryEquipFromExternal(inventoryService, prototypeAxeDefinition);
            if (!equip.IsSuccess)
            {
                Logger.LogError(
                    $"Failed to equip prototype axe. " +
                    $"Code={equip.Code} Slot={equip.SlotId} Item={equip.ItemId} Message='{equip.Message}'");
                return false;
            }

            toolProvider = new SlotToolProvider(equipmentService, itemDatabase, EquipmentSlotId.Axe);
            return true;
        }
    }
}
