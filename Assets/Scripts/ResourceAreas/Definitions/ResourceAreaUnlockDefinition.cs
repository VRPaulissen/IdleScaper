using System;
using System.Collections.Generic;
using ResourceAreas.Runtime;
using UnityEngine;

namespace ResourceAreas.Definitions
{
    /// <summary>
    /// Data-only definition for content unlocked by resource area progress.
    /// </summary>
    [Serializable]
    public sealed class ResourceAreaUnlockDefinition
    {
        [SerializeField, Min(1)] private int requiredLevel = 1;
        [SerializeField] private string displayName;
        [SerializeField] private Sprite icon;
        [SerializeField] private ResourceAreaUnlockType unlockType;
        [SerializeField] private ResourceDefinition resource;
        [SerializeField] private string unlockKey;
        [SerializeField] private List<ResourceBonusEffectDefinition> effects = new List<ResourceBonusEffectDefinition>();

        /// <summary>
        /// Required resource area level for this unlock.
        /// </summary>
        public int RequiredLevel => Math.Max(1, requiredLevel);

        /// <summary>
        /// Name shown in UI for this unlock.
        /// </summary>
        public string DisplayName => displayName;

        /// <summary>
        /// Icon shown in UI for this unlock.
        /// </summary>
        public Sprite Icon => icon;

        /// <summary>
        /// Category of unlocked content.
        /// </summary>
        public ResourceAreaUnlockType UnlockType => unlockType;

        /// <summary>
        /// Optional resource definition associated with this unlock.
        /// </summary>
        public ResourceDefinition Resource => resource;

        /// <summary>
        /// Optional resource id associated with this unlock.
        /// </summary>
        public ResourceId ResourceId => resource != null ? resource.Id : default;

        /// <summary>
        /// Optional stable key associated with this unlock.
        /// </summary>
        public string UnlockKey => unlockKey;

        /// <summary>
        /// Bonus effects applied when this unlock is completed.
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
