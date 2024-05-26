using Il2CppSystem.Text;
using KindredVignettes.Commands.Converter;
using ProjectM;
using ProjectM.Network;
using ProjectM.Tiles;
using Stunlock.Core;
using System;
using System.Collections.Generic;
using System.Linq;
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

        [Command("free", "f", description: "Makes building costs free for everyone", adminOnly: true)]
        public static void ToggleBuildingCostsCommand(ChatCommandContext ctx)
        {
            if (Core.ConfigSettings.FreeBuildDisabled)
            {
                ctx.Reply("Free building is disabled in the config and has to be manually edited to be reenabled");
                return;
            }
            var User = ctx.Event.User;
            var debugEventsSystem = Core.Server.GetExistingSystemManaged<DebugEventsSystem>();

            BuildingCostsDebugSetting.Value = !BuildingCostsDebugSetting.Value;
            debugEventsSystem.SetDebugSetting(User.Index, ref BuildingCostsDebugSetting);

            if (BuildingCostsDebugSetting.Value)
            {
                ctx.Reply("Free building enabled globally");
            }
            else
            {
                ctx.Reply("Free building disabled");
            }
        }

        [Command("restrictions", "r", description: "Toggles building placement restrictions", adminOnly: true)]
        public static void ToggleBuildingPlacementRestrictions(ChatCommandContext ctx)
        {
            if (Core.ConfigSettings.FreeBuildDisabled)
            {
                ctx.Reply("Free building is disabled in the config and has to be manually edited to be reenabled");
                return;
            }
            var User = ctx.Event.User;
            var debugEventsSystem = Core.Server.GetExistingSystemManaged<DebugEventsSystem>();

            BuildingPlacementRestrictionsDisabledSetting.Value = !BuildingPlacementRestrictionsDisabledSetting.Value;
            debugEventsSystem.SetDebugSetting(User.Index, ref BuildingPlacementRestrictionsDisabledSetting);

            CastleLimitsDisabledSetting.Value = !CastleLimitsDisabledSetting.Value;
            debugEventsSystem.SetDebugSetting(User.Index, ref CastleLimitsDisabledSetting);

            if (BuildingPlacementRestrictionsDisabledSetting.Value)
            {
                ctx.Reply("Building placement restrictions disabled. <color=red>Don't place hearts or the server will crash</color>");
            }
            else
            {
                ctx.Reply("Building placement restrictions enabled");
            }
        }

        [Command("disablefreebuild", description: "Disables free building command", adminOnly: true)]
        public static void DisableFreeBuild(ChatCommandContext ctx)
        {
            Core.ConfigSettings.FreeBuildDisabled = true;
            ctx.Reply("Free building command disabled");
        }

        [Command("clearradius", "cr", description: "Clears all tile models in a radius", adminOnly: true)]
        public static void ClearRadius(ChatCommandContext ctx, float radius)
        {
            var charEntity = ctx.Event.SenderCharacterEntity;
            var charPos = charEntity.Read<Translation>().Value;

            Helper.DestroyEntitiesForBuilding(Helper.GetAllEntitiesInRadius<Translation>(charPos.xz, radius));
            ctx.Reply($"Cleared tiles in radius {radius}");
        }

        static readonly Dictionary<Entity, float2> cornerPos = [];
        [Command("setcorner", "sc", description: "Sets a corner for clearing", adminOnly: true)]
        public static void SetCorner(ChatCommandContext ctx)
        {
            var charEntity = ctx.Event.SenderCharacterEntity;
            var charPos = charEntity.Read<Translation>().Value;
            cornerPos[ctx.Event.SenderUserEntity] = charPos.xz;
            ctx.Reply($"Set corner");
        }

        [Command("clearbox", "cb", description: "Clears all tile models in a box", adminOnly: true)]
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

        [Command("clearterritory", "ct", description: "Clears all tile models in a territory", adminOnly: true)]
        public static void ClearTerritory(ChatCommandContext ctx, int territoryIndex)
        {
            Helper.DestroyEntitiesForBuilding(Helper.GetAllEntitiesInTerritory<TileModel>(territoryIndex));
            ctx.Reply($"Cleared tiles in territory {territoryIndex}");
        }

        [Command("delete", description: "Delete the tile model closest to the mouse cursor", adminOnly: true)]
        public static void DeleteTile(ChatCommandContext ctx)
        {
            var startTime = Time.realtimeSinceStartup;
            var aimPos = ctx.Event.SenderCharacterEntity.Read<EntityAimData>().AimPosition;

            Core.Log.LogInfo($"{Time.realtimeSinceStartup - startTime} Deleting tile at {aimPos} now finding closest tile model");
            var closest = Helper.FindClosest<TilePosition>(aimPos, "TM_");
            Core.Log.LogInfo($"{Time.realtimeSinceStartup - startTime} Found closest tile model {closest}");
            var prefabName = closest.Read<PrefabGUID>().LookupName();
            Core.Log.LogInfo($"{Time.realtimeSinceStartup - startTime} Deleting tile {prefabName}");

            Helper.DestroyEntitiesForBuilding(new Entity[] { closest });
            Core.Log.LogInfo($"{Time.realtimeSinceStartup - startTime} Deleted tile {prefabName}");
            ctx.Reply($"Deleted tile {prefabName}");
        }

        [Command("spawn", description: "Spawns a tile at the player's location", adminOnly: true)]
        public static void SpawnTile(ChatCommandContext ctx, FoundTileModel tile)
        {
            if (!Core.PrefabCollection._PrefabLookupMap.TryGetValue(tile.Value, out var prefab))
            {
                ctx.Reply("Tile not found");
                return;
            }

            var spawnPos = ctx.Event.SenderCharacterEntity.Read<EntityAimData>().AimPosition;
            var rot = ctx.Event.SenderCharacterEntity.Read<Rotation>().Value;

            var entity = Core.EntityManager.Instantiate(prefab);
            entity.Write(new Translation { Value = spawnPos });
            entity.Write(new Rotation { Value = rot });

            if (entity.Has<TilePosition>())
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

        [Command("immortal", description: "Makes the tile closest to mouse cursor immortal", adminOnly: true)]
        public static void ImmortalTile(ChatCommandContext ctx)
        {
            var aimPos = ctx.Event.SenderCharacterEntity.Read<EntityAimData>().AimPosition;

            var closest = Helper.FindClosest<TilePosition>(aimPos, "TM_");
            if (!closest.Has<Immortal>())
                closest.Add<Immortal>();
            closest.Write(new Immortal { IsImmortal = true });

            ctx.Reply($"Made tile {closest.Read<PrefabGUID>().LookupName()} immortal");
        }

        [Command("mortal", description: "Makes the tile closest to mouse cursor mortal", adminOnly: true)]
        public static void MortalTile(ChatCommandContext ctx)
        {
            var aimPos = ctx.Event.SenderCharacterEntity.Read<EntityAimData>().AimPosition;

            var closest = Helper.FindClosest<TilePosition>(aimPos, "TM_");
            if (!closest.Has<Immortal>())
                closest.Add<Immortal>();
            closest.Write(new Immortal { IsImmortal = false });
            ctx.Reply($"Made tile {closest.Read<PrefabGUID>().LookupName()} mortal");
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

        [Command("search", "s", adminOnly: true)]
        public static void SearchTile(ChatCommandContext ctx, string search, int page = 1)
        {
            List<(string Name, PrefabGUID Prefab)> searchResults = [];
            try
            {
                foreach (var kvp in Data.Tile.LowerCaseNameToPrefab)
                {
                    if (kvp.Key.Contains(search, StringComparison.OrdinalIgnoreCase))
                    {
                        searchResults.Add((kvp.Key, kvp.Value));
                    }
                }

                if (!searchResults.Any())
                {
                    ctx.Reply("Could not find any matching prefabs.");
                }

                searchResults = searchResults.OrderBy(kvp => kvp.Name).ToList();

                var sb = new StringBuilder();
                var totalCount = searchResults.Count;
                var pageSize = 7;
                var pageLabel = totalCount > pageSize ? $" (Page {page}/{System.Math.Ceiling(totalCount / (float)pageSize)})" : "";

                if (totalCount > pageSize)
                {
                    searchResults = searchResults.Skip((page - 1) * pageSize).Take(pageSize).ToList();
                }

                sb.AppendLine($"Found {totalCount} matches {pageLabel}:");
                foreach (var (Name, Prefab) in searchResults)
                {
                    sb.AppendLine(
                        $"({Prefab.GuidHash}) {Name.Replace(search, $"<b>{search}</b>", StringComparison.OrdinalIgnoreCase)}");
                }

                ctx.Reply(sb.ToString());
            }
            catch (Exception e)
            {
                Core.LogException(e);
            }
        }
    }
    }

