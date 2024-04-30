using HarmonyLib;
using ProjectM;
using Stunlock.Network;

namespace KindredVignettes.Patches;


[HarmonyPatch(typeof(ServerBootstrapSystem), nameof(ServerBootstrapSystem.OnUserConnected))]
public static class OnUserConnected_Patch
{
	public static void Postfix(ServerBootstrapSystem __instance, NetConnectionId netConnectionId)
	{
		Core.InitializeAfterLoaded();
        Plugin.Harmony.Unpatch(typeof(ServerBootstrapSystem).GetMethod("OnUpdate"), typeof(InitializationPatch).GetMethod("Postfix"));
    }
}