using Stunlock.Core;
using System;
using System.Collections.Generic;

namespace KindredSchematics.Data;
internal static class Tile
{
    public static void Populate()
    {
        foreach (var (prefabGuid, name) in Core.PrefabCollection.PrefabGuidToNameDictionary)
        {
            if (!name.StartsWith("TM_")) continue;

            // Verify the prefab actually exists
            if (!Core.PrefabCollection._PrefabGuidToEntityMap.TryGetValue(prefabGuid, out var _) &&
                !Core.PrefabCollection._PrefabLookupMap.TryGetValueWithoutLogging(prefabGuid, out var _))
            {
                continue;
            }

            Named[name] = prefabGuid;
            NameFromPrefab[prefabGuid.GuidHash] = name;
            LowerCaseNameToPrefab[name.ToLower()] = prefabGuid;
        }

        foreach (var (name, prefabGuid) in Core.PrefabCollection._SpawnableNameToPrefabGuidDictionary)
        {
            if (!name.StartsWith("TM_")) continue;

            // Verify the prefab actually exists
            if (!Core.PrefabCollection._PrefabGuidToEntityMap.TryGetValue(prefabGuid, out var _) &&
                !Core.PrefabCollection._PrefabLookupMap.TryGetValueWithoutLogging(prefabGuid, out var _))
            {
                continue;
            }

            Named[name] = prefabGuid;
            NameFromPrefab[prefabGuid.GuidHash] = name;
            LowerCaseNameToPrefab[name.ToLower()] = prefabGuid;
        }
    }

    public static Dictionary<string, PrefabGUID> Named = new Dictionary<string, PrefabGUID>(StringComparer.OrdinalIgnoreCase);
    
    public static Dictionary<int, string> NameFromPrefab = [];
    public static Dictionary<string, PrefabGUID> LowerCaseNameToPrefab = [];

}
