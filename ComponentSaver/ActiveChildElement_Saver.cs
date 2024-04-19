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

        public override object DiffComponents(Entity src, Entity dst, EntityMapper entityMapper)
        {
            var srcData = src.Read<ActiveChildElement>();
            var dstData = dst.Read<ActiveChildElement>();

            var diff = new ActiveChildElement_Save();

            if ( srcData.ChainElementIndex != dstData.ChainElementIndex)
                diff.ChainElementIndex = dstData.ChainElementIndex;

            if (srcData.ActiveEntity != dstData.ActiveEntity)
                diff.ActiveEntityId = entityMapper.IndexOf(dstData.ActiveEntity);

            if (diff.Equals(default(ActiveChildElement_Save)))
                return null;

            return diff;
        }

        public override object SaveComponent(Entity entity, EntityMapper entityMapper)
        {
            throw new System.NotImplementedException();
        }

        public override void ApplyComponentData(Entity entity, JsonElement jsonData, Entity[] entitiesBeingLoaded)
        {
            var activeChildElement = jsonData.Deserialize<ActiveChildElement_Save>(VignetteService.GetJsonOptions());

            if (!entity.Has<ActiveChildElement>())
                entity.Add<ActiveChildElement>();

            var data = entity.Read<ActiveChildElement>();
            if (activeChildElement.ChainElementIndex.HasValue)
                data.ChainElementIndex = activeChildElement.ChainElementIndex.Value;
            if (activeChildElement.ActiveEntityId.HasValue)
                data.ActiveEntity = entitiesBeingLoaded[activeChildElement.ActiveEntityId.Value];
            entity.Write(data);
        }
    }
}
