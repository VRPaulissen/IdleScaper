using System;
using ResourceAreas.Runtime;
using UnityEngine;

namespace ResourceAreas.State
{
    /// <summary>
    /// Serializable runtime progress for one harvestable resource.
    /// </summary>
    [Serializable]
    public sealed class ResourceProgressState
    {
        [SerializeField] private ResourceId resourceId;
        [SerializeField, Min(1)] private int level = 1;
        [SerializeField, Min(0f)] private float xp;

        /// <summary>
        /// Stable id of the resource represented by this state.
        /// </summary>
        public ResourceId ResourceId => resourceId;

        /// <summary>
        /// Current resource level.
        /// </summary>
        public int Level => level;

        /// <summary>
        /// Current resource experience toward the next level.
        /// </summary>
        public float Xp => xp;

        /// <summary>
        /// Creates an empty resource progress state.
        /// </summary>
        public ResourceProgressState()
        {
        }

        /// <summary>
        /// Creates resource progress state for the given resource id.
        /// </summary>
        public ResourceProgressState(ResourceId resourceId)
        {
            this.resourceId = resourceId;
        }

        /// <summary>
        /// Normalizes this progress state after loading save data.
        /// </summary>
        public void Normalize()
        {
            level = Math.Max(1, level);
            xp = Math.Max(0f, xp);
        }

        /// <summary>
        /// Sets level and experience to normalized values.
        /// </summary>
        public void SetProgress(int level, float xp)
        {
            this.level = Math.Max(1, level);
            this.xp = Math.Max(0f, xp);
        }
    }
}
