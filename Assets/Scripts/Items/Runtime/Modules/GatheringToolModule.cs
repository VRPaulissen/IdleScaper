using Items.Definitions;
using Items.Runtime.Diagnostics;
using UnityEngine;

namespace Items.Runtime.Modules
{
    /// <summary>
    /// Adds gathering/tool behavior to an item (e.g., axe/pickaxe), such as hit interval and damage per hit.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Items/Modules/Gathering Tool", fileName = "Mod_GatheringTool_")]
    public sealed class GatheringToolModule : ItemModule
    {
        private const float MIN_HIT_INTERVAL_SECONDS = 0.05f;

        [Header("Core")]
        [SerializeField, Min(MIN_HIT_INTERVAL_SECONDS)] private float hitIntervalSeconds = 2f;
        [SerializeField, Min(1)] private int damagePerHit = 1;

        [Header("Crits")]
        [SerializeField, Range(0f, 1f)] private float critChance = 0.05f;
        [SerializeField, Min(1)] private int critMultiplier = 3;

        [SerializeField, Range(0f, 1f)] private float ultraCritChance = 0.005f;
        [SerializeField, Min(1)] private int ultraCritMultiplier = 10;

        /// <summary>
        /// Time between gathering hits while using this tool.
        /// </summary>
        public float HitIntervalSeconds => hitIntervalSeconds;

        /// <summary>
        /// Base damage applied per gathering hit while using this tool (before crit rolls).
        /// </summary>
        public int DamagePerHit => damagePerHit;

        /// <summary>
        /// Chance [0..1] for a critical hit (applies if Ultra Crit did not trigger).
        /// </summary>
        public float CritChance => critChance;

        /// <summary>
        /// Damage multiplier when a critical hit triggers.
        /// </summary>
        public int CritMultiplier => critMultiplier;

        /// <summary>
        /// Chance [0..1] for an ultra critical hit (rolled first).
        /// </summary>
        public float UltraCritChance => ultraCritChance;

        /// <summary>
        /// Damage multiplier when an ultra critical hit triggers.
        /// </summary>
        public int UltraCritMultiplier => ultraCritMultiplier;

        /// <inheritdoc />
        public override void Validate(ItemDefinition definition)
        {
            if (hitIntervalSeconds < MIN_HIT_INTERVAL_SECONDS)
                hitIntervalSeconds = MIN_HIT_INTERVAL_SECONDS;

            critChance = Mathf.Clamp01(critChance);
            ultraCritChance = Mathf.Clamp01(ultraCritChance);
        }

        /// <inheritdoc />
        public override void CollectDiagnostics(ItemDefinition definition, System.Collections.Generic.List<ItemDiagnostic> results)
        {
            if (results == null)
                return;

            var itemId = definition != null ? definition.Id : default;
            if (hitIntervalSeconds < MIN_HIT_INTERVAL_SECONDS)
                results.Add(ItemDiagnostic.Error("GATHERING_TOOL_HIT_INTERVAL_INVALID", $"GatheringToolModule '{name}' has hit interval below {MIN_HIT_INTERVAL_SECONDS}.", this, itemId));

            if (damagePerHit < 1)
                results.Add(ItemDiagnostic.Error("GATHERING_TOOL_DAMAGE_INVALID", $"GatheringToolModule '{name}' has damage per hit < 1.", this, itemId));

            if (critChance < 0f || critChance > 1f)
                results.Add(ItemDiagnostic.Error("GATHERING_TOOL_CRIT_CHANCE_INVALID", $"GatheringToolModule '{name}' has crit chance outside 0..1.", this, itemId));

            if (ultraCritChance < 0f || ultraCritChance > 1f)
                results.Add(ItemDiagnostic.Error("GATHERING_TOOL_ULTRA_CRIT_CHANCE_INVALID", $"GatheringToolModule '{name}' has ultra crit chance outside 0..1.", this, itemId));
        }
    }
}
