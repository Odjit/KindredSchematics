using KindredSchematics.Services;
using ProjectM;
using System.Text.Json;
using Unity.Entities;

namespace KindredSchematics.ComponentSaver;
[ComponentType(typeof(Attach))]
class Attach_Saver : ComponentSaver
{
    struct Attach_Save
    {
        public int? Parent { get; set; }
    }

    public override object DiffComponents(Entity prefab, Entity entity, EntityMapper entityMapper)
    {
        var prefabData = prefab.Read<Attach>();
        var entityData = entity.Read<Attach>();

        var saveData = new Attach_Save();
        if (prefabData.Parent != entityData.Parent)
            saveData.Parent = entityMapper.IndexOf(entityData.Parent);

        if (saveData.Equals(default(Attach_Save)))
            return null;

        return saveData;
    }

    public override object SaveComponent(Entity entity, EntityMapper entityMapper)
    {
        var data = entity.Read<Attach>();

        var saveData = new Attach_Save()
        {
            Parent = entityMapper.IndexOf(data.Parent)
        };

        return saveData;
    }

    public override void ApplyComponentData(Entity entity, JsonElement jsonData, Entity[] entitiesBeingLoaded)
    {
        var saveData = jsonData.Deserialize<Attach_Save>(SchematicService.GetJsonOptions());

        if (!saveData.Parent.HasValue)
            return;

        if (!entity.Has<Attach>())
            entity.Add<Attach>();

        entity.Write(new Attach(entitiesBeingLoaded[saveData.Parent.Value]));
    }

    public override int[] GetDependencies(JsonElement data)
    {
        var saveData = data.Deserialize<Attach_Save>(SchematicService.GetJsonOptions());

        return [saveData.Parent.Value];
    }
}
