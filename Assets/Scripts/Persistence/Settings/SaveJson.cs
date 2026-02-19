using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace IdleScaper.Persistence.Newtonsoft
{
    /// <summary>
    /// Centralized JSON serialization settings for saves.
    /// </summary>
    public static class SaveJson
    {
        public static readonly JsonSerializerSettings Settings = Create();

        private static JsonSerializerSettings Create()
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                TypeNameHandling = TypeNameHandling.None,
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                }
            };

            settings.Converters.Add(new ItemIdJsonConverter());
            return settings;
        }
    }
}