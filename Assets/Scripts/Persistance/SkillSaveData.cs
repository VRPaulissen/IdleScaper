using System;
using System.Collections.Generic;
using IdleScaper.Skills.Core;

namespace IdleScaper.Persistance
{
    /// <summary>
    /// Serializable container for all skill-related save data.
    /// </summary>
    [Serializable]
    public class SkillSaveData
    {
        /// <summary>All saved skill entries.</summary>
        public List<SkillEntry> Skills = new();
    }

    /// <summary>
    /// Serializable snapshot of a single skill.
    /// </summary>
    [Serializable]
    public class SkillEntry
    {
        public Skill Id;
        public int Level;
        public int Experience;
        public bool IsUnlocked;
    }
}
