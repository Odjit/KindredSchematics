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
            for (int i = 0; i < children.Length; i++)
            {
                // Just adding the child entity to the entity mapper without a dependency
                entityMapper.AddEntity(children[i].ChildEntity.GetEntityOnServer());
            }

            return null;
        }

        public override void ApplyComponentData(Entity entity, JsonElement data, Entity[] entitiesBeingLoaded)
        {
            throw new System.NotImplementedException();
        }
    }
}
