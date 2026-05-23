using System;
using Items.Runtime;
using UnityEngine;

namespace ResourceAreas.Definitions
{
    /// <summary>
    /// Data-only definition for an item drop from a resource.
    /// </summary>
    [Serializable]
    public sealed class ResourceDropDefinition
    {
        [SerializeField] private ItemId itemId;
        [SerializeField] private string displayNameOverride;
        [SerializeField] private ResourceDropCategory category;
        [SerializeField, Range(0f, 1f)] private float baseChance;
        [SerializeField, Min(1)] private int minAmount = 1;
        [SerializeField, Min(1)] private int maxAmount = 1;
        [SerializeField] private string requiredUnlockKey;

        /// <summary>
        /// Stable id of the item produced by this drop.
        /// </summary>
        public ItemId ItemId => itemId;

        /// <summary>
        /// Optional display name override for this drop.
        /// </summary>
        public string DisplayNameOverride => displayNameOverride;

        /// <summary>
        /// Drop category used for presentation and future behavior.
        /// </summary>
        public ResourceDropCategory Category => category;

        /// <summary>
        /// Base drop chance from 0 to 1.
        /// </summary>
        public float BaseChance => ClampChance(baseChance);

        /// <summary>
        /// Minimum item quantity produced by this drop.
        /// </summary>
        public int MinAmount => Math.Max(1, minAmount);

        /// <summary>
        /// Maximum item quantity produced by this drop.
        /// </summary>
        public int MaxAmount => Math.Max(MinAmount, maxAmount);

        /// <summary>
        /// Optional unlock key required before this drop is available.
        /// </summary>
        public string RequiredUnlockKey => requiredUnlockKey;

        /// <summary>
        /// Clamps this definition to valid serialized values.
        /// </summary>
        public void Normalize()
        {
            baseChance = ClampChance(baseChance);
            minAmount = Math.Max(1, minAmount);
            maxAmount = Math.Max(minAmount, maxAmount);
        }

        private static float ClampChance(float chance)
        {
            if (chance < 0f)
                return 0f;

            if (chance > 1f)
                return 1f;

            return chance;
        }
    }
}
