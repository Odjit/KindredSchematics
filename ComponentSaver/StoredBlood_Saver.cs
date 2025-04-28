using KindredSchematics.Services;
using ProjectM;
using Stunlock.Core;
using System.Text.Json;
using Unity.Entities;

namespace KindredSchematics.ComponentSaver;

[ComponentType(typeof(StoredBlood))]
class StoredBlood_Saver : ComponentSaver
{
    struct StoredBlood_Save
    {
        public float? BloodQuality { get; set; }
        public PrefabGUID? BloodType { get; set; }
        public float? SecondaryBloodQuality { get; set; }
        public PrefabGUID? SecondaryBloodType { get; set; }
    }

    public override object DiffComponents(Entity prefab, Entity entity, EntityMapper entityMapper)
    {
        var prefabData = prefab.Read<StoredBlood>();
        var entityData = entity.Read<StoredBlood>();

        var saveData = new StoredBlood_Save();
        if (prefabData.BloodQuality != entityData.BloodQuality)
            saveData.BloodQuality = entityData.BloodQuality;
        if (prefabData.PrimaryBloodType != entityData.PrimaryBloodType)
            saveData.BloodType = entityData.PrimaryBloodType;
        if (prefabData.SecondaryBlood.Quality != entityData.SecondaryBlood.Quality)
            saveData.SecondaryBloodQuality = entityData.SecondaryBlood.Quality;
        if (prefabData.SecondaryBlood.Type != entityData.SecondaryBlood.Type)
            saveData.SecondaryBloodType = entityData.SecondaryBlood.Type;

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
            BloodType = data.PrimaryBloodType,
            SecondaryBloodQuality = data.SecondaryBlood.Quality,
            SecondaryBloodType = data.SecondaryBlood.Type
        };

        return saveData;
    }

    public override void ApplyComponentData(Entity entity, JsonElement jsonData, Entity[] entitiesBeingLoaded)
    {
        var saveData = jsonData.Deserialize<StoredBlood_Save>(SchematicService.GetJsonOptions());

        if (!entity.Has<StoredBlood>())
            entity.Add<StoredBlood>();

        var data = entity.Read<StoredBlood>();

        if (saveData.BloodQuality.HasValue)
            data.BloodQuality = saveData.BloodQuality.Value;
        if (saveData.BloodType.HasValue)
            data.PrimaryBloodType = saveData.BloodType.Value;
        if (saveData.SecondaryBloodQuality.HasValue)
            data.SecondaryBlood.Quality = saveData.SecondaryBloodQuality.Value;
        if (saveData.SecondaryBloodType.HasValue)
            data.SecondaryBlood.Type = saveData.SecondaryBloodType.Value;

        entity.Write(data);
    }
    
}
