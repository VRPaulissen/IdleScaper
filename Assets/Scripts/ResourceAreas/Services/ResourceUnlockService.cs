using System;
using System.Collections.Generic;
using ResourceAreas.Definitions;
using ResourceAreas.Runtime;
using ResourceAreas.State;

namespace ResourceAreas.Services
{
    /// <summary>
    /// Read-only service for resource area unlock checks.
    /// </summary>
    public sealed class ResourceUnlockService
    {
        private readonly ResourceCollectionState state;
        private readonly ResourceAreaCatalog areaCatalog;

        /// <summary>
        /// Creates a resource unlock service.
        /// </summary>
        public ResourceUnlockService(ResourceCollectionState state, ResourceAreaCatalog areaCatalog)
        {
            this.state = state ?? throw new ArgumentNullException(nameof(state));
            this.areaCatalog = areaCatalog ?? throw new ArgumentNullException(nameof(areaCatalog));
        }

        /// <summary>
        /// Returns true when the resource is unlocked in the given resource area.
        /// </summary>
        public bool IsResourceUnlocked(ResourceAreaId areaId, ResourceId resourceId)
        {
            if (!areaId.IsValid || !resourceId.IsValid)
                return false;

            if (!areaCatalog.TryGet(areaId, out var areaDefinition) || areaDefinition == null)
                return false;

            var level = GetAreaLevel(areaId);
            var unlocks = areaDefinition.Unlocks;
            foreach (var unlock in unlocks)
            {
                if (unlock == null)
                    continue;

                if (unlock.UnlockType != ResourceAreaUnlockType.Resource)
                    continue;

                if (unlock.RequiredLevel > level)
                    continue;

                if (unlock.ResourceId == resourceId)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true when the area has reached the required unlock level.
        /// </summary>
        public bool IsAreaUnlockCompleted(ResourceAreaId areaId, int requiredLevel)
        {
            if (!areaId.IsValid || requiredLevel < 1)
                return false;

            if (!areaCatalog.TryGet(areaId, out var areaDefinition) || areaDefinition == null)
                return false;

            return GetAreaLevel(areaId) >= requiredLevel;
        }

        /// <summary>
        /// Fills results with unlocks completed in the given resource area.
        /// </summary>
        public void GetCompletedUnlocks(ResourceAreaId areaId, List<ResourceAreaUnlockDefinition> results)
        {
            if (results == null)
                throw new ArgumentNullException(nameof(results));

            results.Clear();

            if (!areaId.IsValid)
                return;

            if (!areaCatalog.TryGet(areaId, out var areaDefinition) || areaDefinition == null)
                return;

            var level = GetAreaLevel(areaId);
            var unlocks = areaDefinition.Unlocks;
            for (var i = 0; i < unlocks.Count; i++)
            {
                var unlock = unlocks[i];
                if (unlock == null)
                    continue;

                if (unlock.RequiredLevel <= level)
                    results.Add(unlock);
            }
        }

        /// <summary>
        /// Fills results with resource spawns available in the given resource area.
        /// </summary>
        public void GetUnlockedResourceSpawns(ResourceAreaId areaId, List<ResourceSpawnDefinition> results)
        {
            if (results == null)
                throw new ArgumentNullException(nameof(results));

            results.Clear();

            if (!areaId.IsValid)
                return;

            if (!areaCatalog.TryGet(areaId, out var areaDefinition) || areaDefinition == null)
                return;

            var level = GetAreaLevel(areaId);
            var resourceSpawns = areaDefinition.ResourceSpawns;
            for (var i = 0; i < resourceSpawns.Count; i++)
            {
                var spawn = resourceSpawns[i];
                if (spawn == null)
                    continue;

                if (spawn.RequiredAreaLevel > level)
                    continue;

                if (spawn.Resource == null || !spawn.Resource.Id.IsValid)
                    continue;

                if (IsResourceUnlocked(areaId, spawn.Resource.Id))
                    results.Add(spawn);
            }
        }

        private int GetAreaLevel(ResourceAreaId areaId)
        {
            var progress = state.GetOrCreateAreaProgress(areaId);
            if (progress == null)
                return 1;

            progress.Normalize();
            return progress.Level;
        }
    }
}
