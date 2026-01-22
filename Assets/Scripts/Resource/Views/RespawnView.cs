using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Logger = Utilities.Logging.Logger;

namespace Resource.Runtime
{
    /// <summary>
    /// Generic respawn timer handler for various UI nodes.
    /// </summary>
    public class RespawnView : MonoBehaviour
    {
        private Image respawnFill;
        private GameObject respawnRoot;
        
        private ResourceNode node;
        private Coroutine respawnRoutine;
        
        private void Awake()
        {
            node = GetComponentInParent<ResourceNode>();

            respawnRoot = gameObject;
            respawnFill = GetComponent<Image>();
            
            if (respawnFill == null)
            {
                Logger.LogError($"{nameof(ResourceNodeView)} on {name} is missing {nameof(respawnFill)}.", this);
                enabled = false;
            }
     
            if (node == null)
                return;

            node.DepletionStateChanged += HandleDepletionStateChanged;
            node.RespawnStarted        += HandleRespawnStarted;

            HandleDepletionStateChanged(node, node.IsDepleted);
        }

        private void OnEnable()
        {
            HandleDepletionStateChanged(node, node.IsDepleted);
        }

        private void OnDisable()
        {
            StopRespawnRoutine();
        }

        private void OnDestroy()
        {
            if (node == null)
                return;
            
            node.DepletionStateChanged -= HandleDepletionStateChanged;
            node.RespawnStarted        -= HandleRespawnStarted;

            StopRespawnRoutine();
        }
        
        private void HandleDepletionStateChanged(ResourceNode resourceNode, bool isDepleted)
        {
            respawnRoot?.SetActive(isDepleted && resourceNode.Respawns);
            
            if (!isDepleted)
            {
                StopRespawnRoutine();
                respawnFill.fillAmount = 0f;
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
                respawnFill.fillAmount = 1f;
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
            var elapsed = 0f;
            respawnFill.fillAmount = 0f;

            while (elapsed < durationSeconds)
            {
                elapsed += Time.deltaTime;
                respawnFill.fillAmount = Mathf.Clamp01(elapsed / durationSeconds);
                yield return null;
            }

            respawnFill.fillAmount = 1f;
            respawnRoutine = null;
        }
    }
}
