//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Runtime.Collections
{
    using System.Collections.Generic;

    // free-threaded so that it can deal with items releasing references and timer interactions
    // interaction pattern is:
    // 1) item = cache.Take(key); 
    // 2) if (item == null) { Create and Open Item; cache.Add(key, value); }
    // 2) use item, including performing any blocking operations like open/close/etc
    // 3) item.ReleaseReference();
    // 
    // for usability purposes, if a CacheItem is non-null you can always call Release on it
    class ObjectCache<TKey, TValue>
        where TValue : class
    {
        // for performance reasons we don't just blindly start a timer up to clean up 
        // idle cache items. However, if we're above a certain threshold of items, then we'll start the timer.
        const int timerThreshold = 1;

        ObjectCacheSettings settings;
        Dictionary<TKey, Item> cacheItems;
        bool idleTimeoutEnabled;
        bool leaseTimeoutEnabled;
        IOThreadTimer idleTimer;
        static Action<object> onIdle;
        bool disposed;

        public ObjectCache(ObjectCacheSettings settings)
            : this(settings, null)
        {
        }

        public ObjectCache(ObjectCacheSettings settings, IEqualityComparer<TKey> comparer)
        {
            Fx.Assert(settings != null, "caller must use a valid settings object");
            this.settings = settings.Clone();
            this.cacheItems = new Dictionary<TKey, Item>(comparer);

            // idle feature is disabled if settings.IdleTimeout == TimeSpan.MaxValue
            this.idleTimeoutEnabled = (settings.IdleTimeout != TimeSpan.MaxValue);

            // lease feature is disabled if settings.LeaseTimeout == TimeSpan.MaxValue
            this.leaseTimeoutEnabled = (settings.LeaseTimeout != TimeSpan.MaxValue);
        }

        object ThisLock
        {
            get
            {
                return this;
            }
        }

        // Users like ServiceModel can hook this for ICommunicationObject or to handle other non-IDisposable objects
        public Action<TValue> DisposeItemCallback
        {
            get;
            set;
        }

        public int Count
        {
            get
            {
                return this.cacheItems.Count;
            }
        }

        public ObjectCacheItem<TValue> Add(TKey key, TValue value)
        {
            Fx.Assert(key != null, "caller must validate parameters");
            Fx.Assert(value != null, "caller must validate parameters");
            lock (ThisLock)
            {
                if (this.Count >= this.settings.CacheLimit || this.cacheItems.ContainsKey(key))
                {
                    // cache is full or already has an entry - return a shell CacheItem
                    return new Item(key, value, this.DisposeItemCallback);
                }
                else
                {
                    return InternalAdd(key, value);
                }
            }
        }

        public ObjectCacheItem<TValue> Take(TKey key)
        {
            return Take(key, null);
        }

        // this overload is used for cases where a usable object can be atomically created in a non-blocking fashion
        public ObjectCacheItem<TValue> Take(TKey key, Func<TValue> initializerDelegate)
        {
            Fx.Assert(key != null, "caller must validate parameters");
            Item cacheItem = null;

            lock (ThisLock)
            {
                if (this.cacheItems.TryGetValue(key, out cacheItem))
                {
                    // we have an item, add a reference
                    cacheItem.InternalAddReference();
                }
                else
                {
                    if (initializerDelegate == null)
                    {
                        // not found in cache, no way to create. 
                        return null;
                    }

                    TValue createdObject = initializerDelegate();
                    Fx.Assert(createdObject != null, "initializer delegate must always give us a valid object");

                    if (this.Count >= this.settings.CacheLimit)
                    {
                        // cache is full - return a shell CacheItem
                        return new Item(key, createdObject, this.DisposeItemCallback);
                    }

                    cacheItem = InternalAdd(key, createdObject);
                }
            }

            return cacheItem;
        }

        // assumes caller takes lock
        Item InternalAdd(TKey key, TValue value)
        {
            Item cacheItem = new Item(key, value, this);
            if (this.leaseTimeoutEnabled)
            {
                cacheItem.CreationTime = DateTime.UtcNow;
            }

            this.cacheItems.Add(key, cacheItem);
            StartTimerIfNecessary();
            return cacheItem;
        }

        // assumes caller takes lock
        bool Return(TKey key, Item cacheItem)
        {
            bool disposeItem = false;

            if (this.disposed)
            {
                // we would have already disposed this item, do not attempt to return it
                disposeItem = true;
            }
            else
            {
                cacheItem.InternalReleaseReference();
                DateTime now = DateTime.UtcNow;
                if (this.idleTimeoutEnabled)
                {
                    cacheItem.LastUsage = now;
                }
                if (ShouldPurgeItem(cacheItem, now))
                {
                    bool removedFromItems = this.cacheItems.Remove(key);
                    Fx.Assert(removedFromItems, "we should always find the key");
                    cacheItem.LockedDispose();
                    disposeItem = true;
                }
            }
            return disposeItem;
        }

        void StartTimerIfNecessary()
        {
            if (this.idleTimeoutEnabled && this.Count > timerThreshold)
            {
                if (this.idleTimer == null)
                {
                    if (onIdle == null)
                    {
                        onIdle = new Action<object>(OnIdle);
                    }

                    this.idleTimer = new IOThreadTimer(onIdle, this, false);
                }

                this.idleTimer.Set(this.settings.IdleTimeout);
            }
        }

        // timer callback
        static void OnIdle(object state)
        {
            ObjectCache<TKey, TValue> cache = (ObjectCache<TKey, TValue>)state;
            cache.PurgeCache(true);
        }

        static void Add<T>(ref List<T> list, T item) 
        {
            Fx.Assert(!item.Equals(default(T)), "item should never be null");
            if (list == null)
            {
                list = new List<T>();
            }

            list.Add(item);
        }

        bool ShouldPurgeItem(Item cacheItem, DateTime now)
        {
            // only prune items who aren't in use
            if (cacheItem.ReferenceCount > 0)
            {
                return false;
            }

            if (this.idleTimeoutEnabled &&
                now >= (cacheItem.LastUsage + this.settings.IdleTimeout))
            {
                return true;
            }
            else if (this.leaseTimeoutEnabled &&
                (now - cacheItem.CreationTime) >= this.settings.LeaseTimeout)
            {
                return true;
            }

            return false;
        }

        void GatherExpiredItems(ref List<KeyValuePair<TKey, Item>> expiredItems, bool calledFromTimer)
        {
            if (this.Count == 0)
            {
                return;
            }

            if (!this.leaseTimeoutEnabled && !this.idleTimeoutEnabled)
            {
                return;
            }

            DateTime now = DateTime.UtcNow;
            bool setTimer = false;

            lock (ThisLock)
            {
                foreach (KeyValuePair<TKey, Item> cacheItem in this.cacheItems)
                {
                    if (ShouldPurgeItem(cacheItem.Value, now))
                    {
                        cacheItem.Value.LockedDispose();
                        Add(ref expiredItems, cacheItem);
                    }
                }

                // now remove items from the cache
                if (expiredItems != null)
                {
                    for (int i = 0; i < expiredItems.Count; i++)
                    {
                        this.cacheItems.Remove(expiredItems[i].Key);
                    }
                }

                setTimer = calledFromTimer && (this.Count > 0);
            }

            if (setTimer)
            {
                idleTimer.Set(this.settings.IdleTimeout);
            }
        }

        void PurgeCache(bool calledFromTimer)
        {
            List<KeyValuePair<TKey, Item>> itemsToClose = null;
            lock (ThisLock)
            {
                this.GatherExpiredItems(ref itemsToClose, calledFromTimer);
            }

            if (itemsToClose != null)
            {
                for (int i = 0; i < itemsToClose.Count; i++)
                {
                    itemsToClose[i].Value.LocalDispose();
                }
            }
        }

        // dispose all the Items if they are IDisposable
        public void Dispose()
        {
            lock (ThisLock)
            {
                foreach (Item item in this.cacheItems.Values)
                {
                    if (item != null)
                    {
                        // We need to Dispose every item in the cache even when it's refcount is greater than Zero, hence we call Dispose instead of LocalDispose
                        item.Dispose();
                    }
                }
                this.cacheItems.Clear();
                // we don't cache after Dispose
                this.settings.CacheLimit = 0;
                this.disposed = true;
                if (this.idleTimer != null)
                {
                    this.idleTimer.Cancel();
                    this.idleTimer = null;
                }
            }
            
        }

        // public surface area is synchronized through this.parent.ThisLock
        class Item : ObjectCacheItem<TValue>
        {
            readonly ObjectCache<TKey, TValue> parent;
            readonly TKey key;
            readonly Action<TValue> disposeItemCallback;

            TValue value;
            int referenceCount;

            public Item(TKey key, TValue value, Action<TValue> disposeItemCallback)
                : this(key, value)
            {
                this.disposeItemCallback = disposeItemCallback;
            }

            public Item(TKey key, TValue value, ObjectCache<TKey, TValue> parent)
                : this(key, value)
            {
                this.parent = parent;
            }

            Item(TKey key, TValue value)
            {
                this.key = key;
                this.value = value;
                this.referenceCount = 1; // start with a reference
            }

            public int ReferenceCount
            {
                get
                {
                    return this.referenceCount;
                }
            }

            public override TValue Value
            {
                get
                {
                    return this.value;
                }
            }

            public DateTime CreationTime
            {
                get;
                set;
            }

            public DateTime LastUsage
            {
                get;
                set;
            }

            public override bool TryAddReference()
            {
                bool result;

                // item may not be valid or cachable, first let's sniff for disposed without taking a lock
                if (this.parent == null || this.referenceCount == -1)
                {
                    result = false;
                }
                else
                {
                    bool disposeSelf = false;
                    lock (this.parent.ThisLock)
                    {
                        if (this.referenceCount == -1)
                        {
                            result = false;
                        }
                        else if (this.referenceCount == 0 && this.parent.ShouldPurgeItem(this, DateTime.UtcNow))
                        {
                            LockedDispose();
                            disposeSelf = true;
                            result = false;
                            this.parent.cacheItems.Remove(this.key);
                        }
                        else
                        {
                            // we're still in use, simply add-ref and be done
                            this.referenceCount++;
                            Fx.Assert(this.parent.cacheItems.ContainsValue(this), "should have a valid value");
                            Fx.Assert(this.Value != null, "should have a valid value");
                            result = true;
                        }
                    }

                    if (disposeSelf)
                    {
                        this.LocalDispose();
                    }
                }

                return result;
            }

            public override void ReleaseReference()
            {
                bool disposeItem;

                if (this.parent == null)
                {
                    Fx.Assert(this.referenceCount == 1, "reference count should have never increased");
                    this.referenceCount = -1; // not under a lock since we're not really in the cache
                    disposeItem = true;
                }
                else
                {
                    lock (this.parent.ThisLock)
                    {
                        // if our reference count will still be non zero, then simply decrement
                        if (this.referenceCount > 1)
                        {
                            InternalReleaseReference();
                            disposeItem = false;
                        }
                        else
                        {
                            // otherwise we need to coordinate with our parent
                            disposeItem = this.parent.Return(this.key, this);
                        }
                    }
                }

                if (disposeItem)
                {
                    this.LocalDispose();
                }
            }

            internal void InternalAddReference()
            {
                Fx.Assert(this.referenceCount >= 0, "cannot take the item marked for disposal");
                this.referenceCount++;
            }

            internal void InternalReleaseReference()
            {
                Fx.Assert(this.referenceCount > 0, "can only release an item that has references");
                this.referenceCount--;
            }

            // call this part under the lock, and Dispose outside the lock
            public void LockedDispose()
            {
                Fx.Assert(this.referenceCount == 0, "we should only dispose items without references");
                this.referenceCount = -1;
            }

            public void Dispose()
            {
                if (Value != null)
                {
                    Action<TValue> localDisposeItemCallback = this.disposeItemCallback;
                    if (this.parent != null)
                    {
                        Fx.Assert(localDisposeItemCallback == null, "shouldn't have both this.disposeItemCallback and this.parent");
                        localDisposeItemCallback = this.parent.DisposeItemCallback;
                    }

                    if (localDisposeItemCallback != null)
                    {
                        localDisposeItemCallback(Value);
                    }
                    else if (Value is IDisposable)
                    {
                        ((IDisposable)Value).Dispose();
                    }
                }
                this.value = null;
                // this will ensure that TryAddReference returns false
                this.referenceCount = -1;
            }

            public void LocalDispose()
            {
                Fx.Assert(this.referenceCount == -1, "we should only dispose items that have had LockedDispose called on them");
                this.Dispose();
            }
        }

    }
}
