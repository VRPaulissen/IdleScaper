using System;
using System.Collections.Generic;
using Items.Runtime;
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
    }
}