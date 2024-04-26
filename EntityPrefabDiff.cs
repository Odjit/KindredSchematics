using Il2CppInterop.Runtime;
using ProjectM;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace KindredVignettes
{
    public struct ComponentData
    {
        public string component { get; set;}
        public System.Object data { get; set; }
    }

    public struct EntityData

    {
        public PrefabGUID prefab { get; set; }
        public Vector3 pos { get; set; }
        public Quaternion rot { get; set; }
        public ComponentData[] componentData { get; set; }
        public int[] removals { get; set; }
    }

    internal class EntityPrefabDiff
    {
        public static EntityData DiffFromPrefab(Entity entity, EntityMapper entityMapper)
        {
            var diffData = new EntityData();
            // Compare the two entities and return a list of differences
            diffData.prefab = entity.Read<PrefabGUID>();
            diffData.pos = entity.Read<Translation>().Value;
            diffData.rot = entity.Read<Rotation>().Value;

            var componentData = new List<ComponentData>();
            var removals = new List<int>();
            if (Core.PrefabCollection.PrefabLookupMap.TryGetValue(diffData.prefab, out var prefabEntity))
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
                            componentData.Add(new ComponentData
                            {
                                component = entityComponent.GetManagedType().Name,
                                data = saver.SaveComponent(entity, entityMapper)
                            });
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
