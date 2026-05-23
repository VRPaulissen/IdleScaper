using System.Collections.Generic;

namespace ResourceAreas.Runtime
{
    /// <summary>
    /// Read model containing aggregated resource area bonus values.
    /// </summary>
    public sealed class ResourceBonusAggregate
    {
        private readonly Dictionary<ResourceBonusType, float> values = new Dictionary<ResourceBonusType, float>();

        /// <summary>
        /// Aggregated bonus values keyed by bonus type.
        /// </summary>
        public IReadOnlyDictionary<ResourceBonusType, float> Values => values;

        /// <summary>
        /// Adds a contribution to the aggregate.
        /// </summary>
        public void Add(ResourceBonusType type, float value)
        {
            if (values.TryGetValue(type, out var existing))
            {
                values[type] = existing + value;
                return;
            }

            values[type] = value;
        }

        /// <summary>
        /// Gets the aggregated value for a bonus type, or zero if none exists.
        /// </summary>
        public float GetValue(ResourceBonusType type)
        {
            return values.TryGetValue(type, out var value) ? value : 0f;
        }

        /// <summary>
        /// Gets a flat bonus value, or zero if none exists.
        /// </summary>
        public float GetFlat(ResourceBonusType type)
        {
            return GetValue(type);
        }

        /// <summary>
        /// Gets an additive multiplier bonus value, or zero if none exists.
        /// </summary>
        public float GetMultiplier(ResourceBonusType type)
        {
            return GetValue(type);
        }

        /// <summary>
        /// Removes all aggregated values.
        /// </summary>
        public void Clear()
        {
            values.Clear();
        }
    }
}
