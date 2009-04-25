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
    internal class DisabledEntityTracker : IEntityTracker
    {
        private static IEnumerable<EntityTrack> trackedEntities = new EntityTrack[] { };

        /// <summary>
        /// Finds entity by key (PK)
        /// </summary>
        /// <param name="identityKey"></param>
        /// <returns></returns>
        public EntityTrack FindByIdentity(IdentityKey identityKey)
        {
            return null;
        }

        /// <summary>
        /// Returns true if the list contains the entity
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool ContainsReference(object entity)
        {
            return false;
        }

        /// <summary>
        /// Registers an entity to be inserted
        /// </summary>
        /// <param name="entity"></param>
        public void RegisterToInsert(object entity)
        {
        }

        /// <summary>
        /// Registers an entity to be watched
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="identityKey"></param>
        public void RegisterToWatch(object entity, IdentityKey identityKey)
        {
        }

        /// <summary>
        /// Registers entity to be deleted
        /// </summary>
        /// <param name="entity"></param>
        public void RegisterToDelete(object entity)
        {
        }

        /// <summary>
        /// Unregisters the entity after deletion
        /// </summary>
        /// <param name="entity"></param>
        public void RegisterDeleted(object entity)
        {
        }

        /// <summary>
        /// Enumerates all registered entities
        /// </summary>
        /// <returns></returns>
        public IEnumerable<EntityTrack> EnumerateAll()
        {
            return trackedEntities;
        }
    }
}
