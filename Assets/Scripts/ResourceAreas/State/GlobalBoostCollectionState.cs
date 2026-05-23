using System;
using System.Collections.Generic;
using ResourceAreas.Definitions;
using ResourceAreas.Runtime;
using UnityEngine;

namespace ResourceAreas.State
{
    /// <summary>
    /// Serializable runtime state for all global resource boosts.
    /// </summary>
    [Serializable]
    public sealed class GlobalBoostCollectionState
    {
        [SerializeField] private List<GlobalBoostState> boosts = new List<GlobalBoostState>();

        /// <summary>
        /// Global boost states owned by the player.
        /// </summary>
        public List<GlobalBoostState> Boosts => boosts;

        /// <summary>
        /// Normalizes all global boost state after loading save data.
        /// </summary>
        public void Normalize(GlobalBoostCatalog catalog = null)
        {
            boosts ??= new List<GlobalBoostState>();

            for (var i = boosts.Count - 1; i >= 0; i--)
            {
                var boost = boosts[i];
                if (boost == null || !boost.BoostId.IsValid || HasEarlierBoost(boost.BoostId, i))
                {
                    boosts.RemoveAt(i);
                    continue;
                }

                boost.Normalize();
            }
        }

        /// <summary>
        /// Gets a global boost state by id.
        /// </summary>
        public GlobalBoostState GetBoost(GlobalBoostId boostId)
        {
            if (!boostId.IsValid || boosts == null)
                return null;

            for (var i = 0; i < boosts.Count; i++)
            {
                var boost = boosts[i];
                if (boost == null)
                    continue;

                if (boost.BoostId == boostId)
                    return boost;
            }

            return null;
        }

        /// <summary>
        /// Gets existing global boost state or creates it when missing.
        /// </summary>
        public GlobalBoostState GetOrCreateBoost(GlobalBoostId boostId)
        {
            if (!boostId.IsValid)
                return null;

            boosts ??= new List<GlobalBoostState>();

            var existing = GetBoost(boostId);
            if (existing != null)
                return existing;

            var boost = new GlobalBoostState(boostId);
            boosts.Add(boost);
            return boost;
        }

        /// <summary>
        /// Fills results with global boost states active at the given UTC time.
        /// </summary>
        public void GetActiveBoosts(DateTime utcNow, List<GlobalBoostState> results)
        {
            if (results == null)
                throw new ArgumentNullException(nameof(results));

            results.Clear();

            if (boosts == null)
                return;

            for (var i = 0; i < boosts.Count; i++)
            {
                var boost = boosts[i];
                if (boost == null || !boost.BoostId.IsValid)
                    continue;

                if (boost.IsCurrentlyActive(utcNow))
                    results.Add(boost);
            }
        }

        private bool HasEarlierBoost(GlobalBoostId boostId, int beforeIndex)
        {
            for (var i = 0; i < beforeIndex; i++)
            {
                var boost = boosts[i];
                if (boost == null)
                    continue;

                if (boost.BoostId == boostId)
                    return true;
            }

            return false;
        }
    }
}
