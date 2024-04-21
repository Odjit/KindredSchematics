using KindredVignettes.Commands.Converter;
using ProjectM;
using ProjectM.Network;
using ProjectM.Tiles;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using VampireCommandFramework;

namespace KindredVignettes.Commands
{
    [CommandGroup("build", "Build commands")]
    internal class BuildCommands
    {
        public static SetDebugSettingEvent BuildingCostsDebugSetting = new SetDebugSettingEvent()
        {
            SettingType = DebugSettingType.BuildCostsDisabled,
            Value = false
        };

        public static SetDebugSettingEvent BuildingPlacementRestrictionsDisabledSetting = new SetDebugSettingEvent()
        {
            SettingType = DebugSettingType.BuildingPlacementRestrictionsDisabled,
            Value = false
        };

        public static SetDebugSettingEvent CastleLimitsDisabledSetting = new SetDebugSettingEvent()
        {
            SettingType = DebugSettingType.CastleLimitsDisabled,
            Value = false
        };

        [Command("free", description: "Makes building costs free for everyone and removes placement restrictions", adminOnly: true)]
        public static void ToggleBuildingCostsCommand(ChatCommandContext ctx)
        {
            var User = ctx.Event.User;
            var debugEventsSystem = Core.Server.GetExistingSystem<DebugEventsSystem>();

            BuildingCostsDebugSetting.Value = !BuildingCostsDebugSetting.Value;
            debugEventsSystem.SetDebugSetting(User.Index, ref BuildingCostsDebugSetting);

            BuildingPlacementRestrictionsDisabledSetting.Value = !BuildingPlacementRestrictionsDisabledSetting.Value;
            debugEventsSystem.SetDebugSetting(User.Index, ref BuildingPlacementRestrictionsDisabledSetting);

            CastleLimitsDisabledSetting.Value = !CastleLimitsDisabledSetting.Value;
            debugEventsSystem.SetDebugSetting(User.Index, ref CastleLimitsDisabledSetting);

            if (BuildingCostsDebugSetting.Value)
            {
                ctx.Reply("Free building enabled globally -- Do not place hearts with this enabled, they will crash the server");
            }
            else
            {
                ctx.Reply("Free building disabled");
            }
        }

        [Command("clearradius", description: "Clears all tile models in a radius", adminOnly: true)]
        public static void ClearRadius(ChatCommandContext ctx, float radius)
        {
            var charEntity = ctx.Event.SenderCharacterEntity;
            var charPos = charEntity.Read<Translation>().Value;

            Helper.DestroyEntitiesForBuilding(Helper.GetAllEntitiesInRadius<TileModel>(charPos.xz, radius));
            ctx.Reply($"Cleared tiles in radius {radius}");
        }

        static readonly Dictionary<Entity, float2> cornerPos = [];
        [Command("setcorner", description: "Sets a corner for clearing", adminOnly: true)]
        public static void SetCorner(ChatCommandContext ctx)
        {
            var charEntity = ctx.Event.SenderCharacterEntity;
            var charPos = charEntity.Read<Translation>().Value;
            cornerPos[ctx.Event.SenderUserEntity] = charPos.xz;
            ctx.Reply($"Set corner");
        }

        [Command("clearbox", description: "Clears all tile models in a box", adminOnly: true)]
        public static void ClearBox(ChatCommandContext ctx)
        {
            if (!cornerPos.ContainsKey(ctx.Event.SenderUserEntity))
            {
                ctx.Reply("You need to set the other corner first");
                return;
            }

            var charEntity = ctx.Event.SenderCharacterEntity;
            var charPos = charEntity.Read<Translation>().Value;
            var corner = cornerPos[ctx.Event.SenderUserEntity];
            var halfSize = math.abs(charPos.xz - corner) / 2;
            var center = (charPos.xz + corner) / 2;
            Helper.DestroyEntitiesForBuilding(Helper.GetAllEntitiesInBox<TileModel>(center, halfSize));
            ctx.Reply($"Cleared tiles in box");
            cornerPos.Remove(ctx.Event.SenderUserEntity);
        }

        [Command("delete", description: "Delete the tile model closest to the mouse cursor", adminOnly: true)]
        public static void ClearTile(ChatCommandContext ctx)
        {
            var aimPos = ctx.Event.SenderCharacterEntity.Read<EntityAimData>().AimPosition;

            var closest = Helper.FindClosest<TileModel>(aimPos, "TM_");
            var prefabName = closest.Read<PrefabGUID>().LookupName();

            Helper.DestroyEntitiesForBuilding(new Entity[] { closest });
            ctx.Reply($"Deleted tile {prefabName}");
        }

        [Command("spawn", description: "Spawns a tile at the player's location", adminOnly: true)]
        public static void SpawnTile(ChatCommandContext ctx, FoundTileModel tile)
        {
            if (!Core.PrefabCollection.PrefabLookupMap.TryGetValue(tile.Value, out var prefab))
            {
                ctx.Reply("Tile not found");
                return;
            }

            var spawnPos = ctx.Event.SenderCharacterEntity.Read<EntityAimData>().AimPosition;
            var rot = ctx.Event.SenderCharacterEntity.Read<Rotation>().Value;

            var entity = Core.EntityManager.Instantiate(prefab);
            entity.Write(new Translation { Value = spawnPos });
            entity.Write(new Rotation { Value = rot });

            if(entity.Has<TilePosition>())
            {
                var tilePos = entity.Read<TilePosition>();
                // Get rotation around Y axis
                var euler = rot.ToEulerAngles();
                tilePos.TileRotation = (TileRotation)(Mathf.Floor((360 - math.degrees(euler.y) - 45) / 90) % 4);
                entity.Write(tilePos);

                var stc = entity.Read<StaticTransformCompatible>();
                stc.NonStaticTransform_Rotation = tilePos.TileRotation;
                entity.Write(stc);

                entity.Write(new Rotation { Value = quaternion.RotateY(math.radians(90 * (int)tilePos.TileRotation)) });
            }
        }

        [Command("rotate", description: "Rotates the tile closest to mouse cursor by 90 degrees", adminOnly: true)]
        public static void RotateTile(ChatCommandContext ctx)
        {
            var aimPos = ctx.Event.SenderCharacterEntity.Read<EntityAimData>().AimPosition;

            var closest = Helper.FindClosest<TilePosition>(aimPos, "TM_");
            var tilePos = closest.Read<TilePosition>();
            tilePos.TileRotation = (TileRotation)((((int)tilePos.TileRotation) + 1) % 4);
            closest.Write(tilePos);

            var stc = closest.Read<StaticTransformCompatible>();
            stc.NonStaticTransform_Rotation = tilePos.TileRotation;
            closest.Write(stc);

            closest.Write(new Rotation { Value = quaternion.RotateY(math.radians(90 * (int)tilePos.TileRotation)) });

            ctx.Reply($"Rotated tile {closest.Read<PrefabGUID>().LookupName()} to rotation {tilePos.TileRotation}");
        }
    }
}
