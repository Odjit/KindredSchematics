using KindredSchematics.Services;
using ProjectM;
using System.Text.Json;
using Unity.Entities;

namespace KindredSchematics.ComponentSaver;

[ComponentType(typeof(InventoryConnection))]
class InventoryConnection_Saver : ComponentSaver
{
    struct InventoryConnectionSave
    {
        public int? InventoryOwner { get; set; }
    }

    public override object DiffComponents(Entity prefab, Entity entity, EntityMapper entityMapper)
    {
        var prefabData = prefab.Read<InventoryConnection>();
        var entityData = entity.Read<InventoryConnection>();

        var saveData = new InventoryConnectionSave();
        if (prefabData.InventoryOwner != entityData.InventoryOwner)
            saveData.InventoryOwner = entityMapper.IndexOf(entityData.InventoryOwner);

        if (saveData.Equals(default(InventoryConnectionSave)))
            return null;

        return saveData;
    }

    public override object SaveComponent(Entity entity, EntityMapper entityMapper)
    {
        var data = entity.Read<InventoryConnection>();

        var saveData = new InventoryConnectionSave();

        saveData.InventoryOwner = entityMapper.IndexOf(data.InventoryOwner);

        return saveData;
    }

    public override void ApplyComponentData(Entity entity, JsonElement jsonData, Entity[] entitiesBeingLoaded)
    {
        var saveData = jsonData.Deserialize<InventoryConnectionSave>(SchematicService.GetJsonOptions());

        if (!saveData.InventoryOwner.HasValue)
            return;

        if (!entity.Has<InventoryConnection>())
            entity.Add<InventoryConnection>();

        entity.Write(new InventoryConnection()
        {
            InventoryOwner = entitiesBeingLoaded[saveData.InventoryOwner.Value]
        });
    }

    public override int[] GetDependencies(JsonElement data)
    {
        var saveData = data.Deserialize<InventoryConnectionSave>(SchematicService.GetJsonOptions());
        if (!saveData.InventoryOwner.HasValue)
            return [];
        return [saveData.InventoryOwner.Value];
    }
}
