using KindredVignettes.Services;
using ProjectM;
using Stunlock.Core;
using System.Text.Json;
using Unity.Entities;

namespace KindredVignettes.ComponentSaver;

[ComponentType(typeof(StoredBlood))]
class StoredBlood_Saver : ComponentSaver
{
    struct StoredBlood_Save
    {
        public float? BloodQuality { get; set; }
        public PrefabGUID? BloodType { get; set; }
    }

    public override object DiffComponents(Entity prefab, Entity entity, EntityMapper entityMapper)
    {
        var prefabData = prefab.Read<StoredBlood>();
        var entityData = entity.Read<StoredBlood>();

        var saveData = new StoredBlood_Save();
        if (prefabData.BloodQuality != entityData.BloodQuality)
            saveData.BloodQuality = entityData.BloodQuality;
        if (prefabData.BloodType != entityData.BloodType)
            saveData.BloodType = entityData.BloodType;

        if (saveData.Equals(default(StoredBlood_Save)))
            return null;

        return saveData;
    }

    public override object SaveComponent(Entity entity, EntityMapper entityMapper)
    {
        var data = entity.Read<StoredBlood>();

        var saveData = new StoredBlood_Save
        {
            BloodQuality = data.BloodQuality,
            BloodType = data.BloodType
        };

        return saveData;
    }

    public override void ApplyComponentData(Entity entity, JsonElement jsonData, Entity[] entitiesBeingLoaded)
    {
        var saveData = jsonData.Deserialize<StoredBlood_Save>(VignetteService.GetJsonOptions());

        if (!entity.Has<StoredBlood>())
            entity.Add<StoredBlood>();

        var data = entity.Read<StoredBlood>();

        if (saveData.BloodQuality.HasValue)
            data.BloodQuality = saveData.BloodQuality.Value;
        if (saveData.BloodType.HasValue)
            data.BloodType = saveData.BloodType.Value;

        entity.Write(data);
    }
    
}
