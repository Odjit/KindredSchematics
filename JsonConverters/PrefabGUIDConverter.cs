using ProjectM;
using Stunlock.Core;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace KindredSchematics.JsonConverters
{
    public class PrefabGUIDConverter : System.Text.Json.Serialization.JsonConverter<PrefabGUID>
    {
        static Dictionary<string, PrefabGUID> prefabNameToGuid;
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

            if (!GetPrefabGuid(prefabName, out var prefabGUID))
            {
                Core.Log.LogWarning($"Couldn't find {prefabName}");
                missingPrefabs.Add(prefabName);
                return new PrefabGUID(0);
            }

            return prefabGUID;
        }

        bool GetPrefabGuid(string prefabName, out PrefabGUID prefabGUID)
        {
            if (Core.PrefabCollection.SpawnableNameToPrefabGuidDictionary.TryGetValue(prefabName, out prefabGUID)) return true;
            
            if (prefabNameToGuid == null)
            {
                prefabNameToGuid = new(StringComparer.InvariantCultureIgnoreCase);
                foreach (var kvp in Core.PrefabCollection._PrefabGuidToEntityMap)
                {
                    if (!Core.PrefabCollection._PrefabLookupMap.TryGetName(kvp.Key, out var name)) continue;
                    if (Core.PrefabCollection.SpawnableNameToPrefabGuidDictionary.TryGetValue(name, out var _)) continue;
                    prefabNameToGuid[name] = kvp.Key;
                }
            }

            if (prefabNameToGuid.TryGetValue(prefabName, out prefabGUID)) return true;
            return false;
        }

        public override void Write(Utf8JsonWriter writer, PrefabGUID value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(Core.PrefabCollection._PrefabLookupMap.GetName(value));
        }
    }
}
