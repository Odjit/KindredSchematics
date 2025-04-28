using Il2CppInterop.Runtime;
using ProjectM;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace KindredSchematics.Services;
class RespawnPreventionService
{
    bool preventRespawns => preventRespawnsCount > 0;
    int preventRespawnsCount = 0;
    Coroutine respawnPreventionCoroutine;

    public RespawnPreventionService()
    {
        LoadCorrectRespawnData();
    }

    public void PreventRespawns()
    {
        preventRespawnsCount++;
        if (respawnPreventionCoroutine == null)
            respawnPreventionCoroutine = Core.StartCoroutine(KeepFromRespawning());
    }

    public void AllowRespawns()
    {
        preventRespawnsCount--;
        if (preventRespawnsCount < 0)
            preventRespawnsCount = 0;
    }
    void LoadCorrectRespawnData()
    {
        var DataRegex = new Regex(
        @"float3\((-?\d+\.?\d*(?:[eE][-+]?\d+)?f?),\s*(-?\d+\.?\d*(?:[eE][-+]?\d+)?f?),\s*(-?\d+\.?\d*(?:[eE][-+]?\d+)?f?)\),\s*(\d+),\s*(\d+)",
        RegexOptions.Compiled);
        var resourceName = "KindredSchematics.Data.SpawnRegions.txt";
        var assembly = Assembly.GetExecutingAssembly();
        var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null) return;

        using var reader = new StreamReader(stream);
        var buffList = reader.ReadToEnd();
        var posToRespawnTime = new List<(float3, int, int)>();

        // Load data
        foreach (var line in buffList.Split('\n'))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var match = DataRegex.Match(line);
            if (!match.Success)
            {
                Core.Log.LogWarning($"Failed to parse line: {line}");
                continue;
            }

            var x = ParseFloat(match.Groups[1].Value);
            var y = ParseFloat(match.Groups[2].Value);
            var z = ParseFloat(match.Groups[3].Value);
            var pos = new float3(x, y, z);

            var minRespawnTime = int.Parse(match.Groups[4].Value);
            var maxRespawnTime = int.Parse(match.Groups[5].Value);

            posToRespawnTime.Add((pos, minRespawnTime, maxRespawnTime));
        }

        var numberRepaired = 0;
        var entities = Helper.GetEntitiesByComponentType<SpawnRegion>(true);
        foreach (var entity in entities)
        {
            var spawnRegion = entity.Read<SpawnRegion>();
            if (spawnRegion.RespawnDurationMax != 0) continue;
            var ltw = entity.Read<LocalToWorld>();
            var entityPos = ltw.Position;
            // Find the closest posToRespawnTime
            var closestMinRespawnTime = int.MaxValue;
            var closestMaxRespawnTime = int.MaxValue;
            var closestDistance = float.MaxValue;
            foreach (var (pos, minRespawnTime, maxRespawnTime) in posToRespawnTime)
            {
                var distance = math.distance(entityPos, pos);
                if (distance < closestDistance)
                {
                    closestMinRespawnTime = minRespawnTime;
                    closestMaxRespawnTime = maxRespawnTime;
                    closestDistance = distance;
                }
            }

            if (closestDistance > 1f)
            {
                Core.Log.LogWarning($"Failed to find a close enough fixed spawn region for {entityPos}");
                continue;
            }

            spawnRegion.RespawnDurationMin = closestMinRespawnTime;
            spawnRegion.RespawnDurationMax = closestMaxRespawnTime;
            entity.Write(spawnRegion);
            numberRepaired++;
        }
        entities.Dispose();

        if (numberRepaired > 0)
            Core.Log.LogInfo($"Repaired {numberRepaired} spawn regions");

        float ParseFloat(string value)
        {
            // Remove the 'f' suffix if present
            value = value.TrimEnd('f');

            if (float.TryParse(value, out float result))
            {
                return result;
            }

            // If parsing fails, return 0
            return 0f;
        }
    }

    IEnumerator KeepFromRespawning()
    {
        var autoChainQueryBuilder = new EntityQueryBuilder(Allocator.Temp).AddAll(new(Il2CppType.Of<AutoChainInstanceData>(), ComponentType.AccessMode.ReadWrite));
        var autoChainQuery = Core.EntityManager.CreateEntityQuery(ref autoChainQueryBuilder);

        var spawnRegionQueryBuilder = new EntityQueryBuilder(Allocator.Temp).AddAll(new(Il2CppType.Of<SpawnRegion>(), ComponentType.AccessMode.ReadWrite));
        var spawnRegionQuery = Core.EntityManager.CreateEntityQuery(ref spawnRegionQueryBuilder);

        while (preventRespawns)
        {
            var serverTime = Core.ServerTime;
            var entities = autoChainQuery.ToEntityArray(Allocator.Temp);
            foreach (var entity in entities)
            {
                var data = entity.Read<AutoChainInstanceData>();
                if (data.NextTransitionAttempt < serverTime + 5)
                {
                    data.NextTransitionAttempt = Core.ServerTime + 10;
                    entity.Write(data);
                }
            }
            entities.Dispose();

            entities = spawnRegionQuery.ToEntityArray(Allocator.Temp);
            foreach (var entity in entities)
            {
                var data = entity.Read<SpawnRegion>();
                if (data.LastRespawnAttempt < serverTime + 5)
                {
                    data.LastRespawnAttempt = Core.ServerTime + 10;
                    entity.Write(data);
                }

                if (!Core.EntityManager.HasBuffer<SpawnRegionSpawnSlotEntry>(entity)) continue;

                var buffer = Core.EntityManager.GetBuffer<SpawnRegionSpawnSlotEntry>(entity);
                for (int i = 0; i < buffer.Length; i++)
                {
                    var entry = buffer[i];
                    if (entry.BlockRespawnUntil < serverTime + 5)
                    {
                        entry.BlockRespawnUntil = serverTime + 10;
                        buffer[i] = entry;
                    }
                }
            }
            entities.Dispose();
            yield return null;
        }
        respawnPreventionCoroutine = null;
    }
}
