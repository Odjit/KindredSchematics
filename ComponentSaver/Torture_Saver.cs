using KindredVignettes.Services;
using ProjectM;
using System.Collections;
using System.Text.Json;
using Unity.Entities;
using UnityEngine;

namespace KindredVignettes.ComponentSaver
{
    [ComponentType(typeof(Torture))]
    internal class Torture_Saver : ComponentSaver
    {
        struct Torture_Save
        {
            public float? TortureModifier { get; set; }
            public float? TorturePerDamage { get; set; }

        }

        public override object DiffComponents(Entity prefab, Entity entity, EntityMapper entityMapper)
        {
            var prefabData = prefab.Read<Torture>();
            var entityData = entity.Read<Torture>();

            var saveData = new Torture_Save();
            if (prefabData.TortureModifier != entityData.TortureModifier)
                saveData.TortureModifier = entityData.TortureModifier;
            if (prefabData.TorturePerDamage != entityData.TorturePerDamage)
                saveData.TorturePerDamage = entityData.TorturePerDamage;

            if (saveData.Equals(default(Torture_Save)))
                return null;

            return saveData;
        }

        public override object SaveComponent(Entity entity, EntityMapper entityMapper)
        {
            var data = entity.Read<Torture>();

            var saveData = new Torture_Save()
            {
                TortureModifier = data.TortureModifier,
                TorturePerDamage = data.TorturePerDamage,
            };

            return saveData;
        }

        public override void ApplyComponentData(Entity entity, JsonElement jsonData, Entity[] entitiesBeingLoaded)
        {
            var saveData = jsonData.Deserialize<Torture_Save>(VignetteService.GetJsonOptions());

            if (!entity.Has<Torture>())
                entity.Add<Torture>();

            Core.VignetteService.StartCoroutine(DelayLoad());

            IEnumerator DelayLoad()
            {
                yield return new WaitForSeconds(1f);
                var data = entity.Read<Torture>();

                if (saveData.TortureModifier.HasValue)
                    data.TortureModifier = saveData.TortureModifier.Value;
                if (saveData.TorturePerDamage.HasValue)
                    data.TorturePerDamage = saveData.TorturePerDamage.Value;

                entity.Write(data);
            }
        }
    }
}
