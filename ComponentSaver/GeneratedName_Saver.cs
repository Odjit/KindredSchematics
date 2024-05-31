using KindredVignettes.Services;
using ProjectM.Shared;
using Stunlock.Core;
using System.Text.Json;
using Unity.Entities;

namespace KindredVignettes.ComponentSaver;

[ComponentType(typeof(GeneratedName))]
class GeneratedName_Saver : ComponentSaver
{
    struct GeneratedName_Save
    {
        public byte? RandomNamePrefix { get; set; }
        public byte? RandomNamePostfix { get; set; }
        public PrefabGUID? NameGeneratorPrefixSource { get; set; }
        public PrefabGUID? NameGeneratorPostfixSource { get; set; }
    }

    public override object DiffComponents(Entity prefab, Entity entity, EntityMapper entityMapper)
    {
        var prefabData = prefab.Read<GeneratedName>();
        var entityData = entity.Read<GeneratedName>();

        var saveData = new GeneratedName_Save();
        if (prefabData.RandomNamePrefix != entityData.RandomNamePrefix)
            saveData.RandomNamePrefix = entityData.RandomNamePrefix;
        if (prefabData.RandomNamePostfix != entityData.RandomNamePostfix)
            saveData.RandomNamePostfix = entityData.RandomNamePostfix;
        if (prefabData.NameGeneratorPrefixSource != entityData.NameGeneratorPrefixSource)
            saveData.NameGeneratorPrefixSource = entityData.NameGeneratorPrefixSource;
        if (prefabData.NameGeneratorPostfixSource != entityData.NameGeneratorPostfixSource)
            saveData.NameGeneratorPostfixSource = entityData.NameGeneratorPostfixSource;

        if (saveData.Equals(default(GeneratedName_Save)))
            return null;

        return saveData;
    }

    public override object SaveComponent(Entity entity, EntityMapper entityMapper)
    {
        var data = entity.Read<GeneratedName>();

        var saveData = new GeneratedName_Save
        {
            RandomNamePrefix = data.RandomNamePrefix,
            RandomNamePostfix = data.RandomNamePostfix,
            NameGeneratorPrefixSource = data.NameGeneratorPrefixSource,
            NameGeneratorPostfixSource = data.NameGeneratorPostfixSource
        };

        return saveData;
    }

    public override void ApplyComponentData(Entity entity, JsonElement jsonData, Entity[] entitiesBeingLoaded)
    {
        var saveData = jsonData.Deserialize<GeneratedName_Save>(VignetteService.GetJsonOptions());

        if (!entity.Has<GeneratedName>())
            entity.Add<GeneratedName>();

        var data = entity.Read<GeneratedName>();

        if (saveData.RandomNamePrefix.HasValue)
            data.RandomNamePrefix = saveData.RandomNamePrefix.Value;
        if (saveData.RandomNamePostfix.HasValue)
            data.RandomNamePostfix = saveData.RandomNamePostfix.Value;
        if (saveData.NameGeneratorPrefixSource.HasValue)
            data.NameGeneratorPrefixSource = saveData.NameGeneratorPrefixSource.Value;
        if (saveData.NameGeneratorPostfixSource.HasValue)
            data.NameGeneratorPostfixSource = saveData.NameGeneratorPostfixSource.Value;

        entity.Write(data);
    }
}
