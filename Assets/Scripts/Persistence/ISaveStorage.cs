namespace IdleScaper.Persistence.Core
{
    /// <summary>
    /// Defines storage behavior for save data (local file now, cloud later).
    /// </summary>
    public interface ISaveStorage
    {
        /// <summary>Attempts to read the primary save text.</summary>
        bool TryRead(out string text);

        /// <summary>Attempts to read the backup save text.</summary>
        bool TryReadBackup(out string text);

        /// <summary>Writes save text atomically and maintains a backup of the previous save.</summary>
        bool TryWriteAtomic(string text);

        /// <summary>Deletes primary and backup saves (best effort).</summary>
        void DeleteAll();
    }
}