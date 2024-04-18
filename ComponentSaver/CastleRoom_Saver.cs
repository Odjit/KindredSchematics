using KindredVignettes.Services;
using ProjectM;
using ProjectM.CastleBuilding;
using System.Text.Json;
using Unity.Entities;

namespace KindredVignettes.ComponentSaver
{
    [ComponentType(typeof(CastleRoom))]
    internal class CastleRoom_Saver : ComponentSaver
    {
        struct CastleRoom_Save
        {
            public bool? IsMissingWalls { get; set; }
            public bool? HasRoof { get; set; }
        }

        public override object DiffComponents(Entity src, Entity dst, EntityMapper entityMapper)
        {
            var srcData = src.Read<CastleRoom>();
            var dstData = dst.Read<CastleRoom>();

            var diff = new CastleRoom_Save();
            if (srcData.IsMissingWalls != dstData.IsMissingWalls)
                diff.IsMissingWalls = dstData.IsMissingWalls;
            if (srcData.HasRoof != dstData.HasRoof)
                diff.HasRoof = dstData.HasRoof;

            if (diff.Equals(default(CastleRoom_Save)))
                return null;

            return diff;
        }

        public override void ApplyDiff(Entity entity, JsonElement diff, Entity[] entitiesBeingLoaded)
        {
            var dyeDiff = diff.Deserialize<CastleRoom_Save>(VignetteService.GetJsonOptions());
            var data = entity.Read<CastleRoom>();

            if (dyeDiff.IsMissingWalls.HasValue)
                data.IsMissingWalls = dyeDiff.IsMissingWalls.Value;
            if (dyeDiff.HasRoof.HasValue)
                data.HasRoof = dyeDiff.HasRoof.Value;

            entity.Write(data);
        }

        public override object SaveComponent(Entity entity, EntityMapper entityMapper)
        {
            throw new System.NotImplementedException();
        }

        public override void AddComponent(Entity entity, JsonElement data, Entity[] entitiesBeingLoaded)
        {
            throw new System.NotImplementedException();
        }
    }
}
