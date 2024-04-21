using ProjectM.CastleBuilding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;

namespace KindredVignettes.Services
{
    internal class CastleTerritoryService
    {
        const float BLOCK_SIZE = 10;
        Dictionary<int2, int> blockCoordToTerritoryIndex = [];

        public CastleTerritoryService()
        {
            foreach (var castleTerritory in Helper.GetEntitiesByComponentType<CastleTerritory>(true))
            {
                var castleTerritoryIndex = castleTerritory.Read<CastleTerritory>().CastleTerritoryIndex;
                var ctb = Core.EntityManager.GetBuffer<CastleTerritoryBlocks>(castleTerritory);
                for (int i = 0; i < ctb.Length; i++)
                {
                    blockCoordToTerritoryIndex[ctb[i].BlockCoordinate] = castleTerritoryIndex;
                }
            }
        }

        public int GetTerritoryIndex(float3 pos)
        {
            var blockCoord = ConvertPosToBlockCoord(pos);
            if (blockCoordToTerritoryIndex.TryGetValue(blockCoord, out var index))
                return index;
            return -1;
        }

        int2 ConvertPosToBlockCoord(float3 pos)
        {
            var gridPos = Helper.ConvertPosToGrid(pos);
            return new int2((int)math.floor(gridPos.x / BLOCK_SIZE), (int)math.floor(gridPos.z / BLOCK_SIZE));
        }
    }
}
