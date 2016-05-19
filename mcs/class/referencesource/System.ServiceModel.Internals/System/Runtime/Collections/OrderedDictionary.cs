//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Runtime.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;

    // System.Collections.Specialized.OrderedDictionary is NOT generic.
    // This class is essentially a generic wrapper for OrderedDictionary.
    class OrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary
    {
        OrderedDictionary privateDictionary;

        public OrderedDictionary()
        {
            this.privateDictionary = new OrderedDictionary();
        }

        public OrderedDictionary(IDictionary<TKey, TValue> dictionary)
        {
            if (dictionary != null)
            {
                this.privateDictionary = new OrderedDictionary();

                foreach (KeyValuePair<TKey, TValue> pair in dictionary)
                {
                    this.privateDictionary.Add(pair.Key, pair.Value);
                }
            }
        }

        public int Count
        {
            get
            {
                return this.privateDictionary.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                if (key == null)
                {
                    throw Fx.Exception.ArgumentNull("key");
                }

                if (this.privateDictionary.Contains(key))
                {
                    return (TValue)this.privateDictionary[(object)key];
                }
                else
                {
                    throw Fx.Exception.AsError(new KeyNotFoundException(InternalSR.KeyNotFoundInDictionary));
                }
            }
            set
            {
                if (key == null)
                {
                    throw Fx.Exception.ArgumentNull("key");
                }

                this.privateDictionary[(object)key] = value;
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                List<TKey> keys = new List<TKey>(this.privateDictionary.Count);
                
                foreach (TKey key in this.privateDictionary.Keys)
                {
                    keys.Add(key);
                }

                // Keys should be put in a ReadOnlyCollection,
                // but since this is an internal class, for performance reasons,
                // we choose to avoid creating yet another collection.

                return keys;
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                List<TValue> values = new List<TValue>(this.privateDictionary.Count);

                foreach (TValue value in this.privateDictionary.Values)
                {
                    values.Add(value);
                }

                // Values should be put in a ReadOnlyCollection,
                // but since this is an internal class, for performance reasons,
                // we choose to avoid creating yet another collection.

                return values;
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Add(TKey key, TValue value)
        {
            if (key == null)
            {
                throw Fx.Exception.ArgumentNull("key");
            }

            this.privateDictionary.Add(key, value);
        }

        public void Clear()
        {
            this.privateDictionary.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            if (item.Key == null || !this.privateDictionary.Contains(item.Key))
            {
                return false;
            }
            else
            {
                return this.privateDictionary[(object)item.Key].Equals(item.Value);
            }
        }

        public bool ContainsKey(TKey key)
        {
            if (key == null)
            {
                throw Fx.Exception.ArgumentNull("key");
            }

            return this.privateDictionary.Contains(key);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw Fx.Exception.ArgumentNull("array");
            }

            if (arrayIndex < 0)
            {
                throw Fx.Exception.AsError(new ArgumentOutOfRangeException("arrayIndex"));
            }

            if (array.Rank > 1 || arrayIndex >= array.Length || array.Length - arrayIndex < this.privateDictionary.Count)
            {
                throw Fx.Exception.Argument("array", InternalSR.BadCopyToArray);
            }

            int index = arrayIndex;
            foreach (DictionaryEntry entry in this.privateDictionary)
            {
                array[index] = new KeyValuePair<TKey, TValue>((TKey)entry.Key, (TValue)entry.Value);
                index++;
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (DictionaryEntry entry in this.privateDictionary)
            {
                yield return new KeyValuePair<TKey, TValue>((TKey)entry.Key, (TValue)entry.Value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (Contains(item))
            {
                this.privateDictionary.Remove(item.Key);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Remove(TKey key)
        {
            if (key == null)
            {
                throw Fx.Exception.ArgumentNull("key");
            }

            if (this.privateDictionary.Contains(key))
            {
                this.privateDictionary.Remove(key);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (key == null)
            {
                throw Fx.Exception.ArgumentNull("key");
            }

            bool keyExists = this.privateDictionary.Contains(key);
            value = keyExists ? (TValue)this.privateDictionary[(object)key] : default(TValue);

            return keyExists;
        }

        void IDictionary.Add(object key, object value)
        {
            this.privateDictionary.Add(key, value);
        }

        void IDictionary.Clear()
        {
            this.privateDictionary.Clear();
        }

        bool IDictionary.Contains(object key)
        {
            return this.privateDictionary.Contains(key);
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return this.privateDictionary.GetEnumerator();
        }

        bool IDictionary.IsFixedSize
        {
            get
            {
                return ((IDictionary)this.privateDictionary).IsFixedSize;
            }
        }

        bool IDictionary.IsReadOnly
        {
            get
            {
                return this.privateDictionary.IsReadOnly;
            }
        }

        ICollection IDictionary.Keys
        {
            get
            {
                return this.privateDictionary.Keys;
            }
        }

        void IDictionary.Remove(object key)
        {
            this.privateDictionary.Remove(key);
        }

        ICollection IDictionary.Values
        {
            get
            {
                return this.privateDictionary.Values;
            }
        }

        object IDictionary.this[object key]
        {
            get
            {
                return this.privateDictionary[key];
            }
            set
            {
                this.privateDictionary[key] = value;
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            this.privateDictionary.CopyTo(array, index);
        }

        int ICollection.Count
        {
            get
            {
                return this.privateDictionary.Count;
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return ((ICollection)this.privateDictionary).IsSynchronized;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return ((ICollection)this.privateDictionary).SyncRoot;
            }
        }

    }
}
