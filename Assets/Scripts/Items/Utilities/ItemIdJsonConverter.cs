using System;
using Items.Runtime;
using Newtonsoft.Json;

namespace IdleScaper.Persistence.Newtonsoft
{
    /// <summary>
    /// Serializes <see cref="ItemId"/> as a single JSON string value.
    /// </summary>
    public sealed class ItemIdJsonConverter : JsonConverter<ItemId>
    {
        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, ItemId value, JsonSerializer serializer)
        {
            writer.WriteValue(value.Value);
        }

        /// <inheritdoc/>
        public override ItemId ReadJson(
            JsonReader reader,
            Type objectType,
            ItemId existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return default;

            if (reader.TokenType == JsonToken.String)
                return new ItemId((string)reader.Value);

            if (reader.TokenType == JsonToken.StartObject)
            {
                var obj = serializer.Deserialize<LegacyItemId>(reader);
                return new ItemId(obj?.value);
            }

            throw new JsonSerializationException($"Unsupported token {reader.TokenType} for ItemId.");
        }

        [Serializable]
        private sealed class LegacyItemId
        {
            public string value;
        }
    }
}