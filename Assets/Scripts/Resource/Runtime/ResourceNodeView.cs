using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Logger = Utilities.Logging.Logger;

namespace Resource.Runtime
{
    /// <summary>
    /// UI-based view for a <see cref="ResourceNode"/>.
    /// Drives alive/depleted visuals, durability bar, and respawn pie countdown.
    /// </summary>
    [RequireComponent(typeof(ResourceNode))]
    public class ResourceNodeView : MonoBehaviour
    {
        [Header("Visuals")]
        [SerializeField] private Image graphic;

        [Header("Durability UI")]
        [SerializeField] private GameObject durabilityRoot;
        [SerializeField] private Image durabilityFill;

        [Header("Respawn UI")]
        [SerializeField] private GameObject respawnRoot;
        [SerializeField] private Image respawnPieFill;

        private ResourceNode node;
        private Coroutine respawnRoutine;

        private void Awake()
        {
            node = GetComponent<ResourceNode>();

            if (durabilityFill == null)
            {
                Logger.LogError($"{nameof(ResourceNodeView)} on {name} is missing {nameof(durabilityFill)}.", this);
                enabled = false;
                return;
            }

            if (respawnPieFill == null)
            {
                Logger.LogError($"{nameof(ResourceNodeView)} on {name} is missing {nameof(respawnPieFill)}.", this);
                enabled = false;
                return;
            }
        }

        private void OnEnable()
        {
            if (node == null)
                return;

            node.DurabilityChanged     += HandleDurabilityChanged;
            node.DepletionStateChanged += HandleDepletionStateChanged;
            node.RespawnStarted        += HandleRespawnStarted;

            // Initialize view state (important for pooling / scene load).
            HandleDepletionStateChanged(node, node.IsDepleted);

            if (!node.IsDepleted)
            {
                // Force initial bar update if you want deterministic visuals.
                HandleDurabilityChanged(node, node.DurabilityCurrent, node.DurabilityMax);
            }
        }

        private void OnDisable()
        {
            if (node == null)
                return;
            
            node.DurabilityChanged     -= HandleDurabilityChanged;
            node.DepletionStateChanged -= HandleDepletionStateChanged;
            node.RespawnStarted        -= HandleRespawnStarted;

            StopRespawnRoutine();
        }

        private void HandleDurabilityChanged(ResourceNode resourceNode, int current, int max)
        {
            if (max <= 0)
            {
                durabilityFill.fillAmount = 0f;
                return;
            }

            var value = Mathf.Clamp01((float)current / max);
            durabilityFill.fillAmount = value;
        }

        private void HandleDepletionStateChanged(ResourceNode resourceNode, bool isDepleted)
        {
            durabilityRoot?.SetActive(!isDepleted);
            respawnRoot?.SetActive(isDepleted && resourceNode.Respawns);

            graphic.sprite = isDepleted ? 
                resourceNode.Definition.DepletedSprite : 
                resourceNode.Definition.AliveSprite;
            
            if (!isDepleted)
            {
                StopRespawnRoutine();
                respawnPieFill.fillAmount = 0f;

                // When alive again, ensure durability bar reflects the new durability.
                HandleDurabilityChanged(resourceNode, resourceNode.DurabilityCurrent, resourceNode.DurabilityMax);
            }
        }

        private void HandleRespawnStarted(ResourceNode resourceNode, float durationSeconds)
        {
            if (!resourceNode.Respawns)
                return;

            respawnRoot?.SetActive(true);
            StartRespawnRoutine(durationSeconds);
        }

        private void StartRespawnRoutine(float durationSeconds)
        {
            StopRespawnRoutine();

            if (durationSeconds <= 0f)
            {
                respawnPieFill.fillAmount = 1f;
                return;
            }

            respawnRoutine = StartCoroutine(RespawnPieRoutine(durationSeconds));
        }

        private void StopRespawnRoutine()
        {
            if (respawnRoutine == null)
                return;

            StopCoroutine(respawnRoutine);
            respawnRoutine = null;
        }

        private IEnumerator RespawnPieRoutine(float durationSeconds)
        {
            // Fill from 0 -> 1 over the respawn duration.
            var elapsed = 0f;
            respawnPieFill.fillAmount = 0f;

            while (elapsed < durationSeconds)
            {
                elapsed += Time.deltaTime;
                respawnPieFill.fillAmount = Mathf.Clamp01(elapsed / durationSeconds);
                yield return null;
            }

            respawnPieFill.fillAmount = 1f;
            respawnRoutine = null;
        }
    }
}