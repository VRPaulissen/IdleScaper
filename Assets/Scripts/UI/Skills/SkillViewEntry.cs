using IdleScaper.Skills.Core;
using TMPro;
using UnityEngine;

namespace IdleScaper.UI.Skills
{
    /// <summary>
    /// Displays a single skill's level and experience.
    /// </summary>
    public class SkillViewEntry : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private Skill skill;

        [Header("UI")]
        [SerializeField] private TMP_Text skillNameText;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text experienceText;
        [SerializeField] private CanvasGroup lockedOverlay;

        /// <summary>
        /// The skill this entry represents.
        /// </summary>
        public Skill Skill => skill;

        /// <summary>
        /// Initializes the view using the given state.
        /// </summary>
        public void Initialize(SkillState state)
        {
            UpdateView(state);
        }

        /// <summary>
        /// Updates the displayed values from the given state.
        /// </summary>
        public void UpdateView(SkillState state)
        {
            if (state == null) return;

            if (skillNameText != null) skillNameText.text = state.Id.ToString();
            if (levelText != null)     levelText.text = state.IsUnlocked ? $"{state.Level}" : "-";

            if (experienceText != null)
            {
                var currentXp = state.CurrentExperience;
                var currentLevel = state.Level;
                var nextLevel = currentLevel + 1;

                // Get XP required for next level
                var xpForNextLevel = SkillExperienceUtility.GetExperienceForLevel(nextLevel);
                experienceText.text = state.IsUnlocked ? $"{currentXp} / {xpForNextLevel}" : "-";
            }
            
            // Update locked overlay.
            lockedOverlay.alpha = state.IsUnlocked ? 0f : 0.8f;
            lockedOverlay.interactable = !state.IsUnlocked;
            lockedOverlay.blocksRaycasts = !state.IsUnlocked;
        }

        private void OnValidate()
        {
            if (!lockedOverlay)
            {
                lockedOverlay = GetComponentInChildren<CanvasGroup>();
            }
        }
    }
}