using ProjectM;
using ProjectM.Shared;
using Stunlock.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Unity.Entities;

namespace KindredSchematics.Services;
class GlowService
{
    private static readonly string CONFIG_PATH = Path.Combine(BepInEx.Paths.ConfigPath, MyPluginInfo.PLUGIN_NAME);
    private static readonly string GLOW_CHOICES_PATH = Path.Combine(CONFIG_PATH, "glowChoices.txt");

    public readonly Dictionary<string, PrefabGUID> glowChoices = new Dictionary<string, PrefabGUID>(StringComparer.InvariantCultureIgnoreCase)
    {
        { "InkShadow", new PrefabGUID(-1124645803) },
        { "Cursed", new PrefabGUID(1425734039) },
        { "Howl", new PrefabGUID(-91451769)},
        { "Chaos", new PrefabGUID(1163490655)},
        { "Emerald", new PrefabGUID(-1559874083)},
        { "Poison", new PrefabGUID(-1965215729)},
        { "Agony", new PrefabGUID(1025643444)},
        { "Light", new PrefabGUID(178225731)},
    };

    readonly Dictionary<PrefabGUID, string> prefabToGlowName = [];

    public GlowService()
    {
        LoadGlowChoices();
    }

    void SaveGlowChoices()
    {
        if (!Directory.Exists(CONFIG_PATH))
            Directory.CreateDirectory(CONFIG_PATH);
        var sb = new StringBuilder();
        foreach (var entry in glowChoices)
        {
            sb.AppendLine($"{entry.Key}={entry.Value.GuidHash}");
        }
        File.WriteAllText(GLOW_CHOICES_PATH, sb.ToString());
    }

    void LoadGlowChoices()
    {
        if (File.Exists(GLOW_CHOICES_PATH))
        {
            glowChoices.Clear();
            var lines = File.ReadAllLines(GLOW_CHOICES_PATH);
            foreach (var line in lines)
            {
                var parts = line.Split('=');
                if (parts.Length == 2 && int.TryParse(parts[1], out var guid))
                {
                    var prefabGuid = new PrefabGUID(guid);
                    if (Core.PrefabCollection._PrefabGuidToEntityMap.TryGetValue(prefabGuid, out var entity))
                        if (entity.Has<Buff>())
                            glowChoices[parts[0]] = new PrefabGUID(guid);
                        else
                            Core.Log.LogWarning($"Glow choice {parts[0]} of type {prefabGuid.LookupName()} missing buff");
                    else
                        Core.Log.LogWarning($"Glow choice {parts[0]} with guid {guid} not found");
                }
            }
        }

        prefabToGlowName.Clear();
        foreach (var entry in glowChoices)
        {
            prefabToGlowName[entry.Value] = entry.Key;
        }
    }

    public void AddNewGlowChoice(PrefabGUID prefab, string name)
    {
        glowChoices[name] = prefab;
        prefabToGlowName[prefab] = name;
        SaveGlowChoices();
    }

    public bool RemoveGlowChoice(string name)
    {
        if(glowChoices.Remove(name))
        {
            SaveGlowChoices();
            return true;
        }
        return false;
    }

    public PrefabGUID GetGlowEntry(string name)
    {
        if (glowChoices.TryGetValue(name, out var guid))
        {
            return guid;
        }
        return default;
    }

    public IEnumerable<(string, PrefabGUID)> ListGlowChoices()
    {
        foreach (var entry in glowChoices)
        {
            yield return (entry.Key, entry.Value);
        }
    }

    public string GetGlowName(PrefabGUID guid)
    {
        return glowChoices.FirstOrDefault(x => x.Value == guid).Key;
    }

    public void AddGlow(Entity userEntity, Entity targetEntity, PrefabGUID glowPrefab)
    {
        Buffs.RemoveAndAddBuff(userEntity, targetEntity, glowPrefab, -1, glowBuffEntity =>
        {
            if (glowBuffEntity.Has<EntityOwner>())
            {
                glowBuffEntity.Write(new EntityOwner { Owner = targetEntity });
            }

            if (glowBuffEntity.Has<CreateGameplayEventsOnSpawn>())
            {
                glowBuffEntity.Remove<CreateGameplayEventsOnSpawn>();
            }
            if (glowBuffEntity.Has<GameplayEventListeners>())
            {
                glowBuffEntity.Remove<GameplayEventListeners>();
            }
            if (glowBuffEntity.Has<RemoveBuffOnGameplayEvent>())
            {
                glowBuffEntity.Remove<RemoveBuffOnGameplayEvent>();
            }
            if (glowBuffEntity.Has<RemoveBuffOnGameplayEventEntry>())
            {
                glowBuffEntity.Remove<RemoveBuffOnGameplayEventEntry>();
            }
            if (glowBuffEntity.Has<DealDamageOnGameplayEvent>())
            {
                glowBuffEntity.Remove<DealDamageOnGameplayEvent>();
            }
            if (glowBuffEntity.Has<ModifyMovementSpeedBuff>())
            {
                glowBuffEntity.Remove<ModifyMovementSpeedBuff>();
            }
            if (glowBuffEntity.Has<HealOnGameplayEvent>())
            {
                glowBuffEntity.Remove<HealOnGameplayEvent>();
            }
            if (glowBuffEntity.Has<DestroyOnGameplayEvent>())
            {
                glowBuffEntity.Remove<DestroyOnGameplayEvent>();
            }
            if (glowBuffEntity.Has<WeakenBuff>())
            {
                glowBuffEntity.Remove<WeakenBuff>();
            }
            if (glowBuffEntity.Has<ReplaceAbilityOnSlotBuff>())
            {
                glowBuffEntity.Remove<ReplaceAbilityOnSlotBuff>();
            }
            if (glowBuffEntity.Has<AmplifyBuff>())
            {
                glowBuffEntity.Remove<AmplifyBuff>();
            }
            if (glowBuffEntity.Has<EmpowerBuff>())
            {
                glowBuffEntity.Remove<EmpowerBuff>();
            }
            if (glowBuffEntity.Has<BuffResistances>())
            {
                glowBuffEntity.Remove<BuffResistances>();
            }
            if (glowBuffEntity.Has<BuffResistanceElement>())
            {
                glowBuffEntity.Remove<BuffResistanceElement>();
            }
            if (glowBuffEntity.Has<AbsorbBuff>())
            {
                glowBuffEntity.Remove<AbsorbBuff>();
            }
            if (glowBuffEntity.Has<FortifyBuff>())
            {
                glowBuffEntity.Remove<FortifyBuff>();
            }
            if (glowBuffEntity.Has<ModifyUnitStatBuff_DOTS>())
            {
                var modifyStatBuffer = Core.EntityManager.GetBuffer<ModifyUnitStatBuff_DOTS>(glowBuffEntity);
                modifyStatBuffer.Clear();
            }
            if (glowBuffEntity.Has<BuffModificationFlagData>())
            {
                glowBuffEntity.Remove<BuffModificationFlagData>();
            }
            if (glowBuffEntity.Has<SpellModArithmetic>())
            {
                glowBuffEntity.Remove<SpellModArithmetic>();
            }
            if (glowBuffEntity.Has<MoveTowardsPositionBuff>())
            {
                glowBuffEntity.Remove<MoveTowardsPositionBuff>();
            }
            if (glowBuffEntity.Has<Dash>())
            {
                glowBuffEntity.Remove<Dash>();
            }
            if (glowBuffEntity.Has<DashSpawn>())
            {
                glowBuffEntity.Remove<DashSpawn>();
            }
            if (glowBuffEntity.Has<TravelToTarget>())
            {
                glowBuffEntity.Remove<TravelToTarget>();
            }
            if (glowBuffEntity.Has<TravelBuff>())
            {
                glowBuffEntity.Remove<TravelBuff>();
            }
            if (glowBuffEntity.Has<TravelBuffSpawn>())
            {
                glowBuffEntity.Remove<TravelBuffSpawn>();
            }
            if (glowBuffEntity.Has<CreateGameplayEventsOnTick>())
            {
                glowBuffEntity.Remove<CreateGameplayEventsOnTick>();
            }

            // Our way to identify its ours for saving
            glowBuffEntity.Add<HideOutsideVision>();
        });
    }

    public bool RemoveGlow(Entity targetEntity, PrefabGUID glowPrefab)
    {
        if (BuffUtility.HasBuff(Core.EntityManager, targetEntity, glowPrefab))
        {
            Buffs.RemoveBuff(targetEntity, glowPrefab);
            return true;
        }
        return false;
    }

    public IEnumerable<string> ListGlows(Entity targetEntity)
    {
        if(targetEntity == Entity.Null || !targetEntity.Has<BuffBuffer>())
        {
            yield break;
        }

        var buffBuffer = Core.EntityManager.GetBufferReadOnly<BuffBuffer>(targetEntity);
        for (var i = 0; i < buffBuffer.Length; i++)
        {
            var buffEntity = buffBuffer[i].Entity;
            var prefabGuid = buffBuffer[i].PrefabGuid;

            if (buffEntity == Entity.Null || !buffEntity.Has<HideOutsideVision>())
                continue;

            if (prefabToGlowName.TryGetValue(prefabGuid, out var name))
                yield return name;
            else
                yield return $"Untracked glow: {prefabGuid.LookupName()}</color> - <color=white>{prefabGuid._Value}";

        }
    }
}

