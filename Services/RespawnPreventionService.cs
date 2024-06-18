using Il2CppInterop.Runtime;
using ProjectM;
using System.Collections;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace KindredSchematics.Services;
class RespawnPreventionService
{
    bool preventRespawns => preventRespawnsCount > 0;
    int preventRespawnsCount = 0;
    Coroutine respawnPreventionCoroutine;

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

    IEnumerator KeepFromRespawning()
    {
        EntityQueryDesc autoChainQueryDesc = new()
        {
            All = new ComponentType[] { new(Il2CppType.Of<AutoChainInstanceData>(), ComponentType.AccessMode.ReadWrite) }
        };
        var autoChainQuery = Core.EntityManager.CreateEntityQuery(autoChainQueryDesc);

        EntityQueryDesc spawnRegionQueryDesc = new()
        {
            All = new ComponentType[] { new(Il2CppType.Of<SpawnRegion>(), ComponentType.AccessMode.ReadWrite) }
        };
        var spawnRegionQuery = Core.EntityManager.CreateEntityQuery(spawnRegionQueryDesc);

        while (preventRespawns)
        {
            var serverTime = Core.ServerTime;
            var entities = autoChainQuery.ToEntityArray(Allocator.Temp);
            foreach (var entity in entities)
            {
                var data = entity.Read<AutoChainInstanceData>();
                if (data.NextTransitionAttempt < serverTime + 5)
                    entity.Write(new AutoChainInstanceData() { NextTransitionAttempt = Core.ServerTime + 10 });
            }
            entities.Dispose();

            entities = spawnRegionQuery.ToEntityArray(Allocator.Temp);
            foreach (var entity in entities)
            {
                var data = entity.Read<SpawnRegion>();
                if (data.LastRespawnAttempt < serverTime + 5)
                    entity.Write(new SpawnRegion() { LastRespawnAttempt = Core.ServerTime + 10 });

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
            yield return null;
        }
        respawnPreventionCoroutine = null;
    }
}
