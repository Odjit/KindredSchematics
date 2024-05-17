using ProjectM;
using Stunlock.Core;
using System.Linq;
using System.Text;
using VampireCommandFramework;

namespace KindredVignettes.Commands.Converter;

public record FoundTileModel(PrefabGUID Value, string name);

public class FoundTileModelConverter : CommandArgumentConverter<FoundTileModel>
{
    public override FoundTileModel Parse(ICommandContext ctx, string input)
    {
        if(Data.Tile.LowerCaseNameToPrefab.TryGetValue(input.ToLower(), out var guid) ||
           Data.Tile.LowerCaseNameToPrefab.TryGetValue("TM_" + input, out guid))
        {
            return new FoundTileModel(guid, input);
        }

        try
        {
            var guidHash = int.Parse(input);
            if(Data.Tile.NameFromPrefab.TryGetValue(guidHash, out var name))
            {
                return new FoundTileModel(new PrefabGUID(guidHash), name);
            }
        }
        catch { }

        // Search for a tile that contains the input
        var found = Data.Tile.LowerCaseNameToPrefab
            .Where(kv => kv.Key.Contains(input.ToLower()))
            .Select(kv => new FoundTileModel(kv.Value, Data.Tile.NameFromPrefab[kv.Value.GuidHash]))
            .ToList();

        if(found.Count == 1)
            return found[0];
        else if(found.Count > 1)
        {
            // Reply with a list of found tiles up to max of 10
            var sb = new StringBuilder();
            sb.AppendLine($"Found {found.Count} tile matches:");
            foreach(var tile in found.Take(10))
            {
                sb.AppendLine(tile.name);
            }
            if (found.Count > 10)
            {
                sb.AppendLine("...");
            }
            throw ctx.Error(sb.ToString());
        }

       throw ctx.Error($"Tile {input} not found");
    }
}
