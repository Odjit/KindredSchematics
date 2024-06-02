using ProjectM;
using System.Text.Json;
using Unity.Entities;

namespace KindredVignettes.ComponentSaver;

[ComponentType(typeof(AttachedBuffer))]
internal class AttachedBuffer_Saver : ComponentSaver
{
    public override object DiffComponents(Entity prefab, Entity entity, EntityMapper entityMapper)
    {
        return SaveComponent(entity, entityMapper);
    }

    public override object SaveComponent(Entity entity, EntityMapper entityMapper)
    {
        var data = Core.EntityManager.GetBufferReadOnly<AttachedBuffer>(entity);

        for (int i = 0; i < data.Length; i++)
        {
            var prefabGuid = data[i].PrefabGuid;
            if (prefabGuid.IsEmpty())
                continue;

            if (entity.Has<AttachMapIconsToEntity>())
            {
                var mapIcons = Core.EntityManager.GetBufferReadOnly<AttachMapIconsToEntity>(entity);
                var foundMapIcon = false;
                foreach (var mapIcon in mapIcons)
                {
                    if (mapIcon.Prefab == prefabGuid)
                    {
                        foundMapIcon = true;
                        break;
                    }
                }
                if (foundMapIcon)
                    continue;
            }

            entityMapper.IndexOf(data[i].Entity);
        }

        return null;
    }

    public override void ApplyComponentData(Entity entity, JsonElement jsonData, Entity[] entitiesBeingLoaded)
    {
        throw new System.NotImplementedException();
    }
}
