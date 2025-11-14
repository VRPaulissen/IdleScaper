using UnityEngine;

namespace IdleScaper.World
{
    /// <summary>
    /// Type of ground tile used for biome-aware generation.
    /// </summary>
    public enum TileType
    {
        Grass,
        HighGrass,
        Dirt,
        Mud,
        Sand,
        Rock,
        Water
    }
    
    /// <summary>
    /// Base class for all grid tiles.
    /// </summary>
    public abstract class Tile : MonoBehaviour
    {
        [SerializeField] protected int x;
        [SerializeField] protected int y;

        /// <summary>Grid position of this tile.</summary>
        public GridPosition Position => new GridPosition(x, y);

        /// <summary>Type of this tile.</summary>
        public abstract TileType TileType { get; }

        /// <summary>True if units can walk on this tile.</summary>
        public virtual bool IsWalkable => true;

        /// <summary>
        /// Initializes the tile at a specific grid position.
        /// </summary>
        public virtual void Initialize(GridPosition position)
        {
            x = position.X;
            y = position.Y;

            transform.position = GridManager.GridToWorld(position);
        }

        /// <summary>
        /// Called when an actor steps onto this tile.
        /// </summary>
        public virtual void OnEnter(GameObject actor) { }

        /// <summary>
        /// Called each tick while an actor stays on this tile.
        /// </summary>
        public virtual void OnStay(GameObject actor) { }

        /// <summary>
        /// Called when an actor leaves this tile.
        /// </summary>
        public virtual void OnExit(GameObject actor) { }

        protected virtual void OnEnable()
        {
            GridManager.RegisterTile(this);
        }

        protected virtual void OnDisable()
        {
            GridManager.UnregisterTile(this);
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            gameObject.name = $"{TileType} ({x},{y})" + (IsWalkable ? "" : " [Blocked]");
        }
#endif
    }
}