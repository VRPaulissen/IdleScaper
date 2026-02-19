namespace IdleScaper.Persistence.Core
{
    /// <summary>
    /// Defines a migration step from older schema versions to newer ones.
    /// </summary>
    public interface ISaveMigration
    {
        /// <summary>The version this migrator expects as input.</summary>
        int FromVersion { get; }

        /// <summary>The version this migrator outputs.</summary>
        int ToVersion { get; }

        /// <summary>Applies migration in-place.</summary>
        void Migrate(SaveData data);
    }
}