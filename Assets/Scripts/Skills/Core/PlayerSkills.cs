using System;
using System.Collections.Generic;
using IdleScaper.Persistance;
using IdleScaper.Skills.Definitions;
using UnityEngine;

namespace IdleScaper.Skills.Core
{
    /// <summary>
    /// Manages all skill states and XP for a player.
    /// </summary>
    public class PlayerSkills : MonoBehaviour
    {
        [SerializeField] private List<SkillDefinition> skillDefinitions;

        private readonly Dictionary<Skill, SkillState> skills = new();
        private readonly Dictionary<Skill, SkillDefinition> defs = new();

        /// <summary>
        /// Invoked when XP is gained for a skill.
        /// </summary>
        public event Action<Skill, int, SkillExperienceSource> OnExperienceGained;

        /// <summary>
        /// Invoked when a skill levels up.
        /// </summary>
        public event Action<Skill, int> OnLevelUp;

        /// <summary>
        /// Invoked when a skill is unlocked.
        /// </summary>
        public event Action<Skill> OnSkillUnlocked;
        
        /// <summary>
        /// Read-only access to current skill states.
        /// </summary>
        public IReadOnlyDictionary<Skill, SkillState> Skills => skills;

        private void Awake()
        {
            Initialize();
            SkillSaveManager.Load(this);
        }

        private void OnApplicationQuit()
        {
            SkillSaveManager.Save(this);
        }
        
        /// <summary>
        /// Initializes skill states from definitions.
        /// </summary>
        public void Initialize()
        {
            skills.Clear();
            defs.Clear();

            foreach (var def in skillDefinitions)
            {
                if (def == null) continue;

                defs[def.Id] = def;
                skills[def.Id] = new SkillState
                {
                    Id = def.Id,
                    Level = 1,
                    CurrentExperience = 0,
                    IsUnlocked = def.IsUnlocked
                };
            }
        }

        /// <summary>
        /// Gets the current state for a skill.
        /// </summary>
        public SkillState Get(Skill id) => skills[id];

        /// <summary>
        /// Gets the definition for a skill.
        /// </summary>
        public SkillDefinition GetDefinition(Skill id) => defs[id];

        /// <summary>
        /// Unlocks a certain skill.
        /// </summary>
        public void UnlockSkill(Skill id)
        {
            if (!skills.TryGetValue(id, out var state)) return;
            
            state.Unlock();
            OnSkillUnlocked?.Invoke(id);
        }

        /// <summary>
        /// Checks if the player has at least the given level in a skill.
        /// </summary>
        public bool HasLevel(Skill id, int level)
        {
            return skills.TryGetValue(id, out var s) && s.Level >= level;
        }

        /// <summary>
        /// Adds XP to a skill and raises events.
        /// </summary>
        public void AddExperience(Skill id, int amount, SkillExperienceSource source = SkillExperienceSource.Unknown)
        {
            if (!skills.TryGetValue(id, out var state)) return;
            if (!defs.TryGetValue(id, out var def)) return;

            var leveledUp = state.AddExperience(amount, def);
            OnExperienceGained?.Invoke(id, amount, source);

            if (leveledUp)
                OnLevelUp?.Invoke(id, state.Level);
        }

        /// <summary>
        /// Adds XP to multiple skills (e.g. shared combat XP).
        /// </summary>
        public void AddExperienceBatch((Skill skill, int amount)[] grants, SkillExperienceSource source)
        {
            if (grants == null) return;

            foreach (var (skill, amount) in grants)
                AddExperience(skill, amount, source);
        }
    }
}