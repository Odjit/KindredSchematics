using KindredVignettes.Services;
using ProjectM;
using System.Text.Json;
using Unity.Entities;

namespace KindredVignettes.ComponentSaver
{
    [ComponentType(typeof(ServantConnectedCoffin))]
    internal class ServantConnectedCoffin_Saver : ComponentSaver
    {
        struct ServantConnectedCoffin_Save
        {
            public int? CoffinEntity { get; set; }
        }

        public override object DiffComponents(Entity prefab, Entity entity, EntityMapper entityMapper)
        {
            return SaveComponent(entity, entityMapper);
        }

        public override object SaveComponent(Entity entity, EntityMapper entityMapper)
        {
            var data = entity.Read<ServantConnectedCoffin>();

            var saveData = new ServantConnectedCoffin_Save();
            saveData.CoffinEntity = entityMapper.IndexOf(data.CoffinEntity.GetEntityOnServer());

            return saveData;
        }

        public override void ApplyComponentData(Entity entity, JsonElement jsonData, Entity[] entitiesBeingLoaded)
        {
            var saveData = jsonData.Deserialize<ServantConnectedCoffin_Save>(VignetteService.GetJsonOptions());

            if (!entity.Has<ServantConnectedCoffin>())
                entity.Add<ServantConnectedCoffin>();

            entity.Write(new ServantConnectedCoffin()
            {
                CoffinEntity = entitiesBeingLoaded[saveData.CoffinEntity.Value]
            });
        }

        public override int[] GetDependencies(JsonElement data)
        {
            var saveData = data.Deserialize<ServantConnectedCoffin_Save>(VignetteService.GetJsonOptions());

            return [saveData.CoffinEntity.Value];
        }
    }
}
