using KindredVignettes.Services;
using ProjectM.Shared;
using Stunlock.Core;
using System.Text.Json;
using Unity.Entities;

namespace KindredVignettes.ComponentSaver;

[ComponentType(typeof(Durability))]
internal class Durability_Saver : ComponentSaver
{
    struct Durability_Save
    {
        public float? Value { get; set; }
        public float? MaxDurability { get; set; }
        public PrefabGUID? RepairRecipe { get; set; }
        public int? LossType { get; set; }
        public float? TakeDamageDurabilityLossFactor { get; set; }
        public DurabilityDamageModifiers_Save? DealDamageTypeModifiers { get; set; }
        public bool? IsBroken { get; set; }
        public float? OneLevelFactor { get; set; }
        public float? TwoLevelFactor { get; set; }
        public float? ThreeLevelFactor { get; set; }
        public bool? DestroyItemWhenBroken { get; set; }
    }

    struct DurabilityDamageModifiers_Save
    {
        public float? MainDamageModifier { get; set; }
        public float? ResourceDamageModifier { get; set; }
        public float? SiegeDamageModifier { get; set; }
    }

    public override object DiffComponents(Entity prefab, Entity entity, EntityMapper entityMapper)
    {
        var prefabData = prefab.Read<Durability>();
        var entityData = entity.Read<Durability>();

        var saveData = new Durability_Save();
        if (prefabData.Value != entityData.Value)
            saveData.Value = entityData.Value;
        if (prefabData.MaxDurability != entityData.MaxDurability)
            saveData.MaxDurability = entityData.MaxDurability;
        if (prefabData.RepairRecipe != entityData.RepairRecipe)
            saveData.RepairRecipe = entityData.RepairRecipe;
        if (prefabData.LossType != entityData.LossType)
            saveData.LossType = (int)entityData.LossType;
        if (prefabData.TakeDamageDurabilityLossFactor != entityData.TakeDamageDurabilityLossFactor)
            saveData.TakeDamageDurabilityLossFactor = entityData.TakeDamageDurabilityLossFactor;
        if (!prefabData.DealDamageTypeModifiers.Equals(entityData.DealDamageTypeModifiers))
        {
            var modifiers = new DurabilityDamageModifiers_Save();
            if (prefabData.DealDamageTypeModifiers.MainDamageModifier != entityData.DealDamageTypeModifiers.MainDamageModifier)
                modifiers.MainDamageModifier = entityData.DealDamageTypeModifiers.MainDamageModifier;
            if (prefabData.DealDamageTypeModifiers.ResourceDamageModifier != entityData.DealDamageTypeModifiers.ResourceDamageModifier)
                modifiers.ResourceDamageModifier = entityData.DealDamageTypeModifiers.ResourceDamageModifier;
            if (prefabData.DealDamageTypeModifiers.SiegeDamageModifier != entityData.DealDamageTypeModifiers.SiegeDamageModifier)
                modifiers.SiegeDamageModifier = entityData.DealDamageTypeModifiers.SiegeDamageModifier;
            saveData.DealDamageTypeModifiers = modifiers;
        }
        if (prefabData.IsBroken != entityData.IsBroken)
            saveData.IsBroken = entityData.IsBroken;
        if (prefabData.OneLevelFactor != entityData.OneLevelFactor)
            saveData.OneLevelFactor = entityData.OneLevelFactor;
        if (prefabData.TwoLevelFactor != entityData.TwoLevelFactor)
            saveData.TwoLevelFactor = entityData.TwoLevelFactor;
        if (prefabData.ThreeLevelFactor != entityData.ThreeLevelFactor)
            saveData.ThreeLevelFactor = entityData.ThreeLevelFactor;
        if (prefabData.DestroyItemWhenBroken != entityData.DestroyItemWhenBroken)
            saveData.DestroyItemWhenBroken = entityData.DestroyItemWhenBroken;

        return saveData.Equals(default(Durability_Save)) ? null : saveData;
    }

    public override object SaveComponent(Entity entity, EntityMapper entityMapper)
    {
        var data = entity.Read<Durability>();

        var saveData = new Durability_Save
        {
            Value = data.Value,
            MaxDurability = data.MaxDurability,
            RepairRecipe = data.RepairRecipe,
            LossType = (int)data.LossType,
            TakeDamageDurabilityLossFactor = data.TakeDamageDurabilityLossFactor,
            IsBroken = data.IsBroken,
            OneLevelFactor = data.OneLevelFactor,
            TwoLevelFactor = data.TwoLevelFactor,
            ThreeLevelFactor = data.ThreeLevelFactor,
            DestroyItemWhenBroken = data.DestroyItemWhenBroken
        };

        var modifiers = new DurabilityDamageModifiers_Save
        {
            MainDamageModifier = data.DealDamageTypeModifiers.MainDamageModifier,
            ResourceDamageModifier = data.DealDamageTypeModifiers.ResourceDamageModifier,
            SiegeDamageModifier = data.DealDamageTypeModifiers.SiegeDamageModifier
        };
        saveData.DealDamageTypeModifiers = modifiers;

        return saveData;
    }

    public override void ApplyComponentData(Entity entity, JsonElement jsonData, Entity[] entitiesBeingLoaded)
    {
        var saveData = jsonData.Deserialize<Durability_Save>(VignetteService.GetJsonOptions());

        if (!entity.Has<Durability>())
            entity.Add<Durability>();

        var data = entity.Read<Durability>();
        if (saveData.Value.HasValue)
            data.Value = saveData.Value.Value;
        if (saveData.MaxDurability.HasValue)
            data.MaxDurability = saveData.MaxDurability.Value;
        if (saveData.RepairRecipe.HasValue)
            data.RepairRecipe = saveData.RepairRecipe.Value;
        if (saveData.LossType.HasValue)
            data.LossType = (DurabilityLossType)saveData.LossType.Value;
        if (saveData.TakeDamageDurabilityLossFactor.HasValue)
            data.TakeDamageDurabilityLossFactor = saveData.TakeDamageDurabilityLossFactor.Value;
        if (saveData.DealDamageTypeModifiers.HasValue)
        {
            var modifiers = new DurabilityDamageModifiers();
            if (saveData.DealDamageTypeModifiers.Value.MainDamageModifier.HasValue)
                modifiers.MainDamageModifier = saveData.DealDamageTypeModifiers.Value.MainDamageModifier.Value;
            if (saveData.DealDamageTypeModifiers.Value.ResourceDamageModifier.HasValue)
                modifiers.ResourceDamageModifier = saveData.DealDamageTypeModifiers.Value.ResourceDamageModifier.Value;
            if (saveData.DealDamageTypeModifiers.Value.SiegeDamageModifier.HasValue)
                modifiers.SiegeDamageModifier = saveData.DealDamageTypeModifiers.Value.SiegeDamageModifier.Value;
            data.DealDamageTypeModifiers = modifiers;
        }
        if (saveData.IsBroken.HasValue)
            data.IsBroken = saveData.IsBroken.Value;
        if (saveData.OneLevelFactor.HasValue)
            data.OneLevelFactor = saveData.OneLevelFactor.Value;
        if (saveData.TwoLevelFactor.HasValue)
            data.TwoLevelFactor = saveData.TwoLevelFactor.Value;
        if (saveData.ThreeLevelFactor.HasValue)
            data.ThreeLevelFactor = saveData.ThreeLevelFactor.Value;
        if (saveData.DestroyItemWhenBroken.HasValue)
            data.DestroyItemWhenBroken = saveData.DestroyItemWhenBroken.Value;

        entity.Write(data);
    }
}
