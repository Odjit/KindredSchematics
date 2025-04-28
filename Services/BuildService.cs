using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Network;
using ProjectM.Shared;
using ProjectM.Tiles;
using Stunlock.Core;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace KindredSchematics.Services;
class BuildService
{
    static readonly PrefabGUID LeftClick = Data.Prefabs.AB_Emote_Vampire_Beckon_AbilityGroup;
    static readonly PrefabGUID SpaceBar = Data.Prefabs.AB_Emote_Vampire_Clap_AbilityGroup;
    static readonly PrefabGUID KeyQ = Data.Prefabs.AB_Emote_Vampire_Bow_AbilityGroup;
    static readonly PrefabGUID KeyE = Data.Prefabs.AB_Emote_Vampire_Yes_AbilityGroup;
    static readonly PrefabGUID KeyR = Data.Prefabs.AB_Emote_Vampire_Shrug_AbilityGroup;
    static readonly PrefabGUID KeyC = Data.Prefabs.AB_Emote_Vampire_Salute_AbilityGroup;
    static readonly PrefabGUID KeyT = Data.Prefabs.AB_Emote_Vampire_No_AbilityGroup;

    public static readonly PrefabGUID BuildBuff = new PrefabGUID(-480024072);

    readonly PrefabGUID SelectionGlowPrefab = new(1939626064);
    readonly PrefabGUID SelectionGlowPrefab2 = new(-443754441);
    readonly PrefabGUID StaticSelectionGlowPrefab = new(-1602570831);

    struct GrabData
    {
        public Entity TargetEntity;
        public Coroutine MoveCoroutine;
        public bool GrabbedObject;
        public Translation PrevTranslation;
        public Rotation PrevRotation;
        public StaticTransformCompatible PrevSTC;
    }

    Dictionary<Entity, GrabData> playerGrabData = [];

    class SelectionData
    {
        public Entity TargetEntity;
        public Coroutine SelectionCoroutine;
    }

    Dictionary<Entity, SelectionData> playerSelectionData = [];

    Dictionary<Entity, Entity> lastPrefabEntity = [];

    Dictionary<Entity, List<Entity>> playerPalette = [];

    Dictionary<Entity, float> lastAction = [];
    const float ACTION_COOLDOWN = 0.2f;

    static ModifyUnitStatBuff_DOTS Cooldown = new()
    {
        StatType = UnitStatType.CooldownRecoveryRate,
        Value = 100,
        ModificationType = ModificationType.Set,
        Modifier = 1,
        Id = ModificationId.NewId(0)
    };

    public static bool IsCharacterInBuildMode(Entity charEntity)
    {
        return charEntity.Has<AbilityGroupSlotModificationBuffer>();
    }

    public void RemoveBuildModeIfActive(Entity charEntity)
    {
        if (IsCharacterInBuildMode(charEntity))
        {
            RemoveBuildMode(charEntity);
        }
    }

    public bool SwitchToBuildMode(Entity charEntity)
    {
        if (!IsCharacterInBuildMode(charEntity))
        {
            AddAbility(charEntity, 0, LeftClick);
            AddAbility(charEntity, 1, KeyQ);
            AddAbility(charEntity, 2, SpaceBar);
            AddAbility(charEntity, 4, KeyE);
            AddAbility(charEntity, 5, KeyR);
            AddAbility(charEntity, 6, KeyC);
            AddAbility(charEntity, 7, KeyT);

            StartPlayerSelection(charEntity);

            Buffs.RemoveAndAddBuff(charEntity.Read<PlayerCharacter>().UserEntity, charEntity, BuildBuff, -1, UpdateBuildBuff);

            return true;
        }
        else
        {
            RemoveBuildMode(charEntity);

            return false;
        }
    }

    void RemoveBuildMode(Entity charEntity)
    {
        var buffer = Core.EntityManager.GetBuffer<AbilityGroupSlotModificationBuffer>(charEntity);
        foreach (var mod in buffer)
        {
            Core.ServerGameManager.RemoveAbilityGroupModificationOnSlot(charEntity, mod.Slot, mod.ModificationId);
        }
        Core.EntityManager.RemoveComponent<AbilityGroupSlotModificationBuffer>(charEntity);

        ClearCursor(charEntity, false);
        ClearSelection(charEntity);

        Buffs.RemoveBuff(charEntity, BuildBuff);
    }

    static void UpdateBuildBuff(Entity buffEntity)
    {
        var prefabGuid = buffEntity.Read<PrefabGUID>();
        if (prefabGuid != BuildBuff) return;

        if (!buffEntity.Has<ModifyUnitStatBuff_DOTS>())
        {
            var modifyStatBuffer = Core.EntityManager.AddBuffer<ModifyUnitStatBuff_DOTS>(buffEntity);
            modifyStatBuffer.Clear();
            modifyStatBuffer.Add(Cooldown);
        }

        buffEntity.Add<DisableAggroBuff>();
        buffEntity.Write(new DisableAggroBuff
        {
            Mode = DisableAggroBuffMode.OthersDontAttackTarget
        });
    }

    static void AddAbility(Entity charEntity, int slot, PrefabGUID abilityPrefab)
    {
        var modificationId = Core.ServerGameManager.ModifyAbilityGroupOnSlot(charEntity, charEntity, slot, abilityPrefab, 1000);
        var mod = new AbilityGroupSlotModificationBuffer()
        {
            Owner = charEntity,
            Target = charEntity,
            ModificationId = modificationId,
            NewAbilityGroup = abilityPrefab,
            Slot = slot,
            Priority = 1000,
        };

        if (!charEntity.Has<AbilityGroupSlotModificationBuffer>())
            Core.EntityManager.AddComponent<AbilityGroupSlotModificationBuffer>(charEntity);
        var buffer = Core.EntityManager.GetBuffer<AbilityGroupSlotModificationBuffer>(charEntity);
        buffer.Add(mod);
    }

    public bool CheckAbilityUsage(Entity entity, PrefabGUID ability)
    {
        if (!entity.Has<PlayerCharacter>()) return false;
        if (!IsCharacterInBuildMode(entity)) return false;

        if (lastAction.TryGetValue(entity, out var lastTime) && Time.time - lastTime < ACTION_COOLDOWN) return true;
        lastAction[entity] = Time.time;

        if (ability == LeftClick)
        {
            PlaceOrGrabObject(entity);
        }
        else if (ability == KeyQ)
        {
            RotateObjectCCW(entity);
        }
        else if (ability == KeyE)
        {
            RotateObjectCW(entity);
        }
        else if (ability == SpaceBar)
        {
            CopyLastOrClearCursor(entity);
        }
        else if (ability == KeyR)
        {
            CopySelectionOrRevertHolding(entity);
        }
        else if (ability == KeyC)
        {
            AdvanceThroughPalette(entity);
        }
        else if (ability == KeyT)
        {
            RetreatThroughPalette(entity);
        }

        return true;
    }

    public void SetCursor(Entity charEntity, Entity prefab)
    {
        if (!IsCharacterInBuildMode(charEntity)) return;

        if (!prefab.Has<Prefab>())
        {
            var prefabGuid = prefab.Read<PrefabGUID>();
            Core.PrefabCollection._PrefabGuidToEntityMap.TryGetValue(prefabGuid, out prefab);
        }

        if (playerGrabData.TryGetValue(charEntity, out var grabData))
        {
            playerGrabData.Remove(charEntity);

            Core.StopCoroutine(grabData.MoveCoroutine);
            Core.EntityManager.DestroyEntity(grabData.TargetEntity);
        }

        var targetEntity = Core.EntityManager.Instantiate(prefab);
        targetEntity.Add<PhysicsCustomTags>();
        playerGrabData.Add(charEntity, new GrabData
        {
            TargetEntity = targetEntity,
            MoveCoroutine = Core.StartCoroutine(MoveCoroutine(charEntity, targetEntity)),
        });
        SetOwnerForEntity(charEntity, targetEntity);

        lastPrefabEntity[charEntity] = prefab;

        ClearSelection(charEntity);
    }

    void PlaceOrGrabObject(Entity charEntity)
    {
        if (playerGrabData.TryGetValue(charEntity, out var grabData))
        {
            playerGrabData.Remove(charEntity);

            Core.StopCoroutine(grabData.MoveCoroutine);

            if (grabData.TargetEntity.Has<StaticTransformCompatible>())
            {
                var actualPlacement = Core.EntityManager.Instantiate(lastPrefabEntity[charEntity]);
                actualPlacement.Add<PhysicsCustomTags>();

                var translation = grabData.TargetEntity.Read<Translation>();
                var rotation = grabData.TargetEntity.Read<Rotation>();
                var stc = actualPlacement.Read<StaticTransformCompatible>();

                actualPlacement.Write(translation);
                actualPlacement.Write(rotation);
                actualPlacement.Write(stc);

                DestroyUtility.Destroy(Core.EntityManager, grabData.TargetEntity);


                SetOwnerForEntity(charEntity, actualPlacement);
            }

            StartPlayerSelection(charEntity);
            return;
        }

        if (playerSelectionData.TryGetValue(charEntity, out var selectData) && selectData.TargetEntity != Entity.Null &&
            !selectData.TargetEntity.Has<TransitionWhenInventoryIsEmpty>())
        {
            ClearSelection(charEntity);

            if (selectData.TargetEntity.Has<StaticTransformCompatible>() &&
                selectData.TargetEntity.Read<StaticTransformCompatible>().UseStaticTransform)
            {
                SetCursor(charEntity, selectData.TargetEntity);
            }
            else
            {
                var stc = selectData.TargetEntity.Has<StaticTransformCompatible>() ? 
                    selectData.TargetEntity.Read<StaticTransformCompatible>() : new StaticTransformCompatible();
                playerGrabData.Add(charEntity, new GrabData
                {
                    TargetEntity = selectData.TargetEntity,
                    MoveCoroutine = Core.StartCoroutine(MoveCoroutine(charEntity, selectData.TargetEntity)),
                    GrabbedObject = true,
                    PrevTranslation = selectData.TargetEntity.Read<Translation>(),
                    PrevRotation = selectData.TargetEntity.Read<Rotation>(),
                    PrevSTC = stc,
                });

                var prefabGuid = selectData.TargetEntity.Read<PrefabGUID>();
                Core.PrefabCollection._PrefabGuidToEntityMap.TryGetValue(prefabGuid, out var prefab);
                lastPrefabEntity[charEntity] = prefab;
            }
        }
    }

    void RotateObjectCCW(Entity charEntity)
    {
        var rotating = Entity.Null;
        if (playerGrabData.TryGetValue(charEntity, out var grabData))
        {
            rotating = grabData.TargetEntity;
        }
        else if (playerSelectionData.TryGetValue(charEntity, out var selectData))
        {
            rotating = selectData.TargetEntity;
        }

        if (rotating == Entity.Null)
            return;

        var tilePos = rotating.Read<TilePosition>();
        tilePos.TileRotation = (TileRotation)((((int)tilePos.TileRotation) + 3) % 4);
        rotating.Write(tilePos);

        if (rotating.Has<StaticTransformCompatible>())
        {
            var stc = rotating.Read<StaticTransformCompatible>();
            stc.NonStaticTransform_Rotation = tilePos.TileRotation;
            rotating.Write(stc);
        }

        rotating.Write(new Rotation { Value = quaternion.RotateY(math.radians(90 * (int)tilePos.TileRotation)) });
    }

    void RotateObjectCW(Entity charEntity)
    {
        var rotating = Entity.Null;
        if (playerGrabData.TryGetValue(charEntity, out var grabData))
        {
            rotating = grabData.TargetEntity;
        }
        else if (playerSelectionData.TryGetValue(charEntity, out var selectData))
        {
            rotating = selectData.TargetEntity;
        }

        if (rotating == Entity.Null)
            return;

        var tilePos = rotating.Read<TilePosition>();
        tilePos.TileRotation = (TileRotation)((((int)tilePos.TileRotation) + 1) % 4);
        rotating.Write(tilePos);

        if (rotating.Has<StaticTransformCompatible>())
        {
            var stc = rotating.Read<StaticTransformCompatible>();
            stc.NonStaticTransform_Rotation = tilePos.TileRotation;
            rotating.Write(stc);
        }

        rotating.Write(new Rotation { Value = quaternion.RotateY(math.radians(90 * (int)tilePos.TileRotation)) });
    }

    void CopyLastOrClearCursor(Entity charEntity)
    {
        if (playerGrabData.ContainsKey(charEntity))
        {
            ClearCursor(charEntity);
            return;
        }

        if (lastPrefabEntity.TryGetValue(charEntity, out var prefab))
        {
            SetCursor(charEntity, prefab);
        }
    }

    void ClearCursor(Entity charEntity, bool startSelection=true)
    {
        if (playerGrabData.TryGetValue(charEntity, out var grabData))
        {
            playerGrabData.Remove(charEntity);

            Core.StopCoroutine(grabData.MoveCoroutine);
            DestroyUtility.Destroy(Core.EntityManager, grabData.TargetEntity);
            if (startSelection) StartPlayerSelection(charEntity);
        }
    }

    void ClearSelection(Entity charEntity)
    {
        if (playerSelectionData.TryGetValue(charEntity, out var selectionData))
        {
            playerSelectionData.Remove(charEntity);
            Core.StopCoroutine(selectionData.SelectionCoroutine);
            RemoveSelectionGlow(charEntity, selectionData.TargetEntity);
        }
    }

    void CopySelectionOrRevertHolding(Entity charEntity)
    {
        if (playerSelectionData.TryGetValue(charEntity, out var selectData) && selectData.TargetEntity != Entity.Null &&
            !selectData.TargetEntity.Has<TransitionWhenInventoryIsEmpty>())
        {
            SetCursor(charEntity, selectData.TargetEntity);

            ClearSelection(charEntity);
            return;
        }

        if (playerGrabData.TryGetValue(charEntity, out var grabData))
        {
            playerGrabData.Remove(charEntity);
            Core.StopCoroutine(grabData.MoveCoroutine);

            if (grabData.GrabbedObject)
            {
                grabData.TargetEntity.Write(grabData.PrevTranslation);
                grabData.TargetEntity.Write(grabData.PrevRotation);
                if (grabData.TargetEntity.Has<StaticTransformCompatible>())
                {
                    grabData.TargetEntity.Write(grabData.PrevSTC);
                }
            }
            else
            {
                DestroyUtility.Destroy(Core.EntityManager, grabData.TargetEntity);
            }
            StartPlayerSelection(charEntity);
        }
    }

    public void AdvanceThroughPalette(Entity charEntity)
    {
        var palette = GrabPalette(charEntity);
        if (palette.Count == 0)
        {
            var emptyMessage = new FixedString512Bytes("<color=orange>Build palette</color> is <color=red>empty</color>.");
            ServerChatUtils.SendSystemMessageToClient(Core.EntityManager, charEntity.Read<PlayerCharacter>().UserEntity.Read<User>(), ref emptyMessage);
            return;
        }

        lastPrefabEntity.TryGetValue(charEntity, out var curPrefab);
        var index = palette.IndexOf(curPrefab);

        index = (index + 1) % palette.Count;
        ClearCursor(charEntity, false);
        ClearSelection(charEntity);
        SetCursor(charEntity, palette[index]);

        var user = charEntity.Read<PlayerCharacter>().UserEntity.Read<User>();
        var message = new FixedString512Bytes($"<color=orange>Switched to </color><color=green>{palette[index].Read<PrefabGUID>().LookupName()}</color> (<color=white>{index + 1}</color>/<color=yellow>{palette.Count}</color>)");
        ServerChatUtils.SendSystemMessageToClient(Core.EntityManager, user, ref message);
    }

    public void RetreatThroughPalette(Entity charEntity)
    {
        var palette = GrabPalette(charEntity);
        if (palette.Count == 0)
        {
            var emptyMessage = new FixedString512Bytes("<color=orange>Build palette</color> is <color=red>empty</color>.");
            ServerChatUtils.SendSystemMessageToClient(Core.EntityManager, charEntity.Read<PlayerCharacter>().UserEntity.Read<User>(), ref emptyMessage);
            return;
        }

        lastPrefabEntity.TryGetValue(charEntity, out var curPrefab);
        var index = palette.IndexOf(curPrefab);

        index = (index - 1 + palette.Count) % palette.Count;

        ClearCursor(charEntity, false);
        ClearSelection(charEntity);
        SetCursor(charEntity, palette[index]);

        var user = charEntity.Read<PlayerCharacter>().UserEntity.Read<User>();
        var message = new FixedString512Bytes($"<color=orange>Switched to </color><color=green>{palette[index].Read<PrefabGUID>().LookupName()}</color> (<color=white>{index + 1}</color>/<color=yellow>{palette.Count}</color>)");
        ServerChatUtils.SendSystemMessageToClient(Core.EntityManager, user, ref message);
    }

    public void ClearPalette(Entity charEntity)
    {
        if (playerPalette.TryGetValue(charEntity, out var palette))
        {
            palette.Clear();
        }
    }

    public PrefabGUID[] GetPalette(Entity charEntity)
    {
        if (playerPalette.TryGetValue(charEntity, out var palette))
        {
            return palette.Select(x => x.Read<PrefabGUID>()).ToArray();
        }
        return new PrefabGUID[0];
    }

    List<Entity> GrabPalette(Entity charEntity)
    {
        if (!playerPalette.TryGetValue(charEntity, out var palette))
        {
            palette = [];
            playerPalette.Add(charEntity, palette);
        }
        return palette;
    }

    public void AddListToPalette(Entity charEntity, List<Entity> prefabs)
    {
        var palette = GrabPalette(charEntity);

        foreach (var prefab in prefabs)
        {
            if (palette.Contains(prefab)) continue;
            palette.Add(prefab);
        }
    }

    public void RemoveListFromPalette(Entity charEntity, List<Entity> prefabs)
    {
        GrabPalette(charEntity).RemoveAll(x => prefabs.Contains(x));
    }

    static IEnumerator MoveCoroutine(Entity charEntity, Entity targetEntity)
    {
        var sleepYield = new WaitForSeconds(0.033f);
        while (true)
        {
            MoveTargetToCharacterCursor(charEntity, targetEntity);
            yield return sleepYield;
        }
    }

    static void MoveTargetToCharacterCursor(Entity charEntity, Entity targetEntity)
    {
        var aimPos = charEntity.Read<EntityAimData>().AimPositionPlane;

        if (targetEntity.Has<StaticTransformCompatible>())
        {
            var stc = targetEntity.Read<StaticTransformCompatible>();
            stc.UseStaticTransform = false;
            stc.NonStaticTransform_Pos = new Vector2(aimPos.x, aimPos.z);
            stc.NonStaticTransform_Height = Mathf.Floor(aimPos.y);
            targetEntity.Write(stc);
        }
        targetEntity.Write(new Translation { Value = new Vector3(aimPos.x, Mathf.Floor(aimPos.y), aimPos.z) });
    }

    void StartPlayerSelection(Entity charEntity)
    {
        if (playerGrabData.TryGetValue(charEntity, out var grabData))
        {
            playerGrabData.Remove(charEntity);
            Core.StopCoroutine(grabData.MoveCoroutine);
            Core.EntityManager.DestroyEntity(grabData.TargetEntity);
        }

        var selectData = new SelectionData();

        selectData.SelectionCoroutine = Core.StartCoroutine(SelectionCoroutine(charEntity, selectData));

        playerSelectionData.Add(charEntity, selectData);
    }

    IEnumerator SelectionCoroutine(Entity charEntity, SelectionData selectData)
    {
        var sleepYield = new WaitForSeconds(0.033f);

        while (true)
        {
            var aimPos = charEntity.Read<EntityAimData>().AimPosition;
            var closest = Helper.FindClosestTilePosition(aimPos, true);

            if (selectData.TargetEntity != closest)
            {
                if (closest != Entity.Null)
                {
                    AddSelectionGlow(charEntity, closest);
                }

                if (selectData.TargetEntity != Entity.Null)
                {
                    RemoveSelectionGlow(charEntity, selectData.TargetEntity);
                }

                selectData.TargetEntity = closest;
            }

            yield return sleepYield;
        }
    }

    void AddSelectionGlow(Entity charEntity, Entity targetEntity)
    {
        var userEntity = charEntity.Read<PlayerCharacter>().UserEntity;
        var isStatic = targetEntity.Has<StaticTransformCompatible>() && targetEntity.Read<StaticTransformCompatible>().UseStaticTransform;
        if (isStatic)
        {
            Core.GlowService.AddGlow(userEntity, targetEntity, StaticSelectionGlowPrefab);
            return;
        }
        Core.GlowService.AddGlow(userEntity, targetEntity, SelectionGlowPrefab);
        Core.GlowService.AddGlow(userEntity, targetEntity, SelectionGlowPrefab2);
    }

    void RemoveSelectionGlow(Entity charEntity, Entity targetEntity)
    {
        var userEntity = charEntity.Read<PlayerCharacter>().UserEntity;
        var isStatic = targetEntity.Has<StaticTransformCompatible>() && targetEntity.Read<StaticTransformCompatible>().UseStaticTransform;
        if (isStatic)
        {
            Core.GlowService.RemoveGlow(targetEntity, StaticSelectionGlowPrefab);
            return;
        }
        Core.GlowService.RemoveGlow(targetEntity, SelectionGlowPrefab);
        Core.GlowService.RemoveGlow(targetEntity, SelectionGlowPrefab2);
    }

    void SetOwnerForEntity(Entity charEntity, Entity entity)
    {
        Core.SchematicService.GetFallbackCastleHeart(charEntity, out var castleHeartEntity, out var ownerDoors, out var ownerChests);

        if (!ownerDoors && entity.Has<Door>() ||
            !ownerChests && Helper.EntityIsChest(entity))
        {

            if (entity.Has<CastleHeartConnection>())
            {
                entity.Write(new CastleHeartConnection { CastleHeartEntity = Entity.Null });
            }

            var teamRef = Core.SchematicService.NeutralTeam;
            if (entity.Has<Team>())
            {
                var teamData = teamRef.Read<TeamData>();
                entity.Write(new Team() { Value = teamData.TeamValue, FactionIndex = -1 });

                entity.Add<UserOwner>();
                entity.Write(castleHeartEntity.Read<UserOwner>());
            }

            if (entity.Has<TeamReference>() && !teamRef.Equals(Entity.Null))
            {
                var t = new TeamReference();
                t.Value._Value = teamRef;
                entity.Write(t);
            }
        }
        else if (castleHeartEntity != Entity.Null)
        {
            if (entity.Has<CastleHeartConnection>())
            {
                entity.Write(new CastleHeartConnection { CastleHeartEntity = castleHeartEntity });
            }

            var teamRef = (Entity)castleHeartEntity.Read<TeamReference>().Value;
            if (entity.Has<Team>())
            {
                var teamData = teamRef.Read<TeamData>();
                entity.Write(new Team() { Value = teamData.TeamValue, FactionIndex = -1 });

                entity.Add<UserOwner>();
                entity.Write(castleHeartEntity.Read<UserOwner>());
            }

            if (entity.Has<TeamReference>() && !teamRef.Equals(Entity.Null))
            {
                var t = new TeamReference();
                t.Value._Value = teamRef;
                entity.Write(t);
            }
        }
    }
}
