// <copyright file="MemCache.cs" company="Microsoft">
//   Copyright (c) 2009 Microsoft Corporation.  All rights reserved.
// </copyright>
#if USE_MEMORY_CACHE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Security;
using System.Security.Permissions;
using System.Reflection;
using System.Runtime.Caching;
using System.Text;
using System.Web.Configuration;
using System.Web.Util;

namespace System.Web.Caching {
    internal sealed class MemCache: CacheInternal {
        private volatile bool _inited;
        private static object _initLock = new object();
        private MemoryCache _cacheInternal;
        private MemoryCache _cachePublic;
        private volatile bool _disposed;

        internal MemCache(CacheCommon cacheCommon) : base(cacheCommon) {
            // config initialization is done by Init.
            Assembly asm = Assembly.Load("System.Runtime.Caching, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, processorArchitecture=MSIL");
            Type t = asm.GetType("System.Runtime.Caching.MemoryCache", true, false);
            _cacheInternal = HttpRuntime.CreateNonPublicInstance(t, new object[] {"asp_icache", null, true}) as MemoryCache;
            _cachePublic = HttpRuntime.CreateNonPublicInstance(t, new object[] {"asp_pcache", null, true}) as MemoryCache;
        }

        protected override void Dispose(bool disposing) {
            try {
                _disposed = true;
                if (disposing) {
                    if (_cacheInternal != null) {
                        _cacheInternal.Dispose();
                    }
                    if (_cachePublic != null) {
                        _cachePublic.Dispose();
                    }
                }
            }
            finally {
                base.Dispose(disposing);
            }
        }

        internal override int PublicCount {
            get { 
                return (_cachePublic != null) ? (int)_cachePublic.GetCount() : 0;
            }
        }

        internal override long TotalCount {
            get {
                long internalCount = (_cacheInternal != null) ? _cacheInternal.GetCount(null) : 0;
                return internalCount + PublicCount;
            }
        }

        internal void Init(CacheSection cacheSection) {
            if (_inited) {
                return;
            }
            
            lock (_initLock) {
                if (_inited) {
                    return;
                }
                
                NameValueCollection config = null;
                if (cacheSection != null) {
                    //_enableMemoryCollection = (!cacheSection.DisableMemoryCollection);
                    //_enableExpiration = (!cacheSection.DisableExpiration);
                    int physicalMemoryLimitPercentage = cacheSection.PercentagePhysicalMemoryUsedLimit;
                    long cacheMemoryLimitMegabytes = cacheSection.PrivateBytesLimit << 20;
                    TimeSpan pollingInterval = cacheSection.PrivateBytesPollTime;
                    config = new NameValueCollection(3);
                    config["physicalMemoryLimitPercentage"] = physicalMemoryLimitPercentage.ToString(CultureInfo.InvariantCulture);
                    config["cacheMemoryLimitMegabytes"] = cacheMemoryLimitMegabytes.ToString(CultureInfo.InvariantCulture);
                    config["pollingInterval"] = pollingInterval.ToString();
                }

                Type t = _cacheInternal.GetType();

                t.InvokeMember("UpdateConfig",
                               BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, 
                               null, // binder
                               _cacheInternal, // target
                               new object[] {config}, // args
                               CultureInfo.InvariantCulture);
                t.InvokeMember("UpdateConfig",
                               BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, 
                               null, // binder
                               _cachePublic, // target
                               new object[] {config}, // args
                               CultureInfo.InvariantCulture);

                _inited = true;
            }
        }

        // return enumerator for public entries
        internal override IDictionaryEnumerator CreateEnumerator() {
            return (IDictionaryEnumerator)((IEnumerable)_cachePublic).GetEnumerator();
        }

        internal CacheEntryChangeMonitor CreateCacheEntryChangeMonitor(IEnumerable<String> keys, bool isPublic) {
            return (isPublic) ? _cachePublic.CreateCacheEntryChangeMonitor(keys, null) : _cacheInternal.CreateCacheEntryChangeMonitor(keys, null);
        }

        private CacheItemPolicy GetPolicy(CacheEntry newEntry) {
            CacheItemPolicy policy = new CacheItemPolicy();
            policy.SlidingExpiration = newEntry.SlidingExpiration;
            if (policy.SlidingExpiration == ObjectCache.NoSlidingExpiration) {
                policy.AbsoluteExpiration = (newEntry.UtcExpires != Cache.NoAbsoluteExpiration) ? newEntry.UtcExpires : ObjectCache.InfiniteAbsoluteExpiration;
            }
            if (newEntry.Dependency != null) {
                policy.ChangeMonitors.Add(new DependencyChangeMonitor(newEntry.Dependency));
            }
            policy.Priority = (newEntry.UsageBucket == 0xff) ? System.Runtime.Caching.CacheItemPriority.NotRemovable : System.Runtime.Caching.CacheItemPriority.Default;
            CacheItemRemovedCallback callback = newEntry.CacheItemRemovedCallback;
            if (callback != null) {
                policy.RemovedCallback = (new RemovedCallback(callback)).CacheEntryRemovedCallback;
            }            
            return policy;
        }

        internal override CacheEntry UpdateCache(CacheKey cacheKey,
                                                 CacheEntry newEntry,
                                                 bool replace,
                                                 CacheItemRemovedReason removedReason,
                                                 out object valueOld) {
            valueOld = null;
            CacheEntry entry = null;
            string key = cacheKey.Key;
            bool isPublic = cacheKey.IsPublic;
            if (_disposed) {
                return null;
            }

            MemoryCache cache = (isPublic) ? _cachePublic : _cacheInternal;
            if (newEntry == null && !replace) {
                // get
                object o = cache.Get(key);
                if (o != null) {
                    entry = new CacheEntry(key, o, null, null, 
                                           Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration,
                                           CacheItemPriority.Default, isPublic);
                    entry.State = CacheEntry.EntryState.AddedToCache;
                }
            }
            else if (newEntry != null && replace) {
                // set
                try {
                }
                finally {
                    // prevent ThreadAbortEx from interrupting these calls
                    CacheItemPolicy policy = GetPolicy(newEntry);
                    cache.Set(key, newEntry.Value, policy);
                }
            }
            else if (newEntry != null && !replace) {
                // add
                try {
                }
                finally {
                    // prevent ThreadAbortEx from interrupting these calls
                    CacheItemPolicy policy = GetPolicy(newEntry);
                    Object o = cache.AddOrGetExisting(key, newEntry.Value, policy);
                    if (o != null) {
                        entry = new CacheEntry(key, o, null, null, 
                                               Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration,
                                               CacheItemPriority.Default, isPublic);
                        entry.State = CacheEntry.EntryState.AddedToCache;
                    }
                }
            }
            else {
                // remove
                valueOld = cache.Remove(key);
            }
            return entry;
        }

        internal override long TrimIfNecessary(int percent) {
            return _cachePublic.Trim(percent) + _cacheInternal.Trim(percent);            
        }

        internal override void EnableExpirationTimer(bool enable) {
            // This is done by Dispose, so it's a no-op here
        }
    }
}
#endif
