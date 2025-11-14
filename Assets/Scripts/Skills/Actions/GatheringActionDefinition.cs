using System.Collections.Generic;
using IdleScaper.Items.Definitions;
using UnityEngine;

namespace IdleScaper.Skills.Actions
{
    /// <summary>
    /// Defines a generic gathering action with weighted rewards.
    /// Works for woodcutting, mining, foraging, fishing, etc.
    /// </summary>
    [CreateAssetMenu(menuName = "IdleScaper/Actions/Gathering Action")]
    public class GatheringActionDefinition : SkillActionDefinition
    {
        /// <summary>Possible rewards granted when this action succeeds.</summary>
        public GatheringRewardEntry[] Rewards;

        /// <summary>Base time in seconds per attempt.</summary>
        public float ActionTime = 3f;

        /// <summary>
        /// Rolls rewards for a single successful action.
        /// </summary>
        public void GetRolledRewards(List<(ItemDefinition item, int amount)> buffer)
        {
            buffer.Clear();

            if (Rewards == null || Rewards.Length == 0) return;

            for (var i = 0; i < Rewards.Length; i++)
            {
                var entry = Rewards[i];
                if (entry.Item == null || entry.Weight <= 0f)
                    continue;

                // If Weight <= 1: treat as direct chance.
                if (entry.Weight <= 1f)
                {
                    if (Random.value <= entry.Weight)
                    {
                        var amount = Random.Range(entry.MinAmount, entry.MaxAmount + 1);
                        if (amount > 0)
                            buffer.Add((entry.Item, amount));
                    }
                }
                // If Weight > 1: use WeightSumHint as a crude relative weight pool.
                else if (entry.WeightSumHint > 0f && Random.Range(0f, entry.WeightSumHint) < entry.Weight)
                {
                    var amount = Random.Range(entry.MinAmount, entry.MaxAmount + 1);
                    if (amount > 0)
                        buffer.Add((entry.Item, amount));
                }
            }
        }
    }
}