using System.Collections.Generic;
using IdleScaper.Scripts.Skills.Core;
using Scripts.Skills.Definitions;
using UnityEngine;

namespace IdleScaper.Scripts.Skills.Services
{
    /// <summary>
    /// Tracks which skill unlocks are available for a player.
    /// </summary>
    public class SkillUnlockService : MonoBehaviour
    {
        [SerializeField] private PlayerSkills playerSkills;
        [SerializeField] private List<SkillUnlockDefinition> allUnlocks;

        private readonly HashSet<string> unlockedIds = new();

        private void OnEnable()
        {
            playerSkills.OnLevelUp += HandleLevelUp;
        }

        private void OnDisable()
        {
            playerSkills.OnLevelUp -= HandleLevelUp;
        }

        /// <summary>
        /// Checks if an unlock id is available.
        /// </summary>
        public bool IsUnlocked(string unlockId) => unlockedIds.Contains(unlockId);

        /// <summary>
        /// Handles unlock checks when a skill levels up.
        /// </summary>
        private void HandleLevelUp(Skill skill, int newLevel)
        {
            foreach (var def in allUnlocks)
            {
                if (def.Skill != skill) continue;
                if (def.RequiredLevel != newLevel) continue;

                if (unlockedIds.Add(def.UnlockId))
                {
                    Logger.Log($"Unlocking skill {skill} for level {def.RequiredLevel}");
                }
            }
        }
    }
}