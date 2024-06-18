using KindredSchematics.Services;
using ProjectM.CastleBuilding;
using System.Linq;
using System.Text.Json;
using Unity.Entities;

namespace KindredSchematics.ComponentSaver
{
    [ComponentType(typeof(CastleRoomWallsBuffer))]
    internal class CastleRoomWallsBuffer_Saver : ComponentSaver
    {

        struct CastleWall
        {
            public int WallEntity { get; set; }
            public byte WallDirection { get; set; }
        }

        public override object DiffComponents(Entity prefab, Entity entity, EntityMapper entityMapper)
        {
            return SaveComponent(entity, entityMapper);
        }

        public override object SaveComponent(Entity entity, EntityMapper entityMapper)
        {
            var wallBuffer = Core.EntityManager.GetBuffer<CastleRoomWallsBuffer>(entity);
            var walls = new CastleWall[wallBuffer.Length];
            for (int i = 0; i < wallBuffer.Length; i++)
            {
                walls[i] = new CastleWall()
                {
                    WallEntity = entityMapper.IndexOf(wallBuffer[i].WallEntity.GetEntityOnServer()),
                    WallDirection = (byte)wallBuffer[i].WallDirection
                };
            }

            return walls;
        }

        public override void ApplyComponentData(Entity entity, JsonElement data, Entity[] entitiesBeingLoaded)
        {
            DynamicBuffer<CastleRoomWallsBuffer> wallBuffer;
            if (entity.Has<CastleRoomWallsBuffer>())
                wallBuffer = Core.EntityManager.GetBuffer<CastleRoomWallsBuffer>(entity);
            else
                wallBuffer = Core.EntityManager.AddBuffer<CastleRoomWallsBuffer>(entity);
            wallBuffer.Clear();

            var walls = data.Deserialize<CastleWall[]>(SchematicService.GetJsonOptions());
            foreach(var wall in walls)
                wallBuffer.Add(new CastleRoomWallsBuffer {
                    WallEntity = entitiesBeingLoaded[wall.WallEntity],
                    WallDirection = (CardinalDirection)wall.WallDirection
                });
        }

        public override int[] GetDependencies(JsonElement data)
        {
            var saveData = data.Deserialize<CastleWall[]>(SchematicService.GetJsonOptions());
            return saveData.Select(x => x.WallEntity).ToArray();
        }
    }
}
