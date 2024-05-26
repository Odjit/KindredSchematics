using KindredVignettes.Services;
using ProjectM;
using Stunlock.Core;
using System.Text.Json;
using Unity.Entities;

namespace KindredVignettes.ComponentSaver
{
    [ComponentType(typeof(PerksBuffer))]
    internal class PerksBuffer_Saver : ComponentSaver
    {
        struct PerksBuffer_Save
        {
            public PrefabGUID Perk { get; set; }
        }

        public override object DiffComponents(Entity prefab, Entity entity, EntityMapper entityMapper)
        {
            return SaveComponent(entity, entityMapper);
        }

        public override object SaveComponent(Entity entity, EntityMapper entityMapper)
        {
            var data = Core.EntityManager.GetBufferReadOnly<PerksBuffer>(entity);

            var saveData = new PerksBuffer_Save[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                saveData[i].Perk = data[i].Perk;
            }

            return saveData;
        }

        public override void ApplyComponentData(Entity entity, JsonElement jsonData, Entity[] entitiesBeingLoaded)
        {
            var saveData = jsonData.Deserialize<PerksBuffer_Save[]>(VignetteService.GetJsonOptions());
            
            if(!entity.Has<PerksBuffer>())
                Core.EntityManager.AddBuffer<PerksBuffer>(entity);

            var data = Core.EntityManager.GetBuffer<PerksBuffer>(entity);

            foreach (var entry in saveData)
            {
                data.Add(new PerksBuffer
                {
                    Perk = entry.Perk
                });
            }
        }
    }
}
