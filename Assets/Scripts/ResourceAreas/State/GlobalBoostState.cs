using System;
using ResourceAreas.Runtime;
using UnityEngine;

namespace ResourceAreas.State
{
    /// <summary>
    /// Serializable runtime state for one active or owned global resource boost.
    /// </summary>
    [Serializable]
    public sealed class GlobalBoostState
    {
        [SerializeField] private GlobalBoostId boostId;
        [SerializeField] private bool isActive;
        [SerializeField, Min(1)] private int stackCount = 1;
        [SerializeField] private long startedAtUtcTicks;
        [SerializeField] private long endsAtUtcTicks;

        /// <summary>
        /// Stable id of the global boost represented by this state.
        /// </summary>
        public GlobalBoostId BoostId => boostId;

        /// <summary>
        /// Returns true when this boost is marked active.
        /// </summary>
        public bool IsActive => isActive;

        /// <summary>
        /// Stack count for this boost.
        /// </summary>
        public int StackCount => stackCount;

        /// <summary>
        /// UTC ticks when this boost started.
        /// </summary>
        public long StartedAtUtcTicks => startedAtUtcTicks;

        /// <summary>
        /// UTC ticks when this boost ends.
        /// </summary>
        public long EndsAtUtcTicks => endsAtUtcTicks;

        /// <summary>
        /// Returns true when this boost has a configured end time.
        /// </summary>
        public bool HasEndTime => endsAtUtcTicks > 0;

        /// <summary>
        /// Creates an empty global boost state.
        /// </summary>
        public GlobalBoostState()
        {
        }

        /// <summary>
        /// Creates global boost state for the given boost id.
        /// </summary>
        public GlobalBoostState(GlobalBoostId boostId)
        {
            this.boostId = boostId;
        }

        /// <summary>
        /// Normalizes this boost state after loading save data.
        /// </summary>
        public void Normalize()
        {
            if (isActive && stackCount < 1)
                stackCount = 1;

            if (stackCount < 0)
                stackCount = 0;

            if (startedAtUtcTicks < 0)
                startedAtUtcTicks = 0;

            if (endsAtUtcTicks < 0)
                endsAtUtcTicks = 0;
        }

        /// <summary>
        /// Sets whether this boost is active.
        /// </summary>
        public void SetActive(bool active)
        {
            isActive = active;
            Normalize();
        }

        /// <summary>
        /// Sets the boost stack count.
        /// </summary>
        public void SetStackCount(int count)
        {
            stackCount = count;
            Normalize();
        }

        /// <summary>
        /// Sets the UTC timing window for this boost.
        /// </summary>
        public void SetTiming(long startedAtUtcTicks, long endsAtUtcTicks)
        {
            this.startedAtUtcTicks = Math.Max(0, startedAtUtcTicks);
            this.endsAtUtcTicks = Math.Max(0, endsAtUtcTicks);
        }

        /// <summary>
        /// Clears the UTC timing window for this boost.
        /// </summary>
        public void ClearTiming()
        {
            startedAtUtcTicks = 0;
            endsAtUtcTicks = 0;
        }

        /// <summary>
        /// Returns true when this boost is active at the given UTC time.
        /// </summary>
        public bool IsCurrentlyActive(DateTime utcNow)
        {
            if (!isActive)
                return false;

            if (!HasEndTime)
                return true;

            return utcNow.Ticks < endsAtUtcTicks;
        }
    }
}
