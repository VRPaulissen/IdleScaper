using System;
using IdleScaper.Persistence.Core;
using UnityEngine;

namespace IdleScaper.Persistence
{
/// <summary>
    /// Mobile-friendly autosave driver: saves on pause/focus loss and on interval when dirty.
    /// </summary>
    public sealed class SaveAutosaveDriver : MonoBehaviour
    {
        [Header("Timing")]
        [SerializeField] private float autosaveIntervalSeconds = 30f;

        [Header("Offline Progress")]
        [SerializeField] private int maxOfflineHours = 12;

        private SaveManager saveManager;
        private float nextAutosaveTime;

        /// <summary>
        /// Initializes the driver with a save manager instance.
        /// </summary>
        public void Initialize(SaveManager saveManager)
        {
            this.saveManager = saveManager ?? throw new ArgumentNullException(nameof(saveManager));
            nextAutosaveTime = Time.unscaledTime + autosaveIntervalSeconds;
        }

        private void Update()
        {
            if (saveManager == null)
                return;

            if (autosaveIntervalSeconds <= 0.0f)
                return;

            if (Time.unscaledTime < nextAutosaveTime)
                return;

            nextAutosaveTime = Time.unscaledTime + autosaveIntervalSeconds;
            saveManager.SaveIfDirty();
        }

        private void OnApplicationPause(bool pause)
        {
            if (saveManager == null)
                return;

            if (pause)
                saveManager.SaveIfDirty();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (saveManager == null)
                return;

            if (!hasFocus)
                saveManager.SaveIfDirty();
        }

        private void OnDestroy()
        {
            if (saveManager == null)
                return;

            saveManager.SaveIfDirty();
        }

        /// <summary>
        /// Applies offline progress using the manager's helper. You supply the income function.
        /// </summary>
        public void ApplyOfflineProgress(Func<SaveData, double> getIncomePerSecond)
        {
            if (saveManager == null)
                return;

            var maxOffline = TimeSpan.FromHours(Mathf.Max(0, maxOfflineHours));
            saveManager.ApplyOfflineProgress(maxOffline, getIncomePerSecond);
        }
    }
}