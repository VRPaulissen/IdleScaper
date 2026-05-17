using System;
using UnityEngine;

namespace Tools.Runtime
{
    /// <summary>
    /// Serializable definition for one bonus contributed by an installed permanent tool part.
    /// </summary>
    [Serializable]
    public struct ToolBonusValue
    {
        [SerializeField] private ToolBonusType type;
        [SerializeField] private float baseValue;
        [SerializeField] private float valuePerLevel;

        /// <summary>
        /// Bonus category this value contributes to.
        /// </summary>
        public ToolBonusType Type => type;

        /// <summary>
        /// Contribution at level zero.
        /// </summary>
        public float BaseValue => baseValue;

        /// <summary>
        /// Additional contribution per installed part level.
        /// </summary>
        public float ValuePerLevel => valuePerLevel;

        /// <summary>
        /// Creates a bonus value definition.
        /// </summary>
        public ToolBonusValue(ToolBonusType type, float baseValue, float valuePerLevel)
        {
            this.type = type;
            this.baseValue = baseValue;
            this.valuePerLevel = valuePerLevel;
        }

        /// <summary>
        /// Calculates the bonus contribution for the given part level.
        /// </summary>
        public float Evaluate(int level)
        {
            return baseValue + Mathf.Max(0, level) * valuePerLevel;
        }
    }
}
