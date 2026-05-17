using Equipment;
using Items.Runtime;
using Items.Runtime.Modules;

namespace Player
{
    /// <summary>
    /// Tool provider for a specific equipment slot (e.g. Axe, Pickaxe).
    /// </summary>
    public sealed class SlotToolProvider : IResourceToolProvider
    {
        private readonly IEquipmentService equipment;
        private readonly ItemDatabase itemDatabase;
        private readonly EquipmentSlotId toolSlotId;

        /// <summary>
        /// Creates a provider bound to a specific tool slot.
        /// </summary>
        public SlotToolProvider(IEquipmentService equipment, ItemDatabase itemDatabase, EquipmentSlotId toolSlotId)
        {
            this.equipment = equipment;
            this.itemDatabase = itemDatabase;
            this.toolSlotId = toolSlotId;
        }

        /// <inheritdoc />
        public bool TryGetActiveTool(out GatheringToolStats tool)
        {
            tool = default;

            var toolItemId = equipment.GetEquipped(toolSlotId);
            if (!toolItemId.IsValid)
                return false;

            if (!itemDatabase.TryGetItem(toolItemId, out var def) || def == null)
                return false;

            if (!def.TryGetModule<GatheringToolModule>(out var mod) || mod == null)
                return false;

            tool = new GatheringToolStats(
                mod.HitIntervalSeconds,
                mod.DamagePerHit,
                mod.CritChance,
                mod.CritMultiplier,
                mod.UltraCritChance,
                mod.UltraCritMultiplier);

            return true;
        }
    }
}
