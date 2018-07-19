//------------------------------------------------------------------------------
// <copyright file="InProcStateClientManager.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.SessionState {
    using System.Threading;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Runtime.Serialization;

    using System.Text;
    using System.Collections;
    using System.IO;
    using System.Web;
    using System.Web.Caching;
    using System.Web.Util;
    using System.Xml;
    using System.Collections.Specialized; 
    using System.Configuration.Provider;

    internal sealed class InProcSessionStateStore : SessionStateStoreProviderBase {
        internal static readonly int    CACHEKEYPREFIXLENGTH = CacheInternal.PrefixInProcSessionState.Length;
        internal static readonly int    NewLockCookie = 1;

        CacheItemRemovedCallback        _callback;
        
        SessionStateItemExpireCallback  _expireCallback;


        /*
         * Handle callbacks from the cache for session state expiry
         */
        public void OnCacheItemRemoved(String key, Object value, CacheItemRemovedReason reason) {
            InProcSessionState state;
            String id;

            Debug.Trace("SessionOnEnd", "OnCacheItemRemoved called, reason = " + reason);

            PerfCounters.DecrementCounter(AppPerfCounter.SESSIONS_ACTIVE);

            state = (InProcSessionState) value;
            
            if ((state._flags & (int)SessionStateItemFlags.IgnoreCacheItemRemoved) != 0 ||
                (state._flags & (int)SessionStateItemFlags.Uninitialized) != 0) {
                Debug.Trace("SessionOnEnd", "OnCacheItemRemoved ignored");
                return;
            }
            
            switch (reason) {
                case CacheItemRemovedReason.Expired: 
                    PerfCounters.IncrementCounter(AppPerfCounter.SESSIONS_TIMED_OUT);
                    break;

                case CacheItemRemovedReason.Removed:
                    PerfCounters.IncrementCounter(AppPerfCounter.SESSIONS_ABANDONED);
                    break;

                default:
                    break;    
            }

            TraceSessionStats();

            if (_expireCallback != null) {
                id = key.Substring(CACHEKEYPREFIXLENGTH);

                _expireCallback(id, SessionStateUtility.CreateLegitStoreData(null,
                                                        state._sessionItems,
                                                        state._staticObjects,
                                                        state._timeout));
            }
        }

        private string CreateSessionStateCacheKey(String id) {
            return CacheInternal.PrefixInProcSessionState + id;
        }

        public override void Initialize(string name, NameValueCollection config)
        {
            if (String.IsNullOrEmpty(name))
                name = "InProc Session State Provider";
            base.Initialize(name, config);

            _callback = new CacheItemRemovedCallback(this.OnCacheItemRemoved);
        }

        public override bool SetItemExpireCallback(SessionStateItemExpireCallback expireCallback)
        {
            _expireCallback = expireCallback;
            return true;
        }
        
        public override void Dispose()
        {
        }

        public override void InitializeRequest(HttpContext context)
        {
        }

        SessionStateStoreData DoGet(HttpContext context, 
                                        String id,
                                        bool exclusive,
                                        out bool locked,
                                        out TimeSpan lockAge, 
                                        out object lockId,
                                        out SessionStateActions actionFlags) {
            string  key = CreateSessionStateCacheKey(id);

            // Set default return values
            locked = false;
            lockId = null;
            lockAge = TimeSpan.Zero;
            actionFlags = 0;

            // Not technically necessary for InProc, but we do it to be consistent
            // with SQL provider
            SessionIDManager.CheckIdLength(id, true /* throwOnFail */);

            InProcSessionState state = (InProcSessionState)HttpRuntime.Cache.InternalCache.Get(key);
            if (state != null) {
                bool    lockedByOther;       // True if the state is locked by another session
                int initialFlags;

                initialFlags = (int)state._flags;
                if ((initialFlags & (int)SessionStateItemFlags.Uninitialized) != 0) {
                    // It is an uninitialized item.  We have to remove that flag.
                    // We only allow one request to do that.
                    // For details, see inline doc for SessionStateItemFlags.Uninitialized flag.

                    // If initialFlags != return value of CompareExchange, it means another request has
                    // removed the flag.

                    Debug.Trace("SessionStateClientSet", "Removing the Uninit flag for item; key = " + key);
                    if (initialFlags == Interlocked.CompareExchange(
                                            ref state._flags, 
                                            initialFlags & (~((int)SessionStateItemFlags.Uninitialized)), 
                                            initialFlags)) {
                        actionFlags = SessionStateActions.InitializeItem;
                    }
                }

                if (exclusive) {
                    lockedByOther = true;
                    
                    // If unlocked, use a spinlock to test and lock the state.
                    if (!state._locked) {
                        state._spinLock.AcquireWriterLock();
                        try {
                            if (!state._locked) {
                                lockedByOther = false;
                                state._locked = true;
                                state._utcLockDate = DateTime.UtcNow;
                                state._lockCookie++;
                            }
                            lockId = state._lockCookie;
                        }
                        finally {
                            state._spinLock.ReleaseWriterLock();
                        }
                    }
                    else {
                        // It's already locked by another request.  Return the lockCookie to caller.
                        lockId = state._lockCookie;
                    }

                }
                else {
                    state._spinLock.AcquireReaderLock();
                    try {
                        lockedByOther = state._locked;
                        lockId = state._lockCookie;
                    }
                    finally {
                        state._spinLock.ReleaseReaderLock();
                    }
                }
                
                if (lockedByOther) {
                    // Item found, but locked
                    locked = true;
                    lockAge = DateTime.UtcNow - state._utcLockDate;
                    return null;
                }
                else {
                    return SessionStateUtility.CreateLegitStoreData(context, state._sessionItems,
                                                state._staticObjects, state._timeout);
                }
            }

            // Not found
            return null;
        }

        public override SessionStateStoreData GetItem(HttpContext context, 
                                                String id,
                                                out bool locked,
                                                out TimeSpan lockAge, 
                                                out object lockId,
                                                out SessionStateActions actionFlags) {
            return DoGet(context, id, false, out locked, out lockAge, out lockId, out actionFlags);
        }


        public override SessionStateStoreData GetItemExclusive(HttpContext context, 
                                                String id,
                                                out bool locked,
                                                out TimeSpan lockAge, 
                                                out object lockId,
                                                out SessionStateActions actionFlags) {
            return DoGet(context, id, true, out locked, out lockAge, out lockId, out actionFlags);
        }

        // Unlock an item locked by GetExclusive
        // 'lockId' is the lock context returned by previous call to GetExclusive
        public override void ReleaseItemExclusive(HttpContext context, 
                                String id, 
                                object lockId) {
            Debug.Assert(lockId != null, "lockId != null");
            
            string  key = CreateSessionStateCacheKey(id);
            int     lockCookie = (int)lockId;
            
            SessionIDManager.CheckIdLength(id, true /* throwOnFail */);

            InProcSessionState state = (InProcSessionState)HttpRuntime.Cache.InternalCache.Get(key);

            /* If the state isn't there, we probably took too long to run. */
            if (state == null)
                return;

            if (state._locked) {
                state._spinLock.AcquireWriterLock();
                try {
                    if (state._locked && lockCookie == state._lockCookie) {
                        state._locked = false;
                    }
                }
                finally {
                    state._spinLock.ReleaseWriterLock();
                }
            }
        }

        public override void SetAndReleaseItemExclusive(HttpContext context, 
                                    String id, 
                                    SessionStateStoreData item, 
                                    object lockId, 
                                    bool newItem) {
            string          key = CreateSessionStateCacheKey(id);
            bool            doInsert = true;
            CacheStoreProvider     cacheInternal = HttpRuntime.Cache.InternalCache;
            int             lockCookieForInsert = NewLockCookie;
            ISessionStateItemCollection items = null;
            HttpStaticObjectsCollection staticObjects = null;

            Debug.Assert(item.Items != null, "item.Items != null");
            Debug.Assert(item.StaticObjects != null, "item.StaticObjects != null");
            Debug.Assert(item.Timeout <= SessionStateModule.MAX_CACHE_BASED_TIMEOUT_MINUTES, "item.Timeout <= SessionStateModule.MAX_CACHE_BASED_TIMEOUT_MINUTES");

            SessionIDManager.CheckIdLength(id, true /* throwOnFail */);

            if (item.Items.Count > 0) {
                items = item.Items;
            }

            if (!item.StaticObjects.NeverAccessed) {
                staticObjects = item.StaticObjects;
            }

            if (!newItem) {
                Debug.Assert(lockId != null, "lockId != null");
                InProcSessionState  stateCurrent = (InProcSessionState) cacheInternal.Get(key);
                int                 lockCookie = (int)lockId;

                /* If the state isn't there, we probably took too long to run. */
                if (stateCurrent == null)
                    return;

                Debug.Trace("SessionStateClientSet", "state is inStorage; key = " + key);
                Debug.Assert((stateCurrent._flags & (int)SessionStateItemFlags.Uninitialized) == 0, "Should never set an unitialized item; key = " + key);
                
                stateCurrent._spinLock.AcquireWriterLock();
                
                try {
                    /* Only set the state if we are the owner */
                    if (!stateCurrent._locked || stateCurrent._lockCookie != lockCookie) {
                        Debug.Trace("SessionStateClientSet", "Leave because we're not the owner; key = " + key);
                        return;
                    }

                    /* We can change the state in place if the timeout hasn't changed */
                    if (stateCurrent._timeout == item.Timeout) {
                        stateCurrent.Copy(
                            items,
                            staticObjects,
                            item.Timeout,
                            false,
                            DateTime.MinValue,
                            lockCookie,
                            stateCurrent._flags);

                        // Don't need to insert into the Cache because an in-place copy is good enough.
                        doInsert = false;
                        Debug.Trace("SessionStateClientSet", "Changing state inplace; key = " + key);
                    }
                    else {
                        /* We are going to insert a new item to replace the current one in Cache
                           because the expiry time has changed.
                           
                           Pleas note that an insert will cause the Session_End to be incorrectly raised. 
                           
                           Please note that the item itself should not expire between now and
                           where we do UtcInsert below because cacheInternal.Get above have just
                           updated its expiry time.
                        */ 
                        stateCurrent._flags |= (int)SessionStateItemFlags.IgnoreCacheItemRemoved;
                        
                        /* By setting _lockCookie to 0, we prevent an overwriting by ReleaseExclusive 
                           when we drop the lock.
                           The scenario can happen if another request is polling and trying to prempt
                           the lock we have on the item.
                        */
                        lockCookieForInsert = lockCookie;
                        stateCurrent._lockCookie = 0;
                    }
                }
                finally {
                    stateCurrent._spinLock.ReleaseWriterLock();
                }
            } 

            if (doInsert) {
                Debug.Trace("SessionStateClientSet", "Inserting state into Cache; key = " + key);
                InProcSessionState state = new InProcSessionState(
                        items,
                        staticObjects,
                        item.Timeout,
                        false,
                        DateTime.MinValue,
                        lockCookieForInsert,
                        0);

                try {
                }
                finally {
                    // protected from ThreadAbortEx
                    cacheInternal.Insert(key, state, new CacheInsertOptions() {
                                                            SlidingExpiration = new TimeSpan(0, state._timeout, 0),
                                                            Priority = CacheItemPriority.NotRemovable,
                                                            OnRemovedCallback = _callback
                                                        });
                    PerfCounters.IncrementCounter(AppPerfCounter.SESSIONS_TOTAL);
                    PerfCounters.IncrementCounter(AppPerfCounter.SESSIONS_ACTIVE);

                    TraceSessionStats();
                }
            }
        }

        
        public override void CreateUninitializedItem(HttpContext context, String id, int timeout) {
            string          key = CreateSessionStateCacheKey(id);

            Debug.Assert(timeout <= SessionStateModule.MAX_CACHE_BASED_TIMEOUT_MINUTES, "item.Timeout <= SessionStateModule.MAX_CACHE_BASED_TIMEOUT_MINUTES");
            
            SessionIDManager.CheckIdLength(id, true /* throwOnFail */);

            Debug.Trace("SessionStateClientSet", "Inserting an uninitialized item into Cache; key = " + key);

            InProcSessionState state = new InProcSessionState(
                    null,
                    null,
                    timeout,
                    false,
                    DateTime.MinValue,
                    NewLockCookie,
                    (int)SessionStateItemFlags.Uninitialized);

            // DevDivBugs 146875
            // We do not want to overwrite an item with an uninitialized item if it is
            // already in the cache
            try {
            }
            finally {
                // protected from ThreadAbortEx
                object existingEntry = HttpRuntime.Cache.InternalCache.Add(key, state, new CacheInsertOptions() {
                                                                                        SlidingExpiration = new TimeSpan(0, timeout, 0),
                                                                                        Priority = CacheItemPriority.NotRemovable,
                                                                                        OnRemovedCallback = _callback
                                                                                    });
                if (existingEntry == null) {
                    PerfCounters.IncrementCounter(AppPerfCounter.SESSIONS_TOTAL);
                    PerfCounters.IncrementCounter(AppPerfCounter.SESSIONS_ACTIVE);
                }
            }
        }
        
        // Remove an item.  Note that the item is originally obtained by GetExclusive
        // Same note as Set on lockId
        public override void RemoveItem(HttpContext context, 
                                        String id, 
                                        object lockId, 
                                        SessionStateStoreData item) {
            Debug.Assert(lockId != null, "lockId != null");
                
            string          key = CreateSessionStateCacheKey(id);
            CacheStoreProvider     cacheInternal = HttpRuntime.Cache.InternalCache;
            int             lockCookie = (int)lockId;

            SessionIDManager.CheckIdLength(id, true /* throwOnFail */);

            InProcSessionState state = (InProcSessionState) cacheInternal.Get(key);

            /* If the item isn't there, we probably took too long to run. */
            if (state == null)
                return;

            state._spinLock.AcquireWriterLock();
            
            try {
                /* Only remove the item if we are the owner */
                if (!state._locked || state._lockCookie != lockCookie)
                    return;

                /* prevent overwriting when we drop the lock */
                state._lockCookie = 0;
            }
            finally {
                state._spinLock.ReleaseWriterLock();
            }

            cacheInternal.Remove(key);

            TraceSessionStats();
        }

        // Reset the expire time of an item based on its timeout value
        public override void ResetItemTimeout(HttpContext context, String id)
        {
            string  key = CreateSessionStateCacheKey(id);
            
            SessionIDManager.CheckIdLength(id, true /* throwOnFail */);
            HttpRuntime.Cache.InternalCache.Get(key);
        }

        // Create a new SessionStateStoreData.
        public override SessionStateStoreData CreateNewStoreData(HttpContext context, int timeout)
        {
            return SessionStateUtility.CreateLegitStoreData(context, null, null, timeout);
        }

        // Called during EndRequest event
        public override void EndRequest(HttpContext context)
        {
        }
        
        [System.Diagnostics.Conditional("DBG")]
        internal static void TraceSessionStats() {
#if DBG
            Debug.Trace("SessionState", 
                        "sessionsTotal="          + PerfCounters.GetCounter(AppPerfCounter.SESSIONS_TOTAL) + 
                        ", sessionsActive="       + PerfCounters.GetCounter(AppPerfCounter.SESSIONS_ACTIVE) + 
                        ", sessionsAbandoned="    + PerfCounters.GetCounter(AppPerfCounter.SESSIONS_ABANDONED) + 
                        ", sessionsTimedout="     + PerfCounters.GetCounter(AppPerfCounter.SESSIONS_TIMED_OUT)
                        );
#endif
        }
    }

    internal sealed class InProcSessionState {
        internal ISessionStateItemCollection         _sessionItems;
        internal HttpStaticObjectsCollection    _staticObjects;
        internal int                            _timeout;        // USed to set slidingExpiration in CacheEntry
        internal bool                           _locked;         // If it's locked by another thread
        internal DateTime                       _utcLockDate;
        internal int                            _lockCookie;
        #pragma warning disable 0649
        internal ReadWriteSpinLock              _spinLock;
        #pragma warning restore 0649
        internal int                            _flags;

        internal InProcSessionState(
                ISessionStateItemCollection      sessionItems, 
                HttpStaticObjectsCollection staticObjects, 
                int                         timeout,
                bool                        locked,
                DateTime                    utcLockDate,
                int                         lockCookie,
                int                         flags) {

            Copy(sessionItems, staticObjects, timeout, locked, utcLockDate, lockCookie, flags);
        }

        internal void Copy(
                ISessionStateItemCollection      sessionItems, 
                HttpStaticObjectsCollection staticObjects, 
                int                         timeout,
                bool                        locked,
                DateTime                    utcLockDate,
                int                         lockCookie,
                int                         flags) {

            this._sessionItems = sessionItems;
            this._staticObjects = staticObjects;
            this._timeout = timeout;
            this._locked = locked;
            this._utcLockDate = utcLockDate;
            this._lockCookie = lockCookie;
            this._flags = flags;
        }
    }
}
