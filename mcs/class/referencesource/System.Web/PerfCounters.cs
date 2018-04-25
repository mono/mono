//------------------------------------------------------------------------------
// <copyright file="PerfCounters.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * PerfCounters class
 */
namespace System.Web {

    using System.Web.Util;
    using System.Threading;
    using System.Runtime.InteropServices;

    internal sealed class PerfInstanceDataHandle: SafeHandle {
        internal PerfInstanceDataHandle() : base(IntPtr.Zero, true) {
        }
        
        internal IntPtr UnsafeHandle {
            get { return handle; }
        }
        
        public override bool IsInvalid {
            get { return handle == IntPtr.Zero; }
        }
        
        override protected bool ReleaseHandle() {
            UnsafeNativeMethods.PerfCloseAppCounters(handle);
            handle = IntPtr.Zero;
            return true;
        }
    }

    internal sealed class PerfCounters {

        // singleton used for providing an abstraction to callers who require an interface implementation
        internal static readonly IPerfCounters Instance = new PerfCountersInstance();

        private static PerfInstanceDataHandle _instance = null;
        private static IntPtr _global = IntPtr.Zero;
        private static IntPtr _stateService = IntPtr.Zero;

        private PerfCounters () {}

        internal static void Open(string appName) {
            Debug.Assert(appName != null);
            
            OpenCounter(appName);
        }

        internal static void OpenStateCounters() {
            OpenCounter(null);
        }


        // The app name should either be a valid app name or be 'null' to get the state service
        // counters initialized
        private static void OpenCounter(string appName) {
            try {
                // Don't activate perf counters if webengine.dll isn't loaded
                if (! HttpRuntime.IsEngineLoaded) 
                    return;

                // Open the global counters
                if (_global == IntPtr.Zero) {
                    _global = UnsafeNativeMethods.PerfOpenGlobalCounters();
                }

                // If appName is null, then we want the state counters
                if (appName == null) {
                    if (_stateService == IntPtr.Zero) {
                        _stateService = UnsafeNativeMethods.PerfOpenStateCounters();
                    }
                }
                else {
                    if (appName != null) {
                        _instance = UnsafeNativeMethods.PerfOpenAppCounters(appName);
                    }
                }
            }
            catch (Exception e) {
                Debug.Trace("Perfcounters", "Exception: " + e.StackTrace);
            }
        }

        // Make sure webengine.dll is loaded before attempting to call into it (ASURT 98531)

        internal static void IncrementCounter(AppPerfCounter counter) {
            if (_instance != null)
                UnsafeNativeMethods.PerfIncrementCounter(_instance.UnsafeHandle, (int) counter);
        }

        internal static void DecrementCounter(AppPerfCounter counter) {
            if (_instance != null)
                UnsafeNativeMethods.PerfDecrementCounter(_instance.UnsafeHandle, (int) counter);
        }

        internal static void IncrementCounterEx(AppPerfCounter counter, int delta) {
            if (_instance != null)
                UnsafeNativeMethods.PerfIncrementCounterEx(_instance.UnsafeHandle, (int) counter, delta);
        }

        internal static void SetCounter(AppPerfCounter counter, int value) {
            if (_instance != null)
                UnsafeNativeMethods.PerfSetCounter(_instance.UnsafeHandle, (int) counter, value);
        }

        // It's important that this be debug only. We don't want production
        // code to access shared memory that another process could corrupt.
#if DBG
        internal static int GetCounter(AppPerfCounter counter) {
            if (_instance != null)
                return UnsafeNativeMethods.PerfGetCounter(_instance.UnsafeHandle, (int) counter);
            else
                return -1;
        }
#endif

        internal static int GetGlobalCounter(GlobalPerfCounter counter) {
            if (_global != IntPtr.Zero)
                return UnsafeNativeMethods.PerfGetCounter(_global, (int) counter);
            else
                return -1;
        }

        internal static void IncrementGlobalCounter(GlobalPerfCounter counter) {
            if (_global != IntPtr.Zero)
                UnsafeNativeMethods.PerfIncrementCounter(_global, (int) counter);
        }

        internal static void DecrementGlobalCounter(GlobalPerfCounter counter) {
            if (_global != IntPtr.Zero)
                UnsafeNativeMethods.PerfDecrementCounter(_global, (int) counter);
        }

        internal static void SetGlobalCounter(GlobalPerfCounter counter, int value) {
            if (_global != IntPtr.Zero)
                UnsafeNativeMethods.PerfSetCounter(_global, (int) counter, value);
        }

        internal static void IncrementStateServiceCounter(StateServicePerfCounter counter) {
            if (_stateService == IntPtr.Zero)
                return;
            UnsafeNativeMethods.PerfIncrementCounter(_stateService, (int) counter);
            
            switch (counter) {
                case StateServicePerfCounter.STATE_SERVICE_SESSIONS_TOTAL:
                    IncrementGlobalCounter(GlobalPerfCounter.STATE_SERVER_SESSIONS_TOTAL);
                    break;
                case StateServicePerfCounter.STATE_SERVICE_SESSIONS_ACTIVE:
                    IncrementGlobalCounter(GlobalPerfCounter.STATE_SERVER_SESSIONS_ACTIVE);
                    break;
                case StateServicePerfCounter.STATE_SERVICE_SESSIONS_TIMED_OUT:
                    IncrementGlobalCounter(GlobalPerfCounter.STATE_SERVER_SESSIONS_TIMED_OUT);
                    break;
                case StateServicePerfCounter.STATE_SERVICE_SESSIONS_ABANDONED:
                    IncrementGlobalCounter(GlobalPerfCounter.STATE_SERVER_SESSIONS_ABANDONED);
                    break;
                default:
                    break;
            }
        }

        internal static void DecrementStateServiceCounter(StateServicePerfCounter counter) {
            if (_stateService == IntPtr.Zero)
                return;
            UnsafeNativeMethods.PerfDecrementCounter(_stateService, (int) counter);
            
            switch (counter) {
                case StateServicePerfCounter.STATE_SERVICE_SESSIONS_TOTAL:
                    DecrementGlobalCounter(GlobalPerfCounter.STATE_SERVER_SESSIONS_TOTAL);
                    break;
                case StateServicePerfCounter.STATE_SERVICE_SESSIONS_ACTIVE:
                    DecrementGlobalCounter(GlobalPerfCounter.STATE_SERVER_SESSIONS_ACTIVE);
                    break;
                case StateServicePerfCounter.STATE_SERVICE_SESSIONS_TIMED_OUT:
                    DecrementGlobalCounter(GlobalPerfCounter.STATE_SERVER_SESSIONS_TIMED_OUT);
                    break;
                case StateServicePerfCounter.STATE_SERVICE_SESSIONS_ABANDONED:
                    DecrementGlobalCounter(GlobalPerfCounter.STATE_SERVER_SESSIONS_ABANDONED);
                    break;
                default:
                    break;
            }
        }

        internal static void SetStateServiceCounter(StateServicePerfCounter counter, int value) {
            if (_stateService == IntPtr.Zero)
                return;
            UnsafeNativeMethods.PerfSetCounter(_stateService, (int) counter, value);
            
            switch (counter) {
                case StateServicePerfCounter.STATE_SERVICE_SESSIONS_TOTAL:
                    SetGlobalCounter(GlobalPerfCounter.STATE_SERVER_SESSIONS_TOTAL, value);
                    break;
                case StateServicePerfCounter.STATE_SERVICE_SESSIONS_ACTIVE:
                    SetGlobalCounter(GlobalPerfCounter.STATE_SERVER_SESSIONS_ACTIVE, value);
                    break;
                case StateServicePerfCounter.STATE_SERVICE_SESSIONS_TIMED_OUT:
                    SetGlobalCounter(GlobalPerfCounter.STATE_SERVER_SESSIONS_TIMED_OUT, value);
                    break;
                case StateServicePerfCounter.STATE_SERVICE_SESSIONS_ABANDONED:
                    SetGlobalCounter(GlobalPerfCounter.STATE_SERVER_SESSIONS_ABANDONED, value);
                    break;
                default:
                    break;
            }
        }

        private sealed class PerfCountersInstance : IPerfCounters {
            public void IncrementCounter(AppPerfCounter counter) {
                PerfCounters.IncrementCounter(counter);
            }

            public void IncrementCounter(AppPerfCounter counter, int value) {
                PerfCounters.IncrementCounterEx(counter, value);
            }

            public void DecrementCounter(AppPerfCounter counter) {
                PerfCounters.DecrementCounter(counter);
            }

            public void SetCounter(AppPerfCounter counter, int value) {
                PerfCounters.SetCounter(counter, value);
            }
        }
    };

}

