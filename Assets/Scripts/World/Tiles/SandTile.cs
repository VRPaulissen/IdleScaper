using UnityEngine;

namespace IdleScaper.World.Tiles
{
    public class SandTile : Tile
    {
        public override TileType TileType => TileType.Sand;

        public override void OnStay(GameObject actor)
        {
        }
    }
}