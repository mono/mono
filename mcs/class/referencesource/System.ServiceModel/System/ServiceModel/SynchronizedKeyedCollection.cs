//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.Collections.Generic
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    [ComVisible(false)]
    public abstract class SynchronizedKeyedCollection<K, T> : SynchronizedCollection<T>
    {
        const int defaultThreshold = 0;

        IEqualityComparer<K> comparer;
        Dictionary<K, T> dictionary;
        int keyCount;
        int threshold;

        protected SynchronizedKeyedCollection()
        {
            this.comparer = EqualityComparer<K>.Default;
            this.threshold = int.MaxValue;
        }

        protected SynchronizedKeyedCollection(object syncRoot)
            : base(syncRoot)
        {
            this.comparer = EqualityComparer<K>.Default;
            this.threshold = int.MaxValue;
        }

        protected SynchronizedKeyedCollection(object syncRoot, IEqualityComparer<K> comparer)
            : base(syncRoot)
        {
            if (comparer == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("comparer"));

            this.comparer = comparer;
            this.threshold = int.MaxValue;
        }

        protected SynchronizedKeyedCollection(object syncRoot, IEqualityComparer<K> comparer, int dictionaryCreationThreshold)
            : base(syncRoot)
        {
            if (comparer == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("comparer"));

            if (dictionaryCreationThreshold < -1)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("dictionaryCreationThreshold", dictionaryCreationThreshold,
                                                    SR.GetString(SR.ValueMustBeInRange, -1, int.MaxValue)));
            else if (dictionaryCreationThreshold == -1)
                this.threshold = int.MaxValue;
            else
                this.threshold = dictionaryCreationThreshold;

            this.comparer = comparer;
        }

        public T this[K key]
        {
            get
            {
                if (key == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("key"));

                lock (this.SyncRoot)
                {
                    if (this.dictionary != null)
                        return this.dictionary[key];

                    for (int i = 0; i < this.Items.Count; i++)
                    {
                        T item = this.Items[i];
                        if (this.comparer.Equals(key, this.GetKeyForItem(item)))
                            return item;
                    }

                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new KeyNotFoundException());
                }
            }
        }

        protected IDictionary<K, T> Dictionary
        {
            get { return this.dictionary; }
        }

        void AddKey(K key, T item)
        {
            if (this.dictionary != null)
                this.dictionary.Add(key, item);
            else if (this.keyCount == this.threshold)
            {
                this.CreateDictionary();
                this.dictionary.Add(key, item);
            }
            else
            {
                if (this.Contains(key))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.CannotAddTwoItemsWithTheSameKeyToSynchronizedKeyedCollection0)));

                this.keyCount++;
            }
        }

        protected void ChangeItemKey(T item, K newKey)
        {
            // check if the item exists in the collection
            if (!this.ContainsItem(item))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.ItemDoesNotExistInSynchronizedKeyedCollection0)));

            K oldKey = this.GetKeyForItem(item);
            if (!this.comparer.Equals(newKey, oldKey))
            {
                if (newKey != null)
                    this.AddKey(newKey, item);

                if (oldKey != null)
                    this.RemoveKey(oldKey);
            }
        }

        protected override void ClearItems()
        {
            base.ClearItems();

            if (this.dictionary != null)
                this.dictionary.Clear();

            this.keyCount = 0;
        }

        public bool Contains(K key)
        {
            if (key == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("key"));

            lock (this.SyncRoot)
            {
                if (this.dictionary != null)
                    return this.dictionary.ContainsKey(key);

                if (key != null)
                {
                    for (int i = 0; i < Items.Count; i++)
                    {
                        T item = Items[i];
                        if (this.comparer.Equals(key, GetKeyForItem(item)))
                            return true;
                    }
                }
                return false;
            }
        }

        bool ContainsItem(T item)
        {
            K key;
            if ((this.dictionary == null) || ((key = GetKeyForItem(item)) == null))
                return Items.Contains(item);

            T itemInDict;

            if (this.dictionary.TryGetValue(key, out itemInDict))
                return EqualityComparer<T>.Default.Equals(item, itemInDict);

            return false;
        }

        void CreateDictionary()
        {
            this.dictionary = new Dictionary<K, T>(this.comparer);

            foreach (T item in Items)
            {
                K key = GetKeyForItem(item);
                if (key != null)
                    this.dictionary.Add(key, item);
            }
        }

        protected abstract K GetKeyForItem(T item);

        protected override void InsertItem(int index, T item)
        {
            K key = this.GetKeyForItem(item);

            if (key != null)
                this.AddKey(key, item);

            base.InsertItem(index, item);
        }

        public bool Remove(K key)
        {
            if (key == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("key"));

            lock (this.SyncRoot)
            {
                if (this.dictionary != null)
                {
                    if (this.dictionary.ContainsKey(key))
                        return this.Remove(this.dictionary[key]);
                    else
                        return false;
                }
                else
                {
                    for (int i = 0; i < Items.Count; i++)
                    {
                        if (comparer.Equals(key, GetKeyForItem(Items[i])))
                        {
                            this.RemoveItem(i);
                            return true;
                        }
                    }
                    return false;
                }
            }
        }

        protected override void RemoveItem(int index)
        {
            K key = this.GetKeyForItem(this.Items[index]);

            if (key != null)
                this.RemoveKey(key);

            base.RemoveItem(index);
        }

        void RemoveKey(K key)
        {
            if (!(key != null))
            {
                Fx.Assert("key shouldn't be null!");
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("key");
            }
            if (this.dictionary != null)
                this.dictionary.Remove(key);
            else
                this.keyCount--;
        }

        protected override void SetItem(int index, T item)
        {
            K newKey = this.GetKeyForItem(item);
            K oldKey = this.GetKeyForItem(this.Items[index]);

            if (this.comparer.Equals(newKey, oldKey))
            {
                if ((newKey != null) && (this.dictionary != null))
                    this.dictionary[newKey] = item;
            }
            else
            {
                if (newKey != null)
                    this.AddKey(newKey, item);

                if (oldKey != null)
                    this.RemoveKey(oldKey);
            }
            base.SetItem(index, item);
        }
    }
}
