using KindredVignettes.Services;
using ProjectM;
using Stunlock.Core;
using System.Text.Json;
using Unity.Entities;

namespace KindredVignettes.ComponentSaver
{
    [ComponentType(typeof(ServantCoffinstation))]
    internal class ServantCoffinstation_Saver : ComponentSaver
    {
        struct ServantCoffinstation_Save
        {
            public long? InjuryEndTimeTicks { get; set; }
            public float? BloodQuality { get; set; }
            public float? ConvertionProgress { get; set; }
            public string ServantName { get; set; }
            public PrefabGUID? ConvertFromUnit { get; set; }
            public PrefabGUID? ConvertToUnit { get; set; }
            public int? ConnectedServant { get; set; }
            public PrefabGUID? Injury { get; set; }
            public int? State { get; set; }
            public int? ConnectedServantState { get; set; }
            public ushort? ServantSeed { get; set; }
            public byte? ServantEyeColorIndex { get; set; }
            public float? ServantProficiency { get; set; }
            public float? ServantGearLevel { get; set; }            
        }

        public override object DiffComponents(Entity prefab, Entity entity, EntityMapper entityMapper)
        {
            var prefabData = prefab.Read<ServantCoffinstation>();
            var entityData = entity.Read<ServantCoffinstation>();

            var saveData = new ServantCoffinstation_Save();

            if (prefabData.InjuryEndTimeTicks != entityData.InjuryEndTimeTicks)
                saveData.InjuryEndTimeTicks = entityData.InjuryEndTimeTicks;
            if (prefabData.BloodQuality != entityData.BloodQuality)
                saveData.BloodQuality = entityData.BloodQuality;
            if (prefabData.ConvertionProgress != entityData.ConvertionProgress)
                saveData.ConvertionProgress = entityData.ConvertionProgress;
            if (!prefabData.ServantName.Equals(entityData.ServantName))
                saveData.ServantName = entityData.ServantName.ToString();
            if (prefabData.ConvertFromUnit != entityData.ConvertFromUnit)
                saveData.ConvertFromUnit = entityData.ConvertFromUnit;
            if (prefabData.ConvertToUnit != entityData.ConvertToUnit)
                saveData.ConvertToUnit = entityData.ConvertToUnit;
            if (!prefabData.ConnectedServant.Equals(entityData.ConnectedServant))
                saveData.ConnectedServant = entityMapper.IndexOf(entityData.ConnectedServant.GetEntityOnServer());
            if (prefabData.Injury != entityData.Injury)
                saveData.Injury = entityData.Injury;
            if (prefabData.State != entityData.State)
                saveData.State = (int)entityData.State;
            if (prefabData.ConnectedServantState != entityData.ConnectedServantState)
                saveData.ConnectedServantState = (int)entityData.ConnectedServantState;
            if (prefabData.ServantSeed != entityData.ServantSeed)
                saveData.ServantSeed = entityData.ServantSeed;
            if (prefabData.ServantEyeColorIndex != entityData.ServantEyeColorIndex)
                saveData.ServantEyeColorIndex = entityData.ServantEyeColorIndex;
            if (prefabData.ServantProficiency != entityData.ServantProficiency)
                saveData.ServantProficiency = entityData.ServantProficiency;
            if (prefabData.ServantGearLevel != entityData.ServantGearLevel)
                saveData.ServantGearLevel = entityData.ServantGearLevel;

            if (saveData.Equals(default(ServantCoffinstation_Save)))
                return null;

            return saveData;
        }

        public override object SaveComponent(Entity entity, EntityMapper entityMapper)
        {
            var data = entity.Read<ServantCoffinstation>();
            var saveData = new ServantCoffinstation_Save();
            
            saveData.InjuryEndTimeTicks = data.InjuryEndTimeTicks;
            saveData.BloodQuality = data.BloodQuality;
            saveData.ConvertionProgress = data.ConvertionProgress;
            saveData.ServantName = data.ServantName.ToString();
            saveData.ConvertFromUnit = data.ConvertFromUnit;
            saveData.ConvertToUnit = data.ConvertToUnit;
            saveData.ConnectedServant = entityMapper.IndexOf(data.ConnectedServant.GetEntityOnServer());
            saveData.Injury = data.Injury;
            saveData.State = (int)data.State;
            saveData.ConnectedServantState = (int)data.ConnectedServantState;
            saveData.ServantSeed = data.ServantSeed;
            saveData.ServantEyeColorIndex = data.ServantEyeColorIndex;
            saveData.ServantProficiency = data.ServantProficiency;
            saveData.ServantGearLevel = data.ServantGearLevel;

            return saveData;
        }

        public override void ApplyComponentData(Entity entity, JsonElement jsonData, Entity[] entitiesBeingLoaded)
        {
            var saveData = jsonData.Deserialize<ServantCoffinstation_Save>(VignetteService.GetJsonOptions());

            if (!entity.Has<ServantCoffinstation>())
                entity.Add<ServantCoffinstation>();

            var data = entity.Read<ServantCoffinstation>();

            if (saveData.InjuryEndTimeTicks.HasValue)
                data.InjuryEndTimeTicks = saveData.InjuryEndTimeTicks.Value;
            if (saveData.BloodQuality.HasValue)
                data.BloodQuality = saveData.BloodQuality.Value;
            if (saveData.ConvertionProgress.HasValue)
                data.ConvertionProgress = saveData.ConvertionProgress.Value;
            if (saveData.ServantName != null)
                data.ServantName = saveData.ServantName;
            if (saveData.ConvertFromUnit.HasValue)
                data.ConvertFromUnit = saveData.ConvertFromUnit.Value;
            if (saveData.ConvertToUnit.HasValue)
                data.ConvertToUnit = saveData.ConvertToUnit.Value;
            if (saveData.ConnectedServant.HasValue)
                data.ConnectedServant = entitiesBeingLoaded[saveData.ConnectedServant.Value];
            if (saveData.Injury.HasValue)
                data.Injury = saveData.Injury.Value;
            if (saveData.State.HasValue)
                data.State = (ServantCoffinState)saveData.State.Value;
            if (saveData.ConnectedServantState.HasValue)
                data.ConnectedServantState = (GenericEnemyState)saveData.ConnectedServantState.Value;
            if (saveData.ServantSeed.HasValue)
                data.ServantSeed = saveData.ServantSeed.Value;
            if (saveData.ServantEyeColorIndex.HasValue)
                data.ServantEyeColorIndex = saveData.ServantEyeColorIndex.Value;
            if (saveData.ServantProficiency.HasValue)
                data.ServantProficiency = saveData.ServantProficiency.Value;
            if (saveData.ServantGearLevel.HasValue)
                data.ServantGearLevel = saveData.ServantGearLevel.Value;

            entity.Write(data);
        }

        public override int[] GetDependencies(JsonElement data)
        {
            var saveData = data.Deserialize<ServantCoffinstation_Save>(VignetteService.GetJsonOptions());

            if (saveData.ConnectedServant.HasValue)
                return [saveData.ConnectedServant.Value];

            return [];
        }
    }
}
