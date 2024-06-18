using ProjectM.CastleBuilding;
using System;
using System.Collections.Generic;
using Unity.Entities;

namespace KindredSchematics
{
    internal class EntityMapper
    {
        Dictionary<Entity, int> entityIndexLookup = [];
        List<Entity> entities = [];
        public EntityMapper()
        {
            entityIndexLookup = new Dictionary<Entity, int>();
            entities = new List<Entity>();
            AddEntity(Entity.Null);
        }

        public EntityMapper(IEnumerable<Entity> entitiesToAdd)
        {
            entityIndexLookup = new Dictionary<Entity, int>();
            entities = new List<Entity>();

            AddEntity(Entity.Null);

            foreach (var entity in entitiesToAdd)
                AddEntity(entity);
        }

        public int AddEntity(Entity entity)
        {
            if (entity.Has<Prefab>())
                throw new ArgumentException("Cannot add entities with Prefab component to EntityMapper");

            // For now get rid of CastleHeart entities
            if (entity.Has<CastleHeart>())
                return 0;

            if (!entityIndexLookup.TryGetValue(entity, out var index))
            {
                index = entities.Count;
                entityIndexLookup[entity] = index;
                entities.Add(entity);
            }
            return index;
        }

        public Entity this[int index] => entities[index];

        public int Count => entities.Count;

        public int IndexOf(Entity entity)
        {
            if (entityIndexLookup.TryGetValue(entity, out var index))
                return index;
            return AddEntity(entity);
        }

    }
}
