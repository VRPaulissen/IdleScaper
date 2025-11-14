using UnityEngine;

namespace IdleScaper.World.Tiles
{
    public class WaterTile : Tile
    {
        public override TileType TileType => TileType.Water;
        public override bool IsWalkable => false;

        public override void OnEnter(GameObject actor)
        {
            
        }
    }
}