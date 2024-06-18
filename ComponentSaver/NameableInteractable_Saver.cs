using KindredSchematics.Services;
using ProjectM;
using System.Text.Json;
using Unity.Entities;

namespace KindredSchematics.ComponentSaver
{
    [ComponentType(typeof(NameableInteractable))]
    internal class NameableInteractable_Saver : ComponentSaver
    {
        struct NameableInteractable_Save
        {
            public string Name { get; set; }
            public bool? OnlyAllyRename { get; set; }
            public bool? OnlyAllySee { get; set; }
        }

        public override object DiffComponents(Entity prefab, Entity entity, EntityMapper entityMapper)
        {
            var prefabData = prefab.Read<NameableInteractable>();
            var entityData = entity.Read<NameableInteractable>();

            var saveData = new NameableInteractable_Save();

            if (!prefabData.Name.Equals(entityData.Name))
                saveData.Name = entityData.Name.ToString();
            if (prefabData.OnlyAllyRename != entityData.OnlyAllyRename)
                saveData.OnlyAllyRename = entityData.OnlyAllyRename;
            if (prefabData.OnlyAllySee != entityData.OnlyAllySee)
                saveData.OnlyAllySee = entityData.OnlyAllySee;

            if (saveData.Equals(default(NameableInteractable_Save)))
                return null;

            return saveData;
        }

        public override object SaveComponent(Entity entity, EntityMapper entityMapper)
        {
            var data = entity.Read<NameableInteractable>();
            var saveData = new NameableInteractable_Save();
            saveData.Name = data.Name.ToString();
            saveData.OnlyAllyRename = data.OnlyAllyRename;
            saveData.OnlyAllySee = data.OnlyAllySee;

            return saveData;
        }

        public override void ApplyComponentData(Entity entity, JsonElement jsonData, Entity[] entitiesBeingLoaded)
        {
            var saveData = jsonData.Deserialize<NameableInteractable_Save>(SchematicService.GetJsonOptions());

            if (!entity.Has<NameableInteractable>())
                entity.Add<NameableInteractable>();

            var data = entity.Read<NameableInteractable>();

            if (saveData.Name != null)
                data.Name = saveData.Name;
            if (saveData.OnlyAllyRename.HasValue)
                data.OnlyAllyRename = saveData.OnlyAllyRename.Value;
            if (saveData.OnlyAllySee.HasValue)
                data.OnlyAllySee = saveData.OnlyAllySee.Value;

            entity.Write(data);
        }
    }
}
