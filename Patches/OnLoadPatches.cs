using HarmonyLib;
using Unity.Scenes;

namespace KindredSchematics.Patches;

[HarmonyPatch(typeof(SceneSectionStreamingSystem), nameof(SceneSectionStreamingSystem.ShutdownAsynchrnonousStreamingSupport))]
public static class InitializationPatch
{
	[HarmonyPostfix]
	public static void OneShot_AfterLoad_InitializationPatch()
    {
        Core.InitializeAfterLoaded();
		Plugin.Harmony.Unpatch(typeof(SceneSectionStreamingSystem).GetMethod("ShutdownAsynchrnonousStreamingSupport"), typeof(InitializationPatch).GetMethod("OneShot_AfterLoad_InitializationPatch"));
	}
}
