// <copyright file="PhysicalMemoryMonitor.cs" company="Microsoft">
//   Copyright (c) 2009 Microsoft Corporation.  All rights reserved.
// </copyright>
using System;
using System.Runtime.Caching.Configuration;
using System.Security;

namespace System.Runtime.Caching {
    // PhysicalMemoryMonitor monitors the amound of physical memory used on the machine
    // and helps us determine when to drop entries to avoid paging and GC thrashing.
    // The limit is configurable (see ConfigUtil.cs).
    internal sealed class PhysicalMemoryMonitor : MemoryMonitor {
        const int MIN_TOTAL_MEMORY_TRIM_PERCENT = 10;
        static readonly long TARGET_TOTAL_MEMORY_TRIM_INTERVAL_TICKS = 5 * TimeSpan.TicksPerMinute;

        // Returns the percentage of physical machine memory that can be consumed by an 
        // application before ASP.NET starts forcibly removing items from the cache.
        internal long MemoryLimit {
            get { return _pressureHigh; }
        }

        private PhysicalMemoryMonitor() {
            // hide default ctor
        }

        internal PhysicalMemoryMonitor(int physicalMemoryLimitPercentage) {
            /*
              The chart below shows physical memory in megabytes, and the 1, 3, and 10% values.
              When we reach "middle" pressure, we begin trimming the cache.

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

            long memory = TotalPhysical;
            Dbg.Assert(memory != 0, "memory != 0");
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

            SetLimit(physicalMemoryLimitPercentage);
            InitHistory();

            // PerfCounter: Cache Percentage Machine Memory Limit Used
            //    = total physical memory used / total physical memory used limit
            // PerfCounters.SetCounter(AppPerfCounter.CACHE_PERCENT_MACH_MEM_LIMIT_USED_BASE, _pressureHigh);
        }

        [SecuritySafeCritical]
        protected override int GetCurrentPressure() {
            MEMORYSTATUSEX memoryStatusEx = new MEMORYSTATUSEX();
            memoryStatusEx.Init();
            if (UnsafeNativeMethods.GlobalMemoryStatusEx(ref memoryStatusEx) == 0)
                return 0;

            int memoryLoad = memoryStatusEx.dwMemoryLoad;
            //if (_pressureHigh != 0) {
                // PerfCounter: Cache Percentage Machine Memory Limit Used
                //    = total physical memory used / total physical memory used limit
                //PerfCounters.SetCounter(AppPerfCounter.CACHE_PERCENT_MACH_MEM_LIMIT_USED, memoryLoad);
            //}

            return memoryLoad;
        }

        internal override int GetPercentToTrim(DateTime lastTrimTime, int lastTrimPercent) {
            int percent = 0;
            if (IsAboveHighPressure()) {
                // choose percent such that we don't repeat this for ~5 (TARGET_TOTAL_MEMORY_TRIM_INTERVAL) minutes, 
                // but keep the percentage between 10 and 50.
                DateTime utcNow = DateTime.UtcNow;
                long ticksSinceTrim = utcNow.Subtract(lastTrimTime).Ticks;
                if (ticksSinceTrim > 0) {
                    percent = Math.Min(50, (int)((lastTrimPercent * TARGET_TOTAL_MEMORY_TRIM_INTERVAL_TICKS) / ticksSinceTrim));
                    percent = Math.Max(MIN_TOTAL_MEMORY_TRIM_PERCENT, percent);
                }

#if PERF
                SafeNativeMethods.OutputDebugString(String.Format("PhysicalMemoryMonitor.GetPercentToTrim: percent={0:N}, lastTrimPercent={1:N}, secondsSinceTrim={2:N}\n",
                                                    percent,
                                                    lastTrimPercent,
                                                    ticksSinceTrim/TimeSpan.TicksPerSecond));
#endif
            }

            return percent;
        }

        internal void SetLimit(int physicalMemoryLimitPercentage) {
            if (physicalMemoryLimitPercentage == 0) {
                // use defaults
                return;
            }
            _pressureHigh = Math.Max(3, physicalMemoryLimitPercentage);
            _pressureLow = Math.Max(1, _pressureHigh - 9);
#if DBG
            Dbg.Trace("MemoryCacheStats", "PhysicalMemoryMonitor.SetLimit: _pressureHigh=" + _pressureHigh +
                        ", _pressureLow=" + _pressureLow);
#endif
        }
    }
}
