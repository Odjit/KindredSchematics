using KindredVignettes.Services;
using ProjectM.Shared;
using Stunlock.Core;
using System.Text.Json;
using Unity.Entities;

namespace KindredVignettes.ComponentSaver;

[ComponentType(typeof(SpellModSetComponent))]
class SpellModSetComponent_Saver : ComponentSaver
{

    public override object DiffComponents(Entity prefab, Entity entity, EntityMapper entityMapper)
    {
        var prefabData = prefab.Read<SpellModSetComponent>().SpellMods;
        var entityData = entity.Read<SpellModSetComponent>().SpellMods;

        var saveData = new SpellModSet_Save(prefabData, entityData);

        return saveData.Equals(default(SpellModSet_Save)) ? null : saveData;
    }

    public override object SaveComponent(Entity entity, EntityMapper entityMapper)
    {
        var data = entity.Read<SpellModSetComponent>().SpellMods;

        var saveData = new SpellModSet_Save(new SpellModSet(), data);

        return saveData;
    }

    public override void ApplyComponentData(Entity entity, JsonElement jsonData, Entity[] entitiesBeingLoaded)
    {
        var saveData = jsonData.Deserialize<SpellModSet_Save>(VignetteService.GetJsonOptions());

        if (!entity.Has<SpellModSetComponent>())
            entity.Add<SpellModSetComponent>();

        var data = entity.Read<SpellModSetComponent>().SpellMods;

        data = saveData.GetSpellModSet(data);

        entity.Write(new SpellModSetComponent() { SpellMods = data });
    }
}


struct SpellModSet_Save
{
    public SpellMod_Save? Mod0 { get; set; }
    public SpellMod_Save? Mod1 { get; set; }
    public SpellMod_Save? Mod2 { get; set; }
    public SpellMod_Save? Mod3 { get; set; }
    public SpellMod_Save? Mod4 { get; set; }
    public SpellMod_Save? Mod5 { get; set; }
    public SpellMod_Save? Mod6 { get; set; }
    public SpellMod_Save? Mod7 { get; set; }
    public byte? Count { get; set; }

    public SpellModSet_Save(SpellModSet prefab, SpellModSet entity)
    {
        Mod0 = !entity.Mod0.Equals(prefab.Mod0) ? new SpellMod_Save(prefab.Mod0, entity.Mod0) : null;
        Mod1 = !entity.Mod1.Equals(prefab.Mod1) ? new SpellMod_Save(prefab.Mod1, entity.Mod1) : null;
        Mod2 = !entity.Mod2.Equals(prefab.Mod2) ? new SpellMod_Save(prefab.Mod2, entity.Mod2) : null;
        Mod3 = !entity.Mod3.Equals(prefab.Mod3) ? new SpellMod_Save(prefab.Mod3, entity.Mod3) : null;
        Mod4 = !entity.Mod4.Equals(prefab.Mod4) ? new SpellMod_Save(prefab.Mod4, entity.Mod4) : null;
        Mod5 = !entity.Mod5.Equals(prefab.Mod5) ? new SpellMod_Save(prefab.Mod5, entity.Mod5) : null;
        Mod6 = !entity.Mod6.Equals(prefab.Mod6) ? new SpellMod_Save(prefab.Mod6, entity.Mod6) : null;
        Mod7 = !entity.Mod7.Equals(prefab.Mod7) ? new SpellMod_Save(prefab.Mod7, entity.Mod7) : null;
        Count = entity.Count != prefab.Count ? entity.Count : null;
    }

    public SpellModSet GetSpellModSet(SpellModSet prefab)
    {
        return new SpellModSet()
        {
            Mod0 = Mod0.HasValue ? Mod0.Value.GetSpellMod(prefab.Mod0) : prefab.Mod0,
            Mod1 = Mod1.HasValue ? Mod1.Value.GetSpellMod(prefab.Mod1) : prefab.Mod1,
            Mod2 = Mod2.HasValue ? Mod2.Value.GetSpellMod(prefab.Mod2) : prefab.Mod2,
            Mod3 = Mod3.HasValue ? Mod3.Value.GetSpellMod(prefab.Mod3) : prefab.Mod3,
            Mod4 = Mod4.HasValue ? Mod4.Value.GetSpellMod(prefab.Mod4) : prefab.Mod4,
            Mod5 = Mod5.HasValue ? Mod5.Value.GetSpellMod(prefab.Mod5) : prefab.Mod5,
            Mod6 = Mod6.HasValue ? Mod6.Value.GetSpellMod(prefab.Mod6) : prefab.Mod6,
            Mod7 = Mod7.HasValue ? Mod7.Value.GetSpellMod(prefab.Mod7) : prefab.Mod7,
            Count = Count.HasValue ? Count.Value : prefab.Count
        };
    }
}

struct SpellMod_Save
{
    public PrefabGUID? Id { get; set; }
    public float? Power { get; set; }

    public SpellMod_Save(SpellMod spellMod)
    {
        Id = spellMod.Id;
        Power = spellMod.Power;
    }

    public SpellMod_Save(SpellMod prefab, SpellMod entity)
    {
        Id = entity.Id != prefab.Id ? entity.Id : null;
        Power = entity.Power != prefab.Power ? entity.Power : null;
    }

    public SpellMod GetSpellMod(SpellMod prefab)
    {
        return new SpellMod()
        {
            Id = Id.HasValue ? Id.Value : prefab.Id,
            Power = Power.HasValue ? Power.Value : prefab.Power
        };
    }
}
