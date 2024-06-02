using KindredVignettes.Services;
using ProjectM;
using System.Text.Json;
using Unity.Entities;

namespace KindredVignettes.ComponentSaver
{
    [ComponentType(typeof(SpawnChainChild))]
    internal class SpawnChainChild_Saver : ComponentSaver
    {
        struct SpawnChainChild_Save
        {
            public int? SpawnChain { get; set; }
            public int? SpawnChainElementIndex { get; set; }
        }

        public override object DiffComponents(Entity prefab, Entity entity, EntityMapper entityMapper)
        {
            var prefabData = prefab.Read<SpawnChainChild>();
            var entityData = entity.Read<SpawnChainChild>();

            var saveData = new SpawnChainChild_Save();
            
            if (prefabData.SpawnChain != entityData.SpawnChain)
                saveData.SpawnChain = entityMapper.IndexOf(entityData.SpawnChain);

            if (prefabData.SpawnChainElementIndex != entityData.SpawnChainElementIndex)
                saveData.SpawnChainElementIndex = entityData.SpawnChainElementIndex;

            if (saveData.Equals(default(SpawnChainChild_Save)))
                return null;

            return saveData;
        }

        public override object SaveComponent(Entity entity, EntityMapper entityMapper)
        {
            var data = entity.Read<SpawnChainChild>();
            var addition = new SpawnChainChild_Save();
            addition.SpawnChain = entityMapper.IndexOf(data.SpawnChain);
            addition.SpawnChainElementIndex = data.SpawnChainElementIndex;

            return addition;
        }

        public override void ApplyComponentData(Entity entity, JsonElement jsonData, Entity[] entitiesBeingLoaded)
        {
            var spawnChainChildSave = jsonData.Deserialize<SpawnChainChild_Save>(VignetteService.GetJsonOptions());

            if (!entity.Has<SpawnChainChild>())
                entity.Add<SpawnChainChild>();

            var data = entity.Read<SpawnChainChild>();

            if (spawnChainChildSave.SpawnChain.HasValue)
                data.SpawnChain = entitiesBeingLoaded[spawnChainChildSave.SpawnChain.Value];

            if (spawnChainChildSave.SpawnChainElementIndex.HasValue)
                data.SpawnChainElementIndex = spawnChainChildSave.SpawnChainElementIndex.Value;

            entity.Write(data);
        }

        public override int[] GetDependencies(JsonElement data)
        {
            var spawnChainChildSave = data.Deserialize<SpawnChainChild_Save>(VignetteService.GetJsonOptions());

            if (!spawnChainChildSave.SpawnChain.HasValue)
                return [];

            return [spawnChainChildSave.SpawnChain.Value];
        }
    }
}
