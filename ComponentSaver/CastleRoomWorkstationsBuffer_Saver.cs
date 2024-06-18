using KindredSchematics.Services;
using ProjectM.CastleBuilding;
using System.Linq;
using System.Text.Json;
using Unity.Entities;

namespace KindredSchematics.ComponentSaver
{
    [ComponentType(typeof(CastleRoomWorkstationsBuffer))]
    internal class CastleRoomWorkstationsBuffer_Saver : ComponentSaver
    {
        public override object DiffComponents(Entity prefab, Entity entity, EntityMapper entityMapper)
        {
            return SaveComponent(entity, entityMapper);
        }

        public override object SaveComponent(Entity entity, EntityMapper entityMapper)
        {
            var workstationBuffer = Core.EntityManager.GetBuffer<CastleRoomWorkstationsBuffer>(entity);
            var workstationEntities = new int[workstationBuffer.Length];
            for (int i = 0; i < workstationBuffer.Length; i++)
            {
                workstationEntities[i] = entityMapper.IndexOf(workstationBuffer[i].WorkstationEntity);
            }

            return workstationEntities;
        }

        public override void ApplyComponentData(Entity entity, JsonElement data, Entity[] entitiesBeingLoaded)
        {
            DynamicBuffer<CastleRoomWorkstationsBuffer> workstationBuffer;
            if (entity.Has<CastleRoomWorkstationsBuffer>())
                workstationBuffer = Core.EntityManager.GetBuffer<CastleRoomWorkstationsBuffer>(entity);
            else
                workstationBuffer = Core.EntityManager.AddBuffer<CastleRoomWorkstationsBuffer>(entity);
            workstationBuffer.Clear();

            var workstationEntities = data.Deserialize<int[]>(SchematicService.GetJsonOptions());
            foreach(var i in workstationEntities)
                workstationBuffer.Add(new CastleRoomWorkstationsBuffer { WorkstationEntity = entitiesBeingLoaded[i] });
        }

        public override int[] GetDependencies(JsonElement data)
        {
            var saveData = data.Deserialize<int[]>(SchematicService.GetJsonOptions());
            return saveData;
        }
    }
}
