using System;
using System.Collections.Generic;
using ResourceAreas.Definitions;
using ResourceAreas.Runtime;
using ResourceAreas.State;

namespace ResourceAreas.Services
{
    /// <summary>
    /// Resolves and rolls resource drops using the resource bonus pipeline.
    /// </summary>
    public sealed class ResourceDropResolver
    {
        private readonly ResourceCatalog resourceCatalog;
        private readonly ResourceBonusResolver bonusResolver;
        private readonly ResourceCollectionState state;
        private readonly IResourceRandom random;
        private readonly ResourceBonusCollection bonusCollection = new ResourceBonusCollection();
        private readonly List<ResolvedResourceDrop> resolvedDrops = new List<ResolvedResourceDrop>(16);

        /// <summary>
        /// Creates a resource drop resolver.
        /// </summary>
        public ResourceDropResolver(
            ResourceCatalog resourceCatalog,
            ResourceBonusResolver bonusResolver,
            ResourceCollectionState state,
            IResourceRandom random = null)
        {
            this.resourceCatalog = resourceCatalog ?? throw new ArgumentNullException(nameof(resourceCatalog));
            this.bonusResolver = bonusResolver ?? throw new ArgumentNullException(nameof(bonusResolver));
            this.state = state ?? throw new ArgumentNullException(nameof(state));
            this.random = random ?? new UnityResourceRandom();
        }

        /// <summary>
        /// Resolves all configured drops for the context resource.
        /// </summary>
        public bool TryResolveDrops(ResourceBonusContext context, List<ResolvedResourceDrop> results)
        {
            if (results == null)
                return false;

            results.Clear();

            if (!context.ResourceId.IsValid)
                return false;

            if (!resourceCatalog.TryGet(context.ResourceId, out var resource) || resource == null)
                return false;

            ResolveBonuses(context);
            ResolveDropTable(resource, results);
            return true;
        }

        /// <summary>
        /// Rolls all currently rollable drops for the context resource.
        /// </summary>
        public bool TryRollDrops(ResourceBonusContext context, List<ResourceDropResult> results)
        {
            if (results == null)
                return false;

            results.Clear();

            if (!TryResolveDrops(context, resolvedDrops))
                return false;

            for (var i = 0; i < resolvedDrops.Count; i++)
            {
                var drop = resolvedDrops[i];
                if (drop == null || !drop.CanRoll)
                    continue;

                if (random.Next01() > drop.FinalChance)
                    continue;

                results.Add(new ResourceDropResult(
                    drop.ItemId,
                    drop.DisplayName,
                    drop.Category,
                    random.RangeInclusive(drop.MinAmount, drop.MaxAmount),
                    drop.FinalChance));
            }

            return true;
        }

        private void ResolveBonuses(ResourceBonusContext context)
        {
            bonusResolver.Resolve(context, bonusCollection);
        }

        private void ResolveDropTable(ResourceDefinition resource, List<ResolvedResourceDrop> results)
        {
            var dropTable = resource.DropTable;
            for (var i = 0; i < dropTable.Count; i++)
            {
                var drop = dropTable[i];
                if (drop == null)
                {
                    results.Add(CreateInvalidDrop());
                    continue;
                }

                results.Add(ResolveDrop(drop));
            }
        }

        private ResolvedResourceDrop ResolveDrop(ResourceDropDefinition drop)
        {
            var explicitUnlock = IsExplicitlyUnlocked(drop.RequiredUnlockKey);
            var bonusChance = GetBonusChance(drop);
            var finalChance = ClampChance(drop.BaseChance + bonusChance);
            return new ResolvedResourceDrop(
                drop.ItemId,
                drop.DisplayNameOverride,
                drop.Category,
                drop.BaseChance,
                bonusChance,
                finalChance,
                drop.MinAmount,
                drop.MaxAmount,
                drop.RequiredUnlockKey,
                explicitUnlock,
                GetFailureReason(drop, finalChance, explicitUnlock));
        }

        private static ResolvedResourceDrop CreateInvalidDrop()
        {
            return new ResolvedResourceDrop(
                default,
                string.Empty,
                ResourceDropCategory.Common,
                0f,
                0f,
                0f,
                0,
                0,
                string.Empty,
                false,
                "Invalid drop.");
        }

        private float GetBonusChance(ResourceDropDefinition drop)
        {
            var bonus = GetCategoryBonus(drop.Category);
            bonus += GetRareBonus(drop.Category);
            bonus += GetSpecificDropBonus(drop);
            return bonus;
        }

        private float GetCategoryBonus(ResourceDropCategory category)
        {
            switch (category)
            {
                case ResourceDropCategory.Gem:
                    return bonusCollection.GetFlat(ResourceBonusType.GemDropChanceFlat);
                case ResourceDropCategory.Unique:
                    return bonusCollection.GetFlat(ResourceBonusType.UniqueDropChanceFlat);
                case ResourceDropCategory.Rare:
                case ResourceDropCategory.Fragment:
                    return bonusCollection.GetFlat(ResourceBonusType.RareDropChanceFlat);
                default:
                    return 0f;
            }
        }

        private float GetRareBonus(ResourceDropCategory category)
        {
            switch (category)
            {
                case ResourceDropCategory.Gem:
                case ResourceDropCategory.Unique:
                case ResourceDropCategory.Fragment:
                    return bonusCollection.GetFlat(ResourceBonusType.RareDropChanceFlat);
                default:
                    return 0f;
            }
        }

        private float GetSpecificDropBonus(ResourceDropDefinition drop)
        {
            var bonus = 0f;
            var contributions = bonusCollection.Contributions;
            for (var i = 0; i < contributions.Count; i++)
            {
                var contribution = contributions[i];
                if (contribution.Type != ResourceBonusType.SpecificDropChanceFlat)
                    continue;

                if (!MatchesSpecificDropTarget(contribution.SourceId, drop))
                    continue;

                bonus += contribution.Value;
            }

            return bonus;
        }

        private static bool MatchesSpecificDropTarget(string sourceId, ResourceDropDefinition drop)
        {
            if (string.IsNullOrEmpty(sourceId))
                return false;

            if (!string.IsNullOrWhiteSpace(drop.RequiredUnlockKey) &&
                sourceId.IndexOf(drop.RequiredUnlockKey, StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            return drop.ItemId.IsValid &&
                   sourceId.IndexOf(drop.ItemId.Value, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private bool IsExplicitlyUnlocked(string requiredUnlockKey)
        {
            if (string.IsNullOrWhiteSpace(requiredUnlockKey))
                return true;

            var contributions = bonusCollection.Contributions;
            for (var i = 0; i < contributions.Count; i++)
            {
                var contribution = contributions[i];
                if (contribution.Type != ResourceBonusType.DropUnlock)
                    continue;

                if (string.Equals(contribution.SourceId, requiredUnlockKey, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static string GetFailureReason(ResourceDropDefinition drop, float finalChance, bool explicitUnlock)
        {
            if (!drop.ItemId.IsValid && string.IsNullOrWhiteSpace(drop.DisplayNameOverride))
                return "Invalid drop.";

            if (!string.IsNullOrWhiteSpace(drop.RequiredUnlockKey) && !explicitUnlock)
                return "Missing explicit unlock.";

            if (finalChance <= 0f)
                return "Chance is zero.";

            if (drop.MinAmount <= 0 || drop.MaxAmount < drop.MinAmount)
                return "Invalid amount range.";

            return string.Empty;
        }

        private static float ClampChance(float chance)
        {
            if (chance < 0f)
                return 0f;

            if (chance > 1f)
                return 1f;

            return chance;
        }
    }
}
