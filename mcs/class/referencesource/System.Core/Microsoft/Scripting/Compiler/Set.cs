/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System.Collections;
using System.Collections.Generic;

// Note: can't move to Utils because name conflicts with System.Linq.Set
#if CLR2
namespace Microsoft.Scripting.Ast {
#else
namespace System.Linq.Expressions {
#endif

    /// <summary>
    /// A simple hashset, built on Dictionary{K, V}
    /// </summary>
    internal sealed class Set<T> : ICollection<T> {
        private readonly Dictionary<T, object> _data;

        internal Set() {
            _data = new Dictionary<T, object>();
        }

        internal Set(IEqualityComparer<T> comparer) {
            _data = new Dictionary<T, object>(comparer);
        }

        internal Set(IList<T> list) {
            _data = new Dictionary<T, object>(list.Count);
            foreach (T t in list) {
                Add(t);
            }
        }

        internal Set(IEnumerable<T> list) {
            _data = new Dictionary<T, object>();
            foreach (T t in list) {
                Add(t);
            }
        }

        internal Set(int capacity) {
            _data = new Dictionary<T, object>(capacity);
        }

        public void Add(T item) {
            _data[item] = null;
        }

        public void Clear() {
            _data.Clear();
        }

        public bool Contains(T item) {
            return _data.ContainsKey(item);
        }

        public void CopyTo(T[] array, int arrayIndex) {
            _data.Keys.CopyTo(array, arrayIndex);
        }

        public int Count {
            get { return _data.Count; }
        }

        public bool IsReadOnly {
            get { return false; }
        }

        public bool Remove(T item) {
            return _data.Remove(item);
        }

        public IEnumerator<T> GetEnumerator() {
            return _data.Keys.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return _data.Keys.GetEnumerator();
        }
    }
}
