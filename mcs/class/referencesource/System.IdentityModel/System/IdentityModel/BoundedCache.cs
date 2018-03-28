//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading;

namespace System.IdentityModel
{
    /// <summary>
    /// A cache of type T where items are cached and removed 
    /// according to the type specified, currently only 'TimeBounded' is supported.  An items is added with an expiration time.
    /// </summary>
    internal class BoundedCache<T> where T : class
    {
        Dictionary<string, ExpirableItem<T>> _items;
        int _capacity;
        TimeSpan _purgeInterval;
        ReaderWriterLock _readWriteLock;
        DateTime _nextPurgeTime = DateTime.UtcNow;

        /// <summary>
        /// Creates a cache for items of Type 'T' where expired items will purged on a regular interval
        /// </summary>
        /// <param name="capacity">The maximum size of the cache in number of items.
        /// If int.MaxValue is passed then the size is not bound.</param>
        /// <param name="purgeInterval">The time interval for checking expired items.</param>
        /// 
        public BoundedCache(int capacity, TimeSpan purgeInterval)
            : this(capacity, purgeInterval, StringComparer.Ordinal)
        { }

        /// <summary>
        /// Creates a cache for items of Type 'T' where expired items will purged on a regular interval
        /// </summary>
        /// <param name="capacity">The maximum size of the cache in number of items.
        /// If int.MaxValue is passed then the size is not bound.</param>
        /// <param name="purgeInterval">The time interval for checking expired items.</param>
        /// <param name="keyComparer">EqualityComparer for comparing keys.</param>
        /// <exception cref="ArgumentOutOfRangeException">The input parameter 'capacity' is less than or equal to zero.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The input parameter 'purgeInterval' is less than or equal to TimeSpan.Zero.</exception>
        /// <exception cref="ArgumentNullException">The input parameter 'keyComparer' is null.</exception>
        public BoundedCache(int capacity, TimeSpan purgeInterval, IEqualityComparer<string> keyComparer)
        {
            if (capacity <= 0)
            {
                throw DiagnosticUtility.ThrowHelperArgumentOutOfRange("capacity", capacity, SR.GetString(SR.ID0002));
            }

            if (purgeInterval <= TimeSpan.Zero)
            {
                throw DiagnosticUtility.ThrowHelperArgumentOutOfRange("purgeInterval", purgeInterval, SR.GetString(SR.ID0016));
            }

            if (keyComparer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("keyComparer");
            }

            _capacity = capacity;
            _purgeInterval = purgeInterval;
            _items = new Dictionary<string, ExpirableItem<T>>(keyComparer);
            _readWriteLock = new ReaderWriterLock();
        }

        /// <summary>
        /// Gets the ReaderWriterLock for controlling simultaneous reads and writes
        /// </summary>
        protected ReaderWriterLock CacheLock
        {
            get
            {
                return _readWriteLock;
            }
        }

        /// <summary>
        /// Gets or Sets the current Capacity of the cache in number of items.
        /// </summary>
        public virtual int Capacity
        {
            get
            {
                return _capacity;
            }
            set
            {
                if (value <= 0)
                {
                    throw DiagnosticUtility.ThrowHelperArgumentOutOfRange("value", value, SR.GetString(SR.ID0002));
                }
                _capacity = value;
            }
        }

        /// <summary>
        /// Removes all items from the Cache.
        /// </summary>      
        public virtual void Clear()
        {
            // -1 milleseconds is infinite timeout
            _readWriteLock.AcquireWriterLock(TimeSpan.FromMilliseconds(-1));

            try
            {
                _items.Clear();
            }
            finally
            {
                _readWriteLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// Ensures that the maximum size is not exceeded
        /// </summary>
        /// <exception cref="LimitExceededException">If the Capacity of the cache has been reached.</exception>
        void EnforceQuota()
        {
            // int.MaxValue => unbounded
            if (_capacity == int.MaxValue)
            {
                return;
            }

            if (_items.Count >= _capacity)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new LimitExceededException(SR.GetString(SR.ID0021, _capacity)));
            }
        }

        /// <summary>
        /// Increases the maximum number of items that the cache will hold. 
        /// </summary>
        /// <param name="size">The capacity to increase.</param>
        /// <exception cref="ArgumentOutOfRangeException">The input parameter 'size' is less than or equal to zero.</exception>
        /// <returns>Updated capacity</returns>
        /// <remarks>If size + current capacity >= int.MaxValue then capacity will be set to int.MaxValue and the cache will be unbounded</remarks>
        public virtual int IncreaseCapacity(int size)
        {
            if (size <= 0)
            {
                throw DiagnosticUtility.ThrowHelperArgumentOutOfRange("size", size, SR.GetString(SR.ID0002));
            }

            // -1 milleseconds is infinite timeout
            _readWriteLock.AcquireWriterLock(TimeSpan.FromMilliseconds(-1));

            try
            {
                if (int.MaxValue - size <= _capacity)
                {
                    _capacity = int.MaxValue;
                }
                else
                {
                    _capacity = _capacity + size;
                }

                return _capacity;
            }
            finally
            {
                _readWriteLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// Gets the Dictionary that contains the cached items.
        /// </summary>
        protected Dictionary<string, ExpirableItem<T>> Items
        {
            get
            {
                return _items;
            }
        }

        /// <summary>
        /// This method must not be called from within a read or writer lock as a deadlock will occur.
        /// Checks the time a decides if a cleanup needs to occur.
        /// </summary>
        void Purge()
        {
            DateTime currentTime = DateTime.UtcNow;
            if (currentTime < _nextPurgeTime)                
            {
                return;
            }

            _nextPurgeTime = DateTimeUtil.Add(currentTime, _purgeInterval);

            // -1 milleseconds is infinite timeout
            _readWriteLock.AcquireWriterLock(TimeSpan.FromMilliseconds(-1));

            try
            {

                List<string> expiredItems = new List<string>();
                foreach (string key in _items.Keys)
                {
                    if (_items[key].IsExpired())
                    {
                        expiredItems.Add(key);
                    }
                }

                for (int i = 0; i < expiredItems.Count; ++i)
                {
                    _items.Remove(expiredItems[i]);
                }
            }
            finally
            {
                _readWriteLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// Gets or Sets the time interval that will be used for checking expired items.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">If 'value' is less than or equal to TimeSpan.Zero.</exception>
        public TimeSpan PurgeInterval
        {
            get { return _purgeInterval; }
            set
            {
                if (value <= TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ThrowHelperArgumentOutOfRange("value", value, SR.GetString(SR.ID0016));
                }

                _purgeInterval = value;
            }
        }

        /// <summary>
        /// Attempt to add a item to the cache.
        /// </summary>
        /// <param name="key">Key to use when adding item</param>
        /// <param name="item">Item of type 'T' to add to cache</param>
        /// <param name="expirationTime">The expiration time of the entry.</param>
        /// <returns>true if item was added, false if item was not added</returns>
        /// <exception cref="LimitExceededException">Thrown if an attempt is made to add an item when the current 
        /// cache size is equal to the capacity</exception>
        public virtual bool TryAdd(string key, T item, DateTime expirationTime)
        {
            Purge();

            // -1 milleseconds is infinite timeout
            _readWriteLock.AcquireWriterLock(TimeSpan.FromMilliseconds(-1));

            EnforceQuota();

            try
            {
                if (_items.ContainsKey(key))
                {
                    return false;
                }
                else
                {
                    _items[key] = new ExpirableItem<T>(item, expirationTime);
                    return true;
                }
            }
            finally
            {
                _readWriteLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// Attempts to find an item in the cache
        /// </summary>
        /// <param name="key">Item to search for.</param>
        /// <returns>true if item is in cache, false otherwise</returns>
        /// <remarks>Item may be expired and would be purged next cycle</remarks>
        public virtual bool TryFind(string key)
        {
            Purge();

            // -1 milleseconds is infinite timeout
            _readWriteLock.AcquireReaderLock(TimeSpan.FromMilliseconds(-1));
            try
            {
                if (_items.ContainsKey(key) && !_items[key].IsExpired())
                {
                    return true;
                }

                return false;
            }
            finally
            {
                _readWriteLock.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// Attempt to get an item from the Cache
        /// </summary>
        /// <param name="key">Item to seach for</param>
        /// <param name="item">The object refernece that will be set the the retrivied item.</param>
        /// <returns>true if item is found, false otherwise</returns>
        /// <remarks>Item may be expired and would be purged next cycle</remarks>
        public virtual bool TryGet(string key, out T item)
        {
            Purge();

            item = null;

            // -1 milleseconds is infinite timeout

            _readWriteLock.AcquireReaderLock(TimeSpan.FromMilliseconds(-1));
            try
            {
                if (_items.ContainsKey(key))
                {
                    if (!_items[key].IsExpired())
                    {
                        item = _items[key].Item;
                        return true;
                    }
                }

                return false;

            }
            finally
            {
                _readWriteLock.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// Attempts to remove an item from the Cache
        /// </summary>
        /// <param name="key">Item to remove</param>
        /// <returns>true if item was removed, false otherwise</returns>
        public virtual bool TryRemove(string key)
        {
            Purge();

            // -1 milleseconds is infinite timeout

            _readWriteLock.AcquireWriterLock(TimeSpan.FromMilliseconds(-1));
            try
            {
                if (!_items.ContainsKey(key))
                {
                    return false;
                }

                _items.Remove(key);
                return true;
            }
            finally
            {
                _readWriteLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// Wrapper class for objects contained in BoundedCache.  Contains the obj 'T' and 
        /// </summary>
        /// <typeparam name="ET">Type of the item</typeparam>
        protected class ExpirableItem<ET>
        {
            DateTime _expirationTime;
            ET _item;

            public ExpirableItem(ET item, DateTime expirationTime)
            {
                _item = item;
                if (expirationTime.Kind != DateTimeKind.Utc)
                {
                    _expirationTime = DateTimeUtil.ToUniversalTime(expirationTime);
                }
                else
                {
                    _expirationTime = expirationTime;
                }
            }

            public bool IsExpired()
            {
                return (_expirationTime <= DateTime.UtcNow);
            }

            public ET Item
            {
                get { return _item; }
            }
        }

        [Flags]
        internal enum CachingMode
        {
            Time,
            MRU,
            FIFO
        }
    }
}
