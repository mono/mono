//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel
{
    using System.Collections;
    using System.Collections.Generic;

    [Serializable]
    class SerializableReadOnlyDictionary<K, V> : IDictionary<K, V>
    {
        static IDictionary<K, V> empty;
        IDictionary<K, V> dictionary;


        public SerializableReadOnlyDictionary(IDictionary<K, V> dictionary) : this(dictionary, true)
        {

        }
        public SerializableReadOnlyDictionary(IDictionary<K, V> dictionary, bool makeCopy)
        {
            if (makeCopy)
            {
                this.dictionary = new Dictionary<K, V>(dictionary);
            }
            else
            {
                this.dictionary = dictionary;
            }
        }

        public SerializableReadOnlyDictionary(params KeyValuePair<K, V>[] entries)
        {
            this.dictionary = new Dictionary<K, V>(entries.Length);

            foreach (KeyValuePair<K, V> pair in entries)
            {
                this.dictionary.Add(pair);
            }
        }

        public static IDictionary<K, V> Empty
        {
            get
            {
                if (empty == null)
                {
                    empty = new SerializableReadOnlyDictionary<K, V>(new Dictionary<K, V>(0), false);
                }
                return empty;
            }
        }

        public int Count
        {
            get { return this.dictionary.Count; }
        }
        public bool IsReadOnly
        {
            get { return true; }
        }
        public ICollection<K> Keys
        {
            get { return this.dictionary.Keys; }
        }
        public ICollection<V> Values
        {
            get { return this.dictionary.Values; }
        }
        public V this[K key]
        {
            get
            {
                return this.dictionary[key];
            }
            set
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));
            }
        }


        public void Add(K key, V value)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));
        }
        public void Add(KeyValuePair<K, V> item)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));
        }
        public void Clear()
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));
        }
        public bool Contains(KeyValuePair<K, V> item)
        {
            return this.dictionary.Contains(item);
        }
        public bool ContainsKey(K key)
        {
            return this.dictionary.ContainsKey(key);
        }
        public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
        {
            this.dictionary.CopyTo(array, arrayIndex);
        }
        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            return this.dictionary.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        public bool Remove(K key)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));
        }
        public bool Remove(KeyValuePair<K, V> item)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));
        }
        public bool TryGetValue(K key, out V value)
        {
            return this.dictionary.TryGetValue(key, out value);
        }
    }
}
