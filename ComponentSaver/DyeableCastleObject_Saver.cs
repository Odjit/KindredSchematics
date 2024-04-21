using KindredVignettes.Services;
using ProjectM;
using ProjectM.CastleBuilding;
using System.Text.Json;
using Unity.Entities;

namespace KindredVignettes.ComponentSaver
{
    [ComponentType(typeof(DyeableCastleObject))]
    internal class DyeableCastleObject_Saver : ComponentSaver
    {
        struct DyeableCastleObject_Save
        {
            public byte? ActiveColorIndex { get; set; }
            public byte? NumColorChoices { get; set; }
            public byte? PrevColorIndex { get; set; }
            public PrefabGUID? ColorSwatchAssetGuid { get; set; }
        }

        public override object DiffComponents(Entity prefab, Entity entity, EntityMapper entityMapper)
        {
            var prefabData = prefab.Read<DyeableCastleObject>();
            var entityData = entity.Read<DyeableCastleObject>();

            var saveData = new DyeableCastleObject_Save();

            if (prefabData.ActiveColorIndex != entityData.ActiveColorIndex)
                saveData.ActiveColorIndex = entityData.ActiveColorIndex;
            if (prefabData.NumColorChoices != entityData.NumColorChoices)
                saveData.NumColorChoices = entityData.NumColorChoices;
            if (prefabData.PrevColorIndex != entityData.PrevColorIndex)
                saveData.PrevColorIndex = entityData.PrevColorIndex;
            if (prefabData.ColorSwatchAssetGuid != entityData.ColorSwatchAssetGuid)
                saveData.ColorSwatchAssetGuid = entityData.ColorSwatchAssetGuid;

            if (saveData.Equals(default(DyeableCastleObject_Save)))
                return null;

            return saveData;
        }

        public override object SaveComponent(Entity entity, EntityMapper entityMapper)
        {
            throw new System.NotImplementedException();
        }

        public override void ApplyComponentData(Entity entity, JsonElement jsonData, Entity[] entitiesBeingLoaded)
        {
            var saveData = jsonData.Deserialize<DyeableCastleObject_Save>(VignetteService.GetJsonOptions());

            if (!entity.Has<DyeableCastleObject>())
                entity.Add<DyeableCastleObject>();

            var data = entity.Read<DyeableCastleObject>();

            if (saveData.ActiveColorIndex.HasValue)
                data.ActiveColorIndex = saveData.ActiveColorIndex.Value;

            if (saveData.NumColorChoices.HasValue)
                data.NumColorChoices = saveData.NumColorChoices.Value;

            if (saveData.PrevColorIndex.HasValue)
                data.PrevColorIndex = saveData.PrevColorIndex.Value;

            if (saveData.ColorSwatchAssetGuid.HasValue)
                data.ColorSwatchAssetGuid = saveData.ColorSwatchAssetGuid.Value;

            entity.Write(data);
        }
    }
}
