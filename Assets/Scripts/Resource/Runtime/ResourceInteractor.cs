using System;
using System.Collections.Generic;
using Items.Runtime;
using Player;
using Resource.Definitions;
using Utilities.Calculations;

namespace Resource.Runtime
{
    /// <summary>
    /// Applies hits to a resource node and rolls rewards on depletion.
    /// </summary>
    public sealed class ResourceInteractor
    {
        private readonly IRandomSource randomSource;
        private readonly List<ItemInstance> dropsBuffer = new List<ItemInstance>(16);

        /// <summary>
        /// Creates an interactor using the provided RNG.
        /// </summary>
        public ResourceInteractor(IRandomSource randomSource)
        {
            this.randomSource = randomSource ?? throw new ArgumentNullException(nameof(randomSource));
        }

        /// <summary>
        /// Applies one hit to the resource using the provided tool stats and returns true if it was depleted by this hit.
        /// </summary>
        public bool ApplyHit(
            ResourceDefinition definition,
            ResourceRuntimeState state,
            GatheringToolStats tool,
            out GatheringDamageRoll damageRoll,
            out IReadOnlyList<ItemInstance> depletionDrops)
        {
            depletionDrops = Array.Empty<ItemInstance>();
            damageRoll = GatheringDamageRoll.Normal(0);

            if (definition == null)
                return false;

            if (state == null)
                return false;

            // If tool has no valid base damage, fall back to definition base damage as a normal hit.
            damageRoll = tool.BaseDamagePerHit > 0 ? 
                tool.RollDamage(randomSource) : 
                GatheringDamageRoll.Normal(definition.BaseDamagePerHit);

            var damage = damageRoll.FinalDamage;
            if (damage <= 0)
                return false;

            var newDurability = state.DurabilityCurrent - damage;
            state.SetDurability(newDurability);

            if (state.DurabilityCurrent > 0)
                return false;

            definition.Roll(randomSource, dropsBuffer);
            depletionDrops = dropsBuffer;
            return true;
        }
    }
}