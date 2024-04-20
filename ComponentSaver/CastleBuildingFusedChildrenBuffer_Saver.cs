using KindredVignettes.Services;
using ProjectM.CastleBuilding;
using System.Text.Json;
using Unity.Entities;

namespace KindredVignettes.ComponentSaver
{
    [ComponentType(typeof(CastleBuildingFusedChildrenBuffer))]
    internal class CastleBuildingFusedChildrenBuffer_Saver : ComponentSaver
    {

        public override object DiffComponents(Entity src, Entity dst, EntityMapper entityMapper)
        {
            return SaveComponent(dst, entityMapper);
        }

        public override object SaveComponent(Entity entity, EntityMapper entityMapper)
        {
            var fusedChildrenBuffer = Core.EntityManager.GetBuffer<CastleBuildingFusedChildrenBuffer>(entity);
            var children = new int[fusedChildrenBuffer.Length];
            for (int i = 0; i < fusedChildrenBuffer.Length; i++)
            {
                children[i] = entityMapper.IndexOf(fusedChildrenBuffer[i].ChildEntity.GetEntityOnServer());
            }

            return children;
        }

        public override void ApplyComponentData(Entity entity, JsonElement data, Entity[] entitiesBeingLoaded)
        {
            DynamicBuffer<CastleBuildingFusedChildrenBuffer> fusedChildrenBuffer;
            if (entity.Has<CastleBuildingFusedChildrenBuffer>())
                fusedChildrenBuffer = Core.EntityManager.GetBuffer<CastleBuildingFusedChildrenBuffer>(entity);
            else
                fusedChildrenBuffer = Core.EntityManager.AddBuffer<CastleBuildingFusedChildrenBuffer>(entity);
            fusedChildrenBuffer.Clear();

            var children = data.Deserialize<int[]>(VignetteService.GetJsonOptions());
            foreach(var child in children)
                fusedChildrenBuffer.Add(new CastleBuildingFusedChildrenBuffer {
                    ChildEntity = entitiesBeingLoaded[child]
                });
        }
    }
}
