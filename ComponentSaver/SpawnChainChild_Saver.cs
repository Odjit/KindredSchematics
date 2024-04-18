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

        public override object DiffComponents(Entity src, Entity dst, EntityMapper entityMapper)
        {
            var srcData = src.Read<SpawnChainChild>();
            var dstData = dst.Read<SpawnChainChild>();

            var diff = new SpawnChainChild_Save();
            
            if (srcData.SpawnChain != dstData.SpawnChain)
                diff.SpawnChain = entityMapper.IndexOf(dstData.SpawnChain);

            if (srcData.SpawnChainElementIndex != dstData.SpawnChainElementIndex)
                diff.SpawnChainElementIndex = dstData.SpawnChainElementIndex;

            if (diff.Equals(default(SpawnChainChild_Save)))
                return null;

            return diff;
        }

        public override void ApplyDiff(Entity entity, JsonElement diff, Entity[] entitiesBeingLoaded)
        {
            var spawnChainChild = diff.Deserialize<SpawnChainChild_Save>(VignetteService.GetJsonOptions());
            var data = entity.Read<SpawnChainChild>();

            if (spawnChainChild.SpawnChain.HasValue)
                data.SpawnChain = entitiesBeingLoaded[spawnChainChild.SpawnChain.Value];

            if (spawnChainChild.SpawnChainElementIndex.HasValue)
                data.SpawnChainElementIndex = spawnChainChild.SpawnChainElementIndex.Value;

            entity.Write(data);
        }

        public override object SaveComponent(Entity entity, EntityMapper entityMapper)
        {
            var data = entity.Read<SpawnChainChild>();
            var addition = new SpawnChainChild_Save();
            addition.SpawnChain = entityMapper.IndexOf(data.SpawnChain);
            addition.SpawnChainElementIndex = data.SpawnChainElementIndex;

            return addition;
        }

        public override void AddComponent(Entity entity, JsonElement data, Entity[] entitiesBeingLoaded)
        {
            var spawnChainChildSave = data.Deserialize<SpawnChainChild_Save>(VignetteService.GetJsonOptions());
            entity.Add<SpawnChainChild>();
            entity.Write(new SpawnChainChild
            {
                SpawnChain = entitiesBeingLoaded[spawnChainChildSave.SpawnChain.Value],
                SpawnChainElementIndex = spawnChainChildSave.SpawnChainElementIndex.Value
            });

        }
    }
}
