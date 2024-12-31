using ProjectM.CastleBuilding;
using Unity.Transforms;
using UnityEngine;
using VampireCommandFramework;

namespace KindredSchematics.Commands
{
    [CommandGroup("heartlimit", "hl")]
    class HeartLimitCommands
    {
        [Command("floorcount", "fc", description: "change the floorcount of the heart you are next to", adminOnly: true)]
        public static void ChangeFloorCount(ChatCommandContext ctx, int floorCount)
        {
            var castleHearts = Helper.GetEntitiesByComponentType<CastleHeart>();
            var playerPos = ctx.Event.SenderCharacterEntity.Read<LocalToWorld>().Position;
            foreach (var castleHeart in castleHearts)
            {
                var castleHeartPos = castleHeart.Read<LocalToWorld>().Position;

                if (Vector3.Distance(playerPos, castleHeartPos) > 5f)
                {
                    continue;
                }

                var castleHeartComponent = castleHeart.Read<CastleHeart>();
                castleHeartComponent.FloorCount = floorCount;
                castleHeart.Write(castleHeartComponent);
                ctx.Reply($"Changed floor count to {floorCount}");
                return;
            }
            ctx.Reply("Not close enough to a castle heart");

        }

        [Command("blockrelocate", "br", description: "Blocks the ability to relocate the castle", adminOnly: true)]
        public static void BlockRelocation(ChatCommandContext ctx)
        {
            var castleHearts = Helper.GetEntitiesByComponentType<CastleHeart>();
            var playerPos = ctx.Event.SenderCharacterEntity.Read<LocalToWorld>().Position;
            foreach (var castleHeart in castleHearts)
            {
                var castleHeartPos = castleHeart.Read<LocalToWorld>().Position;

                if (Vector3.Distance(playerPos, castleHeartPos) > 5f)
                {
                    continue;
                }

                var castleHeartComponent = castleHeart.Read<CastleHeart>();
                castleHeartComponent.LastRelocationTime = double.PositiveInfinity;
                castleHeart.Write(castleHeartComponent);
                ctx.Reply("Relocation Blocked");
                return;
            }
            ctx.Reply("Not close enough to a castle heart");

        }

        [Command("tombcount", "tc", description: "change the tomb count of the heart you are next to", adminOnly: true)]
        public static void ChangeTombCount(ChatCommandContext ctx, byte tombCount)
        {
            var castleHearts = Helper.GetEntitiesByComponentType<CastleHeart>();
            var playerPos = ctx.Event.SenderCharacterEntity.Read<LocalToWorld>().Position;
            foreach (var castleHeart in castleHearts)
            {
                var castleHeartPos = castleHeart.Read<LocalToWorld>().Position;

                if (Vector3.Distance(playerPos, castleHeartPos) > 5f)
                {
                    continue;
                }

                var castleHeartComponent = castleHeart.Read<CastleHeart>();
                castleHeartComponent.TombCount = tombCount;
                castleHeart.Write(castleHeartComponent);
                ctx.Reply($"Changed floor count to {tombCount}");
                return;
            }
            ctx.Reply("Not close enough to a castle heart");

        }

        [Command("nestcount", "nc", description: "change the nest count of the heart you are next to", adminOnly: true)]
        public static void ChangeNestCount(ChatCommandContext ctx, byte nestCount)
        {
            var castleHearts = Helper.GetEntitiesByComponentType<CastleHeart>();
            var playerPos = ctx.Event.SenderCharacterEntity.Read<LocalToWorld>().Position;
            foreach (var castleHeart in castleHearts)
            {
                var castleHeartPos = castleHeart.Read<LocalToWorld>().Position;

                if (Vector3.Distance(playerPos, castleHeartPos) > 5f)
                {
                    continue;
                }

                var castleHeartComponent = castleHeart.Read<CastleHeart>();
                castleHeartComponent.NestCount = nestCount;
                castleHeart.Write(castleHeartComponent);
                ctx.Reply($"Changed floor count to {nestCount}");
                return;
            }
            ctx.Reply("Not close enough to a castle heart");

        }

        [Command("safetyboxcount", "sbc", description: "change the safety box count of the heart you are next to", adminOnly: true)]
        public static void ChangeSafetyBoxCount(ChatCommandContext ctx, byte safetyBoxCount)
        {
            var castleHearts = Helper.GetEntitiesByComponentType<CastleHeart>();
            var playerPos = ctx.Event.SenderCharacterEntity.Read<LocalToWorld>().Position;
            foreach (var castleHeart in castleHearts)
            {
                var castleHeartPos = castleHeart.Read<LocalToWorld>().Position;

                if (Vector3.Distance(playerPos, castleHeartPos) > 5f)
                {
                    continue;
                }

                var castleHeartComponent = castleHeart.Read<CastleHeart>();
                castleHeartComponent.SafetyBoxCount = safetyBoxCount;
                castleHeart.Write(castleHeartComponent);
                ctx.Reply($"Changed floor count to {safetyBoxCount}");
                return;
            }
            ctx.Reply("Not close enough to a castle heart");

        }

        [Command("eyestructurescount", "esc", description: "change the eye structures count of the heart you are next to", adminOnly: true)]
        public static void ChangeEyeStructuresCount(ChatCommandContext ctx, byte eyeStructuresCount)
        {
            var castleHearts = Helper.GetEntitiesByComponentType<CastleHeart>();
            var playerPos = ctx.Event.SenderCharacterEntity.Read<LocalToWorld>().Position;
            foreach (var castleHeart in castleHearts)
            {
                var castleHeartPos = castleHeart.Read<LocalToWorld>().Position;

                if (Vector3.Distance(playerPos, castleHeartPos) > 5f)
                {
                    continue;
                }

                var castleHeartComponent = castleHeart.Read<CastleHeart>();
                castleHeartComponent.EyeStructuresCount = eyeStructuresCount;
                castleHeart.Write(castleHeartComponent);
                ctx.Reply($"Changed floor count to {eyeStructuresCount}");
                return;
            }
            ctx.Reply("Not close enough to a castle heart");

        }
        [Command("prisoncellcount", "pcc", description: "change the prison cell count of the heart you are next to", adminOnly: true)]
        public static void ChangePrisonCellCount(ChatCommandContext ctx, byte prisonCellCount)
        {
            var castleHearts = Helper.GetEntitiesByComponentType<CastleHeart>();
            var playerPos = ctx.Event.SenderCharacterEntity.Read<LocalToWorld>().Position;
            foreach (var castleHeart in castleHearts)
            {
                var castleHeartPos = castleHeart.Read<LocalToWorld>().Position;

                if (Vector3.Distance(playerPos, castleHeartPos) > 5f)
                {
                    continue;
                }

                var castleHeartComponent = castleHeart.Read<CastleHeart>();
                castleHeartComponent.PrisonCellCount = prisonCellCount;
                castleHeart.Write(castleHeartComponent);
                ctx.Reply($"Changed floor count to {prisonCellCount}");
                return;
            }
            ctx.Reply("Not close enough to a castle heart");

        }

        [Command("servantcount", "sc", description: "change the servant count of the heart you are next to", adminOnly: true)]
        public static void ChangeServantCount(ChatCommandContext ctx, byte servantCount)
        {
            var castleHearts = Helper.GetEntitiesByComponentType<CastleHeart>();
            var playerPos = ctx.Event.SenderCharacterEntity.Read<LocalToWorld>().Position;
            foreach (var castleHeart in castleHearts)
            {
                var castleHeartPos = castleHeart.Read<LocalToWorld>().Position;

                if (Vector3.Distance(playerPos, castleHeartPos) > 5f)
                {
                    continue;
                }

                var castleHeartComponent = castleHeart.Read<CastleHeart>();
                castleHeartComponent.ServantCount = servantCount;
                castleHeart.Write(castleHeartComponent);
                ctx.Reply($"Changed floor count to {servantCount}");
                return;
            }
            ctx.Reply("Not close enough to a castle heart");

        }

        [Command("nethergatecount", "ngc", description: "change the nether gate count of the heart you are next to", adminOnly: true)]
        public static void ChangeNetherGateCount(ChatCommandContext ctx, byte netherGateCount)
        {
            var castleHearts = Helper.GetEntitiesByComponentType<CastleHeart>();
            var playerPos = ctx.Event.SenderCharacterEntity.Read<LocalToWorld>().Position;
            foreach (var castleHeart in castleHearts)
            {
                var castleHeartPos = castleHeart.Read<LocalToWorld>().Position;

                if (Vector3.Distance(playerPos, castleHeartPos) > 5f)
                {
                    continue;
                }

                var castleHeartComponent = castleHeart.Read<CastleHeart>();
                castleHeartComponent.NetherGateCount = netherGateCount;
                castleHeart.Write(castleHeartComponent);
                ctx.Reply($"Changed floor count to {netherGateCount}");
                return;
            }
            ctx.Reply("Not close enough to a castle heart");

        }

        [Command("throneofdarknesscount", "todc", description: "change the throne of darkness count of the heart you are next to", adminOnly: true)]
        public static void ChangeThroneOfDarknessCount(ChatCommandContext ctx, byte throneOfDarknessCount)
        {
            var castleHearts = Helper.GetEntitiesByComponentType<CastleHeart>();
            var playerPos = ctx.Event.SenderCharacterEntity.Read<LocalToWorld>().Position;
            foreach (var castleHeart in castleHearts)
            {
                var castleHeartPos = castleHeart.Read<LocalToWorld>().Position;

                if (Vector3.Distance(playerPos, castleHeartPos) > 5f)
                {
                    continue;
                }

                var castleHeartComponent = castleHeart.Read<CastleHeart>();
                castleHeartComponent.ThroneOfDarknessCount = throneOfDarknessCount;
                castleHeart.Write(castleHeartComponent);
                ctx.Reply($"Changed floor count to {throneOfDarknessCount}");
                return;
            }
            ctx.Reply("Not close enough to a castle heart");

        }

        [Command("musicplayercount", "mpc", description: "change the music player count of the heart you are next to", adminOnly: true)]
        public static void ChangeMusicPlayerCount(ChatCommandContext ctx, byte musicPlayerCount)
        {
            var castleHearts = Helper.GetEntitiesByComponentType<CastleHeart>();
            var playerPos = ctx.Event.SenderCharacterEntity.Read<LocalToWorld>().Position;
            foreach (var castleHeart in castleHearts)
            {
                var castleHeartPos = castleHeart.Read<LocalToWorld>().Position;

                if (Vector3.Distance(playerPos, castleHeartPos) > 5f)
                {
                    continue;
                }

                var castleHeartComponent = castleHeart.Read<CastleHeart>();
                castleHeartComponent.MusicPlayerCount = musicPlayerCount;
                castleHeart.Write(castleHeartComponent);
                ctx.Reply($"Changed floor count to {musicPlayerCount}");
                return;
            }
            ctx.Reply("Not close enough to a castle heart");
        }
    }
}
