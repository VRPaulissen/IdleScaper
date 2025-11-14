using IdleScaper.Items.Definitions;
using UnityEngine;

namespace IdleScaper.Skills.Actions
{
    /// <summary>
    /// Defines a possible reward for a woodcutting action.
    /// </summary>
    [System.Serializable]
    public struct GatheringRewardEntry
    {
        /// <summary>Item that may be rewarded.</summary>
        public ItemDefinition Item;

        /// <summary>
        /// Drop weight or chance.
        /// If between 0 and 1, treated as direct chance.
        /// If greater than 1, treated as a relative weight.
        /// </summary>
        [Range(0f, 100f)]
        public float Weight;

        /// <summary>Minimum quantity when this reward is granted.</summary>
        public int MinAmount;

        /// <summary>Maximum quantity when this reward is granted.</summary>
        public int MaxAmount;

        /// <summary>
        /// Optional hint for total weight normalization in relative mode.
        /// </summary>
        public float WeightSumHint;

        /// <summary>
        /// Normalizes invalid values.
        /// </summary>
        public void Normalize()
        {
            if (MinAmount <= 0) MinAmount = 1;
            if (MaxAmount < MinAmount) MaxAmount = MinAmount;
        }
    }
}