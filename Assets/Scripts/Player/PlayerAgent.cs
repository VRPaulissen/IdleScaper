using System.Collections;
using IdleScaper.Scripts.Items;
using IdleScaper.Scripts.Skills.Actions;
using IdleScaper.Scripts.Skills.Core;
using Scripts.Skills.Definitions.Woodcutting;
using UnityEngine;

namespace IdleScaper.Scripts.Areas
{
    /// <summary>
    /// Controls player movement to a spot and runs AFK actions there.
    /// </summary>
    public class PlayerAgent : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerSkills playerSkills;
        [SerializeField] private Inventory inventory;

        [Header("Config")]
        [SerializeField] private float interactRange = 1.5f;
        [SerializeField] private float actionInterval = 2f;

        private AreaSpotInstance currentSpot;
        private Coroutine afkRoutine;

        private WoodcuttingActionProcessor woodcuttingProcessor;

        private void Awake()
        {
            woodcuttingProcessor = new WoodcuttingActionProcessor(playerSkills, inventory);
        }

        /// <summary>
        /// Sets the current target spot and starts moving/afking.
        /// </summary>
        public void SetTarget(AreaSpotInstance spot)
        {
            currentSpot = spot;
            StopAfk();

            if (currentSpot == null)
                return;

            // TODO: plug into movement system. For now, teleport:
            transform.position = currentSpot.transform.position;

            StartAfk();
        }

        private void StartAfk()
        {
            if (currentSpot == null)
                return;

            afkRoutine = StartCoroutine(AfkLoop());
        }

        private void StopAfk()
        {
            if (afkRoutine != null)
                StopCoroutine(afkRoutine);

            afkRoutine = null;
        }

        /// <summary>
        /// Repeatedly executes the action at the current spot while valid.
        /// </summary>
        private IEnumerator AfkLoop()
        {
            var wait = new WaitForSeconds(actionInterval);

            while (currentSpot != null)
            {
                var action = currentSpot.Action;
                if (action == null)
                {
                    currentSpot = null;
                    yield break;
                }

                // Ensure we are still in range.
                if (Vector3.Distance(transform.position, currentSpot.transform.position) > interactRange)
                {
                    currentSpot = null;
                    yield break;
                }

                ExecuteAction(action);

                yield return wait;
            }
        }

        /// <summary>
        /// Routes and executes the given skill action.
        /// </summary>
        private void ExecuteAction(SkillActionDefinition action)
        {
            switch (action)
            {
                case WoodcuttingActionDefinition woodcutting:
                    woodcuttingProcessor.TryExecute(woodcutting);
                    break;
            }
        }
    }
}