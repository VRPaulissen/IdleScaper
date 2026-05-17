using System.Collections.Generic;
using Items.Definitions;
using Items.Runtime.Diagnostics;
using Tools.Runtime;
using UnityEngine;

namespace Items.Runtime.Modules
{
    /// <summary>
    /// Marks an item definition as an installable part for a permanent player tool.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Items/Modules/Tool Part", fileName = "Mod_ToolPart_")]
    public sealed class ToolPartModule : ItemModule
    {
        [Header("Compatibility")]
        [SerializeField] private ToolId compatibleToolId;
        [SerializeField] private ToolPartSlotId compatibleSlotId;

        [Header("Progression")]
        [SerializeField, Min(1)] private int maxLevel = 1;

        [Header("Bonuses")]
        [SerializeField] private List<ToolBonusValue> bonuses = new List<ToolBonusValue>();

        /// <summary>
        /// Permanent tool this part can be installed into.
        /// </summary>
        public ToolId CompatibleToolId => compatibleToolId;

        /// <summary>
        /// Internal part slot this item can occupy.
        /// </summary>
        public ToolPartSlotId CompatibleSlotId => compatibleSlotId;

        /// <summary>
        /// Maximum level this part can reach.
        /// </summary>
        public int MaxLevel => Mathf.Max(1, maxLevel);

        /// <summary>
        /// Bonuses contributed by this part while installed.
        /// </summary>
        public IReadOnlyList<ToolBonusValue> Bonuses => bonuses != null ? bonuses : EmptyBonuses;

        private static readonly IReadOnlyList<ToolBonusValue> EmptyBonuses = new ToolBonusValue[0];

        /// <summary>
        /// Returns true when this part can be installed into the given tool slot.
        /// </summary>
        public bool IsCompatibleWith(ToolId toolId, ToolPartSlotId slotId)
        {
            if (!toolId.IsValid)
                return false;

            if (!slotId.IsValid)
                return false;

            return compatibleToolId == toolId && compatibleSlotId == slotId;
        }

        /// <inheritdoc />
        public override void Validate(ItemDefinition definition)
        {
            if (definition == null)
                return;

            if (maxLevel < 1)
                maxLevel = 1;

            bonuses ??= new List<ToolBonusValue>();
        }

        /// <inheritdoc />
        public override void CollectDiagnostics(ItemDefinition definition, List<ItemDiagnostic> results)
        {
            if (results == null)
                return;

            var itemId = definition != null ? definition.Id : default;
            if (!compatibleToolId.IsValid)
                results.Add(ItemDiagnostic.Error("TOOL_PART_TOOL_ID_MISSING", $"ToolPartModule '{name}' has no compatible ToolId.", this, itemId));

            if (!compatibleSlotId.IsValid)
                results.Add(ItemDiagnostic.Error("TOOL_PART_SLOT_ID_MISSING", $"ToolPartModule '{name}' has no compatible ToolPartSlotId.", this, itemId));

            if (maxLevel < 1)
                results.Add(ItemDiagnostic.Error("TOOL_PART_MAX_LEVEL_INVALID", $"ToolPartModule '{name}' has max level < 1.", this, itemId));

            if (bonuses == null)
            {
                results.Add(ItemDiagnostic.Warning("TOOL_PART_BONUSES_NULL", $"ToolPartModule '{name}' has a null bonus list.", this, itemId));
                return;
            }

            var bonusTypes = new HashSet<ToolBonusType>();
            for (var i = 0; i < bonuses.Count; i++)
            {
                var bonus = bonuses[i];
                if (bonus.BaseValue < 0f || bonus.ValuePerLevel < 0f)
                    results.Add(ItemDiagnostic.Warning("TOOL_PART_BONUS_NEGATIVE", $"ToolPartModule '{name}' bonus {i} has a negative value.", this, itemId));

                if (!bonusTypes.Add(bonus.Type))
                    results.Add(ItemDiagnostic.Warning("TOOL_PART_BONUS_DUPLICATE_TYPE", $"ToolPartModule '{name}' has duplicate bonus type '{bonus.Type}'.", this, itemId));
            }
        }
    }
}
