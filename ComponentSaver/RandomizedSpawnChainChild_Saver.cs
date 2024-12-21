using KindredSchematics.Services;
using ProjectM;
using System.Text.Json;
using Unity.Entities;

namespace KindredSchematics.ComponentSaver;
[ComponentType(typeof(RandomizedSpawnChainChild))]
class RandomizedSpawnChainChild_Saver : ComponentSaver
{
    struct RandomizedSpawnChainChild_Save
    {
        public int? Parent { get; set; }
    }

    public override object DiffComponents(Entity prefab, Entity entity, EntityMapper entityMapper)
    {
        var prefabData = prefab.Read<RandomizedSpawnChainChild>();
        var entityData = entity.Read<RandomizedSpawnChainChild>();

        var saveData = new RandomizedSpawnChainChild_Save();
        if (prefabData.Parent != entityData.Parent)
            saveData.Parent = entityMapper.IndexOf(entityData.Parent);

        if (saveData.Equals(default(RandomizedSpawnChainChild_Save)))
            return null;

        return saveData;
    }

    public override object SaveComponent(Entity entity, EntityMapper entityMapper)
    {
        var data = entity.Read<RandomizedSpawnChainChild>();

        var saveData = new RandomizedSpawnChainChild_Save()
        {
            Parent = entityMapper.IndexOf(data.Parent)
        };

        return saveData;
    }

    public override void ApplyComponentData(Entity entity, JsonElement jsonData, Entity[] entitiesBeingLoaded)
    {
        var saveData = jsonData.Deserialize<RandomizedSpawnChainChild_Save>(SchematicService.GetJsonOptions());

        if (!saveData.Parent.HasValue)
            return;

        if (!entity.Has<RandomizedSpawnChainChild>())
            entity.Add<RandomizedSpawnChainChild>();

        entity.Write(new RandomizedSpawnChainChild(){
            Parent = entitiesBeingLoaded[saveData.Parent.Value]
        });
    }

    public override int[] GetDependencies(JsonElement data)
    {
        var saveData = data.Deserialize<RandomizedSpawnChainChild_Save>(SchematicService.GetJsonOptions());

        return [saveData.Parent.Value];
    }
}
