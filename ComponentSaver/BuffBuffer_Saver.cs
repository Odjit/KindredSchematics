using KindredSchematics.Data;
using KindredSchematics.Services;
using ProjectM;
using Stunlock.Core;
using System.Collections.Generic;
using System.Text.Json;
using Unity.Entities;

namespace KindredSchematics.ComponentSaver;

[ComponentType(typeof(BuffBuffer))]
internal class BuffBuffer_Saver : ComponentSaver
{
    public override object DiffComponents(Entity prefab, Entity entity, EntityMapper entityMapper)
    {
        return SaveComponent(entity, entityMapper);
    }

    public override object SaveComponent(Entity entity, EntityMapper entityMapper)
    {
        var data = Core.EntityManager.GetBufferReadOnly<BuffBuffer>(entity);
        var glows = new List<PrefabGUID>();

        for (int i = 0; i < data.Length; i++)
        {
            var prefabGuid = data[i].PrefabGuid;
            var buffEntity = data[i].Entity;
            if (prefabGuid.IsEmpty()) continue;
            if (buffEntity == Entity.Null || !buffEntity.Has<HideOutsideVision>()) continue;
        
            glows.Add(prefabGuid);
        }

        if (glows.Count == 0)
            return null;
        return glows.ToArray();
    }

    public override void ApplyComponentData(Entity entity, JsonElement jsonData, Entity[] entitiesBeingLoaded)
    {
        var saveData = jsonData.Deserialize<PrefabGUID[]>(SchematicService.GetJsonOptions());

        foreach(var glowPrefab in saveData)
        {
            Core.GlowService.AddGlow(entity, entity, glowPrefab);
        }
        
    }
}
