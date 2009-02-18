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
namespace System.Data.Linq
#else
namespace DbLinq.Data.Linq
#endif
{
    /// <summary>
    /// The class helps keeping a list of entities, sorted by type, then by list (of same type)
    /// </summary>
    internal class EntityList
    {
        private class EntityByType
        {
            public readonly object Entity;
            public readonly Type Type;

            public EntityByType(object entity, Type type)
            {
                Entity = entity;
                Type = type;
            }
        }

        private readonly List<EntityByType> _entitiesByType = new List<EntityByType>();
        private readonly object _lock = new object();

        /// <summary>
        /// Enumerates all entites
        /// </summary>
        /// <returns></returns>
        public IEnumerable<object> EnumerateAll()
        {
            lock (_lock)
            {
                // do you love linq's extension methods? I do.
                return _entitiesByType.Select(e => e.Entity);
            }
        }

        /// <summary>
        /// Enumerates all items with the same type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IEnumerable<T> Enumerate<T>()
        {
            lock (_lock)
            {
                // you too, have fun with linq
                return from e in _entitiesByType
                       where e.Type == typeof(T)
                       select (T)e.Entity;
            }
        }

        /// <summary>
        /// Adds an item
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        public void Add<T>(T t)
        {
            Add(t, typeof(T));
        }

        /// <summary>
        /// Adds an entity of a given type
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="asType"></param>
        public void Add(object entity, Type asType)
        {
            lock (_lock)
            {
                if (!_entitiesByType.Any(e => e.Entity == entity))
                    _entitiesByType.Add(new EntityByType(entity, asType));
            }
        }

        /// <summary>
        /// Removes an item (and don't care if missing)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        public void Remove<T>(T t)
        {
            Remove(t, typeof(T));
        }

        /// <summary>
        /// Removes an entity, declared as type
        /// This method is O(n), use with caution
        /// </summary>
        /// <param name="e"></param>
        /// <param name="asType"></param>
        public void Remove(object e, Type asType)
        {
            lock (_lock)
            {
                for (int entityIndex = 0; entityIndex < _entitiesByType.Count; entityIndex++)
                {
                    if (_entitiesByType[entityIndex].Entity == e)
                    {
                        _entitiesByType.RemoveAt(entityIndex);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Remove items
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ts"></param>
        public void RemoveRange<T>(IEnumerable<T> ts)
        {
            lock (_lock)
            {
                foreach (var t in ts)
                    Remove(t);
            }
        }

        /// <summary>
        /// Returns the number of items given a type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public int Count<T>()
        {
            lock (_lock)
            {
                return _entitiesByType.Count(e => e.Type == typeof(T));
            }
        }

        /// <summary>
        /// Determines if the given entity is registered
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public bool Contains<T>(T t)
        {
            return Contains(t, typeof(T));
        }

        /// <summary>
        /// Returns true if the entity is already present in list
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="asType"></param>
        /// <returns></returns>
        public bool Contains(object entity, Type asType)
        {
            lock (_lock)
            {
                return _entitiesByType.Any(e => e.Entity == entity);
            }
        }

    }
}
