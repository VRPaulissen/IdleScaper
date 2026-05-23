using System.Collections.Generic;

namespace ResourceAreas.Runtime
{
    /// <summary>
    /// Stores individual resource bonus contributions and resolves totals by bonus type.
    /// </summary>
    public sealed class ResourceBonusCollection
    {
        private readonly List<ResourceBonusContribution> contributions = new List<ResourceBonusContribution>(32);
        private readonly Dictionary<ResourceBonusType, float> totals = new Dictionary<ResourceBonusType, float>();

        /// <summary>
        /// Individual bonus contributions added to this collection.
        /// </summary>
        public IReadOnlyList<ResourceBonusContribution> Contributions => contributions;

        /// <summary>
        /// Aggregated bonus totals by bonus type.
        /// </summary>
        public IReadOnlyDictionary<ResourceBonusType, float> Totals => totals;

        /// <summary>
        /// Removes all contributions and totals.
        /// </summary>
        public void Clear()
        {
            contributions.Clear();
            totals.Clear();
        }

        /// <summary>
        /// Adds one bonus contribution and updates the total for its type.
        /// </summary>
        public void Add(ResourceBonusType type, float value, string sourceName, ResourceBonusSourceType sourceType, string sourceId = null)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                return;

            contributions.Add(new ResourceBonusContribution(type, value, sourceName, sourceType, sourceId));
            AddTotal(type, value);
        }

        /// <summary>
        /// Gets the total value for a bonus type, or zero if none exists.
        /// </summary>
        public float GetTotal(ResourceBonusType type)
        {
            return totals.TryGetValue(type, out var value) ? value : 0f;
        }

        /// <summary>
        /// Gets the flat total for a bonus type, or zero if none exists.
        /// </summary>
        public float GetFlat(ResourceBonusType type)
        {
            return GetTotal(type);
        }

        /// <summary>
        /// Gets the additive multiplier total for a bonus type, or zero if none exists.
        /// </summary>
        public float GetMultiplier(ResourceBonusType type)
        {
            return GetTotal(type);
        }

        private void AddTotal(ResourceBonusType type, float value)
        {
            if (totals.TryGetValue(type, out var existing))
            {
                totals[type] = existing + value;
                return;
            }

            totals[type] = value;
        }
    }
}
