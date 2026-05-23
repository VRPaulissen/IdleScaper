using System.Collections.Generic;
using ResourceAreas.Runtime;
using UnityEngine;

namespace ResourceAreas.Definitions
{
    /// <summary>
    /// Static content definition for a harvestable resource.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Resources/Resource Definition", fileName = "Resource_")]
    public sealed class ResourceDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private ResourceId id;
        [SerializeField] private string displayName;

        [Header("Presentation")]
        [SerializeField] private Sprite icon;

        [Header("Resource")]
        [SerializeField] private ResourceType resourceType;
        [SerializeField, Min(1f)] private float baseHealth = 1f;
        [SerializeField, Min(1)] private int baseYield = 1;
        [SerializeField, Min(0f)] private float baseResourceXp;
        [SerializeField, Min(0f)] private float baseAreaXp;
        [SerializeField, Min(0f)] private float respawnSeconds;
        [SerializeField, Min(1)] private int maxLevel = 1;

        [Header("Drops")]
        [SerializeField] private List<ResourceDropDefinition> dropTable = new List<ResourceDropDefinition>();

        [Header("Upgrades")]
        [SerializeField] private List<ResourceUpgradeLevelDefinition> upgradeLevels = new List<ResourceUpgradeLevelDefinition>();

        /// <summary>
        /// Stable resource id used by runtime state and lookups.
        /// </summary>
        public ResourceId Id => id;

        /// <summary>
        /// Name shown in UI.
        /// </summary>
        public string DisplayName => displayName;

        /// <summary>
        /// Icon used for UI rendering.
        /// </summary>
        public Sprite Icon => icon;

        /// <summary>
        /// Resource category.
        /// </summary>
        public ResourceType ResourceType => resourceType;

        /// <summary>
        /// Base health before future scaling and bonuses.
        /// </summary>
        public float BaseHealth => baseHealth;

        /// <summary>
        /// Base quantity produced by the resource.
        /// </summary>
        public int BaseYield => baseYield;

        /// <summary>
        /// Base resource-specific experience granted by the resource.
        /// </summary>
        public float BaseResourceXp => baseResourceXp;

        /// <summary>
        /// Base area experience granted by the resource.
        /// </summary>
        public float BaseAreaXp => baseAreaXp;

        /// <summary>
        /// Base respawn duration in seconds.
        /// </summary>
        public float RespawnSeconds => respawnSeconds;

        /// <summary>
        /// Maximum progression level for this resource.
        /// </summary>
        public int MaxLevel => maxLevel;

        /// <summary>
        /// Item drops defined for this resource.
        /// </summary>
        public IReadOnlyList<ResourceDropDefinition> DropTable => dropTable;

        /// <summary>
        /// Placeholder upgrade levels defined for this resource.
        /// </summary>
        public IReadOnlyList<ResourceUpgradeLevelDefinition> UpgradeLevels => upgradeLevels;

        private void OnValidate()
        {
            if (!id.IsValid)
                id = new ResourceId(name);

            baseHealth = Mathf.Max(1f, baseHealth);
            baseYield = Mathf.Max(1, baseYield);
            baseResourceXp = Mathf.Max(0f, baseResourceXp);
            baseAreaXp = Mathf.Max(0f, baseAreaXp);
            respawnSeconds = Mathf.Max(0f, respawnSeconds);
            maxLevel = Mathf.Max(1, maxLevel);
            NormalizeDropTable();
            NormalizeUpgradeLevels();
        }

        private void NormalizeDropTable()
        {
            if (dropTable == null)
                return;

            for (var i = 0; i < dropTable.Count; i++)
                dropTable[i]?.Normalize();
        }

        private void NormalizeUpgradeLevels()
        {
            if (upgradeLevels == null)
                return;

            for (var i = 0; i < upgradeLevels.Count; i++)
                upgradeLevels[i]?.Normalize();
        }
    }
}
