#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;

#if MONO_STRICT
using System.Data.Linq.Identity;
#else
using DbLinq.Data.Linq.Identity;
#endif

#if MONO_STRICT
namespace System.Data.Linq.Implementation
#else
namespace DbLinq.Data.Linq.Implementation
#endif
{
    /// <summary>
    /// List of entities, with their corresponding state (to insert, to watch, to delete)
    /// </summary>
    internal class EntityTracker : IEntityTracker
    {
        /// <summary>
        /// Entities being watched
        /// </summary>
        private readonly List<EntityTrack> entities = new List<EntityTrack>();

        /// <summary>
        /// Entities currently being watched and to be updated
        /// </summary>
        private readonly IDictionary<IdentityKey, EntityTrack> entitiesByKey = new Dictionary<IdentityKey, EntityTrack>();

        /// <summary>
        /// Finds an entity tracking info by object reference
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private EntityTrack FindByReference(object entity)
        {
            //    return (from e in entities where e.Entity == entity select e).FirstOrDefault();
            return this.entities.Find(e => object.ReferenceEquals(entity, e.Entity));
        }

        /// <summary>
        /// Finds entity by key (PK)
        /// </summary>
        /// <param name="identityKey"></param>
        /// <returns></returns>
        public EntityTrack FindByIdentity(IdentityKey identityKey)
        {
            EntityTrack entityTrack;
            entitiesByKey.TryGetValue(identityKey, out entityTrack);
            return entityTrack;
        }

        /// <summary>
        /// Returns true if the list contains the entity
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool ContainsReference(object entity)
        {
            return FindByReference(entity) != null;
        }

        /// <summary>
        /// Registers an entity to be inserted
        /// </summary>
        /// <param name="entity"></param>
        public void RegisterToInsert(object entity)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            var entityTrack = FindByReference(entity);
            if (entityTrack == null)
            {
                entityTrack = new EntityTrack(entity, EntityState.ToInsert);
                entities.Add(entityTrack);
            }
            else
            {
                switch (entityTrack.EntityState)
                {
                // if already registered for insert/update, then this is an error
                case EntityState.ToInsert:
                case EntityState.ToWatch:
                    throw new InvalidOperationException();
                // whenever the object is registered for deletion, the fact of
                // registering it for insertion sets it back to watch
                case EntityState.ToDelete:
                    entityTrack.EntityState = EntityState.ToWatch;
                    entitiesByKey[entityTrack.IdentityKey] = entityTrack;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// Registers an entity to be watched
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="identityKey"></param>
        public void RegisterToWatch(object entity, IdentityKey identityKey)
        {
            var entityTrack = FindByReference(entity);
            if (entityTrack == null)
            {
                entityTrack = new EntityTrack(entity, EntityState.ToWatch) { IdentityKey = identityKey };
                entities.Add(entityTrack);
                entitiesByKey[identityKey] = entityTrack;
            }
            else
            {
                // changes the state of the current entity
                switch (entityTrack.EntityState)
                {
                case EntityState.ToInsert:
                    entityTrack.EntityState = EntityState.ToWatch;
                    entityTrack.IdentityKey = identityKey;
                    entitiesByKey[identityKey] = entityTrack;
                    break;
                // watched entities should not be registered again
                case EntityState.ToWatch:
                case EntityState.ToDelete:
                    throw new InvalidOperationException();
                default:
                    throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// Registers entity to be deleted
        /// </summary>
        /// <param name="entity"></param>
        public void RegisterToDelete(object entity)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            var entityTrack = FindByReference(entity);
            if (entityTrack == null)
            {
                entityTrack = new EntityTrack(entity, EntityState.ToDelete);
                entities.Add(entityTrack);
            }
            else
            {
                // changes the state of the current entity
                switch (entityTrack.EntityState)
                {
                // if entity was to be inserted, we just remove it from the list
                // as if it never came here
                case EntityState.ToInsert:
                    entities.Remove(entityTrack);
                    break;
                // watched entities are registered to be removed
                case EntityState.ToWatch:
                    entityTrack.EntityState = EntityState.ToDelete;
                    entitiesByKey.Remove(entityTrack.IdentityKey);
                    break;
                case EntityState.ToDelete:
                    throw new InvalidOperationException();
                default:
                    throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// Unregisters the entity after deletion
        /// </summary>
        /// <param name="entity"></param>
        public void RegisterDeleted(object entity)
        {
            // TODO: we could require an index
            var entityTrack = FindByReference(entity);
            if (entityTrack == null)
            {
                throw new ArgumentException("entity");
            }
            // changes the state of the current entity
            switch (entityTrack.EntityState)
            {
            case EntityState.ToDelete:
                entities.Remove(entityTrack);
                break;
            case EntityState.ToInsert:
            case EntityState.ToWatch:
                throw new InvalidOperationException();
            default:
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Enumerates all registered entities
        /// </summary>
        /// <returns></returns>
        public IEnumerable<EntityTrack> EnumerateAll()
        {
            return entities;
        }
    }
}
