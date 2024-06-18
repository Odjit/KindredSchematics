using KindredSchematics.Services;
using ProjectM.CastleBuilding;
using System.Linq;
using System.Text.Json;
using Unity.Entities;
using UnityEngine.UIElements;

namespace KindredSchematics.ComponentSaver
{
    [ComponentType(typeof(CastleBuildingAttachToParentsBuffer))]
    internal class CastleBuildingAttachToParentsBuffer_Saver : ComponentSaver
    {
        public override object DiffComponents(Entity prefab, Entity entity, EntityMapper entityMapper)
        {
            return SaveComponent(entity, entityMapper);
        }

        public override object SaveComponent(Entity entity, EntityMapper entityMapper)
        {
            var parents = Core.EntityManager.GetBuffer<CastleBuildingAttachToParentsBuffer>(entity);
            var parentEntities = new int[parents.Length];
            for (int i = 0; i < parents.Length; i++)
            {
                parentEntities[i] = entityMapper.IndexOf(parents[i].ParentEntity.GetEntityOnServer());
            }

            return parentEntities;
        }

        public override void ApplyComponentData(Entity entity, JsonElement data, Entity[] entitiesBeingLoaded)
        {
            DynamicBuffer<CastleBuildingAttachToParentsBuffer> parents;
            if (entity.Has<CastleBuildingAttachToParentsBuffer>())
                parents = Core.EntityManager.GetBuffer<CastleBuildingAttachToParentsBuffer>(entity);
            else
                parents = Core.EntityManager.AddBuffer<CastleBuildingAttachToParentsBuffer>(entity);
            parents.Clear();

            var parentEntities = data.Deserialize<int[]>(SchematicService.GetJsonOptions());
            foreach (var i in parentEntities)
            {
                var parentEntity = entitiesBeingLoaded[i];
                parents.Add(new CastleBuildingAttachToParentsBuffer { ParentEntity = parentEntity });

                if(parentEntity.Equals(Entity.Null))
                {
                    Core.Log.LogInfo($"We have a null entity for {i}");
                    continue;
                }

                if (!parentEntity.Has<CastleBuildingAttachedChildrenBuffer>())
                    parentEntity.Add<CastleBuildingAttachedChildrenBuffer>();
                var childrenBuffer = Core.EntityManager.GetBuffer<CastleBuildingAttachedChildrenBuffer>(parentEntity);
                childrenBuffer.Add(new CastleBuildingAttachedChildrenBuffer { ChildEntity = entity });
            }
        }

        public override int[] GetDependencies(JsonElement data)
        {
            var saveData = data.Deserialize<int[]>(SchematicService.GetJsonOptions());
            return saveData;
        }
    }
}
