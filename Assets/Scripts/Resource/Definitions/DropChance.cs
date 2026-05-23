using System;
using System.Collections.Generic;
using Items.Runtime.Diagnostics;
using UnityEngine;
using Utilities.Calculations;

namespace Resource.Definitions
{
    /// <summary>
    /// Represents a probability used for drop rolls (e.g., 1/500).
    /// </summary>
    [Serializable]
    public struct DropChance
    {
        [SerializeField, Min(0)] private int numerator;
        [SerializeField, Min(1)] private int denominator;

        /// <summary>
        /// Numerator for the chance fraction.
        /// </summary>
        public int Numerator => numerator;

        /// <summary>
        /// Denominator for the chance fraction.
        /// </summary>
        public int Denominator => denominator;

        /// <summary>
        /// Creates a chance as a fraction (numerator/denominator).
        /// </summary>
        public DropChance(int numerator, int denominator)
        {
            this.numerator = Mathf.Max(0, numerator);
            this.denominator = Mathf.Max(1, denominator);
        }

        /// <summary>
        /// Returns true if this chance represents a guaranteed success.
        /// </summary>
        public bool IsGuaranteed => numerator >= denominator;

        /// <summary>
        /// Appends non-mutating diagnostics for this chance value.
        /// </summary>
        public void CollectDiagnostics(
            List<ItemDiagnostic> results,
            UnityEngine.Object context,
            string ownerLabel,
            string codePrefix)
        {
            if (results == null)
                return;

            if (numerator < 0)
                results.Add(ItemDiagnostic.Error($"{codePrefix}_NUMERATOR_INVALID", $"{ownerLabel} has a negative drop chance numerator.", context));

            if (denominator <= 0)
                results.Add(ItemDiagnostic.Error($"{codePrefix}_DENOMINATOR_INVALID", $"{ownerLabel} has a drop chance denominator <= 0.", context));
        }

        /// <summary>
        /// Evaluates the chance using an RNG.
        /// </summary>
        public bool Roll(IRandomSource random)
        {
            if (numerator <= 0)
                return false;

            if (IsGuaranteed)
                return true;

            var roll = random.NextInt(0, denominator);
            return roll < numerator;
        }
    }
}
