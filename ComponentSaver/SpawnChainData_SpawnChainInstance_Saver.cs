 using KindredSchematics.Services;
using ProjectM;
using System.Text.Json;
using Unity.Entities;

namespace KindredSchematics.ComponentSaver
{
    [ComponentType(typeof(SpawnChainData.SpawnChainInstance))]
    internal class SpawnChainData_SpawnChainInstance_Saver : ComponentSaver
    {
        struct SpawnChainData_SpawnChainInstance_Save
        {
            public bool LoopOnEndOfChain { get; set; }
        }

        public override object DiffComponents(Entity prefab, Entity entity, EntityMapper entityMapper)
        {
            var prefabData = prefab.Read<SpawnChainData.SpawnChainInstance>();
            var entityData = entity.Read<SpawnChainData.SpawnChainInstance>();

            var saveData = new SpawnChainData_SpawnChainInstance_Save();
            
            if (prefabData.LoopOnEndOfChain != entityData.LoopOnEndOfChain)
                saveData.LoopOnEndOfChain = entityData.LoopOnEndOfChain;

            if (saveData.Equals(default(SpawnChainData_SpawnChainInstance_Saver)))
                return null;

            return saveData;
        }

        public override object SaveComponent(Entity entity, EntityMapper entityMapper)
        {
            var data = entity.Read<SpawnChainData.SpawnChainInstance>();
            var addition = new SpawnChainData_SpawnChainInstance_Save();
            
            addition.LoopOnEndOfChain = data.LoopOnEndOfChain;

            return addition;
        }

        public override void ApplyComponentData(Entity entity, JsonElement jsonData, Entity[] entitiesBeingLoaded)
        {
            var save = jsonData.Deserialize<SpawnChainData_SpawnChainInstance_Save>(SchematicService.GetJsonOptions());

            if (!entity.Has<SpawnChainData.SpawnChainInstance>())
                entity.Add<SpawnChainData.SpawnChainInstance>();

            var data = entity.Read<SpawnChainData.SpawnChainInstance>();
            data.LoopOnEndOfChain = save.LoopOnEndOfChain;

            entity.Write(data);
        }
    }
}
