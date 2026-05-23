using System;
using ResourceAreas.Runtime;
using UnityEngine;

namespace ResourceAreas.State
{
    /// <summary>
    /// Serializable runtime progress for one resource area.
    /// </summary>
    [Serializable]
    public sealed class ResourceAreaProgressState
    {
        [SerializeField] private ResourceAreaId areaId;
        [SerializeField, Min(1)] private int level = 1;
        [SerializeField, Min(0f)] private float xp;
        [SerializeField, Min(1)] private int currentFloor = 1;

        /// <summary>
        /// Stable id of the resource area represented by this state.
        /// </summary>
        public ResourceAreaId AreaId => areaId;

        /// <summary>
        /// Current resource area level.
        /// </summary>
        public int Level => level;

        /// <summary>
        /// Current resource area experience toward the next level.
        /// </summary>
        public float Xp => xp;

        /// <summary>
        /// Current floor within this resource area.
        /// </summary>
        public int CurrentFloor => currentFloor;

        /// <summary>
        /// Creates an empty resource area progress state.
        /// </summary>
        public ResourceAreaProgressState()
        {
        }

        /// <summary>
        /// Creates resource area progress state for the given area id.
        /// </summary>
        public ResourceAreaProgressState(ResourceAreaId areaId)
        {
            this.areaId = areaId;
        }

        /// <summary>
        /// Normalizes this progress state after loading save data.
        /// </summary>
        public void Normalize()
        {
            level = Math.Max(1, level);
            xp = Math.Max(0f, xp);
            currentFloor = Math.Max(1, currentFloor);
        }

        /// <summary>
        /// Adds experience and applies simple level-up logic using the supplied requirement resolver.
        /// </summary>
        public void AddXp(float amount, Func<int, float> xpRequiredResolver)
        {
            if (amount <= 0f)
                return;

            xp += amount;

            if (xpRequiredResolver == null)
            {
                Normalize();
                return;
            }

            while (true)
            {
                var requiredXp = xpRequiredResolver(level);
                if (requiredXp <= 0f || xp < requiredXp)
                    break;

                xp -= requiredXp;
                level++;
            }

            Normalize();
        }
    }
}
