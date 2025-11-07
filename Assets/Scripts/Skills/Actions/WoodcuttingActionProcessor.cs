using System.Collections.Generic;
using IdleScaper.Scripts.Items;
using IdleScaper.Scripts.Items.Definitions;
using IdleScaper.Scripts.Skills.Core;
using Scripts.Skills.Definitions.Woodcutting;

namespace IdleScaper.Scripts.Skills.Actions
{
    /// <summary>
    /// Handles execution of woodcutting actions.
    /// </summary>
    public class WoodcuttingActionProcessor : SkillActionProcessor<WoodcuttingActionDefinition>
    {
        private readonly Inventory inventory;
        private readonly List<(ItemDefinition item, int amount)> rewardBuffer = new();

        public WoodcuttingActionProcessor(PlayerSkills playerSkills, Inventory inventory) : base(playerSkills)
        {
            this.inventory = inventory;
        }

        /// <summary>
        /// Checks if player meets level and tool requirements.
        /// </summary>
        protected override bool MeetsRequirements(WoodcuttingActionDefinition action)
        {
            if (!PlayerSkills.HasLevel(action.PrimarySkill, action.RequiredLevel)) return false;

            if (action.RequiredTools != null)
            {
                for (int i = 0; i < action.RequiredTools.Length; i++)
                {
                    var req = action.RequiredTools[i];
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
        protected override void OnExecute(WoodcuttingActionDefinition action)
        {
            // Consume tools if needed.
            if (action.RequiredTools != null)
            {
                for (int i = 0; i < action.RequiredTools.Length; i++)
                {
                    var req = action.RequiredTools[i];
                    req.Normalize();

                    if (req.Tool == null || !req.Consume)
                        continue;

                    inventory.TryRemove(req.Tool, req.Quantity);
                }
            }

            // Roll and grant rewards.
            action.GetRolledRewards(rewardBuffer);
            for (int i = 0; i < rewardBuffer.Count; i++)
            {
                var (item, amount) = rewardBuffer[i];
                inventory.Add(item, amount);
            }
        }
    }
}