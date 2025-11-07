using IdleScaper.Scripts.Items.Definitions;
using IdleScaper.Scripts.Skills.Core;
using UnityEngine;

namespace IdleScaper.Scripts.Skills.Actions
{
    /// <summary>
    /// Base definition for a skill-related action.
    /// </summary>
    public abstract class SkillActionDefinition : ScriptableObject
    {
        /// <summary>Unique action identifier.</summary>
        public string Id;

        /// <summary>Primary skill used by this action.</summary>
        public Skill PrimarySkill;

        /// <summary>Minimum level required to perform this action.</summary>
        public int RequiredLevel = 1;

        /// <summary>Tools required to perform this action.</summary>
        public ToolRequirement[] RequiredTools;

        /// <summary>Base XP granted on successful completion.</summary>
        public int BaseExperience = 5;
    }
}