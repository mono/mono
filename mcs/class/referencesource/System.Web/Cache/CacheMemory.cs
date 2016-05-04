//------------------------------------------------------------------------------
// <copyright file="CacheMemory.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Caching {
    using System.Web.Configuration;
    using System.Runtime.InteropServices;
    using System.Web.Util;
    using System.Web;
    using System.Web.Hosting;
    using System.Threading;

    abstract class CacheMemoryPressure {
        protected const int     TERABYTE_SHIFT = 40;
        protected const long    TERABYTE = 1L << TERABYTE_SHIFT;
        
        protected const int     GIGABYTE_SHIFT = 30;
        protected const long    GIGABYTE = 1L << GIGABYTE_SHIFT;
        
        protected const int     MEGABYTE_SHIFT = 20;
        protected const long    MEGABYTE = 1L << MEGABYTE_SHIFT; // 1048576
        
        protected const int     KILOBYTE_SHIFT = 10;
        protected const long    KILOBYTE = 1L << KILOBYTE_SHIFT; // 1024

        protected const int     HISTORY_COUNT = 6;

        protected int           _pressureHigh;      // high pressure level
        protected int           _pressureMiddle;    // middle pressure level - target
        protected int           _pressureLow;       // low pressure level - slow growth here

        protected int           _i0;             
        protected int[]         _pressureHist;    
        protected int           _pressureTotal;   
        protected int           _pressureAvg;     

        private static long     s_totalPhysical;
        private static long     s_totalVirtual;

        static CacheMemoryPressure() {
            UnsafeNativeMethods.MEMORYSTATUSEX  memoryStatusEx = new UnsafeNativeMethods.MEMORYSTATUSEX();
            memoryStatusEx.Init();
            if (UnsafeNativeMethods.GlobalMemoryStatusEx(ref memoryStatusEx) != 0) {
                s_totalPhysical = memoryStatusEx.ullTotalPhys;
                s_totalVirtual = memoryStatusEx.ullTotalVirtual;
            }
        }

        internal static long TotalPhysical { get { return s_totalPhysical; } }
        internal static long TotalVirtual { get { return s_totalVirtual; } }

        protected abstract int GetCurrentPressure();
        
        internal abstract int GetPercentToTrim(DateTime lastTrimTime, int lastTrimPercent);

        internal virtual void ReadConfig(CacheSection cacheSection) {}
        
        protected void InitHistory() {
            Debug.Assert(_pressureHigh > 0, "_pressureHigh > 0");
            Debug.Assert(_pressureLow > 0, "_pressureLow > 0");
            Debug.Assert(_pressureLow <= _pressureHigh, "_pressureLow <= _pressureHigh");

            int pressure = GetCurrentPressure();

            _pressureHist = new int[HISTORY_COUNT];
            for (int i = 0; i < HISTORY_COUNT; i++) {
                _pressureHist[i] = pressure;
                _pressureTotal +=  pressure;
            }

            _pressureAvg = pressure;
        }

        // Get current pressure and update history
        internal void Update() {
            int pressure = GetCurrentPressure();

            _i0 = (_i0 + 1) % HISTORY_COUNT;
            _pressureTotal -= _pressureHist[_i0];
            _pressureTotal += pressure;
            _pressureHist[_i0] = pressure;
            _pressureAvg = _pressureTotal / HISTORY_COUNT; 

#if DBG
            Debug.Trace("CacheMemory", this.GetType().Name + ".Update: last=" + pressure 
                        + ",avg=" + PressureAvg 
                        + ",high=" + PressureHigh 
                        + ",low=" + PressureLow 
                        + ",middle=" + PressureMiddle 
                        + " " + Debug.FormatLocalDate(DateTime.Now));
#endif
        }

        internal int PressureLast {
            get { 
                return _pressureHist[_i0]; 
            }
        }

        internal int PressureAvg {
            get { return _pressureAvg; }
        }

        internal int PressureHigh {
            get { return _pressureHigh; } 
        }

        internal int PressureLow {
            get { return _pressureLow; } 
        }

        internal int PressureMiddle {
            get { return _pressureMiddle; } 
        }

        internal bool IsAboveHighPressure() {
            return PressureLast >= PressureHigh;
        }

        internal bool IsAboveMediumPressure() {
            return PressureLast > PressureMiddle;
        }
    }

    // The GC aggressively collects when it receives a low memory notification. Make sure
    // we have released references before it aggressively collects.
    sealed class CacheMemoryTotalMemoryPressure : CacheMemoryPressure {
        const int                       MIN_TOTAL_MEMORY_TRIM_PERCENT                 = 10;
        static readonly long            TARGET_TOTAL_MEMORY_TRIM_INTERVAL_TICKS          = 5 * TimeSpan.TicksPerMinute;

        internal CacheMemoryTotalMemoryPressure() {
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

            _pressureMiddle = _pressureHigh - 2;
            _pressureLow = _pressureHigh - 9;
            
            InitHistory();

            // PerfCounter: Cache Percentage Machine Memory Limit Used
            //    = total physical memory used / total physical memory used limit
            PerfCounters.SetCounter(AppPerfCounter.CACHE_PERCENT_MACH_MEM_LIMIT_USED_BASE, _pressureHigh);
        }

        override internal void ReadConfig(CacheSection cacheSection) {
            // Read the percentagePhysicalMemoryUsedLimit set in config
            int limit = cacheSection.PercentagePhysicalMemoryUsedLimit;
            if (limit == 0) {
                // use defaults
                return;
            }

            _pressureHigh = Math.Max(3, limit);
            _pressureMiddle = Math.Max(2, _pressureHigh - 2);
            _pressureLow = Math.Max(1, _pressureHigh - 9);
            
            // PerfCounter: Cache Percentage Machine Memory Limit Used
            //    = total physical memory used / total physical memory used limit
            PerfCounters.SetCounter(AppPerfCounter.CACHE_PERCENT_MACH_MEM_LIMIT_USED_BASE, _pressureHigh);
            
#if DBG
            Debug.Trace("CacheMemory", "CacheMemoryTotalMemoryPressure.ReadConfig: _pressureHigh=" + _pressureHigh + 
                        ", _pressureMiddle=" + _pressureMiddle + ", _pressureLow=" + _pressureLow);
#endif
        }

        override protected int GetCurrentPressure() {
            UnsafeNativeMethods.MEMORYSTATUSEX  memoryStatusEx = new UnsafeNativeMethods.MEMORYSTATUSEX();
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
        }

        internal override int GetPercentToTrim(DateTime lastTrimTime, int lastTrimPercent) {
            int percent = 0;
            if (IsAboveHighPressure()) {
                // choose percent such that we don't repeat this for ~5 (TARGET_TOTAL_MEMORY_TRIM_INTERVAL) minutes, 
                // but keep the percentage between 10 and 50.
                DateTime utcNow = DateTime.UtcNow;
                long ticksSinceTrim = utcNow.Subtract(lastTrimTime).Ticks;
                if (ticksSinceTrim > 0) {
                    percent = Math.Min(50, (int) ((lastTrimPercent * TARGET_TOTAL_MEMORY_TRIM_INTERVAL_TICKS) / ticksSinceTrim));
                    percent = Math.Max(MIN_TOTAL_MEMORY_TRIM_PERCENT, percent);
                }

#if PERF
                SafeNativeMethods.OutputDebugString(String.Format("CacheMemoryTotalMemoryPressure.GetPercentToTrim: percent={0:N}, lastTrimPercent={1:N}, secondsSinceTrim={2:N}\n",
                                                    percent,
                                                    lastTrimPercent,
                                                    ticksSinceTrim/TimeSpan.TicksPerSecond));
#endif
            }        

            return percent;
        }

        // Returns the percentage of physical machine memory that can be consumed by an 
        // application before ASP.NET starts forcibly removing items from the cache.
        internal long MemoryLimit {
            get { return _pressureHigh; }
        }
    }

    // Make sure we don't hit the per-process private bytes memory limit,
    // or the process will be restarted
    sealed class CacheMemorySizePressure : CacheMemoryPressure {
        const long           PRIVATE_BYTES_LIMIT_2GB                      = 800 * MEGABYTE;
        const long           PRIVATE_BYTES_LIMIT_3GB                      = 1800 * MEGABYTE;
        const long           PRIVATE_BYTES_LIMIT_64BIT                    = 1L * TERABYTE;
        const int            SAMPLE_COUNT                                 = 2;

        static bool s_isIIS6 = HostingEnvironment.IsUnderIIS6Process;
        static long s_autoPrivateBytesLimit = -1;
        static uint s_pid = 0;
        static int  s_pollInterval;
        static long s_workerProcessMemoryLimit = -1;
        static long s_effectiveProcessMemoryLimit = -1;

        long                            _totalCacheSize;
        long[]                          _cacheSizeSamples;
        DateTime[]                      _cacheSizeSampleTimes;
        int                             _idx;
        SRefMultiple                    _sizedRef;
        int                             _gen2Count;

        long        _memoryLimit;
        DateTime    _startupTime;

        internal CacheMemorySizePressure(SRefMultiple sizedRef) {
            _sizedRef = sizedRef;
            _gen2Count = GC.CollectionCount(2);
            _cacheSizeSamples = new long[SAMPLE_COUNT];
            _cacheSizeSampleTimes = new DateTime[SAMPLE_COUNT];

            _pressureHigh = 99;
            _pressureMiddle = 98;
            _pressureLow = 97;

            _startupTime = DateTime.UtcNow;

            InitHistory();
        }

        // Auto-generate the private bytes limit:
        // - On 64bit, the auto value is MIN(60% physical_ram, 1 TB)
        // - On x86, for 2GB, the auto value is MIN(60% physical_ram, 800 MB)
        // - On x86, for 3GB, the auto value is MIN(60% physical_ram, 1800 MB)
        //
        // - If it's not a hosted environment (e.g. console app), the 60% in the above
        //   formulas will become 100% because in un-hosted environment we don't launch
        //   other processes such as compiler, etc.
        private static long AutoPrivateBytesLimit {
            get {
                long memoryLimit = s_autoPrivateBytesLimit;
                if (memoryLimit == -1) {
                
                    bool    is64bit = (IntPtr.Size == 8);
                    
                    long totalPhysical = TotalPhysical;
                    long totalVirtual = TotalVirtual;
                    if (totalPhysical != 0) {
                        long    recommendedPrivateByteLimit;                        
                        if (is64bit) {
                            recommendedPrivateByteLimit = PRIVATE_BYTES_LIMIT_64BIT;
                        }
                        else {
                            // Figure out if it's 2GB or 3GB
                            
                            if (totalVirtual > 2 * GIGABYTE) {
                                recommendedPrivateByteLimit = PRIVATE_BYTES_LIMIT_3GB;
                            }
                            else {
                                recommendedPrivateByteLimit = PRIVATE_BYTES_LIMIT_2GB;
                            }
                        }

                        // if we're hosted, use 60% of physical RAM; otherwise 100%
                        long usableMemory = HostingEnvironment.IsHosted ? totalPhysical * 3 / 5 : totalPhysical;
                        memoryLimit = Math.Min(usableMemory, recommendedPrivateByteLimit);
                    }
                    else {
                        // If GlobalMemoryStatusEx fails, we'll use these as our auto-gen private bytes limit
                        memoryLimit = is64bit ? PRIVATE_BYTES_LIMIT_64BIT : PRIVATE_BYTES_LIMIT_2GB;
                    }
                    Interlocked.Exchange(ref s_autoPrivateBytesLimit, memoryLimit);
                }

                return memoryLimit;
            }
        }

        internal static long EffectiveProcessMemoryLimit {
            get {
                long memoryLimit = s_effectiveProcessMemoryLimit;
                if (memoryLimit == -1) {
                    memoryLimit = WorkerProcessMemoryLimit;
                    if (memoryLimit == 0) {
                        memoryLimit = AutoPrivateBytesLimit;
                    }
                    Interlocked.Exchange(ref s_effectiveProcessMemoryLimit, memoryLimit);
                }
                return memoryLimit;
            }
        }

        internal static long WorkerProcessMemoryLimit {
            get {
                long memoryLimit = s_workerProcessMemoryLimit;
                if (memoryLimit == -1) {
                    // per-process information
                    if (UnsafeNativeMethods.GetModuleHandle(ModName.WP_FULL_NAME) != IntPtr.Zero) {
                        memoryLimit = (long)UnsafeNativeMethods.PMGetMemoryLimitInMB() << 20;
                    }
                    else if (UnsafeNativeMethods.GetModuleHandle(ModName.W3WP_FULL_NAME) != IntPtr.Zero) {
                        IServerConfig serverConfig = ServerConfig.GetInstance();
                        memoryLimit = (long)serverConfig.GetW3WPMemoryLimitInKB() << 10;
                    }
                    Interlocked.Exchange(ref s_workerProcessMemoryLimit, memoryLimit);
                }
                return memoryLimit;
            }
        }

        internal void Dispose() {
            SRefMultiple sref = _sizedRef;
            if (sref != null && Interlocked.CompareExchange(ref _sizedRef, null, sref) == sref) {
                sref.Dispose();
            }
            ApplicationManager appManager = HostingEnvironment.GetApplicationManager();
            if (appManager != null) {
                long sizeUpdate = (0 - _cacheSizeSamples[_idx]);
                appManager.GetUpdatedTotalCacheSize(sizeUpdate);
            }
        }

        override internal void ReadConfig(CacheSection cacheSection) {
            // Read the private bytes limit set in config
            long    privateBytesLimit;
            privateBytesLimit = cacheSection.PrivateBytesLimit;

            // per-process information
            _memoryLimit = WorkerProcessMemoryLimit;
            
            // VSWhidbey 546381: never override what the user specifies as the limit;
            // only call AutoPrivateBytesLimit when the user does not specify one.
            if (privateBytesLimit == 0 && _memoryLimit == 0) {
                // Zero means we impose a limit
                _memoryLimit = EffectiveProcessMemoryLimit;
            }
            else if (privateBytesLimit != 0 && _memoryLimit != 0) {
                // Take the min of "process recycle limit" and our internal "private bytes limit"
                _memoryLimit = Math.Min(_memoryLimit, privateBytesLimit);
            }
            else if (privateBytesLimit != 0) {
                // _memoryLimit is 0, but privateBytesLimit is non-zero, so use it as the limit
                _memoryLimit = privateBytesLimit;
            }

            Debug.Trace("CacheMemory", "CacheMemorySizePressure.ReadConfig: _memoryLimit=" + (_memoryLimit >> MEGABYTE_SHIFT) + "Mb");

            if (_memoryLimit > 0) {

                if (s_pid == 0) // only set this once
                    s_pid = (uint) SafeNativeMethods.GetCurrentProcessId();

                _pressureHigh = 100;
                _pressureMiddle = 90;
                _pressureLow = 80;
            }
            
            // convert <cache privateBytesPollTime/> to milliseconds
            s_pollInterval = (int)Math.Min(cacheSection.PrivateBytesPollTime.TotalMilliseconds, (double)Int32.MaxValue);

            // PerfCounter: Cache Percentage Process Memory Limit Used
            //    = memory used by this process / process memory limit at pressureHigh

            // Set private bytes limit in kilobytes becuase the counter is a DWORD
            PerfCounters.SetCounter(AppPerfCounter.CACHE_PERCENT_PROC_MEM_LIMIT_USED_BASE, (int)(_memoryLimit >> KILOBYTE_SHIFT));

            Debug.Trace("CacheMemory", "CacheMemorySizePressure.ReadConfig: _pressureHigh=" + _pressureHigh + 
                        ", _pressureMiddle=" + _pressureMiddle + ", _pressureLow=" + _pressureLow);
        }

        internal long MemoryLimit {
            get { return _memoryLimit; }
        }

        internal static int PollInterval {
            get { return s_pollInterval; }
        }

        override protected int GetCurrentPressure() {
            // Call GetUpdatedTotalCacheSize to update the total
            // cache size, if there has been a recent Gen 2 Collection.
            // This update must happen, otherwise the CacheManager won't 
            // know the total cache size.
            int gen2Count = GC.CollectionCount(2);
            SRefMultiple sref = _sizedRef;
            if (gen2Count != _gen2Count && sref != null) {
                // update _gen2Count
                _gen2Count = gen2Count;

                // the SizedRef is only updated after a Gen2 Collection
                
                // increment the index (it's either 1 or 0)
                Debug.Assert(SAMPLE_COUNT == 2);
                _idx = _idx ^ 1;
                // remember the sample time
                _cacheSizeSampleTimes[_idx] = DateTime.UtcNow;
                // remember the sample value
                _cacheSizeSamples[_idx] = sref.ApproximateSize;
#if DBG
                Debug.Trace("CacheMemory", "SizedRef.ApproximateSize=" + _cacheSizeSamples[_idx]);
#endif
                // we may not be "hosted"
                ApplicationManager appManager = HostingEnvironment.GetApplicationManager();
                if (appManager != null) {
                    // update total cache size
                    long sizeUpdate = _cacheSizeSamples[_idx] - _cacheSizeSamples[_idx ^ 1];
                    _totalCacheSize = appManager.GetUpdatedTotalCacheSize(sizeUpdate);
                }
                else {
                    // if we're not hosted, this cache's size is the total cache size
                    _totalCacheSize = _cacheSizeSamples[_idx];
                }
            }

            // if there's no memory limit, then there's nothing more to do
            if (_memoryLimit <= 0) {
                return 0;
            }

            long cacheSize =  _cacheSizeSamples[_idx];

            // use _memoryLimit as an upper bound so that pressure is a percentage (between 0 and 100, inclusive).
            if (cacheSize > _memoryLimit) {
                cacheSize = _memoryLimit;
            }

            // PerfCounter: Cache Percentage Process Memory Limit Used
            //    = memory used by this process / process memory limit at pressureHigh
            // Set private bytes used in kilobytes because the counter is a DWORD

            // 
            PerfCounters.SetCounter(AppPerfCounter.CACHE_PERCENT_PROC_MEM_LIMIT_USED, (int)(cacheSize >> KILOBYTE_SHIFT));

            int result = (int)(cacheSize * 100 / _memoryLimit);
            return result;
        }

        internal override int GetPercentToTrim(DateTime lastTrimTime, int lastTrimPercent) {
            int percent = 0;
            if (IsAboveHighPressure()) {
                long cacheSize = _cacheSizeSamples[_idx];
                if (cacheSize > _memoryLimit) {
                    percent = Math.Min(100, (int) ((cacheSize - _memoryLimit) * 100L/ cacheSize));
                }

#if PERF
                SafeNativeMethods.OutputDebugString(String.Format("CacheMemorySizePressure.GetPercentToTrim: percent={0:N}, lastTrimPercent={1:N}\n",
                                                    percent,
                                                    lastTrimPercent));
#endif

            }
            return percent;
        }

        internal bool HasLimit() {
            return _memoryLimit != 0;
        }

    }

    class CacheMemoryStats {
        DateTime                        _lastTrimTime = DateTime.MinValue;
        long                            _lastTrimDurationTicks; // used only for debugging
        int                             _lastTrimPercent;
        long                            _totalCountBeforeTrim;
        long                            _lastTrimCount;
        int                             _lastTrimGen2Count = -1;
        CacheMemoryTotalMemoryPressure  _pressureTotalMemory;
        CacheMemorySizePressure         _pressureCacheSize;

        internal CacheMemoryStats(SRefMultiple sizedRef) {
            _pressureTotalMemory = new CacheMemoryTotalMemoryPressure();
            _pressureCacheSize = new CacheMemorySizePressure(sizedRef);
        }

        internal CacheMemorySizePressure CacheSizePressure {
            get {return _pressureCacheSize;}
        }

        internal CacheMemoryTotalMemoryPressure TotalMemoryPressure {
            get {return _pressureTotalMemory;}
        }

        internal bool IsAboveHighPressure() {
            return _pressureTotalMemory.IsAboveHighPressure() || _pressureCacheSize.IsAboveHighPressure();
        }

        internal bool IsAboveMediumPressure() {
            return _pressureTotalMemory.IsAboveMediumPressure() || _pressureCacheSize.IsAboveMediumPressure();
        }

        internal void ReadConfig(CacheSection cacheSection) {
            _pressureTotalMemory.ReadConfig(cacheSection);
            _pressureCacheSize.ReadConfig(cacheSection);
        }

        internal void Update() {
            _pressureTotalMemory.Update();
            _pressureCacheSize.Update();
        }

        internal void Dispose() {
            _pressureCacheSize.Dispose();
        }

        internal int GetPercentToTrim() {
            int gen2Count = GC.CollectionCount(2);
            // has there been a Gen 2 Collection since the last trim?
            if (gen2Count != _lastTrimGen2Count) {
                return Math.Max(_pressureTotalMemory.GetPercentToTrim(_lastTrimTime, _lastTrimPercent), _pressureCacheSize.GetPercentToTrim(_lastTrimTime, _lastTrimPercent));
            }
            else {
                return 0;
            }
        }

        internal void SetTrimStats(long trimDurationTicks, long totalCountBeforeTrim, long trimCount) {
            _lastTrimDurationTicks = trimDurationTicks;

            int gen2Count = GC.CollectionCount(2);
            // has there been a Gen 2 Collection since the last trim?
            if (gen2Count != _lastTrimGen2Count) {
                _lastTrimTime = DateTime.UtcNow;
                _totalCountBeforeTrim = totalCountBeforeTrim;
                _lastTrimCount = trimCount;
            }
            else {
                // we've done multiple trims between Gen 2 collections, so only add to the trim count
                _lastTrimCount += trimCount;
            }
            _lastTrimGen2Count = gen2Count;

            _lastTrimPercent = (int)((_lastTrimCount * 100L) / _totalCountBeforeTrim);
        }
    }
}
