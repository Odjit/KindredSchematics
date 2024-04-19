using KindredVignettes.JsonConverters;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Physics;
using ProjectM.Tiles;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace KindredVignettes.Services
{
    internal class VignetteService
    {
        static readonly string CONFIG_PATH = Path.Combine(BepInEx.Paths.ConfigPath, MyPluginInfo.PLUGIN_NAME);

        struct Vignette
        {
            public Vector3 location {get; set;}
            public Vector2? halfSize { get; set; } 
            public float? radius { get; set;}
            public TileData[] entities { get; set; }
        }

        readonly List<Entity> usersClearingEntireArea = [];
        readonly List<Entity> usersPlacingOffGrid = [];

        GameObject vignetteSvcGameObject;
        IgnorePhysicsDebugSystem vignetteMonoBehaviour;


        public VignetteService()
        {
            vignetteSvcGameObject = new GameObject("VignetteService");
            vignetteMonoBehaviour = vignetteSvcGameObject.AddComponent<IgnorePhysicsDebugSystem>();
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
            options.Converters.Add(new PrefabGUIDConverter());
            options.Converters.Add(new QuaternionConverter());
            options.Converters.Add(new Vector2Converter());
            options.Converters.Add(new Vector3Converter());
            return options;
        }

        public void SaveVignette(string name, float3 location, float? radius=null, Vector2? halfSize = null)
        {
            var vignette = new Vignette
            {
                location = location,
                halfSize = halfSize,
                radius = radius,
                entities = []
            };

            IEnumerable<Entity> entities;
            if (vignette.radius != null) entities = Helper.GetAllEntitiesInRadius<Translation>(location.xz, vignette.radius.Value);
            else if (vignette.halfSize != null) entities = Helper.GetAllEntitiesInBox<Translation>(location.xz, vignette.halfSize.Value);
            else
            {
                Core.Log.LogError($"Vignette {name} has no radius or halfSize");
                return;
            }

            var entityPrefabDiffs = new List<TileData>();
            var entitiesSaving = entities.Where(entity =>
            {
                if (entity.Has<CastleHeart>())
                    return false;
                
                var prefabName = entity.Read<PrefabGUID>().LookupName();
                return prefabName.StartsWith("TM_") || prefabName.StartsWith("Chain_") || prefabName.StartsWith("BP_") ||
                       entity.Has<CastleRoom>();
            });

            var entityMapper = new EntityMapper(entitiesSaving);
            for (var i=1; i< entityMapper.Count; ++i)
            {
                entityPrefabDiffs.Add(EntityPrefabDiff.DiffFromPrefab(entityMapper[i], entityMapper));
            }

            vignette.entities = entityPrefabDiffs.ToArray();
            
            var json = JsonSerializer.Serialize(vignette, GetJsonOptions());

            if (!Directory.Exists(CONFIG_PATH))
            {
                Directory.CreateDirectory(CONFIG_PATH);
            }
            File.WriteAllText($"{CONFIG_PATH}/{name}.vignette", json);
        }

        public bool LoadVignette(string name, Entity userEntity, Entity charEntity, Vector3? newCenter=null)
        {
            string json;
            try
            {
                json = File.ReadAllText($"{CONFIG_PATH}/{name}.vignette");
            }
            catch (FileNotFoundException)
            {
                Core.Log.LogError($"Vignette {name} not found");
                return false;
            }

            Vignette vignette;
            try
            {
                vignette = JsonSerializer.Deserialize<Vignette>(json, GetJsonOptions());
            }
            catch (JsonException e)
            {
                Core.Log.LogError($"Error loading vignette {name}: {e.Message}");
                return false;
            }

            var center = newCenter ?? vignette.location;
            var translation = newCenter!= null ? center - vignette.location : Vector3.zero;

            // Figure out the translation to keep it on the grid
            if (!usersPlacingOffGrid.Contains(userEntity))
            {
                translation.x = Mathf.Round(translation.x / 5) * 5;
                translation.y = Mathf.Round(translation.y);
                translation.z = Mathf.Round(translation.z / 5) * 5;
            }
            Core.Log.LogWarning($"Translation: {translation}");

            IEnumerable<Entity> entities;
            if (vignette.radius != null)  entities = Helper.GetAllEntitiesInRadius<TileModel>(((float3)center).xz, vignette.radius.Value);
            else if (vignette.halfSize != null) entities = Helper.GetAllEntitiesInBox<TileModel>(((float3)center).xz, vignette.halfSize.Value);
            else
            {
                Core.Log.LogError($"Vignette {name} has no radius or halfSize");
                return false;
            }

            Helper.DestroyEntitiesForBuilding(entities);

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

            // First pass create all the entities
            var createdEntities = new Entity[vignette.entities.Length+1];
            createdEntities[0] = Entity.Null;
            var time = Core.CastleBuffsTickSystem._ServerTime.GetSingleton();
            for (var i=0; i < vignette.entities.Length; ++i)
            {
                var diff = vignette.entities[i];

                if (Core.PrefabCollection.PrefabLookupMap.TryGetValue(diff.prefab, out var prefab))
                {
                    Entity entity = SpawnEntity(userEntity, translation, teamValue, castleHeartEntity, castleTeamReference, diff, prefab);

                    createdEntities[i + 1] = entity;
                }
            }

            // Second pass modify all their components
            for (var i = 0; i < vignette.entities.Length; ++i)
            {
                var diff = vignette.entities[i];
                var entity = createdEntities[i+1];
                ComponentSaver.ComponentSaver.ApplyComponentData(entity, diff.componentData, createdEntities);
                ComponentSaver.ComponentSaver.ApplyRemovals(entity, diff.removals);
            }

            return true;
        }

        private static Entity SpawnEntity(Entity userEntity, Vector3 translation, int teamValue, Entity castleHeartEntity, Entity castleTeamReference, TileData diff, Entity prefab)
        {
            var entity = Core.EntityManager.Instantiate(prefab);
            entity.Write(new Translation { Value = diff.translation + translation });
            entity.Write(new Rotation { Value = diff.rotation });

            if (entity.Has<CastleHeartConnection>())
            {
                entity.Write(new CastleHeartConnection { CastleHeartEntity = castleHeartEntity });

                if (entity.Has<Team>())
                {
                    entity.Write(new Team { Value = teamValue, FactionIndex = -1 });

                    entity.Add<UserOwner>();
                    entity.Write(new UserOwner() { Owner = userEntity });
                }

                if (entity.Has<TeamReference>() && !castleTeamReference.Equals(Entity.Null))
                {
                    var t = new TeamReference();
                    t.Value._Value = castleTeamReference;
                    entity.Write(t);
                }
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
