using System;
using UnityEngine;

namespace Resource.Runtime
{
    /// <summary>
    /// Serializable runtime state for a resource node.
    /// </summary>
    [Serializable]
    public sealed class ResourceRuntimeState
    {
        [SerializeField, Min(0)] private int durabilityCurrent;

        /// <summary>
        /// Current durability remaining.
        /// </summary>
        public int DurabilityCurrent => durabilityCurrent;

        /// <summary>
        /// Sets the current durability.
        /// </summary>
        public void SetDurability(int value)
        {
            durabilityCurrent = Mathf.Max(0, value);
        }
    }
}