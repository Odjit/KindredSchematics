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

        public override object DiffComponents(Entity src, Entity dst, EntityMapper entityMapper)
        {
            var srcData = src.Read<AutoChainInstanceData>();
            var dstData = dst.Read<AutoChainInstanceData>();

            var diff = new AutoChainInstanceData_Save();

            if (srcData.NextTransitionAttempt != dstData.NextTransitionAttempt)
            {
                var nextTime = dstData.NextTransitionAttempt;
                if(nextTime > 0)
                    nextTime -= Core.CastleBuffsTickSystem._ServerTime.GetSingleton().Time;
                diff.NextTransitionAttempt = nextTime;
            }

            if (diff.Equals(default(AutoChainInstanceData_Save)))
                return null;

            return diff;
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
                    nextTime += Core.CastleBuffsTickSystem._ServerTime.GetSingleton().Time;
                data.NextTransitionAttempt = nextTime;
            }
            entity.Write(data);
        }
    }
}
