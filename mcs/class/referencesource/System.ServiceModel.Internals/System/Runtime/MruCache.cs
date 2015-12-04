//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.Runtime
{
    using System.Collections.Generic;

    class MruCache<TKey, TValue>
        where TKey : class
        where TValue : class
    {
        LinkedList<TKey> mruList;
        Dictionary<TKey, CacheEntry> items;
        int lowWatermark;
        int highWatermark;
        CacheEntry mruEntry;

        public MruCache(int watermark)
            : this(watermark * 4 / 5, watermark)
        {
        }

        //
        // The cache will grow until the high watermark. At which point, the least recently used items
        // will be purge until the cache's size is reduced to low watermark
        //
        public MruCache(int lowWatermark, int highWatermark)
            : this(lowWatermark, highWatermark, null)
        {
        }

        public MruCache(int lowWatermark, int highWatermark, IEqualityComparer<TKey> comparer)
        {
            Fx.Assert(lowWatermark < highWatermark, "");
            Fx.Assert(lowWatermark >= 0, "");

            this.lowWatermark = lowWatermark;
            this.highWatermark = highWatermark;
            this.mruList = new LinkedList<TKey>();
            if (comparer == null)
            {
                this.items = new Dictionary<TKey, CacheEntry>();
            }
            else
            {
                this.items = new Dictionary<TKey, CacheEntry>(comparer);
            }
        }

        public int Count 
        {
            get
            {
                return this.items.Count;
            }
        }

        public void Add(TKey key, TValue value)
        {
            Fx.Assert(null != key, "");

            // if anything goes wrong (duplicate entry, etc) we should 
            // clear our caches so that we don't get out of [....]
            bool success = false;
            try
            {
                if (this.items.Count == this.highWatermark)
                {
                    // If the cache is full, purge enough LRU items to shrink the 
                    // cache down to the low watermark
                    int countToPurge = this.highWatermark - this.lowWatermark;
                    for (int i = 0; i < countToPurge; i++)
                    {
                        TKey keyRemove = this.mruList.Last.Value;
                        this.mruList.RemoveLast();
                        TValue item = this.items[keyRemove].value;
                        this.items.Remove(keyRemove);
                        OnSingleItemRemoved(item);
                        OnItemAgedOutOfCache(item);
                    }
                }
                // Add  the new entry to the cache and make it the MRU element
                CacheEntry entry;
                entry.node = this.mruList.AddFirst(key);
                entry.value = value;
                this.items.Add(key, entry);
                this.mruEntry = entry;
                success = true;
            }
            finally
            {
                if (!success)
                {
                    this.Clear();
                }
            }
        }

        public void Clear()
        {
            this.mruList.Clear();
            this.items.Clear();
            this.mruEntry.value = null;
            this.mruEntry.node = null;
        }

        public bool Remove(TKey key)
        {
            Fx.Assert(null != key, "");

            CacheEntry entry;
            if (this.items.TryGetValue(key, out entry))
            {
                this.items.Remove(key);
                OnSingleItemRemoved(entry.value);
                this.mruList.Remove(entry.node);
                if (object.ReferenceEquals(this.mruEntry.node, entry.node))
                {
                    this.mruEntry.value = null;
                    this.mruEntry.node = null;
                }
                return true;
            }

            return false;
        }

        protected virtual void OnSingleItemRemoved(TValue item)
        {
        }

        protected virtual void OnItemAgedOutOfCache(TValue item)
        {
        }
        
        //
        // If found, make the entry most recently used
        //
        public bool TryGetValue(TKey key, out TValue value)
        {
            // first check our MRU item
            if (this.mruEntry.node != null && key != null && key.Equals(this.mruEntry.node.Value))
            {
                value = this.mruEntry.value;
                return true;
            }

            CacheEntry entry;

            bool found = this.items.TryGetValue(key, out entry);
            value = entry.value;

            // Move the node to the head of the MRU list if it's not already there
            if (found && this.mruList.Count > 1
                && !object.ReferenceEquals(this.mruList.First, entry.node))
            {
                this.mruList.Remove(entry.node);
                this.mruList.AddFirst(entry.node);
                this.mruEntry = entry;
            }

            return found;
        }

        struct CacheEntry
        {
            internal TValue value;
            internal LinkedListNode<TKey> node;
        }
    }
}
