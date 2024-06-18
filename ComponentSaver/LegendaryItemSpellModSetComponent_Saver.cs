using KindredSchematics.Services;
using ProjectM;
using ProjectM.Shared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Unity.Entities;

namespace KindredSchematics.ComponentSaver;
[ComponentType(typeof(LegendaryItemSpellModSetComponent))]
class LegendaryItemSpellModSetComponent_Saver : ComponentSaver
{
    struct LegendaryItemSpellModSetComponent_Save
    {
        public SpellModSet_Save? StatMods { get; set; }
        public SpellModSet_Save? AbilityMods0 { get; set; }
        public SpellModSet_Save? AbilityMods1 { get; set; }
    }

    public override object DiffComponents(Entity prefab, Entity entity, EntityMapper entityMapper)
    {
        var prefabData = prefab.Read<LegendaryItemSpellModSetComponent>();
        var entityData = entity.Read<LegendaryItemSpellModSetComponent>();

        var saveData = new LegendaryItemSpellModSetComponent_Save();
        if (!prefabData.StatMods.Equals(entityData.StatMods))
            saveData.StatMods = new SpellModSet_Save(prefabData.StatMods, entityData.StatMods);
        if (!prefabData.AbilityMods0.Equals(entityData.AbilityMods0))
            saveData.AbilityMods0 = new SpellModSet_Save(prefabData.AbilityMods0, entityData.AbilityMods0);
        if (!prefabData.AbilityMods1.Equals(entityData.AbilityMods1))
            saveData.AbilityMods1 = new SpellModSet_Save(prefabData.AbilityMods1, entityData.AbilityMods1);

        if (saveData.Equals(default(LegendaryItemSpellModSetComponent_Save)))
            return null;

        return saveData;
    }

    public override object SaveComponent(Entity entity, EntityMapper entityMapper)
    {
        var data = entity.Read<LegendaryItemSpellModSetComponent>();

        var defaultData = new LegendaryItemSpellModSetComponent();

        var saveData = new LegendaryItemSpellModSetComponent_Save
        {
            StatMods = new SpellModSet_Save(defaultData.StatMods, data.StatMods),
            AbilityMods0 = new SpellModSet_Save(defaultData.AbilityMods0, data.AbilityMods0),
            AbilityMods1 = new SpellModSet_Save(defaultData.AbilityMods1, data.AbilityMods1)
        };

        return saveData;
    }

    public override void ApplyComponentData(Entity entity, JsonElement jsonData, Entity[] entitiesBeingLoaded)
    {
        var saveData = jsonData.Deserialize<LegendaryItemSpellModSetComponent_Save>(SchematicService.GetJsonOptions());

        if (!entity.Has<LegendaryItemSpellModSetComponent>())
            entity.Add<LegendaryItemSpellModSetComponent>();

        var data = entity.Read<LegendaryItemSpellModSetComponent>();

        if (saveData.StatMods.HasValue)
            data.StatMods = saveData.StatMods.Value.GetSpellModSet(data.StatMods);
        if (saveData.AbilityMods0.HasValue)
            data.AbilityMods0 = saveData.AbilityMods0.Value.GetSpellModSet(data.AbilityMods0);
        if (saveData.AbilityMods1.HasValue)
            data.AbilityMods1 = saveData.AbilityMods1.Value.GetSpellModSet(data.AbilityMods1);

        IEnumerator DelayedWriteData()
        {
            while (entity.Has<SpawnTag>())
                yield return null;
            entity.Write(data);
        }

        Core.StartCoroutine(DelayedWriteData());
    }
}
