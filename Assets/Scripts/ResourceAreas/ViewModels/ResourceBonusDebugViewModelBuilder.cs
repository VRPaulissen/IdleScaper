using System;
using System.Collections.Generic;
using ResourceAreas.Definitions;
using ResourceAreas.Runtime;
using ResourceAreas.Services;
using ResourceAreas.State;
using Tools.Definitions;
using Tools.Runtime;

namespace ResourceAreas.ViewModels
{
    /// <summary>
    /// Builds resource bonus debug view models by resolving bonuses through a bonus resolver.
    /// </summary>
    public sealed class ResourceBonusDebugViewModelBuilder
    {
        private readonly ResourceCollectionState state;
        private readonly ResourceAreaCatalog areaCatalog;
        private readonly ResourceCatalog resourceCatalog;
        private readonly ResourceBonusResolver bonusResolver;
        private readonly ToolDefinitionCatalog toolDefinitionCatalog;
        private readonly ResourceBonusCollection bonusCollection = new ResourceBonusCollection();

        /// <summary>
        /// Creates a resource bonus debug view model builder.
        /// </summary>
        public ResourceBonusDebugViewModelBuilder(
            ResourceCollectionState state,
            ResourceAreaCatalog areaCatalog,
            ResourceCatalog resourceCatalog,
            ResourceBonusResolver bonusResolver,
            ToolDefinitionCatalog toolDefinitionCatalog = null)
        {
            this.state = state ?? throw new ArgumentNullException(nameof(state));
            this.areaCatalog = areaCatalog ?? throw new ArgumentNullException(nameof(areaCatalog));
            this.resourceCatalog = resourceCatalog ?? throw new ArgumentNullException(nameof(resourceCatalog));
            this.bonusResolver = bonusResolver;
            this.toolDefinitionCatalog = toolDefinitionCatalog;
        }

        /// <summary>
        /// Builds a debug read model for the requested resource bonus context.
        /// </summary>
        public ResourceBonusDebugViewModel Build(ResourceAreaId areaId, ResourceId resourceId, ToolId toolId)
        {
            if (bonusResolver == null)
                return CreateInvalid(areaId, resourceId, toolId, "Bonus resolver not available.");

            if (!areaId.IsValid)
                return CreateInvalid(areaId, resourceId, toolId, "Resource area not found.");

            if (!areaCatalog.TryGet(areaId, out var areaDefinition) || areaDefinition == null)
                return CreateInvalid(areaId, resourceId, toolId, "Resource area not found.");

            if (!resourceId.IsValid)
                return CreateInvalid(areaId, resourceId, toolId, "Resource not found.");

            if (!resourceCatalog.TryGet(resourceId, out var resourceDefinition) || resourceDefinition == null)
                return CreateInvalid(areaId, resourceId, toolId, "Resource not found.");

            var areaProgress = state.GetOrCreateAreaProgress(areaId);
            var resourceProgress = state.GetOrCreateResourceProgress(resourceId);
            var viewModel = CreateValid(areaDefinition, areaProgress, resourceDefinition, resourceProgress, toolId);
            ResolveBonuses(viewModel, areaId, resourceId, toolId);
            return viewModel;
        }

        private ResourceBonusDebugViewModel CreateValid(
            ResourceAreaDefinition areaDefinition,
            ResourceAreaProgressState areaProgress,
            ResourceDefinition resourceDefinition,
            ResourceProgressState resourceProgress,
            ToolId toolId)
        {
            return new ResourceBonusDebugViewModel(
                areaDefinition.Id,
                areaDefinition.DisplayName,
                GetAreaLevel(areaProgress),
                resourceDefinition.Id,
                resourceDefinition.DisplayName,
                GetResourceLevel(resourceProgress),
                toolId,
                GetToolName(toolId),
                true,
                string.Empty);
        }

        private ResourceBonusDebugViewModel CreateInvalid(ResourceAreaId areaId, ResourceId resourceId, ToolId toolId, string failureText)
        {
            return new ResourceBonusDebugViewModel(
                areaId,
                string.Empty,
                0,
                resourceId,
                string.Empty,
                0,
                toolId,
                GetToolName(toolId),
                false,
                failureText);
        }

        private void ResolveBonuses(ResourceBonusDebugViewModel viewModel, ResourceAreaId areaId, ResourceId resourceId, ToolId toolId)
        {
            bonusResolver.Resolve(new ResourceBonusContext(areaId, resourceId, toolId), bonusCollection);
            AddContributionRows(viewModel, bonusCollection.Contributions);
            AddTotalRows(viewModel, bonusCollection.Totals);
            AddGroups(viewModel);
        }

        private static void AddContributionRows(ResourceBonusDebugViewModel viewModel, IReadOnlyList<ResourceBonusContribution> contributions)
        {
            for (var i = 0; i < contributions.Count; i++)
            {
                var contribution = contributions[i];
                viewModel.AddContribution(new ResourceBonusDebugRow(
                    contribution.Type,
                    contribution.Value,
                    FormatBonusValue(contribution.Type, contribution.Value),
                    contribution.SourceType,
                    contribution.SourceName,
                    contribution.SourceId));
            }
        }

        private static void AddTotalRows(ResourceBonusDebugViewModel viewModel, IReadOnlyDictionary<ResourceBonusType, float> totals)
        {
            foreach (var total in totals)
            {
                viewModel.AddTotal(new ResourceBonusDebugTotalRow(
                    total.Key,
                    total.Value,
                    FormatBonusValue(total.Key, total.Value)));
            }
        }

        private static void AddGroups(ResourceBonusDebugViewModel viewModel)
        {
            AddGroupIfPresent(viewModel, ResourceBonusSourceType.Global, "Global");
            AddGroupIfPresent(viewModel, ResourceBonusSourceType.ResourceArea, "Resource Area");
            AddGroupIfPresent(viewModel, ResourceBonusSourceType.Resource, "Resource");
            AddGroupIfPresent(viewModel, ResourceBonusSourceType.Tool, "Tool");
            AddGroupIfPresent(viewModel, ResourceBonusSourceType.Equipment, "Equipment");
            AddGroupIfPresent(viewModel, ResourceBonusSourceType.Temporary, "Temporary");
            AddGroupIfPresent(viewModel, ResourceBonusSourceType.Event, "Event");
        }

        private static void AddGroupIfPresent(ResourceBonusDebugViewModel viewModel, ResourceBonusSourceType sourceType, string displayName)
        {
            ResourceBonusDebugGroup group = null;
            var contributions = viewModel.Contributions;
            for (var i = 0; i < contributions.Count; i++)
            {
                var row = contributions[i];
                if (row.SourceType != sourceType)
                    continue;

                group ??= new ResourceBonusDebugGroup(sourceType, displayName);
                group.Add(row);
            }

            if (group != null)
                viewModel.AddGroup(group);
        }

        private static string FormatBonusValue(ResourceBonusType type, float value)
        {
            if (IsPercentType(type))
                return FormatSignedValue(value * 100f, "%");

            return FormatSignedValue(value, string.Empty);
        }

        private static string FormatSignedValue(float value, string suffix)
        {
            return value >= 0f ? "+" + value.ToString("0.##") + suffix : value.ToString("0.##") + suffix;
        }

        private static bool IsPercentType(ResourceBonusType type)
        {
            switch (type)
            {
                case ResourceBonusType.ResourceYieldMultiplier:
                case ResourceBonusType.ResourceXpMultiplier:
                case ResourceBonusType.ResourceAreaXpMultiplier:
                case ResourceBonusType.MiningDamageMultiplier:
                case ResourceBonusType.RespawnTimeMultiplier:
                case ResourceBonusType.RareDropChanceFlat:
                case ResourceBonusType.GemDropChanceFlat:
                case ResourceBonusType.UniqueDropChanceFlat:
                case ResourceBonusType.SpecificDropChanceFlat:
                case ResourceBonusType.ResourceSellValueMultiplier:
                    return true;
                default:
                    return false;
            }
        }

        private static int GetAreaLevel(ResourceAreaProgressState progress)
        {
            if (progress == null)
                return 1;

            return Math.Max(1, progress.Level);
        }

        private static int GetResourceLevel(ResourceProgressState progress)
        {
            if (progress == null)
                return 1;

            return Math.Max(1, progress.Level);
        }

        private string GetToolName(ToolId toolId)
        {
            if (!toolId.IsValid)
                return string.Empty;

            if (toolDefinitionCatalog != null && toolDefinitionCatalog.TryGet(toolId, out var definition) && definition != null)
                return definition.DisplayName;

            return toolId.ToString();
        }
    }
}
