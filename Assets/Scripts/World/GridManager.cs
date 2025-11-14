using System.Collections.Generic;
using UnityEngine;

namespace IdleScaper.World
{
    /// <summary>
    /// Manages tiles and provides grid utilities.
    /// </summary>
    public static class GridManager
    {
        private const float CELL_SIZE = 1f;

        private static readonly Dictionary<GridPosition, Tile> tiles = new();
        
        /// <summary>
        /// Registers a tile at its grid position.
        /// </summary>
        public static void RegisterTile(Tile tile)
        {
            if (tile == null) return;

            var pos = tile.Position;
            if (!tiles.TryAdd(pos, tile))
            {
                Debug.LogWarning($"Duplicate tile at {pos}", tile);
            }
        }

        /// <summary>
        /// Unregisters a tile.
        /// </summary>
        public static void UnregisterTile(Tile tile)
        {
            if (tile == null) return;

            var pos = tile.Position;
            if (tiles.TryGetValue(pos, out var existing) && existing == tile)
            {
                tiles.Remove(pos);
            }
        }

        /// <summary>
        /// Tries to get the tile at the given grid position.
        /// </summary>
        public static bool TryGetTile(GridPosition pos, out Tile tile) =>
            tiles.TryGetValue(pos, out tile);

        /// <summary>
        /// Returns true if the given grid position is walkable.
        /// </summary>
        public static bool IsWalkable(GridPosition pos)
        {
            return tiles.TryGetValue(pos, out var tile) && tile.IsWalkable;
        }

        /// <summary>
        /// Converts a world position to nearest grid position.
        /// </summary>
        public static GridPosition WorldToGrid(Vector3 worldPos)
        {
            var x = Mathf.RoundToInt(worldPos.x / CELL_SIZE);
            var y = Mathf.RoundToInt(worldPos.z / CELL_SIZE);
            return new GridPosition(x, y);
        }

        /// <summary>
        /// Converts grid position to world position (center of tile).
        /// </summary>
        public static Vector3 GridToWorld(GridPosition pos)
        {
            return new Vector3(pos.X * CELL_SIZE, 0f, pos.Y * CELL_SIZE);
        }

        /// <summary>
        /// Gets 4-direction neighbors of a position.
        /// </summary>
        public static IEnumerable<GridPosition> GetNeighbors(GridPosition pos)
        {
            yield return new GridPosition(pos.X + 1, pos.Y);
            yield return new GridPosition(pos.X - 1, pos.Y);
            yield return new GridPosition(pos.X, pos.Y + 1);
            yield return new GridPosition(pos.X, pos.Y - 1);
        }
    }
}
