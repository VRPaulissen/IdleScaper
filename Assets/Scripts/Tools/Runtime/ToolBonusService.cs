using System;
using Items.Runtime;
using Items.Runtime.Modules;
using Tools.State;
using UnityEngine;
using ToolLogger = Utilities.Logging.Logger;

namespace Tools.Runtime
{
    /// <summary>
    /// Read-only service that aggregates bonuses from installed permanent tool parts.
    /// </summary>
    public sealed class ToolBonusService : IToolBonusService
    {
        private readonly ToolCollectionState tools;
        private readonly ItemDatabase itemDatabase;

        /// <summary>
        /// Creates a permanent tool bonus aggregation service.
        /// </summary>
        public ToolBonusService(ToolCollectionState tools, ItemDatabase itemDatabase)
        {
            this.tools = tools ?? throw new ArgumentNullException(nameof(tools));
            this.itemDatabase = itemDatabase ?? throw new ArgumentNullException(nameof(itemDatabase));
        }

        /// <inheritdoc />
        public ToolBonusAggregate GetActiveBonuses(ToolId toolId)
        {
            var aggregate = new ToolBonusAggregate();
            if (!toolId.IsValid)
                return aggregate;

            var tool = tools.GetTool(toolId);
            if (tool == null)
                return aggregate;

            var preset = tool.GetPreset(tool.ActivePresetIndex);
            if (preset == null)
                return aggregate;

            var slots = preset.Slots;
            if (slots == null)
                return aggregate;

            for (var i = 0; i < slots.Count; i++)
            {
                AddSlotBonuses(toolId, slots[i], aggregate);
            }

            return aggregate;
        }

        private void AddSlotBonuses(ToolId toolId, ToolPartSlotState slot, ToolBonusAggregate aggregate)
        {
            if (slot == null)
                return;

            if (!slot.HasInstalledPart)
                return;

            if (!itemDatabase.TryGet(slot.InstalledPartItemId, out var itemDefinition) || itemDefinition == null)
            {
                ToolLogger.LogWarning($"Installed tool part '{slot.InstalledPartItemId}' was not found while aggregating bonuses.");
                return;
            }

            if (!itemDefinition.TryGetModule<ToolPartModule>(out var module) || module == null)
            {
                ToolLogger.LogWarning($"Installed item '{slot.InstalledPartItemId}' has no ToolPartModule while aggregating bonuses.");
                return;
            }

            if (!module.IsCompatibleWith(toolId, slot.SlotId))
            {
                ToolLogger.LogWarning($"Installed tool part '{slot.InstalledPartItemId}' is incompatible with '{toolId}' slot '{slot.SlotId}' while aggregating bonuses.");
                return;
            }

            AddModuleBonuses(module, GetEffectiveLevel(slot, module), aggregate);
        }

        private static int GetEffectiveLevel(ToolPartSlotState slot, ToolPartModule module)
        {
            var effectiveLevel = Mathf.Clamp(slot.PartLevel, 0, module.MaxLevel);
            if (effectiveLevel == slot.PartLevel)
                return effectiveLevel;

            ToolLogger.LogWarning($"Tool part '{slot.InstalledPartItemId}' level {slot.PartLevel} was clamped to {effectiveLevel} while aggregating bonuses.");
            return effectiveLevel;
        }

        private static void AddModuleBonuses(ToolPartModule module, int level, ToolBonusAggregate aggregate)
        {
            var bonuses = module.Bonuses;
            for (var i = 0; i < bonuses.Count; i++)
            {
                var bonus = bonuses[i];
                aggregate.Add(bonus.Type, bonus.Evaluate(level));
            }
        }
    }
}
