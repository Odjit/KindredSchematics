using KindredVignettes.Services;
using ProjectM;
using System.Text.Json;
using Unity.Entities;

namespace KindredVignettes.ComponentSaver
{
    [ComponentType(typeof(Restricted_InventoryBuffer))]
    internal class Restricted_InventoryBuffer_Saver : ComponentSaver
    {
        struct Restricted_InventoryBuffer_Save
        {
            public PrefabGUID? RestrictedType { get; set; }
            public long? RestrictedItemCategory { get; set; }
        }

        public override object DiffComponents(Entity prefab, Entity entity, EntityMapper entityMapper)
        {
            return SaveComponent(entity, entityMapper);
        }

        public override object SaveComponent(Entity entity, EntityMapper entityMapper)
        {
            var buffer = Core.EntityManager.GetBuffer<Restricted_InventoryBuffer>(entity);
            var saveData = new Restricted_InventoryBuffer_Save[buffer.Length];

            for(var i=0; i < buffer.Length; ++i)
            {
                var data = buffer[i];
                saveData[i] = new Restricted_InventoryBuffer_Save
                {
                    RestrictedType = data.RestrictedType,
                    RestrictedItemCategory = (int)data.RestrictedItemCategory
                };
            }

            return saveData;
        }

        public override void ApplyComponentData(Entity entity, JsonElement jsonData, Entity[] entitiesBeingLoaded)
        {
            var saveData = jsonData.Deserialize<Restricted_InventoryBuffer_Save[]>(VignetteService.GetJsonOptions());

            if (!entity.Has<Restricted_InventoryBuffer>())
                Core.EntityManager.AddBuffer<Restricted_InventoryBuffer>(entity);
            var buffer = Core.EntityManager.GetBuffer<Restricted_InventoryBuffer>(entity);

            buffer.Clear();

            foreach(var saveEntry in saveData)
            {
                buffer.Add(new Restricted_InventoryBuffer
                {
                    RestrictedType = saveEntry.RestrictedType.HasValue ? saveEntry.RestrictedType.Value : PrefabGUID.Empty,
                    RestrictedItemCategory = saveEntry.RestrictedItemCategory.HasValue ? (ItemCategory)saveEntry.RestrictedItemCategory : ItemCategory.NONE
                });
            }
        }
    }
}
