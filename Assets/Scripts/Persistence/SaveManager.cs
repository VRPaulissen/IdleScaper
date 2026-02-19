using System;
using System.Collections.Generic;
using IdleScaper.Persistence.Core;
using IdleScaper.Persistence.Integrity;
using UnityEngine;

namespace IdleScaper.Persistence
{
    /// <summary>
    /// Coordinates loading/saving, optional integrity checks, and migration steps.
    /// </summary>
    public sealed class SaveManager
    {
        /// <summary>Raised after a successful load (including newly created saves).</summary>
        public event Action<SaveData> Loaded;

        /// <summary>Raised after a successful write to storage.</summary>
        public event Action<SaveData> Saved;

        /// <summary>Current in-memory save payload.</summary>
        public SaveData Data { get; private set; }

        private readonly ISaveStorage storage;
        private readonly ISaveSerializer serializer;
        private readonly ISaveIntegrity integrity;
        private readonly Dictionary<int, ISaveMigration> migrationsByFromVersion;

        private bool isDirty;

        /// <summary>
        /// Creates a save manager.
        /// </summary>
        public SaveManager(
            ISaveStorage storage,
            ISaveSerializer serializer,
            ISaveIntegrity integrity,
            IReadOnlyList<ISaveMigration> migrations)
        {
            this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
            this.serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            this.integrity = integrity ?? throw new ArgumentNullException(nameof(integrity));

            migrationsByFromVersion = new Dictionary<int, ISaveMigration>(16);
            if (migrations == null) return;
            
            foreach (var mig in migrations)
            {
                if (mig == null)
                    continue;

                migrationsByFromVersion[mig.FromVersion] = mig;
            }
        }

        /// <summary>
        /// Marks the save as needing a write. 
        /// </summary>
        public void MarkDirty()
        {
            isDirty = true;
        }

        /// <summary>
        /// Loads from primary, then backup, else creates a new save. Applies integrity checks and migrations.
        /// </summary>
        public void LoadOrCreate()
        {
            if (TryLoadFromStorage(primary: true, out var loaded))
            {
                Data = loaded;
                Loaded?.Invoke(Data);
                return;
            }

            if (TryLoadFromStorage(primary: false, out loaded))
            {
                Data = loaded;
                ForceSave();
                Loaded?.Invoke(Data);
                return;
            }

            Data = CreateNew();
            
            Data.Inventory ??= new Inventory.InventoryState();
            Data.Inventory.EnsureSize(100);
            
            ForceSave();
            Loaded?.Invoke(Data);
        }

        /// <summary>
        /// Saves if the save is dirty, otherwise does nothing.
        /// </summary>
        public void SaveIfDirty()
        {
            if (!isDirty)
                return;

            ForceSave();
        }

        /// <summary>
        /// Forces a save write even if not dirty.
        /// </summary>
        public void ForceSave()
        {
            Data ??= CreateNew();
            Data.LastSavedUtcTicks = DateTime.UtcNow.Ticks;

            // Sign payload without Signature included.
            var payload = SerializeWithoutSignature(Data);
            Data.Signature = integrity?.ComputeSignature(payload);

            var finalText = serializer.Serialize(Data);

            var ok = storage.TryWriteAtomic(finalText);
            if (ok)
            {
                isDirty = false;
                Saved?.Invoke(Data);
            }
        }

        /// <summary>
        /// Applies offline progress based on time elapsed since last save (clamped).
        /// Call after LoadOrCreate() and before gameplay begins.
        /// </summary>
        public void ApplyOfflineProgress(TimeSpan maxOffline, Func<SaveData, double> getIncomePerSecond)
        {
            if (Data == null)
                return;

            var nowTicks = DateTime.UtcNow.Ticks;
            var lastTicks = Data.LastSavedUtcTicks;

            if (lastTicks <= 0 || lastTicks > nowTicks)
            {
                Data.LastSavedUtcTicks = nowTicks;
                return;
            }

            var delta = new TimeSpan(nowTicks - lastTicks);
            if (delta <= TimeSpan.Zero)
                return;

            if (delta > maxOffline)
                delta = maxOffline;

            // Add stuff here that need to be calculated.
            Data.LastSavedUtcTicks = nowTicks;
        }

        private bool TryLoadFromStorage(bool primary, out SaveData data)
        {
            data = null;

            var hasText = primary ? storage.TryRead(out var text) : storage.TryReadBackup(out text);
            if (!hasText)
                return false;

            if (!serializer.TryDeserialize(text, out var candidate))
                return false;

            if (!VerifyIntegrity(candidate))
                return false;

            candidate = MigrateToLatest(candidate);
            data = candidate;
            isDirty = false;
            return true;
        }

        private bool VerifyIntegrity(SaveData candidate)
        {
            if (candidate == null)
                return false;

            if (integrity == null)
                return true;

            var storedSig = candidate.Signature;

            // Verify payload without signature.
            candidate.Signature = null;
            var payload = serializer.Serialize(candidate);
            candidate.Signature = storedSig;

            return integrity.VerifySignature(payload, storedSig);
        }

        private SaveData MigrateToLatest(SaveData data)
        {
            if (data == null)
                return CreateNew();

            // Apply chained migrations while we find steps.
            var guard = 64;
            while (guard-- > 0 && migrationsByFromVersion.TryGetValue(data.SaveVersion, out var mig))
            {
                if (mig == null)
                    break;

                mig.Migrate(data);
                data.SaveVersion = mig.ToVersion;
                isDirty = true;
            }

            if (guard <= 0)
            {
                Debug.LogWarning("[SaveManager] Migration loop guard hit. Check migration chain.");
            }

            if (isDirty)
                ForceSave();

            return data;
        }

        private SaveData CreateNew()
        {
            return new SaveData
            {
                SaveVersion = GetLatestVersion(),
                LastSavedUtcTicks = DateTime.UtcNow.Ticks,
                Signature = null
            };
        }

        private int GetLatestVersion()
        {
            var latest = 1;
            foreach (var kv in migrationsByFromVersion)
            {
                var mig = kv.Value;
                if (mig != null && mig.ToVersion > latest)
                {
                    latest = mig.ToVersion;
                }
            }
            return latest;
        }

        private string SerializeWithoutSignature(SaveData data)
        {
            var stored = data.Signature;
            data.Signature = null;
            var payload = serializer.Serialize(data);
            data.Signature = stored;
            return payload;
        }
    }
}