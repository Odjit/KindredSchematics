using KindredVignettes.Services;
using ProjectM.CastleBuilding;
using System.Text.Json;
using Unity.Entities;

namespace KindredVignettes.ComponentSaver
{
    [ComponentType(typeof(CastleBuildingFusedChild))]
    internal class CastleBuildingFusedChild_Saver : ComponentSaver
    {
        struct CastleBuildingFusedChild_Save
        {
            public int? ParentEntity { get; set; }
        }

        public override object DiffComponents(Entity src, Entity dst, EntityMapper entityMapper)
        {
            var srcData = src.Read<CastleBuildingFusedChild>();
            var dstData = dst.Read<CastleBuildingFusedChild>();

            var diff = new CastleBuildingFusedChild_Save();
            if (srcData.ParentEntity.GetEntityOnServer() != dstData.ParentEntity.GetEntityOnServer())
                diff.ParentEntity = entityMapper.IndexOf(dstData.ParentEntity.GetEntityOnServer());

            if (diff.Equals(default(CastleBuildingFusedChild_Save)))
                return null;

            return diff;
        }

        public override object SaveComponent(Entity entity, EntityMapper entityMapper)
        {
            throw new System.NotImplementedException();
        }

        public override void ApplyComponentData(Entity entity, JsonElement jsonData, Entity[] entitiesBeingLoaded)
        {
            var saveData = jsonData.Deserialize<CastleBuildingFusedChild_Save>(VignetteService.GetJsonOptions());
            
            if(!entity.Has<CastleBuildingFusedChild>())
                entity.Add<CastleBuildingFusedChild>();
            
            var data = entity.Read<CastleBuildingFusedChild>();

            if (saveData.ParentEntity.HasValue)
                data.ParentEntity = entitiesBeingLoaded[saveData.ParentEntity.Value];

            entity.Write(data);
        }
    }
}
