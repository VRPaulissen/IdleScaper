using System;
using UnityEngine;

namespace IdleScaper.Skills.Core
{
    /// <summary>
    /// Provides XP calculation utilities for skills.
    /// </summary>
    public static class SkillExperienceUtility
    {
        /// <summary>
        /// Gets the total XP required.
        /// </summary>
        public static int GetExperienceForLevel(int level)
        {
            level = Mathf.Max(1, level);

            double points = 0;
            for (var lvl = 1; lvl < level; lvl++)
            {
                points += Math.Floor(lvl + 300.0 * Math.Pow(2.0, lvl / 7.0));
            }

            return (int)Math.Floor(points / 4.0);
        }

        /// <summary>
        /// Gets the level for the given XP.
        /// </summary>
        public static int GetLevelForExperience(int xp, int maxLevel)
        {
            xp = Math.Max(0, xp);
            for (var level = maxLevel; level >= 1; level--)
            {
                if (xp >= GetExperienceForLevel(level))
                    return level;
            }

            return 1;
        }
    }
}