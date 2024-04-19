using KindredVignettes.Services;
using ProjectM.CastleBuilding;
using System.Text.Json;
using Unity.Entities;

namespace KindredVignettes.ComponentSaver
{
    [ComponentType(typeof(CastleBuildingAttachToParentsBuffer))]
    internal class CastleBuildingAttachToParentsBuffer_Saver : ComponentSaver
    {
        public override object DiffComponents(Entity src, Entity dst, EntityMapper entityMapper)
        {
            return SaveComponent(dst, entityMapper);
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

            var parentEntities = data.Deserialize<int[]>(VignetteService.GetJsonOptions());
            foreach (var i in parentEntities)
                parents.Add(new CastleBuildingAttachToParentsBuffer { ParentEntity = entitiesBeingLoaded[i] });
        }
    }
}
