//------------------------------------------------------------------------------
// <copyright file="cache.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * Cache class
 *
 * Copyright (c) 1999 Microsoft Corporation
 */

namespace System.Web.Caching {
    using System.Collections;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Configuration.Provider;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Web.Util;
    using System.Web;
    using Microsoft.Win32;
    using System.Security.Permissions;
    using System.Globalization;
    using System.Web.Configuration;
    using System.Web.Hosting;
    using System.Web.Management;
    using Debug = System.Web.Util.Debug;


    /// <devdoc>
    /// <para>Represents the method that will handle the <see langword='onRemoveCallback'/>
    /// event of a System.Web.Caching.Cache instance.</para>
    /// </devdoc>
    public delegate void CacheItemRemovedCallback(
            string key, object value, CacheItemRemovedReason reason);

    /// <devdoc>
    /// <para>Represents the method that will handle the <see langword='onUpdateCallback'/>
    /// event of a System.Web.Caching.Cache instance.</para>
    /// </devdoc>
    [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters",
                     Justification="Shipped this way in NetFx 2.0 SP2")]
    public delegate void CacheItemUpdateCallback(
            string key, CacheItemUpdateReason reason,
            out object expensiveObject, out CacheDependency dependency, out DateTime absoluteExpiration, out TimeSpan slidingExpiration);

    /// <devdoc>
    /// <para> Specifies the relative priority of items stored in the System.Web.Caching.Cache. When the Web
    ///    server runs low on memory, the Cache selectively purges items to free system
    ///    memory. Items with higher priorities are less likely to be removed from the
    ///    cache when the server is under load. Web
    ///    applications can use these
    ///    values to prioritize cached items relative to one another. The default is
    ///    normal.</para>
    /// </devdoc>
    public enum CacheItemPriority {

        /// <devdoc>
        ///    <para> The cahce items with this priority level will be the first
        ///       to be removed when the server frees system memory by deleting items from the
        ///       cache.</para>
        /// </devdoc>
        Low = 1,

        /// <devdoc>
        ///    <para> The cache items with this priority level
        ///       are in the second group to be removed when the server frees system memory by
        ///       deleting items from the cache. </para>
        /// </devdoc>
        BelowNormal,

        /// <devdoc>
        ///    <para> The cache items with this priority level are in
        ///       the third group to be removed when the server frees system memory by deleting items from the cache. This is the default. </para>
        /// </devdoc>
        Normal,

        /// <devdoc>
        ///    <para> The cache items with this priority level are in the
        ///       fourth group to be removed when the server frees system memory by deleting items from the
        ///       cache. </para>
        /// </devdoc>
        AboveNormal,

        /// <devdoc>
        ///    <para>The cache items with this priority level are in the fifth group to be removed
        ///       when the server frees system memory by deleting items from the cache. </para>
        /// </devdoc>
        High,

        /// <devdoc>
        ///    <para>The cache items with this priority level will not be removed when the server
        ///       frees system memory by deleting items from the cache. </para>
        /// </devdoc>
        NotRemovable,

        /// <devdoc>
        ///    <para>The default value is Normal.</para>
        /// </devdoc>
        Default = Normal
    }


    /// <devdoc>
    ///    <para>Specifies the reason that a cached item was removed.</para>
    /// </devdoc>
    public enum CacheItemRemovedReason {

        /// <devdoc>
        /// <para>The item was removed from the cache by the 'System.Web.Caching.Cache.Remove' method, or by an System.Web.Caching.Cache.Insert method call specifying the same key.</para>
        /// </devdoc>
        Removed = 1,

        /// <devdoc>
        ///    <para>The item was removed from the cache because it expired. </para>
        /// </devdoc>
        Expired,

        /// <devdoc>
        ///    <para>The item was removed from the cache because the value in the hitInterval
        ///       parameter was not met, or because the system removed it to free memory.</para>
        /// </devdoc>
        Underused,

        /// <devdoc>
        ///    <para>The item was removed from the cache because a file or key dependency was
        ///       changed.</para>
        /// </devdoc>
        DependencyChanged
    }

    /// <devdoc>
    ///    <para>Specifies the reason why a cached item needs to be updated.</para>
    /// </devdoc>
    [SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue",
            Justification = "This enum should mirror CacheItemRemovedReason enum in design")]
    public enum CacheItemUpdateReason {

        /// <devdoc>
        ///    <para>The item needs to be updated because it expired. </para>
        /// </devdoc>
        Expired = 1,

        /// <devdoc>
        ///    <para>The item needs to be updated because a file or key dependency was
        ///       changed.</para>
        /// </devdoc>
        DependencyChanged
    }

    /// <devdoc>
    ///    <para>Implements the cache for a Web application. There is only one instance of
    ///       this class per application domain, and it remains valid only as long as the
    ///       application domain remains active. Information about an instance of this class
    ///       is available through the <see langword='Cache'/> property of the System.Web.HttpContext.</para>
    /// </devdoc>

    //
    // Extra notes:
    // - The Cache object contains a ICacheStore object and wraps it for public consumption.
    //
    public sealed class Cache : IEnumerable {

        /// <devdoc>
        ///    <para>Sets the absolute expiration policy to, in essence,
        ///       never. When set, this field is equal to the the System.DateTime.MaxValue , which is a constant
        ///       representing the largest possible <see langword='DateTime'/> value. The maximum date and
        ///       time value is equivilant to "12/31/9999 11:59:59 PM". This field is read-only.</para>
        /// </devdoc>
        public static readonly DateTime NoAbsoluteExpiration = DateTime.MaxValue;


        /// <devdoc>
        ///    <para>Sets the amount of time for sliding cache expirations to
        ///       zero. When set, this field is equal to the System.TimeSpan.Zero field, which is a constant value of
        ///       zero. This field is read-only.</para>
        /// </devdoc>
        public static readonly TimeSpan NoSlidingExpiration = TimeSpan.Zero;

        static CacheStoreProvider   _objectCache = null;
        static CacheStoreProvider   _internalCache = null;
        static CacheItemRemovedCallback s_sentinelRemovedCallback = new CacheItemRemovedCallback(SentinelEntry.OnCacheItemRemovedCallback);

        /// <internalonly/>
        /// <devdoc>
        ///    <para>This constructor is for internal use only, and was accidentally made public - do not use.</para>
        /// </devdoc>
        [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
        public Cache() {
        }

        //
        // internal ctor used by CacheCommon that avoids the demand for UnmanagedCode.
        //
        internal Cache(int dummy) {
        }


        /// <devdoc>
        ///    <para>Gets the number of items stored in the cache. This value can be useful when
        ///       monitoring your application's performance or when using the ASP.NET tracing
        ///       functionality.</para>
        /// </devdoc>
        public int Count {
            get {
                return Convert.ToInt32(ObjectCache.ItemCount);
            }
        }


        internal CacheStoreProvider GetInternalCache(bool createIfDoesNotExist) {
            if (_internalCache == null && createIfDoesNotExist) {
                lock (this) {
                    if (_internalCache == null) {
                        NameValueCollection cacheProviderSettings = HostingEnvironment.CacheStoreProviderSettings;

                        if (cacheProviderSettings != null) {
                            string providerName = (string)cacheProviderSettings["name"]; // Grab this now, as InstantiateProvider will remove it from settings
                            cacheProviderSettings["isPublic"] = "false";
                            _internalCache = (CacheStoreProvider)ProvidersHelper.InstantiateProvider(cacheProviderSettings, typeof(CacheStoreProvider));
                            _internalCache.Initialize(providerName, cacheProviderSettings);
                        }
                        else {
                            if (_objectCache is AspNetCache) {
                                _internalCache = new AspNetCache((AspNetCache)_objectCache, isPublic: false);
                            }
                            else {
                                _internalCache = new AspNetCache(isPublic: false);
                            }
                            _internalCache.Initialize(null, new NameValueCollection());
                        }
                    }
                }
            }

            return _internalCache;
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "We carefully control this method's callers.")]
        internal CacheStoreProvider GetObjectCache(bool createIfDoesNotExist) {
            if (_objectCache == null && createIfDoesNotExist) {
                lock (this) {
                    if (_objectCache == null) {
                        NameValueCollection cacheProviderSettings = HostingEnvironment.CacheStoreProviderSettings;

                        if (cacheProviderSettings != null) {
                            string providerName = (string)cacheProviderSettings["name"]; // Grab this now, as InstantiateProvider will remove it from settings
                            cacheProviderSettings["isPublic"] = "true";
                            _objectCache = (CacheStoreProvider)ProvidersHelper.InstantiateProvider(cacheProviderSettings, typeof(CacheStoreProvider));
                            _objectCache.Initialize(providerName, cacheProviderSettings);
                        }
                        else {
                            if (_internalCache is AspNetCache) {
                                _objectCache = new AspNetCache((AspNetCache)_internalCache, isPublic: true);
                            }
                            else {
                                _objectCache = new AspNetCache(isPublic: true);
                            }
                            _objectCache.Initialize(null, new NameValueCollection());
                        }
                    }
                }
            }

            return _objectCache;
        }

        /// <devdoc>
        ///    <para>Provides access to the cache store used by ASP.Net internals.</para>
        /// </devdoc>
        internal CacheStoreProvider InternalCache {
            get { return GetInternalCache(createIfDoesNotExist: true); }
        }

        /// <devdoc>
        ///    <para>Provides access to the cache store that backs HttpRuntime.Cache.</para>
        /// </devdoc>
        internal CacheStoreProvider ObjectCache {
            get { return GetObjectCache(createIfDoesNotExist: true); }
        }

        /// <internalonly/>
        IEnumerator IEnumerable.GetEnumerator() {
            return ObjectCache.GetEnumerator();
        }


        /// <devdoc>
        ///    <para>Returns a dictionary enumerator used for iterating through the key/value
        ///       pairs contained in the cache. Items can be added to or removed from the cache
        ///       while this method is enumerating through the cache items.</para>
        /// </devdoc>
        public IDictionaryEnumerator GetEnumerator() {
            return ObjectCache.GetEnumerator();
        }


        /// <devdoc>
        ///    <para>Gets or sets an item in the cache.</para>
        /// </devdoc>
        public object this[string key] {
            get {
                return Get(key);
            }

            set {
                Insert(key, value);
            }
        }

        private class SentinelEntry {
            private string _key;
            private CacheDependency _expensiveObjectDependency;
            private CacheItemUpdateCallback _cacheItemUpdateCallback;

            public SentinelEntry(string key, CacheDependency expensiveObjectDependency, CacheItemUpdateCallback callback) {
                _key = key;
                _expensiveObjectDependency = expensiveObjectDependency;
                _cacheItemUpdateCallback = callback;
            }

            public string Key {
                get { return _key; }
            }

            public CacheDependency ExpensiveObjectDependency {
                get { return _expensiveObjectDependency; }
            }

            public CacheItemUpdateCallback CacheItemUpdateCallback {
                get { return _cacheItemUpdateCallback; }
            }

            public static void OnCacheItemRemovedCallback(string key, object value, CacheItemRemovedReason reason) {
                CacheItemUpdateReason updateReason;
                SentinelEntry entry = value as SentinelEntry;

                switch (reason) {
                    case CacheItemRemovedReason.Expired:
                        updateReason = CacheItemUpdateReason.Expired;
                        break;
                    case CacheItemRemovedReason.DependencyChanged:
                        updateReason = CacheItemUpdateReason.DependencyChanged;
                        if (entry.ExpensiveObjectDependency.HasChanged) {
                            // If the expensiveObject has been removed explicitly by Cache.Remove,
                            // return from the SentinelEntry removed callback
                            // thus effectively removing the SentinelEntry from the cache.
                            return;
                        }
                        break;
                    case CacheItemRemovedReason.Underused:
                        Debug.Fail("Reason should never be CacheItemRemovedReason.Underused since the entry was inserted as NotRemovable.");
                        return;
                    default:
                        // do nothing if reason is Removed
                        return;
                }

                CacheDependency cacheDependency;
                DateTime absoluteExpiration;
                TimeSpan slidingExpiration;
                object expensiveObject;
                CacheItemUpdateCallback callback = entry.CacheItemUpdateCallback;
                // invoke update callback
                try {
                    callback(entry.Key, updateReason, out expensiveObject, out cacheDependency, out absoluteExpiration, out slidingExpiration);
                    // Dev10 861163 - Only update the "expensive" object if the user returns a new object and the
                    // cache dependency hasn't changed.  (Inserting with a cache dependency that has already changed will cause recursion.)
                    if (expensiveObject != null && (cacheDependency == null || !cacheDependency.HasChanged)) {
                        HttpRuntime.Cache.Insert(entry.Key, expensiveObject, cacheDependency, absoluteExpiration, slidingExpiration, entry.CacheItemUpdateCallback);
                    }
                    else {
                        HttpRuntime.Cache.Remove(entry.Key);
                    }
                }
                catch (Exception e) {
                    HttpRuntime.Cache.Remove(entry.Key);
                    try {
                        WebBaseEvent.RaiseRuntimeError(e, value);
                    }
                    catch {
                    }
                }
            }
        }

        /// <devdoc>
        ///    <para>Retrieves an item from the cache.</para>
        /// </devdoc>
        public object Get(string key) {
            return ObjectCache.Get(key);
        }


        /// <devdoc>
        ///    <para>Inserts an item into the Cache with default values.</para>
        /// </devdoc>
        public void Insert(string key, object value) {
            ObjectCache.Insert(key, value, options: null);
        }


        /// <devdoc>
        /// <para>Inserts an object into the System.Web.Caching.Cache that has file or key
        ///    dependencies.</para>
        /// </devdoc>
        public void Insert(string key, object value, CacheDependency dependencies) {
            ObjectCache.Insert(key, value, new CacheInsertOptions() { Dependencies = dependencies });
        }


        /// <devdoc>
        /// <para>Inserts an object into the System.Web.Caching.Cache that has file or key dependencies and
        ///    expires at the value set in the <paramref name="absoluteExpiration"/> parameter.</para>
        /// </devdoc>
        public void Insert(string key, object value, CacheDependency dependencies, DateTime absoluteExpiration, TimeSpan slidingExpiration) {
            DateTime utcAbsoluteExpiration = DateTimeUtil.ConvertToUniversalTime(absoluteExpiration);
            ObjectCache.Insert(key, value, new CacheInsertOptions() {
                                                Dependencies = dependencies,
                                                AbsoluteExpiration = utcAbsoluteExpiration,
                                                SlidingExpiration = slidingExpiration
                                            });
        }

        public void Insert(
                string key,
                object value,
                CacheDependency dependencies,
                DateTime absoluteExpiration,
                TimeSpan slidingExpiration,
                CacheItemPriority priority,
                CacheItemRemovedCallback onRemoveCallback) {

            DateTime utcAbsoluteExpiration = DateTimeUtil.ConvertToUniversalTime(absoluteExpiration);
            ObjectCache.Insert(key, value, new CacheInsertOptions() {
                                                Dependencies = dependencies,
                                                AbsoluteExpiration = utcAbsoluteExpiration,
                                                SlidingExpiration = slidingExpiration,
                                                Priority = priority,
                                                OnRemovedCallback = onRemoveCallback
                                            });
        }

        // DevDiv Bugs 162763: 
        // Add a an event that fires *before* an item is evicted from the ASP.NET Cache
        public void Insert(
                string key,
                object value,
                CacheDependency dependencies,
                DateTime absoluteExpiration,
                TimeSpan slidingExpiration,
                CacheItemUpdateCallback onUpdateCallback) {

            if (dependencies == null && absoluteExpiration == Cache.NoAbsoluteExpiration && slidingExpiration == Cache.NoSlidingExpiration) {
                throw new ArgumentException(SR.GetString(SR.Invalid_Parameters_To_Insert));
            }
            if (onUpdateCallback == null) {
                throw new ArgumentNullException("onUpdateCallback");
            }
            DateTime utcAbsoluteExpiration = DateTimeUtil.ConvertToUniversalTime(absoluteExpiration);
            // Insert updatable cache entry
            ObjectCache.Insert(key, value, new CacheInsertOptions() { Priority = CacheItemPriority.NotRemovable });

            // Ensure the sentinel depends on its updatable entry
            string[] cacheKeys = { key };
            CacheDependency expensiveObjectDep = new CacheDependency(null, cacheKeys);
            if (dependencies == null) {
                dependencies = expensiveObjectDep;
            }
            else {
                AggregateCacheDependency deps = new AggregateCacheDependency();
                deps.Add(dependencies, expensiveObjectDep);
                dependencies = deps;
            }
            // Insert sentinel entry for the updatable cache entry
            HttpRuntime.Cache.InternalCache.Insert(
                        CacheInternal.PrefixValidationSentinel + key,
                        new SentinelEntry(key, expensiveObjectDep, onUpdateCallback),
                        new CacheInsertOptions() {
                            Dependencies = dependencies,
                            AbsoluteExpiration = utcAbsoluteExpiration,
                            SlidingExpiration = slidingExpiration,
                            Priority = CacheItemPriority.NotRemovable,
                            OnRemovedCallback = Cache.s_sentinelRemovedCallback
                        });
        }


        public object Add(
                string key,
                object value,
                CacheDependency dependencies,
                DateTime absoluteExpiration,
                TimeSpan slidingExpiration,
                CacheItemPriority priority,
                CacheItemRemovedCallback onRemoveCallback) {

            DateTime utcAbsoluteExpiration = DateTimeUtil.ConvertToUniversalTime(absoluteExpiration);
            return ObjectCache.Add(key, value, new CacheInsertOptions() {
                        Dependencies = dependencies,
                        AbsoluteExpiration = utcAbsoluteExpiration,
                        SlidingExpiration = slidingExpiration,
                        Priority = priority,
                        OnRemovedCallback = onRemoveCallback
                });
        }


        /// <devdoc>
        ///    <para>Removes the specified item from the cache. </para>
        /// </devdoc>
        public object Remove(string key) {
            return ObjectCache.Remove(key, CacheItemRemovedReason.Removed);
        }

        public long EffectivePrivateBytesLimit {
            get {
                return AspNetMemoryMonitor.ProcessPrivateBytesLimit;
            }
        }

        public long EffectivePercentagePhysicalMemoryLimit {
            get {
                return AspNetMemoryMonitor.PhysicalMemoryPercentageLimit;
            }
        }
    }

}

