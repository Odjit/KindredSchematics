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


        struct Wallpaper_FourSplits_saveData
        {
            public WallpaperOrientationData_Save? Data_0 { get; set; }
            public WallpaperOrientationData_Save? Data_90 { get; set; }
            public WallpaperOrientationData_Save? Data_180 { get; set; }
            public WallpaperOrientationData_Save? Data_270 { get; set; }

            public byte? ActiveStyleOverride { get; set; }
            public byte? ActiveVariationOverride { get; set; }
            public int? OverrideOrientation { get; set; }
        }

        public override object DiffComponents(Entity prefab, Entity entity, EntityMapper entityMapper)
        {
            var prefabData = prefab.Read<Wallpaper_FourSplits>();
            var entityData = entity.Read<Wallpaper_FourSplits>();

            var saveData = new Wallpaper_FourSplits_saveData();

            var prefabData0 = new WallpaperOrientationData_Save(prefabData.Data_0);
            var entityData0 = new WallpaperOrientationData_Save(entityData.Data_0);
            if (prefabData0 != entityData0)
                saveData.Data_0 = entityData0;

            var prefabData90 = new WallpaperOrientationData_Save(prefabData.Data_90);
            var entityData90 = new WallpaperOrientationData_Save(entityData.Data_90);
            if (prefabData90 != entityData90)
                saveData.Data_90 = entityData90;

            var prefabData180 = new WallpaperOrientationData_Save(prefabData.Data_180);
            var entityData180 = new WallpaperOrientationData_Save(entityData.Data_180);
            if (prefabData180 != entityData180)
                saveData.Data_180 = entityData180;

            var prefabData270 = new WallpaperOrientationData_Save(prefabData.Data_270);
            var entityData270 = new WallpaperOrientationData_Save(entityData.Data_270);
            if (prefabData270 != entityData270)
                saveData.Data_270 = entityData270;

            if (prefabData.ActiveStyleOverride != entityData.ActiveStyleOverride)
                saveData.ActiveStyleOverride = entityData.ActiveStyleOverride;

            if (prefabData.ActiveVariationOverride != entityData.ActiveVariationOverride)
                saveData.ActiveVariationOverride = entityData.ActiveVariationOverride;

            if (prefabData.OverrideOrientation != entityData.OverrideOrientation)
                saveData.OverrideOrientation = (int)entityData.OverrideOrientation;

            if (saveData.Equals(default(Wallpaper_FourSplits_saveData)))
                return null;

            return saveData;
        }

        public override object SaveComponent(Entity entity, EntityMapper entityMapper)
        {
            throw new System.NotImplementedException();
        }

        public override void ApplyComponentData(Entity entity, JsonElement jsonData, Entity[] entitiesBeingLoaded)
        {
            var wallpapersaveData = jsonData.Deserialize<Wallpaper_FourSplits_saveData>();

            if (!entity.Has<Wallpaper_FourSplits>())
                entity.Add<Wallpaper_FourSplits>();

            var data = entity.Read<Wallpaper_FourSplits>();

            if (wallpapersaveData.Data_0.HasValue)
                data.Data_0 = wallpapersaveData.Data_0.Value.ConvertBack();

            if (wallpapersaveData.Data_90.HasValue)
                data.Data_90 = wallpapersaveData.Data_90.Value.ConvertBack();

            if (wallpapersaveData.Data_180.HasValue)
                data.Data_180 = wallpapersaveData.Data_180.Value.ConvertBack();

            if (wallpapersaveData.Data_270.HasValue)
                data.Data_270 = wallpapersaveData.Data_270.Value.ConvertBack();

            if (wallpapersaveData.ActiveStyleOverride.HasValue)
                data.ActiveStyleOverride = wallpapersaveData.ActiveStyleOverride.Value;

            if (wallpapersaveData.ActiveVariationOverride.HasValue)
                data.ActiveVariationOverride = wallpapersaveData.ActiveVariationOverride.Value;

            if (wallpapersaveData.OverrideOrientation.HasValue)
                data.OverrideOrientation = (WallpaperOrientation)wallpapersaveData.OverrideOrientation.Value;

            entity.Write(data);
        }
    }
}
