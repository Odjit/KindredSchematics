using KindredVignettes.Services;
using System.Text.Json;
using Unity.Entities;
using static ProjectM.SpawnChainData;

namespace KindredVignettes.ComponentSaver
{
    [ComponentType(typeof(ActiveChildElement))]
    internal class ActiveChildElement_Saver : ComponentSaver
    {
        struct ActiveChildElement_Save
        {
            public int? ChainElementIndex { get; set; }
            public int? ActiveEntityId { get; set; }
        }

        public override object DiffComponents(Entity prefab, Entity entity, EntityMapper entityMapper)
        {
            var prefabData = prefab.Read<ActiveChildElement>();
            var entityData = entity.Read<ActiveChildElement>();

            var saveData = new ActiveChildElement_Save();

            if ( prefabData.ChainElementIndex != entityData.ChainElementIndex)
                saveData.ChainElementIndex = entityData.ChainElementIndex;

            if (prefabData.ActiveEntity != entityData.ActiveEntity)
                saveData.ActiveEntityId = entityMapper.IndexOf(entityData.ActiveEntity);

            if (saveData.Equals(default(ActiveChildElement_Save)))
                return null;

            return saveData;
        }

        public override object SaveComponent(Entity entity, EntityMapper entityMapper)
        {
            throw new System.NotImplementedException();
        }

        public override void ApplyComponentData(Entity entity, JsonElement jsonData, Entity[] entitiesBeingLoaded)
        {
            var saveData = jsonData.Deserialize<ActiveChildElement_Save>(VignetteService.GetJsonOptions());

            if (!entity.Has<ActiveChildElement>())
                entity.Add<ActiveChildElement>();

            var data = entity.Read<ActiveChildElement>();
            if (saveData.ChainElementIndex.HasValue)
                data.ChainElementIndex = saveData.ChainElementIndex.Value;
            if (saveData.ActiveEntityId.HasValue)
                data.ActiveEntity = entitiesBeingLoaded[saveData.ActiveEntityId.Value];
            entity.Write(data);
        }
    }
}
