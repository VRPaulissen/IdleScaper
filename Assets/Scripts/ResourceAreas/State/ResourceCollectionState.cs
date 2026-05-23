using System;
using System.Collections.Generic;
using ResourceAreas.Definitions;
using ResourceAreas.Runtime;
using UnityEngine;

namespace ResourceAreas.State
{
    /// <summary>
    /// Serializable runtime state for all player resource area and resource progress.
    /// </summary>
    [Serializable]
    public sealed class ResourceCollectionState
    {
        [SerializeField] private List<ResourceAreaProgressState> areaProgress = new List<ResourceAreaProgressState>();
        [SerializeField] private List<ResourceProgressState> resourceProgress = new List<ResourceProgressState>();

        /// <summary>
        /// Resource area progress states owned by the player.
        /// </summary>
        public List<ResourceAreaProgressState> AreaProgress => areaProgress;

        /// <summary>
        /// Resource progress states owned by the player.
        /// </summary>
        public List<ResourceProgressState> ResourceProgress => resourceProgress;

        /// <summary>
        /// Normalizes all resource progress after loading save data.
        /// </summary>
        public void Normalize(ResourceAreaCatalog areaCatalog = null, ResourceCatalog resourceCatalog = null)
        {
            areaProgress ??= new List<ResourceAreaProgressState>();
            resourceProgress ??= new List<ResourceProgressState>();

            NormalizeAreaProgress();
            NormalizeResourceProgress();
            EnsureCatalogAreas(areaCatalog);
            EnsureCatalogResources(resourceCatalog);
        }

        /// <summary>
        /// Gets existing resource area progress or creates it when missing.
        /// </summary>
        public ResourceAreaProgressState GetOrCreateAreaProgress(ResourceAreaId areaId)
        {
            if (!areaId.IsValid)
                return null;

            areaProgress ??= new List<ResourceAreaProgressState>();

            var existing = GetAreaProgress(areaId);
            if (existing != null)
                return existing;

            var progress = new ResourceAreaProgressState(areaId);
            areaProgress.Add(progress);
            return progress;
        }

        /// <summary>
        /// Gets resource area progress by id.
        /// </summary>
        public ResourceAreaProgressState GetAreaProgress(ResourceAreaId areaId)
        {
            if (!areaId.IsValid || areaProgress == null)
                return null;

            for (var i = 0; i < areaProgress.Count; i++)
            {
                var progress = areaProgress[i];
                if (progress == null)
                    continue;

                if (progress.AreaId == areaId)
                    return progress;
            }

            return null;
        }

        /// <summary>
        /// Gets existing resource progress or creates it when missing.
        /// </summary>
        public ResourceProgressState GetOrCreateResourceProgress(ResourceId resourceId)
        {
            if (!resourceId.IsValid)
                return null;

            resourceProgress ??= new List<ResourceProgressState>();

            var existing = GetResourceProgress(resourceId);
            if (existing != null)
                return existing;

            var progress = new ResourceProgressState(resourceId);
            resourceProgress.Add(progress);
            return progress;
        }

        /// <summary>
        /// Gets resource progress by id.
        /// </summary>
        public ResourceProgressState GetResourceProgress(ResourceId resourceId)
        {
            if (!resourceId.IsValid || resourceProgress == null)
                return null;

            for (var i = 0; i < resourceProgress.Count; i++)
            {
                var progress = resourceProgress[i];
                if (progress == null)
                    continue;

                if (progress.ResourceId == resourceId)
                    return progress;
            }

            return null;
        }

        private void NormalizeAreaProgress()
        {
            for (var i = areaProgress.Count - 1; i >= 0; i--)
            {
                var progress = areaProgress[i];
                if (progress == null || !progress.AreaId.IsValid || HasEarlierAreaProgress(progress.AreaId, i))
                {
                    areaProgress.RemoveAt(i);
                    continue;
                }

                progress.Normalize();
            }
        }

        private void NormalizeResourceProgress()
        {
            for (var i = resourceProgress.Count - 1; i >= 0; i--)
            {
                var progress = resourceProgress[i];
                if (progress == null || !progress.ResourceId.IsValid || HasEarlierResourceProgress(progress.ResourceId, i))
                {
                    resourceProgress.RemoveAt(i);
                    continue;
                }

                progress.Normalize();
            }
        }

        private bool HasEarlierAreaProgress(ResourceAreaId areaId, int beforeIndex)
        {
            for (var i = 0; i < beforeIndex; i++)
            {
                var progress = areaProgress[i];
                if (progress == null)
                    continue;

                if (progress.AreaId == areaId)
                    return true;
            }

            return false;
        }

        private bool HasEarlierResourceProgress(ResourceId resourceId, int beforeIndex)
        {
            for (var i = 0; i < beforeIndex; i++)
            {
                var progress = resourceProgress[i];
                if (progress == null)
                    continue;

                if (progress.ResourceId == resourceId)
                    return true;
            }

            return false;
        }

        private void EnsureCatalogAreas(ResourceAreaCatalog areaCatalog)
        {
            if (areaCatalog == null || areaCatalog.Areas == null)
                return;

            var areas = areaCatalog.Areas;
            for (var i = 0; i < areas.Count; i++)
            {
                var definition = areas[i];
                if (definition == null || !definition.Id.IsValid)
                    continue;

                GetOrCreateAreaProgress(definition.Id).Normalize();
            }
        }

        private void EnsureCatalogResources(ResourceCatalog resourceCatalog)
        {
            if (resourceCatalog == null || resourceCatalog.Resources == null)
                return;

            var resources = resourceCatalog.Resources;
            for (var i = 0; i < resources.Count; i++)
            {
                var definition = resources[i];
                if (definition == null || !definition.Id.IsValid)
                    continue;

                GetOrCreateResourceProgress(definition.Id).Normalize();
            }
        }
    }
}
