namespace IdleScaper.World.Tiles
{
    public class RockTile : Tile
    {
        public override TileType TileType => TileType.Rock;
        public override bool IsWalkable => false;
    }
}