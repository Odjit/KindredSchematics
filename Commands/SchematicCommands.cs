using ProjectM;
using ProjectM.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using VampireCommandFramework;

namespace KindredSchematics.Commands
{
    [CommandGroup("schematic", "sc")]
    internal class SchematicCommands
    {
        [Command("list", "l", description: "Lists all saved schematics", adminOnly: true)]
        public static void ListSchematics(ChatCommandContext ctx)
        {
            var schematics = Core.SchematicService.GetSchematics();
            if (schematics.Any())
            {
                var sb = new StringBuilder();
                sb.AppendLine("Saved schematics:");
                foreach (var name in schematics)
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
                ctx.Reply("No saved schematics");
            }
        }

        [Command("save", "s", description: "Saves the current area to a schematic", adminOnly: true)]
        public static void SaveSchematic(ChatCommandContext ctx, string schematicName, float radius = 5)
        {
            var userEntity = ctx.Event.SenderUserEntity;
            var userPos = userEntity.Read<Translation>().Value;

            Core.SchematicService.SaveSchematic(schematicName, userPos, radius: radius);
            ctx.Reply($"Saved schematic {schematicName}");
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

        [Command("savebox", "sb", description: "Saves the current area to a schematic", adminOnly: true)]
        public static void SaveSchematicBox(ChatCommandContext ctx, string schematicName)
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
            Core.SchematicService.SaveSchematic(schematicName, new float3(center.x, charPos.y, center.y), halfSize: halfSize);
            ctx.Reply($"Saved schematic {schematicName}");
            cornerPos.Remove(ctx.Event.SenderUserEntity);
        }

        [Command("saveterritory", "st", description: "Saves the current/specified territory to a schematic", adminOnly: true)]
        public static void SaveTerritorySchematic(ChatCommandContext ctx, string schematicName, int? territoryIndex = null)
        {
            if (territoryIndex == null)
            {
                var charEntity = ctx.Event.SenderCharacterEntity;
                var charPos = charEntity.Read<Translation>().Value;
                territoryIndex = Core.CastleTerritory.GetTerritoryIndex(charPos);

                if (territoryIndex.Value == -1)
                {
                    ctx.Reply("Not in a territory");
                    return;
                }
            }
            Core.SchematicService.SaveSchematic(schematicName, territoryIndex: territoryIndex.Value);
            ctx.Reply($"Saved territory {territoryIndex} to schematic {schematicName}");
        }

        [Command("load", "l", description: "Loads a schematic", adminOnly: true)]
        public static void LoadSchematic(ChatCommandContext ctx, string schematicName, float expandClear = 0)
        {
            var failReason = Core.SchematicService.LoadSchematic(schematicName, ctx.Event.SenderUserEntity, ctx.Event.SenderCharacterEntity, expandClear);
            if (failReason == null)
            {
                ctx.Reply($"Loading schematic {schematicName}");
            }
            else if (failReason == "")
            {
                ctx.Reply($"Failed to load schematic {schematicName}");
            }
            else
            {
                ctx.Reply($"Failed to load schematic {schematicName}: {failReason}");
            }
        }

        [Command("loadatpos", "lp", description: "Loads a schematic where you are standing", adminOnly: true)]
        public static void LoadSchematicAtPosition(ChatCommandContext ctx, string schematicName, float expandClear = 1, float heightOffset = 0)
        {
            var failReason = Core.SchematicService.LoadSchematic(schematicName, ctx.Event.SenderUserEntity, ctx.Event.SenderCharacterEntity, expandClear,
                ctx.Event.SenderCharacterEntity.Read<Translation>().Value + new float3(0, heightOffset, 0));
            if (failReason == null)
            {
                ctx.Reply($"Loading schematic {schematicName}");
            }
            else if (failReason == "")
            {
                ctx.Reply($"Failed to load schematic {schematicName}");
            }
            else
            {
                ctx.Reply($"Failed to load schematic {schematicName}: {failReason}");
            }
        }

        [Command("loadat", "la", description: "Loads a schematic where you specify", adminOnly: true)]
        public static void LoadSchematicAtPosition(ChatCommandContext ctx, string schematicName, float x, float y, float z, float expandClear = 1)
        {
            var failReason = Core.SchematicService.LoadSchematic(schematicName, ctx.Event.SenderUserEntity, ctx.Event.SenderCharacterEntity, expandClear,
                new float3(x, y, z));
            if (failReason == null)
            {
                ctx.Reply($"Loading schematic {schematicName}");
            }
            else if (failReason == "")
            {
                ctx.Reply($"Failed to load schematic {schematicName}");
            }
            else
            {
                ctx.Reply($"Failed to load schematic {schematicName}: {failReason}");
            }
        }

        [Command("toggleplacegrid", description: "Toggles placing schematics on the grid", adminOnly: true)]
        public static void TogglePlacingOffGrid(ChatCommandContext ctx)
        {
            if (Core.SchematicService.TogglePlacingOffGrid(ctx.Event.SenderUserEntity))
            {
                ctx.Reply("Placing off the grid");
            }
            else
            {
                ctx.Reply("Placing on the grid");
            }
        }

        [Command("remap", description: "Remaps one prefab to another for loading schematics", adminOnly: true)]
        public static void Remap(ChatCommandContext ctx, string sourcePrefabName, string targetPrefabName)
        {
            // Validate the target name
            if (!Core.PrefabCollection._SpawnableNameToPrefabGuidDictionary.ContainsKey(targetPrefabName))
            {
                ctx.Reply($"Target prefab name {targetPrefabName} isn't a valid prefab");
                return;
            }

            Core.PrefabRemap.AddPrefabMapping(sourcePrefabName, targetPrefabName);
            ctx.Reply($"Prefab '{sourcePrefabName}' will now be loaded as {targetPrefabName}");
        }

        [Command("removeremap", description: "Removes a single specified remap", adminOnly: true)]
        public static void RemoveRemap(ChatCommandContext ctx, string remapPrefabName)
        {
            if (Core.PrefabRemap.RemovePrefabMapping(remapPrefabName))
            {
                ctx.Reply("Removed remap for " + remapPrefabName);
            }
            else
            {
                ctx.Reply("There wasn't a prefab remap for " + remapPrefabName);
            }
        }

        [Command("clearremaps", description: "Clears all prefab remappings", adminOnly: true)]
        public static void ClearRemaps(ChatCommandContext ctx)
        {
            Core.PrefabRemap.ClearMappings();
            ctx.Reply("Prefab remappings all cleared.");
        }

        [Command("listremaps", description: "List all prefab remappings", adminOnly: true)]
        public static void ListRemaps(ChatCommandContext ctx)
        {
            var i = 0;
            var sb = new StringBuilder("Prefab Remaps");
            foreach (var mapping in Core.PrefabRemap.GetMappings())
            {
                sb.AppendLine($"{mapping.Key} -> {mapping.Value}");
                if (++i == 7)
                {
                    ctx.Reply(sb.ToString());
                    sb.Clear();
                    i = 0;
                }
            }
        }

        [Command("deleteallschematicentities", description: "Delete all schematic spawned entities", adminOnly: true)]
        public static void DeleteAllSchematicEntities(ChatCommandContext ctx)
        {
            var entitiesToDestroy = Helper.GetEntitiesByComponentType<PhysicsCustomTags>(includeDisabled: true);
            foreach (var entity in entitiesToDestroy)
            {
                DestroyUtility.Destroy(Core.EntityManager, entity);
            }
            ctx.Reply($"All {entitiesToDestroy.Length} schematic spawned entities marked for deletion");
        }

        [Command("removeschematicrange", description: "Deletes all schematic spawned entities within a range", adminOnly: true)]
        public static void RemoveSchematicRange(ChatCommandContext ctx, float range = 5)
        {
            var charEntity = ctx.Event.SenderCharacterEntity;
            var charPos = charEntity.Read<Translation>().Value;
            var entitiesToDestroy = Helper.GetAllEntitiesInRadius<PhysicsCustomTags>(charPos.xz, range).ToList();
            foreach (var entity in entitiesToDestroy)
            {
                DestroyUtility.Destroy(Core.EntityManager, entity);
            }
            ctx.Reply($"Removed {entitiesToDestroy.Count} schematic spawned entities within {range}m");
        }
    }
}
