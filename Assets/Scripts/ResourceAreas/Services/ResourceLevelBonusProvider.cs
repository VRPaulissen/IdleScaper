using System;
using ResourceAreas.Definitions;
using ResourceAreas.Runtime;
using ResourceAreas.State;

namespace ResourceAreas.Services
{
    /// <summary>
    /// Provides bonuses unlocked by resource-specific progression.
    /// </summary>
    public sealed class ResourceLevelBonusProvider : IResourceBonusProvider
    {
        private readonly ResourceCollectionState state;
        private readonly ResourceCatalog resourceCatalog;

        /// <summary>
        /// Creates a resource level bonus provider.
        /// </summary>
        public ResourceLevelBonusProvider(ResourceCollectionState state, ResourceCatalog resourceCatalog)
        {
            this.state = state ?? throw new ArgumentNullException(nameof(state));
            this.resourceCatalog = resourceCatalog ?? throw new ArgumentNullException(nameof(resourceCatalog));
        }

        /// <inheritdoc />
        public void AddBonuses(ResourceBonusContext context, ResourceBonusCollection bonuses)
        {
            if (bonuses == null)
                return;

            if (!context.ResourceId.IsValid)
                return;

            if (!resourceCatalog.TryGet(context.ResourceId, out var resource) || resource == null)
                return;

            var progress = state.GetResourceProgress(context.ResourceId);
            if (progress == null)
                return;

            AddCompletedUpgradeBonuses(context, resource, Math.Max(1, progress.Level), bonuses);
        }

        private static void AddCompletedUpgradeBonuses(
            ResourceBonusContext context,
            ResourceDefinition resource,
            int resourceLevel,
            ResourceBonusCollection bonuses)
        {
            var upgrades = resource.UpgradeLevels;
            for (var i = 0; i < upgrades.Count; i++)
            {
                var upgrade = upgrades[i];
                if (upgrade == null)
                    continue;

                if (upgrade.RequiredLevel > resourceLevel)
                    continue;

                AddUpgradeBonuses(context, resource, upgrade, bonuses);
            }
        }

        private static void AddUpgradeBonuses(
            ResourceBonusContext context,
            ResourceDefinition resource,
            ResourceUpgradeLevelDefinition upgrade,
            ResourceBonusCollection bonuses)
        {
            var effects = upgrade.Effects;
            if (effects == null)
                return;

            var source = CreateSource(resource, upgrade);
            for (var i = 0; i < effects.Count; i++)
            {
                var effect = effects[i];
                if (effect == null)
                    continue;

                effect.AddBonuses(context, bonuses, source);
            }
        }

        private static ResourceBonusEffectSource CreateSource(ResourceDefinition resource, ResourceUpgradeLevelDefinition upgrade)
        {
            var sourceName = resource.DisplayName + " Lv. " + upgrade.RequiredLevel;
            if (!string.IsNullOrWhiteSpace(upgrade.DisplayName))
                sourceName = upgrade.DisplayName;

            return new ResourceBonusEffectSource(
                sourceName,
                ResourceBonusSourceType.Resource,
                resource.Id.Value + ".level." + upgrade.RequiredLevel);
        }
    }
}
