using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using Stunlock.Network;
using System;
using Unity.Entities;

namespace KindredSchematics.Patches;

[HarmonyPatch(typeof(ServerBootstrapSystem), nameof(ServerBootstrapSystem.OnUserConnected))]
public static class OnUserConnected_Patch
{
    public static void Postfix(ServerBootstrapSystem __instance, NetConnectionId netConnectionId)
    {
        try
        {
            var em = __instance.EntityManager;
            var userIndex = __instance._NetEndPointToApprovedUserIndex[netConnectionId];
            var serverClient = __instance._ApprovedUsersLookup[userIndex];
            var userEntity = serverClient.UserEntity;
            var user = __instance.EntityManager.GetComponentData<User>(userEntity);
            
            if (user.LocalCharacter.GetEntityOnServer() != Entity.Null)
                Core.BuildService.RemoveBuildModeIfActive(user.LocalCharacter.GetEntityOnServer());

        }
        catch (Exception e)
        {
            Core.Log.LogError($"Failure in {nameof(ServerBootstrapSystem.OnUserConnected)}\nMessage: {e.Message} Inner:{e.InnerException?.Message}\n\nStack: {e.StackTrace}\nInner Stack: {e.InnerException?.StackTrace}");
        }
    }
}

[HarmonyPatch(typeof(ServerBootstrapSystem), nameof(ServerBootstrapSystem.OnUserDisconnected))]
public static class OnUserDisconnected_Patch
{
    private static void Prefix(ServerBootstrapSystem __instance, NetConnectionId netConnectionId, ConnectionStatusChangeReason connectionStatusReason, string extraData)
    {
        try
        {
            var userIndex = __instance._NetEndPointToApprovedUserIndex[netConnectionId];
            var serverClient = __instance._ApprovedUsersLookup[userIndex];
            var user = __instance.EntityManager.GetComponentData<User>(serverClient.UserEntity);
            if (user.LocalCharacter.GetEntityOnServer() != Entity.Null)
                Core.BuildService.RemoveBuildModeIfActive(user.LocalCharacter.GetEntityOnServer());
        }
        catch { };
    }
}
