using KindredSchematics.Services;
using ProjectM.CastleBuilding;
using System.Text.Json;
using Unity.Entities;

namespace KindredSchematics.ComponentSaver
{
    [ComponentType(typeof(CastleRoomFloorsBuffer))]
    internal class CastleRoomFloorsBuffer_Saver : ComponentSaver
    {
        public override object DiffComponents(Entity prefab, Entity entity, EntityMapper entityMapper)
        {
            return SaveComponent(entity, entityMapper);
        }

        public override object SaveComponent(Entity entity, EntityMapper entityMapper)
        {
            var floors = Core.EntityManager.GetBuffer<CastleRoomFloorsBuffer>(entity);
            var floorEntities = new int[floors.Length];
            for (int i = 0; i < floors.Length; i++)
            {
                floorEntities[i] = entityMapper.IndexOf(floors[i].FloorEntity.GetEntityOnServer());
            }

            return floorEntities;
        }

        public override void ApplyComponentData(Entity entity, JsonElement data, Entity[] entitiesBeingLoaded)
        {
            DynamicBuffer<CastleRoomFloorsBuffer> floors;
            if (entity.Has<CastleRoomFloorsBuffer>())
                floors = Core.EntityManager.GetBuffer<CastleRoomFloorsBuffer>(entity);
            else
                floors = Core.EntityManager.AddBuffer<CastleRoomFloorsBuffer>(entity);
            floors.Clear();

            var floorEntities = data.Deserialize<int[]>(SchematicService.GetJsonOptions());
            foreach(var i in floorEntities)
                floors.Add(new CastleRoomFloorsBuffer { FloorEntity = entitiesBeingLoaded[i] });
        }

        public override int[] GetDependencies(JsonElement data)
        {
            var saveData = data.Deserialize<int[]>(SchematicService.GetJsonOptions());
            return saveData;
        }
    }
}
