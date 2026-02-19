using IdleScaper.Persistence.Core;
using Newtonsoft.Json;

namespace IdleScaper.Persistence
{
    /// <summary>
    /// JSON serializer using Unity's JsonUtility (fast, IL2CPP-friendly, limited types).
    /// </summary>
    public sealed class JsonSaveSerializer : ISaveSerializer
    {
        /// <inheritdoc />
        public string Serialize(SaveData data)
        {
            return JsonConvert.SerializeObject(data);
        }

        /// <inheritdoc />
        public bool TryDeserialize(string text, out SaveData data)
        {
            data = null;

            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            try
            {
                data = JsonConvert.DeserializeObject<SaveData>(text);
                return data != null;
            }
            catch
            {
                data = null;
                return false;
            }
        }
    }
}