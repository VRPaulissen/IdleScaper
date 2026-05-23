using System.Collections.Generic;
using ResourceAreas.Runtime;
using Tools.Definitions;
using Tools.Runtime;
using UnityEngine;

namespace ResourceAreas.Definitions
{
    /// <summary>
    /// Static content definition for a resource area such as a mine, forest, or fishing spot.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Resources/Resource Area Definition", fileName = "ResourceArea_")]
    public sealed class ResourceAreaDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private ResourceAreaId id;
        [SerializeField] private string displayName;

        [Header("Presentation")]
        [SerializeField] private Sprite icon;
        [SerializeField] private Sprite banner;

        [Header("Area")]
        [SerializeField] private ResourceAreaType areaType;
        [SerializeField] private ToolDefinition requiredTool;
        [SerializeField, Min(1)] private int maxLevel = 1;

        [Header("Content")]
        [SerializeField] private List<ResourceAreaUnlockDefinition> unlocks = new List<ResourceAreaUnlockDefinition>();
        [SerializeField] private List<ResourceSpawnDefinition> resourceSpawns = new List<ResourceSpawnDefinition>();

        /// <summary>
        /// Stable resource area id used by runtime state and lookups.
        /// </summary>
        public ResourceAreaId Id => id;

        /// <summary>
        /// Name shown in UI.
        /// </summary>
        public string DisplayName => displayName;

        /// <summary>
        /// Icon used for UI rendering.
        /// </summary>
        public Sprite Icon => icon;

        /// <summary>
        /// Banner used for UI rendering.
        /// </summary>
        public Sprite Banner => banner;

        /// <summary>
        /// Resource area category.
        /// </summary>
        public ResourceAreaType AreaType => areaType;

        /// <summary>
        /// Tool definition required to interact with this area.
        /// </summary>
        public ToolDefinition RequiredTool => requiredTool;

        /// <summary>
        /// Stable id of the tool required to interact with this area.
        /// </summary>
        public ToolId RequiredToolId => requiredTool != null ? requiredTool.Id : default;

        /// <summary>
        /// Maximum progression level for this area.
        /// </summary>
        public int MaxLevel => maxLevel;

        /// <summary>
        /// Content unlocked by progressing this area.
        /// </summary>
        public IReadOnlyList<ResourceAreaUnlockDefinition> Unlocks => unlocks;

        /// <summary>
        /// Resource spawn definitions available in this area.
        /// </summary>
        public IReadOnlyList<ResourceSpawnDefinition> ResourceSpawns => resourceSpawns;

        private void OnValidate()
        {
            if (!id.IsValid)
                id = new ResourceAreaId(name);

            maxLevel = Mathf.Max(1, maxLevel);
            NormalizeUnlocks();
            NormalizeResourceSpawns();
        }

        private void NormalizeUnlocks()
        {
            if (unlocks == null)
                return;

            for (var i = 0; i < unlocks.Count; i++)
                unlocks[i]?.Normalize();
        }

        private void NormalizeResourceSpawns()
        {
            if (resourceSpawns == null)
                return;

            for (var i = 0; i < resourceSpawns.Count; i++)
                resourceSpawns[i]?.Normalize();
        }
    }
}
