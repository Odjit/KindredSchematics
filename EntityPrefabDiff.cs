using Il2CppInterop.Runtime;
using ProjectM;
using Stunlock.Core;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace KindredSchematics
{
    public struct ComponentData
    {
        public string component { get; set;}
        public System.Object data { get; set; }
    }

    public struct EntityData
    {
        public PrefabGUID prefab { get; set; }
        public Vector3? pos { get; set; }
        public Quaternion? rot { get; set; }
        public int2? tilePos { get; set; }
        public int2? tileBoundsMin { get; set; }
        public int2? tileBoundsMax { get; set; }
        public ComponentData[] componentData { get; set; }
        public int[] removals { get; set; }

        public bool? notCastleTeam { get; set; } // Using a negative so by default it's not included in the diff
    }

    internal class EntityPrefabDiff
    {
        public static EntityData DiffFromPrefab(Entity entity, EntityMapper entityMapper)
        {
            var diffData = new EntityData();
            // Compare the two entities and return a list of differences
            diffData.prefab = entity.Read<PrefabGUID>();
            if(entity.Has<Translation>())
                diffData.pos = entity.Read<Translation>().Value;
            if(entity.Has<Rotation>())
                diffData.rot = entity.Read<Rotation>().Value;
            if(entity.Has<TilePosition>())
                diffData.tilePos = entity.Read<TilePosition>().Tile;
            if(entity.Has<TileBounds>())
            {
                var bounds = entity.Read<TileBounds>();
                diffData.tileBoundsMin = bounds.Value.Min;
                diffData.tileBoundsMax = bounds.Value.Max;
            }

            if(entity.Has<TeamReference>())
            {
                var teamReference = entity.Read<TeamReference>().Value;
                if(teamReference.Value != Entity.Null)
                {
                    diffData.notCastleTeam = !teamReference.Value.Has<CastleTeamData>();
                }
            }

            var componentData = new List<ComponentData>();
            var removals = new List<int>();
            if (Core.PrefabCollection._PrefabLookupMap.TryGetValue(diffData.prefab, out var prefabEntity))
            {
                var entityComponents = Core.EntityManager.GetComponentTypes(entity).ToArray().ToList();
                var prefabComponents = Core.EntityManager.GetComponentTypes(prefabEntity).ToArray().ToList();
                entityComponents.Sort((a, b) => a.ToString().CompareTo(b.ToString()));
                prefabComponents.Sort((a, b) => a.ToString().CompareTo(b.ToString()));

                // Compare the two lists of components
                var prefabIndex = 0;
                for(var entityIndex = 0; entityIndex < entityComponents.Count; entityIndex++)
                {
                    var entityComponent = entityComponents[entityIndex];
                    var prefabComponent = prefabComponents[prefabIndex];

                    var comparison = entityComponent.ToString().CompareTo(prefabComponent.ToString());
                    if (comparison == 0)
                    {
                        // We've got a match
                        var saver = ComponentSaver.ComponentSaver.GetComponentSaver(entityComponent.TypeIndex);
                        if(saver != null)
                        {
                            var diff = saver.DiffComponents(prefabEntity, entity, entityMapper);
                            if(diff != null)
                                componentData.Add(new ComponentData {
                                    component = entityComponent.GetManagedType().Name,
                                    data = diff
                                });
                        }
                        prefabIndex++;
                    }
                    else if (comparison < 0)
                    {
                        var saver = ComponentSaver.ComponentSaver.GetComponentSaver(entityComponent.TypeIndex);
                        if (saver != null)
                        {
                            var data = saver.SaveComponent(entity, entityMapper);
                            if (data != null)
                            {
                                componentData.Add(new ComponentData
                                {
                                    component = entityComponent.GetManagedType().Name,
                                    data = data
                                });
                            }
                        }
                    }
                    else
                    {
                        if (prefabComponent != new ComponentType(Il2CppType.Of<Prefab>()) &&
                            prefabComponent != new ComponentType(Il2CppType.Of<SpawnTag>()))
                        {
                            removals.Add(prefabComponent.TypeIndex);
                        }
                        prefabIndex++;
                        entityIndex--;
                    }
                }

                if(componentData.Count > 0)
                    diffData.componentData = componentData.ToArray();
                if(removals.Count > 0)
                    diffData.removals = removals.ToArray();
            }

            return diffData;
        }
    }
}
