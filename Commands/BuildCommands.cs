using Il2CppSystem.Text;
using KindredSchematics.Commands.Converter;
using KindredSchematics.Services;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Network;
using ProjectM.Tiles;
using Stunlock.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using VampireCommandFramework;
using Math = System.Math;

namespace KindredSchematics.Commands
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

        public static SetDebugSettingEvent FloorPlacementRestrictionsSetting = new SetDebugSettingEvent()
        {
            SettingType = DebugSettingType.FloorPlacementRestrictionsDisabled,
            Value = false
        };

        public static SetDebugSettingEvent FreeBuildingPlacementSetting = new SetDebugSettingEvent()
        {
            SettingType = DebugSettingType.FreeBuildingPlacementEnabled,
            Value = false
        };


        [Command("free", "f", description: "Makes building costs free for everyone (Global)", adminOnly: true)]
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

            FloorPlacementRestrictionsSetting.Value = !FloorPlacementRestrictionsSetting.Value;
            debugEventsSystem.SetDebugSetting(User.Index, ref FloorPlacementRestrictionsSetting);

            if (BuildingCostsDebugSetting.Value)
            {
                ctx.Reply("Free building enabled globally");
            }
            else
            {
                ctx.Reply("Free building disabled");
            }
        }

        [Command("restrictions", "r", description: "Toggles building placement restrictions. Also disables all respawns. This is a GLOBAL mode.", adminOnly: true)]
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
            if (BuildingPlacementRestrictionsDisabledSetting.Value)
            {
                Core.RespawnPrevention.PreventRespawns();
            }
            debugEventsSystem.SetDebugSetting(User.Index, ref BuildingPlacementRestrictionsDisabledSetting);

            CastleLimitsDisabledSetting.Value = !CastleLimitsDisabledSetting.Value;
            debugEventsSystem.SetDebugSetting(User.Index, ref CastleLimitsDisabledSetting);

            if (BuildingPlacementRestrictionsDisabledSetting.Value)
            {
                var message = new FixedString512Bytes("Building placement restrictions disabled. Respawns are also disabled <color=red>Cannot place castlehearts.</color>");
                ServerChatUtils.SendSystemMessageToAllClients(Core.EntityManager, ref message);
            }
            else
            {
                ctx.Reply("Building placement restrictions enabled");
                Core.RespawnPrevention.AllowRespawns();
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
            var closest = Helper.FindClosestTilePosition(aimPos);
            Core.Log.LogInfo($"{Time.realtimeSinceStartup - startTime} Found closest tile model {closest}");
            var prefabName = closest.Read<PrefabGUID>().LookupName();
            Core.Log.LogInfo($"{Time.realtimeSinceStartup - startTime} Deleting tile {prefabName}");

            Helper.DestroyEntitiesForBuilding(new Entity[] { closest }, ignorePortalsAndWaygates: false);
            Core.Log.LogInfo($"{Time.realtimeSinceStartup - startTime} Deleted tile {prefabName}");
            ctx.Reply($"Deleted tile {prefabName}");
        }

        [Command("spawn", description: "Spawns a tile at the player's location", adminOnly: true)]
        public static void SpawnTile(ChatCommandContext ctx, FoundTileModel tile)
        {
            if (!Core.PrefabCollection._PrefabLookupMap.TryGetValueWithoutLogging(tile.Value, out var prefab) &&
                !Core.PrefabCollection._PrefabGuidToEntityMap.TryGetValue(tile.Value, out prefab))
            {
                ctx.Reply("Tile not found");
                return;
            }

            var spawnPos = ctx.Event.SenderCharacterEntity.Read<EntityAimData>().AimPosition;
            var rot = ctx.Event.SenderCharacterEntity.Read<Rotation>().Value;

            var entity = Core.EntityManager.Instantiate(prefab);
            entity.Add<PhysicsCustomTags>();
            entity.Write(new Translation { Value = spawnPos });
            entity.Write(new Rotation { Value = rot });

            if (entity.Has<TilePosition>())
            {
                var tilePos = entity.Read<TilePosition>();
                // Get rotation around Y axis
                var euler = new Quaternion(rot.value.x, rot.value.y, rot.value.z, rot.value.w).eulerAngles;
                tilePos.TileRotation = (TileRotation)(Mathf.Floor((360 - math.degrees(euler.y) - 45) / 90) % 4);
                entity.Write(tilePos);

                if (entity.Has<StaticTransformCompatible>())
                {
                    var stc = entity.Read<StaticTransformCompatible>();
                    stc.NonStaticTransform_Rotation = tilePos.TileRotation;
                    entity.Write(stc);
                }

                entity.Write(new Rotation { Value = quaternion.RotateY(math.radians(90 * (int)tilePos.TileRotation)) });
            }
        }

        [Command("check", description: "Retrieves the prefab of the entity closest to mouse", adminOnly: true)]
        public static void CheckTile(ChatCommandContext ctx)
        {
            var aimPos = ctx.Event.SenderCharacterEntity.Read<EntityAimData>().AimPosition;
            var closest = Helper.FindClosestTilePosition(aimPos);
            var guid = closest.Read<PrefabGUID>();
            ctx.Reply($"PrefabGuid: {guid.LookupName()}");
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

        [Command("changeheart", "ch", description: "Changes the heart of the tile to the current fallback", adminOnly: true)]
        public static void ChangeHeart(ChatCommandContext ctx)
        {
            var aimPos = ctx.Event.SenderCharacterEntity.Read<EntityAimData>().AimPosition;

            var closest = Helper.FindClosestTilePosition(aimPos);

            if (closest == Entity.Null)
            {
                ctx.Reply("No tile found");
                return;
            }

            var prefabGuid = closest.Read<PrefabGUID>();

            if (!closest.Has<CastleHeartConnection>())
            {
                ctx.Reply($"Tile {prefabGuid.LookupName()} does not have a castle heart connection");
                return;
            }

            Core.SchematicService.GetFallbackCastleHeart(ctx.Event.SenderCharacterEntity, out var castleHeartEntity, out var ownerDoors, out var ownerChests);
            if (!ownerDoors && closest.Has<Door>() ||
                !ownerChests && Helper.EntityIsChest(closest))
            {
                if (closest.Has<CastleHeartConnection>())
                    closest.Write(new CastleHeartConnection { CastleHeartEntity = Entity.Null });
                closest.Write(new Team() { Value = 1, FactionIndex = -1 });
                if (closest.Has<TeamReference>())
                {
                    var t = new TeamReference();
                    t.Value._Value = Core.SchematicService.NeutralTeam;
                    closest.Write(t);
                }

                if (closest.Has<EditableTileModel>())
                {
                    var etm = closest.Read<EditableTileModel>();
                    etm.CanDismantle = true;
                    closest.Write(etm);
                }

                ctx.Reply($"Changed {closest.Read<PrefabGUID>().LookupName()} to be neutral");
            }
            else
            {
                if (castleHeartEntity == Entity.Null)
                {
                    ctx.Reply("No fallback castle heart set");
                    return;
                }

                if (closest.Has<CastleHeartConnection>())
                    closest.Write(new CastleHeartConnection { CastleHeartEntity = castleHeartEntity });

                var castleTeamReference = (Entity)castleHeartEntity.Read<TeamReference>().Value;
                var teamData = castleTeamReference.Read<TeamData>();
                closest.Write(castleHeartEntity.Read<UserOwner>());

                if (closest.Has<Team>())
                {
                    closest.Write(new Team() { Value = teamData.TeamValue, FactionIndex = -1 });
                }

                if (closest.Has<TeamReference>())
                {
                    var t = new TeamReference();
                    t.Value._Value = castleTeamReference;
                    closest.Write(t);
                }

                if (closest.Has<EditableTileModel>())
                {
                    var etm = closest.Read<EditableTileModel>();
                    etm.CanDismantle = false;
                    closest.Write(etm);
                }

                ctx.Reply($"Changed the heart connection for {closest.Read<PrefabGUID>().LookupName()}");
            }
        }

        [Command("changeheartrange", "chr", description: "Changes the heart of all tiles within a range to the current fallback", adminOnly: true)]
        public static void ChangeHeartRange(ChatCommandContext ctx, float range)
        {
            Core.SchematicService.GetFallbackCastleHeart(ctx.Event.SenderCharacterEntity, out var castleHeartEntity, out var ownerDoors, out var ownerChests);

            var aimPos = ctx.Event.SenderCharacterEntity.Read<EntityAimData>().AimPosition;

            var closest = Helper.FindClosestTilePosition(aimPos);
            var closestPos = closest.Read<Translation>().Value.xz;

            var tiles = Helper.GetAllEntitiesInRadius<CastleHeartConnection>(closestPos, range);
            foreach (var tile in tiles)
            {
                if (!ownerDoors && tile.Has<Door>() || !ownerChests && Helper.EntityIsChest(tile))
                {
                    if (tile.Has<CastleHeartConnection>())
                        tile.Write(new CastleHeartConnection { CastleHeartEntity = Entity.Null });
                    tile.Write(new Team() { Value = 1, FactionIndex = -1 });
                    if (tile.Has<TeamReference>())
                    {
                        var t = new TeamReference();
                        t.Value._Value = Core.SchematicService.NeutralTeam;
                        tile.Write(t);
                    }

                    if (tile.Has<EditableTileModel>())
                    {
                        var etm = tile.Read<EditableTileModel>();
                        etm.CanDismantle = true;
                        tile.Write(etm);
                    }
                }
                else
                {
                    if (castleHeartEntity == Entity.Null)
                    {
                        ctx.Reply("No fallback castle heart set");
                        return;
                    }

                    if (tile.Has<CastleHeartConnection>())
                        tile.Write(new CastleHeartConnection { CastleHeartEntity = castleHeartEntity });

                    var castleTeamReference = (Entity)castleHeartEntity.Read<TeamReference>().Value;
                    var teamData = castleTeamReference.Read<TeamData>();
                    if (tile.Has<UserOwner>())
                    {
                        tile.Write(castleHeartEntity.Read<UserOwner>());
                    }

                    if (tile.Has<Team>())
                    {
                        tile.Write(new Team() { Value = teamData.TeamValue, FactionIndex = -1 });
                    }

                    if (tile.Has<TeamReference>())
                    {
                        var t = new TeamReference();
                        t.Value._Value = castleTeamReference;
                        tile.Write(t);
                    }
                }
            }
        }



        [Command("lookupheart", "lh", description: "Looks up the heart of the tile", adminOnly: true)]
        public static void LookupHeart(ChatCommandContext ctx)
        {
            var aimPos = ctx.Event.SenderCharacterEntity.Read<EntityAimData>().AimPosition;

            var closest = Helper.FindClosestTilePosition(aimPos);
            if (closest == Entity.Null)
            {
                ctx.Reply("No tile found");
                return;
            }

            if (closest.Has<CastleHeartConnection>())
            {
                var castleHeartEntity = closest.Read<CastleHeartConnection>().CastleHeartEntity.GetEntityOnServer();
                if (castleHeartEntity == Entity.Null)
                {
                    ctx.Reply("No heart connected");
                    return;
                }

                var pos = castleHeartEntity.Read<LocalToWorld>().Position;
                var playerPos = ctx.Event.SenderCharacterEntity.Read<LocalToWorld>().Position;
                var distance = math.distance(playerPos.xz, pos.xz);
                // Want a simple direction of N, NW, W, SW, S, SE, E, NE
                var direction = (int)Math.Round(Math.Atan2(pos.z - playerPos.z, pos.x - playerPos.x) / (Math.PI / 4));
                direction = (direction + 8) % 8;

                var heartData = castleHeartEntity.Read<CastleHeart>();
                var castleTerritoryEntity = heartData.CastleTerritoryEntity;
                var territoryIndex = castleTerritoryEntity.Equals(Entity.Null) ?
                                     -1 :
                                     castleTerritoryEntity.Read<CastleTerritory>().CastleTerritoryIndex;

                var owner = castleHeartEntity.Read<UserOwner>().Owner.GetEntityOnServer();
                var ownerName = "<None>";
                if (owner != Entity.Null)
                    ownerName = owner.Read<User>().CharacterName.ToString();
                ctx.Reply($"Heart connected to {ownerName} on territory {territoryIndex} at {pos} ({distance}m away to the {stringDirections[direction]})");
            }
            else
            {
                ctx.Reply("No heart connected");
            }
        }

        [Command("setfallbackheart", "sfh", description: "Sets the fallback castle heart for loading or building without restrictions to the nearby heart", adminOnly: true)]
        public static void SetFallbackHeart(ChatCommandContext ctx, bool useOwnerDoor = true, bool useOwnerChest = true)
        {
            var castleHearts = Helper.GetEntitiesByComponentType<CastleHeart>();
            var playerPos = ctx.Event.SenderCharacterEntity.Read<LocalToWorld>().Position;
            try
            {
                foreach (var castleHeart in castleHearts)
                {
                    var castleHeartPos = castleHeart.Read<LocalToWorld>().Position;

                    if (Vector3.Distance(playerPos, castleHeartPos) > 5f)
                    {
                        continue;
                    }

                    Core.SchematicService.SetFallbackCastleHeart(ctx.Event.SenderCharacterEntity, castleHeart, useOwnerDoor, useOwnerChest);
                    ctx.Reply("Fallback castle heart set");
                    return;
                }
            }
            finally
            {
                castleHearts.Dispose();
            }

            ctx.Reply("Not close enough to a castle heart");
        }

        [Command("neutraldoors", "nd", description: "Sets the doors to be neutral", adminOnly: true)]
        public static void NeutralDoors(ChatCommandContext ctx)
        {
            Core.SchematicService.UseNeutralDoors(ctx.Event.SenderCharacterEntity);
            ctx.Reply("Doors will be built as neutral");
        }

        [Command("ownerdoors", "od", description: "Sets the doors to be owner based", adminOnly: true)]
        public static void OwnerDoors(ChatCommandContext ctx)
        {
            Core.SchematicService.UseOwnerDoors(ctx.Event.SenderCharacterEntity);
            ctx.Reply("Doors will be owned by the current castle");
        }

        [Command("neutralchests", "nc", description: "Sets the chests to be neutral", adminOnly: true)]
        public static void NeutralChests(ChatCommandContext ctx)
        {
            Core.SchematicService.UseNeutralChests(ctx.Event.SenderCharacterEntity);
            ctx.Reply("Chests will be built as neutral");
        }

        [Command("ownerchests", "oc", description: "Sets the chests to be owner based", adminOnly: true)]
        public static void OwnerChests(ChatCommandContext ctx)
        {
            Core.SchematicService.UseOwnerChests(ctx.Event.SenderCharacterEntity);
            ctx.Reply("Chests will be owned by the current castle");
        }

        static readonly string[] stringDirections = { "W", "SW", "S", "SE", "E", "NE", "N", "NW" };

        [Command("settings", description: "Checks the fallback castle heart, and build augments statuses.", adminOnly: true)]
        public static void CheckFallbackHeart(ChatCommandContext ctx)
        {
            Core.SchematicService.GetFallbackCastleHeart(ctx.Event.SenderCharacterEntity, out var castleHeartEntity, out var ownerDoors, out var ownerChests);
            if (castleHeartEntity == Entity.Null)
            {
                ctx.Reply("No fallback castle heart set");
                return;
            }

            var fallbackHeartPos = castleHeartEntity.Read<LocalToWorld>().Position;
            var playerPos = ctx.Event.SenderCharacterEntity.Read<LocalToWorld>().Position;
            var distance = math.distance(playerPos.xz, fallbackHeartPos.xz);
            // Want a simple direction of N, NW, W, SW, S, SE, E, NE
            var direction = (int)Math.Round(Math.Atan2(fallbackHeartPos.z - playerPos.z, fallbackHeartPos.x - playerPos.x) / (Math.PI / 4));
            direction = (direction + 8) % 8;

            // Get the territory index
            var heartData = castleHeartEntity.Read<CastleHeart>();
            var castleTerritoryEntity = heartData.CastleTerritoryEntity;
            var territoryIndex = castleTerritoryEntity.Equals(Entity.Null) ?
                                 -1 :
                                 castleTerritoryEntity.Read<CastleTerritory>().CastleTerritoryIndex;

            var usingNeutralDoors = ownerDoors ? "Owner doors" : "Neutral doors";
            var usingNeutralChests = ownerChests ? "Owner chests" : "Neutral chests";

            var owner = castleHeartEntity.Read<UserOwner>().Owner.GetEntityOnServer();
            var ownerName = "<None>";
            if (owner != Entity.Null)
                ownerName = owner.Read<User>().CharacterName.ToString();

            ctx.Reply($"Fallback castle heart owned by {ownerName} set on territory {territoryIndex} at {fallbackHeartPos} ({distance}m away to the {stringDirections[direction]})\n{usingNeutralDoors}\n{usingNeutralChests}");
        }

        [Command("mode", description:"toggle into and out of build mode", adminOnly: true)]
        public static void SwitchToBuildMode(ChatCommandContext ctx)
        {
            if (Core.BuildService.SwitchToBuildMode(ctx.Event.SenderCharacterEntity))
            {
                ctx.Reply("Switched to build mode");
            }
            else
            {
                ctx.Reply("Switched out of build mode");
            }
        }

        [Command("setcursor", adminOnly: true)]
        public static void SetCursor(ChatCommandContext ctx, FoundTileModel tile)
        {
            if (!BuildService.IsCharacterInBuildMode(ctx.Event.SenderCharacterEntity))
            {
                ctx.Reply("You are not in build mode");
                return;
            }

            if (!Core.PrefabCollection._PrefabLookupMap.TryGetValueWithoutLogging(tile.Value, out var prefab) &&
                !Core.PrefabCollection._PrefabGuidToEntityMap.TryGetValue(tile.Value, out prefab))
            {
                ctx.Reply("Tile not found");
                return;
            }

            Core.BuildService.SetCursor(ctx.Event.SenderCharacterEntity, prefab);
            ctx.Reply($"Set cursor to {tile.Value.LookupName()}");
        }

        [Command("ysnap", description: "Toggles Y-value snapping when placing objects in build mode", adminOnly: true)]
        public static void ToggleBuildSnapping(ChatCommandContext ctx)
        {
            var charEntity = ctx.Event.SenderCharacterEntity;
            bool isNowEnabled = Core.BuildService.ToggleYSnapping(charEntity);
            
            if (isNowEnabled)
            {
                ctx.Reply("Y-value snapping is now <color=green>enabled</color>");
            }
            else
            {
                ctx.Reply("Y-value snapping is now <color=red>disabled</color>");
            }
        }

        [Command("plane", description: "Toggles using AimPositionPlane (default ON) or AimPosition for object movement in build mode", adminOnly: true)]
        public static void ToggleBuildPlane(ChatCommandContext ctx)
        {
            var charEntity = ctx.Event.SenderCharacterEntity;
            bool isNowEnabled = Core.BuildService.TogglePlaneMode(charEntity);

            if (isNowEnabled)
            {
                ctx.Reply("Now using <color=green>AimPositionPlane</color> for object movement.");
            }
            else
            {
                ctx.Reply("Now using <color=yellow>AimPosition</color> for object movement.");
            }
        }

        [Command("yoffset", description: "Sets Y-axis offset when placing objects in build mode", adminOnly: true)]
        public static void SetYOffset(ChatCommandContext ctx, float offset = 0f)
        {
            var charEntity = ctx.Event.SenderCharacterEntity;
            Core.BuildService.SetYOffset(charEntity, offset);
            ctx.Reply($"Y offset set to: <color=green>{offset}</color>");
        }

        [Command("xsnap", description: "Toggles X-value snapping when placing objects in build mode", adminOnly: true)]
        public static void ToggleXSnapping(ChatCommandContext ctx)
        {
            var charEntity = ctx.Event.SenderCharacterEntity;
            bool isNowEnabled = Core.BuildService.ToggleXSnapping(charEntity);
            
            if (isNowEnabled)
            {
                ctx.Reply("X-value snapping is now <color=green>enabled</color>");
            }
            else
            {
                ctx.Reply("X-value snapping is now <color=red>disabled</color>");
            }
        }

        [Command("zsnap", description: "Toggles Z-value snapping when placing objects in build mode", adminOnly: true)]
        public static void ToggleZSnapping(ChatCommandContext ctx)
        {
            var charEntity = ctx.Event.SenderCharacterEntity;
            bool isNowEnabled = Core.BuildService.ToggleZSnapping(charEntity);
            
            if (isNowEnabled)
            {
                ctx.Reply("Z-value snapping is now <color=green>enabled</color>");
            }
            else
            {
                ctx.Reply("Z-value snapping is now <color=red>disabled</color>");
            }
        }

        [Command("snapstatus", description: "Shows current snapping settings for build mode", adminOnly: true)]
        public static void ShowSnappingStatus(ChatCommandContext ctx)
        {
            var charEntity = ctx.Event.SenderCharacterEntity;
            bool xSnap = Core.BuildService.IsXSnappingEnabled(charEntity);
            bool ySnap = Core.BuildService.IsYSnappingEnabled(charEntity);
            bool zSnap = Core.BuildService.IsZSnappingEnabled(charEntity);
            float yOffset = Core.BuildService.GetYOffset(charEntity);
            bool planeMode = Core.BuildService.IsPlaneModeEnabled(charEntity);

            ctx.Reply($"Build settings:\n" +
                      $"X Snapping: {(xSnap ? "<color=green>ON</color>" : "<color=red>OFF</color>")}\n" +
                      $"Y Snapping: {(ySnap ? "<color=green>ON</color>" : "<color=red>OFF</color>")}\n" +
                      $"Z Snapping: {(zSnap ? "<color=green>ON</color>" : "<color=red>OFF</color>")}\n" +
                      $"Y Offset: <color=yellow>{yOffset}</color>\n" +
                      $"Using Plane Mode: {(planeMode ? "<color=green>YES</color> (AimPositionPlane)" : "<color=yellow>NO</color> (AimPosition)")}");
        }

        [Command("teleporters", adminOnly: true)]
        public static void TeleportersEveryone(ChatCommandContext ctx)
        {

            SyncToUserBitMask all = new();
            all.Value._Value = new int4(-1, -1, -1, -1);

            var teleportEntities = Helper.GetEntitiesByComponentType<CastleTeleporterComponent>(includeDisabled: true);
            foreach (var teleportEntity in teleportEntities)
            {
                if (!teleportEntity.Has<SyncToUserBitMask>())
                    teleportEntity.Add<SyncToUserBitMask>();
                if (teleportEntity.Has<DisableWhenNoPlayersInRange>())
                    teleportEntity.Remove<DisableWhenNoPlayersInRange>();
                if (!Core.EntityManager.IsEnabled(teleportEntity))
                {
                    Core.EntityManager.SetEnabled(teleportEntity, true);
                    if (teleportEntity.Has<DisabledDueToNoPlayersInRange>())
                        teleportEntity.Remove<DisabledDueToNoPlayersInRange>();
                }
                teleportEntity.Write(all);
            }
        }
    }
}

