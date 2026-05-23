using System.Collections.Generic;
using ResourceAreas.Runtime;
using UnityEngine;

namespace ResourceAreas.Definitions
{
    /// <summary>
    /// Static definition for a global resource boost.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Resource Areas/Global Boost", fileName = "GlobalBoost_")]
    public sealed class GlobalBoostDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private GlobalBoostId id;
        [SerializeField] private string displayName;
        [SerializeField] private string description;

        [Header("Presentation")]
        [SerializeField] private Sprite icon;

        [Header("Effects")]
        [SerializeField] private List<ResourceBonusEffectDefinition> effects = new List<ResourceBonusEffectDefinition>();

        /// <summary>
        /// Stable global boost id used by runtime state and lookups.
        /// </summary>
        public GlobalBoostId Id => id;

        /// <summary>
        /// Name shown in UI.
        /// </summary>
        public string DisplayName => displayName;

        /// <summary>
        /// Description shown in UI.
        /// </summary>
        public string Description => description;

        /// <summary>
        /// Icon used for UI rendering.
        /// </summary>
        public Sprite Icon => icon;

        /// <summary>
        /// Bonus effects applied while this global boost is active.
        /// </summary>
        public IReadOnlyList<ResourceBonusEffectDefinition> Effects => effects;

        private void OnValidate()
        {
            if (!id.IsValid)
                id = new GlobalBoostId(name);

            effects ??= new List<ResourceBonusEffectDefinition>();
        }
    }
}
