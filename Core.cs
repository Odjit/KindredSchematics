using BepInEx.Logging;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using KindredVignettes.Data;
using KindredVignettes.Services;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Physics;
using ProjectM.Scripting;
using System.Collections;
using System.Runtime.CompilerServices;
using Unity.Entities;
using UnityEngine;

namespace KindredVignettes;

internal static class Core
{
	public static World Server { get; } = GetWorld("Server") ?? throw new System.Exception("There is no Server world (yet). Did you install a server mod on the client?");

	public static EntityManager EntityManager { get; } = Server.EntityManager;
    public static double ServerTime => ServerGameManager.ServerTime;
    public static ServerGameManager ServerGameManager => ServerScriptMapper.GetServerGameManager();
    public static CastleBuildingAttachmentBuffSystem CastleBuildingAttachmentBuffSystem { get; } = Server.GetExistingSystemManaged<CastleBuildingAttachmentBuffSystem>();
    public static CastleTerritoryService CastleTerritory { get; private set; }
    public static PrefabCollectionSystem PrefabCollection { get; } = Server.GetExistingSystemManaged<PrefabCollectionSystem>();

    public static RespawnPreventionService RespawnPrevention { get; } = new();
    public static VignetteService VignetteService { get; } = new();
	public static ConfigSettingsService ConfigSettings { get; } = new();

    public const int MAX_REPLY_LENGTH = 509;

	static ServerScriptMapper serverScriptMapper;
	public static ServerScriptMapper ServerScriptMapper { get
		{
			if (serverScriptMapper == null)
			{
                serverScriptMapper = Server.GetExistingSystemManaged<ServerScriptMapper>();
            }
			return serverScriptMapper;
		}
	}

    public static ManualLogSource Log { get; } = Plugin.PluginLog;

    static MonoBehaviour monoBehaviour;

    public static void LogException(System.Exception e, [CallerMemberName] string caller = null)
	{
		Core.Log.LogError($"Failure in {caller}\nMessage: {e.Message} Inner:{e.InnerException?.Message}\n\nStack: {e.StackTrace}\nInner Stack: {e.InnerException?.StackTrace}");
	}


	internal static void InitializeAfterLoaded()
	{
		if (_hasInitialized) return;

		_hasInitialized = true;

        ComponentSaver.ComponentSaver.PopulateComponentSavers();
		CastleTerritory = new();

        Tile.Populate();

        Log.LogInfo($"{nameof(InitializeAfterLoaded)} completed");
	}
	private static bool _hasInitialized = false;

	private static World GetWorld(string name)
	{
		foreach (var world in World.s_AllWorlds)
		{
			if (world.Name == name)
			{
				return world;
			}
		}

		return null;
    }

    public static Coroutine StartCoroutine(IEnumerator routine)
    {
        if (monoBehaviour == null)
        {
            var go = new GameObject("KindredVignettes");
            monoBehaviour = go.AddComponent<IgnorePhysicsDebugSystem>();
            Object.DontDestroyOnLoad(go);
        }

        return monoBehaviour.StartCoroutine(routine.WrapToIl2Cpp());
    }

    public static void StopCoroutine(Coroutine coroutine)
    {
        if (monoBehaviour == null)
        {
            return;
        }

        monoBehaviour.StopCoroutine(coroutine);
    }
}
