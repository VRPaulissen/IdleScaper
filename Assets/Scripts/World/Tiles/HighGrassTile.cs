using UnityEngine;

namespace IdleScaper.World.Tiles
{
    public class HighGrassTile : Tile
    {
        public override TileType TileType => TileType.HighGrass;

        public override void OnEnter(GameObject actor)
        {
            // e.g. maybe visual rustle / slightly slower move.
        }
    }
}