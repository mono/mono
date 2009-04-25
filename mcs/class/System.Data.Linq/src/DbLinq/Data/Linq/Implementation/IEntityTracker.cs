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
    /// Interface of Entity Trackers
    /// </summary>
    internal interface IEntityTracker
    {
        /// <summary>
        /// Finds entity by key (PK)
        /// </summary>
        /// <param name="identityKey"></param>
        /// <returns></returns>
        EntityTrack FindByIdentity(IdentityKey identityKey);

        /// <summary>
        /// Returns true if the list contains the entity
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        bool ContainsReference(object entity);

        /// <summary>
        /// Registers an entity to be inserted
        /// </summary>
        /// <param name="entity"></param>
        void RegisterToInsert(object entity);

        /// <summary>
        /// Registers an entity to be watched
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="identityKey"></param>
        void RegisterToWatch(object entity, IdentityKey identityKey);

        /// <summary>
        /// Registers entity to be deleted
        /// </summary>
        /// <param name="entity"></param>
        void RegisterToDelete(object entity);

        /// <summary>
        /// Unregisters the entity after deletion
        /// </summary>
        /// <param name="entity"></param>
        void RegisterDeleted(object entity);

        /// <summary>
        /// Enumerates all registered entities
        /// </summary>
        /// <returns></returns>
        IEnumerable<EntityTrack> EnumerateAll();
    }
}
