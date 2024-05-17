using KindredVignettes.Services;
using ProjectM;
using System.Text.Json;
using Unity.Entities;

namespace KindredVignettes.ComponentSaver
{
    [ComponentType(typeof(AutoChainInstanceData))]
    internal class AutoChainInstanceData_Saver : ComponentSaver
    {
        struct AutoChainInstanceData_Save
        {
            public double? NextTransitionAttempt { get; set; }
        }

        public override object DiffComponents(Entity prefab, Entity entity, EntityMapper entityMapper)
        {
            var prefabData = prefab.Read<AutoChainInstanceData>();
            var entityData = entity.Read<AutoChainInstanceData>();

            var saveData = new AutoChainInstanceData_Save();

            if (prefabData.NextTransitionAttempt != entityData.NextTransitionAttempt)
            {
                var nextTime = entityData.NextTransitionAttempt;
                if(nextTime > 0)
                    nextTime -= Core.ServerTime;
                saveData.NextTransitionAttempt = nextTime;
            }

            if (saveData.Equals(default(AutoChainInstanceData_Save)))
                return null;

            return saveData;
        }

        public override object SaveComponent(Entity entity, EntityMapper entityMapper)
        {
            throw new System.NotImplementedException();
        }

        public override void ApplyComponentData(Entity entity, JsonElement jsonData, Entity[] entitiesBeingLoaded)
        {
            var AutoChainInstanceData = jsonData.Deserialize<AutoChainInstanceData_Save>(VignetteService.GetJsonOptions());

            if(!entity.Has<AutoChainInstanceData>())
                entity.Add<AutoChainInstanceData>();

            var data = entity.Read<AutoChainInstanceData>();
            if (AutoChainInstanceData.NextTransitionAttempt.HasValue)
            {
                var nextTime = AutoChainInstanceData.NextTransitionAttempt.Value;
                if (nextTime > 0)
                    nextTime += Core.ServerTime;
                data.NextTransitionAttempt = nextTime;
            }
            entity.Write(data);
        }
    }
}
