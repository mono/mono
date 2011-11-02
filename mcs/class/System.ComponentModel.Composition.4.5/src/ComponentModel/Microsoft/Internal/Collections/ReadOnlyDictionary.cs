// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Internal;

// This is using the desktop namespace for ReadOnlyDictionary, the source code is in Microsoft\Internal\Collections to keep it seperate from the main MEF codebase.
namespace System.Collections.ObjectModel
{
    
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(ReadOnlyDictionaryDebuggerProxy<,>))]
    internal sealed partial class ReadOnlyDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private readonly IDictionary<TKey, TValue> _innerDictionary;

        public ReadOnlyDictionary(IDictionary<TKey, TValue> dictionary)
        {
            this._innerDictionary = dictionary ?? new Dictionary<TKey, TValue>(0);
        }

        public int Count
        {
            get { return this._innerDictionary.Count; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public ICollection<TKey> Keys
        {
            get { return this._innerDictionary.Keys; }
        }

        public TValue this[TKey key]
        {
            get { return this._innerDictionary[key]; }
            set { throw new NotSupportedException(Strings.NotSupportedReadOnlyDictionary); }
        }

        public ICollection<TValue> Values
        {
            get { return this._innerDictionary.Values; }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return this._innerDictionary.Contains(item);
        }

        public bool ContainsKey(TKey key)
        {
            return this._innerDictionary.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            this._innerDictionary.CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return this._innerDictionary.GetEnumerator();
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return this._innerDictionary.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._innerDictionary.GetEnumerator();
        }

        void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
        {
            throw new NotSupportedException(Strings.NotSupportedReadOnlyDictionary);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            throw new NotSupportedException(Strings.NotSupportedReadOnlyDictionary);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Clear()
        {
            throw new NotSupportedException(Strings.NotSupportedReadOnlyDictionary);
        }

        bool IDictionary<TKey, TValue>.Remove(TKey key)
        {
            throw new NotSupportedException(Strings.NotSupportedReadOnlyDictionary);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new NotSupportedException(Strings.NotSupportedReadOnlyDictionary);
        }
    }
}
