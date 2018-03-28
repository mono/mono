//------------------------------------------------------------------------------
// <copyright file="OrderedDictionary.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.Util {
    using System;
    using System.Collections;
    using System.Collections.Generic;

    // This is different from the BCL's SortedDictionary in that SortedDictionary sorts by the keys'
    // values (e.g., alphabetical order), but OrderedDictionary sorts by the order in which the keys
    // were inserted into the dictionary.

    internal class OrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue> {
        private Dictionary<TKey, TValue> _dictionary;
        private List<TKey> _keys;
        private List<TValue> _values;

        // Cannot easily support ctor that takes IEqualityComparer, since List doesn't have an easy
        // way to use the IEqualityComparer.
        public OrderedDictionary()
            : this(0) {
        }

        public OrderedDictionary(int capacity) {
            _dictionary = new Dictionary<TKey, TValue>(capacity);
            _keys = new List<TKey>(capacity);
            _values = new List<TValue>(capacity);
        }

        public int Count {
            get {
                return _dictionary.Count;
            }
        }

        public ICollection<TKey> Keys {
            get {
                return _keys.AsReadOnly();
            }
        }

        public TValue this[TKey key] {
            get {
                return _dictionary[key];
            }
            set {
                // If key has already been added, we must first remove it from the lists so it is not
                // in the lists multiple times.
                RemoveFromLists(key);

                _dictionary[key] = value;
                _keys.Add(key);
                _values.Add(value);
            }
        }

        public ICollection<TValue> Values {
            get {
                return _values.AsReadOnly();
            }
        }

        public void Add(TKey key, TValue value) {
            // Dictionary.Add() will throw if it already contains key
            _dictionary.Add(key, value);
            _keys.Add(key);
            _values.Add(value);
        }

        public void Clear() {
            _dictionary.Clear();
            _keys.Clear();
            _values.Clear();
        }

        public bool ContainsKey(TKey key) {
            return _dictionary.ContainsKey(key);
        }

        public bool ContainsValue(TValue value) {
            return _dictionary.ContainsValue(value);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
            int i = 0;
            // Must use foreach instead of a for loop, since we want the underlying List enumerator to
            // throw an exception if the list is modified during enumeration.
            foreach (TKey key in _keys) {
                yield return new KeyValuePair<TKey, TValue>(key, _values[i]);
                i++;
            }
        }

        private void RemoveFromLists(TKey key) {
            int index = _keys.IndexOf(key);
            if (index != -1) {
                _keys.RemoveAt(index);
                _values.RemoveAt(index);
            }
        }

        public bool Remove(TKey key) {
            RemoveFromLists(key);
            return _dictionary.Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value) {
            return _dictionary.TryGetValue(key, out value);
        }

        #region ICollection<KeyValuePair<TKey,TValue>> Members
        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly {
            get {
                return ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).IsReadOnly;
            }
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) {
            Add(item.Key, item.Value);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item) {
            return ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).Contains(item);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
            ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).CopyTo(array, arrayIndex);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) {
            bool removed = ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).Remove(item);

            // Only remove from lists if it was removed from the dictionary, since the dictionary may contain
            // the key but not the value.
            if (removed) {
                RemoveFromLists(item.Key);
            }

            return removed;
        }
        #endregion

        #region IEnumerable Members
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
        #endregion
    }
}
