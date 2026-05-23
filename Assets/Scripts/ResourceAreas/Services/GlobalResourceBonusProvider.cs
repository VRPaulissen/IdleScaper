using System;
using System.Collections.Generic;
using ResourceAreas.Definitions;
using ResourceAreas.Runtime;
using ResourceAreas.State;

namespace ResourceAreas.Services
{
    /// <summary>
    /// Provides resource bonuses from active global boost state.
    /// </summary>
    public sealed class GlobalResourceBonusProvider : IResourceBonusProvider
    {
        private readonly GlobalBoostCollectionState state;
        private readonly GlobalBoostCatalog catalog;
        private readonly Func<DateTime> utcNowProvider;
        private readonly List<GlobalBoostState> activeBoosts = new List<GlobalBoostState>(8);

        /// <summary>
        /// Creates a global resource bonus provider.
        /// </summary>
        public GlobalResourceBonusProvider(
            GlobalBoostCollectionState state,
            GlobalBoostCatalog catalog,
            Func<DateTime> utcNowProvider = null)
        {
            this.state = state;
            this.catalog = catalog;
            this.utcNowProvider = utcNowProvider ?? (() => DateTime.UtcNow);
        }

        /// <inheritdoc />
        public void AddBonuses(ResourceBonusContext context, ResourceBonusCollection bonuses)
        {
            if (bonuses == null)
                return;

            if (state == null || catalog == null)
                return;

            state.GetActiveBoosts(utcNowProvider(), activeBoosts);
            for (var i = 0; i < activeBoosts.Count; i++)
            {
                AddBoostBonuses(context, activeBoosts[i], bonuses);
            }
        }

        private void AddBoostBonuses(ResourceBonusContext context, GlobalBoostState boostState, ResourceBonusCollection bonuses)
        {
            if (boostState == null || !boostState.BoostId.IsValid)
                return;

            if (!catalog.TryGet(boostState.BoostId, out var definition) || definition == null)
                return;

            var stackCount = Math.Max(1, boostState.StackCount);
            for (var stack = 0; stack < stackCount; stack++)
            {
                AddDefinitionEffects(context, definition, bonuses);
            }
        }

        private static void AddDefinitionEffects(ResourceBonusContext context, GlobalBoostDefinition definition, ResourceBonusCollection bonuses)
        {
            var effects = definition.Effects;
            if (effects == null)
                return;

            var source = new ResourceBonusEffectSource(
                definition.DisplayName,
                ResourceBonusSourceType.Global,
                definition.Id.Value);

            for (var i = 0; i < effects.Count; i++)
            {
                var effect = effects[i];
                if (effect == null)
                    continue;

                effect.AddBonuses(context, bonuses, source);
            }
        }
    }
}
