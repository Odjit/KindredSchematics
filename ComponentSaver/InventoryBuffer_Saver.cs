using KindredVignettes.Services;
using ProjectM;
using System.Text.Json;
using Unity.Entities;

namespace KindredVignettes.ComponentSaver
{
    [ComponentType(typeof(InventoryBuffer))]
    internal class InventoryBuffer_Saver : ComponentSaver
    {
        struct InventoryItem
        {
            public int ItemEntity { get; set; }
            public PrefabGUID? ItemType { get; set; }
            public int? Amount { get; set; }
            public int? MaxAmountOverride { get; set; }
        }

        public override object DiffComponents(Entity src, Entity dst, EntityMapper entityMapper)
        {
            return SaveComponent(dst, entityMapper);
        }

        public override void ApplyDiff(Entity entity, JsonElement diff, Entity[] entitiesBeingLoaded)
        {
            AddComponent(entity, diff, entitiesBeingLoaded);
        }

        public override object SaveComponent(Entity entity, EntityMapper entityMapper)
        {
            var inventory = Core.EntityManager.GetBuffer<InventoryBuffer>(entity);
            var items = new InventoryItem[inventory.Length];
            for (var i = 0; i < inventory.Length; i++)
            {
                var item = inventory[i];
                items[i] = new InventoryItem
                {
                    ItemEntity = entityMapper.IndexOf(item.ItemEntity.GetEntityOnServer()),
                    ItemType = item.ItemType.IsEmpty() ? null : item.ItemType,
                    Amount = item.Amount == 0 ? null : item.Amount,
                    MaxAmountOverride = item.MaxAmountOverride == 0 ? null : item.MaxAmountOverride
                };
            }

            return items;
        }

        public override void AddComponent(Entity entity, JsonElement data, Entity[] entitiesBeingLoaded)
        {
            var items = data.Deserialize<InventoryItem[]>(VignetteService.GetJsonOptions());

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
    }
}
