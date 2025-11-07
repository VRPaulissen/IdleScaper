using System;
using IdleScaper.Scripts.Skills.Core;
using UnityEngine;

namespace IdleScaper.Scripts.Skills
{
    /// <summary>
    /// Simple on-screen UI for debugging skill experience and unlocks.
    /// </summary>
    public class SkillExperienceDebugGUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerSkills playerSkills;

        [Header("Config")]
        [SerializeField] private bool visible = true;

        /// <summary>Selected skill to modify.</summary>
        [SerializeField] private Skill selectedSkill = Skill.Woodcutting;

        /// <summary>Experience amount to add per click.</summary>
        [SerializeField] private int experienceAmount = 100;

        private Rect windowRect = new Rect(10, 10, 480, 860);
        private Vector2 scrollPos;

        private void Awake()
        {
            if (playerSkills == null)
                playerSkills = FindObjectOfType<PlayerSkills>();
        }

        /// <summary>
        /// Toggles visibility from code if needed.
        /// </summary>
        public void SetVisible(bool isVisible)
        {
            visible = isVisible;
        }

        private void OnGUI()
        {
            if (!visible || playerSkills == null)
                return;

            windowRect = GUI.Window(
                GetInstanceID(),
                windowRect,
                DrawWindow,
                "Skill Debug");
        }

        /// <summary>
        /// Draws the debug window contents.
        /// </summary>
        private void DrawWindow(int windowId)
        {
            scrollPos = GUILayout.BeginScrollView(scrollPos);
            GUILayout.BeginVertical();

            DrawSkillSelection();
            GUILayout.Space(6);
            DrawExperienceControls();
            GUILayout.Space(10);
            DrawSelectedSkillUnlockControls();
            GUILayout.Space(10);
            DrawAllSkillsOverview();

            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }

        /// <summary>
        /// Renders skill selection UI.
        /// </summary>
        private void DrawSkillSelection()
        {
            GUILayout.Label("Selected Skill:");
            var names = Enum.GetNames(typeof(Skill));
            var currentIndex = Array.IndexOf(names, selectedSkill.ToString());
            var newIndex = GUILayout.SelectionGrid(currentIndex, names, 2);

            if (newIndex >= 0 && newIndex < names.Length)
                selectedSkill = (Skill)Enum.Parse(typeof(Skill), names[newIndex]);
        }

        /// <summary>
        /// Renders XP input and add buttons.
        /// </summary>
        private void DrawExperienceControls()
        {
            GUILayout.Label($"XP Amount: {experienceAmount}");
            var xpText = GUILayout.TextField(experienceAmount.ToString(), 8);
            if (int.TryParse(xpText, out var parsedXp) && parsedXp > 0)
                experienceAmount = parsedXp;

            GUILayout.Space(4);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+1")) AddExperience(1);
            if (GUILayout.Button("+10")) AddExperience(10);
            if (GUILayout.Button("+100")) AddExperience(100);
            if (GUILayout.Button("+1000")) AddExperience(1000);
            GUILayout.EndHorizontal();

            if (GUILayout.Button($"+{experienceAmount} XP to {selectedSkill}"))
                AddExperience(experienceAmount);
        }

        /// <summary>
        /// Renders unlock controls for the selected skill.
        /// </summary>
        private void DrawSelectedSkillUnlockControls()
        {
            if (!playerSkills.Skills.TryGetValue(selectedSkill, out var state))
                return;

            GUILayout.Label($"Selected: {selectedSkill}");
            GUILayout.Label($"Level: {state.Level} | XP: {state.CurrentExperience}");
            GUILayout.Label($"Status: {(state.IsUnlocked ? "Unlocked" : "Locked")}");

            if (!state.IsUnlocked)
            {
                if (GUILayout.Button($"Unlock {selectedSkill}"))
                    playerSkills.UnlockSkill(selectedSkill);
            }
        }

        /// <summary>
        /// Renders a quick overview of all skills and unlock buttons.
        /// </summary>
        private void DrawAllSkillsOverview()
        {
            GUILayout.Label("All Skills:");

            foreach (Skill skill in Enum.GetValues(typeof(Skill)))
            {
                if (!playerSkills.Skills.TryGetValue(skill, out var state))
                    continue;

                GUILayout.BeginHorizontal();

                GUILayout.Label(
                    $"{skill}: Lvl {state.Level} | XP {state.CurrentExperience}",
                    GUILayout.Width(210));

                GUILayout.Label(
                    state.IsUnlocked ? "Unlocked" : "Locked",
                    GUILayout.Width(70));

                if (!state.IsUnlocked)
                {
                    if (GUILayout.Button("Unlock", GUILayout.Width(60)))
                        playerSkills.UnlockSkill(skill);
                }

                GUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Unlock All Skills"))
            {
                foreach (Skill skill in Enum.GetValues(typeof(Skill)))
                    playerSkills.UnlockSkill(skill);
            }
        }

        /// <summary>
        /// Adds experience to the selected skill.
        /// </summary>
        private void AddExperience(int amount)
        {
            if (amount <= 0)
                return;

            playerSkills.AddExperience(selectedSkill, amount, SkillExperienceSource.Bonus);
        }
    }
}