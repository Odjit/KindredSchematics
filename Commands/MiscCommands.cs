using ProjectM;
using ProjectM.CastleBuilding;
using System;
using System.Linq;
using Unity.Transforms;
using VampireCommandFramework;

namespace KindredVignettes.Commands
{
    internal class MiscCommands
    {
        [Command("flyheight", description: "Sets the fly height for the user", adminOnly: true)]
        public static void SetFlyHeight(ChatCommandContext ctx, float height = 30)
        {
            var charEntity = ctx.Event.SenderCharacterEntity;
            var canFly = charEntity.Read<CanFly>();
            canFly.FlyingHeight.Value = height;
            charEntity.Write(canFly);
            ctx.Reply($"Set fly height to {height}");
        }

        [Command("flyobstacleheight", description: "Set the height to fly above any obstacles", adminOnly: true)]
        public static void SetFlyObstacleHeight(ChatCommandContext ctx, float height = 7)
        {
            var charEntity = ctx.Event.SenderCharacterEntity;
            var canFly = charEntity.Read<CanFly>();
            canFly.HeightAboveObstacle.Value = height;
            charEntity.Write(canFly);
            ctx.Reply($"Set fly obstacle height to {height}");
        }

        [Command("forcechain", description: "Set the chain transition time for nearby chains to now", adminOnly: true)]
        public static void ChainTransition(ChatCommandContext ctx, float range=10)
        {
            var charEntity = ctx.Event.SenderCharacterEntity;
            var time = Core.CastleBuffsTickSystem._ServerTime.GetSingleton().Time;
            foreach(var chainEntity in Helper.GetAllEntitiesInRadius<AutoChainInstanceData>(charEntity.Read<Translation>().Value.xz, range))
            {
                chainEntity.Write(new AutoChainInstanceData() { NextTransitionAttempt=time });
            }
        }

        [Command("floorup", "fu", description: "Move up a floor", adminOnly: true)]
        public static void FloorUp(ChatCommandContext ctx, int numFloors = 1)
        {
            var charEntity = ctx.Event.SenderCharacterEntity;
            var charPos = charEntity.Read<Translation>().Value;
            var gridPos = Helper.ConvertPosToGrid(charPos);


            var floors = Helper.GetAllEntitiesInRadius<CastleFloor>(charPos.xz, 5).
                Where(f =>
                {
                    if (!f.Has<TileBounds>())
                        return false;
                    var tb = f.Read<TileBounds>().Value;
                    return tb.Min.x <= gridPos.x && tb.Max.x >= gridPos.x && tb.Min.y <= gridPos.z && tb.Max.y >= gridPos.z;
                }).ToArray();

            Array.Sort(floors, (a, b) =>
            {
                var aPos = a.Read<Translation>().Value;
                var bPos = b.Read<Translation>().Value;
                return Math.Sign(aPos.y - bPos.y);
            });

            // Determine what floor we are on
            var floorIndex = -1;
            for (int i = 0; i < floors.Length; i++)
            {
                var floorPos = floors[i].Read<Translation>().Value;
                if (floorPos.y <= charPos.y)
                {
                    floorIndex = i;
                }
                else
                {
                    break;
                }
            }

            floorIndex = Math.Min(floors.Length - 1, floorIndex + numFloors);
            var newCharPos = charPos;
            if (floorIndex >= 0)
                newCharPos.y = floors[floorIndex].Read<Translation>().Value.y;
            else
                newCharPos.y = 0;
            charEntity.Write(new Translation() { Value = newCharPos });
            charEntity.Write(new LastTranslation() { Value = newCharPos });

            ctx.Reply($"Moved up {numFloors} floors to {(floorIndex < 0 ? "ground" : $"floor {floorIndex + 1}")}");
        }

        [Command("floordown", "fd", description: "Move down a floor", adminOnly: true)]
        public static void FloorDown(ChatCommandContext ctx, int numFloors = 1)
        {
            var charEntity = ctx.Event.SenderCharacterEntity;
            var charPos = charEntity.Read<Translation>().Value;
            var gridPos = Helper.ConvertPosToGrid(charPos);


            var floors = Helper.GetAllEntitiesInRadius<CastleFloor>(charPos.xz, 5).
                Where(f =>
                {
                    if (!f.Has<TileBounds>())
                        return false;
                    var tb = f.Read<TileBounds>().Value;
                    return tb.Min.x <= gridPos.x && tb.Max.x >= gridPos.x && tb.Min.y <= gridPos.z && tb.Max.y >= gridPos.z;
                }).ToArray();

            Array.Sort(floors, (a, b) =>
            {
                var aPos = a.Read<Translation>().Value;
                var bPos = b.Read<Translation>().Value;
                return Math.Sign(aPos.y - bPos.y);
            });

            // Determine what floor we are on
            var floorIndex = -1;
            for (int i = 0; i < floors.Length; i++)
            {
                var floorPos = floors[i].Read<Translation>().Value;
                if (floorPos.y <= charPos.y)
                {
                    floorIndex = i;
                }
                else
                {
                    break;
                }
            }

            // Log out the floors with their index
            Core.Log.LogInfo($"Player is at {charPos.y} on floor {floorIndex}");
            for (int i = 0; i < floors.Length; i++)
            {
                var floor = floors[i];
                var floorPos = floor.Read<Translation>().Value;
                Core.Log.LogInfo($"Floor {i} at {floorPos.y}");
            }

            floorIndex = Math.Min(floors.Length - 1, floorIndex - numFloors);
            var newCharPos = charPos;
            if (floorIndex >= 0)
                newCharPos.y = floors[floorIndex].Read<Translation>().Value.y;
            else
                newCharPos.y = 0;
            charEntity.Write(new Translation() { Value = newCharPos });
            charEntity.Write(new LastTranslation() { Value = newCharPos });

            ctx.Reply($"Moved down {numFloors} floors to {(floorIndex < 0 ? "ground" : $"floor {floorIndex + 1}")}");
        }
    }
}
