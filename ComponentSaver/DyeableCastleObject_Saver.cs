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

        public override object DiffComponents(Entity src, Entity dst, EntityMapper entityMapper)
        {
            var srcData = src.Read<DyeableCastleObject>();
            var dstData = dst.Read<DyeableCastleObject>();

            var diff = new DyeableCastleObject_Save();

            if (srcData.ActiveColorIndex != dstData.ActiveColorIndex)
                diff.ActiveColorIndex = dstData.ActiveColorIndex;
            if (srcData.NumColorChoices != dstData.NumColorChoices)
                diff.NumColorChoices = dstData.NumColorChoices;
            if (srcData.PrevColorIndex != dstData.PrevColorIndex)
                diff.PrevColorIndex = dstData.PrevColorIndex;
            if (srcData.ColorSwatchAssetGuid != dstData.ColorSwatchAssetGuid)
                diff.ColorSwatchAssetGuid = dstData.ColorSwatchAssetGuid;

            if (diff.Equals(default(DyeableCastleObject_Save)))
                return null;

            return diff;
        }

        public override object SaveComponent(Entity entity, EntityMapper entityMapper)
        {
            throw new System.NotImplementedException();
        }

        public override void ApplyComponentData(Entity entity, JsonElement jsonData, Entity[] entitiesBeingLoaded)
        {
            var dyeDiff = jsonData.Deserialize<DyeableCastleObject_Save>(VignetteService.GetJsonOptions());

            if (!entity.Has<DyeableCastleObject>())
                entity.Add<DyeableCastleObject>();

            var data = entity.Read<DyeableCastleObject>();

            if (dyeDiff.ActiveColorIndex.HasValue)
                data.ActiveColorIndex = dyeDiff.ActiveColorIndex.Value;

            if (dyeDiff.NumColorChoices.HasValue)
                data.NumColorChoices = dyeDiff.NumColorChoices.Value;

            if (dyeDiff.PrevColorIndex.HasValue)
                data.PrevColorIndex = dyeDiff.PrevColorIndex.Value;

            if (dyeDiff.ColorSwatchAssetGuid.HasValue)
                data.ColorSwatchAssetGuid = dyeDiff.ColorSwatchAssetGuid.Value;

            entity.Write(data);
        }
    }
}
