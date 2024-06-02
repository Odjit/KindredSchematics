using KindredVignettes.Services;
using ProjectM.CastleBuilding;
using System.Linq;
using System.Text.Json;
using Unity.Entities;
using UnityEngine.UIElements;

namespace KindredVignettes.ComponentSaver
{
    [ComponentType(typeof(CastleBuildingFusedChild))]
    internal class CastleBuildingFusedChild_Saver : ComponentSaver
    {
        struct CastleBuildingFusedChild_Save
        {
            public int? ParentEntity { get; set; }
        }

        public override object DiffComponents(Entity prefab, Entity entity, EntityMapper entityMapper)
        {
            var prefabData = prefab.Read<CastleBuildingFusedChild>();
            var entityData = entity.Read<CastleBuildingFusedChild>();

            var saveData = new CastleBuildingFusedChild_Save();
            if (prefabData.ParentEntity.GetEntityOnServer() != entityData.ParentEntity.GetEntityOnServer())
                saveData.ParentEntity = entityMapper.IndexOf(entityData.ParentEntity.GetEntityOnServer());

            if (saveData.Equals(default(CastleBuildingFusedChild_Save)))
                return null;

            return saveData;
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

        public override int[] GetDependencies(JsonElement data)
        {
            var saveData = data.Deserialize<CastleBuildingFusedChild_Save>(VignetteService.GetJsonOptions());
            if (saveData.ParentEntity.HasValue)
                return [saveData.ParentEntity.Value];
            return [];
        }
    }
}
