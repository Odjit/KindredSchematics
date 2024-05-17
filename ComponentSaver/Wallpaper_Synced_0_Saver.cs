using KindredVignettes.Services;
using ProjectM;
using System.Text.Json;
using Unity.Entities;

namespace KindredVignettes.ComponentSaver
{
    struct Wallpaper_Save
    {
        public byte Style { get; set; }
        public byte Variation { get; set; }

        public WallpaperDescription GetWallPaperDescription()
        {
            return new WallpaperDescription()
            {
                Style = Style,
                Variation = Variation,
            };
        }
    }

    [ComponentType(typeof(Wallpaper_Synced_0))]
    internal class Wallpaper_Synced_0_Saver : ComponentSaver
    {

        public override object DiffComponents(Entity prefab, Entity entity, EntityMapper entityMapper)
        {
            return SaveComponent(entity, entityMapper);
        }

        public override object SaveComponent(Entity entity, EntityMapper entityMapper)
        {
            var data = entity.Read<Wallpaper_Synced_0>();

            var saveData = new Wallpaper_Save()
            {
                Style = data.Server.Style,
                Variation = data.Server.Variation,
            };

            return saveData;
        }

        public override void ApplyComponentData(Entity entity, JsonElement jsonData, Entity[] entitiesBeingLoaded)
        {
            var saveData = jsonData.Deserialize<Wallpaper_Save>(VignetteService.GetJsonOptions());

            if (!entity.Has<Wallpaper_Synced_0>())
                entity.Add<Wallpaper_Synced_0>();

            var data = entity.Read<Wallpaper_Synced_0>();

            data.Server = saveData.GetWallPaperDescription();

            entity.Write(data);
        }
    }
}
