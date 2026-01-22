using System;
using System.Collections.Generic;
using UnityEngine;

namespace Resource.Runtime
{
    /// <summary>
    /// Coordinates resource node interactions and enforces a maximum number of concurrent interactions.
    /// </summary>
    public sealed class ResourceInteractorCoordinator : MonoBehaviour
    {
        [SerializeField, Min(1)] private int maxConcurrentInteractions = 2;

        private readonly HashSet<ResourceNode> registeredNodes = new();
        private readonly List<ResourceNode> activeNodes = new(8);

        /// <summary>
        /// Raised when the active set changes (started or stopped).
        /// </summary>
        public event Action<ResourceNode> InteractionStarted;

        /// <summary>
        /// Raised when the active set changes (started or stopped).
        /// </summary>
        public event Action<ResourceNode> InteractionStopped;

        /// <summary>
        /// Registers a node to participate in coordinated interaction.
        /// </summary>
        public void Register(ResourceNode node)
        {
            if (node == null)
                return;

            registeredNodes.Add(node);
        }

        /// <summary>
        /// Unregisters a node and stops it if it is currently active.
        /// </summary>
        public void Unregister(ResourceNode node)
        {
            if (node == null)
                return;

            registeredNodes.Remove(node);

            if (!activeNodes.Contains(node))
                return;

            StopInternal(node);
        }

        /// <summary>
        /// Toggles interaction: if the node is active, stop it; otherwise start it.
        /// Enforces <see cref="maxConcurrentInteractions"/>.
        /// </summary>
        public void RequestToggle(ResourceNode node)
        {
            if (!CanConsider(node))
                return;

            if (activeNodes.Contains(node))
            {
                StopInternal(node);
                return;
            }

            RequestStart(node);
        }

        /// <summary>
        /// Requests starting interaction for a node, enforcing <see cref="maxConcurrentInteractions"/>.
        /// </summary>
        public void RequestStart(ResourceNode node)
        {
            if (!CanConsider(node))
                return;

            if (activeNodes.Contains(node))
                return;

            EnsureCapacity();

            activeNodes.Add(node);
            node.StartInteraction();
            InteractionStarted?.Invoke(node);
        }

        /// <summary>
        /// Requests stopping interaction for a node.
        /// </summary>
        public void RequestStop(ResourceNode node)
        {
            if (node == null)
                return;

            if (!activeNodes.Contains(node))
                return;

            StopInternal(node);
        }

        private bool CanConsider(ResourceNode node)
        {
            if (node == null)
                return false;

            if (!node.isActiveAndEnabled)
                return false;

            if (node.IsDepleted)
                return false;

            return registeredNodes.Contains(node);
        }

        private void EnsureCapacity()
        {
            while (activeNodes.Count >= maxConcurrentInteractions)
            {
                var oldest = activeNodes[0];
                StopInternal(oldest);
            }
        }

        private void StopInternal(ResourceNode node)
        {
            activeNodes.Remove(node);
            node.StopInteraction();
            InteractionStopped?.Invoke(node);
        }
    }
}
