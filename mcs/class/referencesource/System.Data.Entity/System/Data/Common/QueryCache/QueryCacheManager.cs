//------------------------------------------------------------------------------
// <copyright file="QueryCacheManager.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
// @backupOwner Microsoft
//------------------------------------------------------------------------------

namespace System.Data.Common.QueryCache
{
    using System;
    using System.Collections.Generic;
    using System.Data.EntityClient;
    using System.Data.Metadata.Edm;
    using System.Data.Objects.Internal;
    using System.Diagnostics;
    using System.Threading;
    using System.Data.Common.Internal.Materialization;
    using System.Data.Entity.Util;

    /// <summary>
    /// Provides Query Execution Plan Caching Service 
    /// </summary>
    /// <remarks>
    /// Thread safe.
    /// Dispose <b>must</b> be called as there is no finalizer for this class
    /// </remarks>
    internal class QueryCacheManager : IDisposable
    {
        #region Constants/Default values for configuration parameters

        /// <summary>
        /// Default high mark for starting sweeping process
        /// default value: 80% of MaxNumberOfEntries
        /// </summary>
        const float DefaultHighMarkPercentageFactor = 0.8f; // 80% of MaxLimit

        /// <summary>
        /// Recycler timer period
        /// </summary>
        const int DefaultRecyclerPeriodInMilliseconds = 60 * 1000;

        #endregion

        #region Fields
        
        /// <summary>
        /// cache lock object
        /// </summary>
        private readonly object _cacheDataLock = new object();

        /// <summary>
        /// cache data
        /// </summary>
        private readonly Dictionary<QueryCacheKey, QueryCacheEntry> _cacheData = new Dictionary<QueryCacheKey, QueryCacheEntry>(32);

        /// <summary>
        /// soft maximum number of entries in the cache
        /// </summary>
        private readonly int _maxNumberOfEntries;

        /// <summary>
        /// high mark of the number of entries to trigger the sweeping process
        /// </summary>
        private readonly int _sweepingTriggerHighMark;

        /// <summary>
        /// Eviction timer 
        /// </summary>
        private readonly EvictionTimer _evictionTimer;

        #endregion
        
        #region Construction and Initialization

        /// <summary>
        /// Constructs a new Query Cache Manager instance, with default values for all 'configurable' parameters.
        /// </summary>
        /// <returns>A new instance of <see cref="QueryCacheManager"/> configured with default entry count, load factor and recycle period</returns>
        internal static QueryCacheManager Create()
        {
            return new QueryCacheManager(AppSettings.QueryCacheSize, DefaultHighMarkPercentageFactor, DefaultRecyclerPeriodInMilliseconds);
        }

        /// <summary>
        /// Cache Constructor
        /// </summary>
        /// <param name="maximumSize">
        ///   Maximum number of entries that the cache should contain.
        /// </param>
        /// <param name="loadFactor">
        ///   The number of entries that must be present, as a percentage, before entries should be removed
        ///   according to the eviction policy.
        ///   Must be greater than 0 and less than or equal to 1.0
        /// </param>
        /// <param name="recycleMillis">
        ///   The interval, in milliseconds, at which the number of entries will be compared to the load factor
        ///   and eviction carried out if necessary.
        /// </param>
        private QueryCacheManager(int maximumSize, float loadFactor, int recycleMillis)
        {
            Debug.Assert(maximumSize > 0, "Maximum size must be greater than zero");
            Debug.Assert(loadFactor > 0 && loadFactor <= 1, "Load factor must be greater than 0.0 and less than or equal to 1.0");
            Debug.Assert(recycleMillis >= 0, "Recycle period in milliseconds must not be negative");

            //
            // Load hardcoded defaults
            //
            this._maxNumberOfEntries = maximumSize;

            //
            // set sweeping high mark trigger value
            //
            this._sweepingTriggerHighMark = (int)(_maxNumberOfEntries * loadFactor);

            //
            // Initialize Recycler
            //
            this._evictionTimer = new EvictionTimer(this, recycleMillis);           
        }
                
        #endregion

        #region 'External' interface
        /// <summary>
        /// Adds new entry to the cache using "abstract" cache context and
        /// value; returns an existing entry if the key is already in the
        /// dictionary.
        /// </summary>
        /// <param name="inQueryCacheEntry"></param>
        /// <param name="outQueryCacheEntry">
        /// The existing entry in the dicitionary if already there;
        /// inQueryCacheEntry if none was found and inQueryCacheEntry
        /// was added instead.
        /// </param>
        /// <returns>true if the output entry was already found; false if it had to be added.</returns>
        internal bool TryLookupAndAdd(QueryCacheEntry inQueryCacheEntry, out QueryCacheEntry outQueryCacheEntry)
        {
            Debug.Assert(null != inQueryCacheEntry, "qEntry must not be null");

            outQueryCacheEntry = null;

            lock (_cacheDataLock)
            {
                if (!_cacheData.TryGetValue(inQueryCacheEntry.QueryCacheKey, out outQueryCacheEntry))
                {
                    //
                    // add entry to cache data
                    //
                    _cacheData.Add(inQueryCacheEntry.QueryCacheKey, inQueryCacheEntry);
                    if (_cacheData.Count > _sweepingTriggerHighMark)
                    {
                        _evictionTimer.Start();
                    }

                    return false;
                }
                else
                {
                    outQueryCacheEntry.QueryCacheKey.UpdateHit();

                    return true;
                }
            }
        }

        /// <summary>
        /// Lookup service for a cached value.
        /// </summary>
        internal bool TryCacheLookup<TK, TE>(TK key, out TE value)
            where TK : QueryCacheKey
        {
            Debug.Assert(null != key, "key must not be null");

            value = default(TE);

            //
            // invoke internal lookup
            //
            QueryCacheEntry qEntry = null;
            bool bHit = TryInternalCacheLookup(key, out qEntry);

            //
            // if it is a hit, 'extract' the entry strong type cache value
            //
            if (bHit)
            {
                value = (TE)qEntry.GetTarget();
            }

            return bHit;
        }

        /// <summary>
        /// Clears the Cache
        /// </summary>
        internal void Clear()
        {
            lock (_cacheDataLock)
            {
                _cacheData.Clear();
            }
        }
        #endregion

        #region Private Members
        
        /// <summary>
        /// lookup service
        /// </summary>
        /// <param name="queryCacheKey"></param>
        /// <param name="queryCacheEntry"></param>
        /// <returns>true if cache hit, false if cache miss</returns>
        private bool TryInternalCacheLookup( QueryCacheKey queryCacheKey, out QueryCacheEntry queryCacheEntry )
        {
            Debug.Assert(null != queryCacheKey, "queryCacheKey must not be null");

            queryCacheEntry = null;

            bool bHit = false;

            //
            // lock the cache for the minimal possible period
            //
            lock (_cacheDataLock)
            {
                bHit = _cacheData.TryGetValue(queryCacheKey, out queryCacheEntry);
            }

            //
            // if cache hit
            //
            if (bHit)
            {
                //
                // update hit mark in cache key
                //
                queryCacheEntry.QueryCacheKey.UpdateHit();
            }
            
            return bHit;
        }


        /// <summary>
        /// Recycler handler. This method is called directly by the eviction timer.
        /// It should take no action beyond invoking the <see cref="SweepCache"/> method on the 
        /// cache manager instance passed as <paramref name="state"/>.
        /// </summary>
        /// <param name="state">The cache manager instance on which the 'recycle' handler should be invoked</param>
        private static void CacheRecyclerHandler(object state)
        {
            ((QueryCacheManager)state).SweepCache();
        }

        /// <summary>
        /// Aging factor
        /// </summary>
        private static readonly int[] _agingFactor = {1,1,2,4,8,16};
        private static readonly int AgingMaxIndex = _agingFactor.Length - 1;

        /// <summary>
        /// Sweeps the cache removing old unused entries.
        /// This method implements the query cache eviction policy.
        /// </summary>
        private void SweepCache()
        {
            if (!this._evictionTimer.Suspend())
            {
                // Return of false from .Suspend means that the manager and timer have been disposed.
                return;
            }

            bool disabledEviction = false;
            lock (_cacheDataLock)
            {
                //
                // recycle only if entries exceeds the high mark factor
                //
                if (_cacheData.Count > _sweepingTriggerHighMark)
                {
                    //
                    // sweep the cache
                    //
                    uint evictedEntriesCount = 0;
                    List<QueryCacheKey> cacheKeys = new List<QueryCacheKey>(_cacheData.Count);
                    cacheKeys.AddRange(_cacheData.Keys);
                    for (int i = 0; i < cacheKeys.Count; i++)
                    {
                        //
                        // if entry was not used in the last time window, then evict the entry
                        //
                        if (0 == cacheKeys[i].HitCount)
                        {
                            _cacheData.Remove(cacheKeys[i]);
                            evictedEntriesCount++;
                        }
                        //
                        // otherwise, age the entry in a progressive scheme
                        //
                        else
                        {
                            int agingIndex = unchecked(cacheKeys[i].AgingIndex + 1);
                            if (agingIndex > AgingMaxIndex)
                            {
                                agingIndex = AgingMaxIndex;
                            }
                            cacheKeys[i].AgingIndex = agingIndex;
                            cacheKeys[i].HitCount = cacheKeys[i].HitCount >> _agingFactor[agingIndex];
                        }
                    }
                }
                else
                {
                    _evictionTimer.Stop();
                    disabledEviction = true;
                }
            }

            if (!disabledEviction)
            {
                this._evictionTimer.Resume();
            }
        }
              
        #endregion

        #region IDisposable Members
        
        /// <summary>
        /// Dispose instance
        /// </summary>
        /// <remarks>Dispose <b>must</b> be called as there are no finalizers for this class</remarks>
        public void Dispose()
        {
            // Technically, calling GC.SuppressFinalize is not required because the class does not
            // have a finalizer, but it does no harm, protects against the case where a finalizer is added
            // in the future, and prevents an FxCop warning.
            GC.SuppressFinalize(this);
            if (this._evictionTimer.Stop())
            {
                this.Clear();
            }
        }

        #endregion

        /// <summary>
        /// Periodically invokes cache cleanup logic on a specified <see cref="QueryCacheManager"/> instance,
        /// and allows this periodic callback to be suspended, resumed or stopped in a thread-safe way.
        /// </summary>
        private sealed class EvictionTimer
        {
            /// <summary>Used to control multi-threaded accesses to this instance</summary>
            private readonly object _sync = new object();

            /// <summary>The required interval between invocations of the cache cleanup logic</summary>
            private readonly int _period;

            /// <summary>The underlying QueryCacheManger that the callback will act on</summary>
            private readonly QueryCacheManager _cacheManager;

            /// <summary>The underlying <see cref="Timer"/> that implements the periodic callback</summary>
            private Timer _timer;

            internal EvictionTimer(QueryCacheManager cacheManager, int recyclePeriod)
            {
                this._cacheManager = cacheManager;
                this._period = recyclePeriod;
            }

            internal void Start()
            {
                lock (_sync)
                {
                    if (_timer == null)
                    {
                        this._timer = new Timer(QueryCacheManager.CacheRecyclerHandler, _cacheManager, _period, _period);
                    }
                }
            }
                        
            /// <summary>
            /// Permanently stops the eviction timer.
            /// It will no longer generate periodic callbacks and further calls to <see cref="Suspend"/>, <see cref="Resume"/>, or <see cref="Stop"/>,
            /// though thread-safe, will have no effect.
            /// </summary>
            /// <returns>
            ///   If this eviction timer has already been stopped (using the <see cref="Stop"/> method), returns <c>false</c>;
            ///   otherwise, returns <c>true</c> to indicate that the call successfully stopped and cleaned up the underlying timer instance.
            /// </returns>
            /// <remarks>
            ///   Thread safe. May be called regardless of the current state of the eviction timer.
            ///   Once stopped, an eviction timer cannot be restarted with the <see cref="Resume"/> method.
            /// </remarks>
            internal bool Stop()
            {
                lock (_sync)
                {
                    if (this._timer != null)
                    {
                        this._timer.Dispose();
                        this._timer = null;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            /// <summary>
            /// Pauses the operation of the eviction timer. 
            /// </summary>
            /// <returns>
            ///   If this eviction timer has already been stopped (using the <see cref="Stop"/> method), returns <c>false</c>;
            ///   otherwise, returns <c>true</c> to indicate that the call successfully suspended the inderlying <see cref="Timer"/>
            ///   and no further periodic callbacks will be generated until the <see cref="Resume"/> method is called.
            /// </returns>
            /// <remarks>
            ///   Thread-safe. May be called regardless of the current state of the eviction timer.
            ///   Once suspended, an eviction timer may be resumed or stopped.
            /// </remarks>
            internal bool Suspend()
            {
                lock (_sync)
                {
                    if (this._timer != null)
                    {
                        this._timer.Change(Timeout.Infinite, Timeout.Infinite);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            /// <summary>
            /// Causes this eviction timer to generate periodic callbacks, provided it has not been permanently stopped (using the <see cref="Stop"/> method).
            /// </summary>
            /// <remarks>
            ///   Thread-safe. May be called regardless of the current state of the eviction timer.
            /// </remarks>
            internal void Resume()
            {
                lock (_sync)
                {
                    if (this._timer != null)
                    {
                        this._timer.Change(this._period, this._period);
                    }
                }
            }
        }
    }
}
