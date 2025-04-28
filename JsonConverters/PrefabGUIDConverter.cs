using Stunlock.Core;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace KindredSchematics.JsonConverters
{
    public class PrefabGUIDConverter : System.Text.Json.Serialization.JsonConverter<PrefabGUID>
    {
        public static readonly HashSet<string> missingPrefabs = [];
        public override PrefabGUID Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException("PrefabGUID should be an object");
            }

            if (reader.TokenType == JsonTokenType.Null)
                return PrefabGUID.Empty;

            var prefabName = reader.GetString();
            prefabName = Core.PrefabRemap.GetPrefabMapping(prefabName);

            if (!Core.PrefabCollection.SpawnableNameToPrefabGuidDictionary.TryGetValue(prefabName, out var prefabGUID))
            {
                missingPrefabs.Add(prefabName);
                return new PrefabGUID(0);
            }

            return prefabGUID;
        }

        public override void Write(Utf8JsonWriter writer, PrefabGUID value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(Core.PrefabCollection._PrefabLookupMap.GetName(value));
        }
    }
}
