using KindredVignettes.Services;
using ProjectM.CastleBuilding;
using System.Text.Json;
using Unity.Entities;

namespace KindredVignettes.ComponentSaver
{
    [ComponentType(typeof(CastleRoomWorkstationsBuffer))]
    internal class CastleRoomWorkstationsBuffer_Saver : ComponentSaver
    {
        public override object DiffComponents(Entity src, Entity dst, EntityMapper entityMapper)
        {
            return SaveComponent(dst, entityMapper);
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

            var workstationEntities = data.Deserialize<int[]>(VignetteService.GetJsonOptions());
            foreach(var i in workstationEntities)
                workstationBuffer.Add(new CastleRoomWorkstationsBuffer { WorkstationEntity = entitiesBeingLoaded[i] });
        }
    }
}
