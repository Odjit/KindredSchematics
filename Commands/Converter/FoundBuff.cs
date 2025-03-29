using ProjectM;
using Stunlock.Core;
using Stunlock.Localization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Unity.Collections;
using Unity.Entities.UniversalDelegates;
using VampireCommandFramework;

namespace KindredSchematics.Commands.Converter;

public record FoundBuff(PrefabGUID prefabGuid, string name);

public class FoundBuffConverter : CommandArgumentConverter<FoundBuff>
{
    public static Dictionary<string, PrefabGUID> buffPrefabs = new Dictionary<string, PrefabGUID>(StringComparer.InvariantCultureIgnoreCase);

    static PrefabGUID[] skipBuffs =
    {
        new PrefabGUID(1540104932),
        new PrefabGUID(-246207628),
        new PrefabGUID(-1130746976),
        new PrefabGUID(-1148833103),
        new PrefabGUID(-395364978),
        new PrefabGUID(985937733),
        new PrefabGUID(-1625210735),
        new PrefabGUID(1023033912),
        new PrefabGUID(317372843),
        new PrefabGUID(-398835659),
        new PrefabGUID(-1942510877),
        new PrefabGUID(1156367321),
        new PrefabGUID(1521207380),
        new PrefabGUID(1593366305),
        new PrefabGUID(424796826),
        new PrefabGUID(-1935151884),
        new PrefabGUID(1107291914),
        new PrefabGUID(-1149702315),
        new PrefabGUID(-128520871),
        new PrefabGUID(1290990039),
        new PrefabGUID(-165284501),
        new PrefabGUID(-352192969),
        //new PrefabGUID(1092547531)
        new PrefabGUID(-276630616),
        new PrefabGUID(390591357),
        new PrefabGUID(2113270604),
        new PrefabGUID(-695537141),//sticky bomb, not a problem but gblows up other dummies
        new PrefabGUID(-2135755764), //2270
        new PrefabGUID(817492469), //explodes, damaging others.
        new PrefabGUID(745999782),
        new PrefabGUID(1629786723),
    };

    public static void InitializeBuffPrefabs()
    {
        buffPrefabs.Clear();
        var resourceName = "KindredSchematics.Data.VisibleBuffs.txt";

        var assembly = Assembly.GetExecutingAssembly();
        var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream != null)
        {
            using var reader = new StreamReader(stream);
            string buffList = reader.ReadToEnd();
            // Process each line in the file to a PrefabGUID
            var lines = buffList.Split('\n');
            foreach (var line in lines)
            {
                if (int.TryParse(line, out var guid))
                {
                    var prefabGuid = new PrefabGUID(guid);

                    if (skipBuffs.Contains(prefabGuid)) continue;

                    if (Core.PrefabCollection._PrefabGuidToEntityMap.TryGetValue(prefabGuid, out var entity) && entity.Has<Buff>() && Core.PrefabCollection._PrefabGuidToNameDictionary.TryGetValue(prefabGuid, out var name))
                    {
                        buffPrefabs[name] = prefabGuid;
                    }
                }
            }
        }
        else
        {
            Console.WriteLine("Resource not found!");
        }

/*        buffPrefabs.Clear();
        var keyValueNativeArray = Core.PrefabCollection._PrefabGuidToEntityMap.GetKeyValueArrays(Allocator.Temp);
        for (var i = 0; i < keyValueNativeArray.Length; ++i)
        {
            var prefabEntity = keyValueNativeArray.Values[i];
            if (!prefabEntity.Has<Buff>()) continue;
            if (prefabEntity.Has<ReplaceAbilityOnSlotBuff>()) continue;
            if (prefabEntity.Has<BloodBuff>()) continue;
            if (prefabEntity.Has<Passive>()) continue;
            if (prefabEntity.Has<JumpFromCliffsTravelBuff>()) continue;
            if (skipBuffs.Contains(keyValueNativeArray.Keys[i])) continue;
            var prefabGuid = keyValueNativeArray.Keys[i];
            var prefabName = Core.PrefabCollection._PrefabGuidToNameDictionary[prefabGuid];
            buffPrefabs[prefabName] = prefabGuid;
        }
        keyValueNativeArray.Dispose();*/
    }
    public override FoundBuff Parse(ICommandContext ctx, string input)
    {
        if(buffPrefabs.TryGetValue(input, out var guid) ||
           buffPrefabs.TryGetValue("Buff_" + input, out guid))
        {
            return new FoundBuff(guid, input);
        }

        try
        {
            var prefab = new PrefabGUID(int.Parse(input));
            if(Core.PrefabCollection._PrefabGuidToEntityMap.TryGetValue(prefab, out var entity) && entity.Has<Buff>() && Core.PrefabCollection._PrefabGuidToNameDictionary.TryGetValue(prefab, out var name))
            {
                return new FoundBuff(prefab, name);
            }
        }
        catch { }

        // Search for a buff that contains the input
        var found = buffPrefabs
            .Where(kv => kv.Key.ToLower().Contains(input.ToLower()))
            .Select(kv => new FoundBuff(kv.Value, kv.Key))
            .ToList();

        if(found.Count == 1)
            return found[0];
        else if(found.Count > 1)
        {
            // Reply with a list of found buffs up to max of 10
            var sb = new StringBuilder();
            sb.AppendLine($"Found {found.Count} buff matches:");
            foreach(var buff in found.Take(10))
            {
                sb.AppendLine(buff.name);
            }
            if (found.Count > 10)
            {
                sb.AppendLine("...");
            }
            throw ctx.Error(sb.ToString());
        }

       throw ctx.Error($"Buff {input} not found");
    }
}
