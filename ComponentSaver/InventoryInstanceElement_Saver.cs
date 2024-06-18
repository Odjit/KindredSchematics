using KindredSchematics.Services;
using ProjectM;
using Stunlock.Core;
using System.Linq;
using System.Text.Json;
using Unity.Entities;
using static ProjectM.InventoryInstanceElement;

namespace KindredSchematics.ComponentSaver
{
    [ComponentType(typeof(InventoryInstanceElement))]
    internal class InventoryInstanceElement_Saver : ComponentSaver
    {
        struct InventoryInstanceElementSave
        {
            public int Category { get; set; }
            public int Slots { get; set; }
            public int MaxSlots { get; set; }
            public PrefabGUID ExternalInventoryEntityPrefabGuid { get; set; }
            public int ExternalInventoryEntity { get; set; }
            public PrefabGUID RestrictedType { get; set; }
            public long RestrictedCategory { get; set; }
        }

        public override object DiffComponents(Entity prefab, Entity entity, EntityMapper entityMapper)
        {
            return SaveComponent(entity, entityMapper);
        }

        public override object SaveComponent(Entity entity, EntityMapper entityMapper)
        {
            var inventoryInstances = Core.EntityManager.GetBuffer<InventoryInstanceElement>(entity);
            var saveBuffer = new InventoryInstanceElementSave[inventoryInstances.Length];
            for (var i = 0; i < inventoryInstances.Length; i++)
            {
                var instance = inventoryInstances[i];
                saveBuffer[i] = new InventoryInstanceElementSave
                {
                    Category = (int)instance.Category,
                    Slots = instance.Slots,
                    MaxSlots = instance.MaxSlots,
                    ExternalInventoryEntityPrefabGuid = instance.ExternalInventoryEntityPrefabGuid,
                    ExternalInventoryEntity = entityMapper.IndexOf(instance.ExternalInventoryEntity.GetEntityOnServer()),
                    RestrictedType = instance.RestrictedType,
                    RestrictedCategory = instance.RestrictedCategory,
                };
            }

            return saveBuffer;
        }

        public override void ApplyComponentData(Entity entity, JsonElement data, Entity[] entitiesBeingLoaded)
        {
            var inventoryData = data.Deserialize<InventoryInstanceElementSave[]>(SchematicService.GetJsonOptions());

            DynamicBuffer<InventoryInstanceElement> inventoryInstances;
            if (entity.Has<InventoryInstanceElement>())
                inventoryInstances = Core.EntityManager.GetBuffer<InventoryInstanceElement>(entity);
            else
                inventoryInstances = Core.EntityManager.AddBuffer<InventoryInstanceElement>(entity);
            inventoryInstances.Clear();

            foreach (var inventory in inventoryData)
            {
                inventoryInstances.Add(new InventoryInstanceElement
                {
                    Category = (InstanceCategory)inventory.Category,
                    Slots = inventory.Slots,
                    MaxSlots = inventory.MaxSlots,
                    ExternalInventoryEntityPrefabGuid = inventory.ExternalInventoryEntityPrefabGuid,
                    ExternalInventoryEntity = entitiesBeingLoaded[inventory.ExternalInventoryEntity],
                    RestrictedType = inventory.RestrictedType,
                    RestrictedCategory = inventory.RestrictedCategory,
                });
            }
        }

        public override int[] GetDependencies(JsonElement data)
        {
            var saveData = data.Deserialize<InventoryInstanceElementSave[]>(SchematicService.GetJsonOptions());
            return saveData.Select(x => x.ExternalInventoryEntity).ToArray();
        }
    }
}
