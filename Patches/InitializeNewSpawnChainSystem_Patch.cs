using HarmonyLib;
using ProjectM;
using System.Linq;


namespace KindredVignettes.Patches;

[HarmonyPatch(typeof(InitializeNewSpawnChainSystem), nameof(InitializeNewSpawnChainSystem.OnUpdate))]
public static class InitializeNewSpawnChainSystem_Patch
{
	public static bool skipOnce = false;
	public static bool Prefix(InitializeNewSpawnChainSystem __instance)
	{
		if(skipOnce)
		{
            skipOnce = false;
			return false;
		}

		return true;
	}
}
