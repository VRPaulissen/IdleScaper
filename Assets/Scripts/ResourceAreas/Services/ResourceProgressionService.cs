using System;
using ResourceAreas.Definitions;
using ResourceAreas.Runtime;
using ResourceAreas.State;

namespace ResourceAreas.Services
{
    /// <summary>
    /// Service that applies resource area and resource experience progression.
    /// </summary>
    public sealed class ResourceProgressionService
    {
        private readonly ResourceCollectionState state;
        private readonly ResourceAreaCatalog areaCatalog;
        private readonly ResourceCatalog resourceCatalog;

        /// <summary>
        /// Raised when resource area experience or level changes.
        /// </summary>
        public event Action<ResourceAreaProgressChangedEventData> AreaProgressChanged;

        /// <summary>
        /// Raised when a resource area gains a level.
        /// </summary>
        public event Action<ResourceAreaLeveledUpEventData> AreaLeveledUp;

        /// <summary>
        /// Raised when resource experience or level changes.
        /// </summary>
        public event Action<ResourceProgressChangedEventData> ResourceProgressChanged;

        /// <summary>
        /// Raised when a resource gains a level.
        /// </summary>
        public event Action<ResourceLeveledUpEventData> ResourceLeveledUp;

        /// <summary>
        /// Creates a resource progression service.
        /// </summary>
        public ResourceProgressionService(
            ResourceCollectionState state,
            ResourceAreaCatalog areaCatalog,
            ResourceCatalog resourceCatalog)
        {
            this.state = state ?? throw new ArgumentNullException(nameof(state));
            this.areaCatalog = areaCatalog ?? throw new ArgumentNullException(nameof(areaCatalog));
            this.resourceCatalog = resourceCatalog ?? throw new ArgumentNullException(nameof(resourceCatalog));
        }

        /// <summary>
        /// Adds experience to the given resource area when possible.
        /// </summary>
        public bool TryAddAreaXp(ResourceAreaId areaId, float amount)
        {
            if (!areaId.IsValid || amount <= 0f)
                return false;

            if (!areaCatalog.TryGet(areaId, out var areaDefinition) || areaDefinition == null)
                return false;

            var progress = state.GetOrCreateAreaProgress(areaId);
            if (progress == null)
                return false;

            return AddAreaXp(progress, areaDefinition, amount);
        }

        /// <summary>
        /// Adds experience to the given resource when possible.
        /// </summary>
        public bool TryAddResourceXp(ResourceId resourceId, float amount)
        {
            if (!resourceId.IsValid || amount <= 0f)
                return false;

            if (!resourceCatalog.TryGet(resourceId, out var resourceDefinition) || resourceDefinition == null)
                return false;

            var progress = state.GetOrCreateResourceProgress(resourceId);
            if (progress == null)
                return false;

            return AddResourceXp(progress, resourceDefinition, amount);
        }

        private bool AddAreaXp(ResourceAreaProgressState progress, ResourceAreaDefinition definition, float amount)
        {
            progress.Normalize();

            var previousLevel = progress.Level;
            var previousXp = progress.Xp;
            var maxLevel = Math.Max(1, definition.MaxLevel);
            var level = Math.Min(progress.Level, maxLevel);
            var xp = progress.Xp;

            if (level >= maxLevel)
            {
                progress.SetProgress(maxLevel, 0f);
                return false;
            }

            xp += amount;
            while (level < maxLevel)
            {
                var requiredXp = GetXpRequiredForNextLevel(level);
                if (requiredXp <= 0f || xp < requiredXp)
                    break;

                xp -= requiredXp;
                var oldLevel = level;
                level++;
                AreaLeveledUp?.Invoke(new ResourceAreaLeveledUpEventData(progress.AreaId, oldLevel, level, level >= maxLevel ? 0f : xp, amount));
            }

            if (level >= maxLevel)
                xp = 0f;

            progress.SetProgress(level, xp);
            AreaProgressChanged?.Invoke(new ResourceAreaProgressChangedEventData(progress.AreaId, previousLevel, progress.Level, previousXp, progress.Xp, amount));
            return true;
        }

        private bool AddResourceXp(ResourceProgressState progress, ResourceDefinition definition, float amount)
        {
            progress.Normalize();

            var previousLevel = progress.Level;
            var previousXp = progress.Xp;
            var maxLevel = Math.Max(1, definition.MaxLevel);
            var level = Math.Min(progress.Level, maxLevel);
            var xp = progress.Xp;

            if (level >= maxLevel)
            {
                progress.SetProgress(maxLevel, 0f);
                return false;
            }

            xp += amount;
            while (level < maxLevel)
            {
                var requiredXp = GetXpRequiredForNextLevel(level);
                if (requiredXp <= 0f || xp < requiredXp)
                    break;

                xp -= requiredXp;
                var oldLevel = level;
                level++;
                ResourceLeveledUp?.Invoke(new ResourceLeveledUpEventData(progress.ResourceId, oldLevel, level, level >= maxLevel ? 0f : xp, amount));
            }

            if (level >= maxLevel)
                xp = 0f;

            progress.SetProgress(level, xp);
            ResourceProgressChanged?.Invoke(new ResourceProgressChangedEventData(progress.ResourceId, previousLevel, progress.Level, previousXp, progress.Xp, amount));
            return true;
        }

        private static float GetXpRequiredForNextLevel(int currentLevel)
        {
            return 100f * Math.Max(1, currentLevel);
        }

    }
}
