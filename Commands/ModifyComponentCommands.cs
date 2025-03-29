using ProjectM;
using ProjectM.Tiles;
using Stunlock.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Transforms;
using VampireCommandFramework;

namespace KindredSchematics.Commands;

[CommandGroup("modifytile", "modt")]
internal class ModifyComponentCommands
{
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

    [Command("pavementspeed", description: "Changes the PavementBonusSource component's MovementSpeed to the given value for the entire territory that you're on", adminOnly: true)]
    public static void PavementSpeed(ChatCommandContext ctx, float speed, int territoryIndex)
    {
        var tiles = Helper.GetAllEntitiesInTerritory<TilePosition>(territoryIndex);
        foreach (var tile in tiles)
        {
            if (tile.Has<PavementBonusSource>())
            {
                var etm = tile.Read<PavementBonusSource>();
                etm.MovementSpeed = speed;
                tile.Write(etm);
            }
        }

        ctx.Reply($"Move unlocked {tiles.Count()} tiles in territory {territoryIndex}");
    }
}

