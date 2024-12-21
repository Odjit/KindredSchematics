using KindredSchematics.Services;
using ProjectM;
using System.Text.Json;
using Unity.Entities;

namespace KindredSchematics.ComponentSaver
{
    [ComponentType(typeof(SpawnChainData.ActiveChildElement))]
    internal class SpawnChainData_ActiveChildElement_Saver : ComponentSaver
    {
        struct SpawnChainData_ActiveChildElement_Save
        {
            public int ChainElementIndex { get; set; }
            public int? ActiveEntity { get; set; }
        }

        public override object DiffComponents(Entity prefab, Entity entity, EntityMapper entityMapper)
        {
            var prefabData = prefab.Read<SpawnChainData.ActiveChildElement>();
            var entityData = entity.Read<SpawnChainData.ActiveChildElement>();

            var saveData = new SpawnChainData_ActiveChildElement_Save();

            if (prefabData.ChainElementIndex != entityData.ChainElementIndex)
                saveData.ChainElementIndex = entityData.ChainElementIndex;

            if (prefabData.ActiveEntity != entityData.ActiveEntity)
                saveData.ActiveEntity = entityMapper.IndexOf(entityData.ActiveEntity);

            if (saveData.Equals(default(SpawnChainData_ActiveChildElement_Save)))
                return null;

            return saveData;
        }

        public override object SaveComponent(Entity entity, EntityMapper entityMapper)
        {
            var data = entity.Read<SpawnChainData.ActiveChildElement>();
            var addition = new SpawnChainData_ActiveChildElement_Save();
            addition.ChainElementIndex = data.ChainElementIndex;
            addition.ActiveEntity = entityMapper.IndexOf(data.ActiveEntity);

            return addition;
        }

        public override void ApplyComponentData(Entity entity, JsonElement jsonData, Entity[] entitiesBeingLoaded)
        {
            var save = jsonData.Deserialize<SpawnChainData_ActiveChildElement_Save>(SchematicService.GetJsonOptions());

            if (!entity.Has<SpawnChainData.ActiveChildElement>())
                entity.Add<SpawnChainData.ActiveChildElement>();

            var data = entity.Read<SpawnChainData.ActiveChildElement>();
            data.ChainElementIndex = save.ChainElementIndex;
            data.ActiveEntity = save.ActiveEntity.HasValue ? entitiesBeingLoaded[save.ActiveEntity.Value] : Entity.Null;

            entity.Write(data);
        }

        public override int[] GetDependencies(JsonElement data)
        {
            var save = data.Deserialize<SpawnChainData_ActiveChildElement_Save>(SchematicService.GetJsonOptions());

            if (!save.ActiveEntity.HasValue)
                return [];

            return [save.ActiveEntity.Value];
        }
    }
}
