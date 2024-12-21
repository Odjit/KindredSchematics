using KindredSchematics.Services;
using ProjectM;
using Stunlock.Core;
using System.Text.Json;
using Unity.Entities;

namespace KindredSchematics.ComponentSaver;
[ComponentType(typeof(RandomizedSpawnChain))]
class RandomizedSpawnChain_Saver : ComponentSaver
{
    struct RandomizedSpawnChain_Save
    {
        public PrefabGUID? Settings { get; set; }
        public int? SpawnChainInstance { get; set; }
        public double LastChildSurplusAutoChainTime { get; set; }
        public bool Initialized { get; set; }
    }

    public override object DiffComponents(Entity prefab, Entity entity, EntityMapper entityMapper)
    {
        var prefabData = prefab.Read<RandomizedSpawnChain>();
        var entityData = entity.Read<RandomizedSpawnChain>();

        var saveData = new RandomizedSpawnChain_Save();
        if (prefabData.Settings != entityData.Settings)
            saveData.Settings = entityData.Settings;
        if (prefabData.SpawnChainInstance != entityData.SpawnChainInstance)
            saveData.SpawnChainInstance = entityMapper.IndexOf(entityData.SpawnChainInstance);
        if (prefabData.LastChildSurplusAutoChainTime != entityData.LastChildSurplusAutoChainTime)
            saveData.LastChildSurplusAutoChainTime = entityData.LastChildSurplusAutoChainTime;
        if (prefabData.Initialized != entityData.Initialized)
            saveData.Initialized = entityData.Initialized;

        if (saveData.Equals(default(RandomizedSpawnChain_Save)))
            return null;

        return saveData;
    }

    public override object SaveComponent(Entity entity, EntityMapper entityMapper)
    {
        var data = entity.Read<RandomizedSpawnChain>();

        var saveData = new RandomizedSpawnChain_Save()
        {
            Settings = data.Settings,
            SpawnChainInstance = entityMapper.IndexOf(data.SpawnChainInstance),
            LastChildSurplusAutoChainTime = data.LastChildSurplusAutoChainTime,
            Initialized = data.Initialized
        };

        return saveData;
    }

    public override void ApplyComponentData(Entity entity, JsonElement jsonData, Entity[] entitiesBeingLoaded)
    {
        var saveData = jsonData.Deserialize<RandomizedSpawnChain_Save>(SchematicService.GetJsonOptions());

        if (!entity.Has<RandomizedSpawnChain>())
            entity.Add<RandomizedSpawnChain>();

        var randomizedSpawnChain = entity.Read<RandomizedSpawnChain>();

        if (saveData.Settings.HasValue)
            randomizedSpawnChain.Settings = saveData.Settings.Value;
        if (saveData.SpawnChainInstance.HasValue)
            randomizedSpawnChain.SpawnChainInstance = entitiesBeingLoaded[saveData.SpawnChainInstance.Value];
        randomizedSpawnChain.LastChildSurplusAutoChainTime = saveData.LastChildSurplusAutoChainTime;
        randomizedSpawnChain.Initialized = saveData.Initialized;

        entity.Write(randomizedSpawnChain);
    }

    public override int[] GetDependencies(JsonElement data)
    {
        var saveData = data.Deserialize<RandomizedSpawnChain_Save>(SchematicService.GetJsonOptions());

        return [saveData.SpawnChainInstance.Value];
    }
}
