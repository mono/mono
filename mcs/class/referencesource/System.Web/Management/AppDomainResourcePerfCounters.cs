//------------------------------------------------------------------------------
// <copyright file="AppDomainResourcePerfCounters.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Management {
    using System;
    using System.Configuration;
    using System.Web;
    using System.Threading;

    internal class AppDomainResourcePerfCounters {

        private const  uint      NUM_SECONDS_TO_POLL = 5;

        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        internal static void Init() {
            if (_fInit)
                return;

            lock (_InitLock) {
                if (_fInit)
                    return;

                if (AppDomain.MonitoringIsEnabled) {
                    PerfCounters.SetCounter(AppPerfCounter.APP_CPU_USED_BASE, 100);
                    _Timer = new Timer((new AppDomainResourcePerfCounters()).TimerCallback, null,
                                       NUM_SECONDS_TO_POLL * 1000, NUM_SECONDS_TO_POLL * 1000);
                }
                _fInit = true;
            }
        }

        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        internal static void Stop() {
            if (_Timer == null)
                return; // already stopped

            _StopRequested = true;

            lock (_InitLock) {
                if (_Timer != null) {
                    ((IDisposable)_Timer).Dispose();
                    _Timer = null;
                }
            }

            // Wait for the _inProgressLock lock
            while (_inProgressLock != 0) {
                Thread.Sleep(100);
            }
        }

        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        // Static data
        private static bool      _fInit                = false;
        private static object    _InitLock             = new object();
        private static Timer     _Timer                = null;
        private static int       _inProgressLock       = 0;
        private static bool      _StopRequested        = false;

        // Instance data
        private int       _MemUsageLastReported = 0;
        private int       _CPUUsageLastReported = 0;
        private TimeSpan  _TotalCPUTime;
        private DateTime  _LastCollectTime;


        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        private AppDomainResourcePerfCounters() {
            _TotalCPUTime = AppDomain.CurrentDomain.MonitoringTotalProcessorTime;
            _LastCollectTime = DateTime.UtcNow;
        }

        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        private void TimerCallback(Object state) {

            if ( _StopRequested ||  // Stop has been called -- exit immediately
                 !AppDomain.MonitoringIsEnabled || // Monitoring APIs will throw NotSupportedException if not-enabled
                 Interlocked.Exchange(ref _inProgressLock, 1) != 0) // Is some thread currently executing the callback
            {
                return;
            }

            try {
                SetPerfCounters();
            } catch { // don't bubble up exceptions, since we are on a timer thread
            } finally {
                Interlocked.Exchange(ref _inProgressLock, 0);
            }
        }

        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        private void SetPerfCounters() {

            ////////////////////////////////////////////////////////////
            // Calculate memory: Limited to 2TB (Int32.MaxValue * 1024 bytes)
            long memInKB = (AppDomain.CurrentDomain.MonitoringSurvivedMemorySize / 1024); // Mem used in KB
            _MemUsageLastReported = (int) Math.Min(Int32.MaxValue, Math.Max(0, memInKB)); // Make sure its within 0 and Int32.MaxValue
            PerfCounters.SetCounter(AppPerfCounter.APP_MEMORY_USED, _MemUsageLastReported);

            ////////////////////////////////////////////////////////////
            // Calculate CPU
            DateTime dtUtcNow = DateTime.UtcNow;
            TimeSpan newTotalCPUTime = AppDomain.CurrentDomain.MonitoringTotalProcessorTime;

            double  timeElapsed = (dtUtcNow - _LastCollectTime).TotalMilliseconds; // Total time since last collect
            double  cpuTimeUsed = (newTotalCPUTime - _TotalCPUTime).TotalMilliseconds; // Total CPU time used since last collect
            int     cpuPercent  = (int) ((cpuTimeUsed * 100) / timeElapsed); // Percent of CPU time used

            _CPUUsageLastReported = Math.Min(100, Math.Max(0, cpuPercent)); // Make sure it's within 0 and 100
            PerfCounters.SetCounter(AppPerfCounter.APP_CPU_USED, _CPUUsageLastReported);

            // Update variables for next time
            _TotalCPUTime = newTotalCPUTime;
            _LastCollectTime = dtUtcNow;
        }
    }
}

