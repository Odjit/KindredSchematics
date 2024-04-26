﻿using KindredVignettes.Services;
using ProjectM;
using System.Collections;
using System.Text.Json;
using Unity.Entities;
using UnityEngine;

namespace KindredVignettes.ComponentSaver
{
    [ComponentType(typeof(Health))]
    internal class Health_Saver : ComponentSaver
    {
        struct Health_Save
        {
            public float? MaxHealth { get; set; }
            public double? TimeOfDeath { get; set; }
            public float? Value { get; set; }
            public float? MaxRecoveryHealth { get; set; }
            public bool? IsDead { get; set; }

        }

        public override object DiffComponents(Entity prefab, Entity entity, EntityMapper entityMapper)
        {
            var prefabData = prefab.Read<Health>();
            var entityData = entity.Read<Health>();

            var saveData = new Health_Save();
            if (prefabData.MaxHealth != entityData.MaxHealth)
                saveData.MaxHealth = entityData.MaxHealth;
            if (prefabData.TimeOfDeath != entityData.TimeOfDeath)
                saveData.TimeOfDeath = entityData.TimeOfDeath;
            if (prefabData.Value != entityData.Value)
                saveData.Value = entityData.Value;
            if (prefabData.MaxRecoveryHealth != entityData.MaxRecoveryHealth)
                saveData.MaxRecoveryHealth = entityData.MaxRecoveryHealth;
            if (prefabData.IsDead != entityData.IsDead)
                saveData.IsDead = entityData.IsDead;

            if (saveData.Equals(default(Health_Save)))
                return null;

            return saveData;
        }

        public override object SaveComponent(Entity entity, EntityMapper entityMapper)
        {
            var data = entity.Read<Health>();

            var saveData = new Health_Save()
            {
                MaxHealth = data.MaxHealth,
                TimeOfDeath = data.TimeOfDeath,
                Value = data.Value,
                MaxRecoveryHealth = data.MaxRecoveryHealth,
                IsDead = data.IsDead
            };

            return saveData;
        }

        public override void ApplyComponentData(Entity entity, JsonElement jsonData, Entity[] entitiesBeingLoaded)
        {
            var saveData = jsonData.Deserialize<Health_Save>(VignetteService.GetJsonOptions());

            if (!entity.Has<Health>())
                entity.Add<Health>();

            Core.VignetteService.StartCoroutine(DelayLoad());

            IEnumerator DelayLoad()
            {
                yield return new WaitForSeconds(1f);

                var data = entity.Read<Health>();

                if (saveData.MaxHealth.HasValue)
                    data.MaxHealth.Value = saveData.MaxHealth.Value;
                if (saveData.TimeOfDeath.HasValue)
                    data.TimeOfDeath = saveData.TimeOfDeath.Value;
                if (saveData.Value.HasValue)
                    data.Value = saveData.Value.Value;
                if (saveData.MaxRecoveryHealth.HasValue)
                    data.MaxRecoveryHealth = saveData.MaxRecoveryHealth.Value;
                if (saveData.IsDead.HasValue)
                    data.IsDead = saveData.IsDead.Value;

                entity.Write(data);
            }
        }
    }
}
