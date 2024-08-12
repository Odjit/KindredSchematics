using KindredSchematics.Commands;
using ProjectM;
using ProjectM.Network;
using ProjectM.Shared;
using Unity.Collections;
using Unity.Entities;

namespace KindredSchematics.Patches;

[HarmonyLib.HarmonyPatch(typeof(PlaceTileModelSystem), nameof(PlaceTileModelSystem.OnUpdate))]
static class PlaceTileModelSystemPatch
{
    static void Prefix(PlaceTileModelSystem __instance)
    {
        if (!BuildCommands.BuildingPlacementRestrictionsDisabledSetting.Value) return;

        var buildEvents = __instance._BuildTileQuery.ToEntityArray(Allocator.Temp);

        foreach (var buildEvent in buildEvents)
        {
            var btme = buildEvent.Read<BuildTileModelEvent>();
            if (btme.PrefabGuid == Data.Prefabs.TM_BloodFountain_CastleHeart)
            {
                var fromCharacter = buildEvent.Read<FromCharacter>();
                var user = fromCharacter.User.Read<User>();
                ServerChatUtils.SendSystemMessageToClient(Core.EntityManager, user, "Can't place Castle Hearts while build restrictions are disabled.");
                buildEvent.Add<Disabled>();
                Core.EntityManager.DestroyEntity(buildEvent);
                DestroyUtility.CreateDestroyEvent(Core.EntityManager, buildEvent, DestroyReason.Default, DestroyDebugReason.ByScript);
                continue;
            }
        }

        buildEvents.Dispose();
    }
}