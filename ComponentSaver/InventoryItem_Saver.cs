using KindredVignettes.Services;
using ProjectM;
using System.Text.Json;
using Unity.Entities;

namespace KindredVignettes.ComponentSaver;

[ComponentType(typeof(InventoryItem))]
class InventoryItem_Saver : ComponentSaver
{
    struct InventoryItem_Save
    {
        public int? ContainerEntity { get; set; }
    }

    public override object DiffComponents(Entity prefab, Entity entity, EntityMapper entityMapper)
    {
        var prefabData = prefab.Read<InventoryItem>();
        var entityData = entity.Read<InventoryItem>();

        var saveData = new InventoryItem_Save();
        if (prefabData.ContainerEntity != entityData.ContainerEntity)
            saveData.ContainerEntity = entityMapper.IndexOf(entityData.ContainerEntity);

        if (saveData.Equals(default(InventoryItem_Save)))
            return null;

        return saveData;
    }

    public override object SaveComponent(Entity entity, EntityMapper entityMapper)
    {
        var data = entity.Read<InventoryItem>();

        var saveData = new InventoryItem_Save()
        {
            ContainerEntity = entityMapper.IndexOf(data.ContainerEntity)
        };

        return saveData;
    }

    public override void ApplyComponentData(Entity entity, JsonElement jsonData, Entity[] entitiesBeingLoaded)
    {
        var saveData = jsonData.Deserialize<InventoryItem_Save>(VignetteService.GetJsonOptions());

        if (!saveData.ContainerEntity.HasValue)
            return;

        if (!entity.Has<InventoryItem>())
            entity.Add<InventoryItem>();

        entity.Write(new InventoryItem()
        {
            ContainerEntity = entitiesBeingLoaded[saveData.ContainerEntity.Value]
        });
    }

    public override int[] GetDependencies(JsonElement data)
    {
        var saveData = data.Deserialize<InventoryItem_Save>(VignetteService.GetJsonOptions());

        if (!saveData.ContainerEntity.HasValue)
            return [];

        return [saveData.ContainerEntity.Value];
    }
}
