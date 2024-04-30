using ProjectM;
using ProjectM.CastleBuilding;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using VampireCommandFramework;

namespace KindredVignettes.Commands
{
    internal class MiscCommands
    {
        [Command("floorup", "fu", description: "Move up a floor", adminOnly: true)]
        public static void FloorUp(ChatCommandContext ctx, int numFloors = 1)
        {
            var charEntity = ctx.Event.SenderCharacterEntity;
            var charPos = charEntity.Read<Translation>().Value;

            GetFloors(charPos, out var floors, out var floorIndex);

            floorIndex = Math.Min(floors.Count - 1, floorIndex + numFloors);
            var newCharPos = charPos;
            if (floorIndex >= 0)
                newCharPos.y = floors[floorIndex].y;
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

            GetFloors(charPos, out var floors, out var floorIndex);

            floorIndex = Math.Min(floors.Count - 1, floorIndex - numFloors);
            var newCharPos = charPos;
            if (floorIndex >= 0)
                newCharPos.y = floors[floorIndex].y;
            else
                newCharPos.y = 0;
            charEntity.Write(new Translation() { Value = newCharPos });
            charEntity.Write(new LastTranslation() { Value = newCharPos });

            ctx.Reply($"Moved down {numFloors} floors to {(floorIndex < 0 ? "ground" : $"floor {floorIndex + 1}")}");
        }

        static void GetFloors(float3 charPos, out List<(float y, Entity entity, Entity fusedChild)> floors, out int floorIndex)
        {
            var gridPos = Helper.ConvertPosToGrid(charPos);
            floors = Helper.GetAllEntitiesInRadius<CastleFloor>(charPos.xz, 5).
                Where(f =>
                {
                    if (!f.Has<TileBounds>())
                        return false;
                    var tb = f.Read<TileBounds>().Value;
                    return tb.Min.x <= gridPos.x && tb.Max.x >= gridPos.x && tb.Min.y <= gridPos.z && tb.Max.y >= gridPos.z;
                }).Select(entity =>
                {
                    var y = entity.Read<Translation>().Value.y;
                    var fusedChild = entity.Has<CastleBuildingFusedChild>() ? entity.Read<CastleBuildingFusedChild>().ParentEntity.GetEntityOnServer() : Entity.Null;
                    return (y, entity, fusedChild);
                }).ToList();
            floors.Sort((a, b) =>
            {
                var t = a.y;
                return a.y.CompareTo(b.y);
            });

            // Eliminate floors at the same height or with same fused child in reverse order
            for (int i = floors.Count - 1; i > 0; i--)
            {
                if (floors[i].y == floors[i - 1].y ||
                    (!floors[i].fusedChild.Equals(Entity.Null) && floors[i].fusedChild.Equals(floors[i - 1].fusedChild)))
                {
                    floors.RemoveAt(i);
                }
            }

            // Determine what floor we are on
            floorIndex = -1;
            for (int i = 0; i < floors.Count; i++)
            {
                var floorPos = floors[i].y;
                if (floorPos <= charPos.y)
                {
                    floorIndex = i;
                }
                else
                {
                    break;
                }
            }
        }
    }
}
