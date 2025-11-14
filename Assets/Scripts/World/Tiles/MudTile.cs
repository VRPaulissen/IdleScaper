using UnityEngine;

namespace IdleScaper.World.Tiles
{
    public class MudTile : Tile
    {
        public override TileType TileType => TileType.Mud;

        public override void OnStay(GameObject actor)
        {
        }
    }
}