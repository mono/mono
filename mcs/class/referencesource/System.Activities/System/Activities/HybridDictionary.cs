//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime;

    internal class HybridDictionary<TKey, TValue> : IDictionary<TKey, TValue>
        where TKey : class
        where TValue : class
    {
        TKey singleItemKey;
        TValue singleItemValue;
        IDictionary<TKey, TValue> dictionary;

        public int Count
        {
            get 
            {
                if (this.singleItemKey != null)
                {
                    return 1;
                }
                else if (this.dictionary != null)
                {
                    return this.dictionary.Count;
                }

                return 0;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                if (this.singleItemKey != null)
                {
                    return new ReadOnlyCollection<TValue>(new List<TValue>() { this.singleItemValue });
                }
                else if (this.dictionary != null)
                {
                    return new ReadOnlyCollection<TValue>(new List<TValue>(this.dictionary.Values));
                }

                return null;
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                if (this.singleItemKey != null)
                {
                    return new ReadOnlyCollection<TKey>(new List<TKey>() { this.singleItemKey });
                }
                else if (this.dictionary != null)
                {
                    return new ReadOnlyCollection<TKey>(new List<TKey>(this.dictionary.Keys));
                }

                return null;
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                if (this.singleItemKey == key)
                {
                    return this.singleItemValue;
                }
                else if (this.dictionary != null)
                {
                    return this.dictionary[key];
                }

                return null;
            }

            set
            {
                this.Add(key, value);
            }
        }

        public void Add(TKey key, TValue value)
        {
            if (key == null)
            {
                throw FxTrace.Exception.ArgumentNull("key");
            }

            if (this.singleItemKey == null && this.singleItemValue == null && this.dictionary == null)
            {
                this.singleItemKey = key;
                this.singleItemValue = value;
            }
            else if (this.singleItemKey != null)
            {
                this.dictionary = new Dictionary<TKey, TValue>();

                this.dictionary.Add(this.singleItemKey, this.singleItemValue);

                this.singleItemKey = null;
                this.singleItemValue = null;

                this.dictionary.Add(key, value);
                return;
            }
            else
            {
                Fx.Assert(this.dictionary != null, "We should always have a dictionary at this point");

                this.dictionary.Add(key, value);
            }
        }

        public bool ContainsKey(TKey key)
        {
            if (key == null)
            {
                throw FxTrace.Exception.ArgumentNull("key");
            }

            if (this.singleItemKey != null)
            {
                return this.singleItemKey == key;
            }
            else if (this.dictionary != null)
            {
                return this.dictionary.ContainsKey(key);
            }

            return false;
        }

        public bool Remove(TKey key)
        {
            if (this.singleItemKey == key)
            {
                this.singleItemKey = null;
                this.singleItemValue = null;

                return true;
            }
            else if (this.dictionary != null)
            {
                bool ret = this.dictionary.Remove(key);

                if (this.dictionary.Count == 0)
                {
                    this.dictionary = null;
                }

                return ret;
            }

            return false;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (this.singleItemKey == key)
            {
                value = this.singleItemValue;
                return true;
            }
            else if (this.dictionary != null)
            {
                return this.dictionary.TryGetValue(key, out value);
            }

            value = null;
            return false;
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            this.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            this.singleItemKey = null;
            this.singleItemValue = null;
            this.dictionary = null;
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            if (this.singleItemKey != null)
            {
                return this.singleItemKey == item.Key && this.singleItemValue == item.Value;
            }
            else if (this.dictionary != null)
            {
                return this.dictionary.Contains(item);
            }

            return false;
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (this.singleItemKey != null)
            {
                array[arrayIndex] = new KeyValuePair<TKey, TValue>(this.singleItemKey, this.singleItemValue);
            }
            else if (this.dictionary != null)
            {
                this.dictionary.CopyTo(array, arrayIndex);
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return this.Remove(item.Key);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            if (this.singleItemKey != null)
            {
                yield return new KeyValuePair<TKey, TValue>(this.singleItemKey, this.singleItemValue);
            }
            else if (this.dictionary != null)
            {
                foreach (KeyValuePair<TKey, TValue> kvp in this.dictionary)
                {
                    yield return kvp;
                }
            }
        }

        Collections.IEnumerator Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
