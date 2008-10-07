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

#if MONO_STRICT
namespace System.Data.Linq.Identity
#else
namespace DbLinq.Data.Linq.Identity
#endif
{
    /// <summary>
    /// Identifies an object in a unique way (think Primay Keys in a database table)
    /// Identity is:
    /// - A type
    /// - A collection 
    /// 
    /// Example: to store Product with ProductID=1, we use the following IdentityKey:
    ///  IdentityKey{Type=Product, Keys={1}}
    /// </summary>
#if MONO_STRICT
    internal
#else
    public
#endif
    class IdentityKey
    {
        /// <summary>
        /// Entity type
        /// </summary>
        public Type Type { get; private set; }
        /// <summary>
        /// Entity keys
        /// </summary>
        public IList<object> Keys { get; private set; }

        public override bool Equals(object obj)
        {
            var other = (IdentityKey)obj;
            if (Type != other.Type)
                return false;
            if (Keys.Count != other.Keys.Count)
                return false;
            for (int keyIndex = 0; keyIndex < Keys.Count; keyIndex++)
            {
                if (!Equals(Keys[keyIndex], other.Keys[keyIndex]))
                    return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            int hash = Type.GetHashCode();
            foreach (object key in Keys)
            {
                hash ^= key.GetHashCode();
            }
            return hash;
        }

        public IdentityKey(Type type, IEnumerable<object> keys)
        {
            Type = type;
            Keys = new List<object>(keys);
        }

        public IdentityKey(Type type, params object[] keys)
        {
            Type = type;
            Keys = new List<object>(keys);
        }
    }
}