//------------------------------------------------------------------------------
// <copyright file="LowMemoryMonitor.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

 /*
 * LowMemoryMonitor and related classes
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

     public class LowPhysicalMemoryObserver : IObserver<LowPhysicalMemoryInfo> {
        private const int MIN_TOTAL_MEMORY_TRIM_PERCENT = 10;
        private const long TARGET_TOTAL_MEMORY_TRIM_INTERVAL_TICKS = 5 * TimeSpan.TicksPerMinute;

         private int _lastTrimPercent = 10;
        private int _lastTrimGen2Count = -1;
        private DateTime _lastTrimTime = DateTime.MinValue;

         public void OnCompleted() { }

         public void OnError(Exception error) { }

         public void OnNext(LowPhysicalMemoryInfo lowMemoryInfo) {

             int percent = 0;
            int gen2Count = GC.CollectionCount(2);
            DateTime utcNow = DateTime.UtcNow;

             // has there been a Gen 2 Collection since the last trim?
            if (gen2Count != _lastTrimGen2Count) {
                // Old code used to check this, but if you look closely, when wired up in the default way,
                // the default memory monitor only triggers the LowMemoryAction under high pressure. So this
                // should always be true.
                //if (IsAboveHighPressure()) {

                 // choose percent such that we don't repeat this for ~5 (TARGET_TOTAL_MEMORY_TRIM_INTERVAL) minutes, 
                // but keep the percentage between 10 and 50.
                long ticksSinceTrim = utcNow.Subtract(_lastTrimTime).Ticks;
                if (ticksSinceTrim > 0) {
                    percent = Math.Min(50, (int)((_lastTrimPercent * TARGET_TOTAL_MEMORY_TRIM_INTERVAL_TICKS) / ticksSinceTrim));
                    percent = Math.Max(MIN_TOTAL_MEMORY_TRIM_PERCENT, percent);
                }
#if PERF
                SafeNativeMethods.OutputDebugString(String.Format("AspNetMemoryMonitor.GetPercentToTrim: percent={0:N}, lastTrimPercent={1:N}, secondsSinceTrim={2:N}\n",
                                                percent,
                                                _lastLowMemoryTrimPercent,
                                                ticksSinceTrim/TimeSpan.TicksPerSecond));
#endif

                 HostingEnvironment.TrimCache(percent);

                 _lastTrimGen2Count = gen2Count;
                _lastTrimTime = utcNow;
                _lastTrimPercent = percent;
            }
        }
    }

     class LowPhysicalMemoryMonitor {

         const int HISTORY_COUNT = 6;

         static int s_configuredPollInterval = Int32.MaxValue;

         int _pressureHigh;      // high pressure level
        int _pressureLow;       // low pressure level - slow growth here
        int[] _pressureHist;
        int _i0;

         private object _timerLock = new object();
        private Timer _timer;
        private int _currentPollInterval = Timeout.Infinite;
        private int _inMonitorThread = 0;
        private ApplicationManager _appManager;

         private List<IObserver<LowPhysicalMemoryInfo>> _observers;

         internal int PressureLast {
            get { return _pressureHist[_i0]; }
        }

         internal int PressureHigh {
            get { return _pressureHigh; }
        }

         internal int PressureLow {
            get { return _pressureLow; }
        }

         internal LowPhysicalMemoryMonitor() {
            /*
              The chart below shows physical memory in megabytes, and the 1, 3, and 10% values.
               RAM     1%      3%      10%
              -----------------------------
              128     1.28    3.84    12.8
              256     2.56    7.68    25.6
              512     5.12    15.36   51.2
              1024    10.24   30.72   102.4
              2048    20.48   61.44   204.8
              4096    40.96   122.88  409.6
              8192    81.92   245.76  819.2
               Low memory notifications from CreateMemoryResourceNotification are calculated as follows
              (.\base\ntos\mm\initsup.c):
              
              MiInitializeMemoryEvents() {
              ...
              //
              // Scale the threshold so on servers the low threshold is
              // approximately 32MB per 4GB, capping it at 64MB.
              //
              
              MmLowMemoryThreshold = MmPlentyFreePages;
              
              if (MmNumberOfPhysicalPages > 0x40000) {
                  MmLowMemoryThreshold = MI_MB_TO_PAGES (32);
                  MmLowMemoryThreshold += ((MmNumberOfPhysicalPages - 0x40000) >> 7);
              }
              else if (MmNumberOfPhysicalPages > 0x8000) {
                  MmLowMemoryThreshold += ((MmNumberOfPhysicalPages - 0x8000) >> 5);
              }
              
              if (MmLowMemoryThreshold > MI_MB_TO_PAGES (64)) {
                  MmLowMemoryThreshold = MI_MB_TO_PAGES (64);
              }
              ...
               E.g.
               RAM(mb) low      %
              -------------------
              256	  20	  92%
              512	  24	  95%
              768	  28	  96%
              1024	  32	  97%
              2048	  40	  98%
              3072	  48	  98%
              4096	  56	  99%
              5120	  64	  99%
            */

             long memory = AspNetMemoryMonitor.s_totalPhysical;
            Debug.Assert(memory != 0, "memory != 0");
            if (memory >= 0x100000000) {
                _pressureHigh = 99;
            }
            else if (memory >= 0x80000000) {
                _pressureHigh = 98;
            }
            else if (memory >= 0x40000000) {
                _pressureHigh = 97;
            }
            else if (memory >= 0x30000000) {
                _pressureHigh = 96;
            }
            else {
                _pressureHigh = 95;
            }

             _pressureLow = _pressureHigh - 9;

             InitHistory();

             _appManager = ApplicationManager.GetApplicationManager();

             _observers = new List<IObserver<LowPhysicalMemoryInfo>>();

             CacheSection cacheConfig = null;
            RuntimeConfig lkgAppConfig = RuntimeConfig.GetAppLKGConfig();
            RuntimeConfig appConfig = null;
            try {
                appConfig = RuntimeConfig.GetAppConfig();
                cacheConfig = appConfig.Cache;
            }
            catch (Exception) {
                cacheConfig = lkgAppConfig.Cache;
            }
            ReadConfig(cacheConfig);

#if !FEATURE_PAL
             // PerfCounter: Cache Percentage Machine Memory Limit Used
            //    = total physical memory used / total physical memory used limit
            PerfCounters.SetCounter(AppPerfCounter.CACHE_PERCENT_MACH_MEM_LIMIT_USED_BASE, _pressureHigh);
#endif
             // Create timer - poll interval is 'Infinite' until somebody calls Start/AdjustTimer()
            _timer = new Timer(new TimerCallback(this.MonitorThread), null, _currentPollInterval, _currentPollInterval);
        }

         void InitHistory() {
            Debug.Assert(_pressureHigh > 0, "_pressureHigh > 0");
            Debug.Assert(_pressureLow > 0, "_pressureLow > 0");
            Debug.Assert(_pressureLow <= _pressureHigh, "_pressureLow <= _pressureHigh");

             int pressure = GetCurrentPressure();

             _pressureHist = new int[HISTORY_COUNT];
            for (int i = 0; i < HISTORY_COUNT; i++) {
                _pressureHist[i] = pressure;
            }
        }

         void ReadConfig(CacheSection cacheSection) {
            if (cacheSection == null) {
                return;
            }

             // convert <cache privateBytesPollTime/> to milliseconds
            s_configuredPollInterval = (int)Math.Min(cacheSection.PrivateBytesPollTime.TotalMilliseconds, (double)Int32.MaxValue);

             // Read the percentagePhysicalMemoryUsedLimit set in config
            int limit = cacheSection.PercentagePhysicalMemoryUsedLimit;
            if (limit == 0) {
                // use defaults
                return;
            }

             _pressureHigh = Math.Max(3, limit);
            _pressureLow = Math.Max(1, _pressureHigh - 9);

 #if DBG
            Debug.Trace("CacheMemory", "LowMemoryMonitor.ReadConfig: _pressureHigh=" + _pressureHigh + 
                        ", _pressureLow=" + _pressureLow);
#endif
        }

         void Update() {
            int pressure = GetCurrentPressure();

             _i0 = (_i0 + 1) % HISTORY_COUNT;
            _pressureHist[_i0] = pressure;

 #if DBG
            Debug.Trace("CacheMemory", this.GetType().Name + ".Update: last=" + pressure 
                        + ",high=" + PressureHigh 
                        + ",low=" + PressureLow 
                        + " " + Debug.FormatLocalDate(DateTime.Now));
#endif
        }

         int GetCurrentPressure() {
#if !FEATURE_PAL
            UnsafeNativeMethods.MEMORYSTATUSEX memoryStatusEx = new UnsafeNativeMethods.MEMORYSTATUSEX();
            memoryStatusEx.Init();
            if (UnsafeNativeMethods.GlobalMemoryStatusEx(ref memoryStatusEx) == 0)
                return 0;

             int memoryLoad = memoryStatusEx.dwMemoryLoad;


            if (_pressureHigh != 0) {
                // PerfCounter: Cache Percentage Machine Memory Limit Used
                //    = total physical memory used / total physical memory used limit
                PerfCounters.SetCounter(AppPerfCounter.CACHE_PERCENT_MACH_MEM_LIMIT_USED, memoryLoad);
            }

             return memoryLoad;
#else
            return 0;
#endif
        }

         internal void AdjustTimer(bool disable = false) {
            lock (_timerLock) {

                 if (_timer == null)
                    return;

                 if (disable) {
                    _currentPollInterval = Timeout.Infinite;
                    _timer.Change(Timeout.Infinite, Timeout.Infinite);
                    return;
                }

                 // the order of these if statements is important
                // Also note that we use CacheSizeMonitor.PollInterval here. We can do this because we know
                // the value is initialized, since we wire up CacheInternal as the default cache from the get-go,
                // even if it is later replaced.

                 // If there is no pressure, interval should be the value from config
                int newPollInterval = s_configuredPollInterval;

                 // When above the high pressure mark, interval should be 5 seconds or less
                if (PressureLast >= PressureHigh) {
                    newPollInterval = Math.Min(newPollInterval, CacheCommon.MEMORYSTATUS_INTERVAL_5_SECONDS);
                }

                 // When above half the low pressure mark, interval should be 30 seconds or less
                else if (PressureLast > PressureLow / 2) {
                    newPollInterval = Math.Min(newPollInterval, CacheCommon.MEMORYSTATUS_INTERVAL_30_SECONDS);
                }

                 if (newPollInterval != _currentPollInterval)
                {
                    _currentPollInterval = newPollInterval;
                    _timer.Change(_currentPollInterval, _currentPollInterval);
                }
            }
        }

         internal void MonitorThread(object state) {
            bool requestGC;

             if (Interlocked.Exchange(ref _inMonitorThread, 1) != 0)
                return;
#if DBG
            Debug.Trace("LowMemoryMonitor", "**BEG** MonitorThread " + HttpRuntime.AppDomainAppId + ", " + DateTime.Now.ToString("T", System.Globalization.CultureInfo.InvariantCulture));
#endif
            try {
                // Dev10 633335: if the timer has been disposed, return without doing anything
                if (_timer == null)
                    return;

                 // The timer thread must always call Update so that the CacheManager
                // knows the size of the cache.
                Update();
                AdjustTimer();

                 if (PressureLast >= PressureHigh) {

                     long beginCount = HttpRuntime.Cache.InternalCache.ItemCount + HttpRuntime.Cache.ObjectCache.ItemCount;
                    Stopwatch sw = Stopwatch.StartNew();
                    requestGC = RaiseLowMemoryEvent(PressureLast, PressureHigh);
                    sw.Stop();
                    long trimmedOrExpired = Math.Max(0, beginCount - HttpRuntime.Cache.InternalCache.ItemCount - HttpRuntime.Cache.ObjectCache.ItemCount);

 #if DBG
                    Debug.Trace("LowMemoryMonitor", "**END** MonitorThread: " + HttpRuntime.AppDomainAppId
                                + ", beginTotalCount=" + beginCount
                                + ", trimmed=" + trimmedOrExpired
                                + ", Milliseconds=" + sw.ElapsedMilliseconds);
#endif

 #if PERF
                    SafeNativeMethods.OutputDebugString("LowMemoryMonitor.MonitorThread:"
                                                    + ", beginTotalCount=" + beginCount
                                                    + ", trimmed=" + trimmedOrExpired
                                                    + ", Milliseconds=" + sw.ElapsedMilliseconds + "\n");
#endif
                    if (!requestGC || _appManager.ShutdownInProgress)
                        return;

                     // collect and record statistics
                    Stopwatch sw2 = Stopwatch.StartNew();
                    GC.Collect();
                    sw2.Stop();
                }
            }
            finally {
                Interlocked.Exchange(ref _inMonitorThread, 0);
            }
        }

         bool RaiseLowMemoryEvent(int current, int limit) {
            LowPhysicalMemoryInfo info = new LowPhysicalMemoryInfo(current, limit);
            IObserver<LowPhysicalMemoryInfo>[] observers;

             lock (_observers) {
                observers = _observers.ToArray();
            }

             foreach (IObserver<LowPhysicalMemoryInfo> obs in observers) {
                try {
                    obs.OnNext(info);
                }
                catch (Exception e) {
                    // Unhandled Exceptions here will crash the process
                    Misc.ReportUnhandledException(e, new string[] { SR.GetString(SR.Unhandled_Monitor_Exception, "RaiseLowMemoryEvent", "LowMemoryMonitor") });
                }
            }

             return info.RequestGC;
        }

         internal void Subscribe(IObserver<LowPhysicalMemoryInfo> observer) {
            if (_observers != null && observer != null) {
                lock (_observers) {
                    if (_observers != null && observer != null) {
                        _observers.Add(observer);
                    }
                }
            }
        }

         internal void Unsubscribe(IObserver<LowPhysicalMemoryInfo> observer) {
            if (_observers != null && observer != null) {
                lock (_observers) {
                    if (_observers != null && observer != null) {
                        _observers.Remove(observer);
                    }
                }
            }
        }

         public void Start() {
            AdjustTimer();
        }

         public void Stop() {
            AdjustTimer(disable:true);
        }
    }
}