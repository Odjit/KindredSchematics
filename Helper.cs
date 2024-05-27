using Il2CppInterop.Runtime;
using KindredCommands.Data;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Shared;
using Stunlock.Core;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace KindredVignettes;

// This is an anti-pattern, move stuff away from Helper not into it
internal static partial class Helper
{
	public static AdminAuthSystem adminAuthSystem = Core.Server.GetExistingSystemManaged<AdminAuthSystem>();

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
            guid = new PrefabGUID(0);
		}
		return guid;
	}


	public static Entity AddItemToInventory(Entity recipient, PrefabGUID guid, int amount)
	{
		try
		{
			var gameData = Core.Server.GetExistingSystemManaged<GameDataSystem>();
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

    public static int GetEntityTerritoryIndex(Entity entity)
    {
        if (entity.Has<TilePosition>())
        {
            var pos = entity.Read<TilePosition>().Tile;
            var territoryIndex = Core.CastleTerritory.GetTerritoryIndexFromTileCoord(pos);
            if (territoryIndex != -1)
            {
                return territoryIndex;
            }
        }

        if (entity.Has<TileBounds>())
        {
            var bounds = entity.Read<TileBounds>().Value;
            for(var x=bounds.Min.x; x<=bounds.Max.x; x++)
            {
                for(var y=bounds.Min.y; y<=bounds.Max.y; y++)
                {
                    var territoryIndex = Core.CastleTerritory.GetTerritoryIndexFromTileCoord(new int2(x, y));
                    if (territoryIndex != -1)
                    {
                        return territoryIndex;
                    }
                }
            }
        }

        if (entity.Has<Translation>())
        {
            var pos = entity.Read<Translation>().Value;
            return Core.CastleTerritory.GetTerritoryIndex(pos);
        }

        if (entity.Has<LocalToWorld>()) 
        {
            var pos = entity.Read<LocalToWorld>().Position;
            return Core.CastleTerritory.GetTerritoryIndex(pos);
        }

        return -1;
    }   


    public static IEnumerable<Entity> GetAllEntitiesInTerritory<T>(int territoryIndex)
    {
        var entities = GetEntitiesByComponentType<T>(includeSpawn: true, includeDisabled: true);
        foreach (var entity in entities)
        {
            if (GetEntityTerritoryIndex(entity) == territoryIndex)
            {
                yield return entity;
            }
        }
        entities.Dispose();
    }

    public static IEnumerable<Entity> GetAllEntitiesInRadius<T>(float2 center, float radius)
	{
        var entities = GetEntitiesByComponentType<T>(includeSpawn: true, includeDisabled: true);
        foreach (var entity in entities)
		{
            if (!entity.Has<Translation>()) continue;
            var pos = entity.Read<Translation>().Value;
            if (math.distance(center, pos.xz) <= radius)
			{
                yield return entity;
            }
        }
        entities.Dispose();
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
        entities.Dispose();
    }

	public static float3 ConvertPosToTileGrid(float3 pos)
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
		return aabb.Contains(ConvertPosToTileGrid(pos)) || GetAabb(entity, out var otherAabb) && aabb.Overlaps(otherAabb);
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
        entities.Dispose();
    }

    public static void DestroyEntitiesForBuilding(IEnumerable<Entity> entities)
	{
		foreach (var entity in entities)
		{
            if (entity.Has<CastleHeart>()) continue;

            var prefabName = GetPrefabGUID(entity).LookupName();
            if (!prefabName.StartsWith("TM_") && !prefabName.StartsWith("Chain_") && !entity.Has<CastleBuildingFusedRoot>())
            {
                continue;
            }

            DestroyEntityAndCastleAttachments(entity);
        }
	}

    public static void DestroyEntityAndCastleAttachments(Entity entity, HashSet<Entity> alreadyVisited=null)
    {
        if (alreadyVisited == null)
        {
            alreadyVisited = new HashSet<Entity>();
        }
        if (alreadyVisited.Contains(entity)) return;
        alreadyVisited.Add(entity);

        if(Core.EntityManager.HasBuffer<CastleBuildingAttachToParentsBuffer>(entity))
        {
            var castleAttachments = Core.EntityManager.GetBufferReadOnly<CastleBuildingAttachToParentsBuffer>(entity);
            foreach (var attachment in castleAttachments)
            {
                DestroyEntityAndCastleAttachments(attachment.ParentEntity.GetEntityOnServer(), alreadyVisited);
            }
        }

        // Windows are attached opposite as you would expect, so we need to handle them specially
        if(entity.Has<PrefabGUID>() && entity.Read<PrefabGUID>() == Prefabs.TM_Castle_Wall_Tier02_Stone_Window)
        {
            if(Core.EntityManager.HasBuffer<CastleBuildingAttachedChildrenBuffer>(entity))
            {
                var castleAttachments = Core.EntityManager.GetBufferReadOnly<CastleBuildingAttachedChildrenBuffer>(entity);
                foreach (var attachment in castleAttachments)
                {
                    var attachmentEntity = attachment.ChildEntity.GetEntityOnServer();
                    if(attachmentEntity.Has<EntityCategory>())
                    {
                        // See if this matches with a window
                        var ec = attachmentEntity.Read<EntityCategory>();
                        if (ec.MainCategory == MainEntityCategory.Structure &&
                            ec.UnitCategory == UnitCategory.Human &&
                            ec.StructureCategory == StructureCategory.BasicStructure &&
                            ec.MaterialCategory == MaterialCategory.None &&
                            ec.ResourceLevel == 0)
                        {
                            DestroyEntityAndCastleAttachments(attachmentEntity, alreadyVisited);
                        }
                    }
                }
            }
        }

        DestroyUtility.Destroy(Core.EntityManager, entity);
    }

	public static Entity FindClosest<T>(Vector3 pos, string startsWith = null, float maxDistance = -1)
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
            if (distance < closestDistance && (maxDistance < 0 || distance < maxDistance))
			{
                closestDistance = distance;
                closestEntity = entity;
            }
        }
        entities.Dispose();
        return closestEntity;
    }
}
