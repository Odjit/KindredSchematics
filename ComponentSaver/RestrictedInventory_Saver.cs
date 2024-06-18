using KindredSchematics.Services;
using ProjectM;
using Stunlock.Core;
using System.Text.Json;
using Unity.Entities;

namespace KindredSchematics.ComponentSaver;
[ComponentType(typeof(RestrictedInventory))]
class RestrictedInventory_Saver : ComponentSaver
{
    struct RestrictedInventory_Save
    {
        public PrefabGUID? RestrictedItemType { get; set; }
        public int? RestrictedItemCategory { get; set; }
    }

    public override object DiffComponents(Entity prefab, Entity entity, EntityMapper entityMapper)
    {
        var prefabData = prefab.Read<RestrictedInventory>();
        var entityData = entity.Read<RestrictedInventory>();

        var saveData = new RestrictedInventory_Save();
        if (prefabData.RestrictedItemType != entityData.RestrictedItemType)
            saveData.RestrictedItemType = entityData.RestrictedItemType;
        if (prefabData.RestrictedItemCategory != entityData.RestrictedItemCategory)
            saveData.RestrictedItemCategory = (int)entityData.RestrictedItemCategory;

        if (saveData.Equals(default(RestrictedInventory_Save)))
            return null;

        return saveData;
    }

    public override object SaveComponent(Entity entity, EntityMapper entityMapper)
    {
        var data = entity.Read<RestrictedInventory>();

        var saveData = new RestrictedInventory_Save()
        {
            RestrictedItemType = data.RestrictedItemType,
            RestrictedItemCategory = (int)data.RestrictedItemCategory
        };

        return saveData;
    }

    public override void ApplyComponentData(Entity entity, JsonElement jsonData, Entity[] entitiesBeingLoaded)
    {
        var saveData = jsonData.Deserialize<RestrictedInventory_Save>(SchematicService.GetJsonOptions());

        if (!entity.Has<RestrictedInventory>())
            entity.Add<RestrictedInventory>();

        var data = entity.Read<RestrictedInventory>();

        if(saveData.RestrictedItemType.HasValue)
            data.RestrictedItemType = saveData.RestrictedItemType.Value;
        if(saveData.RestrictedItemCategory.HasValue)
            data.RestrictedItemCategory = (ItemCategory)saveData.RestrictedItemCategory.Value;

        entity.Write(data);
    }
}
