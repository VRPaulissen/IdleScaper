using System;
using System.Collections.Generic;
using Equipment;
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
        [SerializeField] private ItemDatabase itemDatabase;
        [SerializeField] private ItemDefinition axeDefinition;
        
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
                    Logger.LogError(
                        $"{nameof(ResourceNodeSpawner)} could not place node {i + 1}/{spawnCount}. " +
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
            // RectTransform.rect is in the container’s local space (anchoredPosition space)
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
