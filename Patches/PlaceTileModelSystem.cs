using HarmonyLib;
using KindredVignettes;
using ProjectM;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

public static class PlaceTileModelSystem_Patch
{
    [HarmonyPatch(typeof(PlaceTileModelSystem), nameof(PlaceTileModelSystem.OnUpdate))]
    public class TileCheck
    {
        public static void Prefix(PlaceTileModelSystem __instance)
        {
            EntityManager entityManager = __instance.EntityManager;

            // if you're outside your castle and not an admin you ain't dismantling, moving or editing shit
            NativeArray<Entity> dismantleArray = __instance._DismantleTileQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity dismantleEntity in dismantleArray)
            {
                if (dismantleEntity.Has<DismantleTileModelEvent>())
                {
                    var dismantleEvent = dismantleEntity.Read<DismantleTileModelEvent>();

                    var targetEntity = dismantleEvent.Target.GetEntity();
                    if (targetEntity.Has<PrefabGUID>())
                    {
                        Core.Log.LogInfo($"Dismantle {targetEntity.Read<PrefabGUID>().LookupName()}");
                    }
                    else
                        Core.Log.LogInfo($"Dismantling {dismantleEvent.Target}");
                }
                else
                {
                    var allTypes = entityManager.GetComponentTypes(dismantleEntity).ToArray();
                    var components = string.Join("\n", allTypes.Select(x => x.ToString()));
                    Core.Log.LogInfo($"Dismantling entity with components\n{components}");
                }

            }
            dismantleArray.Dispose();
            NativeArray<Entity> moveArray = __instance._MoveTileQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity movingEntity in moveArray)
            {
                if (movingEntity.Has<MoveTileModelEvent>())
                {
                    var moveTileEvent = movingEntity.Read<MoveTileModelEvent>();
                    Core.Log.LogInfo($"Moving {moveTileEvent.Target} to {moveTileEvent.NewTranslation}");
                }
                else
                {
                    var allTypes = entityManager.GetComponentTypes(movingEntity).ToArray();
                    var components = string.Join("\n", allTypes.Select(x => x.ToString()));
                    Core.Log.LogInfo($"Moving entity with components\n{components}");
                }
            }
            moveArray.Dispose();

            NativeArray<Entity> buildArray = __instance._BuildTileQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity buildingEntity in buildArray)
            {
                if (buildingEntity.Has<BuildTileModelEvent>())
                {
                    var buildTileEvent = buildingEntity.Read<BuildTileModelEvent>();
                    var transformedEntity = buildTileEvent.TransformedEntity.GetEntity();

                    if (transformedEntity != Entity.Null)
                    {
                        if (transformedEntity.Has<PrefabGUID>())
                        {
                            Core.Log.LogInfo($"Building {buildTileEvent.PrefabGuid.LookupName()} on {transformedEntity.Read<PrefabGUID>().LookupName()} at {transformedEntity.Read<Translation>().Value}");
                            
                        }
                        else
                        {
                            var allTypes = entityManager.GetComponentTypes(transformedEntity).ToArray();
                            var components = string.Join("\n", allTypes.Select(x => x.ToString()));
                            Core.Log.LogInfo($"Building {buildTileEvent.PrefabGuid.LookupName()} entity with components on transformed entity \n{components}");
                        }
                    }
                    else
                    {
                        Core.Log.LogInfo($"Building {buildTileEvent.PrefabGuid.LookupName()} on {buildTileEvent.TransformedEntity}");
                    }
                }
                else
                {
                    var allTypes = entityManager.GetComponentTypes(buildingEntity).ToArray();
                    var components = string.Join("\n", allTypes.Select(x => x.ToString()));
                    Core.Log.LogInfo($"Building entity with components\n{components}");
                }
            }
            buildArray.Dispose();


        }
    }


    /*[HarmonyPatch(typeof(PlaceTileModelSystem), nameof(PlaceTileModelSystem.TryDoStuff))]
    public class DoStuff
    {
        public static CollisionWorld CollisionWorld => _collisionWorld;
        static CollisionWorld _collisionWorld;
        public static EntityCommandBuffer SpawnCommandBuffer;
        public static EntityCommandBuffer DestroyCommandBuffer;

        public static void Postfix(ref bool __result, PlaceTileModelSystem __instance, BuildTileModelData data, PrefabLookupMap prefabMap, CollisionWorld collisionWorld, GetPlacementResult.SystemData.PrepareJobData prepareJobData, EntityCommandBuffer spawnCommandBuffer, EntityCommandBuffer destroyCommandBuffer, double serverTime)
        {
            _collisionWorld = collisionWorld;
            SpawnCommandBuffer = spawnCommandBuffer;
            DestroyCommandBuffer = destroyCommandBuffer;
            if (data.PrefabGuid != null)
            {
                Core.Log.LogInfo($"Building {data.PrefabGuid.LookupName()} at {data.Translation.Value}");
            }
        }
    }*/
}
