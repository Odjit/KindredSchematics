using KindredSchematics.Services;
using ProjectM;
using Stunlock.Core;
using System.Linq;
using System.Text.Json;
using Unity.Entities;

namespace KindredSchematics.ComponentSaver
{
    [ComponentType(typeof(InventoryBuffer))]
    internal class InventoryBuffer_Saver : ComponentSaver
    {
        struct InventoryBuffer_Save
        {
            public int ItemEntity { get; set; }
            public PrefabGUID? ItemType { get; set; }
            public int? Amount { get; set; }
            public int? MaxAmountOverride { get; set; }
        }

        public override object DiffComponents(Entity prefab, Entity entity, EntityMapper entityMapper)
        {
            return SaveComponent(entity, entityMapper);
        }

        public override object SaveComponent(Entity entity, EntityMapper entityMapper)
        {
            var inventory = Core.EntityManager.GetBuffer<InventoryBuffer>(entity);
            var items = new InventoryBuffer_Save[inventory.Length];
            for (var i = 0; i < inventory.Length; i++)
            {
                var item = inventory[i];

                // See if this is a broken item from Logistics and fix it here
                if (item.ItemEntity.GetEntityOnServer() != Entity.Null)
                {
                    var itemEntity = item.ItemEntity.GetEntityOnServer();
                    if (itemEntity.Has<InventoryItem>())
                    {
                        var inventoryItem = itemEntity.Read<InventoryItem>();
                        inventoryItem.ContainerEntity = entity;
                        itemEntity.Write(inventoryItem);
                    }
                }

                items[i] = new InventoryBuffer_Save
                {
                    ItemEntity = entityMapper.IndexOf(item.ItemEntity.GetEntityOnServer()),
                    ItemType = item.ItemType.IsEmpty() ? null : item.ItemType,
                    Amount = item.Amount == 0 ? null : item.Amount,
                    MaxAmountOverride = item.MaxAmountOverride == 0 ? null : item.MaxAmountOverride
                };
            }

            return items;
        }

        public override void ApplyComponentData(Entity entity, JsonElement data, Entity[] entitiesBeingLoaded)
        {
            var items = data.Deserialize<InventoryBuffer_Save[]>(SchematicService.GetJsonOptions());

            DynamicBuffer<InventoryBuffer> inventory;
            if (entity.Has<InventoryBuffer>())
                inventory = Core.EntityManager.GetBuffer<InventoryBuffer>(entity);
            else
                inventory = Core.EntityManager.AddBuffer<InventoryBuffer>(entity);
            inventory.Clear();

            foreach (var item in items)
            {
                inventory.Add(new InventoryBuffer
                {
                    ItemEntity = entitiesBeingLoaded[item.ItemEntity],
                    ItemType = item.ItemType.HasValue ? item.ItemType.Value : PrefabGUID.Empty,
                    Amount = item.Amount.HasValue ? item.Amount.Value : 0,
                    MaxAmountOverride = item.MaxAmountOverride.HasValue ? item.MaxAmountOverride.Value : 0
                });
            }
        }

        public override int[] GetDependencies(JsonElement data)
        {
            var saveData = data.Deserialize<InventoryBuffer_Save[]>(SchematicService.GetJsonOptions());
            return saveData.Select(x => x.ItemEntity).Where(x => x != 0).ToArray();
        }
    }
}
