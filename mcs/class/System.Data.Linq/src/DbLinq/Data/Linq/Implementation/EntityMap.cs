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
using System.Text;
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
    /// this is the 'live object cache'
    /// </summary>
    internal class EntityMap : IEntityMap
    {
        private readonly IDictionary<IdentityKey, object> entities = new Dictionary<IdentityKey, object>();

        public IEnumerable<IdentityKey> Keys
        {
            get
            {
                lock (entities)
                    return entities.Keys;
            }
        }

        /// <summary>
        /// lookup or store an object in the 'live object cache'.
        /// Example:
        /// To store Product with ProductID=1, we use the following IdentityKey:
        ///  IdentityKey{Type=Product, Keys={1}}
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object this[IdentityKey key]
        {
            get
            {
                object o;
                lock (entities)
                    entities.TryGetValue(key, out o);
                return o;
            }
            set
            {
                lock (entities)
                    entities[key] = value;
            }
        }

        public void Remove(IdentityKey key)
        {
            lock (entities)
                entities.Remove(key);
        }
    }
}