using Items.Definitions;
using UnityEngine;

namespace Items.Runtime.Modules
{
    /// <summary>
    /// Adds gathering/tool behavior to an item (e.g., axe/pickaxe), such as hit interval and damage per hit.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Items/Modules/Gathering Tool", fileName = "Mod_GatheringTool_")]
    public sealed class GatheringToolModule : ItemModule
    {
        [SerializeField, Min(0.05f)] private float hitIntervalSeconds = 2f;
        [SerializeField, Min(1)] private int damagePerHit = 1;

        /// <summary>
        /// Time between gathering hits while using this tool.
        /// </summary>
        public float HitIntervalSeconds => hitIntervalSeconds;

        /// <summary>
        /// Damage applied per gathering hit while using this tool.
        /// </summary>
        public int DamagePerHit => damagePerHit;

        /// <inheritdoc />
        public override void Validate(ItemDefinition definition)
        {
            if (hitIntervalSeconds < 0.05f)
                hitIntervalSeconds = 0.05f;

            if (damagePerHit < 1)
                damagePerHit = 1;
        }
    }
}