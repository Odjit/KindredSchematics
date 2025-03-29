using HarmonyLib;
using ProjectM;
using ProjectM.Gameplay.Systems;
using ProjectM.Network;
using Stunlock.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;

namespace KindredSchematics.Patches;


[HarmonyPatch(typeof(AbilityRunScriptsSystem), nameof(AbilityRunScriptsSystem.OnUpdate))]
internal class AbilityRunScriptsSystemPatch
{
    public static bool Prefix(AbilityRunScriptsSystem __instance)
    {
        var entities = __instance._OnCastStartedQuery.ToEntityArray(Allocator.Temp);
        foreach (var entity in entities)
        {
            var acse = entity.Read<AbilityCastStartedEvent>();
            if (Core.BuildService.CheckAbilityUsage(acse.Character, acse.AbilityGroup.Read<PrefabGUID>()))
            {
                entity.Remove<AbilityCastStartedEvent>();
            }
        }
        entities.Dispose();
        return true;


    }
}