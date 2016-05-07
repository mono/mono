﻿// <copyright file="MemoryCache.cs" company="Microsoft">
//   Copyright (c) 2009 Microsoft Corporation.  All rights reserved.
// </copyright>
using System;
using System.Runtime.Caching.Configuration;
using System.Runtime.Caching.Resources;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Security;
using System.Security.Permissions;
using System.Threading;

namespace System.Runtime.Caching {
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification= "The class represents a type of cache")]
    public class MemoryCache : ObjectCache, IEnumerable, IDisposable {
        private const DefaultCacheCapabilities CAPABILITIES = DefaultCacheCapabilities.InMemoryProvider
                                                              | DefaultCacheCapabilities.CacheEntryChangeMonitors
                                                              | DefaultCacheCapabilities.AbsoluteExpirations
                                                              | DefaultCacheCapabilities.SlidingExpirations
                                                              | DefaultCacheCapabilities.CacheEntryUpdateCallback
                                                              | DefaultCacheCapabilities.CacheEntryRemovedCallback;
        private static readonly TimeSpan OneYear = new TimeSpan(365, 0, 0, 0);
        private static object s_initLock = new object();
        private static MemoryCache s_defaultCache;
        private static CacheEntryRemovedCallback s_sentinelRemovedCallback = new CacheEntryRemovedCallback(SentinelEntry.OnCacheEntryRemovedCallback);
        private MemoryCacheStore[] _stores;
        private int _storeCount;
        private int _storeMask;
        private int _disposed;
        private MemoryCacheStatistics _stats;
        private string _name;
        private PerfCounters _perfCounters;
        private bool _configLess;
        EventHandler _onAppDomainUnload;
        UnhandledExceptionEventHandler _onUnhandledException;

        private bool IsDisposed { get { return (_disposed == 1); } }
        internal bool ConfigLess { get { return _configLess; } }

        private class SentinelEntry {
            private string _key;
            private ChangeMonitor _expensiveObjectDependency;
            private CacheEntryUpdateCallback _updateCallback;

            internal SentinelEntry(string key, ChangeMonitor expensiveObjectDependency, CacheEntryUpdateCallback callback) {
                _key = key;
                _expensiveObjectDependency = expensiveObjectDependency;
                _updateCallback = callback;
            }

            internal string Key {
                get { return _key; }
            }

            internal ChangeMonitor ExpensiveObjectDependency {
                get { return _expensiveObjectDependency; }
            }

            internal CacheEntryUpdateCallback CacheEntryUpdateCallback {
                get { return _updateCallback; }
            }

            private static bool IsPolicyValid(CacheItemPolicy policy) {
                if (policy == null) {
                    return false;
                }
                // see if any change monitors have changed
                bool hasChanged = false;
                Collection<ChangeMonitor> changeMonitors = policy.ChangeMonitors;
                if (changeMonitors != null) {
                    foreach (ChangeMonitor monitor in changeMonitors) {
                        if (monitor != null && monitor.HasChanged) {
                            hasChanged = true;
                            break;
                        }
                    }
                }
                // if the monitors haven't changed yet and we have an update callback
                // then the policy is valid
                if (!hasChanged && policy.UpdateCallback != null) {
                    return true;
                }
                // if the monitors have changed we need to dispose them
                if (hasChanged) {
                    foreach (ChangeMonitor monitor in changeMonitors) {
                        if (monitor != null) {
                            monitor.Dispose();
                        }
                    }
                }
                return false;
            }

            internal static void OnCacheEntryRemovedCallback(CacheEntryRemovedArguments arguments) {
                MemoryCache cache = arguments.Source as MemoryCache;
                SentinelEntry entry = arguments.CacheItem.Value as SentinelEntry;
                CacheEntryRemovedReason reason = arguments.RemovedReason;
                switch (reason) {
                    case CacheEntryRemovedReason.Expired:
                        break;
                    case CacheEntryRemovedReason.ChangeMonitorChanged:
                        if (entry.ExpensiveObjectDependency.HasChanged) {
                            // If the expensiveObject has been removed explicitly by Cache.Remove,
                            // return from the SentinelEntry removed callback
                            // thus effectively removing the SentinelEntry from the cache.
                            return;
                        }
                        break;
                    case CacheEntryRemovedReason.Evicted:
                        Dbg.Fail("Reason should never be CacheEntryRemovedReason.Evicted since the entry was inserted as NotRemovable.");
                        return;
                    default:
                        // do nothing if reason is Removed or CacheSpecificEviction
                        return;
                }

                // invoke update callback
                try {
                    CacheEntryUpdateArguments args = new CacheEntryUpdateArguments(cache, reason, entry.Key, null);
                    entry.CacheEntryUpdateCallback(args);
                    Object expensiveObject = (args.UpdatedCacheItem != null) ? args.UpdatedCacheItem.Value : null;
                    CacheItemPolicy policy = args.UpdatedCacheItemPolicy;
                    // Dev10 861163 - Only update the "expensive" object if the user returns a new object,
                    // a policy with update callback, and the change monitors haven't changed.  (Inserting
                    // with change monitors that have already changed will cause recursion.)
                    if (expensiveObject != null && IsPolicyValid(policy)) {
                        cache.Set(entry.Key, expensiveObject, policy);
                    }
                    else {
                        cache.Remove(entry.Key);
                    }
                }
                catch {
                    cache.Remove(entry.Key);
                    // Review: What should we do with this exception?
                }
            }
        }


        // private and internal

        internal MemoryCacheStore GetStore(MemoryCacheKey cacheKey) {
            // Dev10 865907: Math.Abs throws OverflowException for Int32.MinValue
            int hashCode = cacheKey.Hash;
            if (hashCode < 0) {
                hashCode = (hashCode == Int32.MinValue) ? 0 : -hashCode;
            }
            int idx = hashCode & _storeMask;
            return _stores[idx];
        }

        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        [SuppressMessage("Microsoft.Security", "CA2106:SecureAsserts", Justification = "Grandfathered suppression from original caching code checkin")]
        private void InitDisposableMembers(NameValueCollection config) {
            bool dispose = true;
            try {
                try {
                    _perfCounters = new PerfCounters(_name);
                }
                catch {
                    // ignore exceptions from perf counters
                }
                for (int i = 0; i < _stores.Length; i++) {
                    _stores[i] = new MemoryCacheStore(this, _perfCounters);
                }
                _stats = new MemoryCacheStatistics(this, config);
                AppDomain appDomain = Thread.GetDomain();
                EventHandler onAppDomainUnload = new EventHandler(OnAppDomainUnload);
                appDomain.DomainUnload += onAppDomainUnload;
                _onAppDomainUnload = onAppDomainUnload;
                UnhandledExceptionEventHandler onUnhandledException = new UnhandledExceptionEventHandler(OnUnhandledException);
                appDomain.UnhandledException += onUnhandledException;
                _onUnhandledException = onUnhandledException;
                dispose = false;
            }
            finally {
                if (dispose) {
                    Dispose();
                }
            }
        }

        private void OnAppDomainUnload(Object unusedObject, EventArgs unusedEventArgs) {
            Dispose();
        }

        private void OnUnhandledException(Object sender, UnhandledExceptionEventArgs eventArgs) {
            // if the CLR is terminating, dispose the cache. 
            // This will dispose the perf counters (see Dev10 680819).
            if (eventArgs.IsTerminating) {
                Dispose();
            }
        }

        private void ValidatePolicy(CacheItemPolicy policy) {
            if (policy.AbsoluteExpiration != ObjectCache.InfiniteAbsoluteExpiration
                && policy.SlidingExpiration != ObjectCache.NoSlidingExpiration) {
                throw new ArgumentException(R.Invalid_expiration_combination, "policy");
            }            
            if (policy.SlidingExpiration < ObjectCache.NoSlidingExpiration || OneYear < policy.SlidingExpiration) {
                throw new ArgumentOutOfRangeException("policy", RH.Format(R.Argument_out_of_range, "SlidingExpiration", ObjectCache.NoSlidingExpiration, OneYear));
            }
            if (policy.RemovedCallback != null
                && policy.UpdateCallback != null) {
                throw new ArgumentException(R.Invalid_callback_combination, "policy");
            }
            if (policy.Priority != CacheItemPriority.Default && policy.Priority != CacheItemPriority.NotRemovable) {
                throw new ArgumentOutOfRangeException("policy", RH.Format(R.Argument_out_of_range, "Priority", CacheItemPriority.Default, CacheItemPriority.NotRemovable));
            }
        }

        // public

        // Amount of memory that can be used before
        // the cache begins to forcibly remove items.
        public long CacheMemoryLimit {
            get {
                return _stats.CacheMemoryLimit;
            }
        }

        public static MemoryCache Default { 
            get {
                if (s_defaultCache == null) {
                    lock (s_initLock) {
                        if (s_defaultCache == null) {
                            s_defaultCache = new MemoryCache();
                        }
                    }
                }
                return s_defaultCache;
            }
        }

        public override DefaultCacheCapabilities DefaultCacheCapabilities {
            get {
                return CAPABILITIES;
            }
        }

        public override string Name
        {
            get { return _name; }
        }

        // Percentage of physical memory that can be used before
        // the cache begins to forcibly remove items.
        public long PhysicalMemoryLimit {
            get {
                return _stats.PhysicalMemoryLimit;
            }
        }

        // The maximum interval of time afterwhich the cache
        // will update its memory statistics.
        public TimeSpan PollingInterval {
            get {
                return _stats.PollingInterval;
            }
        }

        // Only used for Default MemoryCache
        private MemoryCache() {
            _name = "Default";
            Init(null);
        }

        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification="This is assembly is a special case approved by the NetFx API review board")]
        public MemoryCache(string name, NameValueCollection config = null) {
            if (name == null) {
                throw new ArgumentNullException("name");
            }
            if (name == String.Empty) {
                throw new ArgumentException(R.Empty_string_invalid, "name");
            }
            if (String.Equals(name, "default", StringComparison.OrdinalIgnoreCase)) {
                throw new ArgumentException(R.Default_is_reserved, "name");
            }
            _name = name;
            Init(config);
        }

        // Configless is used when redirecting ASP.NET cache into the MemoryCache.  This avoids infinite recursion
        // due to the fact that the (ASP.NET) config system uses the cache, and the cache uses the
        // config system.  This method could be made public, perhaps with CAS to prevent partial trust callers.
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Grandfathered suppression from original caching code checkin")]
        internal MemoryCache(string name, NameValueCollection config, bool configLess) {
            if (name == null) {
                throw new ArgumentNullException("name");
            }
            if (name == String.Empty) {
                throw new ArgumentException(R.Empty_string_invalid, "name");
            }
            if (String.Equals(name, "default", StringComparison.OrdinalIgnoreCase)) {
                throw new ArgumentException(R.Default_is_reserved, "name");
            }
            _name = name;
            _configLess = configLess;
            Init(config);
        }

        private void Init(NameValueCollection config) {
            _storeCount = Environment.ProcessorCount;
#if MONO
            if (config != null) {
                if (config ["__MonoEmulateOneCPU"] == "true")
                    _storeCount = 1;
                if (config ["__MonoTimerPeriod"] != null) {
                    try {
                        int parsed = (int)UInt32.Parse (config ["__MonoTimerPeriod"]);
                        CacheExpires.EXPIRATIONS_INTERVAL = new TimeSpan (0, 0, parsed);
                    } catch {
                        //
                    }
                }
            }
#endif
            _storeMask = _storeCount - 1;
            _stores = new MemoryCacheStore[_storeCount];
            InitDisposableMembers(config);
        }

        private object AddOrGetExistingInternal(string key, object value, CacheItemPolicy policy) {
            if (key == null) {
                throw new ArgumentNullException("key");
            }
            DateTimeOffset absExp = ObjectCache.InfiniteAbsoluteExpiration;
            TimeSpan slidingExp = ObjectCache.NoSlidingExpiration;
            CacheItemPriority priority = CacheItemPriority.Default;
            Collection<ChangeMonitor> changeMonitors = null;
            CacheEntryRemovedCallback removedCallback = null;
            if (policy != null) {
                ValidatePolicy(policy);
                if (policy.UpdateCallback != null) {
                    throw new ArgumentException(R.Update_callback_must_be_null, "policy");
                }
                absExp = policy.AbsoluteExpiration;
                slidingExp = policy.SlidingExpiration;
                priority = policy.Priority;
                changeMonitors = policy.ChangeMonitors;
                removedCallback = policy.RemovedCallback;
            }            
            if (IsDisposed) {
                if (changeMonitors != null) {
                    foreach (ChangeMonitor monitor in changeMonitors) {
                        if (monitor != null) {
                            monitor.Dispose();
                        }
                    }
                }
                return null;            
            }
            MemoryCacheKey cacheKey = new MemoryCacheKey(key);
            MemoryCacheStore store = GetStore(cacheKey);
            MemoryCacheEntry entry = store.AddOrGetExisting(cacheKey, new MemoryCacheEntry(key, value, absExp, slidingExp, priority, changeMonitors, removedCallback, this));
            return (entry != null) ? entry.Value : null;
        }

        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification="This is assembly is a special case approved by the NetFx API review board")]
        public override CacheEntryChangeMonitor CreateCacheEntryChangeMonitor(IEnumerable<String> keys, String regionName = null) {
            if (regionName != null) {
                throw new NotSupportedException(R.RegionName_not_supported);
            }
            if (keys == null) {
                throw new ArgumentNullException("keys");
            }
            List<String> keysClone = new List<String>(keys);
            if (keysClone.Count == 0) {
                throw new ArgumentException(RH.Format(R.Empty_collection, "keys"));
            }

            foreach (string key in keysClone) {
                if (key == null) {
                    throw new ArgumentException(RH.Format(R.Collection_contains_null_element, "keys"));
                }
            }

            return new MemoryCacheEntryChangeMonitor(keysClone.AsReadOnly(), regionName, this);
        }

        public void Dispose() {
            if (Interlocked.Exchange(ref _disposed, 1) == 0) {
                // unhook domain events
                DisposeSafeCritical();
                // stats must be disposed prior to disposing the stores.
                if (_stats != null) {
                    _stats.Dispose();
                }
                if (_stores != null) {
                    foreach (MemoryCacheStore store in _stores) {
                        if (store != null) {
                            store.Dispose();
                        }
                    }
                }
                if (_perfCounters != null) {
                    _perfCounters.Dispose();
                }
                GC.SuppressFinalize(this);
            }
        }

        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        [SuppressMessage("Microsoft.Security", "CA2106:SecureAsserts", Justification = "Grandfathered suppression from original caching code checkin")]
        private void DisposeSafeCritical() {
            AppDomain appDomain = Thread.GetDomain();
            if (_onAppDomainUnload != null) {
                appDomain.DomainUnload -= _onAppDomainUnload;
            }
            if (_onUnhandledException != null) {
                appDomain.UnhandledException -= _onUnhandledException;
            }
        }

        private object GetInternal(string key, string regionName) {
            if (regionName != null) {
                throw new NotSupportedException(R.RegionName_not_supported);
            }
            if (key == null) {
                throw new ArgumentNullException("key");
            }
            MemoryCacheEntry entry = GetEntry(key);
            return (entry != null) ? entry.Value : null;
        }

        internal MemoryCacheEntry GetEntry(String key) {
            if (IsDisposed) {
                return null;
            }
            MemoryCacheKey cacheKey = new MemoryCacheKey(key);
            MemoryCacheStore store = GetStore(cacheKey);
            return store.Get(cacheKey);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            Hashtable h = new Hashtable();
            if (!IsDisposed) {
                foreach (MemoryCacheStore store in _stores) {
                    store.CopyTo(h);
                }
            }
            return h.GetEnumerator();
        }

        protected override IEnumerator<KeyValuePair<string, object>> GetEnumerator() {
            Dictionary<string, object> h = new Dictionary<string, object>();
            if (!IsDisposed) {
                foreach (MemoryCacheStore store in _stores) {
                    store.CopyTo(h);
                }
            }
            return h.GetEnumerator();
        }

        internal MemoryCacheEntry RemoveEntry(string key, MemoryCacheEntry entry, CacheEntryRemovedReason reason) {
            MemoryCacheKey cacheKey = new MemoryCacheKey(key);
            MemoryCacheStore store = GetStore(cacheKey);
            return store.Remove(cacheKey, entry, reason);
        }

        public long Trim(int percent) {
            if (percent > 100) {
                percent = 100;
            }
            long trimmed = 0;
            if (_disposed == 0) {
                foreach (MemoryCacheStore store in _stores) {
                    trimmed += store.TrimInternal(percent);
                }
            }
            return trimmed;
        }

        //Default indexer property
        public override object this[string key] {
            get {
                return GetInternal(key, null);
            }
            set {
                Set(key, value, ObjectCache.InfiniteAbsoluteExpiration);
            }
        }

        //Existence check for a single item
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification="This is assembly is a special case approved by the NetFx API review board")]
        public override bool Contains(string key, string regionName = null) {
            return (GetInternal(key, regionName) != null);
        }

        // Dev10 907758: Breaking bug in System.RuntimeCaching.MemoryCache.AddOrGetExisting (CacheItem, CacheItemPolicy)
        public override bool Add(CacheItem item, CacheItemPolicy policy) {
            CacheItem existingEntry = AddOrGetExisting(item, policy);
            return (existingEntry == null || existingEntry.Value == null);
        }

        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification="This is assembly is a special case approved by the NetFx API review board")]
        public override object AddOrGetExisting(string key, object value, DateTimeOffset absoluteExpiration, string regionName = null) {
            if (regionName != null) {
                throw new NotSupportedException(R.RegionName_not_supported);
            }
            CacheItemPolicy policy = new CacheItemPolicy();
            policy.AbsoluteExpiration = absoluteExpiration;
            return AddOrGetExistingInternal(key, value, policy);
        }

        public override CacheItem AddOrGetExisting(CacheItem item, CacheItemPolicy policy) {
            if (item == null) {
                throw new ArgumentNullException("item");
            }
            return new CacheItem(item.Key, AddOrGetExistingInternal(item.Key, item.Value, policy));
        }

        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification="This is assembly is a special case approved by the NetFx API review board")]
        public override object AddOrGetExisting(string key, object value, CacheItemPolicy policy, string regionName = null) {
            if (regionName != null) {
                throw new NotSupportedException(R.RegionName_not_supported);
            }
            return AddOrGetExistingInternal(key, value, policy);
        }

        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification="This is assembly is a special case approved by the NetFx API review board")]
        public override object Get(string key, string regionName = null) {
            return GetInternal(key, regionName);
        }

        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification="This is assembly is a special case approved by the NetFx API review board")]
        public override CacheItem GetCacheItem(string key, string regionName = null) {
            object value = GetInternal(key, regionName);
            return (value != null) ? new CacheItem(key, value) : null;
        }

        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification="This is assembly is a special case approved by the NetFx API review board")]
        public override void Set(string key, object value, DateTimeOffset absoluteExpiration, string regionName = null) {
            if (regionName != null) {
                throw new NotSupportedException(R.RegionName_not_supported);
            }
            CacheItemPolicy policy = new CacheItemPolicy();
            policy.AbsoluteExpiration = absoluteExpiration;
            Set(key, value, policy);
        }

        public override void Set(CacheItem item, CacheItemPolicy policy) {
            if (item == null) {
                throw new ArgumentNullException("item");
            }
            Set(item.Key, item.Value, policy);
        }

        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification="This is assembly is a special case approved by the NetFx API review board")]
        public override void Set(string key, object value, CacheItemPolicy policy, string regionName = null) {
            if (regionName != null) {
                throw new NotSupportedException(R.RegionName_not_supported);
            }
            if (key == null) {
                throw new ArgumentNullException("key");
            }
            DateTimeOffset absExp = ObjectCache.InfiniteAbsoluteExpiration;
            TimeSpan slidingExp = ObjectCache.NoSlidingExpiration;
            CacheItemPriority priority = CacheItemPriority.Default;
            Collection<ChangeMonitor> changeMonitors = null;
            CacheEntryRemovedCallback removedCallback = null;
            if (policy != null) {
                ValidatePolicy(policy);
                if (policy.UpdateCallback != null) {
                    Set(key, value, policy.ChangeMonitors, policy.AbsoluteExpiration, policy.SlidingExpiration, policy.UpdateCallback);
                    return;
                }
                absExp = policy.AbsoluteExpiration;
                slidingExp = policy.SlidingExpiration;
                priority = policy.Priority;
                changeMonitors = policy.ChangeMonitors;
                removedCallback = policy.RemovedCallback;
            }
            if (IsDisposed) {
                if (changeMonitors != null) {
                    foreach (ChangeMonitor monitor in changeMonitors) {
                        if (monitor != null) {
                            monitor.Dispose();
                        }
                    }
                }
                return;
            }            
            MemoryCacheKey cacheKey = new MemoryCacheKey(key);
            MemoryCacheStore store = GetStore(cacheKey);
            store.Set(cacheKey, new MemoryCacheEntry(key, value, absExp, slidingExp, priority, changeMonitors, removedCallback, this));
        }

        // DevDiv Bugs 162763: 
        // Add a an event that fires *before* an item is evicted from the ASP.NET Cache
        internal void Set(string key, 
                          object value,
                          Collection<ChangeMonitor> changeMonitors,
                          DateTimeOffset absoluteExpiration,
                          TimeSpan slidingExpiration,
                          CacheEntryUpdateCallback onUpdateCallback) {
            if (key == null) {
                throw new ArgumentNullException("key");
            }
            if (changeMonitors == null
                && absoluteExpiration == ObjectCache.InfiniteAbsoluteExpiration 
                && slidingExpiration == ObjectCache.NoSlidingExpiration) {
                throw new ArgumentException(R.Invalid_argument_combination);
            }
            if (onUpdateCallback == null) {
                throw new ArgumentNullException("onUpdateCallback");
            }
            if (IsDisposed) {
                if (changeMonitors != null) {
                    foreach (ChangeMonitor monitor in changeMonitors) {
                        if (monitor != null) {
                            monitor.Dispose();
                        }
                    }
                }
                return;
            }
            // Insert updatable cache entry
            MemoryCacheKey cacheKey = new MemoryCacheKey(key);
            MemoryCacheStore store = GetStore(cacheKey);
            MemoryCacheEntry cacheEntry = new MemoryCacheEntry(key, 
                                                               value, 
                                                               ObjectCache.InfiniteAbsoluteExpiration, 
                                                               ObjectCache.NoSlidingExpiration, 
                                                               CacheItemPriority.NotRemovable, 
                                                               null,
                                                               null, 
                                                               this);
            store.Set(cacheKey, cacheEntry);

            // Ensure the sentinel depends on its updatable entry
            string[] cacheKeys = { key };
            ChangeMonitor expensiveObjectDep = CreateCacheEntryChangeMonitor(cacheKeys);
            if (changeMonitors == null) {
                changeMonitors = new Collection<ChangeMonitor>();
            }
            changeMonitors.Add(expensiveObjectDep);

            // Insert sentinel entry for the updatable cache entry 
            MemoryCacheKey sentinelCacheKey = new MemoryCacheKey("OnUpdateSentinel" + key);
            MemoryCacheStore sentinelStore = GetStore(sentinelCacheKey);
            MemoryCacheEntry sentinelCacheEntry = new MemoryCacheEntry(sentinelCacheKey.Key,
                                                                       new SentinelEntry(key, expensiveObjectDep, onUpdateCallback),
                                                                       absoluteExpiration, 
                                                                       slidingExpiration,
                                                                       CacheItemPriority.NotRemovable, 
                                                                       changeMonitors,
                                                                       s_sentinelRemovedCallback, 
                                                                       this);
            sentinelStore.Set(sentinelCacheKey, sentinelCacheEntry);
            cacheEntry.ConfigureUpdateSentinel(sentinelStore, sentinelCacheEntry);
        }

        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification="This is assembly is a special case approved by the NetFx API review board")]
        public override object Remove(string key, string regionName = null) {
            if (regionName != null) {
                throw new NotSupportedException(R.RegionName_not_supported);
            }
            if (key == null) {
                throw new ArgumentNullException("key");
            }
            if (IsDisposed) {
                return null;
            }
            MemoryCacheEntry entry = RemoveEntry(key, null, CacheEntryRemovedReason.Removed);
            return (entry != null) ? entry.Value : null;
        }

        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification="This is assembly is a special case approved by the NetFx API review board")]
        public override long GetCount(string regionName = null) {
            if (regionName != null) {
                throw new NotSupportedException(R.RegionName_not_supported);
            }
            long count = 0;
            if (!IsDisposed) {
                foreach (MemoryCacheStore store in _stores) {
                    count += store.Count;
                }
            }
            return count;
        }

        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification="This is assembly is a special case approved by the NetFx API review board")]
        public override IDictionary<string, object> GetValues(IEnumerable<String> keys, string regionName = null) {
            if (regionName != null) {
                throw new NotSupportedException(R.RegionName_not_supported);
            }
            if (keys == null) {
                throw new ArgumentNullException("keys");
            }
            Dictionary<string, object> values = null;
            if (!IsDisposed) {
                foreach (string key in keys) {
                    if (key == null) {
                        throw new ArgumentException(RH.Format(R.Collection_contains_null_element, "keys"));
                    }
                    object value = GetInternal(key, null);
                    if (value != null) {
                        if (values == null) {
                            values = new Dictionary<string, object>();
                        }
                        values[key] = value;
                    }
                }
            }
            return values;
        }

        // used when redirecting ASP.NET cache into the MemoryCache.  This avoids infinite recursion
        // due to the fact that the (ASP.NET) config system uses the cache, and the cache uses the
        // config system.
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Grandfathered suppression from original caching code checkin")]
        internal void UpdateConfig(NameValueCollection config) {
            if (config == null) {
                throw new ArgumentNullException("config");
            }
            if (!IsDisposed) {
                _stats.UpdateConfig(config);
            }
        }
    }
}
