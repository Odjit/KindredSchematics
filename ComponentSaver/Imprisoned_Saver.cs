using KindredVignettes.Services;
using ProjectM;
using System.Text.Json;
using Unity.Entities;

namespace KindredVignettes.ComponentSaver
{
    [ComponentType(typeof(Imprisoned))]
    internal class Imprisoned_Saver : ComponentSaver
    {
        struct Imprisoned_Save
        {
            public int? PrisonCellEntity { get; set; }
        }

        public override object DiffComponents(Entity prefab, Entity entity, EntityMapper entityMapper)
        {
            var prefabData = prefab.Read<Imprisoned>();
            var entityData = entity.Read<Imprisoned>();

            var saveData = new Imprisoned_Save();
            if (prefabData.PrisonCellEntity != entityData.PrisonCellEntity)
                saveData.PrisonCellEntity = entityMapper.IndexOf(entityData.PrisonCellEntity);

            if (saveData.Equals(default(Imprisoned_Save)))
                return null;

            return saveData;
        }

        public override object SaveComponent(Entity entity, EntityMapper entityMapper)
        {
            var data = entity.Read<Imprisoned>();

            var saveData = new Imprisoned_Save();
            saveData.PrisonCellEntity = entityMapper.IndexOf(data.PrisonCellEntity);

            return saveData;
        }

        public override void ApplyComponentData(Entity entity, JsonElement jsonData, Entity[] entitiesBeingLoaded)
        {
            var saveData = jsonData.Deserialize<Imprisoned_Save>(VignetteService.GetJsonOptions());

            if (!entity.Has<Imprisoned>())
                entity.Add<Imprisoned>();

            var data = entity.Read<Imprisoned>();

            data.PrisonCellEntity = entitiesBeingLoaded[saveData.PrisonCellEntity.Value];

            entity.Write(data);
        }
    }
}
