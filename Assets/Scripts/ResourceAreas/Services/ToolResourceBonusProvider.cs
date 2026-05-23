using System;
using System.Collections.Generic;
using ResourceAreas.Runtime;
using Tools.Runtime;

namespace ResourceAreas.Services
{
    /// <summary>
    /// Adapts permanent tool bonuses into resource bonus contributions.
    /// </summary>
    public sealed class ToolResourceBonusProvider : IResourceBonusProvider
    {
        private readonly IToolBonusService toolBonusService;

        /// <summary>
        /// Creates a tool resource bonus provider.
        /// </summary>
        public ToolResourceBonusProvider(IToolBonusService toolBonusService)
        {
            this.toolBonusService = toolBonusService ?? throw new ArgumentNullException(nameof(toolBonusService));
        }

        /// <inheritdoc />
        public void AddBonuses(ResourceBonusContext context, ResourceBonusCollection bonuses)
        {
            if (bonuses == null)
                return;

            if (!context.ToolId.IsValid)
                return;

            var toolBonuses = toolBonusService.GetActiveBonuses(context.ToolId);
            if (toolBonuses == null)
                return;

            AddMappedToolBonuses(context.ToolId, toolBonuses.Values, bonuses);
        }

        private static void AddMappedToolBonuses(
            ToolId toolId,
            IReadOnlyDictionary<ToolBonusType, float> toolBonuses,
            ResourceBonusCollection bonuses)
        {
            if (toolBonuses == null)
                return;

            foreach (var toolBonus in toolBonuses)
            {
                if (!TryMapBonusType(toolBonus.Key, out var resourceBonusType))
                    continue;

                bonuses.Add(
                    resourceBonusType,
                    toolBonus.Value,
                    toolId.ToString(),
                    ResourceBonusSourceType.Tool,
                    toolId.Value);
            }
        }

        private static bool TryMapBonusType(ToolBonusType toolBonusType, out ResourceBonusType resourceBonusType)
        {
            switch (toolBonusType)
            {
                case ToolBonusType.ResourceYieldMultiplier:
                    resourceBonusType = ResourceBonusType.ResourceYieldMultiplier;
                    return true;
                case ToolBonusType.RareDropChanceFlat:
                    resourceBonusType = ResourceBonusType.RareDropChanceFlat;
                    return true;
                case ToolBonusType.MiningDamageMultiplier:
                    resourceBonusType = ResourceBonusType.MiningDamageMultiplier;
                    return true;
                default:
                    resourceBonusType = default;
                    return false;
            }
        }
    }
}
