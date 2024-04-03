using KindredCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Transforms;
using VampireCommandFramework;
using ProjectM;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.TextCore;
using KindredVignettes.Data;
using static ProjectM.PlaceTileModelSystem;
using ProjectM.Scripting;
using Unity.Physics.Systems;
using Unity.Physics;
using static ProjectM.CastleBuilding.Placement.GetPlacementResult.SystemData;


namespace KindredVignettes.Commands
{
    public record struct TileUnit(string Name, PrefabGUID Prefab);

    public class TileUnitConverter : CommandArgumentConverter<TileUnit>
    {
        public override TileUnit Parse(ICommandContext ctx, string input)
        {
            if (Tile.Named.TryGetValue(input, out var unit) || Tile.Named.TryGetValue("TM_" + input, out unit))
            {
                return new(Tile.NameFromPrefab[unit.GuidHash], unit);
            }
            if (int.TryParse(input, out var id) && Tile.NameFromPrefab.TryGetValue(id, out var name))
            {
                return new(name, new(id));
            }

            throw ctx.Error($"Can't find unit {input.Bold()}");
        }
    }

    internal class spawn
    {
        [Command("spawntile", description: "Spawns a tile at the player's location", adminOnly: true)]
        public static void SpawnTile(ChatCommandContext ctx, TileUnit Tile)
        {

            var pos = Core.EntityManager.GetComponentData<LocalToWorld>(ctx.Event.SenderCharacterEntity).Position;
            //Tile
            //Services.TileSpawnerService.Spawn(ctx.Event.SenderUserEntity, tile.Prefab, new(pos.x, pos.z), 1, 2, -1);
            BuildTileModelData tileModelData = new BuildTileModelData()
            {
                Character = ctx.Event.SenderCharacterEntity,
                User = ctx.Event.SenderUserEntity,
                PrefabGuid = Tile.Prefab,
            };
            /*
            tileModelData.Translation.Value = ctx.Event.SenderCharacterEntity.Read<Translation>().Value;
            var ptms = Core.Server.GetExistingSystem<PlaceTileModelSystem>();

            var serverGameManager = Core.ServerScriptMapper.GetServerGameManager();

            PrepareJobData a = new PrepareJobData();

            ptms.TryDoStuff(tileModelData, serverGameManager.PrefabLookupMap, PlaceTileModelSystem_Patch.DoStuff.CollisionWorld, a,
                PlaceTileModelSystem_Patch.DoStuff.SpawnCommandBuffer, PlaceTileModelSystem_Patch.DoStuff.DestroyCommandBuffer, serverGameManager.ServerTime,
                out var placementJobResult, out var resourcesResult, out var placementJobParams, out var placementAbility);

            Debug.Log($"PlacementJobResult: {placementJobResult}\nResources Result {resourcesResult}\n{placementJobParams}");*/



            //ctx.Reply($"Spawned tile {tileName} at {playerPos}");
        }
        
    }
}
