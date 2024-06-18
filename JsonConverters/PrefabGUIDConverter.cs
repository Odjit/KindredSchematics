using ProjectM;
using Stunlock.Core;
using System;
using System.Text.Json;

namespace KindredSchematics.JsonConverters
{
    public class PrefabGUIDConverter : System.Text.Json.Serialization.JsonConverter<PrefabGUID>
    {
        public override PrefabGUID Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException("PrefabGUID should be an object");
            }

            if (reader.TokenType == JsonTokenType.Null)
                return PrefabGUID.Empty;

            var prefabName = reader.GetString();

            if (!Core.PrefabCollection.NameToPrefabGuidDictionary.TryGetValue(prefabName, out var prefabGUID))
                return new PrefabGUID(0);
                //throw new JsonException($"Invalid PrefabGUID name {prefabName}");
            return prefabGUID;
        }

        public override void Write(Utf8JsonWriter writer, PrefabGUID value, JsonSerializerOptions options)
        {
            if (!Core.PrefabCollection._PrefabGuidToNameDictionary.TryGetValue(value, out var prefabName))
                writer.WriteNullValue();
            else
                writer.WriteStringValue(prefabName);
        }
    }
}
