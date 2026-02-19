namespace IdleScaper.Persistence.Core
{
    /// <summary>
    /// Defines serialization behavior for save payloads.
    /// </summary>
    public interface ISaveSerializer
    {
        /// <summary>Serializes a save payload to text.</summary>
        string Serialize(SaveData data);

        /// <summary>Deserializes text to a save payload.</summary>
        bool TryDeserialize(string text, out SaveData data);
    }
}