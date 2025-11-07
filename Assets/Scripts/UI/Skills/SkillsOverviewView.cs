using System.Collections.Generic;
using IdleScaper.Scripts.Skills.Core;
using UnityEngine;

namespace IdleScaper.Scripts.UI.Skills
{
    /// <summary>
    /// Updates a set of predefined skill view entries from PlayerSkills.
    /// </summary>
    public class SkillsOverviewView : MonoBehaviour
    {
        [Header("References")] 
        [SerializeField] private PlayerSkills playerSkills;

        [Header("Entries")] 
        [SerializeField] private SkillViewEntry[] entries;

        private readonly Dictionary<Skill, SkillViewEntry> entryBySkill = new();

        private void OnEnable()
        {
            if (playerSkills == null || entries == null)
                return;

            BuildLookup();
            InitializeEntries();

            playerSkills.OnExperienceGained += HandleExperienceGained;
            playerSkills.OnLevelUp += HandleLevelUp;
            playerSkills.OnSkillUnlocked += HandleUnlocked;
        }


        private void OnDisable()
        {
            if (playerSkills == null)
                return;

            playerSkills.OnExperienceGained -= HandleExperienceGained;
            playerSkills.OnLevelUp -= HandleLevelUp;
        }

        /// <summary>
        /// Builds a quick lookup from skill to entry.
        /// </summary>
        private void BuildLookup()
        {
            entryBySkill.Clear();

            foreach (var entry in entries)
            {
                if (entry == null) continue;

                var skill = entry.Skill;
                entryBySkill.TryAdd(skill, entry);
            }
        }

        /// <summary>
        /// Initializes all entries with the current skill states.
        /// </summary>
        private void InitializeEntries()
        {
            foreach (var pair in entryBySkill)
            {
                var skill = pair.Key;
                var entry = pair.Value;

                if (!playerSkills.Skills.TryGetValue(skill, out var state))
                    continue;

                entry.Initialize(state);
            }
        }

        /// <summary>
        /// Updates the relevant entry when XP is gained.
        /// </summary>
        private void HandleExperienceGained(Skill skill, int amount, SkillExperienceSource source)
        {
            if (!entryBySkill.TryGetValue(skill, out var entry)) return;
            if (!playerSkills.Skills.TryGetValue(skill, out var state)) return;

            entry.UpdateView(state);
        }

        /// <summary>
        /// Updates the relevant entry on level up.
        /// </summary>
        private void HandleLevelUp(Skill skill, int newLevel)
        {
            if (!entryBySkill.TryGetValue(skill, out var entry)) return;
            if (!playerSkills.Skills.TryGetValue(skill, out var state)) return;

            entry.UpdateView(state);
        }
        
        /// <summary>
        /// Updates the relevant entry on unlocking it.
        /// </summary>
        private void HandleUnlocked(Skill skill)
        {
            if (!entryBySkill.TryGetValue(skill, out var entry)) return;
            if (!playerSkills.Skills.TryGetValue(skill, out var state)) return;
            
            entry.UpdateView(state);
        }
    }
}