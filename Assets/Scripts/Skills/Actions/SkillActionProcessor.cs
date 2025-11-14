using IdleScaper.Skills.Core;

namespace IdleScaper.Skills.Actions
{
    /// <summary>
    /// Base processor for handling a specific action definition type.
    /// </summary>
    /// <typeparam name="TAction">Action definition type.</typeparam>
    public abstract class SkillActionProcessor<TAction> where TAction : SkillActionDefinition
    {
        /// <summary>Player skills reference.</summary>
        protected readonly PlayerSkills PlayerSkills;

        protected SkillActionProcessor(PlayerSkills playerSkills)
        {
            PlayerSkills = playerSkills;
        }

        /// <summary>
        /// Executes the action if requirements are met.
        /// </summary>
        public void TryExecute(TAction action)
        {
            if (!MeetsRequirements(action))
                return;

            OnExecute(action);
            GrantXp(action);
        }

        /// <summary>
        /// Checks if the action can be executed.
        /// </summary>
        protected abstract bool MeetsRequirements(TAction action);

        /// <summary>
        /// Applies the action-specific effects (items, timers, etc.). 
        /// </summary>
        protected abstract void OnExecute(TAction action);

        /// <summary>
        /// Grants XP for the action.
        /// </summary>
        protected virtual void GrantXp(TAction action)
        {
            PlayerSkills.AddExperience
            (
                id:     action.PrimarySkill, 
                amount: action.BaseExperience, 
                source: SkillExperienceSource.IdleArea
            );
        }
    }
}