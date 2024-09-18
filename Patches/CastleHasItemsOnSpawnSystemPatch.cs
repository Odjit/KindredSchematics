using KindredSchematics.Commands;
using KindredSchematics.Services;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;

namespace KindredSchematics.Patches;


[HarmonyLib.HarmonyPatch(typeof(CastleHasItemsOnSpawnSystem), nameof(CastleHasItemsOnSpawnSystem.OnUpdate))]
public static class CastleHasItemsOnSpawnSystemPatch
{
    public static void Prefix(CastleHasItemsOnSpawnSystem __instance)
    {
        if (!BuildCommands.BuildingPlacementRestrictionsDisabledSetting.Value) return;

        var spawnCastleEntities = __instance.__query_60442477_0.ToEntityArray(Allocator.Temp);
        foreach (var castleEntity in spawnCastleEntities)
        {
            if (!castleEntity.Has<UserOwner>()) continue;

            var userEntity = castleEntity.Read<UserOwner>().Owner.GetEntityOnServer();
            if (userEntity == Entity.Null) continue;

            var charEntity = userEntity.Read<User>().LocalCharacter.GetEntityOnServer();
            Core.SchematicService.GetFallbackCastleHeart(charEntity, out var castleHeartEntity, out var ownerDoors, out var ownerChests);

            if (castleHeartEntity == Entity.Null)
            {
                if (castleEntity.Has<EditableTileModel>())
                {
                    var etm = castleEntity.Read<EditableTileModel>();
                    etm.CanDismantle = false;
                    castleEntity.Write(etm);
                }

                continue;
            }

            if (!(!ownerDoors && castleEntity.Has<Door>() ||
                  !ownerChests && Helper.EntityIsChest(castleEntity)))
            {
                castleEntity.Write(new CastleHeartConnection { CastleHeartEntity = castleHeartEntity });

                var castleTeamReference = (Entity)castleHeartEntity.Read<TeamReference>().Value;
                var teamData = castleTeamReference.Read<TeamData>();
                castleEntity.Write(castleHeartEntity.Read<UserOwner>());

                if (castleEntity.Has<Team>())
                {
                    castleEntity.Write(new Team() { Value = teamData.TeamValue, FactionIndex = -1 });
                }

                if (castleEntity.Has<TeamReference>())
                {
                    var t = new TeamReference();
                    t.Value._Value = castleTeamReference;
                    castleEntity.Write(t);
                }
            }
            else if(castleEntity.Has<EditableTileModel>())
            {
                var etm = castleEntity.Read<EditableTileModel>();
                etm.CanDismantle = false;
                castleEntity.Write(etm);
            }
        }
        spawnCastleEntities.Dispose();
    }
}


