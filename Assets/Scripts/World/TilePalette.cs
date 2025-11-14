using UnityEngine;

namespace IdleScaper.World
{
    [CreateAssetMenu(menuName = "IdleScaper/World/Tile Palette")]
    public class TilePalette : ScriptableObject
    {
        public Tile grassTile;
        public Tile highGrassTile;
        public Tile dirtTile;
        public Tile mudTile;
        public Tile sandTile;
        public Tile rockTile;
        public Tile waterTile;

        public Tile GetPrefab(TileType type)
        {
            return type switch
            {
                TileType.Grass => grassTile,
                TileType.HighGrass => highGrassTile,
                TileType.Dirt => dirtTile,
                TileType.Mud => mudTile,
                TileType.Sand => sandTile,
                TileType.Rock => rockTile,
                TileType.Water => waterTile,
                _ => grassTile
            };
        }
    }
}