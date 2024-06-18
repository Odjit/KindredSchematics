using KindredSchematics.Services;
using ProjectM.Shared;
using Stunlock.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Unity.Entities;

namespace KindredSchematics.ComponentSaver;

[ComponentType(typeof(JewelInstance))]
class JewelInstance_Saver : ComponentSaver
{
    struct JewelInstance_Save
    {
        public PrefabGUID? SpellSchool { get; set; }
        public PrefabGUID? Ability { get; set; }
        public byte? TierIndex { get; set; }
        public PrefabGUID? OverrideAbilityType { get; set; }
        public bool? Initialized { get; set; }
    }

    public override object DiffComponents(Entity prefab, Entity entity, EntityMapper entityMapper)
    {
        var prefabData = prefab.Read<JewelInstance>();
        var entityData = entity.Read<JewelInstance>();

        var saveData = new JewelInstance_Save();
        if (prefabData.SpellSchool != entityData.SpellSchool)
            saveData.SpellSchool = entityData.SpellSchool;
        if (prefabData.Ability != entityData.Ability)
            saveData.Ability = entityData.Ability;
        if (prefabData.TierIndex != entityData.TierIndex)
            saveData.TierIndex = entityData.TierIndex;
        if (prefabData.OverrideAbilityType != entityData.OverrideAbilityType)
            saveData.OverrideAbilityType = entityData.OverrideAbilityType;
        if (prefabData.Initialized != entityData.Initialized)
            saveData.Initialized = entityData.Initialized;

        if (saveData.Equals(default(JewelInstance_Save)))
            return null;

        return saveData;
    }

    public override object SaveComponent(Entity entity, EntityMapper entityMapper)
    {
        var data = entity.Read<JewelInstance>();

        var saveData = new JewelInstance_Save
        {
            SpellSchool = data.SpellSchool,
            Ability = data.Ability,
            TierIndex = data.TierIndex,
            OverrideAbilityType = data.OverrideAbilityType,
            Initialized = data.Initialized
        };

        return saveData;
    }

    public override void ApplyComponentData(Entity entity, JsonElement jsonData, Entity[] entitiesBeingLoaded)
    {
        var saveData = jsonData.Deserialize<JewelInstance_Save>(SchematicService.GetJsonOptions());

        if (!entity.Has<JewelInstance>())
            entity.Add<JewelInstance>();
        
        var data = entity.Read<JewelInstance>();

        if (saveData.SpellSchool.HasValue)
            data.SpellSchool = saveData.SpellSchool.Value;
        if (saveData.Ability.HasValue)
            data.Ability = saveData.Ability.Value;
        if (saveData.TierIndex.HasValue)
            data.TierIndex = saveData.TierIndex.Value;
        if (saveData.OverrideAbilityType.HasValue)
            data.OverrideAbilityType = saveData.OverrideAbilityType.Value;
        if (saveData.Initialized.HasValue)
            data.Initialized = saveData.Initialized.Value;

        entity.Write(data);
    }
}
