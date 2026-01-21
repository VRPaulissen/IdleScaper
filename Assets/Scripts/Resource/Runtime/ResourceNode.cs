using System;
using System.Collections;
using System.Collections.Generic;
using Inventory;
using Items.Runtime;
using Player;
using Resource.Definitions;
using UnityEngine;
using UnityEngine.EventSystems;
using Utilities.Calculations;
using Logger = Utilities.Logging.Logger;

namespace Resource.Runtime
{
    /// <summary>
    /// World resource node that can be interacted with and depleted.
    /// </summary>
    [RequireComponent(typeof(ResourceNodeView))]
    public sealed class ResourceNode : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private ResourceDefinition definition;

        [Header("Respawn")]
        [SerializeField] private bool respawns = true;
        [SerializeField, Min(0.1f)] private float respawnSeconds = 10f;

        private readonly ResourceRuntimeState state = new();

        private ResourceInteractor interactor;
        private IInventoryService inventoryService;
        private IResourceToolProvider toolProvider;

        private Coroutine interactionRoutine;
        private bool isDepleted;
        
        /// <summary>
        /// Raised when durability changes.
        /// </summary>
        public event Action<ResourceNode, int, int> DurabilityChanged;

        /// <summary>
        /// Raised when the node is depleted and rewards are rolled.
        /// </summary>
        public event Action<ResourceNode, IReadOnlyList<ItemInstance>> Depleted;
        
        /// <summary>
        /// Raised when the depleted state changes.
        /// </summary>
        public event Action<ResourceNode, bool> DepletionStateChanged;
        
        /// <summary>
        /// Raised when a respawn countdown starts.
        /// </summary>
        public event Action<ResourceNode, float> RespawnStarted;

        /// <summary>
        /// The linked definition of this resource node.
        /// </summary>
        public ResourceDefinition Definition => definition;
        
        /// <summary>
        /// True if the node is currently depleted.
        /// </summary>
        public bool IsDepleted => isDepleted;

        /// <summary>
        /// True if the node will respawn after depletion.
        /// </summary>
        public bool Respawns => respawns;

        /// <summary>
        /// Respawn delay in seconds.
        /// </summary>
        public float RespawnSeconds => respawnSeconds;
        
        /// <summary>
        /// Current durability of the resource.
        /// </summary>
        public int DurabilityCurrent => state.DurabilityCurrent;

        /// <summary>
        /// Maximum durability of the resource.
        /// </summary>
        public int DurabilityMax => definition.DurabilityMax;
        
        private void Awake()
        {
            if (definition == null)
            {
                Logger.LogError($"{nameof(ResourceNode)} on {name} is missing a definition.");
                enabled = false;
                return;
            }

            state.SetDurability(definition.DurabilityMax);
            interactor = new ResourceInteractor(new UnityRandomSource());
        }

        /// <summary>
        /// Injects runtime dependencies for rewards and tool selection.
        /// </summary>
        public void Initialize(IInventoryService inventory, IResourceToolProvider resourceToolProvider)
        {
            inventoryService = inventory;
            toolProvider = resourceToolProvider;
        }

        /// <summary>
        /// Handles pointer click to start interaction.
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            TryStartInteraction();
        }

        #region Interaction
        
        /// <summary>
        /// Starts the interaction loop if possible.
        /// </summary>
        private void TryStartInteraction()
        {
            if (!isActiveAndEnabled)
                return;

            if (isDepleted)
                return;

            if (interactionRoutine != null)
                return;

            if (toolProvider == null || !toolProvider.TryGetActiveTool(out _))
                return;

            interactionRoutine = StartCoroutine(InteractionLoop());
        }

        /// <summary>
        /// Stops the interaction loop.
        /// </summary>
        public void StopInteraction()
        {
            if (interactionRoutine == null)
                return;

            StopCoroutine(interactionRoutine);
            interactionRoutine = null;
        }
        
        private IEnumerator InteractionLoop()
        {
            while (!isDepleted)
            {
                if (toolProvider == null || !toolProvider.TryGetActiveTool(out var tool))
                {
                    interactionRoutine = null;
                    yield break;
                }

                yield return new WaitForSeconds(tool.HitIntervalSeconds);

                if (isDepleted)
                    yield break;

                var wasDepleted = interactor.ApplyHit(
                    definition,
                    state,
                    damageOverride: tool.DamagePerHit,
                    out var drops);

                DurabilityChanged?.Invoke(this, state.DurabilityCurrent, definition.DurabilityMax);

                if (!wasDepleted)
                    continue;

                HandleDepleted(drops);
                yield break;
            }
        }

        private void HandleDepleted(IReadOnlyList<ItemInstance> drops)
        {
            isDepleted = true;
            interactionRoutine = null;

            if (drops != null && drops.Count > 0)
            {
                for (var i = 0; i < drops.Count; i++)
                {
                    var drop = drops[i];
                    var add = inventoryService.TryAdd(drop.ItemId, drop.Quantity);

                    if (add.IsSuccess)
                        continue;

                    var addedQuantity = drop.Quantity - add.UnprocessedQuantity;
                    Logger.LogWarning(
                        $"{nameof(ResourceNode)} on {name} failed to add drop. " +
                        $"Item={drop.ItemId} Requested={drop.Quantity} Added={addedQuantity} NotAdded={add.UnprocessedQuantity} " +
                        $"Code={add.Code} Message='{add.Message}'");
                }
            }

            Depleted?.Invoke(this, drops);
            DepletionStateChanged?.Invoke(this, isDepleted);
            
            if (!respawns)
                return;

            StartCoroutine(RespawnAfterDelay());
        }

        private IEnumerator RespawnAfterDelay()
        {
            RespawnStarted?.Invoke(this, respawnSeconds);
            
            yield return new WaitForSeconds(respawnSeconds);

            isDepleted = false;
            state.SetDurability(definition.DurabilityMax);
            DurabilityChanged?.Invoke(this, state.DurabilityCurrent, definition.DurabilityMax);
            DepletionStateChanged?.Invoke(this, isDepleted);
        }
        
        #endregion
    }
}
