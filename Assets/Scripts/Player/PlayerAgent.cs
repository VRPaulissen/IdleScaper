using System.Collections;
using IdleScaper.Areas;
using IdleScaper.Items;
using IdleScaper.Skills.Actions;
using IdleScaper.Skills.Core;
using UnityEngine;

namespace IdleScaper.Player
{
    /// <summary>
    /// Controls player movement to a spot and runs AFK actions there.
    /// </summary>
    public class PlayerAgent : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerSkills playerSkills;
        [SerializeField] private PlayerMover playerMover;
        [SerializeField] private Inventory inventory;
        
        [Header("Config")]
        [SerializeField] private float interactRange = 1.5f;
        [SerializeField] private float actionInterval = 2f;

        private AreaSpotInstance currentSpot;
        private Coroutine afkRoutine;
        private GatheringActionProcessor gatheringProcessor;

        private void Awake()
        {
            if (playerMover == null) playerMover = GetComponent<PlayerMover>();
            
            gatheringProcessor = new GatheringActionProcessor(playerSkills, inventory);
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

            playerMover.MoveTo(currentSpot.transform.position);

            // Option 1: simple polling to start AFK when close:
            if (afkRoutine != null)
                StopCoroutine(afkRoutine);
            afkRoutine = StartCoroutine(AfkWhenInRange());
        }

        private IEnumerator AfkWhenInRange()
        {
            var wait = new WaitForSeconds(0.1f);
            while (currentSpot != null)
            {
                if (Vector3.Distance(transform.position, currentSpot.transform.position) <= interactRange)
                {
                    StartAfk();
                    yield break;
                }

                yield return wait;
            }
        }

        private void StartAfk()
        {
            if (currentSpot == null)
                return;

            if (afkRoutine != null)
                StopCoroutine(afkRoutine);

            afkRoutine = StartCoroutine(AfkLoop());
        }

        private void StopAfk()
        {
            if (afkRoutine != null)
                StopCoroutine(afkRoutine);

            afkRoutine = null;
        }

        private IEnumerator AfkLoop()
        {
            var wait = new WaitForSeconds(actionInterval);

            while (currentSpot != null)
            {
                if (Vector3.Distance(transform.position, currentSpot.transform.position) > interactRange)
                    yield break;

                var action = currentSpot.Action as GatheringActionDefinition;
                if (action != null)
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
                case GatheringActionDefinition woodcutting:
                    gatheringProcessor.TryExecute(woodcutting);
                    break;
            }
        }
    }
}