using Stunlock.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VampireCommandFramework;

namespace KindredSchematics.Commands;

[CommandGroup("palette")]
static class PaletteCommands
{
    [Command("add", "a", "Adds a specified prefab search to the build palette.", adminOnly: true)]
    public static void AddToPalette(ChatCommandContext ctx, string search)
    {
        List<(string Name, PrefabGUID Prefab)> searchResults = [];
        try
        {
            foreach (var kvp in Data.Tile.LowerCaseNameToPrefab)
            {
                if (kvp.Key.Contains(search, StringComparison.OrdinalIgnoreCase))
                {
                    searchResults.Add((kvp.Key, kvp.Value));
                }
            }

            if (!searchResults.Any())
            {
                ctx.Reply("Could not find any matching prefabs.");
            }

            searchResults = searchResults.OrderBy(kvp => kvp.Name).ToList();

            Core.BuildService.AddListToPalette(ctx.Event.SenderCharacterEntity,
                searchResults.Select(kvp => Core.PrefabCollection._PrefabGuidToEntityMap[kvp.Prefab]).ToList());

            var sb = new StringBuilder();
            var totalCount = searchResults.Count;
            var pageSize = 7;

            if (totalCount > pageSize)
            {
                searchResults = searchResults.Take(pageSize).ToList();
            }

            sb.AppendLine($"<color=orange>Found</color> <color=white>{totalCount}</color> <color=orange>matches adding to the palette.</color>");
            foreach (var (Name, Prefab) in searchResults)
            {
                sb.AppendLine($"({Prefab.GuidHash}) {Name.Replace(search, $"<b>{search}</b>", StringComparison.OrdinalIgnoreCase)}");
            }

            ctx.Reply(sb.ToString());

        }
        catch (Exception e)
        {
            Core.LogException(e);
        }
    }

    [Command("remove", "r", "Removes a specified prefab search from the build palette.", adminOnly: true)]
    public static void RemoveFromPalette(ChatCommandContext ctx, string search)
    {
        List<(string Name, PrefabGUID Prefab)> searchResults = [];
        try
        {
            foreach (var kvp in Data.Tile.LowerCaseNameToPrefab)
            {
                if (kvp.Key.Contains(search, StringComparison.OrdinalIgnoreCase))
                {
                    searchResults.Add((kvp.Key, kvp.Value));
                }
            }

            if (!searchResults.Any())
            {
                ctx.Reply("Could not find any matching prefabs.");
            }

            searchResults = searchResults.OrderBy(kvp => kvp.Name).ToList();

            Core.BuildService.RemoveListFromPalette(ctx.Event.SenderCharacterEntity,
                               searchResults.Select(kvp => Core.PrefabCollection._PrefabGuidToEntityMap[kvp.Prefab]).ToList());

            var sb = new StringBuilder();
            var totalCount = searchResults.Count;
            var pageSize = 7;

            if (totalCount > pageSize)
            {
                searchResults = searchResults.Take(pageSize).ToList();
            }

            sb.AppendLine($"<color=orange>Found</color> <color=white>{totalCount}</color> <color=orange>matches removing from the palette</color>");
            foreach (var (Name, Prefab) in searchResults)
            {
                sb.AppendLine($"({Prefab.GuidHash}) {Name.Replace(search, $"<b>{search}</b>", StringComparison.OrdinalIgnoreCase)}");
            }

            ctx.Reply(sb.ToString());
        }
        catch (Exception e)
        {
            Core.LogException(e);
        }
    }

    [Command("list", "l", "Lists all prefabs in the build palette.", adminOnly: true)]
    public static void ListPalette(ChatCommandContext ctx, int page = 0)
    {
        var palette = Core.BuildService.GetPalette(ctx.Event.SenderCharacterEntity);

        if (palette.Length == 0)
        {
            ctx.Reply("<color=orange>Build palette</color> is <color=red>empty</color>.");
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine("<color=orange>Build Palette</color>:");

        var pageSize = 7;
        var pageCount = (palette.Length + pageSize - 1) / pageSize;
        page = Math.Clamp(page, 0, pageCount - 1);
        foreach(var prefabGuid in palette.Skip(page * pageSize).Take(pageSize))
        {
            var prefab = Core.PrefabCollection._PrefabGuidToEntityMap[prefabGuid];
            sb.AppendLine($"{prefabGuid.LookupName()}");
        }

        if (pageCount > 1)
        {
            sb.AppendLine($"<color=orange>Page</color> <color=white>{page + 1}</color>/<color=yellow>{pageCount}</color>");
        }

        ctx.Reply(sb.ToString());
    }

    [Command("clear", "c", "Clears the build palette.", adminOnly: true)]
    public static void ClearPalette(ChatCommandContext ctx)
    {
        Core.BuildService.ClearPalette(ctx.Event.SenderCharacterEntity);
        ctx.Reply("<color=orange>Build palette</color> <color=red>cleared</color>.");
    }
}
