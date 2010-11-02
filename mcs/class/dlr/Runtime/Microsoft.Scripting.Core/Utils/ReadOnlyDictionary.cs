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

#if CLR2
using Microsoft.Scripting.Ast;
#else
using System.Linq.Expressions;
#endif
#if SILVERLIGHT
using System.Core;
#endif

using System.Collections.Generic;

namespace System.Dynamic.Utils {

    // Like ReadOnlyCollection<T>: wraps an IDictionary<K, V> in a read-only wrapper
    internal sealed class ReadOnlyDictionary<K, V> : IDictionary<K, V> {

        // For wrapping non-readonly Keys, Values collections
        // Not used for standard dictionaries, which return read-only Keys and Values
        private sealed class ReadOnlyWrapper<T> : ICollection<T> {
            // no idea why this warning is here
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
            private readonly ICollection<T> _collection;
            
            internal ReadOnlyWrapper(ICollection<T> collection) {
                _collection = collection;
            }

            #region ICollection<T> Members

            public void Add(T item) {
                throw Error.CollectionReadOnly();
            }

            public void Clear() {
                throw Error.CollectionReadOnly();
            }

            public bool Contains(T item) {
                return _collection.Contains(item);
            }

            public void CopyTo(T[] array, int arrayIndex) {
                _collection.CopyTo(array, arrayIndex);
            }

            public int Count {
                get { return _collection.Count; }
            }

            public bool IsReadOnly {
                get { return true; }
            }

            public bool Remove(T item) {
                throw Error.CollectionReadOnly();
            }

            #endregion

            #region IEnumerable<T> Members

            public IEnumerator<T> GetEnumerator() {
                return _collection.GetEnumerator();
            }

            #endregion

            #region IEnumerable Members

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
                return _collection.GetEnumerator();
            }

            #endregion
        }

        private readonly IDictionary<K, V> _dict;

        internal ReadOnlyDictionary(IDictionary<K, V> dict) {
            ReadOnlyDictionary<K, V> rodict = dict as ReadOnlyDictionary<K, V>;
            _dict = (rodict != null) ? rodict._dict : dict;
        }

        #region IDictionary<K,V> Members

        public bool ContainsKey(K key) {
            return _dict.ContainsKey(key);
        }

        public ICollection<K> Keys {
            get {
                ICollection<K> keys = _dict.Keys;
                if (!keys.IsReadOnly) {
                    return new ReadOnlyWrapper<K>(keys);
                }
                return keys;
            }
        }

        public bool TryGetValue(K key, out V value) {
            return _dict.TryGetValue(key, out value);
        }

        public ICollection<V> Values {
            get {
                ICollection<V> values = _dict.Values;
                if (!values.IsReadOnly) {
                    return new ReadOnlyWrapper<V>(values);
                }
                return values;
            }
        }

        public V this[K key] {
            get {
                return _dict[key];
            }
        }


        void IDictionary<K, V>.Add(K key, V value) {
            throw Error.CollectionReadOnly();
        }

        bool IDictionary<K, V>.Remove(K key) {
            throw Error.CollectionReadOnly();
        }

        V IDictionary<K, V>.this[K key] {
            get {
                return _dict[key];
            }
            set {
                throw Error.CollectionReadOnly();
            }
        }

        #endregion

        #region ICollection<KeyValuePair<K,V>> Members

        public bool Contains(KeyValuePair<K, V> item) {
            return _dict.Contains(item);
        }

        public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex) {
            _dict.CopyTo(array, arrayIndex);
        }

        public int Count {
            get { return _dict.Count; }
        }

        public bool IsReadOnly {
            get { return true; }
        }

        void ICollection<KeyValuePair<K, V>>.Add(KeyValuePair<K, V> item) {
            throw Error.CollectionReadOnly();
        }

        void ICollection<KeyValuePair<K, V>>.Clear() {
            throw Error.CollectionReadOnly();
        }

        bool ICollection<KeyValuePair<K,V>>.Remove(KeyValuePair<K, V> item) {
            throw Error.CollectionReadOnly();
        }

        #endregion

        #region IEnumerable<KeyValuePair<K,V>> Members

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator() {
            return _dict.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return _dict.GetEnumerator();
        }

        #endregion
    }
}
