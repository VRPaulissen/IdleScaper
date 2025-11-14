using System.Collections.Generic;
using IdleScaper.Items;
using IdleScaper.Items.Definitions;
using IdleScaper.Skills.Core;

namespace IdleScaper.Skills.Actions
{
    /// <summary>
    /// Handles execution of gathering actions.
    /// </summary>
    public class GatheringActionProcessor : SkillActionProcessor<GatheringActionDefinition>
    {
        private readonly Inventory inventory;
        private readonly List<(ItemDefinition item, int amount)> rewardBuffer = new();

        public GatheringActionProcessor(PlayerSkills playerSkills, Inventory inventory) : base(playerSkills)
        {
            this.inventory = inventory;
        }

        /// <summary>
        /// Checks if player meets level and tool requirements.
        /// </summary>
        protected override bool MeetsRequirements(GatheringActionDefinition action)
        {
            if (!PlayerSkills.HasLevel(action.PrimarySkill, action.RequiredLevel)) 
                return false;

            if (action.RequiredTools != null)
            {
                foreach (var req in action.RequiredTools)
                {
                    req.Normalize();

                    if (req.Tool == null)
                        continue;

                    if (!inventory.HasItem(req.Tool, req.Quantity))
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Applies rewards and consumes tools if configured.
        /// </summary>
        protected override void OnExecute(GatheringActionDefinition action)
        {
            // Consume tools if needed.
            if (action.RequiredTools != null)
            {
                foreach (var req in action.RequiredTools)
                {
                    req.Normalize();

                    if (req.Tool == null || !req.Consume)
                        continue;

                    inventory.TryRemove(req.Tool, req.Quantity);
                }
            }

            // Roll and grant rewards.
            action.GetRolledRewards(rewardBuffer);
            foreach (var (item, amount) in rewardBuffer)
            {
                inventory.Add(item, amount);
            }
        }
    }
}