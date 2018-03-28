//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.Runtime
{
    using System.Collections;
    using System.Collections.Generic;

    // This class is for back-compat with 4.0, where we exposed read-only dictionaries that threw
    // InvalidOperation if mutated. Any new usages should use the CLR's public ReadOnlyDictionary
    // (which throws NotSupported).
    [Serializable]
    class ReadOnlyDictionaryInternal<TKey, TValue> : IDictionary<TKey, TValue>
    {
        IDictionary<TKey, TValue> dictionary;

        public ReadOnlyDictionaryInternal(IDictionary<TKey, TValue> dictionary)
        {
            this.dictionary = dictionary;
        }

        public int Count
        {
            get { return this.dictionary.Count; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public ICollection<TKey> Keys
        {
            get { return this.dictionary.Keys; }
        }

        public ICollection<TValue> Values
        {
            get { return this.dictionary.Values; }
        }

        public TValue this[TKey key]
        {
            get
            {
                return this.dictionary[key];
            }
            set
            {
                throw Fx.Exception.AsError(CreateReadOnlyException());
            }
        }

        public static IDictionary<TKey, TValue> Create(IDictionary<TKey, TValue> dictionary)
        {
            if (dictionary.IsReadOnly)
            {
                return dictionary;
            }
            else
            {
                return new ReadOnlyDictionaryInternal<TKey, TValue>(dictionary);
            }
        }

        Exception CreateReadOnlyException()
        {
            return new InvalidOperationException(InternalSR.DictionaryIsReadOnly);
        }

        public void Add(TKey key, TValue value)
        {
            throw Fx.Exception.AsError(CreateReadOnlyException());
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            throw Fx.Exception.AsError(CreateReadOnlyException());
        }

        public void Clear()
        {
            throw Fx.Exception.AsError(CreateReadOnlyException());
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return this.dictionary.Contains(item);
        }
        public bool ContainsKey(TKey key)
        {
            return this.dictionary.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            this.dictionary.CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return this.dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public bool Remove(TKey key)
        {
            throw Fx.Exception.AsError(CreateReadOnlyException());
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            throw Fx.Exception.AsError(CreateReadOnlyException());
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return this.dictionary.TryGetValue(key, out value);
        }
    }
}
