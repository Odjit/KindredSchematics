using KindredVignettes.Services;
using ProjectM;
using Stunlock.Core;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Unity.Entities;

namespace KindredVignettes.ComponentSaver
{
    [ComponentType(typeof(Refinementstation))]
    internal class Refinementstation_Saver : ComponentSaver
    {
        struct Refinementstation_Save
        {
            public double? RefiningStartTime { get; set; }
            public object InputInventory { get; set; }
            public object OutputInventory { get; set; }
            public PrefabGUID? CurrentRecipeGuid { get; set; }
            public int? Status { get; set; }
            public PrefabGUID? InventoryPrefabGuid { get; set; }
            public int? ActiveSequenceGuid { get; set; }
            public int? InactiveSequenceGuid { get; set; }
            public int? ActiveSequenceState { get; set; }
            public int? InactiveSequenceState { get; set; }
            public bool? IsWorking { get; set; }
        }

        public override object DiffComponents(Entity prefab, Entity entity, EntityMapper entityMapper)
        {
            var prefabData = prefab.Read<Refinementstation>();
            var entityData = entity.Read<Refinementstation>();

            var saveData = new Refinementstation_Save();
            if (prefabData.RefiningStartTime != entityData.RefiningStartTime && saveData.RefiningStartTime >= 0)
                saveData.RefiningStartTime -= Core.ServerTime;
            if (!entityData.InputInventoryEntity.Equals(Entity.Null))
                saveData.InputInventory = new InventoryBuffer_Saver().SaveComponent(entityData.InputInventoryEntity.GetEntityOnServer(), entityMapper);
            if (!entityData.OutputInventoryEntity.Equals(Entity.Null))
                saveData.OutputInventory = new InventoryBuffer_Saver().SaveComponent(entityData.OutputInventoryEntity.GetEntityOnServer(), entityMapper);
            if (prefabData.CurrentRecipeGuid != entityData.CurrentRecipeGuid)
                saveData.CurrentRecipeGuid = entityData.CurrentRecipeGuid;
            if (!prefabData.Status.Equals(entityData.Status))
                saveData.Status = (int)entityData.Status;
            if (prefabData.InventoryPrefabGuid != entityData.InventoryPrefabGuid)
                saveData.InventoryPrefabGuid = entityData.InventoryPrefabGuid;
            if (prefabData.ActiveSequenceGuid != entityData.ActiveSequenceGuid)
                saveData.ActiveSequenceGuid = entityData.ActiveSequenceGuid.GuidHash;
            if (prefabData.InactiveSequenceGuid != entityData.InactiveSequenceGuid)
                saveData.InactiveSequenceGuid = entityData.InactiveSequenceGuid.GuidHash;
            if (!prefabData.ActiveSequenceState.Equals(entityData.ActiveSequenceState))
                saveData.ActiveSequenceState = entityMapper.IndexOf(entityData.ActiveSequenceState.Id);
            if (!prefabData.InactiveSequenceState.Equals(entityData.InactiveSequenceState))
                saveData.InactiveSequenceState = entityMapper.IndexOf(entityData.InactiveSequenceState.Id);
            if (prefabData.IsWorking != entityData.IsWorking)
                saveData.IsWorking = entityData.IsWorking;

            if (saveData.Equals(default(Refinementstation_Save)))
                return null;

            return saveData;
        }

        public override object SaveComponent(Entity entity, EntityMapper entityMapper)
        {
            var data = entity.Read<Refinementstation>();
            var saveData = new Refinementstation_Save();

            saveData.RefiningStartTime = data.RefiningStartTime;
            if (saveData.RefiningStartTime >= 0)
                saveData.RefiningStartTime -= Core.ServerTime;
            if (!data.InputInventoryEntity.Equals(Entity.Null))
                saveData.InputInventory = new InventoryBuffer_Saver().SaveComponent(data.InputInventoryEntity.GetEntityOnServer(), entityMapper);
            if (!data.OutputInventoryEntity.Equals(Entity.Null))
                saveData.OutputInventory = new InventoryBuffer_Saver().SaveComponent(data.OutputInventoryEntity.GetEntityOnServer(), entityMapper);
            saveData.CurrentRecipeGuid = data.CurrentRecipeGuid;
            saveData.Status = (int)data.Status;
            saveData.InventoryPrefabGuid = data.InventoryPrefabGuid;
            saveData.ActiveSequenceGuid = data.ActiveSequenceGuid.GuidHash;
            saveData.InactiveSequenceGuid = data.InactiveSequenceGuid.GuidHash;
            saveData.ActiveSequenceState = entityMapper.IndexOf(data.ActiveSequenceState.Id);
            saveData.InactiveSequenceState = entityMapper.IndexOf(data.InactiveSequenceState.Id);
            saveData.IsWorking = data.IsWorking;

            return saveData;
        }

        public override void ApplyComponentData(Entity entity, JsonElement jsonData, Entity[] entitiesBeingLoaded)
        {
            var saveData = jsonData.Deserialize<Refinementstation_Save>(VignetteService.GetJsonOptions());

            if (!entity.Has<Refinementstation>())
                entity.Add<Refinementstation>();

            var data = entity.Read<Refinementstation>();

            if (saveData.RefiningStartTime.HasValue)
                data.RefiningStartTime = saveData.RefiningStartTime.Value + Core.ServerTime;
            if (saveData.CurrentRecipeGuid.HasValue)
                data.CurrentRecipeGuid = saveData.CurrentRecipeGuid.Value;
            if (saveData.Status.HasValue)
                data.Status = (RefinementStatus)saveData.Status.Value;
            if (saveData.InventoryPrefabGuid.HasValue)
                data.InventoryPrefabGuid = saveData.InventoryPrefabGuid.Value;
            if (saveData.ActiveSequenceGuid.HasValue)
                data.ActiveSequenceGuid = new SequenceGUID() { GuidHash = saveData.ActiveSequenceGuid.Value };
            if (saveData.InactiveSequenceGuid.HasValue)
                data.InactiveSequenceGuid = new SequenceGUID() { GuidHash = saveData.InactiveSequenceGuid.Value };
            if (saveData.ActiveSequenceState.HasValue)
                data.ActiveSequenceState = new SequenceState() { Id = entitiesBeingLoaded[saveData.ActiveSequenceState.Value] };
            if (saveData.InactiveSequenceState.HasValue)
                data.InactiveSequenceState = new SequenceState() { Id = entitiesBeingLoaded[saveData.InactiveSequenceState.Value] };
            if (saveData.IsWorking.HasValue)
                data.IsWorking = saveData.IsWorking.Value;

            Core.VignetteService.StartCoroutine(DelayAddInventory(entity, saveData, entitiesBeingLoaded));

            entity.Write(data);
        }

        IEnumerator DelayAddInventory(Entity entity, Refinementstation_Save saveData, Entity[] entitiesBeingLoaded)
        {
            var loadedInputInventory = saveData.InputInventory == null;
            var loadedOutputInventory = saveData.OutputInventory == null;
            while(!loadedInputInventory && !loadedOutputInventory)
            {
                var data = entity.Read<Refinementstation>();
                if (!loadedInputInventory && !data.InputInventoryEntity.Equals(NetworkedEntity.Empty))
                {
                    new InventoryBuffer_Saver().ApplyComponentData(data.InputInventoryEntity.GetEntityOnServer(), (JsonElement)saveData.InputInventory, entitiesBeingLoaded);
                    loadedInputInventory = true;
                }
                if(!loadedOutputInventory && !data.OutputInventoryEntity.Equals(NetworkedEntity.Empty))
                {
                    new InventoryBuffer_Saver().ApplyComponentData(data.OutputInventoryEntity.GetEntityOnServer(), (JsonElement)saveData.OutputInventory, entitiesBeingLoaded);
                    loadedOutputInventory = true;
                }
                yield return null;
            }
        }

        public override int[] GetDependencies(JsonElement data)
        {
            var saveData = data.Deserialize<Refinementstation_Save>(VignetteService.GetJsonOptions());
            var dependencies = new List<int>();

            if (saveData.InputInventory != null)
                dependencies.AddRange(new InventoryBuffer_Saver().GetDependencies((JsonElement)saveData.InputInventory));
            if (saveData.OutputInventory != null)
                dependencies.AddRange(new InventoryBuffer_Saver().GetDependencies((JsonElement)saveData.OutputInventory));

            if (saveData.ActiveSequenceState.HasValue)
                dependencies.Add(saveData.ActiveSequenceState.Value);
            if (saveData.InactiveSequenceState.HasValue)
                dependencies.Add(saveData.InactiveSequenceState.Value);

            return dependencies.Where(x => x != 0).ToArray();
        }
    }
}
