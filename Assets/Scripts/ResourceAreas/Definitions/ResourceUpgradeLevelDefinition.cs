using System;
using System.Collections.Generic;
using UnityEngine;

namespace ResourceAreas.Definitions
{
    /// <summary>
    /// Data-only placeholder definition for a future resource upgrade level.
    /// </summary>
    [Serializable]
    public sealed class ResourceUpgradeLevelDefinition
    {
        [SerializeField, Min(1)] private int requiredLevel = 1;
        [SerializeField] private string displayName;
        [SerializeField] private string description;
        [SerializeField] private Sprite icon;
        [SerializeField] private string unlockKey;
        [SerializeField] private List<ResourceBonusEffectDefinition> effects = new List<ResourceBonusEffectDefinition>();

        /// <summary>
        /// Required resource level for this upgrade level.
        /// </summary>
        public int RequiredLevel => Math.Max(1, requiredLevel);

        /// <summary>
        /// Name shown in UI for this upgrade level.
        /// </summary>
        public string DisplayName => displayName;

        /// <summary>
        /// Description shown in UI for this upgrade level.
        /// </summary>
        public string Description => description;

        /// <summary>
        /// Icon shown in UI for this upgrade level.
        /// </summary>
        public Sprite Icon => icon;

        /// <summary>
        /// Optional stable unlock key associated with this upgrade level.
        /// </summary>
        public string UnlockKey => unlockKey;

        /// <summary>
        /// Bonus effects applied when this upgrade level is completed.
        /// </summary>
        public IReadOnlyList<ResourceBonusEffectDefinition> Effects => effects;

        /// <summary>
        /// Clamps this definition to valid serialized values.
        /// </summary>
        public void Normalize()
        {
            requiredLevel = Math.Max(1, requiredLevel);
            effects ??= new List<ResourceBonusEffectDefinition>();
        }
    }
}
