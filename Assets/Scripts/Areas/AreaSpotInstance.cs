using IdleScaper.Scripts.Skills.Actions;
using UnityEngine;

namespace IdleScaper.Scripts.Areas
{
    /// <summary>
    /// Represents a single interactive spot in an idle area.
    /// </summary>
    public class AreaSpotInstance : MonoBehaviour
    {
        /// <summary>Assigned action for this spot.</summary>
        public SkillActionDefinition Action { get; private set; }

        /// <summary>
        /// Initializes this spot with the given action.
        /// </summary>
        public void Initialize(SkillActionDefinition action)
        {
            Action = action;
        }

        /// <summary>
        /// Called by your input system when the spot is clicked.
        /// </summary>
        public void OnClicked(PlayerAgent agent)
        {
            if (Action == null || agent == null) return;
            agent.SetTarget(this);
        }
    }
}