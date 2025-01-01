using Il2CppSystem.Text;
using KindredSchematics.Commands.Converter;
using ProjectM;
using ProjectM.Behaviours;
using ProjectM.CastleBuilding;
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
using static Unity.Entities.ComponentSystemSorter;
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
                ServerChatUtils.SendSystemMessageToAllClients(Core.EntityManager, "Building placement restrictions disabled. Respawns are also disabled <color=red>Cannot place castlehearts.</color>");
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

            Helper.DestroyEntitiesForBuilding(new Entity[] { closest });
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
            entity.Write(new Translation { Value = spawnPos });
            entity.Write(new Rotation { Value = rot });

            if (entity.Has<TilePosition>())
            {
                var tilePos = entity.Read<TilePosition>();
                // Get rotation around Y axis
                var euler = rot.ToEulerAngles();
                tilePos.TileRotation = (TileRotation)(Mathf.Floor((360 - math.degrees(euler.y) - 45) / 90) % 4);
                entity.Write(tilePos);

                if(entity.Has<StaticTransformCompatible>())
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

        [Command("lock", description: "Prevents the tile from being dismantled", adminOnly: true)]
        public static void LockedTile(ChatCommandContext ctx)
        {
            var aimPos = ctx.Event.SenderCharacterEntity.Read<EntityAimData>().AimPosition;

            var closest = Helper.FindClosestTilePosition(aimPos);
            if (!closest.Has<EditableTileModel>())
            {
                ctx.Reply("Tile is not lockable");
                return;
            }
            var etm = closest.Read<EditableTileModel>();
            etm.CanDismantle = false;
            closest.Write(etm);

            ctx.Reply($"Locked tile {closest.Read<PrefabGUID>().LookupName()}");
        }

        [Command("unlock", description: "Allows the tile to be dismantled", adminOnly: true)]
        public static void UnlockedTile(ChatCommandContext ctx)
        {
            var aimPos = ctx.Event.SenderCharacterEntity.Read<EntityAimData>().AimPosition;

            var closest = Helper.FindClosestTilePosition(aimPos);
            if (!closest.Has<EditableTileModel>())
            {
                ctx.Reply("Tile is not unlockable");
                return;
            }
            var etm = closest.Read<EditableTileModel>();
            etm.CanDismantle = true;
            closest.Write(etm);

            ctx.Reply($"Unlocked tile {closest.Read<PrefabGUID>().LookupName()}");
        }

        [Command("unlockrange", description: "Unlocks all tiles within a range", adminOnly: true)]
        public static void UnlockRange(ChatCommandContext ctx, float range)
        {
            var aimPos = ctx.Event.SenderCharacterEntity.Read<EntityAimData>().AimPosition;

            var closest = Helper.FindClosestTilePosition(aimPos);
            var closestPos = closest.Read<Translation>().Value.xz;

            var tiles = Helper.GetAllEntitiesInRadius<TilePosition>(closestPos, range);
            foreach (var tile in tiles)
            {
                if (tile.Has<EditableTileModel>())
                {
                    var etm = tile.Read<EditableTileModel>();
                    etm.CanDismantle = true;
                    tile.Write(etm);
                }
            }

            ctx.Reply($"Unlocked {tiles.Count()} tiles within {range}");
        }

        [Command("unlockterritory", description: "Unlocks all tiles in a territory", adminOnly: true)]
        public static void UnlockTerritory(ChatCommandContext ctx, int territoryIndex)
        {
            var tiles = Helper.GetAllEntitiesInTerritory<TilePosition>(territoryIndex);
            foreach (var tile in tiles)
            {
                if (tile.Has<EditableTileModel>())
                {
                    var etm = tile.Read<EditableTileModel>();
                    etm.CanDismantle = true;
                    tile.Write(etm);
                }
            }

            ctx.Reply($"Unlocked {tiles.Count()} tiles in territory {territoryIndex}");
        }

        [Command("lockrange", description: "Locks all tiles within a range", adminOnly: true)]
        public static void LockRange(ChatCommandContext ctx, float range)
        {
            var aimPos = ctx.Event.SenderCharacterEntity.Read<EntityAimData>().AimPosition;

            var closest = Helper.FindClosestTilePosition(aimPos);
            var closestPos = closest.Read<Translation>().Value.xz;

            var tiles = Helper.GetAllEntitiesInRadius<TilePosition>(closestPos, range);
            foreach (var tile in tiles)
            {
                if (tile.Has<EditableTileModel>())
                {
                    var etm = tile.Read<EditableTileModel>();
                    etm.CanDismantle = false;
                    tile.Write(etm);
                }
            }

            ctx.Reply($"Locked {tiles.Count()} tiles within {range}");
        }

        [Command("lockterritory", description: "Locks all tiles in a territory", adminOnly: true)]
        public static void LockTerritory(ChatCommandContext ctx, int territoryIndex)
        {
            var tiles = Helper.GetAllEntitiesInTerritory<TilePosition>(territoryIndex);
            foreach (var tile in tiles)
            {
                if (tile.Has<EditableTileModel>())
                {
                    var etm = tile.Read<EditableTileModel>();
                    etm.CanDismantle = false;
                    tile.Write(etm);
                }
            }

            ctx.Reply($"Locked {tiles.Count()} tiles in territory {territoryIndex}");
        }
        [Command("movelock", description: "Prevents the tile from being moved", adminOnly: true)]
        public static void MoveLockedTile(ChatCommandContext ctx)
        {
            var aimPos = ctx.Event.SenderCharacterEntity.Read<EntityAimData>().AimPosition;

            var closest = Helper.FindClosestTilePosition(aimPos);
            if (!closest.Has<EditableTileModel>())
            {
                ctx.Reply("Tile is not lockable");
                return;
            }
            var etm = closest.Read<EditableTileModel>();
            etm.CanMoveAfterBuild = false;
            closest.Write(etm);

            ctx.Reply($"Move locked tile {closest.Read<PrefabGUID>().LookupName()}");
        }

        [Command("moveunlock", description: "Allows the tile to be moved", adminOnly: true)]
        public static void MoveUnlockedTile(ChatCommandContext ctx)
        {
            var aimPos = ctx.Event.SenderCharacterEntity.Read<EntityAimData>().AimPosition;

            var closest = Helper.FindClosestTilePosition(aimPos);
            if (!closest.Has<EditableTileModel>())
            {
                ctx.Reply("Tile is not unlockable");
                return;
            }
            var etm = closest.Read<EditableTileModel>();
            etm.CanMoveAfterBuild = true;
            closest.Write(etm);

            ctx.Reply($"Move unlocked tile {closest.Read<PrefabGUID>().LookupName()}");
        }

        [Command("movelockterritory", description: "Unlocks all tiles in a territory", adminOnly: true)]
        public static void MoveLockTerritory(ChatCommandContext ctx, int territoryIndex)
        {
            var tiles = Helper.GetAllEntitiesInTerritory<TilePosition>(territoryIndex);
            foreach (var tile in tiles)
            {
                if (tile.Has<EditableTileModel>())
                {
                    var etm = tile.Read<EditableTileModel>();
                    etm.CanMoveAfterBuild = false;
                    tile.Write(etm);
                }
            }

            ctx.Reply($"Move locked {tiles.Count()} tiles in territory {territoryIndex}");
        }

        [Command("moveunlockterritory", description: "Unlocks all tiles in a territory", adminOnly: true)]
        public static void MoveUnlockTerritory(ChatCommandContext ctx, int territoryIndex)
        {
            var tiles = Helper.GetAllEntitiesInTerritory<TilePosition>(territoryIndex);
            foreach (var tile in tiles)
            {
                if (tile.Has<EditableTileModel>())
                {
                    var etm = tile.Read<EditableTileModel>();
                    etm.CanMoveAfterBuild = true;
                    tile.Write(etm);
                }
            }

            ctx.Reply($"Move unlocked {tiles.Count()} tiles in territory {territoryIndex}");
        }


        [Command("immortal", description: "Makes the tile closest to mouse cursor immortal", adminOnly: true)]
        public static void ImmortalTile(ChatCommandContext ctx)
        {
            var aimPos = ctx.Event.SenderCharacterEntity.Read<EntityAimData>().AimPosition;

            var closest = Helper.FindClosestTilePosition(aimPos);
            if (!closest.Has<Immortal>())
                closest.Add<Immortal>();
            closest.Write(new Immortal { IsImmortal = true });

            if (closest.Has<EntityCategory>())
            {
                var entityCategory = closest.Read<EntityCategory>();
                if (entityCategory.MaterialCategory == MaterialCategory.Vegetation)
                {
                    entityCategory.MaterialCategory = MaterialCategory.Mineral;
                    closest.Write(entityCategory);

                }
            }

            ctx.Reply($"Made tile {closest.Read<PrefabGUID>().LookupName()} immortal");
        }

        [Command("immortalrange", "ir", description: "Makes all tiles within a range immortal", adminOnly: true)]
        public static void ImmortalRange(ChatCommandContext ctx, float range)
        {
            var aimPos = ctx.Event.SenderCharacterEntity.Read<EntityAimData>().AimPosition;

            var closestPos = ctx.Event.SenderCharacterEntity.Read<Translation>().Value.xz;

            var tiles = Helper.GetAllEntitiesInRadius<TilePosition>(closestPos, range)
                .Where(e => e.Read<PrefabGUID>().LookupName().StartsWith("TM_"));
            foreach (var tile in tiles)
            {
                if (!tile.Has<Immortal>())
                    tile.Add<Immortal>();
                tile.Write(new Immortal { IsImmortal = true });
                if (tile.Has<EntityCategory>())
                {
                    var entityCategory = tile.Read<EntityCategory>();
                    if (entityCategory.MaterialCategory == MaterialCategory.Vegetation)
                    {
                        entityCategory.MaterialCategory = MaterialCategory.Mineral;
                        tile.Write(entityCategory);
                    }
                }
            }

            ctx.Reply($"Made {tiles.Count()} tiles within {range} immortal");
        }

        [Command("mortal", description: "Makes the tile closest to mouse cursor mortal", adminOnly: true)]
        public static void MortalTile(ChatCommandContext ctx)
        {
            var aimPos = ctx.Event.SenderCharacterEntity.Read<EntityAimData>().AimPosition;

            var closest = Helper.FindClosestTilePosition(aimPos);
            if (closest.Has<Immortal>())
                closest.Remove<Immortal>();
            
            if (closest.Has<EntityCategory>())
            {
                var prefabGuid = closest.Read<PrefabGUID>();
                var prefab = Core.PrefabCollection._PrefabLookupMap[prefabGuid];
                var entityCategory = prefab.Read<EntityCategory>();
                if (entityCategory.MaterialCategory == MaterialCategory.Vegetation)
                {
                    closest.Write(entityCategory);
                }
            }
            ctx.Reply($"Made tile {closest.Read<PrefabGUID>().LookupName()} mortal");
        }

        [Command("mortalrange", "mr", description: "Makes all tiles within a range mortal", adminOnly: true)]
        public static void MortalRange(ChatCommandContext ctx, float range)
        {
            var aimPos = ctx.Event.SenderCharacterEntity.Read<EntityAimData>().AimPosition;

            var closestPos = ctx.Event.SenderCharacterEntity.Read<Translation>().Value.xz;

            var tiles = Helper.GetAllEntitiesInRadius<TilePosition>(closestPos, range)
                .Where(e => e.Read<PrefabGUID>().LookupName().StartsWith("TM_"));
            foreach (var tile in tiles)
            {
                if (tile.Has<Immortal>())
                    tile.Remove<Immortal>();

                if (tile.Has<EntityCategory>())
                {
                    var prefabGuid = tile.Read<PrefabGUID>();
                    var prefab = Core.PrefabCollection._PrefabLookupMap[prefabGuid];
                    var entityCategory = prefab.Read<EntityCategory>();
                    if (entityCategory.MaterialCategory == MaterialCategory.Vegetation)
                    {
                        tile.Write(entityCategory);
                    }
                }
            }

            ctx.Reply($"Made {tiles.Count()} tiles within {range} mortal");
        }

        //[Command("persist", description: "Makes the tile closest to mouse cursor stay loaded when no players are in range. Keeps things loaded, use sparingly.", adminOnly: true)]
        public static void PersistTile(ChatCommandContext ctx)
        {
            var aimPos = ctx.Event.SenderCharacterEntity.Read<EntityAimData>().AimPosition;

            var closest = Helper.FindClosestTilePosition(aimPos);
            if (!closest.Has<CanPreventDisableWhenNoPlayersInRange>())
                closest.Add<CanPreventDisableWhenNoPlayersInRange>();

            closest.Write(new CanPreventDisableWhenNoPlayersInRange());

            ctx.Reply($"Made tile {closest.Read<PrefabGUID>().LookupName()} persist");
        }


        [Command("rotate", description: "Rotates the tile closest to mouse cursor by 90 degrees", adminOnly: true)]
        public static void RotateTile(ChatCommandContext ctx)
        {
            var aimPos = ctx.Event.SenderCharacterEntity.Read<EntityAimData>().AimPosition;

            var closest = Helper.FindClosestTilePosition(aimPos);
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
    }
}

