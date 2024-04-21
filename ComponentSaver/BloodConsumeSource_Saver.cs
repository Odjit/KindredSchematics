using KindredVignettes.Services;
using ProjectM;
using System.Text.Json;
using Unity.Entities;

namespace KindredVignettes.ComponentSaver
{
    [ComponentType(typeof(BloodConsumeSource))]
    internal class BloodConsumeSource_Saver : ComponentSaver
    {
        struct BloodConsumeSource_Save
        {
            public float? BloodQuality { get; set; }
            public PrefabGUID? UnitBloodType { get; set; }
            public CurveReference? OverrideBloodCurve { get; set; }
            public bool? ForceBadBloodQuality { get; set; }
            public int? BloodQualityBuffRequirement { get; set; }
            public bool? CanBeConsumed { get; set; }

        }

        public override object DiffComponents(Entity prefab, Entity entity, EntityMapper entityMapper)
        {
            var prefabData = prefab.Read<BloodConsumeSource>();
            var entityData = entity.Read<BloodConsumeSource>();


            var saveData = new BloodConsumeSource_Save();
            if (prefabData.BloodQuality != entityData.BloodQuality)
                saveData.BloodQuality = entityData.BloodQuality;
            if (prefabData.UnitBloodType != entityData.UnitBloodType)
                saveData.UnitBloodType = entityData.UnitBloodType;
            if (!prefabData.OverrideBloodCurve.Equals(entityData.OverrideBloodCurve))
                saveData.OverrideBloodCurve = entityData.OverrideBloodCurve;
            if (prefabData.ForceBadBloodQuality != entityData.ForceBadBloodQuality)
                saveData.ForceBadBloodQuality = entityData.ForceBadBloodQuality;
            if (prefabData.BloodQualityBuffRequirement != entityData.BloodQualityBuffRequirement)
                saveData.BloodQualityBuffRequirement = entityData.BloodQualityBuffRequirement;
            if (prefabData.CanBeConsumed != entityData.CanBeConsumed)
                saveData.CanBeConsumed = entityData.CanBeConsumed;


            if (saveData.Equals(default(BloodConsumeSource_Save)))
                return null;

            return saveData;
        }

        public override object SaveComponent(Entity entity, EntityMapper entityMapper)
        {
            var data = entity.Read<BloodConsumeSource>();

            var saveData = new BloodConsumeSource_Save
            {
                BloodQuality = data.BloodQuality,
                UnitBloodType = data.UnitBloodType,
                OverrideBloodCurve = data.OverrideBloodCurve,
                ForceBadBloodQuality = data.ForceBadBloodQuality,
                BloodQualityBuffRequirement = data.BloodQualityBuffRequirement,
                CanBeConsumed = data.CanBeConsumed
            };

            return saveData;
        }

        public override void ApplyComponentData(Entity entity, JsonElement jsonData, Entity[] entitiesBeingLoaded)
        {
            var saveData = jsonData.Deserialize<BloodConsumeSource_Save>(VignetteService.GetJsonOptions());

            if (!entity.Has<BloodConsumeSource>())
                entity.Add<BloodConsumeSource>();

            var data = entity.Read<BloodConsumeSource>();

            if (saveData.BloodQuality.HasValue)
                data.BloodQuality = saveData.BloodQuality.Value;
            if (saveData.UnitBloodType.HasValue)
                data.UnitBloodType = saveData.UnitBloodType.Value;
            if (saveData.OverrideBloodCurve.HasValue)
                data.OverrideBloodCurve = saveData.OverrideBloodCurve.Value;
            if (saveData.ForceBadBloodQuality.HasValue)
                data.ForceBadBloodQuality = saveData.ForceBadBloodQuality.Value;
            if (saveData.BloodQualityBuffRequirement.HasValue)
                data.BloodQualityBuffRequirement = saveData.BloodQualityBuffRequirement.Value;
            if (saveData.CanBeConsumed.HasValue)
                data.CanBeConsumed = saveData.CanBeConsumed.Value;

            entity.Write(data);
        }
    }
}
