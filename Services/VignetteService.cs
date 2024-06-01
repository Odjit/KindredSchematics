using BepInEx.Unity.IL2CPP.Utils.Collections;
using KindredCommands.Data;
using KindredVignettes.JsonConverters;
using KindredVignettes.Patches;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Physics;
using ProjectM.Shared;
using ProjectM.Tiles;
using Stunlock.Core;
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
            public Vector3? location {get; set;}
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


        public VignetteService()
        {
            vignetteSvcGameObject = new GameObject("VignetteService");
            vignetteMonoBehaviour = vignetteSvcGameObject.AddComponent<IgnorePhysicsDebugSystem>();
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

        public void SaveVignette(string name, float3? location=null, float? radius=null, Vector2? halfSize = null, int? territoryIndex = null)
        {
            
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
            for (var i=1; i<entityMapper.Count; ++i)
            {
                var entity = entityMapper[i];
                entityPrefabDiffs.Add(EntityPrefabDiff.DiffFromPrefab(entity, entityMapper));
                if (territoryIndex==null && Helper.GetAabb(entity, out var aabb))
                {
                    
                    aabbs.Add(aabb);
                    aabb.Include(vignette.boundingBox);
                    vignette.boundingBox = aabb;
                }

                if(entity.Has<CastleFloor>())
                {
                    var castleFloor = entity.Read<CastleFloor>();
                    if(castleFloor.NeighbourFloorNorth.Entity != Entity.Null)
                    {
                        var neighbour = entityMapper.IndexOf(castleFloor.NeighbourFloorNorth.Entity);
                    }
                    if(castleFloor.NeighbourFloorEast.Entity != Entity.Null)
                    {
                        var neighbour = entityMapper.IndexOf(castleFloor.NeighbourFloorEast.Entity);
                    }
                    if(castleFloor.NeighbourFloorSouth.Entity != Entity.Null)
                    {
                        var neighbour = entityMapper.IndexOf(castleFloor.NeighbourFloorSouth.Entity);
                    }
                    if(castleFloor.NeighbourFloorWest.Entity != Entity.Null)
                    {
                        var neighbour = entityMapper.IndexOf(castleFloor.NeighbourFloorWest.Entity);
                    }
                    if(castleFloor.NeighbourFloorUp.Entity != Entity.Null)
                    {
                        var neighbour = entityMapper.IndexOf(castleFloor.NeighbourFloorUp.Entity);
                    }
                    if(castleFloor.NeighbourFloorDown.Entity != Entity.Null)
                    {
                        var neighbour = entityMapper.IndexOf(castleFloor.NeighbourFloorDown.Entity);
                    }
                    if(castleFloor.WallNorth != Entity.Null)
                    {
                        var neighbour = entityMapper.IndexOf(castleFloor.WallNorth);
                    }
                    if(castleFloor.WallEast != Entity.Null)
                    {
                        var neighbour = entityMapper.IndexOf(castleFloor.WallEast);
                    }
                    if(castleFloor.WallSouth != Entity.Null)
                    {
                        var neighbour = entityMapper.IndexOf(castleFloor.WallSouth);
                    }
                    if(castleFloor.WallWest != Entity.Null)
                    {
                        var neighbour = entityMapper.IndexOf(castleFloor.WallWest);
                    }
                }
            }

            vignette.entities = entityPrefabDiffs.ToArray();
            if (territoryIndex == null)
                vignette.aabbs = aabbs.ToArray();
            
            var json = JsonSerializer.Serialize(vignette, GetJsonOptions());

            if (!Directory.Exists(CONFIG_PATH))
            {
                Directory.CreateDirectory(CONFIG_PATH);
            }
            File.WriteAllText($"{CONFIG_PATH}/{name}.vignette", json);
        }

        public Entity CurUserEntity { get; private set; }
        public Entity CurCharEntity { get; private set; }
        public string LoadVignette(string name, Entity userEntity, Entity charEntity, float expandClear, Vector3? newCenter=null)
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

            var translation = Vector3.zero;
            var heartAabbsInLoadArea = new List<Aabb>();
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

                var entities = Helper.GetAllEntitiesInTileAabb<Translation>(aabb).
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
                    });

                entities = entities.
                    Where(x =>
                    {
                        if(!x.Has<PrefabGUID>())
                            return false;

                        // Keep entities protected by the heart
                        foreach(var heartAabb in heartAabbsInLoadArea)
                        {
                            if (Helper.IsEntityInAabb(x, heartAabb))
                                return false;
                        }

                        foreach (var aabb in vignette.aabbs)
                        {
                            var newAabb = aabb;
                            newAabb.Min += gridTranslation;
                            newAabb.Max += gridTranslation;
                            newAabb.Expand(expandClear);
                            if (Helper.IsEntityInAabb(x, newAabb))
                                return true;
                        }

                        var prefabName = x.Read<PrefabGUID>().LookupName();
                        return prefabName.StartsWith("TM_") || prefabName.StartsWith("Chain_");
                    });

                Helper.DestroyEntitiesForBuilding(entities);
            }
            else if(vignette.territoryIndex.HasValue)
            {
                var entities = Helper.GetAllEntitiesInTerritory<Translation>(vignette.territoryIndex.Value).
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
                        return prefabName.StartsWith("TM_") || prefabName.StartsWith("Chain_");
                    });

                Helper.DestroyEntitiesForBuilding(entities);
            }

            var teamValue = charEntity.Read<Team>().Value;
            var castleHeartEntity = Entity.Null;
            var castleTeamReference = Entity.Null;
            if(charEntity.Has<TeamReference>())
            {
                var team = charEntity.Read<TeamReference>().Value;
                foreach(var allyEntries in Core.EntityManager.GetBuffer<TeamAllies>(team))
                {
                    var allyEntity = allyEntries.Value;
                    if(allyEntity.Has<CastleTeamData>())
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

            // First pass create all the entities
            var createdEntities = new Entity[vignette.entities.Length+1];
            createdEntities[0] = Entity.Null;
            var time = Core.ServerTime;
            for (var i=0; i < vignette.entities.Length; ++i)
            {
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
                        if(!entity.Has<Immortal>())
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
            for (var i = 0; i < vignette.entities.Length; ++i)
            {
                var diff = vignette.entities[i];
                var entity = createdEntities[i+1];

                if (entity.Equals(Entity.Null))
                    continue;

                ComponentSaver.ComponentSaver.ApplyComponentData(entity, diff.componentData, createdEntities);
                ComponentSaver.ComponentSaver.ApplyRemovals(entity, diff.removals);

                // See if they have attachment apply buffs
                if (entity.Has<CastleBuildingAttachmentApplyBuff>())
                {
                    var applyBuffs = Core.EntityManager.GetBuffer<CastleBuildingAttachmentApplyBuff>(entity);
                    foreach (var buffToApply in applyBuffs)
                    {
                        if (buffToApply.ApplyOn == CastleBuildingAttachmentBuffApplyOn.This)
                        {
                        }
                    }
                }
            }

            return null;
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

            int2 offset = new (Mathf.FloorToInt(translation.x * 2), Mathf.FloorToInt(translation.z * 2));
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
                entity.Write(new TileBounds { Value = new (){ Min = diff.tileBoundsMin.Value + offset, Max = diff.tileBoundsMax.Value + offset } });
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
