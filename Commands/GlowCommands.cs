using Il2CppInterop.Runtime;
using KindredSchematics.Commands.Converter;
using ProjectM;
using ProjectM.Behaviours;
using ProjectM.CastleBuilding;
using ProjectM.Network;
using ProjectM.Shared;
using ProjectM.Tiles;
using Stunlock.Core;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using VampireCommandFramework;

namespace KindredSchematics.Commands;

[CommandGroup("glow", "gl")]


class GlowCommands
{
    [Command("add", description: "Makes the tile closest to mouse cursor glow", adminOnly: true, usage: ".gl add")]
    public static void GlowTile(ChatCommandContext ctx, FoundGlow glow)
    {
        var aimPos = ctx.Event.SenderCharacterEntity.Read<EntityAimData>().AimPosition;
        var closest = Helper.FindClosestTilePosition(aimPos);

        Core.GlowService.AddGlow(ctx.Event.SenderUserEntity, closest, glow.prefab);

        ctx.Reply($"Added glow <color=yellow>{glow.name}</color> to tile <color=white>{closest.Read<PrefabGUID>().LookupName()}</color> glow");
    }

    [Command("remove", description: "Removes the glow from the tile closest to mouse cursor", adminOnly: true)]
    public static void RemoveGlowTile(ChatCommandContext ctx, FoundGlow glow)
    {
        var aimPos = ctx.Event.SenderCharacterEntity.Read<EntityAimData>().AimPosition;
        var closest = Helper.FindClosestTilePosition(aimPos);
        
        if(Core.GlowService.RemoveGlow(closest, glow.prefab))
            ctx.Reply($"Removed glow <color=yellow>{glow.name}</color> from tile <color=white>{closest.Read<PrefabGUID>().LookupName()}</color>");
        else
            ctx.Reply($"Glow <color=yellow>{glow.name}</color> not found on tile <color=white>{closest.Read<PrefabGUID>().LookupName()}</color>");
    }

    [Command("new", description: "Adds a new glow choice", adminOnly: true)]
    public static void AddNewGlow(ChatCommandContext ctx, FoundBuff buff, string name)
    {
        Core.GlowService.AddNewGlowChoice(buff.prefabGuid, name);
        ctx.Reply($"Added glow choice <color=yellow>{name}</color> of buff <color=white>{buff.name}</color>");
    }

    [Command("delete", description: "Deletes a glow choice", adminOnly: true)]
    public static void DeleteGlow(ChatCommandContext ctx, string name)
    {
        if(Core.GlowService.RemoveGlowChoice(name))
            ctx.Reply($"Deleted glow <color=yellow>{name}</color>");
        else
            ctx.Reply($"Glow choice <color=yellow>{name}</color> not found");
    }

    [Command("list", description: "Lists all available glow choices", adminOnly: true)]
    public static void ListGlows(ChatCommandContext ctx)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Available glow choices:");
        foreach (var (glowName, prefab) in Core.GlowService.ListGlowChoices())
        {
            var nextLine = $"<color=yellow>{glowName}</color> - <color=white>{prefab.LookupName()}</color>";
            if (sb.Length + nextLine.Length > Core.MAX_REPLY_LENGTH)
            {
                ctx.Reply(sb.ToString());
                sb.Clear();
            }
            sb.AppendLine(nextLine);
        }
        ctx.Reply(sb.ToString());
    }

    [Command("check", description: "Checks the glows of the tile closest to mouse cursor", adminOnly: true)]
    public static void CheckGlowTile(ChatCommandContext ctx)
    {
        var aimPos = ctx.Event.SenderCharacterEntity.Read<EntityAimData>().AimPosition;
        var closest = Helper.FindClosestTilePosition(aimPos);

        var glowsOnTarget = Core.GlowService.ListGlows(closest);

        if (glowsOnTarget.Any())
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Tile <color=white>{closest.Read<PrefabGUID>().LookupName()}</color> has the following glows:");
            foreach (var glow in glowsOnTarget)
            {
                var nextLine = $"<color=yellow>{glow}</color>";
                if (sb.Length + nextLine.Length > Core.MAX_REPLY_LENGTH)
                {
                    ctx.Reply(sb.ToString());
                    sb.Clear();
                }
                sb.AppendLine(nextLine);
            }
            ctx.Reply(sb.ToString());
        }
        else
        {
            ctx.Reply($"Tile {closest.Read<PrefabGUID>().LookupName()} has no glows");
            if (closest.Has<BuffBuffer>())
            {
                var buffs = Core.EntityManager.GetBuffer<BuffBuffer>(closest);
                foreach (var buff in buffs)
                {
                    ctx.Reply($"Does have buff <color=yellow>{buff.PrefabGuid.LookupName()}</color> - <color=white>{buff.PrefabGuid._Value}</color>");
                }
            }
        }
    }

    static readonly float3 LIBRARY_POS = new float3(-2657.5f, 10f, -987.5f);

    [Command("library", description: "Spawns/visit a library of valid buffs for glows", adminOnly: true)]
    public static void SpawnGlowLibrary(ChatCommandContext ctx)
    {
        var dummyQueryDesc = new EntityQueryDesc
        {
            All = new ComponentType[] {
                new(Il2CppType.Of<Health>(), ComponentType.AccessMode.ReadWrite),
                new(Il2CppType.Of<ArmorLevel>(), ComponentType.AccessMode.ReadWrite),
                new(Il2CppType.Of<CastleBuildingMaxRange>(), ComponentType.AccessMode.ReadWrite),
                new(Il2CppType.Of<Translation>(), ComponentType.AccessMode.ReadWrite),
                new(Il2CppType.Of<TilePosition>(), ComponentType.AccessMode.ReadWrite),
                new(Il2CppType.Of<PrefabGUID>(), ComponentType.AccessMode.ReadWrite),
            },
        };
        var dummyQuery = Core.EntityManager.CreateEntityQuery(dummyQueryDesc);
        var entities = dummyQuery.ToEntityArray(Allocator.Temp);

        foreach(var entity in entities)
        {
            if (entity.Read<PrefabGUID>()._Value != 230163020) continue;
            var pos = entity.Read<Translation>().Value;
            if (math.distance(pos, LIBRARY_POS) < 5f)
            {
                ctx.Reply("Library already spawned so teleporting");
                ctx.Event.SenderCharacterEntity.Write<Translation>(new Translation { Value = LIBRARY_POS });
                ctx.Event.SenderCharacterEntity.Write<LastTranslation>(new LastTranslation { Value = LIBRARY_POS });
                return;
            }
        }
        
        entities.Dispose();
        dummyQuery.Dispose();

        ctx.Reply("Spawning library of buffs and teleporting");

        Core.StartCoroutine(SpawningGlowLibrary(ctx, LIBRARY_POS));
    }

    static bool spawningPaused;
    static Stack<(Entity entity, string buffName, PrefabGUID buff)> entitiesSpawned = new();
    static Dictionary<Entity, PrefabGUID> entityAndBuff = new();

    private static IEnumerator SpawningGlowLibrary(ChatCommandContext ctx, float3 pos)
    {
        var waitLength = new WaitForSeconds(0.025f);
        spawningPaused = false;
        Core.PrefabCollection._PrefabGuidToEntityMap.TryGetValue(new PrefabGUID(230163020), out var targetDummyPrefab);
        Core.PrefabCollection._PrefabGuidToEntityMap.TryGetValue(new PrefabGUID(-1897961716), out var foundationPrefab);
        int count = 0;
        int x = 0, y = 0, dx = 1, dy = 0;
        int segmentLength = 1;
        int segmentPassed = 0;
        foreach (var buffEntry in FoundBuffConverter.buffPrefabs.Skip(count))
        {
            var dummyToAdd = Core.EntityManager.Instantiate(targetDummyPrefab);
            var foundation = Core.EntityManager.Instantiate(foundationPrefab);

            // Place the dummy in a spiral grid pattern
            var dummyPos = pos + new float3(x * 5f, 0, y * 5f);

            dummyToAdd.Write(new Translation { Value = dummyPos });
            foundation.Write(new Translation { Value = dummyPos });

            entityAndBuff[dummyToAdd] = buffEntry.Value;
            entitiesSpawned.Push((dummyToAdd, buffEntry.Key, buffEntry.Value));
            Core.Log.LogInfo($"SpawningGlowLibrary dummy with buff {count} {buffEntry.Key} {buffEntry.Value._Value}");

            Core.GlowService.AddNewGlowChoice(buffEntry.Value, $"{buffEntry.Key}</color> - <color=green>{buffEntry.Value._Value}");
            Core.GlowService.AddGlow(ctx.Event.SenderUserEntity, dummyToAdd, buffEntry.Value);

            yield return waitLength;

            // Teleport the player
            if (count == 0)
            {
                ctx.Event.SenderCharacterEntity.Write<Translation>(new Translation { Value = pos });
                ctx.Event.SenderCharacterEntity.Write<LastTranslation>(new LastTranslation { Value = pos });
            }

            while (spawningPaused)
            {
                yield return waitLength;
            }

            count += 1;

            x += dx;
            y += dy;
            segmentPassed++;

            if (segmentPassed == segmentLength)
            {
                segmentPassed = 0;

                // Change direction
                int temp = dx;
                dx = -dy;
                dy = temp;

                // Increase segment length after completing a vertical movement
                if (dy == 0)
                {
                    segmentLength++;
                }
            }
        }
    }

    /*
    static HashSet<int> removedBuffs = [];
    [HarmonyLib.HarmonyPatch(typeof(AbilityRunScriptsSystem), nameof(AbilityRunScriptsSystem.OnUpdate))]
    public static class UpdatePatch
    {
        public static void Postfix(AbilityRunScriptsSystem __instance)
        {
            var entities = __instance._OnCastStartedQuery.ToEntityArray(Allocator.TempJob);
            foreach(var entity in entities)
            {
                var acse = entity.Read<AbilityCastStartedEvent>();
                var abilityPrefab = acse.Ability.Read<PrefabGUID>();
                var abilityName = abilityPrefab.LookupName();
                
                if (abilityName.StartsWith("AB_Vampire_Unarmed_Primary_MeleeAttack_Cast"))
                {
                    spawningPaused = !spawningPaused;
                    Core.Log.LogInfo($"Paused spawning: {spawningPaused}");
                    var pc = acse.Character.Read<PlayerCharacter>();
                    var user = pc.UserEntity.Read<User>();
                    ServerChatUtils.SendSystemMessageToClient(Core.EntityManager, user, $"Paused spawning: {spawningPaused}");
                }
                else if (abilityName.Contains("DashCast") && entitiesSpawned.Count > 0)
                {
                    var removing = entitiesSpawned.Pop();
                    Core.GlowService.RemoveGlow(removing.entity, removing.buff);
                    DestroyUtility.Destroy(Core.EntityManager, removing.entity);
                    Core.Log.LogInfo($"Removed dummy with buff {removing.buffName}");
                    var pc = acse.Character.Read<PlayerCharacter>();
                    var user = pc.UserEntity.Read<User>();
                    ServerChatUtils.SendSystemMessageToClient(Core.EntityManager, user, $"Removed dummy with buff {removing.buffName}");
                }
                else if (abilityPrefab._Value == 863435029)
                {
                    foreach (var e in Helper.GetAllEntitiesInRadius<TileModel>(acse.Character.Read<Translation>().Value.xz, 1f))
                    {
                        var p = e.Read<PrefabGUID>();
                        if (p._Value == 230163020)
                        {
                            var removingBuff = entityAndBuff[e];
                            removedBuffs.Add(removingBuff._Value);
                            Core.GlowService.RemoveGlow(e, removingBuff);
                            DestroyUtility.Destroy(Core.EntityManager, e);

                            // Save out values of buffsPrefabs to a file
                            var sb = new StringBuilder();
                            foreach (var (name, guid) in FoundBuffConverter.buffPrefabs)
                            {
                                if (!removedBuffs.Contains(guid._Value))
                                    sb.AppendLine($"{guid._Value}");
                            }

                            System.IO.File.WriteAllText("potentialBuffs.txt", sb.ToString());

                            var removedMessage = $"Removed dummy with buff {removingBuff.LookupName()} {removingBuff._Value}";
                            Core.Log.LogInfo(removedMessage);
                            var pc = acse.Character.Read<PlayerCharacter>();
                            var user = pc.UserEntity.Read<User>();
                            ServerChatUtils.SendSystemMessageToClient(Core.EntityManager, user, removedMessage);
                        }
                    }

                }
            }
            entities.Dispose();
        }
    }*/
}

