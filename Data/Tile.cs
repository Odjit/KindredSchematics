using ProjectM;
using ProjectM.CastleBuilding;
using Stunlock.Core;
using System;
using System.Collections.Generic;
using Unity.Entities;

namespace KindredSchematics.Data;
internal static class Tile
{
    public static void Populate()
    {
        foreach (var (name, prefabGuid) in Core.PrefabCollection.SpawnableNameToPrefabGuidDictionary)
        {
            if (!name.StartsWith("TM_")) continue;

            // Verify the prefab actually exists
            var prefab2 = Entity.Null;
            if (!Core.PrefabCollection._PrefabGuidToEntityMap.TryGetValue(prefabGuid, out var prefab1) &&
                !Core.PrefabCollection._PrefabLookupMap.TryGetValueWithoutLogging(prefabGuid, out prefab2))
            {
                continue;
            }

            if (prefab1 != Entity.Null && prefab1.Has<TransitionWhenInventoryIsEmpty>() ||
                prefab2 != Entity.Null && prefab2.Has<TransitionWhenInventoryIsEmpty>())
                continue;

            if (prefab1 != Entity.Null && prefab1.Has<CastleHeart>() ||
                prefab2 != Entity.Null && prefab2.Has<CastleHeart>())
                continue;

            Named[name] = prefabGuid;
            NameFromPrefab[prefabGuid.GuidHash] = name;
            LowerCaseNameToPrefab[name.ToLower()] = prefabGuid;
        }

        foreach (var (name, prefabGuid) in Core.PrefabCollection._SpawnableNameToPrefabGuidDictionary)
        {
            if (!name.StartsWith("TM_")) continue;

            // Verify the prefab actually exists
            var prefab2 = Entity.Null;
            if (!Core.PrefabCollection._PrefabGuidToEntityMap.TryGetValue(prefabGuid, out var prefab1) &&
                !Core.PrefabCollection._PrefabLookupMap.TryGetValueWithoutLogging(prefabGuid, out prefab2))
            {
                continue;
            }

            if (prefab1 != Entity.Null && prefab1.Has<TransitionWhenInventoryIsEmpty>() ||
                prefab2 != Entity.Null && prefab2.Has<TransitionWhenInventoryIsEmpty>())
                continue;

            if (prefab1 != Entity.Null && prefab1.Has<CastleHeart>() ||
                prefab2 != Entity.Null && prefab2.Has<CastleHeart>())
                continue;

            Named[name] = prefabGuid;
            NameFromPrefab[prefabGuid.GuidHash] = name;
            LowerCaseNameToPrefab[name.ToLower()] = prefabGuid;
        }
    }

    public static Dictionary<string, PrefabGUID> Named = new Dictionary<string, PrefabGUID>(StringComparer.OrdinalIgnoreCase);
    
    public static Dictionary<int, string> NameFromPrefab = [];
    public static Dictionary<string, PrefabGUID> LowerCaseNameToPrefab = [];

}
