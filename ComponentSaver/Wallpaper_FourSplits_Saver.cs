using ProjectM;
using System.Text.Json;
using Unity.Entities;

namespace KindredVignettes.ComponentSaver
{
    [ComponentType(typeof(Wallpaper_FourSplits))]
    internal class Wallpaper_FourSplits_Saver : ComponentSaver
    {
        struct Indices
        {
            public byte Active { get; set; }
            public byte Current { get; set; }
        }

        struct WallpaperOrientationData_Save
        {
            public Indices StyleIndices { get; set; }
            public Indices VariationIndices { get; set; }

            public WallpaperOrientationData_Save(WallpaperOrientationData data)
            {
                StyleIndices = new Indices { Active = data.StyleIndices.Active, Current = data.StyleIndices.Current };
                VariationIndices = new Indices { Active = data.VariationIndices.Active, Current = data.VariationIndices.Current };
            }

            public WallpaperOrientationData ConvertBack()
            {
                var r = new WallpaperOrientationData();
                r.StyleIndices.Active = StyleIndices.Active;
                r.StyleIndices.Current = StyleIndices.Current;
                r.VariationIndices.Active = VariationIndices.Active;
                r.VariationIndices.Current = VariationIndices.Current;
                return r;
            }

            public override readonly bool Equals(object other)
            {
                if (!(other is WallpaperOrientationData_Save))
                    return false;
                var otherWOD = (WallpaperOrientationData_Save)other;
                return StyleIndices.Active == otherWOD.StyleIndices.Active && StyleIndices.Current == otherWOD.StyleIndices.Current &&
                    VariationIndices.Active == otherWOD.VariationIndices.Active && VariationIndices.Current == otherWOD.VariationIndices.Current;
            }

            public override readonly int GetHashCode()
            {
                return StyleIndices.Active.GetHashCode() ^ StyleIndices.Current.GetHashCode() ^ VariationIndices.Active.GetHashCode() ^ VariationIndices.Current.GetHashCode();
            }

            public static bool operator ==(WallpaperOrientationData_Save a, WallpaperOrientationData_Save b)
            {
                return a.Equals(b);
            }

            public static bool operator !=(WallpaperOrientationData_Save a, WallpaperOrientationData_Save b)
            {
                return !(a == b);
            }
        }


        struct Wallpaper_FourSplits_Diff
        {
            public WallpaperOrientationData_Save? Data_0 { get; set; }
            public WallpaperOrientationData_Save? Data_90 { get; set; }
            public WallpaperOrientationData_Save? Data_180 { get; set; }
            public WallpaperOrientationData_Save? Data_270 { get; set; }

            public byte? ActiveStyleOverride { get; set; }
            public byte? ActiveVariationOverride { get; set; }
            public int? OverrideOrientation { get; set; }
        }

        public override object DiffComponents(Entity src, Entity dst, EntityMapper entityMapper)
        {
            var srcData = src.Read<Wallpaper_FourSplits>();
            var dstData = dst.Read<Wallpaper_FourSplits>();

            var diff = new Wallpaper_FourSplits_Diff();

            var srcData0 = new WallpaperOrientationData_Save(srcData.Data_0);
            var dstData0 = new WallpaperOrientationData_Save(dstData.Data_0);
            if (srcData0 != dstData0)
                diff.Data_0 = dstData0;

            var srcData90 = new WallpaperOrientationData_Save(srcData.Data_90);
            var dstData90 = new WallpaperOrientationData_Save(dstData.Data_90);
            if (srcData90 != dstData90)
                diff.Data_90 = dstData90;

            var srcData180 = new WallpaperOrientationData_Save(srcData.Data_180);
            var dstData180 = new WallpaperOrientationData_Save(dstData.Data_180);
            if (srcData180 != dstData180)
                diff.Data_180 = dstData180;

            var srcData270 = new WallpaperOrientationData_Save(srcData.Data_270);
            var dstData270 = new WallpaperOrientationData_Save(dstData.Data_270);
            if (srcData270 != dstData270)
                diff.Data_270 = dstData270;

            if (srcData.ActiveStyleOverride != dstData.ActiveStyleOverride)
                diff.ActiveStyleOverride = dstData.ActiveStyleOverride;

            if (srcData.ActiveVariationOverride != dstData.ActiveVariationOverride)
                diff.ActiveVariationOverride = dstData.ActiveVariationOverride;

            if (srcData.OverrideOrientation != dstData.OverrideOrientation)
                diff.OverrideOrientation = (int)dstData.OverrideOrientation;

            if (diff.Equals(default(Wallpaper_FourSplits_Diff)))
                return null;

            return diff;
        }

        public override void ApplyDiff(Entity entity, JsonElement diff, Entity[] entitiesBeingLoaded)
        {
            var wallpaperDiff = diff.Deserialize<Wallpaper_FourSplits_Diff>();
            var data = entity.Read<Wallpaper_FourSplits>();

            if (wallpaperDiff.Data_0.HasValue)
                data.Data_0 = wallpaperDiff.Data_0.Value.ConvertBack();

            if (wallpaperDiff.Data_90.HasValue)
                data.Data_90 = wallpaperDiff.Data_90.Value.ConvertBack();

            if (wallpaperDiff.Data_180.HasValue)
                data.Data_180 = wallpaperDiff.Data_180.Value.ConvertBack();

            if (wallpaperDiff.Data_270.HasValue)
                data.Data_270 = wallpaperDiff.Data_270.Value.ConvertBack();

            if (wallpaperDiff.ActiveStyleOverride.HasValue)
                data.ActiveStyleOverride = wallpaperDiff.ActiveStyleOverride.Value;

            if (wallpaperDiff.ActiveVariationOverride.HasValue)
                data.ActiveVariationOverride = wallpaperDiff.ActiveVariationOverride.Value;

            if (wallpaperDiff.OverrideOrientation.HasValue)
                data.OverrideOrientation = (WallpaperOrientation)wallpaperDiff.OverrideOrientation.Value;

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
