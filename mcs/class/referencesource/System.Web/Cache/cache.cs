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
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
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

    enum CacheGetOptions {
        None                = 0,
        ReturnCacheEntry    = 0x1,
    }


    /// <devdoc>
    ///    <para>Implements the cache for a Web application. There is only one instance of
    ///       this class per application domain, and it remains valid only as long as the
    ///       application domain remains active. Information about an instance of this class
    ///       is available through the <see langword='Cache'/> property of the System.Web.HttpContext.</para>
    /// </devdoc>

    //
    // Extra notes:
    // - The Cache object contains a CacheInternal object.
    // - The CacheInternal object is either a CacheSingle, or a CacheMultiple which contains mulitple
    //  CacheSingle objects.
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

        CacheInternal   _cacheInternal;
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

        internal void SetCacheInternal(CacheInternal cacheInternal) {
            _cacheInternal = cacheInternal;
        }


        /// <devdoc>
        ///    <para>Gets the number of items stored in the cache. This value can be useful when
        ///       monitoring your application's performance or when using the ASP.NET tracing
        ///       functionality.</para>
        /// </devdoc>
        public int Count {
            get {
                return _cacheInternal.PublicCount;
            }
        }


        /// <internalonly/>
        IEnumerator IEnumerable.GetEnumerator() {
            return ((IEnumerable)_cacheInternal).GetEnumerator();
        }


        /// <devdoc>
        ///    <para>Returns a dictionary enumerator used for iterating through the key/value
        ///       pairs contained in the cache. Items can be added to or removed from the cache
        ///       while this method is enumerating through the cache items.</para>
        /// </devdoc>
        public IDictionaryEnumerator GetEnumerator() {
            return _cacheInternal.GetEnumerator();
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
            return _cacheInternal.DoGet(true, key, CacheGetOptions.None);
        }

        internal object Get(string key, CacheGetOptions getOptions) {
            return _cacheInternal.DoGet(true, key, getOptions);
        }


        /// <devdoc>
        ///    <para>Inserts an item into the Cache with default values.</para>
        /// </devdoc>
        public void Insert(string key, object value) {
            _cacheInternal.DoInsert(
                        true,
                        key,
                        value,
                        null,
                        NoAbsoluteExpiration,
                        NoSlidingExpiration,
                        CacheItemPriority.Default,
                        null,
                        true);
        }


        /// <devdoc>
        /// <para>Inserts an object into the System.Web.Caching.Cache that has file or key
        ///    dependencies.</para>
        /// </devdoc>
        public void Insert(string key, object value, CacheDependency dependencies) {
            _cacheInternal.DoInsert(
                        true,
                        key,
                        value,
                        dependencies,
                        NoAbsoluteExpiration,
                        NoSlidingExpiration,
                        CacheItemPriority.Default,
                        null,
                        true);
        }


        /// <devdoc>
        /// <para>Inserts an object into the System.Web.Caching.Cache that has file or key dependencies and
        ///    expires at the value set in the <paramref name="absoluteExpiration"/> parameter.</para>
        /// </devdoc>
        public void Insert(string key, object value, CacheDependency dependencies, DateTime absoluteExpiration, TimeSpan slidingExpiration) {
            DateTime utcAbsoluteExpiration = DateTimeUtil.ConvertToUniversalTime(absoluteExpiration);
            _cacheInternal.DoInsert(
                        true,
                        key,
                        value,
                        dependencies,
                        utcAbsoluteExpiration,
                        slidingExpiration,
                        CacheItemPriority.Default,
                        null,
                        true);
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
            _cacheInternal.DoInsert(
                        true,
                        key,
                        value,
                        dependencies,
                        utcAbsoluteExpiration,
                        slidingExpiration,
                        priority,
                        onRemoveCallback,
                        true);
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
            _cacheInternal.DoInsert (
                        true,
                        key,
                        value,
                        null,
                        Cache.NoAbsoluteExpiration,
                        Cache.NoSlidingExpiration,
                        CacheItemPriority.NotRemovable,
                        null,
                        true);
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
            _cacheInternal.DoInsert(
                        false,
                        CacheInternal.PrefixValidationSentinel + key,
                        new SentinelEntry(key, expensiveObjectDep, onUpdateCallback),
                        dependencies,
                        utcAbsoluteExpiration,
                        slidingExpiration,
                        CacheItemPriority.NotRemovable,
                        Cache.s_sentinelRemovedCallback,
                        true);
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
            return _cacheInternal.DoInsert(
                        true,
                        key,
                        value,
                        dependencies,
                        utcAbsoluteExpiration,
                        slidingExpiration,
                        priority,
                        onRemoveCallback,
                        false);
        }


        /// <devdoc>
        ///    <para>Removes the specified item from the cache. </para>
        /// </devdoc>
        public object Remove(string key) {
            CacheKey cacheKey = new CacheKey(key, true);
            return _cacheInternal.DoRemove(cacheKey, CacheItemRemovedReason.Removed);
        }

        public long EffectivePrivateBytesLimit {
            get {
                return _cacheInternal.EffectivePrivateBytesLimit;
            }
        }

        public long EffectivePercentagePhysicalMemoryLimit {
            get {
                return _cacheInternal.EffectivePercentagePhysicalMemoryLimit;
            }
        }
    }

    class CacheCommon {
        const int MEMORYSTATUS_INTERVAL_5_SECONDS = 5 * Msec.ONE_SECOND;
        const int MEMORYSTATUS_INTERVAL_30_SECONDS = 30 * Msec.ONE_SECOND;

        internal CacheInternal              _cacheInternal;
        internal Cache                      _cachePublic;
        internal protected CacheMemoryStats _cacheMemoryStats;
        private  object                     _timerLock = new object();
        private  Timer                      _timer;
        private  int                        _currentPollInterval = MEMORYSTATUS_INTERVAL_30_SECONDS;
        internal int                        _inCacheManagerThread;
        internal bool                       _enableMemoryCollection;
        internal bool                       _enableExpiration;
        internal bool                       _internalConfigRead;
        internal SRefMultiple               _srefMultiple;

        internal CacheCommon() {
            _cachePublic = new Cache(0);
            _srefMultiple = new SRefMultiple();
            _cacheMemoryStats = new CacheMemoryStats(_srefMultiple);
            _enableMemoryCollection = true;
            _enableExpiration = true;
        }

        internal void Dispose(bool disposing) {
            if (disposing) {
                EnableCacheMemoryTimer(false);
                _cacheMemoryStats.Dispose();
            }
        }

        internal void AddSRefTarget(CacheInternal c) {
            _srefMultiple.AddSRefTarget(c);
        }

        internal void SetCacheInternal(CacheInternal cacheInternal) {
            _cacheInternal = cacheInternal;
            _cachePublic.SetCacheInternal(cacheInternal);
        }

        internal void ReadCacheInternalConfig(CacheSection cacheSection) {
            if (_internalConfigRead) {
                return;
            }

            lock (this) {
                if (_internalConfigRead) {
                    return;
                }

                // Set it to true here so that even if we have to call ReadCacheInternalConfig
                // from the code below, we won't get into an infinite loop.
                _internalConfigRead = true;

                if (cacheSection != null) {
                    _enableMemoryCollection = (!cacheSection.DisableMemoryCollection);
                    _enableExpiration = (!cacheSection.DisableExpiration);
                    _cacheMemoryStats.ReadConfig(cacheSection);
                    _currentPollInterval = CacheMemorySizePressure.PollInterval;
                    ResetFromConfigSettings();
                }
            }
        }

        internal void ResetFromConfigSettings() {
            EnableCacheMemoryTimer(_enableMemoryCollection);
            _cacheInternal.EnableExpirationTimer(_enableExpiration);
        }

        internal void EnableCacheMemoryTimer(bool enable) {
            lock (_timerLock) {
#if DBG
                if (Debug.IsTagPresent("Timer") && !Debug.IsTagEnabled("Timer")) {
                    enable = false;
                }
                
#endif
                
                if (enable) {
                    
                    if (_timer == null) {
                        // <cache privateBytesPollTime> has not been read yet
                        _timer = new Timer(new TimerCallback(this.CacheManagerTimerCallback), null, _currentPollInterval, _currentPollInterval);
                        Debug.Trace("Cache", "Started CacheMemoryTimers");
                    }
                    else {
                        _timer.Change(_currentPollInterval, _currentPollInterval);
                    }
                }
                else {
                    Timer timer = _timer;
                    if (timer != null && Interlocked.CompareExchange(ref _timer, null, timer) == timer) {
                        timer.Dispose();
                        Debug.Trace("Cache", "Stopped CacheMemoryTimers");
                    }
                }
            }

            if (!enable) {
                // wait for CacheManagerTimerCallback to finish
                while(_inCacheManagerThread != 0) {
                    Thread.Sleep(100);
                }
            }
        }

        void AdjustTimer() {
            lock (_timerLock) {

                if (_timer == null)
                    return;

                // the order of these if statements is important
                
                // When above the high pressure mark, interval should be 5 seconds or less
                if (_cacheMemoryStats.IsAboveHighPressure()) {
                    if (_currentPollInterval > MEMORYSTATUS_INTERVAL_5_SECONDS) {
                        _currentPollInterval = MEMORYSTATUS_INTERVAL_5_SECONDS;
                        _timer.Change(_currentPollInterval, _currentPollInterval);
                    }
                    return;
                }
                
                // When above half the low pressure mark, interval should be 30 seconds or less
                if ((_cacheMemoryStats.CacheSizePressure.PressureLast > _cacheMemoryStats.CacheSizePressure.PressureLow/2)
                    || (_cacheMemoryStats.TotalMemoryPressure.PressureLast > _cacheMemoryStats.TotalMemoryPressure.PressureLow/2)) {
                    // DevDivBugs 104034: allow interval to fall back down when memory pressure goes away
                    int newPollInterval = Math.Min(CacheMemorySizePressure.PollInterval, MEMORYSTATUS_INTERVAL_30_SECONDS);
                    if (_currentPollInterval != newPollInterval) {
                        _currentPollInterval = newPollInterval;
                        _timer.Change(_currentPollInterval, _currentPollInterval);
                    }
                    return;
                }
                
                // there is no pressure, interval should be the value from config
                if (_currentPollInterval != CacheMemorySizePressure.PollInterval) {
                    _currentPollInterval = CacheMemorySizePressure.PollInterval;
                    _timer.Change(_currentPollInterval, _currentPollInterval);
                }
            }
        }

        void CacheManagerTimerCallback(object state) {
            CacheManagerThread(0);
        }
        
        internal long CacheManagerThread(int minPercent) {
            if (Interlocked.Exchange(ref _inCacheManagerThread, 1) != 0)
                return 0;
#if DBG
            Debug.Trace("CacheMemory", "**BEG** CacheManagerThread " + HttpRuntime.AppDomainAppId + ", " + DateTime.Now.ToString("T", CultureInfo.InvariantCulture));
#endif
            try {
                // Dev10 633335: if the timer has been disposed, return without doing anything
                if (_timer == null)
                    return 0;

                // The timer thread must always call Update so that the CacheManager
                // knows the size of the cache.
                _cacheMemoryStats.Update();
                AdjustTimer();
                int percent = Math.Max(minPercent, _cacheMemoryStats.GetPercentToTrim());
                long beginTotalCount = _cacheInternal.TotalCount;
                Stopwatch sw = Stopwatch.StartNew();
                long trimmedOrExpired = _cacheInternal.TrimIfNecessary(percent);
                sw.Stop();
                // 1) don't update stats if the trim happend because MAX_COUNT was exceeded
                // 2) don't update stats unless we removed at least one entry
                if (percent > 0 && trimmedOrExpired > 0) {
                    _cacheMemoryStats.SetTrimStats(sw.Elapsed.Ticks, beginTotalCount, trimmedOrExpired);
                }

#if DBG
                Debug.Trace("CacheMemory", "**END** CacheManagerThread: " + HttpRuntime.AppDomainAppId
                            + ", percent=" + percent
                            + ", beginTotalCount=" + beginTotalCount
                            + ", trimmed=" + trimmedOrExpired
                            + ", Milliseconds=" + sw.ElapsedMilliseconds);
#endif

#if PERF
                SafeNativeMethods.OutputDebugString("CacheCommon.CacheManagerThread:"
                                                    + " minPercent= " + minPercent
                                                    + ", percent= " + percent
                                                    + ", beginTotalCount=" + beginTotalCount
                                                    + ", trimmed=" + trimmedOrExpired
                                                    + ", Milliseconds=" + sw.ElapsedMilliseconds + "\n");
#endif
                return trimmedOrExpired;
            }
            finally {
                Interlocked.Exchange(ref _inCacheManagerThread, 0);
            }            
        }
    }

    abstract class CacheInternal : IEnumerable, IDisposable {
        // cache key prefixes - they keep cache keys short and prevent conflicts

        // NOTE: Since we already used up all the lowercase letters from 'a' to 'z',
        // we are now using uppercase letters from 'A' to 'Z'
        internal const string PrefixFIRST                   = "A";
        internal const string PrefixResourceProvider        = "A";
        internal const string PrefixMapPathVPPFile          = "Bf";
        internal const string PrefixMapPathVPPDir           = "Bd";

        // Next prefix goes here, until we get to 'Z'

        internal const string PrefixOutputCache             = "a";
        internal const string PrefixSqlCacheDependency      = "b";
        internal const string PrefixMemoryBuildResult       = "c";
        internal const string PrefixPathData                = "d";
        internal const string PrefixHttpCapabilities        = "e";
        internal const string PrefixMapPath                 = "f";
        internal const string PrefixHttpSys                 = "g";
        internal const string PrefixFileSecurity            = "h";
        internal const string PrefixInProcSessionState      = "j";
        internal const string PrefixStateApplication        = "k";
        internal const string PrefixPartialCachingControl   = "l";
        internal const string UNUSED                        = "m";
        internal const string PrefixAdRotator               = "n";
        internal const string PrefixWebServiceDataSource    = "o";
        internal const string PrefixLoadXPath               = "p";
        internal const string PrefixLoadXml                 = "q";
        internal const string PrefixLoadTransform           = "r";
        internal const string PrefixAspCompatThreading      = "s";
        internal const string PrefixDataSourceControl       = "u";
        internal const string PrefixValidationSentinel      = "w";
        internal const string PrefixWebEventResource        = "x";
        internal const string PrefixAssemblyPath            = "y";
        internal const string PrefixBrowserCapsHash         = "z";
        internal const string PrefixLAST                    = "z";

        protected CacheCommon _cacheCommon;
        private int _disposed;

        // virtual methods requiring implementation
        internal abstract int PublicCount   {get;}

        internal abstract long TotalCount   {get;}

        internal abstract IDictionaryEnumerator CreateEnumerator();

        internal abstract CacheEntry UpdateCache(
                CacheKey                cacheKey,
                CacheEntry              newEntry,
                bool                    replace,
                CacheItemRemovedReason  removedReason,
                out object              valueOld);

        internal abstract long TrimIfNecessary(int percent);

        internal abstract void EnableExpirationTimer(bool enable);

        // If UseMemoryCache is true, we will direct all ASP.NET
        // cache usage into System.Runtime.Caching.dll.  This allows
        // us to test System.Runtime.Caching.dll with all existing
        // ASP.NET test cases (functional, perf, and stress).
#if USE_MEMORY_CACHE
        private static bool _useMemoryCache;
        private static volatile bool _useMemoryCacheInited;
        internal static bool UseMemoryCache {
            get {
                if (!_useMemoryCacheInited) {
                    RegistryKey regKey = null;
                    try {
                        regKey = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\ASP.NET");
                        if (regKey != null) {
                            if ((int)regKey.GetValue("UseMemoryCache", 0)== 1) {
                                _useMemoryCache = true;
                            }
                        }
                    }
                    finally {
                        if (regKey != null) {
                            regKey.Close();
                        }
                    }
                    _useMemoryCacheInited = true;
                }
                return _useMemoryCache;
            }
        }
#endif

        // common implementation
        static internal CacheInternal Create() {
            CacheCommon cacheCommon = new CacheCommon();
            CacheInternal cacheInternal;
#if USE_MEMORY_CACHE
            if (UseMemoryCache) {
                cacheInternal = new MemCache(cacheCommon);
                cacheCommon.AddSRefTarget(cacheInternal);
            }
            else {
#endif
                int numSubCaches = 0;
                uint numCPUs = (uint) SystemInfo.GetNumProcessCPUs();
                // the number of subcaches is the minimal power of 2 greater
                // than or equal to the number of cpus
                numSubCaches = 1;
                numCPUs -= 1;
                while (numCPUs > 0) {
                    numSubCaches <<= 1;
                    numCPUs >>= 1;
                }
                if (numSubCaches == 1) {
                    cacheInternal = new CacheSingle(cacheCommon, null, 0);
                }
                else {
                    cacheInternal = new CacheMultiple(cacheCommon, numSubCaches);
                }
#if USE_MEMORY_CACHE
            }
#endif
            cacheCommon.SetCacheInternal(cacheInternal);
            cacheCommon.ResetFromConfigSettings();

            return cacheInternal;
        }

        protected CacheInternal(CacheCommon cacheCommon) {
            _cacheCommon = cacheCommon;
        }

        protected virtual void Dispose(bool disposing) {
            _cacheCommon.Dispose(disposing);
        }

        public void Dispose() {
            _disposed = 1;
            Dispose(true);
            // no destructor, don't need it.
            // System.GC.SuppressFinalize(this);
        }

        internal bool IsDisposed { get { return _disposed == 1; } }

        internal void ReadCacheInternalConfig(CacheSection cacheSection) {
            _cacheCommon.ReadCacheInternalConfig(cacheSection);
        }

        internal long TrimCache(int percent) {
            return _cacheCommon.CacheManagerThread(percent);
        }

        internal Cache CachePublic {
            get {return _cacheCommon._cachePublic;}
        }

        internal long EffectivePrivateBytesLimit {
            get { return _cacheCommon._cacheMemoryStats.CacheSizePressure.MemoryLimit; }
        }

        internal long EffectivePercentagePhysicalMemoryLimit {
            get { return _cacheCommon._cacheMemoryStats.TotalMemoryPressure.MemoryLimit; }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return CreateEnumerator();
        }

        public IDictionaryEnumerator GetEnumerator() {
            return CreateEnumerator();
        }

        internal object this[string key] {
            get {
                return Get(key);
            }
        }

        internal object Get(string key) {
            return DoGet(false, key, CacheGetOptions.None);
        }

        internal object Get(string key, CacheGetOptions getOptions) {
            return DoGet(false, key, getOptions);
        }

        internal object DoGet(bool isPublic, string key, CacheGetOptions getOptions) {
            CacheEntry  entry;
            CacheKey    cacheKey;
            object      dummy;

            cacheKey = new CacheKey(key, isPublic);
            entry = UpdateCache(cacheKey, null, false, CacheItemRemovedReason.Removed, out dummy);
            if (entry != null) {
                if ((getOptions & CacheGetOptions.ReturnCacheEntry) != 0) {
                    return entry;
                }
                else {
                    return entry.Value;
                }
            }
            else {
                return null;
            }
        }

        internal void UtcInsert(string key, object value) {
            DoInsert(false,
                     key,
                     value,
                     null,
                     Cache.NoAbsoluteExpiration,
                     Cache.NoSlidingExpiration,
                     CacheItemPriority.Default,
                     null,
                     true);

        }

        internal void UtcInsert(string key, object value, CacheDependency dependencies) {
            DoInsert(false,
                     key,
                     value,
                     dependencies,
                     Cache.NoAbsoluteExpiration,
                     Cache.NoSlidingExpiration,
                     CacheItemPriority.Default,
                     null,
                     true);
        }

        internal void UtcInsert(
                string key,
                object value,
                CacheDependency dependencies,
                DateTime utcAbsoluteExpiration,
                TimeSpan slidingExpiration) {

            DoInsert(false,
                     key,
                     value,
                     dependencies,
                     utcAbsoluteExpiration,
                     slidingExpiration,
                     CacheItemPriority.Default,
                     null,
                     true);
        }

        internal void UtcInsert(
                string key,
                object value,
                CacheDependency dependencies,
                DateTime utcAbsoluteExpiration,
                TimeSpan slidingExpiration,
                CacheItemPriority priority,
                CacheItemRemovedCallback onRemoveCallback) {

            DoInsert(false,
                     key,
                     value,
                     dependencies,
                     utcAbsoluteExpiration,
                     slidingExpiration,
                     priority,
                     onRemoveCallback,
                     true);
        }

        internal object UtcAdd(
                string key,
                object value,
                CacheDependency dependencies,
                DateTime utcAbsoluteExpiration,
                TimeSpan slidingExpiration,
                CacheItemPriority priority,
                CacheItemRemovedCallback onRemoveCallback) {

            return DoInsert(
                        false,
                        key,
                        value,
                        dependencies,
                        utcAbsoluteExpiration,
                        slidingExpiration,
                        priority,
                        onRemoveCallback,
                        false);

        }

        internal object DoInsert(
                bool isPublic,
                string key,
                object value,
                CacheDependency dependencies,
                DateTime utcAbsoluteExpiration,
                TimeSpan slidingExpiration,
                CacheItemPriority priority,
                CacheItemRemovedCallback onRemoveCallback,
                bool replace) {


            /*
             * If we throw an exception, prevent a leak by a user who
             * writes the following:
             *
             *     Cache.Insert(key, value, new CacheDependency(file));
             */
            using (dependencies) {
                CacheEntry      entry;
                object          dummy;

                entry = new CacheEntry(
                        key,
                        value,
                        dependencies,
                        onRemoveCallback,
                        utcAbsoluteExpiration,
                        slidingExpiration,
                        priority,
                        isPublic);

                entry = UpdateCache(entry, entry, replace, CacheItemRemovedReason.Removed, out dummy);

                /*
                 * N.B. A set can fail if two or more threads set the same key
                 * at the same time.
                 */
#if DBG
                if (replace) {
                    string yesno = (entry != null) ? "succeeded" : "failed";
                    Debug.Trace("CacheAPIInsert", "Cache.Insert " + yesno + ": " + key);
                }
                else {
                    if (entry == null) {
                        Debug.Trace("CacheAPIAdd", "Cache.Add added new item: " + key);
                    }
                    else {
                        Debug.Trace("CacheAPIAdd", "Cache.Add returned existing item: " + key);
                    }
                }
#endif

                if (entry != null) {
                    return entry.Value;
                }
                else {
                    return null;
                }
            }
        }

        internal object Remove(string key) {
            CacheKey cacheKey = new CacheKey(key, false);
            return DoRemove(cacheKey, CacheItemRemovedReason.Removed);
        }

        internal object Remove(CacheKey cacheKey, CacheItemRemovedReason reason)  {
            return DoRemove(cacheKey, reason);
        }

        /*
         * Remove an item from the cache, with a specific reason.
         * This is package access so only the cache can specify
         * a reason other than REMOVED.
         *
         * @param key The key for the item.
         * @exception ArgumentException
         */
        internal object DoRemove(CacheKey cacheKey, CacheItemRemovedReason reason)  {
            object      valueOld;

            UpdateCache(cacheKey, null, true, reason, out valueOld);

#if DBG
            if (valueOld != null) {
                Debug.Trace("CacheAPIRemove", "Cache.Remove succeeded, reason=" + reason + ": " + cacheKey);
            }
            else {
                Debug.Trace("CacheAPIRemove", "Cache.Remove failed, reason=" + reason + ": " + cacheKey);
            }
#endif

            return valueOld;
        }
    }

    sealed class CacheKeyComparer : IEqualityComparer  {
        static CacheKeyComparer    s_comparerInstance;

        static internal CacheKeyComparer GetInstance() {
            if (s_comparerInstance == null) {
                s_comparerInstance = new CacheKeyComparer();
            }

            return s_comparerInstance;
        }

        private CacheKeyComparer()
        {
        }

        bool IEqualityComparer.Equals(Object x, Object y)
        {
            return Compare(x, y) == 0;
        }

        // Compares two objects. An implementation of this method must return a
        // value less than zero if x is less than y, zero if x is equal to y, or a
        // value greater than zero if x is greater than y.
        private int Compare(Object x, Object y) {
            CacheKey  a, b;

            Debug.Assert(x != null && x is CacheKey);
            Debug.Assert(y != null && y is CacheKey);

            a = (CacheKey) x;
            b = (CacheKey) y;

            if (a.IsPublic) {
                if (b.IsPublic) {
                    return String.Compare(a.Key, b.Key, StringComparison.Ordinal);
                }
                else {
                    return 1;
                }
            }
            else {
                if (!b.IsPublic) {
                    return String.Compare(a.Key, b.Key, StringComparison.Ordinal);
                }
                else {
                    return -1;
                }
            }
        }
        // Returns a hash code for the given object.
        //
        int IEqualityComparer.GetHashCode(Object obj) {
            Debug.Assert(obj != null && obj is CacheKey);

            CacheKey cacheKey = (CacheKey) obj;

            return cacheKey.GetHashCode();
        }
    }

    /*
     * The cache.
     */
    sealed class CacheSingle : CacheInternal {
        // cache stats
        static readonly TimeSpan    INSERT_BLOCK_WAIT = new TimeSpan(0, 0, 10);
        const int                   MAX_COUNT = Int32.MaxValue / 2;
        const int                   MIN_COUNT = 10;


        Hashtable           _entries;           /* lookup table of entries */
        CacheExpires        _expires;           /* expires tables */
        CacheUsage          _usage;             /* usage tables */
        object              _lock;              /* read/write synchronization for _entries */
        int                 _disposed;          /* disposed */
        int                 _totalCount;        /* count of total entries */
        int                 _publicCount;       /* count of public entries */
        ManualResetEvent    _insertBlock;       /* event to block inserts during high mem usage */
        bool                _useInsertBlock;    /* use insert block? */
        int                 _insertBlockCalls;  /* number of callers using insert block */
        int                 _iSubCache;         /* index of this cache */
        CacheMultiple       _cacheMultiple;     /* the CacheMultiple containing this cache */

        /*
         * Constructs a new Cache.
         */
        internal CacheSingle(CacheCommon cacheCommon, CacheMultiple cacheMultiple, int iSubCache) : base(cacheCommon) {
            _cacheMultiple = cacheMultiple;
            _iSubCache = iSubCache;
            _entries = new Hashtable(CacheKeyComparer.GetInstance());
            _expires = new CacheExpires(this);
            _usage = new CacheUsage(this);
            _lock = new object();
            _insertBlock = new ManualResetEvent(true);
            cacheCommon.AddSRefTarget(this);
        }

        /*
         * Dispose the cache.
         */
        protected override void Dispose(bool disposing) {
            if (disposing) {
                if (Interlocked.Exchange(ref _disposed, 1) == 0) {
                    if (_expires != null) {
                        _expires.EnableExpirationTimer(false);
                    }

                    // close all items
                    CacheEntry[] entries = null;

                    lock (_lock) {
                        entries = new CacheEntry[_entries.Count];
                        int i = 0;
                        foreach (DictionaryEntry d in _entries) {
                            entries[i++] = (CacheEntry) d.Value;
                        }
                    }

                    foreach (CacheEntry entry in entries) {
                        Remove(entry, CacheItemRemovedReason.Removed);
                    }

                    // force any waiters to complete their waits. Note
                    // that the insert block cannot be reacquired, as UseInsertBlock
                    // checks the _disposed field.
                    _insertBlock.Set();

                    // release the block, causing it to be disposed when there
                    // are no more callers.
                    ReleaseInsertBlock();

                    Debug.Trace("CacheDispose", "Cache disposed");
                }
            }

            base.Dispose(disposing);
        }

        // Get the insert block manual reset event if it has not been disposed.
        ManualResetEvent UseInsertBlock() {
            for (;;) {
                if (_disposed == 1)
                    return null;

                int n = _insertBlockCalls;
                if (n < 0) {
                    return null;
                }

                if (Interlocked.CompareExchange(ref _insertBlockCalls, n + 1, n) == n) {
                    return _insertBlock;
                }
            }
        }

        // Release the insert block event, and dispose it if it has been released
        // more times than it has been used
        void ReleaseInsertBlock() {
            if (Interlocked.Decrement(ref _insertBlockCalls) < 0) {
                ManualResetEvent e = _insertBlock;
                _insertBlock = null;

                // now close
                e.Close();
            }
        }

        // Set the insert block event.
        void SetInsertBlock() {
            ManualResetEvent e = null;
            try {
                e = UseInsertBlock();
                if (e != null) {
                    e.Set();
                }
            }
            finally {
                if (e != null) {
                    ReleaseInsertBlock();
                }
            }
        }

        // Reset the insert block event.
        void ResetInsertBlock() {
            ManualResetEvent e = null;
            try {
                e = UseInsertBlock();
                if (e != null) {
                    e.Reset();
                }
            }
            finally {
                if (e != null) {
                    ReleaseInsertBlock();
                }
            }
        }

        // Wait on the insert block event.
        bool WaitInsertBlock() {
            bool signaled = false;
            ManualResetEvent e = null;
            try {
                e = UseInsertBlock();
                if (e != null) {
                    Debug.Trace("CacheMemoryTrimInsertBlock", "WaitInsertBlock: Cache " + _iSubCache + ": _useInsertBlock=true");
                    signaled = e.WaitOne(INSERT_BLOCK_WAIT, false);
                    Debug.Trace("CacheMemoryTrimInsertBlock", "Done waiting");
                }
            }
            finally {
                if (e != null) {
                    ReleaseInsertBlock();
                }
            }

            return signaled;
        }

        internal void BlockInsertIfNeeded() {
            if (_cacheCommon._cacheMemoryStats.IsAboveHighPressure()) {
                Debug.Trace("CacheMemoryTrimInsertBlock", "BlockInsertIfNeeded: Cache " + _iSubCache + ": _useInsertBlock=true");
                _useInsertBlock = true;
                ResetInsertBlock();
            }
        }

        internal void UnblockInsert() {
            if (_useInsertBlock) {
                _useInsertBlock = false;
                SetInsertBlock();
                Debug.Trace("CacheMemoryTrimInsertBlock", "UnblockInsert: Cache " + _iSubCache + ": _useInsertBlock=false");
            }
        }


        internal override int PublicCount {
            get {return _publicCount;}
        }

        internal override long TotalCount {
            get {return _totalCount;}
        }

        internal override IDictionaryEnumerator CreateEnumerator() {
            Hashtable h = new Hashtable(_publicCount);
            DateTime utcNow = DateTime.UtcNow;

            lock (_lock) {
                foreach (DictionaryEntry d in _entries) {
                    CacheEntry entry = (CacheEntry) d.Value;

                    // note that ASP.NET does not use this enumerator internally,
                    // so we just choose public items.
                    if (entry.IsPublic &&
                        entry.State == CacheEntry.EntryState.AddedToCache &&
                        ((!_cacheCommon._enableExpiration) || (utcNow <= entry.UtcExpires))) {
                        h[entry.Key] = entry.Value;
                    }
                }
            }

            return h.GetEnumerator();
        }

        /*
         * Performs all operations on the cache, with the
         * exception of Clear. The arguments indicate the type of operation:
         *
         * @param key The key of the object.
         * @param newItem The new entry to be added to the cache.
         * @param replace Whether or not newEntry should replace an existing object in the cache.
         * @return The item requested. May be null.
         */
        internal override CacheEntry UpdateCache(
                CacheKey                cacheKey,
                CacheEntry              newEntry,
                bool                    replace,
                CacheItemRemovedReason  removedReason,
                out object              valueOld)
        {
            CacheEntry              entry = null;
            CacheEntry              oldEntry = null;
            bool                    expired = false;
            DateTime                utcNow;
            CacheDependency         newEntryDependency = null;
            bool                    isGet, isAdd;
            bool                    removeExpired = false;
            bool                    updateExpires = false;
            DateTime                utcNewExpires = DateTime.MinValue;
            CacheEntry.EntryState   entryState = CacheEntry.EntryState.NotInCache;
            bool                    newEntryNeedsClose = false;
            CacheItemRemovedReason  newEntryRemovedReason = CacheItemRemovedReason.Removed;

            valueOld = null;
            isGet = !replace && newEntry == null;
            isAdd = !replace && newEntry != null;

            /*
             * Perform update of cache data structures in a series to
             * avoid overlapping locks.
             *
             * First, update the hashtable. The hashtable is the place
             * that guarantees what is in or out of the cache.
             *
             * Loop here to remove expired items in a Get or Add, where
             * we can't otherwise delete an item.
             */
            for (;;) {
                if (removeExpired) {
                    Debug.Trace("CacheUpdate", "Removing expired item found in Get: " + cacheKey);
                    UpdateCache(cacheKey, null, true, CacheItemRemovedReason.Expired, out valueOld);
                    removeExpired = false;
                }

                entry = null;
                utcNow = DateTime.UtcNow;

                if (_useInsertBlock && newEntry != null && newEntry.HasUsage() /* HasUsage() means it's not NonRemovable */) {
                    bool insertBlockReleased = WaitInsertBlock();

#if DBG
                    if (!insertBlockReleased) {
                        Debug.Trace("CacheUpdateWaitFailed", "WaitInsertBlock failed.");
                    }
#endif
                }

                // the _entries hashtable supports multiple readers or one writer
                bool isLockEntered = false;
                if (!isGet) {
                    Monitor.Enter(_lock, ref isLockEntered);
                }
                try {
                    entry = (CacheEntry) _entries[cacheKey];
                    Debug.Trace("CacheUpdate", "Entry " + ((entry != null) ? "found" : "not found") + "in hashtable: " + cacheKey);

                    if (entry != null) {
                        entryState = entry.State;

                        // If isGet == true, we are not hold any lock and so entryState can be anything
                        Debug.Assert(
                            isGet ||
                            entryState == CacheEntry.EntryState.AddingToCache ||
                            entryState == CacheEntry.EntryState.AddedToCache,
                            "entryState == CacheEntry.EntryState.AddingToCache || entryState == CacheEntry.EntryState.AddedToCache");

                        expired = (_cacheCommon._enableExpiration) && (entry.UtcExpires < utcNow);
                        if (expired) {
                            if (isGet) {
                                /*
                                 * If the expired item is Added to the cache, remove it now before
                                 * its expiration timer fires up to a minute in the future.
                                 * Otherwise, just return null to indicate the item is not available.
                                 */
                                if (entryState == CacheEntry.EntryState.AddedToCache) {
                                    removeExpired = true;
                                    continue;
                                }

                                entry = null;
                            }
                            else {
                                /*
                                 * If it's a call to Add, replace the item
                                 * when it has expired.
                                 */
                                replace = true;

                                /*
                                 * Change the removed reason.
                                 */
                                removedReason = CacheItemRemovedReason.Expired;
                            }
                        }
                        else {
                            updateExpires = (_cacheCommon._enableExpiration) && (entry.SlidingExpiration > TimeSpan.Zero);
                        }
                    }

                    /*
                     * Avoid running unnecessary code in a Get request by this simple test:
                     */
                    if (!isGet) {
                        /*
                         * Remove an item from the hashtable.
                         */
                        if (replace && entry != null) {
                            bool doRemove = (entryState != CacheEntry.EntryState.AddingToCache);
                            if (doRemove) {
                                oldEntry = entry;

                                oldEntry.State = CacheEntry.EntryState.RemovingFromCache;

                                _entries.Remove(oldEntry);
                                Debug.Trace("CacheUpdate", "Entry removed from hashtable: " + cacheKey);
                            }
                            else {
                                /*
                                 * If we're removing and couldn't remove the old item
                                 * because its state was AddingToCache, return null
                                 * to indicate failure.
                                 */
                                if (newEntry == null) {
                                    Debug.Trace("CacheUpdate", "Removal from hashtable failed: " + cacheKey);
                                    entry = null;
                                }
                            }
                        }

                        /*
                         * Add an item to the hashtable.
                         */
                        if (newEntry != null) {
                            bool doAdd = true;

                            if (entry != null) {
                                if (oldEntry == null) {
                                    /*
                                     * We could not remove the existing entry,
                                     * either because it simply exists and replace == false,
                                     * or replace == true and it's state was AddingToCache when
                                     * we tried to remove it.
                                    */
                                    doAdd = false;
                                    newEntryRemovedReason = CacheItemRemovedReason.Removed;
                                }

#if DBG
                                if (!doAdd) {
                                    Debug.Trace("CacheUpdate", "Insertion into hashtable failed because old entry was not removed: " + cacheKey);
                                }
#endif
                            }


                            if (doAdd) {
                                /* non-definitive check */
                                newEntryDependency = newEntry.Dependency;
                                if (newEntryDependency != null) {
                                    if (newEntryDependency.HasChanged) {
                                        doAdd = false;
                                        newEntryRemovedReason = CacheItemRemovedReason.DependencyChanged;
                                    }

#if DBG
                                    if (!doAdd) {
                                        Debug.Trace("CacheUpdate", "Insertion into hashtable failed because dependency changed: " + cacheKey);
                                    }
#endif
                                }
                            }

                            if (doAdd) {
                                newEntry.State = CacheEntry.EntryState.AddingToCache;
                                _entries.Add(newEntry, newEntry);

                                /*
                                 * If this is an Add operation, indicate success
                                 * by returning null.
                                 */
                                if (isAdd) {
                                    Debug.Assert(entry == null || expired, "entry == null || expired");
                                    entry = null;
                                }
                                else {
                                    /*
                                     * Indicate success by returning the inserted entry.
                                     */
                                    entry = newEntry;
                                }

                                Debug.Trace("CacheUpdate", "Entry added to hashtable: " + cacheKey);
                            }
                            else {
                                if (!isAdd) {
                                    /*
                                     * If we failed for an Insert, indicate failure by returning null.
                                     */
                                    entry = null;
                                    newEntryNeedsClose = true;
                                }
                                else {
                                    /*
                                     * If we failed for an Add (e.g. Dependency has changed),
                                     * return the existing value. If existing value is null,
                                     * we have to close the newEntry ourselves.  Otherwise, we'll
                                     * return non-null and the caller should close the item.
                                     */
                                    newEntryNeedsClose = (entry == null);
                                }

                                /*
                                 * If newEntry cannot be inserted, and it does not need to be
                                 * closed, set it to null so that we don't insert it later.
                                 * Leave it non-null when it needs to be closed that that we
                                 * can close it.
                                 */
                                if (!newEntryNeedsClose) {
                                    newEntry = null;
                                }

                            }
                        }
                    }

                    break;
                }
                finally {
                    if (isLockEntered) {
                        Monitor.Exit(_lock);
                    }
                }
            }

            /*
             * Since we want Get to be fast, check here for a get without
             * alteration to cache.
             */
            if (isGet) {
                if (entry != null) {
                    if (updateExpires) {
                        utcNewExpires = utcNow + entry.SlidingExpiration;
                        if (utcNewExpires - entry.UtcExpires >= CacheExpires.MIN_UPDATE_DELTA || utcNewExpires < entry.UtcExpires) {
                            _expires.UtcUpdate(entry, utcNewExpires);
                        }
                    }

                    UtcUpdateUsageRecursive(entry, utcNow);
                }

                if (cacheKey.IsPublic) {
                    PerfCounters.IncrementCounter(AppPerfCounter.API_CACHE_RATIO_BASE);
                    if (entry != null) {
                        PerfCounters.IncrementCounter(AppPerfCounter.API_CACHE_HITS);
                    }
                    else {
                        PerfCounters.IncrementCounter(AppPerfCounter.API_CACHE_MISSES);
                    }
                }

                PerfCounters.IncrementCounter(AppPerfCounter.TOTAL_CACHE_RATIO_BASE);
                if (entry != null) {
                    PerfCounters.IncrementCounter(AppPerfCounter.TOTAL_CACHE_HITS);
                }
                else {
                    PerfCounters.IncrementCounter(AppPerfCounter.TOTAL_CACHE_MISSES);
                }

#if DBG
                if (entry != null) {
                    Debug.Trace("CacheUpdate", "Cache hit: " + cacheKey);
                }
                else {
                    Debug.Trace("CacheUpdate", "Cache miss: " + cacheKey);
                }
#endif

            }
            else {
                int totalDelta = 0;
                int publicDelta = 0;
                int totalTurnover = 0;
                int publicTurnover = 0;

                if (oldEntry != null) {
                    if (oldEntry.InExpires()) {
                        _expires.Remove(oldEntry);
                    }

                    if (oldEntry.InUsage()) {
                        _usage.Remove(oldEntry);
                    }

                    Debug.Assert(oldEntry.State == CacheEntry.EntryState.RemovingFromCache, "oldEntry.State == CacheEntry.EntryState.RemovingFromCache");
                    oldEntry.State = CacheEntry.EntryState.RemovedFromCache;
                    valueOld = oldEntry.Value;

                    totalDelta--;
                    totalTurnover++;
                    if (oldEntry.IsPublic) {
                        publicDelta--;
                        publicTurnover++;
                    }

#if DBG
                    Debug.Trace("CacheUpdate", "Entry removed from cache, reason=" + removedReason + ": " + (CacheKey) oldEntry);
#endif
                }

                if (newEntry != null) {
                    if (newEntryNeedsClose) {
                        // Call close if newEntry could not be added.
                        newEntry.State = CacheEntry.EntryState.RemovedFromCache;
                        newEntry.Close(newEntryRemovedReason);
                        newEntry = null;
                    }
                    else {
                        Debug.Assert(!newEntry.InExpires());
                        Debug.Assert(!newEntry.InUsage());

                        if (_cacheCommon._enableExpiration && newEntry.HasExpiration()) {
                            _expires.Add(newEntry);
                        }

                        if (    _cacheCommon._enableMemoryCollection && newEntry.HasUsage() &&
                                (   // Don't bother to set usage if it's going to expire very soon
                                    !newEntry.HasExpiration() ||
                                    newEntry.SlidingExpiration > TimeSpan.Zero ||
                                    newEntry.UtcExpires - utcNow >= CacheUsage.MIN_LIFETIME_FOR_USAGE)) {

                            _usage.Add(newEntry);
                        }

                        newEntry.State = CacheEntry.EntryState.AddedToCache;

                        Debug.Trace("CacheUpdate", "Entry added to cache: " + (CacheKey)newEntry);

                        totalDelta++;
                        totalTurnover++;
                        if (newEntry.IsPublic) {
                            publicDelta++;
                            publicTurnover++;
                        }
                    }
                }

                // Call close after the newEntry has been fully added to the cache,
                // so the OnRemoveCallback can take a dependency on the newly inserted item.
                if (oldEntry != null) {
                    oldEntry.Close(removedReason);
                }

                // Delay monitoring change events until the oldEntry has been completely removed
                // from the cache, and its OnRemoveCallback called. This way we won't call the
                // OnRemoveCallback for newEntry before doing so for oldEntry.
                if (newEntry != null) {
                    // listen to change events
                    newEntry.MonitorDependencyChanges();

                    /*
                     * NB: We have to check for dependency changes after we add the item
                     * to cache, because otherwise we may not remove it if it changes
                     * between the time we check for a dependency change and the time
                     * we set the AddedToCache bit. The worst that will happen is that
                     * a get can occur on an item that has changed, but that can happen
                     * anyway. The important thing is that we always remove an item that
                     * has changed.
                     */
                    if (newEntryDependency != null && newEntryDependency.HasChanged) {
                        Remove(newEntry, CacheItemRemovedReason.DependencyChanged);
                    }
                }
                
                // update counts and counters
                if (totalDelta == 1) {
                    Interlocked.Increment(ref _totalCount);
                    PerfCounters.IncrementCounter(AppPerfCounter.TOTAL_CACHE_ENTRIES);
                }
                else if (totalDelta == -1) {
                    Interlocked.Decrement(ref _totalCount);
                    PerfCounters.DecrementCounter(AppPerfCounter.TOTAL_CACHE_ENTRIES);
                }

                if (publicDelta == 1) {
                    Interlocked.Increment(ref _publicCount);
                    PerfCounters.IncrementCounter(AppPerfCounter.API_CACHE_ENTRIES);
                }
                else if (publicDelta == -1) {
                    Interlocked.Decrement(ref _publicCount);
                    PerfCounters.DecrementCounter(AppPerfCounter.API_CACHE_ENTRIES);
                }

                if (totalTurnover > 0) {
                    PerfCounters.IncrementCounterEx(AppPerfCounter.TOTAL_CACHE_TURNOVER_RATE, totalTurnover);
                }

                if (publicTurnover > 0) {
                    PerfCounters.IncrementCounterEx(AppPerfCounter.API_CACHE_TURNOVER_RATE, publicTurnover);
                }
            }

            return entry;
        }

        void UtcUpdateUsageRecursive(CacheEntry entry, DateTime utcNow) {
            CacheDependency dependency;
            CacheEntry[]    entries;

            // Don't update if the last update is less than 1 sec away.  This way we'll
            // avoid over updating the usage in the scenario where a cache makes several
            // update requests.
            if (utcNow - entry.UtcLastUsageUpdate > CacheUsage.CORRELATED_REQUEST_TIMEOUT || utcNow < entry.UtcLastUsageUpdate) {
                entry.UtcLastUsageUpdate = utcNow;
                if (entry.InUsage()) {
                    CacheSingle cacheSingle;
                    if (_cacheMultiple == null) {
                        cacheSingle = this;
                    }
                    else {
                        cacheSingle = _cacheMultiple.GetCacheSingle(entry.Key.GetHashCode());
                    }

                    cacheSingle._usage.Update(entry);
                }

                dependency = entry.Dependency;
                if (dependency != null) {
                    entries = dependency.CacheEntries;
                    if (entries != null) {
                        foreach (CacheEntry dependent in entries) {
                            UtcUpdateUsageRecursive(dependent, utcNow);
                        }
                    }
                }
            }
        }

        internal override long TrimIfNecessary(int percent) {
            Debug.Assert(_cacheCommon._inCacheManagerThread == 1, "Trim should only occur when we're updating memory statistics.");
            if (!_cacheCommon._enableMemoryCollection)
                return 0;

            int toTrim = 0;
            // do we need to drop a percentage of entries?
            if (percent > 0) {
                toTrim = (int)(((long)_totalCount * (long)percent) / 100L);
            }
            // would this leave us above MAX_COUNT?
            int minTrim = _totalCount - MAX_COUNT;
            if (toTrim < minTrim) {
                toTrim = minTrim;
            }
            // would this put us below MIN_COUNT?
            int maxTrim = _totalCount - MIN_COUNT;
            if (toTrim > maxTrim) {
                toTrim = maxTrim;
            }
            // do we need to trim?
            if (toTrim <= 0 || HostingEnvironment.ShutdownInitiated) {
                return 0;
            }
            
            int ocEntriesTrimmed = 0; // number of output cache entries trimmed
            int publicEntriesTrimmed = 0; // number of public entries trimmed            
            int totalTrimmed = 0; // total number of entries trimmed
            int trimmedOrExpired = 0;        
            int beginTotalCount = _totalCount;

            try {
                trimmedOrExpired = _expires.FlushExpiredItems(true);
                if (trimmedOrExpired < toTrim) {
                    totalTrimmed = _usage.FlushUnderUsedItems(toTrim - trimmedOrExpired, ref publicEntriesTrimmed, ref ocEntriesTrimmed);
                    trimmedOrExpired += totalTrimmed;
                }
                
                if (totalTrimmed > 0) {
                    // Update values for perfcounters
                    PerfCounters.IncrementCounterEx(AppPerfCounter.CACHE_TOTAL_TRIMS, totalTrimmed);
                    PerfCounters.IncrementCounterEx(AppPerfCounter.CACHE_API_TRIMS, publicEntriesTrimmed);
                    PerfCounters.IncrementCounterEx(AppPerfCounter.CACHE_OUTPUT_TRIMS, ocEntriesTrimmed);
                }
            }
            catch {
            }
            
#if DBG
            Debug.Trace("CacheMemory", "TrimIfNecessary: _iSubCache= " + _iSubCache 
                        + ", beginTotalCount=" + beginTotalCount
                        + ", endTotalCount=" + _totalCount
                        + ", percent=" + percent 
                        + ", trimmed=" + totalTrimmed);
#endif
            return trimmedOrExpired;
        }
        
        internal override void EnableExpirationTimer(bool enable) {
            if (_expires != null) {
                _expires.EnableExpirationTimer(enable);
            }
        }
    }

    class CacheMultiple : CacheInternal {
        int             _disposed;
        CacheSingle[]   _caches;
        int             _cacheIndexMask;

        internal CacheMultiple(CacheCommon cacheCommon, int numSingleCaches) : base(cacheCommon) {
            Debug.Assert(numSingleCaches > 1, "numSingleCaches is not greater than 1");
            Debug.Assert((numSingleCaches & (numSingleCaches - 1)) == 0, "numSingleCaches is not a power of 2");
            _cacheIndexMask = numSingleCaches - 1;
            _caches = new CacheSingle[numSingleCaches];
            for (int i = 0; i < numSingleCaches; i++) {
                _caches[i] = new CacheSingle(cacheCommon, this, i);
            }
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                if (Interlocked.Exchange(ref _disposed, 1) == 0) {
                    foreach (CacheSingle cacheSingle in _caches) {
                        cacheSingle.Dispose();
                    }
                }
            }

            base.Dispose(disposing);
        }

        internal override int PublicCount {
            get {
                int count = 0;
                foreach (CacheSingle cacheSingle in _caches) {
                    count += cacheSingle.PublicCount;
                }

                return count;
            }
        }

        internal override long TotalCount {
            get {
                long count = 0;
                foreach (CacheSingle cacheSingle in _caches) {
                    count += cacheSingle.TotalCount;
                }

                return count;
            }
        }

        internal override IDictionaryEnumerator CreateEnumerator() {
            IDictionaryEnumerator[] enumerators = new IDictionaryEnumerator[_caches.Length];
            for (int i = 0, c = _caches.Length; i < c; i++) {
                enumerators[i] = _caches[i].CreateEnumerator();
            }

            return new AggregateEnumerator(enumerators);
        }

        internal CacheSingle GetCacheSingle(int hashCode) {
            Debug.Assert(_caches != null && _caches.Length != 0);
            // Dev10 865907: Math.Abs throws OverflowException for Int32.MinValue
            if (hashCode < 0) {
                hashCode = (hashCode == Int32.MinValue) ? 0 : -hashCode;
            }
            int index = (hashCode & _cacheIndexMask);
            return _caches[index];
        }

        internal override CacheEntry UpdateCache(
                CacheKey cacheKey,
                CacheEntry newEntry,
                bool replace,
                CacheItemRemovedReason removedReason,
                out object valueOld) {

            int hashCode = cacheKey.Key.GetHashCode();
            CacheSingle cacheSingle = GetCacheSingle(hashCode);
            return cacheSingle.UpdateCache(cacheKey, newEntry, replace, removedReason, out valueOld);
        }

        internal override long TrimIfNecessary(int percent) {
            long count = 0;
            foreach (CacheSingle cacheSingle in _caches) {
                count += cacheSingle.TrimIfNecessary(percent);
            }
            return count;
        }

        internal override void EnableExpirationTimer(bool enable) {
            foreach (CacheSingle cacheSingle in _caches) {
                cacheSingle.EnableExpirationTimer(enable);
            }
        }
    }

    class AggregateEnumerator : IDictionaryEnumerator {
        IDictionaryEnumerator []    _enumerators;
        int                         _iCurrent;

        internal AggregateEnumerator(IDictionaryEnumerator [] enumerators) {
            _enumerators = enumerators;
        }

        public bool MoveNext() {
            bool more;

            for (;;) {
                more = _enumerators[_iCurrent].MoveNext();
                if (more)
                    break;

                if (_iCurrent == _enumerators.Length - 1)
                    break;

                _iCurrent++;
            }

            return more;
        }

        public void Reset() {
            for (int i = 0; i <= _iCurrent; i++) {
                _enumerators[i].Reset();
            }

            _iCurrent = 0;
        }

        public Object Current {
            get {
                return _enumerators[_iCurrent].Current;
            }
        }

        public Object Key {
            get {
                return _enumerators[_iCurrent].Key;
            }
        }

        public Object Value {
            get {
                return _enumerators[_iCurrent].Value;
            }
        }

    	public DictionaryEntry Entry {
            get {
                return _enumerators[_iCurrent].Entry;
            }
        }
    }
}

