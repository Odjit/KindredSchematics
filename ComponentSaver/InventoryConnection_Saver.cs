using KindredVignettes.Services;
using ProjectM;
using ProjectM.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Unity.Entities;

namespace KindredVignettes.ComponentSaver;

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
            saveData.InventoryOwner = entityMapper.AddEntity(entityData.InventoryOwner);

        if (saveData.Equals(default(InventoryConnectionSave)))
            return null;

        return saveData;
    }

    public override object SaveComponent(Entity entity, EntityMapper entityMapper)
    {
        var data = entity.Read<InventoryConnection>();

        var saveData = new InventoryConnectionSave();

        saveData.InventoryOwner = entityMapper.AddEntity(data.InventoryOwner);

        return saveData;
    }

    public override void ApplyComponentData(Entity entity, JsonElement jsonData, Entity[] entitiesBeingLoaded)
    {
        var saveData = jsonData.Deserialize<InventoryConnectionSave>(VignetteService.GetJsonOptions());

        if (!saveData.InventoryOwner.HasValue)
            return;

        if (!entity.Has<InventoryConnection>())
            entity.Add<InventoryConnection>();

        entity.Write(new InventoryConnection()
        {
            InventoryOwner = entitiesBeingLoaded[saveData.InventoryOwner.Value]
        });
    }
}
