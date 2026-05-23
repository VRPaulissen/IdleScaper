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
    /// Builds resource drop debug view models by resolving drops through a drop resolver.
    /// </summary>
    public sealed class ResourceDropDebugViewModelBuilder
    {
        private readonly ResourceCollectionState state;
        private readonly ResourceAreaCatalog areaCatalog;
        private readonly ResourceCatalog resourceCatalog;
        private readonly ResourceDropResolver dropResolver;
        private readonly ToolDefinitionCatalog toolDefinitionCatalog;
        private readonly List<ResolvedResourceDrop> resolvedDrops = new List<ResolvedResourceDrop>(16);

        /// <summary>
        /// Creates a resource drop debug view model builder.
        /// </summary>
        public ResourceDropDebugViewModelBuilder(
            ResourceCollectionState state,
            ResourceAreaCatalog areaCatalog,
            ResourceCatalog resourceCatalog,
            ResourceDropResolver dropResolver,
            ToolDefinitionCatalog toolDefinitionCatalog = null)
        {
            this.state = state ?? throw new ArgumentNullException(nameof(state));
            this.areaCatalog = areaCatalog ?? throw new ArgumentNullException(nameof(areaCatalog));
            this.resourceCatalog = resourceCatalog ?? throw new ArgumentNullException(nameof(resourceCatalog));
            this.dropResolver = dropResolver;
            this.toolDefinitionCatalog = toolDefinitionCatalog;
        }

        /// <summary>
        /// Builds a debug read model for resolved possible drops.
        /// </summary>
        public ResourceDropDebugViewModel Build(ResourceAreaId areaId, ResourceId resourceId, ToolId toolId)
        {
            if (dropResolver == null)
                return CreateInvalid(areaId, resourceId, toolId, "Drop resolver not available.");

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
            if (!ResolveDrops(viewModel, areaId, resourceId, toolId))
                return CreateInvalid(areaId, resourceId, toolId, "Could not resolve drops.");

            return viewModel;
        }

        private bool ResolveDrops(ResourceDropDebugViewModel viewModel, ResourceAreaId areaId, ResourceId resourceId, ToolId toolId)
        {
            if (!dropResolver.TryResolveDrops(new ResourceBonusContext(areaId, resourceId, toolId), resolvedDrops))
                return false;

            for (var i = 0; i < resolvedDrops.Count; i++)
            {
                var drop = resolvedDrops[i];
                if (drop == null)
                    continue;

                viewModel.AddDrop(CreateRow(drop));
            }

            return true;
        }

        private ResourceDropDebugViewModel CreateValid(
            ResourceAreaDefinition areaDefinition,
            ResourceAreaProgressState areaProgress,
            ResourceDefinition resourceDefinition,
            ResourceProgressState resourceProgress,
            ToolId toolId)
        {
            return new ResourceDropDebugViewModel(
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

        private ResourceDropDebugViewModel CreateInvalid(ResourceAreaId areaId, ResourceId resourceId, ToolId toolId, string failureText)
        {
            return new ResourceDropDebugViewModel(
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

        private static ResourceDropDebugRow CreateRow(ResolvedResourceDrop drop)
        {
            return new ResourceDropDebugRow(
                drop.ItemId,
                drop.DisplayName,
                drop.Category,
                drop.BaseChance,
                drop.BonusChance,
                drop.FinalChance,
                FormatChance(drop.BaseChance),
                FormatBonusChance(drop.BonusChance),
                FormatChance(drop.FinalChance),
                drop.MinAmount,
                drop.MaxAmount,
                FormatAmount(drop.MinAmount, drop.MaxAmount),
                drop.HasExplicitUnlockRequirement,
                drop.IsExplicitlyUnlocked,
                drop.CanRoll,
                drop.FailureReason);
        }

        private static string FormatChance(float value)
        {
            return (value * 100f).ToString("0.##") + "%";
        }

        private static string FormatBonusChance(float value)
        {
            var percent = value * 100f;
            return percent >= 0f ? "+" + percent.ToString("0.##") + "%" : percent.ToString("0.##") + "%";
        }

        private static string FormatAmount(int minAmount, int maxAmount)
        {
            if (minAmount == maxAmount)
                return minAmount.ToString();

            return minAmount + "-" + maxAmount;
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
