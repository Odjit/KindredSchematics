using KindredVignettes.Services;
using ProjectM;
using Stunlock.Core;
using System.Text.Json;
using Unity.Entities;

namespace KindredVignettes.ComponentSaver
{
    [ComponentType(typeof(ServantPower))]
    internal class ServantPower_Saver : ComponentSaver
    {
        struct ServantPower_Save
        {
            public float? GearLevel { get; set; }
            public float? Expertise { get; set; }
            public float? Power { get; set; }
        }

        public override object DiffComponents(Entity prefab, Entity entity, EntityMapper entityMapper)
        {
            var prefabData = prefab.Read<ServantPower>();
            var entityData = entity.Read<ServantPower>();

            var saveData = new ServantPower_Save();

            if (prefabData.GearLevel != entityData.GearLevel)
                saveData.GearLevel = entityData.GearLevel;
            if (prefabData.Expertise != entityData.Expertise)
                saveData.Expertise = entityData.Expertise;
            if (prefabData.Power != entityData.Power)
                saveData.Power = entityData.Power;

            if (saveData.Equals(default(ServantPower_Save)))
                return null;

            return saveData;
        }

        public override object SaveComponent(Entity entity, EntityMapper entityMapper)
        {
            var data = entity.Read<ServantPower>();
            var saveData = new ServantPower_Save
            {
                GearLevel = data.GearLevel,
                Expertise = data.Expertise,
                Power = data.Power
            };

            return saveData;
        }

        public override void ApplyComponentData(Entity entity, JsonElement jsonData, Entity[] entitiesBeingLoaded)
        {
            var saveData = jsonData.Deserialize<ServantPower_Save>(VignetteService.GetJsonOptions());

            if (!entity.Has<ServantPower>())
                entity.Add<ServantPower>();

            var data = entity.Read<ServantPower>();

            if (saveData.GearLevel.HasValue)
                data.GearLevel = saveData.GearLevel.Value;
            if (saveData.Expertise.HasValue)
                data.Expertise = saveData.Expertise.Value;
            if (saveData.Power.HasValue)
                data.Power = saveData.Power.Value;

            entity.Write(data);
        }
    }
}
