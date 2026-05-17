using System.Collections.Generic;

namespace Tools.Runtime
{
    /// <summary>
    /// Read model containing aggregated permanent tool bonus values.
    /// </summary>
    public sealed class ToolBonusAggregate
    {
        private readonly Dictionary<ToolBonusType, float> values = new Dictionary<ToolBonusType, float>();

        /// <summary>
        /// Aggregated bonus values keyed by bonus type.
        /// </summary>
        public IReadOnlyDictionary<ToolBonusType, float> Values => values;

        /// <summary>
        /// Adds a contribution to the aggregate.
        /// </summary>
        public void Add(ToolBonusType type, float value)
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
        public float GetValue(ToolBonusType type)
        {
            return values.TryGetValue(type, out var value) ? value : 0f;
        }

        /// <summary>
        /// Gets a flat bonus value, or zero if none exists.
        /// </summary>
        public float GetFlat(ToolBonusType type)
        {
            return GetValue(type);
        }

        /// <summary>
        /// Gets an additive multiplier bonus value, or zero if none exists.
        /// </summary>
        public float GetMultiplier(ToolBonusType type)
        {
            return GetValue(type);
        }
    }
}
