using System.Collections.Generic;
using IdleScaper.Persistence;
using IdleScaper.Persistence.Core;
using IdleScaper.Persistence.Integrity;
using Items.Runtime;
using UnityEngine;

namespace IdleScaper.Bootstrap
{
/// <summary>
    /// Boots the save system and keeps it alive across scenes.
    /// </summary>
    public sealed class SaveBootstrap : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private SaveAutosaveDriver autosaveDriver;
        [SerializeField] private ItemDatabase itemDatabase;

        private SaveManager saveManager;
        private const string TEMP_SECRET = "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            // Storage + serializer
            var storage = new FileSaveStorage();
            var serializer = new JsonSaveSerializer();
            var integrity = new HmacSha256Integrity(TEMP_SECRET);

            var migrations = new List<ISaveMigration>();

            saveManager = new SaveManager(storage, serializer, integrity, migrations);
            saveManager.LoadOrCreate(itemDatabase);
            autosaveDriver.Initialize(saveManager);
            autosaveDriver.ApplyOfflineProgress(GetIncomePerSecond);
            saveManager.SaveIfDirty();
        }

        /// <summary>
        /// Example income function for offline progress. Replace with your economy logic.
        /// </summary>
        private double GetIncomePerSecond(SaveData data)
        {
            return 1.0;
        }

        /// <summary>
        /// Call this when you change persistent state, e.g. after purchases or upgrades.
        /// </summary>
        public void MarkSaveDirty()
        {
            saveManager.MarkDirty();
        }

        /// <summary>
        /// Exposes current save for gameplay systems (prefer passing references, not statics).
        /// </summary>
        public SaveData GetSaveData()
        {
            return saveManager.Data;
        }
    }
}
