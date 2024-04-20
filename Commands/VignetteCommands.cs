using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using VampireCommandFramework;

namespace KindredVignettes.Commands
{
    [CommandGroup("vignette", "v")]
    internal class VignetteCommands
    {
        [Command("list", "l", description: "Lists all saved vignettes")]
        public static void ListVignettes(ChatCommandContext ctx)
        {
            var vignetteNames = Core.VignetteService.GetVignetteNames();
            if (vignetteNames.Any())
            {
                var sb = new StringBuilder();
                sb.AppendLine("Saved vignettes:");
                foreach (var name in vignetteNames)
                {
                    if (sb.Length + name.Length + 2 > Core.MAX_REPLY_LENGTH)
                    {
                        ctx.Reply(sb.ToString());
                        sb.Clear();
                    }
                    sb.AppendLine(name);
                }
                ctx.Reply(sb.ToString());
            }
            else
            {
                ctx.Reply("No saved vignettes");
            }
        }

        [Command("save", "s", description: "Saves the current area to a vignette", adminOnly: true)]
        public static void SaveVignette(ChatCommandContext ctx, string vignetteName, float radius=5)
        {
            var userEntity = ctx.Event.SenderUserEntity;
            var userPos = userEntity.Read<Translation>().Value;
            
            Core.VignetteService.SaveVignette(vignetteName, userPos, radius: radius);
            ctx.Reply($"Saved vignette {vignetteName}");
        }

        static readonly Dictionary<Entity, float2> cornerPos = [];
        [Command("setcorner", "sc", description: "Sets a corner for saving", adminOnly: true)]
        public static void SetCorner(ChatCommandContext ctx)
        {
            var userEntity = ctx.Event.SenderUserEntity;
            var userPos = userEntity.Read<Translation>().Value;
            cornerPos[ctx.Event.SenderUserEntity] = userPos.xz;
            ctx.Reply($"Corner set");
        }

        [Command("savebox", "sb", description: "Saves the current area to a vignette", adminOnly: true)]
        public static void SaveVignetteBox(ChatCommandContext ctx, string vignetteName)
        {
            if (!cornerPos.ContainsKey(ctx.Event.SenderUserEntity))
            {
                ctx.Reply("You need to set the other corner first");
                return;
            }
            var charEntity = ctx.Event.SenderCharacterEntity;
            var charPos = charEntity.Read<Translation>().Value;
            var corner = cornerPos[ctx.Event.SenderUserEntity];
            var halfSize = math.abs(charPos.xz - corner) / 2;
            var center = (charPos.xz + corner) / 2;
            Core.VignetteService.SaveVignette(vignetteName, new float3(center.x, charPos.y, center.y), halfSize: halfSize);
            ctx.Reply($"Saved vignette {vignetteName}");
            cornerPos.Remove(ctx.Event.SenderUserEntity);
        }

        [Command("load", "l", description: "Loads a vignette", adminOnly: true)]
        public static void LoadVignette(ChatCommandContext ctx, string vignetteName, float expandClear=0)
        {
            if(Core.VignetteService.LoadVignette(vignetteName, ctx.Event.SenderUserEntity, ctx.Event.SenderCharacterEntity, expandClear))
            {
                ctx.Reply($"Loaded vignette {vignetteName}");
            }
            else
            {
                ctx.Reply($"Failed to load vignette {vignetteName}");
            }
        }

        [Command("loadatpos", "lp", description: "Loads a vignette where you are standing", adminOnly: true)]
        public static void LoadVignetteAtPosition(ChatCommandContext ctx, string vignetteName, float expandClear=1, float heightOffset=0)
        {
            if (Core.VignetteService.LoadVignette(vignetteName, ctx.Event.SenderUserEntity, ctx.Event.SenderCharacterEntity, expandClear,
                ctx.Event.SenderCharacterEntity.Read<Translation>().Value + new float3(0, heightOffset, 0)))
            {
                ctx.Reply($"Loaded vignette {vignetteName}");
            }
            else
            {
                ctx.Reply($"Failed to load vignette {vignetteName}");
            }
        }

        [Command("loadat", "la", description: "Loads a vignette where you specify", adminOnly: true)]
        public static void LoadVignetteAtPosition(ChatCommandContext ctx, string vignetteName, float x, float y, float z, float expandClear = 1)
        {
            if (Core.VignetteService.LoadVignette(vignetteName, ctx.Event.SenderUserEntity, ctx.Event.SenderCharacterEntity, expandClear,
                new float3(x, y, z)))
            {
                ctx.Reply($"Loaded vignette {vignetteName}");
            }
            else
            {
                ctx.Reply($"Failed to load vignette {vignetteName}");
            }
        }

        [Command("toggleclearall", description: "Toggles clearing the entire area within radius/2d rectangle instead of just what overlaps", adminOnly: true)]
        public static void ToggleClearAll(ChatCommandContext ctx)
        {
            if (Core.VignetteService.ToggleClearingEntireArea(ctx.Event.SenderUserEntity))
            {
                ctx.Reply("Clearing entire area");
            }
            else
            {
                ctx.Reply("Cleared only overlaps");
            }
        }

        [Command("toggleplacegrid", description: "Toggles placing vignettes on the grid", adminOnly: true)]
        public static void TogglePlacingOffGrid(ChatCommandContext ctx)
        {
            if (Core.VignetteService.TogglePlacingOffGrid(ctx.Event.SenderUserEntity))
            {
                ctx.Reply("Placing off the grid");
            }
            else
            {
                ctx.Reply("Placing on the grid");
            }
        }
    }
}
