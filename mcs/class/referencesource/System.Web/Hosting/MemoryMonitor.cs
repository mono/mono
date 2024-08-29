//------------------------------------------------------------------------------
// <copyright file="MemoryMonitor.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

 /*
 * AspNetMemoryMonitor and related classes
 *
 * Copyright (c) 2016 Microsoft Corporation
 */

 using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Web.Caching;
using System.Web.Configuration;
using System.Web.Util;
using System.Web;
using System.Collections.Generic;
using System.Threading;
using Stopwatch = System.Diagnostics.Stopwatch;

 namespace System.Web.Hosting {

     public enum RecycleLimitNotificationFrequency {
        High,
        Medium,
        Low
    };

     /// <devdoc>
    ///    <para>Relevent parameters describing the level of memory pressure detected.</para>
    ///       <paramref name="RequestGC">If the action taken as a result of this notification could
    ///          benefit from inducing a GarbageCollection, then set this to true before returning.
    ///          Default = false.</paramref>
    /// </devdoc>
    public sealed class RecycleLimitInfo {
        private long _currentPB;
        private long _recycleLimit;
        private RecycleLimitNotificationFrequency _recycleLimitNearFrequency;
        private bool _requestGC;

         public RecycleLimitInfo(long currentPrivateBytes, long recycleLimit, RecycleLimitNotificationFrequency recycleLimitNearFrequency) {
            _currentPB = currentPrivateBytes;
            _recycleLimit = recycleLimit;
            _recycleLimitNearFrequency = recycleLimitNearFrequency;
            _requestGC = false;
        }

         /// <devdoc>
        ///    <para>Detected private bytes usage for the current process.</para>
        /// </devdoc>
        public long CurrentPrivateBytes { get { return _currentPB; } }

         /// <devdoc>
        ///    <para>The determined private bytes limit for the current process.</para>
        /// </devdoc>
        public long RecycleLimit { get { return _recycleLimit; } }

         /// <devdoc>
        ///    <para>An enum indicating how frequently the memory monitor perceives we are asking listeners
        ///    to react to the approaching memory threshold.</para>
        /// </devdoc>
        public RecycleLimitNotificationFrequency TrimFrequency { get { return _recycleLimitNearFrequency; } }

         /// <devdoc>
        ///    <para>If the action taken as a result of this notification could
        ///          benefit from inducing a GarbageCollection, then set this to true before returning.
        ///          Default = false.</para>
        /// </devdoc>
        public bool RequestGC {
            get { return _requestGC; }
            set { _requestGC |= value; }
        }
    }

     /// <devdoc>
    ///    <para>Relevent parameters describing a low memory condition on the host machine.</para>
    ///       <paramref name="RequestGC">If the action taken as a result of this notification could
    ///          benefit from inducing a GarbageCollection, then set this to true before returning.
    ///          Default = false.</paramref>
    /// </devdoc>
    public sealed class LowPhysicalMemoryInfo {
        private int _currentPercent;
        private int _limit;
        private bool _requestGC;

         public LowPhysicalMemoryInfo(int currentPercentUsed, int percentLimit) {
            _currentPercent = currentPercentUsed;
            _limit = percentLimit;
            _requestGC = false;
        }

         /// <devdoc>
        ///    <para>Detected percent of total RAM used on the local machine.</para>
        /// </devdoc>
        public int CurrentPercentUsed { get { return _currentPercent; } }

         /// <devdoc>
        ///    <para>The determined used-RAM percentage limit for the current machine.</para>
        /// </devdoc>
        public int PercentLimit { get { return _limit; } }

         /// <devdoc>
        ///    <para>If the action taken as a result of this notification could
        ///          benefit from inducing a GarbageCollection, then set this to true before returning.
        ///          Default = false.</para>
        /// </devdoc>
        public bool RequestGC {
            get { return _requestGC; }
            set { _requestGC |= value; }
        }
    }

     /// <devdoc>
    ///    <para>ASP.Net default implementation of memory monitor.</para>
    /// </devdoc>
    public sealed class AspNetMemoryMonitor : IApplicationMonitor, IObservable<RecycleLimitInfo>, IObservable<LowPhysicalMemoryInfo> {

         internal const long TERABYTE = 1L << 40;
        internal const long GIGABYTE = 1L << 30;
        internal const long MEGABYTE = 1L << 20;
        internal const long KILOBYTE = 1L << 10;

         internal const long PRIVATE_BYTES_LIMIT_2GB = 800 * MEGABYTE;
        internal const long PRIVATE_BYTES_LIMIT_3GB = 1800 * MEGABYTE;
        internal const long PRIVATE_BYTES_LIMIT_64BIT = 1L * TERABYTE;

         internal static long s_totalPhysical;
        internal static long s_totalVirtual;
        internal static long s_processPrivateBytesLimit = -1;
        internal static long s_configuredProcessMemoryLimit = 0;

         private static AspNetMemoryMonitor _firstMemoryMonitor = null;

         private RecycleLimitMonitor _recycleMonitor = null;
        private IObserver<RecycleLimitInfo> _defaultRecycleObserver = null;
        private IDisposable _defaultRecycleSubscription = null;

         private LowPhysicalMemoryMonitor _lowMemoryMonitor = null;
        private IObserver<LowPhysicalMemoryInfo> _defaultLowMemObserver = null;
        private IDisposable _defaultLowMemSubscription = null;

         internal static long ConfiguredProcessMemoryLimit {
            get {
                long memoryLimit = s_configuredProcessMemoryLimit;

#if !FEATURE_PAL
                 if (memoryLimit == 0) {
                    // WorkerProcessMemoryLimit : per-process information
                    if (UnsafeNativeMethods.GetModuleHandle(ModName.WP_FULL_NAME) != IntPtr.Zero) {
                        memoryLimit = (long)UnsafeNativeMethods.PMGetMemoryLimitInMB() << 20;
                    }
                    else if (UnsafeNativeMethods.GetModuleHandle(ModName.W3WP_FULL_NAME) != IntPtr.Zero) {
                        IServerConfig serverConfig = ServerConfig.GetInstance();
                        memoryLimit = (long)serverConfig.GetW3WPMemoryLimitInKB() << 10;
                    }
                    Interlocked.Exchange(ref s_configuredProcessMemoryLimit, memoryLimit);
                }
#endif

                 return memoryLimit;
            }
        }

         internal static long ProcessPrivateBytesLimit {
            get {
                long memoryLimit = s_processPrivateBytesLimit;
                if (memoryLimit == -1) {
                    memoryLimit = ConfiguredProcessMemoryLimit;

                     // AutoPrivateBytesLimit
                    if (memoryLimit == 0) {
                        bool is64bit = (IntPtr.Size == 8);
                        if (s_totalPhysical != 0) {
                            long recommendedPrivateByteLimit;
                            if (is64bit) {
                                recommendedPrivateByteLimit = PRIVATE_BYTES_LIMIT_64BIT;
                            }
                            else {
                                // Figure out if it's 2GB or 3GB
                                if (s_totalVirtual > 2 * GIGABYTE) {
                                    recommendedPrivateByteLimit = PRIVATE_BYTES_LIMIT_3GB;
                                }
                                else {
                                    recommendedPrivateByteLimit = PRIVATE_BYTES_LIMIT_2GB;
                                }
                            }

                             // if we're hosted, use 60% of physical RAM; otherwise 100%
                            long usableMemory = HostingEnvironment.IsHosted ? s_totalPhysical * 3 / 5 : s_totalPhysical;
                            memoryLimit = Math.Min(usableMemory, recommendedPrivateByteLimit);
                        }
                        else {
                            // If GlobalMemoryStatusEx fails, we'll use these as our auto-gen private bytes limit
                            memoryLimit = is64bit ? PRIVATE_BYTES_LIMIT_64BIT : PRIVATE_BYTES_LIMIT_2GB;
                        }
                    }
                    Interlocked.Exchange(ref s_processPrivateBytesLimit, memoryLimit);
                }
                return memoryLimit;
            }
        }

         internal static long PhysicalMemoryPercentageLimit {
            get {
                    if (_firstMemoryMonitor != null && _firstMemoryMonitor._lowMemoryMonitor != null) {
                        return _firstMemoryMonitor._lowMemoryMonitor.PressureHigh;
                    }
                return 0;
            }
        }

         public IObserver<LowPhysicalMemoryInfo> DefaultLowPhysicalMemoryObserver {
            get {
                return _defaultLowMemObserver;
            }
            set {
                if (_defaultLowMemSubscription != null) {
                    _defaultLowMemSubscription.Dispose();
                    _defaultLowMemSubscription = null;
                }
                _defaultLowMemObserver = null;

                 if (value != null) {
                    _defaultLowMemObserver = value;
                    _defaultLowMemSubscription = Subscribe(value);
                }
            }
        }

         public IObserver<RecycleLimitInfo> DefaultRecycleLimitObserver {
            get {
                return _defaultRecycleObserver;
            }
            set {
                if (_defaultRecycleSubscription != null) {
                    _defaultRecycleSubscription.Dispose();
                    _defaultRecycleSubscription = null;
                }
                _defaultRecycleObserver = null;

                 if (value != null) {
                    _defaultRecycleObserver = value;
                    _defaultRecycleSubscription = Subscribe(value);
                }
            }
        }

         static AspNetMemoryMonitor() {
#if !FEATURE_PAL
            UnsafeNativeMethods.MEMORYSTATUSEX memoryStatusEx = new UnsafeNativeMethods.MEMORYSTATUSEX();
            memoryStatusEx.Init();
            if (UnsafeNativeMethods.GlobalMemoryStatusEx(ref memoryStatusEx) != 0) {
                s_totalPhysical = memoryStatusEx.ullTotalPhys;
                s_totalVirtual = memoryStatusEx.ullTotalVirtual;
            }
#endif
        }

         internal AspNetMemoryMonitor() {
            _recycleMonitor = new RecycleLimitMonitor();
            DefaultRecycleLimitObserver = new RecycleLimitObserver();

             _lowMemoryMonitor = new LowPhysicalMemoryMonitor();
            DefaultLowPhysicalMemoryObserver = new LowPhysicalMemoryObserver();

             if (_firstMemoryMonitor == null) {
                _firstMemoryMonitor = this;
            }
        }

         public IDisposable Subscribe(IObserver<LowPhysicalMemoryInfo> observer) {
            if (_lowMemoryMonitor != null) {
                _lowMemoryMonitor.Subscribe(observer);
            }

             return new Unsubscriber(() => { _lowMemoryMonitor.Unsubscribe(observer); });
        }

         public IDisposable Subscribe(IObserver<RecycleLimitInfo> observer) {
            if (_recycleMonitor != null) {
                _recycleMonitor.Subscribe(observer);
            }

             return new Unsubscriber(() => { _recycleMonitor.Unsubscribe(observer); });
        }

         public void Start() {
            _recycleMonitor.Start();
            _lowMemoryMonitor.Start();
        }

         public void Stop() {
            _recycleMonitor.Stop();
            _lowMemoryMonitor.Stop();
        }

         public void Dispose() {
            DefaultLowPhysicalMemoryObserver = null;
            DefaultRecycleLimitObserver = null;
            _recycleMonitor.Dispose();
        }

         class Unsubscriber : IDisposable {
            Action _unsub;

             public Unsubscriber(Action unsubscribeAction) {
                _unsub = unsubscribeAction;
            }

             public void Dispose() {
                if (_unsub != null) {
                    _unsub.Invoke();
                }
            }
        }
    }
}