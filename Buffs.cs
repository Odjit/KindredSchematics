using ProjectM.Network;
using ProjectM;
using Unity.Entities;
using ProjectM.Shared;

namespace KindredVignettes;
internal class Buffs
{
	public static void AddBuff(Entity User, Entity Character, PrefabGUID buffPrefab, bool dontExpire = false)
	{
		var des = Core.Server.GetExistingSystem<DebugEventsSystem>();
		var buffEvent = new ApplyBuffDebugEvent()
		{
			BuffPrefabGUID = buffPrefab
		};

		var fromCharacter = new FromCharacter()
		{
			User = User,
			Character = Character
		};
		des.ApplyBuff(fromCharacter, buffEvent);

		if (dontExpire)
		{
			if (BuffUtility.TryGetBuff(Core.EntityManager, Character, buffPrefab, out var buffEntity))
			{
				if (buffEntity.Has<LifeTime>())
				{
					var lifetime = buffEntity.Read<LifeTime>();
					lifetime.Duration = -1;
					lifetime.EndAction = LifeTimeEndAction.None;
					buffEntity.Write(lifetime);
				}
			}
		}
	}

	public static void RemoveBuff(Entity Character, PrefabGUID buffPrefab)
	{
		if (BuffUtility.TryGetBuff(Core.EntityManager, Character, buffPrefab, out var buffEntity))
		{
			DestroyUtility.Destroy(Core.EntityManager, buffEntity, DestroyDebugReason.TryRemoveBuff);
		}
	}
}
