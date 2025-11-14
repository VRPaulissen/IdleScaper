using System;
using IdleScaper.Skills.Definitions;

namespace IdleScaper.Skills.Core
{
    /// <summary>
    /// Holds runtime XP and level for a single skill.
    /// </summary>
    [Serializable]
    public class SkillState
    {
        /// <summary>Skill identifier.</summary>
        public Skill Id;

        /// <summary>Current level.</summary>
        public int Level = 1;

        /// <summary>Current XP total.</summary>
        public int CurrentExperience;
        
        /// <summary>Unlocked state.</summary>
        public bool IsUnlocked = false;

        /// <summary>
        /// Adds XP and updates level based on the given definition.
        /// </summary>
        public bool AddExperience(int amount, SkillDefinition def)
        {
            if (!IsUnlocked)
            {
                Logger.LogWarning($"Skill is not unlocked.");
                return false;
            }
            
            if (amount <= 0 || def == null) return false;

            CurrentExperience += amount;

            var newLevel = SkillExperienceUtility.GetLevelForExperience(CurrentExperience, def.MaxLevel);
            if (newLevel > Level)
            {
                Level = newLevel;
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Unlocks the skills.
        /// </summary>
        public void Unlock(bool unlocked = true)
        {
            IsUnlocked = unlocked;
        }
    }
}