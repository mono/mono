//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Runtime.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime;

    class NullableKeyDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        bool isNullKeyPresent;
        TValue nullKeyValue;
        IDictionary<TKey, TValue> innerDictionary;

        public NullableKeyDictionary()
            : base()
        {
            this.innerDictionary = new Dictionary<TKey, TValue>();
        }

        public int Count
        {
            get { return this.innerDictionary.Count + (this.isNullKeyPresent ? 1 : 0); }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                return new NullKeyDictionaryKeyCollection<TKey, TValue>(this);
            }
        }

        public ICollection<TValue> Values
        {
            get { return new NullKeyDictionaryValueCollection<TKey, TValue>(this); }
        }

        public TValue this[TKey key]
        {
            get
            {
                if (key == null)
                {
                    if (this.isNullKeyPresent)
                    {
                        return this.nullKeyValue;
                    }
                    else
                    {
                        throw Fx.Exception.AsError(new KeyNotFoundException());
                    }
                }
                else
                {
                    return this.innerDictionary[key];
                }
            }
            set
            {
                if (key == null)
                {
                    this.isNullKeyPresent = true;
                    this.nullKeyValue = value;
                }
                else
                {
                    this.innerDictionary[key] = value;
                }
            }
        }

        public void Add(TKey key, TValue value)
        {
            if (key == null)
            {
                if (this.isNullKeyPresent)
                {
                    throw Fx.Exception.Argument("key", InternalSR.NullKeyAlreadyPresent);
                }
                this.isNullKeyPresent = true;
                this.nullKeyValue = value;
            }
            else
            {
                this.innerDictionary.Add(key, value);
            }
        }

        public bool ContainsKey(TKey key)
        {
            return key == null ? this.isNullKeyPresent : this.innerDictionary.ContainsKey(key);
        }

        public bool Remove(TKey key)
        {
            if (key == null)
            {
                bool result = this.isNullKeyPresent;
                this.isNullKeyPresent = false;
                this.nullKeyValue = default(TValue);
                return result;
            }
            else
            {
                return this.innerDictionary.Remove(key);
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (key == null)
            {
                if (this.isNullKeyPresent)
                {
                    value = this.nullKeyValue;
                    return true;
                }
                else
                {
                    value = default(TValue);
                    return false;
                }
            }
            else
            {
                return this.innerDictionary.TryGetValue(key, out value);
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            this.isNullKeyPresent = false;
            this.nullKeyValue = default(TValue);
            this.innerDictionary.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            if (item.Key == null)
            {
                if (this.isNullKeyPresent)
                {
                    return item.Value == null ? this.nullKeyValue == null : item.Value.Equals(this.nullKeyValue);
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return this.innerDictionary.Contains(item);
            }
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            this.innerDictionary.CopyTo(array, arrayIndex);
            if (this.isNullKeyPresent)
            {
                array[arrayIndex + this.innerDictionary.Count] = new KeyValuePair<TKey, TValue>(default(TKey), this.nullKeyValue);
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (item.Key == null)
            {
                if (this.Contains(item))
                {
                    this.isNullKeyPresent = false;
                    this.nullKeyValue = default(TValue);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return this.innerDictionary.Remove(item);
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            IEnumerator<KeyValuePair<TKey, TValue>> innerEnumerator = this.innerDictionary.GetEnumerator() as IEnumerator<KeyValuePair<TKey, TValue>>;

            while (innerEnumerator.MoveNext())
            {
                yield return innerEnumerator.Current;
            }

            if (this.isNullKeyPresent)
            {
                yield return new KeyValuePair<TKey, TValue>(default(TKey), this.nullKeyValue);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<TKey, TValue>>)this).GetEnumerator();
        }

        class NullKeyDictionaryKeyCollection<TypeKey, TypeValue> : ICollection<TypeKey>
        {
            NullableKeyDictionary<TypeKey, TypeValue> nullKeyDictionary;

            public NullKeyDictionaryKeyCollection(NullableKeyDictionary<TypeKey, TypeValue> nullKeyDictionary)
            {
                this.nullKeyDictionary = nullKeyDictionary;
            }

            public int Count
            {
                get
                {
                    int count = this.nullKeyDictionary.innerDictionary.Keys.Count;
                    if (this.nullKeyDictionary.isNullKeyPresent)
                    {
                        count++;
                    }
                    return count;
                }
            }

            public bool IsReadOnly
            {
                get { return true; }
            }

            public void Add(TypeKey item)
            {
                throw Fx.Exception.AsError(new NotSupportedException(InternalSR.KeyCollectionUpdatesNotAllowed));
            }

            public void Clear()
            {
                throw Fx.Exception.AsError(new NotSupportedException(InternalSR.KeyCollectionUpdatesNotAllowed));
            }

            public bool Contains(TypeKey item)
            {
                return item == null ? this.nullKeyDictionary.isNullKeyPresent : this.nullKeyDictionary.innerDictionary.Keys.Contains(item);
            }

            public void CopyTo(TypeKey[] array, int arrayIndex)
            {
                this.nullKeyDictionary.innerDictionary.Keys.CopyTo(array, arrayIndex);
                if (this.nullKeyDictionary.isNullKeyPresent)
                {
                    array[arrayIndex + this.nullKeyDictionary.innerDictionary.Keys.Count] = default(TypeKey);
                }
            }

            public bool Remove(TypeKey item)
            {
                throw Fx.Exception.AsError(new NotSupportedException(InternalSR.KeyCollectionUpdatesNotAllowed));
            }

            public IEnumerator<TypeKey> GetEnumerator()
            {
                foreach (TypeKey item in this.nullKeyDictionary.innerDictionary.Keys)
                {
                    yield return item;
                }

                if (this.nullKeyDictionary.isNullKeyPresent)
                {
                    yield return default(TypeKey);
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable<TypeKey>)this).GetEnumerator();
            }
        }

        class NullKeyDictionaryValueCollection<TypeKey, TypeValue> : ICollection<TypeValue>
        {
            NullableKeyDictionary<TypeKey, TypeValue> nullKeyDictionary;

            public NullKeyDictionaryValueCollection(NullableKeyDictionary<TypeKey, TypeValue> nullKeyDictionary)
            {
                this.nullKeyDictionary = nullKeyDictionary;
            }

            public int Count
            {
                get
                {
                    int count = this.nullKeyDictionary.innerDictionary.Values.Count;
                    if (this.nullKeyDictionary.isNullKeyPresent)
                    {
                        count++;
                    }
                    return count;
                }
            }

            public bool IsReadOnly
            {
                get { return true; }
            }

            public void Add(TypeValue item)
            {
                throw Fx.Exception.AsError(new NotSupportedException(InternalSR.ValueCollectionUpdatesNotAllowed));
            }

            public void Clear()
            {
                throw Fx.Exception.AsError(new NotSupportedException(InternalSR.ValueCollectionUpdatesNotAllowed));
            }

            public bool Contains(TypeValue item)
            {
                return this.nullKeyDictionary.innerDictionary.Values.Contains(item) ||
                    (this.nullKeyDictionary.isNullKeyPresent && this.nullKeyDictionary.nullKeyValue.Equals(item));
            }

            public void CopyTo(TypeValue[] array, int arrayIndex)
            {
                this.nullKeyDictionary.innerDictionary.Values.CopyTo(array, arrayIndex);
                if (this.nullKeyDictionary.isNullKeyPresent)
                {
                    array[arrayIndex + this.nullKeyDictionary.innerDictionary.Values.Count] = this.nullKeyDictionary.nullKeyValue;
                }
            }

            public bool Remove(TypeValue item)
            {
                throw Fx.Exception.AsError(new NotSupportedException(InternalSR.ValueCollectionUpdatesNotAllowed));
            }

            public IEnumerator<TypeValue> GetEnumerator()
            {
                foreach (TypeValue item in this.nullKeyDictionary.innerDictionary.Values)
                {
                    yield return item;
                }

                if (this.nullKeyDictionary.isNullKeyPresent)
                {
                    yield return this.nullKeyDictionary.nullKeyValue;
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable<TypeValue>)this).GetEnumerator();
            }
        }
    }
}
