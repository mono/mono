// <copyright file="PerfCounters.cs" company="Microsoft">
//   Copyright (c) 2009 Microsoft Corporation.  All rights reserved.
// </copyright>
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Caching.Hosting;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading;

namespace System.Runtime.Caching {
    internal sealed class PerfCounters : IDisposable {
        private const string PERF_COUNTER_CATEGORY  = ".NET Memory Cache 4.0";
        private const string CACHE_ENTRIES          = "Cache Entries";
        private const string CACHE_HITS             = "Cache Hits";
        private const string CACHE_HIT_RATIO        = "Cache Hit Ratio";
        private const string CACHE_HIT_RATIO_BASE   = "Cache Hit Ratio Base";
        private const string CACHE_MISSES           = "Cache Misses";
        private const string CACHE_TRIMS            = "Cache Trims";
        private const string CACHE_TURNOVER         = "Cache Turnover Rate";
        private const int NUM_COUNTERS = 7;

        private static string s_appId;

        private PerformanceCounter[] _counters;
        private long[] _counterValues;

        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        [SuppressMessage("Microsoft.Security", "CA2106:SecureAsserts", Justification = "Grandfathered suppression from original caching code checkin")]
        private static void EnsureAppIdInited() {
            if (s_appId == null) {
                IApplicationIdentifier ai = null;
                IServiceProvider host = ObjectCache.Host;
                if (host != null) {
                    ai = host.GetService(typeof(IApplicationIdentifier)) as IApplicationIdentifier;
                }
                // if the host has an identifier, use it
                string appId = (ai != null) ? ai.GetApplicationId() : null;
                // otherwise, use the process name wihtout file extension
                if (String.IsNullOrEmpty(appId)) {
                    StringBuilder sb = new StringBuilder(512);
                    if (UnsafeNativeMethods.GetModuleFileName(IntPtr.Zero, sb, 512) != 0) {
                        appId = Path.GetFileNameWithoutExtension(sb.ToString());
                    }
                }
                // if all else fails, use AppDomain.FriendlyName
                if (String.IsNullOrEmpty(appId)) {  
                    appId = AppDomain.CurrentDomain.FriendlyName;
                }
                Interlocked.CompareExchange(ref s_appId, appId, null);
            }
        }

        private void InitDisposableMembers(string cacheName) {
            bool dispose = true;
            try {
                StringBuilder sb = (s_appId != null) ? new StringBuilder(s_appId + ":" + cacheName) : new StringBuilder(cacheName);
                for (int i = 0; i < sb.Length; i++) {
                    switch (sb[i]) {
                        case '(':
                            sb[i] = '[';
                            break;
                        case ')':
                            sb[i] = ']';
                            break;
                        case '/':
                        case '\\':
                        case '#':
                            sb[i] = '_';
                            break;
                    }
                }
                string instanceName = sb.ToString();
                _counters = new PerformanceCounter[NUM_COUNTERS];
                _counterValues = new long[NUM_COUNTERS];
                _counters[(int)PerfCounterName.Entries] = new PerformanceCounter(PERF_COUNTER_CATEGORY, CACHE_ENTRIES, instanceName, false);
                _counters[(int)PerfCounterName.Hits] = new PerformanceCounter(PERF_COUNTER_CATEGORY, CACHE_HITS, instanceName, false);
                _counters[(int)PerfCounterName.HitRatio] = new PerformanceCounter(PERF_COUNTER_CATEGORY, CACHE_HIT_RATIO, instanceName, false);
                _counters[(int)PerfCounterName.HitRatioBase] = new PerformanceCounter(PERF_COUNTER_CATEGORY, CACHE_HIT_RATIO_BASE, instanceName, false);
                _counters[(int)PerfCounterName.Misses] = new PerformanceCounter(PERF_COUNTER_CATEGORY, CACHE_MISSES, instanceName, false);
                _counters[(int)PerfCounterName.Trims] = new PerformanceCounter(PERF_COUNTER_CATEGORY, CACHE_TRIMS, instanceName, false);
                _counters[(int)PerfCounterName.Turnover] = new PerformanceCounter(PERF_COUNTER_CATEGORY, CACHE_TURNOVER, instanceName, false);                
                dispose = false;
            }
            finally {
                if (dispose) {
                    Dispose();
                }
            }
        }

        private PerfCounters() {
            // hide default ctor
        }

        internal PerfCounters(string cacheName) {
            if (cacheName == null) {
                throw new ArgumentNullException("cacheName");
            }

            EnsureAppIdInited();
            
            InitDisposableMembers(cacheName);
        }

        internal void Decrement(PerfCounterName name) {
            int idx = (int) name;
            PerformanceCounter counter = _counters[idx];
            counter.Decrement();
            Interlocked.Decrement(ref _counterValues[idx]);
        }
        
        internal void Increment(PerfCounterName name) {
            int idx = (int) name;
            PerformanceCounter counter = _counters[idx];
            counter.Increment();
            Interlocked.Increment(ref _counterValues[idx]);
        }
        
        internal void IncrementBy(PerfCounterName name, long value) {
            int idx = (int) name;
            PerformanceCounter counter = _counters[idx];
            counter.IncrementBy(value);
            Interlocked.Add(ref _counterValues[idx], value);
        }        

        public void Dispose() {
            PerformanceCounter[] counters = _counters;
            // ensure this only happens once
            if (counters != null && Interlocked.CompareExchange(ref _counters, null, counters) == counters) {
                for (int i = 0; i < NUM_COUNTERS; i++) {
                    PerformanceCounter counter = counters[i];
                    if (counter != null) {
                        // decrement counter by its current value, to zero it out for this instance of the named cache (see Dev10 Bug 680819)
                        long value = Interlocked.Exchange(ref _counterValues[i], 0);
                        if (value != 0) {
                            counter.IncrementBy(-value);
                        }
                        counter.Dispose();
                    }
                }
            }
            // Don't need to call GC.SuppressFinalize(this) for sealed types without finalizers.
        }
    }
}
