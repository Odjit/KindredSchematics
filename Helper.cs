using Bloodstone.API;
using KindredCommands.Data;
using Il2CppInterop.Runtime;
using Il2CppSystem;
using ProjectM;
using ProjectM.Shared;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using VampireCommandFramework;
using System.Collections.Generic;
using UnityEngine;
using ProjectM.Tiles;
using Unity.Physics;

namespace KindredVignettes;

// This is an anti-pattern, move stuff away from Helper not into it
internal static partial class Helper
{
	public static AdminAuthSystem adminAuthSystem = VWorld.Server.GetExistingSystem<AdminAuthSystem>();

	public static PrefabGUID GetPrefabGUID(Entity entity)
	{
		var entityManager = Core.EntityManager;
		PrefabGUID guid;
		try
		{
			guid = entityManager.GetComponentData<PrefabGUID>(entity);
		}
		catch
		{
			guid.GuidHash = 0;
		}
		return guid;
	}


	public static Entity AddItemToInventory(Entity recipient, PrefabGUID guid, int amount)
	{
		try
		{
			var gameData = Core.Server.GetExistingSystem<GameDataSystem>();
			var itemSettings = AddItemSettings.Create(Core.EntityManager, gameData.ItemHashLookupMap);
			var inventoryResponse = InventoryUtilitiesServer.TryAddItem(itemSettings, recipient, guid, amount);

			return inventoryResponse.NewEntity;
		}
		catch (System.Exception e)
		{
			Core.LogException(e);
		}
		return new Entity();
	}

	public static NativeArray<Entity> GetEntitiesByComponentType<T1>(bool includeAll = false, bool includeDisabled = false, bool includeSpawn = false, bool includePrefab = false, bool includeDestroyed = false)
	{
		EntityQueryOptions options = EntityQueryOptions.Default;
		if (includeAll) options |= EntityQueryOptions.IncludeAll;
		if (includeDisabled) options |= EntityQueryOptions.IncludeDisabled;
		if (includeSpawn) options |= EntityQueryOptions.IncludeSpawnTag;
		if (includePrefab) options |= EntityQueryOptions.IncludePrefab;
		if (includeDestroyed) options |= EntityQueryOptions.IncludeDestroyTag;

		EntityQueryDesc queryDesc = new()
		{
			All = new ComponentType[] { new(Il2CppType.Of<T1>(), ComponentType.AccessMode.ReadWrite) },
			Options = options
		};

		var query = Core.EntityManager.CreateEntityQuery(queryDesc);

		var entities = query.ToEntityArray(Allocator.Temp);
		return entities;
	}

	public static NativeArray<Entity> GetEntitiesByComponentTypes<T1, T2>(bool includeAll = false, bool includeDisabled = false, bool includeSpawn = false, bool includePrefab = false, bool includeDestroyed = false)
	{
		EntityQueryOptions options = EntityQueryOptions.Default;
		if (includeAll) options |= EntityQueryOptions.IncludeAll;
		if (includeDisabled) options |= EntityQueryOptions.IncludeDisabled;
		if (includeSpawn) options |= EntityQueryOptions.IncludeSpawnTag;
		if (includePrefab) options |= EntityQueryOptions.IncludePrefab;
		if (includeDestroyed) options |= EntityQueryOptions.IncludeDestroyTag;

		EntityQueryDesc queryDesc = new()
		{
			All = new ComponentType[] { new(Il2CppType.Of<T1>(), ComponentType.AccessMode.ReadWrite), new(Il2CppType.Of<T2>(), ComponentType.AccessMode.ReadWrite) },
			Options = options
		};

		var query = Core.EntityManager.CreateEntityQuery(queryDesc);

		var entities = query.ToEntityArray(Allocator.Temp);
		return entities;
	}

	public static void RepairGear(Entity Character, bool repair = true)
	{
		Equipment equipment = Character.Read<Equipment>();
        NativeList<Entity> equippedItems = new(Allocator.Temp);
		equipment.GetAllEquipmentEntities(equippedItems);
		foreach (var equippedItem in equippedItems)
		{
			if (equippedItem.Has<Durability>())
			{
				var durability = equippedItem.Read<Durability>();
				if (repair)
				{
					durability.Value = durability.MaxDurability;
				}
				else
				{
					durability.Value = 0;
				}

				equippedItem.Write(durability);
			}
		}
		equippedItems.Dispose();

		for (int i = 0; i < 36; i++)
		{
			if (InventoryUtilities.TryGetItemAtSlot(Core.EntityManager, Character, i, out InventoryBuffer item))
			{
				var itemEntity = item.ItemEntity._Entity;
				if (itemEntity.Has<Durability>())
				{
					var durability = itemEntity.Read<Durability>();
					if (repair)
					{
						durability.Value = durability.MaxDurability;
					}
					else
					{
						durability.Value = 0;
					}

					itemEntity.Write(durability);
				}
			}
		}
	}

	public static void ReviveCharacter(Entity Character, Entity User, ChatCommandContext ctx = null)
	{
		var health = Character.Read<Health>();
		ctx?.Reply("TryGetbuff");
		if (BuffUtility.TryGetBuff(Core.EntityManager, Character, Prefabs.Buff_General_Vampire_Wounded_Buff, out var buffData))
		{
			ctx?.Reply("Destroy");
			DestroyUtility.Destroy(Core.EntityManager, buffData, DestroyDebugReason.TryRemoveBuff);

			ctx?.Reply("Health");
			health.Value = health.MaxHealth;
			health.MaxRecoveryHealth = health.MaxHealth;
			Character.Write(health);
		}
		if (health.IsDead)
		{
			ctx?.Reply("Respawn");
			var pos = Character.Read<LocalToWorld>().Position;

			Nullable_Unboxed<float3> spawnLoc = new()
			{
				value = pos,
				has_value = true
			};

			ctx?.Reply("Respawn2");
			var sbs = VWorld.Server.GetExistingSystem<ServerBootstrapSystem>();
			var bufferSystem = VWorld.Server.GetExistingSystem<EntityCommandBufferSystem>();
			var buffer = bufferSystem.CreateCommandBuffer();
			ctx?.Reply("Respawn3");
			sbs.RespawnCharacter(buffer, User,
				customSpawnLocation: spawnLoc,
				previousCharacter: Character);
		}
    }

    public static void ClearExtraBuffs(Entity player)
    {
        var buffs = Core.EntityManager.GetBuffer<BuffBuffer>(player);
        var stringsToIgnore = new List<string>
        {
            "BloodBuff",
            "SetBonus",
            "EquipBuff",
            "Combat",
            "VBlood_Ability_Replace",
            "Shapeshift",
            "Interact",
            "AB_Consumable",
        };

        foreach (var buff in buffs)
        {
            bool shouldRemove = true;
            foreach (string word in stringsToIgnore)
            {
                if (buff.PrefabGuid.LookupName().Contains(word))
                {
                    shouldRemove = false;
                    break;
                }
            }
            if (shouldRemove)
            {
                DestroyUtility.Destroy(Core.EntityManager, buff.Entity, DestroyDebugReason.TryRemoveBuff);
            }
        }
        var equipment = player.Read<Equipment>();
        if (!equipment.IsEquipped(Prefabs.Item_Cloak_Main_ShroudOfTheForest, out var _) && BuffUtility.HasBuff(Core.EntityManager, player, Prefabs.EquipBuff_ShroudOfTheForest))
        {
			Buffs.RemoveBuff(player, Prefabs.EquipBuff_ShroudOfTheForest);
        }
    }

	public static IEnumerable<Entity> GetAllEntitiesInRadius<T>(float2 center, float radius)
	{
        var entities = GetEntitiesByComponentType<T>(includeSpawn: true, includeDisabled: true);
        foreach (var entity in entities)
		{
            if (!entity.Has<Translation>()) continue;
            var pos = entity.Read<Translation>().Value;
            if (Vector2.Distance(center, pos.xz) <= radius)
			{
                yield return entity;
            }
        }
    }

	public static IEnumerable<Entity> GetAllEntitiesInBox<T>(float2 center, float2 halfSize)
	{
        var entities = GetEntitiesByComponentType<T>(includeSpawn: true, includeDisabled: true);
        foreach (var entity in entities)
		{
            if (!entity.Has<Translation>()) continue;
            var pos = entity.Read<Translation>().Value;
            if (Mathf.Abs(center.x - pos.x) <= halfSize.x && Mathf.Abs(center.y - pos.z) <= halfSize.y)
			{
                yield return entity;
            }
        }
    }

	public static float3 ConvertPosToGrid(float3 pos)
	{
		return new float3(Mathf.FloorToInt(pos.x * 2) + 6400, pos.y, Mathf.FloorToInt(pos.z * 2) + 6400);
	}

    public static bool GetAabb(Entity entity, out Aabb aabb)
    {
        aabb = new Aabb();
        if (entity.Has<TileBounds>())
        {
            var bounds = entity.Read<TileBounds>();

			if (bounds.Value.Max.x == 0 && bounds.Value.Max.y == 0 && bounds.Value.Min.x == 0 && bounds.Value.Min.y == 0)
				return false;

            var minHeight = 0f;
            var maxHeight = 0f;

            if (entity.Has<TileData>())
            {
                var tileData = entity.Read<TileData>();
                if (tileData.Data.IsCreated)
                {
                    unsafe
                    {
                        TileBlob tileBlob = *(TileBlob*)tileData.Data.GetUnsafePtr();
                        minHeight = tileBlob.MinHeight;
                        maxHeight = tileBlob.MaxHeight;
                    }
                }
            }

            // Handling at least a minumum height
            if (maxHeight <= 0.1)
            {
                maxHeight = 0.1f;
            }

            var translation = entity.Read<Translation>().Value;

            aabb.Min = new float3(bounds.Value.Min.x, minHeight + translation.y, bounds.Value.Min.y);
            aabb.Max = new float3(bounds.Value.Max.x, maxHeight + translation.y, bounds.Value.Max.y);
            return true;
        }
        return false;
    }

	public static bool IsEntityInAabb(Entity entity, Aabb aabb)
	{
		if (!entity.Has<Translation>()) return false;
		var pos = entity.Read<Translation>().Value;
		return aabb.Contains(ConvertPosToGrid(pos)) || GetAabb(entity, out var otherAabb) && aabb.Overlaps(otherAabb);
    }

    public static IEnumerable<Entity> GetAllEntitiesInTileAabb<T>(Aabb aabb)
    {
        var entities = GetEntitiesByComponentType<T>(includeSpawn: true, includeDisabled: true);
        foreach (var entity in entities)
        {
            if (IsEntityInAabb(entity, aabb))
            {
                yield return entity;
            }
        }
    }

    public static void DestroyEntitiesForBuilding(IEnumerable<Entity> entities)
	{
		foreach (var entity in entities)
		{
            var prefabName = GetPrefabGUID(entity).LookupName();
            if (!prefabName.StartsWith("TM_") && !prefabName.StartsWith("Chain_")) continue;

            if (entity.Has<SpawnChainChild>())
                entity.Remove<SpawnChainChild>();

            if (entity.Has<DropTableBuffer>())
                entity.Remove<DropTableBuffer>();

            DestroyUtility.Destroy(Core.EntityManager, entity);
        }
	}

	public static Entity FindClosest<T>(Vector3 pos, string startsWith = null)
	{
        var closestEntity = Entity.Null;
        var closestDistance = float.MaxValue;
        var entities = GetEntitiesByComponentType<T>(includeSpawn: true, includeDisabled: true);
        foreach (var entity in entities)
		{
            if (!entity.Has<Translation>()) continue;
			if (startsWith != null)
			{
				var prefabName = GetPrefabGUID(entity).LookupName();
				if (!prefabName.StartsWith(startsWith)) continue;
			}

            var entityPos = entity.Read<Translation>().Value;
            var distance = Vector3.Distance(pos, entityPos);
            if (distance < closestDistance)
			{
                closestDistance = distance;
                closestEntity = entity;
            }
        }
        return closestEntity;
    }
}
