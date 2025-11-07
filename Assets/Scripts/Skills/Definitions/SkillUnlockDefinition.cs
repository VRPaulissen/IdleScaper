using IdleScaper.Scripts.Skills.Core;
using UnityEngine;

namespace Scripts.Skills.Definitions
{
    /// <summary>
    /// Types of content that can be unlocked by a skill.
    /// </summary>
    public enum SkillUnlockType
    {
        None,
        Action,
        Item,
        Area,
        Other
    }
    
    /// <summary>
    /// Defines a level-based unlock for a skill.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Skills/Skill Unlock")]
    public class SkillUnlockDefinition : ScriptableObject
    {
        /// <summary>Skill this unlock belongs to.</summary>
        public Skill Skill;

        /// <summary>Required level.</summary>
        public int RequiredLevel;

        /// <summary>Category of unlock target.</summary>
        public SkillUnlockType UnlockType = SkillUnlockType.None;

        /// <summary>
        /// Optional direct reference to the unlocked asset
        /// (e.g. a specific action, node, item, or area definition).
        /// </summary>
        public ScriptableObject Target;

        /// <summary>
        /// Optional identifier used when no direct reference is set
        /// (e.g. for scene logic, addressables, or custom systems).
        /// </summary>
        public string UnlockId;

        /// <summary>Description for UI.</summary>
        [TextArea]
        public string Description;

        /// <summary>
        /// Gets a human-readable label for the unlocked content.
        /// </summary>
        public string GetLabel()
        {
            if (Target != null) return Target.name;

            if (!string.IsNullOrWhiteSpace(UnlockId))
                return UnlockId;

            return Description;
        }
    }
}