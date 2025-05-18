using Newtonsoft.Json.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace KindredSchematics.Services;
class PrefabRemapService
{
    static readonly string CONFIG_PATH = Path.Combine(BepInEx.Paths.ConfigPath, MyPluginInfo.PLUGIN_NAME);
    private static readonly string PREFAB_REMAP_PATH = Path.Combine(CONFIG_PATH, "prefabRemaps.txt");

    readonly Dictionary<string, string> remap = [];

    public PrefabRemapService()
    {
        LoadMappings();
    }

    public string GetPrefabMapping(string prefabName)
    {
        if (remap.TryGetValue(prefabName, out var mapping))
            return mapping;
        return prefabName;
    }

    public void AddPrefabMapping(string prefabName, string mapping)
    {
        remap[prefabName] = mapping;
        SaveMappings();
    }

    public bool RemovePrefabMapping(string prefabName)
    {
        var result = remap.Remove(prefabName);
        if (result)
            SaveMappings();
        return result;
    }

    public IEnumerable<KeyValuePair<string, string>> GetMappings()
    {
        return remap;
    }

    public void ClearMappings()
    {
        remap.Clear();
    }

    void SaveMappings()
    {
        if (!Directory.Exists(CONFIG_PATH))
            Directory.CreateDirectory(CONFIG_PATH);
        var sb = new StringBuilder();
        foreach (var kvp in remap)
        {
            sb.AppendLine($"{kvp.Key}={kvp.Value}");
        }
        File.WriteAllText(PREFAB_REMAP_PATH, sb.ToString());
    }

    void LoadMappings()
    {
        if (!File.Exists(PREFAB_REMAP_PATH))
        {
            // Default mappings
            AddPrefabMapping("TM_Castle_ObjectDecor_Table_3x3_Cabal01", "TM_Castle_Module_Parent_RoundTable_3x3_Cabal01");
            AddPrefabMapping("TM_Castle_ObjectDecor_Table_10x6_Gothic01", "TM_Castle_Module_Parent_RectangularTable_10x6_Gothic01");
            return;
        }

        remap.Clear();
        var lines = File.ReadAllLines(PREFAB_REMAP_PATH);
        foreach (var line in lines)
        {
            var parts = line.Split('=');
            if (parts.Length != 2) continue;
            remap[parts[0].Trim()] = parts[1].Trim();
        }
    }
}
