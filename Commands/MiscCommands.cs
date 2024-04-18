using ProjectM;
using Unity.Transforms;
using VampireCommandFramework;

namespace KindredVignettes.Commands
{
    internal class MiscCommands
    {
        [Command("flyheight", description: "Sets the fly height for the user", adminOnly: true)]
        public static void SetFlyHeight(ChatCommandContext ctx, float height = 30)
        {
            var charEntity = ctx.Event.SenderCharacterEntity;
            var canFly = charEntity.Read<CanFly>();
            canFly.FlyingHeight.Value = height;
            charEntity.Write(canFly);
            ctx.Reply($"Set fly height to {height}");
        }

        [Command("flyobstacleheight", description: "Set the height to fly above any obstacles", adminOnly: true)]
        public static void SetFlyObstacleHeight(ChatCommandContext ctx, float height = 7)
        {
            var charEntity = ctx.Event.SenderCharacterEntity;
            var canFly = charEntity.Read<CanFly>();
            canFly.HeightAboveObstacle.Value = height;
            charEntity.Write(canFly);
            ctx.Reply($"Set fly obstacle height to {height}");
        }

        [Command("forcechain", description: "Set the chain transition time for nearby chains to now", adminOnly: true)]
        public static void ChainTransition(ChatCommandContext ctx, float range=10)
        {
            var charEntity = ctx.Event.SenderCharacterEntity;
            var time = Core.CastleBuffsTickSystem._ServerTime.GetSingleton().Time;
            foreach(var chainEntity in Helper.GetAllEntitiesInRadius<AutoChainInstanceData>(charEntity.Read<Translation>().Value.xz, range))
            {
                chainEntity.Write(new AutoChainInstanceData() { NextTransitionAttempt=time });
            }
        }
    }
}
