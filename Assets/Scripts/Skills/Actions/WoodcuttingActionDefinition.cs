using System.Collections.Generic;
using IdleScaper.Scripts.Items.Definitions;
using IdleScaper.Scripts.Skills.Actions;
using UnityEngine;

namespace Scripts.Skills.Definitions.Woodcutting
{
     /// <summary>
    /// Defines a woodcutting action with weighted rewards.
    /// </summary>
    [CreateAssetMenu(menuName = "IdleScaper/Actions/Woodcutting Action")]
    public class WoodcuttingActionDefinition : SkillActionDefinition
    {
        /// <summary>Possible rewards granted when this action succeeds.</summary>
        public GatheringRewardEntry[] Rewards;

        /// <summary>Base time in seconds per chop attempt.</summary>
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

                // Roll based on weight treated as chance per action (0-1) or relative weight.
                if (entry.Weight <= 1f)
                {
                    if (Random.value <= entry.Weight)
                    {
                        var amount = Random.Range(entry.MinAmount, entry.MaxAmount + 1);
                        if (amount > 0)
                            buffer.Add((entry.Item, amount));
                    }
                }
                else
                {
                    // Relative weight: normalize via random range.
                    if (Random.Range(0f, entry.WeightSumHint) < entry.Weight)
                    {
                        var amount = Random.Range(entry.MinAmount, entry.MaxAmount + 1);
                        if (amount > 0)
                            buffer.Add((entry.Item, amount));
                    }
                }
            }
        }
    }
}