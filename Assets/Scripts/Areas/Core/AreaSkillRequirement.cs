using System;
using IdleScaper.Scripts.Skills.Core;

namespace IdleScaper.Scripts.Areas.Core
{
    /// <summary>
    /// Skill requirement for entering or using an area.
    /// </summary>
    [Serializable]
    public struct AreaSkillRequirement
    {
        /// <summary>Required skill.</summary>
        public Skill Skill;

        /// <summary>Minimum level.</summary>
        public int RequiredLevel;
    }
}