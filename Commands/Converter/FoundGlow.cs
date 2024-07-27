using Stunlock.Core;
using VampireCommandFramework;

namespace KindredSchematics.Commands.Converter;
public record FoundGlow(PrefabGUID prefab, string name);

public class FoundGlowConverter : CommandArgumentConverter<FoundGlow>
{
    public override FoundGlow Parse(ICommandContext ctx, string input)
    {
        var guid = Core.GlowService.GetGlowEntry(input);
        if (guid != default)
        {
            return new FoundGlow(guid, input);
        }
        throw ctx.Error($"Glow {input} not found");
    }
}
