using System;
using System.Collections.Generic;
using System.Linq;
using IdleScaper.Areas.Core;
using IdleScaper.Areas.Definitions;
using IdleScaper.Skills.Core;
using IdleScaper.World;
using UnityEngine;

namespace IdleScaper.Areas
{
    /// <summary>
    /// Runtime instance of an idle area that spawns resource spots.
    /// </summary>
    public class AreaInstance : MonoBehaviour
    {
        [Header("Definition")]
        [SerializeField] private AreaDefinition definition;

        [Header("References")]
        [SerializeField] private PlayerSkills playerSkills;
        [SerializeField] private TilePalette tilePalette;

        [Header("Shape Settings")]
        [SerializeField] private int width = 80;
        [SerializeField] private int height = 80;
        [SerializeField] private int tileCount = 120;
        [SerializeField] private int randomSeed = 12345;

        private readonly List<AreaSpotInstance> activeSpots = new();
        private readonly List<Tile> spawnedTiles = new();
        
        [Header("Debug")]
        [SerializeField] private GridPosition entranceGridPos;
        
        /// <summary>
        /// Returns true if the player meets all entry requirements.
        /// </summary>
        public bool CanEnter()
        {
            if (definition == null || definition.EntryRequirements == null) 
                return true;

            return definition.EntryRequirements.All(req => 
                playerSkills.HasLevel(req.Skill, req.RequiredLevel));
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                randomSeed = UnityEngine.Random.Range(1000, 99999999);
                InitializeArea();
            }
        }


        /// <summary>
        /// Initializes the area by generating tiles and spawning resource spots.
        /// Call when the player enters.
        /// </summary>
        public void InitializeArea()
        {
            ClearSpots();
            ClearTiles();

            if (definition == null || tilePalette == null)
            {
                Debug.LogWarning("AreaInstance: Missing AreaDefinition or TilePalette.");
                return;
            }

            // Generate connected shape within bounds.
            var shape = AreaShapeGenerator.GenerateShape(width, height, tileCount, randomSeed);

            // Generate biome-aware tile types for that shape.
            var tileTypes = AreaTileGenerator.GenerateTiles(definition.Biome, shape, randomSeed + 1);

            var w = shape.GetLength(0);
            var h = shape.GetLength(1);

            // Compute origin in grid coords based on this transform.
            var originGrid = GridManager.WorldToGrid(transform.position);

            // Spawn tiles using palette + tile types.
            for (var x = 0; x < w; x++)
            {
                for (var y = 0; y < h; y++)
                {
                    if (!shape[x, y])
                        continue;

                    var type = tileTypes[x, y];
                    var prefab = tilePalette.GetPrefab(type);
                    if (prefab == null)
                        continue;

                    var gridPos = new GridPosition(originGrid.X + x, originGrid.Y + y);
                    var tile = Instantiate(prefab, transform);
                    tile.Initialize(gridPos);
                    spawnedTiles.Add(tile);
                }
            }

            // Determine entrance tile (local → global).
            var localEntrance = AreaShapeGenerator.FindEntrance(shape);
            entranceGridPos = new GridPosition(originGrid.X + localEntrance.X, originGrid.Y + localEntrance.Y);

            // Spawn resource spots on valid walkable tiles.
            SpawnSpotsOnWalkableTiles(shape, originGrid);
        }

        /// <summary>
        /// World position of the entrance tile.
        /// </summary>
        public Vector3 GetEntranceWorldPosition()
        {
            return GridManager.GridToWorld(entranceGridPos);
        }

        /// <summary>
        /// Spawns resource spots only on walkable tiles.
        /// </summary>
        private void SpawnSpotsOnWalkableTiles(bool[,] shape, GridPosition originGrid)
        {
            if (definition.Spots == null || definition.Spots.Length == 0)
                return;
            
            var w = shape.GetLength(0);
            var h = shape.GetLength(1);
            var rand = new System.Random(randomSeed + 2);

            foreach (var spotDef in definition.Spots)
            {
                var toSpawn = Mathf.Max(0, spotDef.MaxInstances);
                var spawned = 0;

                for (var attempts = 0; attempts < 200 && spawned < toSpawn; attempts++)
                {
                    var x = rand.Next(0, w);
                    var y = rand.Next(0, h);
                    if (!shape[x, y])
                        continue;

                    var gridPos = new GridPosition(originGrid.X + x, originGrid.Y + y);

                    // Only place on walkable tiles.
                    if (!GridManager.IsWalkable(gridPos))
                        continue;

                    var worldPos = GridManager.GridToWorld(gridPos);
                    var prefab = spotDef.SpotPrefab;
                    if (prefab == null || spotDef.Action == null)
                        continue;

                    var go = Instantiate(prefab, worldPos, Quaternion.identity, transform);
                    var instance = go.GetComponent<AreaSpotInstance>() ?? go.AddComponent<AreaSpotInstance>();
                    instance.Initialize(spotDef.Action);

                    activeSpots.Add(instance);
                    spawned++;
                }
            }
        }

        /// <summary>Clears all spawned spots.</summary>
        public void ClearSpots()
        {
            foreach (var spot in activeSpots.Where(spot => spot != null))
            {
                Destroy(spot.gameObject);
            }

            activeSpots.Clear();
        }

        /// <summary>Clears all spawned tiles.</summary>
        private void ClearTiles()
        {
            foreach (var tile in spawnedTiles.Where(tile => tile != null))
            {
                Destroy(tile.gameObject);
            }

            spawnedTiles.Clear();
        }
    }
}