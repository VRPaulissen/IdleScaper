using System;
using System.Collections.Generic;
using Items.Runtime;
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
        /// Applies one hit to the resource and returns true if it was depleted by this hit.
        /// </summary>
        public bool ApplyHit(
            ResourceDefinition definition, 
            ResourceRuntimeState state, 
            int damageOverride, 
            out IReadOnlyList<ItemInstance> depletionDrops)
        {
            depletionDrops = Array.Empty<ItemInstance>();

            if (definition == null)
                return false;

            if (state == null)
                return false;

            var damage = damageOverride > 0 ? damageOverride : definition.BaseDamagePerHit;
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