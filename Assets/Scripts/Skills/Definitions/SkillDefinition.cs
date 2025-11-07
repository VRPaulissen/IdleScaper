using IdleScaper.Script.Skills.Core;
using IdleScaper.Scripts.Skills.Core;
using UnityEngine;

namespace Scripts.Skills.Definitions
{
    /// <summary>
    /// Defines static data and progression for a skill.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Skill Definition")]
    public class SkillDefinition : ScriptableObject
    {
        /// <summary>Unique skill identifier.</summary>
        public Skill Id;

        /// <summary>Display name shown in UI.</summary>
        public string DisplayName;

        /// <summary>Skill category for grouping.</summary>
        public SkillCategory Category;

        /// <summary>Maximum achievable level.</summary>
        public int MaxLevel = SkillConstants.MAX_LEVEL;

        /// <summary>Indicate is skill is unlocked initially.</summary>
        public bool IsUnlocked = false;
        
        /// <summary>
        /// Gets total XP required for the specified level.
        /// </summary>
        public float GetXpForLevel(int level)
        {
            level = Mathf.Clamp(level, 1, MaxLevel);
            return SkillExperienceUtility.GetExperienceForLevel(level);
        }
    }
}