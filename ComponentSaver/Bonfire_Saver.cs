using KindredSchematics.Services;
using ProjectM;
using Stunlock.Core;
using System.Text.Json;
using Unity.Entities;

namespace KindredSchematics.ComponentSaver;

[ComponentType(typeof(Bonfire))]
internal class Bonfire_Saver : ComponentSaver
{
    struct Bonfire_Save
    {
        public int? ActiveSequenceGuid { get; set; }
        public int? ActiveSequenceState { get; set; }
        public PrefabGUID? InputItem { get; set; }
        public float? Strength { get; set; }
        public float? BurnTime { get; set; }
        public float? TimeToGetToFullStrength { get; set; }
        public float? TimeToGetToZeroStrength { get; set; }
        public float? StartScale { get; set; }
        public float? EndScale { get; set; }
        public bool? IsActive { get; set; }
    }

    public override object DiffComponents(Entity prefab, Entity entity, EntityMapper entityMapper)
    {
        var prefabData = prefab.Read<Bonfire>();
        var entityData = entity.Read<Bonfire>();

        var saveData = new Bonfire_Save();
        if(prefabData.ActiveSequenceGuid != entityData.ActiveSequenceGuid)
            saveData.ActiveSequenceGuid = entityData.ActiveSequenceGuid.GuidHash;
        if(!prefabData.ActiveSequenceState.Equals(entityData.ActiveSequenceState))
            saveData.ActiveSequenceState = entityMapper.IndexOf(entityData.ActiveSequenceState.Id);
        if(prefabData.InputItem != entityData.InputItem)
            saveData.InputItem = entityData.InputItem;
        if(prefabData.Strength != entityData.Strength)
            saveData.Strength = entityData.Strength;
        if(prefabData.BurnTime != entityData.BurnTime)
            saveData.BurnTime = entityData.BurnTime;
        if(prefabData.TimeToGetToFullStrength != entityData.TimeToGetToFullStrength)
            saveData.TimeToGetToFullStrength = entityData.TimeToGetToFullStrength;
        if(prefabData.TimeToGetToZeroStrength != entityData.TimeToGetToZeroStrength)
            saveData.TimeToGetToZeroStrength = entityData.TimeToGetToZeroStrength;
        if(prefabData.StartScale != entityData.StartScale)
            saveData.StartScale = entityData.StartScale;
        if(prefabData.EndScale != entityData.EndScale)
            saveData.EndScale = entityData.EndScale;
        if(prefabData.IsActive != entityData.IsActive)
            saveData.IsActive = entityData.IsActive;

        if (saveData.Equals(default(Bonfire_Save)))
            return null;

        return saveData;
    }

    public override object SaveComponent(Entity entity, EntityMapper entityMapper)
    {
        var data = entity.Read<Bonfire>();

        var saveData = new Bonfire_Save
        {
            ActiveSequenceGuid = data.ActiveSequenceGuid.GuidHash,
            ActiveSequenceState = entityMapper.IndexOf(data.ActiveSequenceState.Id),
            InputItem = data.InputItem,
            Strength = data.Strength,
            BurnTime = data.BurnTime,
            TimeToGetToFullStrength = data.TimeToGetToFullStrength,
            TimeToGetToZeroStrength = data.TimeToGetToZeroStrength,
            StartScale = data.StartScale,
            EndScale = data.EndScale,
            IsActive = data.IsActive
        };

        return saveData;
    }

    public override void ApplyComponentData(Entity entity, JsonElement jsonData, Entity[] entitiesBeingLoaded)
    {
        var saveData = jsonData.Deserialize<Bonfire_Save>(SchematicService.GetJsonOptions());

        if (!entity.Has<Bonfire>())
            entity.Add<Bonfire>();

        var data = entity.Read<Bonfire>();

        if(saveData.ActiveSequenceGuid.HasValue)
            data.ActiveSequenceGuid = new SequenceGUID(saveData.ActiveSequenceGuid.Value);
        if(saveData.ActiveSequenceState.HasValue)
            data.ActiveSequenceState = entitiesBeingLoaded[saveData.ActiveSequenceState.Value];
        if(saveData.InputItem.HasValue)
            data.InputItem = saveData.InputItem.Value;
        if(saveData.Strength.HasValue)
            data.Strength = saveData.Strength.Value;
        if(saveData.BurnTime.HasValue)
            data.BurnTime = saveData.BurnTime.Value;
        if(saveData.TimeToGetToFullStrength.HasValue)
            data.TimeToGetToFullStrength = saveData.TimeToGetToFullStrength.Value;
        if(saveData.TimeToGetToZeroStrength.HasValue)
            data.TimeToGetToZeroStrength = saveData.TimeToGetToZeroStrength.Value;
        if(saveData.StartScale.HasValue)
            data.StartScale = saveData.StartScale.Value;
        if(saveData.EndScale.HasValue)
            data.EndScale = saveData.EndScale.Value;
        if(saveData.IsActive.HasValue)
            data.IsActive = saveData.IsActive.Value;

        entity.Write(data);
    }
    public override int[] GetDependencies(JsonElement data)
    {
        var saveData = data.Deserialize<Bonfire_Save>(SchematicService.GetJsonOptions());
        if (saveData.ActiveSequenceState.HasValue)
            return new int[] { saveData.ActiveSequenceState.Value };
        return new int[0];
    }
}
