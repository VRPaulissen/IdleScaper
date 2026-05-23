using System;
using ResourceAreas.Definitions;
using ResourceAreas.Runtime;
using ResourceAreas.State;

namespace ResourceAreas.Services
{
    /// <summary>
    /// Provides bonuses unlocked by resource area progression.
    /// </summary>
    public sealed class ResourceAreaBonusProvider : IResourceBonusProvider
    {
        private readonly ResourceCollectionState state;
        private readonly ResourceAreaCatalog areaCatalog;

        /// <summary>
        /// Creates a resource area bonus provider.
        /// </summary>
        public ResourceAreaBonusProvider(ResourceCollectionState state, ResourceAreaCatalog areaCatalog)
        {
            this.state = state ?? throw new ArgumentNullException(nameof(state));
            this.areaCatalog = areaCatalog ?? throw new ArgumentNullException(nameof(areaCatalog));
        }

        /// <inheritdoc />
        public void AddBonuses(ResourceBonusContext context, ResourceBonusCollection bonuses)
        {
            if (bonuses == null)
                return;

            if (!context.AreaId.IsValid)
                return;

            if (!areaCatalog.TryGet(context.AreaId, out var area) || area == null)
                return;

            var areaLevel = GetAreaLevel(context.AreaId);
            var unlocks = area.Unlocks;
            for (var i = 0; i < unlocks.Count; i++)
            {
                var unlock = unlocks[i];
                if (unlock == null)
                    continue;

                if (unlock.RequiredLevel > areaLevel)
                    continue;

                AddUnlockBonuses(context, unlock, bonuses);
            }
        }

        private int GetAreaLevel(ResourceAreaId areaId)
        {
            var progress = state.GetAreaProgress(areaId);
            return progress != null ? Math.Max(1, progress.Level) : 1;
        }

        private static void AddUnlockBonuses(ResourceBonusContext context, ResourceAreaUnlockDefinition unlock, ResourceBonusCollection bonuses)
        {
            var effects = unlock.Effects;
            if (effects == null)
                return;

            var source = CreateSource(context, unlock);
            for (var i = 0; i < effects.Count; i++)
            {
                var effect = effects[i];
                if (effect == null)
                    continue;

                effect.AddBonuses(context, bonuses, source);
            }
        }

        private static ResourceBonusEffectSource CreateSource(ResourceBonusContext context, ResourceAreaUnlockDefinition unlock)
        {
            var sourceId = unlock.UnlockKey;
            if (string.IsNullOrWhiteSpace(sourceId))
                sourceId = context.AreaId.Value + ".level." + unlock.RequiredLevel;

            return new ResourceBonusEffectSource(
                unlock.DisplayName,
                ResourceBonusSourceType.ResourceArea,
                sourceId);
        }
    }
}
