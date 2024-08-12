﻿using KindredSchematics.Commands;
using KindredSchematics.Services;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;

namespace KindredSchematics.Patches;


[HarmonyLib.HarmonyPatch(typeof(CastleHasItemsOnSpawnSystem), nameof(CastleHasItemsOnSpawnSystem.OnUpdate))]
public static class GetPlacementResultAsyncSystemPatch
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
            Core.SchematicService.GetFallbackCastleHeart(charEntity, out var castleHeartEntity, out var castleTeamReference);

            castleEntity.Write(new CastleHeartConnection { CastleHeartEntity = castleHeartEntity });
            castleEntity.Write(castleHeartEntity.Read<UserOwner>());

            if (castleEntity.Has<Team>())
            {
                castleEntity.Write(castleHeartEntity.Read<Team>());
            }

            if (castleEntity.Has<TeamReference>())
            {
                var t = new TeamReference();
                t.Value._Value = castleTeamReference;
                castleEntity.Write(t);
            }
        }
    }
}


