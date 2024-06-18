using ProjectM.CastleBuilding;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;

namespace KindredSchematics.Services
{
    internal class CastleTerritoryService
    {
        const float BLOCK_SIZE = 10;
        Dictionary<int2, int> blockCoordToTerritoryIndex = [];

        public CastleTerritoryService()
        {
            var entities = Helper.GetEntitiesByComponentType<CastleTerritory>(true);
            foreach (var castleTerritory in entities)
            {
                var castleTerritoryIndex = castleTerritory.Read<CastleTerritory>().CastleTerritoryIndex;
                var ctb = Core.EntityManager.GetBuffer<CastleTerritoryBlocks>(castleTerritory);
                for (int i = 0; i < ctb.Length; i++)
                {
                    blockCoordToTerritoryIndex[ctb[i].BlockCoordinate] = castleTerritoryIndex;
                }
            }
            entities.Dispose();
        }

        public int GetTerritoryIndexFromTileCoord(int2 tilePos)
        {
            var blockCoord = ConvertTileGridToBlockCoord(tilePos);
            if (blockCoordToTerritoryIndex.TryGetValue(blockCoord, out var index))
                return index;
            return -1;
        }

        public int GetTerritoryIndex(float3 pos)
        {
            var blockCoord = ConvertPosToBlockCoord(pos);
            if (blockCoordToTerritoryIndex.TryGetValue(blockCoord, out var index))
                return index;
            return -1;
        }

        public Entity GetHeartForTerritory(int territoryIndex)
        {
            if(territoryIndex == -1)
                return Entity.Null;
            var castleHearts = Helper.GetEntitiesByComponentType<CastleHeart>();
            foreach(var heart in castleHearts)
            {
                var heartData = heart.Read<CastleHeart>();
                var castleTerritoryEntity = heartData.CastleTerritoryEntity;
                if (castleTerritoryEntity.Equals(Entity.Null))
                    continue;
                var heartTerritoryIndex = castleTerritoryEntity.Read<CastleTerritory>().CastleTerritoryIndex;
                if (heartTerritoryIndex == territoryIndex)
                    return heart;
            }
            castleHearts.Dispose();
            return Entity.Null;
        }

        int2 ConvertPosToBlockCoord(float3 pos)
        {
            var gridPos = Helper.ConvertPosToTileGrid(pos);
            return new int2((int)math.floor(gridPos.x / BLOCK_SIZE), (int)math.floor(gridPos.z / BLOCK_SIZE));
        }

        int2 ConvertTileGridToBlockCoord(int2 tileCoord)
        {
            return new int2((int)math.floor(tileCoord.x / BLOCK_SIZE), (int)math.floor(tileCoord.y / BLOCK_SIZE));
        }
    }
}
