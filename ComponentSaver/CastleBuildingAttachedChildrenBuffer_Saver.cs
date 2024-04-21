using KindredVignettes.Services;
using ProjectM.CastleBuilding;
using System.Text.Json;
using Unity.Entities;

namespace KindredVignettes.ComponentSaver
{
    [ComponentType(typeof(CastleBuildingAttachedChildrenBuffer))]
    internal class CastleBuildingAttachedChildBuffer_Saver : ComponentSaver
    {

        public override object DiffComponents(Entity prefab, Entity entity, EntityMapper entityMapper)
        {
            return SaveComponent(entity, entityMapper);
        }

        public override object SaveComponent(Entity entity, EntityMapper entityMapper)
        {
            var children = Core.EntityManager.GetBuffer<CastleBuildingAttachedChildrenBuffer>(entity);
            var childEntities = new int[children.Length];
            for (int i = 0; i < children.Length; i++)
            {
                childEntities[i] = entityMapper.IndexOf(children[i].ChildEntity.GetEntityOnServer());
            }

            return childEntities;
        }

        public override void ApplyComponentData(Entity entity, JsonElement data, Entity[] entitiesBeingLoaded)
        {
            DynamicBuffer<CastleBuildingAttachedChildrenBuffer> children;
            if (entity.Has<CastleBuildingAttachedChildrenBuffer>())
                children = Core.EntityManager.GetBuffer<CastleBuildingAttachedChildrenBuffer>(entity);
            else
                children = Core.EntityManager.AddBuffer<CastleBuildingAttachedChildrenBuffer>(entity);
            children.Clear();

            var childEntities = data.Deserialize<int[]>(VignetteService.GetJsonOptions());
            foreach (var i in childEntities)
                children.Add(new CastleBuildingAttachedChildrenBuffer { ChildEntity = entitiesBeingLoaded[i] });
        }
    }
}
