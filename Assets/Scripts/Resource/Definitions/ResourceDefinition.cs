using System;
using System.Collections.Generic;
using Items.Runtime;
using Items.Runtime.Diagnostics;
using UnityEngine;
using Utilities.Calculations;

namespace Resource.Definitions
{
    /// <summary>
    /// Defines how a resource node behaves (durability, hit interval, and depletion drops).
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Resources/Resource Definition", fileName = "ResourceDefinition")]
    public sealed class ResourceDefinition : ScriptableObject
    {
        [Header("Sprites")]
        [SerializeField] private Sprite aliveSprite;
        [SerializeField] private Sprite depletedSprite;
        
        [Header("Durability")]
        [SerializeField, Min(1)] private int durabilityMax = 5;

        [Header("Interaction")]
        [SerializeField, Min(0.1f)] private float hitIntervalSeconds = 2f;
        [SerializeField, Min(1)] private int baseDamagePerHit = 1;

        [Header("Drops")]
        [SerializeField] private List<DropEntry> entries = new();

        /// <summary>
        /// Alive sprite of the resource.
        /// </summary>
        public Sprite AliveSprite => aliveSprite;
        
        /// <summary>
        /// Depleted sprite of the resource.
        /// </summary>
        public Sprite DepletedSprite => depletedSprite;
        
        /// <summary>
        /// Maximum durability for this resource.
        /// </summary>
        public int DurabilityMax => durabilityMax;

        /// <summary>
        /// Time between hits while interacting.
        /// </summary>
        public float HitIntervalSeconds => hitIntervalSeconds;

        /// <summary>
        /// Base damage applied per hit before player/tool modifiers.
        /// </summary>
        public int BaseDamagePerHit => baseDamagePerHit;

        /// <summary>
        /// Appends non-mutating diagnostics for this resource definition.
        /// </summary>
        public void CollectDiagnostics(List<ItemDiagnostic> results, ItemDatabase itemDatabase = null)
        {
            if (results == null)
                return;

            if (string.IsNullOrWhiteSpace(name))
                results.Add(ItemDiagnostic.Warning("RESOURCE_NAME_MISSING", "ResourceDefinition has no asset name.", this));

            if (aliveSprite == null)
                results.Add(ItemDiagnostic.Warning("RESOURCE_ALIVE_SPRITE_MISSING", $"Resource '{name}' has no alive sprite.", this));

            if (depletedSprite == null)
                results.Add(ItemDiagnostic.Warning("RESOURCE_DEPLETED_SPRITE_MISSING", $"Resource '{name}' has no depleted sprite.", this));

            if (durabilityMax <= 0)
                results.Add(ItemDiagnostic.Error("RESOURCE_DURABILITY_INVALID", $"Resource '{name}' has max durability <= 0.", this));

            if (hitIntervalSeconds <= 0f)
                results.Add(ItemDiagnostic.Error("RESOURCE_HIT_INTERVAL_INVALID", $"Resource '{name}' has hit interval <= 0.", this));

            if (baseDamagePerHit <= 0)
                results.Add(ItemDiagnostic.Error("RESOURCE_BASE_DAMAGE_INVALID", $"Resource '{name}' has base damage <= 0.", this));

            CollectDropDiagnostics(results, itemDatabase);
        }
        
        /// <summary>
        /// Rolls the table and returns the resulting drops.
        /// Each entry is rolled independently (supports guaranteed + rare rolls).
        /// </summary>
        public void Roll(IRandomSource random, List<ItemInstance> resultsBuffer)
        {
            if (resultsBuffer == null)
                throw new ArgumentNullException(nameof(resultsBuffer));

            resultsBuffer.Clear();

            for (var i = 0; i < entries.Count; i++)
            {
                if (entries[i].TryRoll(random, out var item))
                    resultsBuffer.Add(item);
            }
        }

        private void CollectDropDiagnostics(List<ItemDiagnostic> results, ItemDatabase itemDatabase)
        {
            if (entries == null)
            {
                results.Add(ItemDiagnostic.Warning("RESOURCE_DROPS_NULL", $"Resource '{name}' has a null drop list.", this));
                return;
            }

            var itemIds = new HashSet<ItemId>();
            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                entry.CollectDiagnostics(results, this, name, i, itemDatabase);
                if (!entry.TryGetItemId(out var itemId))
                    continue;

                if (!itemIds.Add(itemId))
                    results.Add(ItemDiagnostic.Warning("RESOURCE_DROP_DUPLICATE_ITEM", $"Resource '{name}' has duplicate drop item '{itemId}'.", this, itemId));
            }
        }
    }
}
