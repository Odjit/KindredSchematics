using BepInEx.Unity.IL2CPP.Utils.Collections;
using KindredVignettes.Data;
using KindredVignettes.JsonConverters;
using KindredVignettes.Patches;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Network;
using ProjectM.Physics;
using ProjectM.Shared;
using ProjectM.Tiles;
using Stunlock.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace KindredVignettes.Services
{
    internal class VignetteService
    {
        static readonly string CONFIG_PATH = Path.Combine(BepInEx.Paths.ConfigPath, MyPluginInfo.PLUGIN_NAME);

        struct Vignette
        {
            public string version { get; set; }
            public Vector3? location { get; set; }
            public int? territoryIndex { get; set; }
            public Aabb boundingBox { get; set; }
            public Aabb[] aabbs { get; set; }
            public EntityData[] entities { get; set; }
        }

        struct HeartInfo
        {
            public Entity CastleHeart;
            public Entity TeamReference;
            public int TeamValue;
        };

        readonly List<Entity> usersClearingEntireArea = [];
        readonly List<Entity> usersPlacingOffGrid = [];

        GameObject vignetteSvcGameObject;
        IgnorePhysicsDebugSystem vignetteMonoBehaviour;

        HashSet<PrefabGUID> prefabsAllowedToDestroy = [];


        public VignetteService()
        {
            vignetteSvcGameObject = new GameObject("VignetteService");
            vignetteMonoBehaviour = vignetteSvcGameObject.AddComponent<IgnorePhysicsDebugSystem>();

            foreach(var (prefabGUID, prefabName) in Core.PrefabCollection._PrefabGuidToNameDictionary)
            {
                if(prefabName.StartsWith("TM_") || prefabName.StartsWith("Chain_") || 
                   (Core.PrefabCollection._PrefabGuidToEntityMap.TryGetValue(prefabGUID, out var prefab) &&
                    prefab.Has<CastleBuildingFusedRoot>()))
                {
                    prefabsAllowedToDestroy.Add(prefabGUID);
                }
            }
        }

        public void StartCoroutine(IEnumerator routine)
        {
            vignetteMonoBehaviour.StartCoroutine(routine.WrapToIl2Cpp());
        }

        public IEnumerable<string> GetVignetteNames()
        {
            if (!Directory.Exists(CONFIG_PATH))
            {
                yield break;
            }

            foreach (var file in Directory.EnumerateFiles(CONFIG_PATH, "*.vignette"))
            {
                yield return Path.GetFileNameWithoutExtension(file);
            }
        }

        public static JsonSerializerOptions GetJsonOptions()
        {
            var options = new JsonSerializerOptions()
            {
                WriteIndented = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault
            };
            options.Converters.Add(new AabbConverter());
            options.Converters.Add(new AssetGUIDConverter());
            options.Converters.Add(new CurveReferenceConverter());
            options.Converters.Add(new int2Converter());
            options.Converters.Add(new PrefabGUIDConverter());
            options.Converters.Add(new QuaternionConverter());
            options.Converters.Add(new Vector2Converter());
            options.Converters.Add(new Vector3Converter());
            return options;
        }

        public void SaveVignette(string name, float3? location = null, float? radius = null, Vector2? halfSize = null, int? territoryIndex = null)
        {
            var startTime = Time.realtimeSinceStartup;
            float GetElapseTime() => Time.realtimeSinceStartup - startTime;
            Core.Log.LogInfo($"{GetElapseTime()} Starting to save {name}");
            var vignette = new Vignette
            {
                version = "1.0",
                entities = []
            };

            if (territoryIndex.HasValue)
                vignette.territoryIndex = territoryIndex;
            else
            {
                var gridLocation = Helper.ConvertPosToTileGrid(location.Value);
                vignette.boundingBox = new Aabb { Min = gridLocation, Max = gridLocation };
                vignette.location = location;
            }

            IEnumerable<Entity> entities;
            if (radius != null) entities = Helper.GetAllEntitiesInRadius<Translation>(location.Value.xz, radius.Value);
            else if (halfSize != null) entities = Helper.GetAllEntitiesInBox<Translation>(location.Value.xz, halfSize.Value);
            else if (territoryIndex != null) entities = Helper.GetAllEntitiesInTerritory<Translation>(territoryIndex.Value);
            else
            {
                Core.Log.LogError($"Vignette {name} has no radius, halfSize, or territory index");
                return;
            }

            Core.Log.LogInfo($"{GetElapseTime()} Gathered entities for {name}");

            var entityPrefabDiffs = new List<EntityData>();
            var aabbs = new List<Aabb>();
            var entitiesSaving = entities.Where(entity =>
            {
                if (entity.Has<CastleHeart>())
                    return false;

                if (!entity.Has<PrefabGUID>())
                    return false;

                if (entity.Has<CastleRoof>())
                    return false;

                var prefabGUID = entity.Read<PrefabGUID>();

                // For some reason this prefab is missing the correct stuff on the server
                if (prefabGUID == Prefabs.TM_Castle_Wall_Tier02_Stone_EntranceCrown)
                    return false;

                var prefabName = prefabGUID.LookupName();
                return prefabName.StartsWith("TM_") || prefabName.StartsWith("Chain_") || prefabName.StartsWith("BP_");
            });

            var entityMapper = new EntityMapper(entitiesSaving);
            Core.Log.LogInfo($"{GetElapseTime()} Gathered and Filtered entities for {name} and now saving");
            for (var i = 1; i < entityMapper.Count; ++i)
            {
                var entity = entityMapper[i];
                var data = EntityPrefabDiff.DiffFromPrefab(entity, entityMapper);
                data.id = i;
                entityPrefabDiffs.Add(data);
                if (territoryIndex == null && Helper.GetAabb(entity, out var aabb))
                {

                    aabbs.Add(aabb);
                    aabb.Include(vignette.boundingBox);
                    vignette.boundingBox = aabb;
                }

                if (entity.Has<CastleFloor>())
                {
                    var castleFloor = entity.Read<CastleFloor>();
                    if (castleFloor.NeighbourFloorNorth.Entity != Entity.Null)
                    {
                        var neighbour = entityMapper.IndexOf(castleFloor.NeighbourFloorNorth.Entity);
                    }
                    if (castleFloor.NeighbourFloorEast.Entity != Entity.Null)
                    {
                        var neighbour = entityMapper.IndexOf(castleFloor.NeighbourFloorEast.Entity);
                    }
                    if (castleFloor.NeighbourFloorSouth.Entity != Entity.Null)
                    {
                        var neighbour = entityMapper.IndexOf(castleFloor.NeighbourFloorSouth.Entity);
                    }
                    if (castleFloor.NeighbourFloorWest.Entity != Entity.Null)
                    {
                        var neighbour = entityMapper.IndexOf(castleFloor.NeighbourFloorWest.Entity);
                    }
                    if (castleFloor.NeighbourFloorUp.Entity != Entity.Null)
                    {
                        var neighbour = entityMapper.IndexOf(castleFloor.NeighbourFloorUp.Entity);
                    }
                    if (castleFloor.NeighbourFloorDown.Entity != Entity.Null)
                    {
                        var neighbour = entityMapper.IndexOf(castleFloor.NeighbourFloorDown.Entity);
                    }
                    if (castleFloor.WallNorth != Entity.Null)
                    {
                        var neighbour = entityMapper.IndexOf(castleFloor.WallNorth);
                    }
                    if (castleFloor.WallEast != Entity.Null)
                    {
                        var neighbour = entityMapper.IndexOf(castleFloor.WallEast);
                    }
                    if (castleFloor.WallSouth != Entity.Null)
                    {
                        var neighbour = entityMapper.IndexOf(castleFloor.WallSouth);
                    }
                    if (castleFloor.WallWest != Entity.Null)
                    {
                        var neighbour = entityMapper.IndexOf(castleFloor.WallWest);
                    }
                }
            }

            Core.Log.LogInfo($"{GetElapseTime()} Saved and now Starting to merge aabbs");
            var startNumAabbs = aabbs.Count;
            AabbHelper.MergeAabbsTogether(aabbs);
            Core.Log.LogInfo($"{GetElapseTime()} Reduced by {startNumAabbs - aabbs.Count} aabbs from {startNumAabbs} to {aabbs.Count}");

            vignette.entities = entityPrefabDiffs.ToArray();
            if (territoryIndex == null)
                vignette.aabbs = aabbs.ToArray();

            Core.Log.LogInfo($"{GetElapseTime()} Serializing {vignette.entities.Length} entities for {name}");
            var json = JsonSerializer.Serialize(vignette, GetJsonOptions());

            Core.Log.LogInfo($"{GetElapseTime()} Writing {name}.vignette");
            if (!Directory.Exists(CONFIG_PATH))
            {
                Directory.CreateDirectory(CONFIG_PATH);
            }
            File.WriteAllText($"{CONFIG_PATH}/{name}.vignette", json);
            Core.Log.LogInfo($"{GetElapseTime()} Finished writing {name}.vignette");
        }

        public Entity CurUserEntity { get; private set; }
        public Entity CurCharEntity { get; private set; }
        public string LoadVignette(string name, Entity userEntity, Entity charEntity, float expandClear, Vector3? newCenter = null)
        {
            CurUserEntity = userEntity;
            CurCharEntity = charEntity;

            string json;
            try
            {
                json = File.ReadAllText($"{CONFIG_PATH}/{name}.vignette");
            }
            catch (FileNotFoundException)
            {
                return $"Vignette not found";
            }

            Vignette vignette;
            try
            {
                vignette = JsonSerializer.Deserialize<Vignette>(json, GetJsonOptions());
            }
            catch (JsonException e)
            {
                Core.Log.LogError($"Error loading vignette {name}: {e.Message}");
                return "Error in file";
            }

            if (vignette.version != "1.0")
            {
                return $"Has an unsupported version '{vignette.version}' loading old versions is coming soon";
            }

            Core.StartCoroutine(FinishLoadingVignette(userEntity, charEntity, expandClear, newCenter, vignette));

            return null;
        }

        private IEnumerator FinishLoadingVignette(Entity userEntity, Entity charEntity, float expandClear, Vector3? newCenter, Vignette vignette)
        {
            const float MESSAGE_FREQUENCY = 2.5f;
            var startTime = Time.realtimeSinceStartup;
            float GetElapseTime() => Time.realtimeSinceStartup - startTime;
            var lastYieldTime = Time.realtimeSinceStartup;
            var timeSinceLastMessage = Time.realtimeSinceStartup;

            void MessageUser(string message, bool always=false)
            {
                if (always || Time.realtimeSinceStartup - timeSinceLastMessage > MESSAGE_FREQUENCY)
                {
                    ServerChatUtils.SendSystemMessageToClient(Core.EntityManager, userEntity.Read<User>(), message);
                    timeSinceLastMessage = Time.realtimeSinceStartup;
                }
            }

            var translation = Vector3.zero;
            var heartAabbsInLoadArea = new List<Aabb>();
            List<Entity> entitiesToDestroy = [];
            if (vignette.location.HasValue)
            {
                var center = newCenter ?? vignette.location.Value;
                translation = newCenter != null ? center - vignette.location.Value : Vector3.zero;

                // Figure out the translation to keep it on the grid
                if (!usersPlacingOffGrid.Contains(userEntity))
                {
                    translation.x = Mathf.Round(translation.x / 5) * 5;
                    translation.y = Mathf.Round(translation.y);
                    translation.z = Mathf.Round(translation.z / 5) * 5;
                }

                var gridTranslation = new float3(translation.x * 2, translation.y, translation.z * 2);

                var aabb = vignette.boundingBox;
                aabb.Min += gridTranslation;
                aabb.Max += gridTranslation;
                aabb.Expand(expandClear);

                Core.Log.LogInfo($"{GetElapseTime():f4} Getting entities in {aabb}");
                var entitiesToCheck = Helper.GetAllEntitiesInTileAabb<Translation>(aabb).
                    Where(x =>
                    {
                        if (x.Has<CastleHeart>())
                        {
                            var pos = x.Read<Translation>().Value;
                            var heartAabb = new Aabb
                            {
                                Min = new float3(pos.x - 2.5f, pos.y, pos.z - 2.5f),
                                Max = new float3(pos.x + 2.5f, pos.y + 0.1f, pos.z + 2.5f)
                            };
                            heartAabbsInLoadArea.Add(heartAabb);
                            return false;
                        }

                        return true;
                    }).ToArray();

                Core.Log.LogInfo($"{GetElapseTime():f4} Checking {entitiesToCheck.Length} entities to see if they need to be cleared");
                MessageUser($"Checking {entitiesToCheck.Length} entities to see if they need to be cleared");
                var aabbArray = new Aabb[vignette.aabbs.Length];
                for (var i = 0; i < vignette.aabbs.Length; i++)
                {
                    var newAabb = vignette.aabbs[i];
                    newAabb.Min += gridTranslation;
                    newAabb.Max += gridTranslation;
                    newAabb.Expand(expandClear);
                    aabbArray[i] = newAabb;
                }

                for(var i=0; i < entitiesToCheck.Length; i++)
                {
                    var entity = entitiesToCheck[i];
                    if (Time.realtimeSinceStartup - lastYieldTime > 0.05f)
                    {
                        MessageUser($"Have checked {i}/{entitiesToCheck.Length} for deletion so far with {entitiesToDestroy.Count} marked to be deleted");
                        lastYieldTime = Time.realtimeSinceStartup;
                        yield return null;
                    }

                    if (!entity.Has<PrefabGUID>())
                        continue;

                    if (!prefabsAllowedToDestroy.Contains(entity.Read<PrefabGUID>()))
                        continue;

                    // Keep entities protected by the heart
                    foreach (var heartAabb in heartAabbsInLoadArea)
                    {
                        if (Helper.IsEntityInAabb(entity, heartAabb))
                            continue;
                    }

                    foreach (var aabbToTest in aabbArray)
                    {
                        if (Helper.IsEntityInAabb(entity, aabbToTest))
                        {
                            entitiesToDestroy.Add(entity);
                            break;
                        }
                    }
                }
            }
            else if (vignette.territoryIndex.HasValue)
            {
                entitiesToDestroy.AddRange(Helper.GetAllEntitiesInTerritory<Translation>(vignette.territoryIndex.Value).
                    Where(x =>
                    {
                        if (!x.Has<PrefabGUID>())
                            return false;

                        if (x.Has<CastleHeart>())
                        {
                            var pos = x.Read<Translation>().Value;
                            var heartAabb = new Aabb
                            {
                                Min = new float3(pos.x - 2.5f, pos.y, pos.z - 2.5f),
                                Max = new float3(pos.x + 2.5f, pos.y + 0.1f, pos.z + 2.5f)
                            };
                            heartAabbsInLoadArea.Add(heartAabb);
                            return false;
                        }

                        var prefabName = x.Read<PrefabGUID>().LookupName();
                        return prefabName.StartsWith("TM_") || prefabName.StartsWith("Chain_") || x.Has<CastleBuildingFusedRoot>();
                    }));
            }

            Core.Log.LogInfo($"{GetElapseTime():f4} Filter {entitiesToDestroy.Count()} entities for clearing");
            MessageUser($"Now deleting {entitiesToDestroy.Count()} entities before loading in the vignette", true);
            yield return null;
            Core.RespawnPrevention.PreventRespawns();
            var entitiesDestroyingThisFrame = 0;
            foreach(var entity in entitiesToDestroy)
            {
                Helper.DestroyEntityAndCastleAttachments(entity);
                entitiesDestroyingThisFrame++;

                if (entitiesDestroyingThisFrame > 200)
                {
                    entitiesDestroyingThisFrame = 0;
                    yield return null;
                    lastYieldTime = Time.realtimeSinceStartup;
                }
            }

            MessageUser("Starting to load in the vignette", true);
            yield return null;

            var teamValue = charEntity.Read<Team>().Value;
            var castleHeartEntity = Entity.Null;
            var castleTeamReference = Entity.Null;
            if (charEntity.Has<TeamReference>())
            {
                var team = charEntity.Read<TeamReference>().Value;
                foreach (var allyEntries in Core.EntityManager.GetBuffer<TeamAllies>(team))
                {
                    var allyEntity = allyEntries.Value;
                    if (allyEntity.Has<CastleTeamData>())
                    {
                        castleHeartEntity = allyEntity.Read<CastleTeamData>().CastleHeart;
                        castleTeamReference = allyEntity;
                        break;
                    }
                }
            }

            var defaultHeartInfo = new HeartInfo
            {
                CastleHeart = castleHeartEntity,
                TeamReference = castleTeamReference,
                TeamValue = teamValue
            };

            var territoryToHeartInfo = new Dictionary<int, HeartInfo>
            {
                { -1, defaultHeartInfo }
            };

            // Disable spawn chain system for one frame
            InitializeNewSpawnChainSystem_Patch.skipOnce = true;

            // First pass create all the entitiesToDestroy
            var createdEntities = new Entity[vignette.entities.Length + 1];
            createdEntities[0] = Entity.Null;

            Core.Log.LogInfo($"{GetElapseTime():f4} Figuring out dependency groups");
            var dependentGroups = new Dictionary<int, List<int>>();
            // Initialize dependent groups with a list of everyone
            for (var i = 0; i < vignette.entities.Length; ++i)
            {
                dependentGroups.Add(i, new List<int> { i });
            }

            var dependencies = vignette.entities.Select(x => ComponentSaver.ComponentSaver.GetDependencies(x.componentData)).ToArray();

            // Grouping entities that are dependent on each other into groups that load together
            for (var i = 0; i < vignette.entities.Length; ++i)
            {
                var group = dependentGroups[i];
                foreach (var dependentOn in dependencies[i])
                {
                    var dependentIndex = dependentOn - 1;

                    // Have they already have been combined
                    if (group.Contains(dependentIndex))
                        continue;

                    if (dependencies[dependentIndex].Contains(i + 1))
                    {
                        var otherGroup = dependentGroups[dependentIndex];
                        foreach(var k in otherGroup)
                        {
                            dependentGroups[k] = group;
                            group.Add(k);
                        }

                        var newDependencies = dependencies[dependentIndex]
                                                .Union(dependencies[i])
                                                .Where(x => x != (i + 1) && x != dependentOn)
                                                .ToArray();

                        foreach (var k in group)
                        {
                            dependencies[k] = newDependencies;
                        }
                    }
                }
            }
            Core.Log.LogInfo($"{GetElapseTime():f4} Finished figuring out {dependentGroups.Count} dependency groups from {vignette.entities.Length} entitiesToDestroy");

            var entitiesLoaded = new HashSet<int>();
            var entitiesLoadedThisFrame = 0;
            do
            {
                var time = Core.ServerTime;

                List<int> entityGroupToLoad = [];
                for (var i = 0; i < vignette.entities.Length; ++i)
                {
                    if (entitiesLoaded.Contains(i))
                        continue;

                    // Check if we have all the groupDependencies
                    if (dependencies[i].Any(x => !entitiesLoaded.Contains(x - 1)))
                        continue;

                    // Lets load the group with these entities
                    entityGroupToLoad = dependentGroups[i];
                    break;
                }

                if (entityGroupToLoad.Count == 0)
                {
                    Core.Log.LogError($"{GetElapseTime():f4} Failed to find entity group to load so loading the remaining now");
                    yield return null;
                    for (var i = 0; i < vignette.entities.Length; ++i)
                        if (!entitiesLoaded.Contains(i))
                            entityGroupToLoad.Add(i);
                }

                foreach(var i in entityGroupToLoad)
                {
                    entitiesLoaded.Add(i);
                    entitiesLoadedThisFrame++;

                    var entityData = vignette.entities[i];

                    if (entityData.prefab.GuidHash == 0)
                        continue;

                    if (Core.PrefabCollection._PrefabLookupMap.TryGetValue(entityData.prefab, out var prefab))
                    {
                        Entity entity = SpawnEntity(userEntity, translation, entityData, prefab);

                        var territoryIndex = Helper.GetEntityTerritoryIndex(entity);

                        var heartInfo = defaultHeartInfo;
                        if (!territoryToHeartInfo.TryGetValue(territoryIndex, out heartInfo))
                        {

                            var heartEntity = Core.CastleTerritory.GetHeartForTerritory(territoryIndex);
                            if (heartEntity.Equals(Entity.Null))
                            {
                                heartInfo = defaultHeartInfo;
                            }
                            else
                            {
                                heartInfo.CastleHeart = heartEntity;
                                heartInfo.TeamValue = heartEntity.Read<Team>().Value;
                                heartInfo.TeamReference = Entity.Null;
                                if (heartEntity.Has<TeamReference>())
                                    heartInfo.TeamReference = heartEntity.Read<TeamReference>().Value;
                            }
                            territoryToHeartInfo.Add(territoryIndex, heartInfo);
                        }

                        if (entity.Has<CastleHeartConnection>())
                        {
                            entity.Write(new CastleHeartConnection { CastleHeartEntity = heartInfo.CastleHeart });
                        }

                        if (!entityData.notCastleTeam.HasValue || !entityData.notCastleTeam.Value)
                        {
                            if (entity.Has<Team>())
                            {
                                entity.Write(new Team { Value = heartInfo.TeamValue, FactionIndex = -1 });

                                entity.Add<UserOwner>();
                                entity.Write(new UserOwner() { Owner = userEntity });
                            }

                            if (entity.Has<TeamReference>() && !heartInfo.TeamReference.Equals(Entity.Null))
                            {
                                var t = new TeamReference();
                                t.Value._Value = heartInfo.TeamReference;
                                entity.Write(t);
                            }
                        }

                        if (territoryIndex == -1 && entity.Has<TileModel>())
                        {
                            if (!entity.Has<Immortal>())
                                entity.Add<Immortal>();
                            entity.Write(new Immortal() { IsImmortal = true });
                        }

                        // Can't have entities overlap a heart so they have to be destroyed
                        foreach (var heartAabb in heartAabbsInLoadArea)
                        {
                            if (Helper.IsEntityInAabb(entity, heartAabb))
                            {
                                DestroyUtility.Destroy(Core.EntityManager, entity);
                                entity = Entity.Null;
                                break;
                            }
                        }

                        createdEntities[i + 1] = entity;
                    }
                }

                // Second pass modify all their components
                foreach (var i in entityGroupToLoad)
                {
                    var diff = vignette.entities[i];
                    var entity = createdEntities[i + 1];

                    if (entity.Equals(Entity.Null))
                        continue;

                    ComponentSaver.ComponentSaver.ApplyComponentData(entity, diff.componentData, createdEntities);
                    ComponentSaver.ComponentSaver.ApplyRemovals(entity, diff.removals);
                }

                if (Time.realtimeSinceStartup - lastYieldTime > 0.05f)
                {
                    Core.Log.LogInfo($"{GetElapseTime():f4} Loaded {entitiesLoadedThisFrame} entities this frame for {100 * (float)entitiesLoaded.Count / (float)vignette.entities.Length:F1}% complete");
                    MessageUser($"Loading {100 * (float)entitiesLoaded.Count / (float)vignette.entities.Length:F1}% complete");
                    yield return null;
                    entitiesLoadedThisFrame = 0;
                    lastYieldTime = Time.realtimeSinceStartup;
                }
            } while (entitiesLoaded.Count < vignette.entities.Length);

            Core.Log.LogInfo($"{GetElapseTime():f4} Finished Loading Vignette");
            ServerChatUtils.SendSystemMessageToClient(Core.EntityManager, userEntity.Read<User>(), $"Finished Loading Vignette");
            Core.RespawnPrevention.AllowRespawns();
        }

        private static Entity SpawnEntity(Entity userEntity, Vector3 translation, EntityData diff, Entity prefab)
        {
            var entity = Core.EntityManager.Instantiate(prefab);
            if (diff.pos.HasValue)
            {
                if (!entity.Has<Translation>())
                    entity.Add<Translation>();
                entity.Write(new Translation { Value = diff.pos.Value + translation });
                if (entity.Has<LastTranslation>())
                    entity.Write(new LastTranslation { Value = diff.pos.Value + translation });
            }
            if (diff.rot.HasValue)
            {
                if (!entity.Has<Rotation>())
                    entity.Add<Rotation>();
                entity.Write(new Rotation { Value = diff.rot.Value });
            }

            int2 offset = new(Mathf.FloorToInt(translation.x * 2), Mathf.FloorToInt(translation.z * 2));
            if (diff.tilePos.HasValue)
            {
                if (!entity.Has<TilePosition>())
                    entity.Add<TilePosition>();
                entity.Write(new TilePosition { Tile = diff.tilePos.Value + offset });
            }

            if (diff.tileBoundsMin.HasValue && diff.tileBoundsMax.HasValue)
            {
                if (!entity.Has<TileBounds>())
                    entity.Add<TileBounds>();
                entity.Write(new TileBounds { Value = new() { Min = diff.tileBoundsMin.Value + offset, Max = diff.tileBoundsMax.Value + offset } });
            }

            return entity;
        }

        public bool ToggleClearingEntireArea(Entity userEntity)
        {
            if (usersClearingEntireArea.Contains(userEntity))
            {
                usersClearingEntireArea.Remove(userEntity);
                return false;
            }
            usersClearingEntireArea.Add(userEntity);
            return true;
        }

        public bool TogglePlacingOffGrid(Entity userEntity)
        {
            if (usersPlacingOffGrid.Contains(userEntity))
            {
                usersPlacingOffGrid.Remove(userEntity);
                return false;
            }
            usersPlacingOffGrid.Add(userEntity);
            return true;
        }
    }
}
