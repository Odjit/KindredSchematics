using KindredVignettes.Services;
using ProjectM;
using System.Text.Json;
using Unity.Entities;

namespace KindredVignettes.ComponentSaver
{
    [ComponentType(typeof(PrisonCell))]
    internal class PrisonCell_Saver : ComponentSaver
    {
        struct PrisonCell_Save
        {
            public PrefabGUID? Buff_PsychicForm { get; set; }
            public AssetGuid? LKey_RequiresPsychicForm { get; set; }
            public AssetGuid? LKey_TargetIsImmune { get; set; }
            public PrefabGUID? ImprisonedBuff { get; set; }
            public int? ImprisonedEntity { get; set; }
        }

        public override object DiffComponents(Entity prefab, Entity entity, EntityMapper entityMapper)
        {
            var prefabData = prefab.Read<PrisonCell>();
            var entityData = entity.Read<PrisonCell>();

            var saveData = new PrisonCell_Save();

            if (!prefabData.Buff_PsychicForm.Equals(entityData.Buff_PsychicForm))
                saveData.Buff_PsychicForm = entityData.Buff_PsychicForm;
            if (prefabData.LKey_RequiresPsychicForm != entityData.LKey_RequiresPsychicForm)
                saveData.LKey_RequiresPsychicForm = entityData.LKey_RequiresPsychicForm;
            if (prefabData.LKey_TargetIsImmune != entityData.LKey_TargetIsImmune)
                saveData.LKey_TargetIsImmune = entityData.LKey_TargetIsImmune;
            if (prefabData.ImprisonedBuff != entityData.ImprisonedBuff)
                saveData.ImprisonedBuff = entityData.ImprisonedBuff;
            if (!prefabData.ImprisonedEntity.Equals(entityData.ImprisonedEntity))
                saveData.ImprisonedEntity = entityMapper.IndexOf(entityData.ImprisonedEntity.GetEntityOnServer());

            if (saveData.Equals(default(PrisonCell_Save)))
                return null;

            return saveData;
        }

        public override object SaveComponent(Entity entity, EntityMapper entityMapper)
        {
            var data = entity.Read<PrisonCell>();
            var saveData = new PrisonCell_Save();
            saveData.Buff_PsychicForm = data.Buff_PsychicForm;
            saveData.LKey_RequiresPsychicForm = data.LKey_RequiresPsychicForm;
            saveData.LKey_TargetIsImmune = data.LKey_TargetIsImmune;
            saveData.ImprisonedBuff = data.ImprisonedBuff;
            saveData.ImprisonedEntity = entityMapper.IndexOf(data.ImprisonedEntity.GetEntityOnServer());

            return saveData;
        }

        public override void ApplyComponentData(Entity entity, JsonElement jsonData, Entity[] entitiesBeingLoaded)
        {
            var saveData = jsonData.Deserialize<PrisonCell_Save>(VignetteService.GetJsonOptions());

            if (!entity.Has<PrisonCell>())
                entity.Add<PrisonCell>();

            var data = entity.Read<PrisonCell>();

            if (saveData.Buff_PsychicForm != null)
                data.Buff_PsychicForm = saveData.Buff_PsychicForm.Value;
            if (saveData.LKey_RequiresPsychicForm.HasValue)
                data.LKey_RequiresPsychicForm = saveData.LKey_RequiresPsychicForm.Value;
            if (saveData.LKey_TargetIsImmune.HasValue)
                data.LKey_TargetIsImmune = saveData.LKey_TargetIsImmune.Value;
            if (saveData.ImprisonedBuff != null)
                data.ImprisonedBuff = saveData.ImprisonedBuff.Value;
            if (saveData.ImprisonedEntity != null)
                data.ImprisonedEntity = entitiesBeingLoaded[saveData.ImprisonedEntity.Value];

            entity.Write(data);
        }
    }
}
