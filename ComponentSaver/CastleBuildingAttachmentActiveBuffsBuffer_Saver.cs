using KindredVignettes.Services;
using ProjectM;
using ProjectM.CastleBuilding;
using System.Text.Json;
using Unity.Entities;

namespace KindredVignettes.ComponentSaver
{
    [ComponentType(typeof(CastleBuildingAttachmentActiveBuffsBuffer))]
    internal class CastleBuildingAttachmentActiveBuffsBuffer_Saver : ComponentSaver
    {
        struct SaveData
        {
            public int ParentEntityId { get; set; }
            public int ChildEntityId { get; set; }
            public PrefabGUID Buff { get; set; }
        }

        public override object DiffComponents(Entity prefab, Entity entity, EntityMapper entityMapper)
        {
            return SaveComponent(entity, entityMapper);
        }

        public override object SaveComponent(Entity entity, EntityMapper entityMapper)
        {
            var buffer = Core.EntityManager.GetBuffer<CastleBuildingAttachmentActiveBuffsBuffer>(entity);
            var saveData = new SaveData[buffer.Length];
            for (int i = 0; i < buffer.Length; i++)
            {
                saveData[i].ParentEntityId = entityMapper.IndexOf(buffer[i].ParentEntity);
                saveData[i].ChildEntityId = entityMapper.IndexOf(buffer[i].ChildEntity);
                saveData[i].Buff = buffer[i].BuffEntity.Read<PrefabGUID>();
            }

            return saveData;
        }

        public override void ApplyComponentData(Entity entity, JsonElement data, Entity[] entitiesBeingLoaded)
        {
            DynamicBuffer<CastleBuildingAttachmentActiveBuffsBuffer> buffer;
            if (entity.Has<CastleBuildingAttachmentActiveBuffsBuffer>())
                buffer = Core.EntityManager.GetBuffer<CastleBuildingAttachmentActiveBuffsBuffer>(entity);
            else
                buffer = Core.EntityManager.AddBuffer<CastleBuildingAttachmentActiveBuffsBuffer>(entity);
            buffer.Clear();

            var applyBuffBuffer = Core.EntityManager.GetBuffer<CastleBuildingAttachmentApplyBuff>(entity);

            var saveData = data.Deserialize<SaveData[]>(VignetteService.GetJsonOptions());
            foreach (var entry in saveData)
            {
                CastleBuildingAttachmentApplyBuff applyBuff = default(CastleBuildingAttachmentApplyBuff);
                foreach(var item in applyBuffBuffer)
                {
                    if(item.BuffPrefab.Equals(entry.Buff))
                    {
                        applyBuff = item;
                        break;
                    }
                }

                if (applyBuff.Equals(default(CastleBuildingAttachmentApplyBuff)))
                    continue;

                var eventEntity = Core.EntityManager.CreateEntity(ComponentType.ReadWrite<CastleBuildingAttachmentAddedEvent>());
                eventEntity.Write(new CastleBuildingAttachmentAddedEvent()
                {
                    AttachTo = applyBuff.WhenMatchesTypes,
                    PlacementTypes = applyBuff.WhenMatchesTypes,
                    ChildEntity = entitiesBeingLoaded[entry.ChildEntityId],
                    ParentEntity = entitiesBeingLoaded[entry.ParentEntityId]
                });
            }
        }
    }
}
