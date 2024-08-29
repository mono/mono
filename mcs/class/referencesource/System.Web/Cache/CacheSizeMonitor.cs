//------------------------------------------------------------------------------
// <copyright file="CacheSizeMonitor.cs" company="Microsoft">
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
    using Stopwatch = System.Diagnostics.Stopwatch;

     // Make sure we don't hit the per-process private bytes memory limit,
    // or the process will be restarted
    sealed class CacheSizeMonitor {

         const int SAMPLE_COUNT = 2;
        const int HISTORY_COUNT = 6;
        const int MEGABYTE_SHIFT = 20;
        const int KILOBYTE_SHIFT = 10;

         static uint s_pid = 0;
        static int  s_pollInterval;

         long[]          _cacheSizeSamples;
        DateTime[]      _cacheSizeSampleTimes;
        int             _idx;
        SRefMultiple    _sizedRef;
        int             _gen2Count;
        long            _memoryLimit;

         int           _pressureHigh;      // high pressure level
        int           _pressureMiddle;    // middle pressure level - target
        int           _pressureLow;       // low pressure level - slow growth here

         int           _i0;             
        int[]         _pressureHist;    
        int           _pressureTotal;   
        int           _pressureAvg;

         DateTime      _lastTrimTime = DateTime.MinValue;
        long          _lastTrimDurationTicks; // used only for debugging
        int           _lastTrimPercent;
        long          _totalCountBeforeTrim;
        long          _lastTrimCount;
        int           _lastTrimGen2Count = -1;

         static CacheSizeMonitor() {
        }

         internal CacheSizeMonitor(SRefMultiple sizedRef) {
            _sizedRef = sizedRef;
            _gen2Count = GC.CollectionCount(2);
            _cacheSizeSamples = new long[SAMPLE_COUNT];
            _cacheSizeSampleTimes = new DateTime[SAMPLE_COUNT];

             _pressureHigh = 99;
            _pressureMiddle = 98;
            _pressureLow = 97;

             InitHistory();
        }

         internal static int PollInterval {
            get { return s_pollInterval; }
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

         void InitHistory() {
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

         internal void Dispose() {
            SRefMultiple sref = _sizedRef;
            if (sref != null && Interlocked.CompareExchange(ref _sizedRef, null, sref) == sref) {
                sref.Dispose();
            }
        }

         internal void ReadConfig(CacheSection cacheSection) {
            // Read the private bytes limit set in config
            long    privateBytesLimit;
            privateBytesLimit = cacheSection.PrivateBytesLimit;
            _memoryLimit = 0;
             // per-process information
            //_memoryLimit = AspNetMemoryMonitor.ConfiguredProcessMemoryLimit;

             // VSWhidbey 546381: never override what the user specifies as the limit;
            // only call AutoPrivateBytesLimit when the user does not specify one.
            if (privateBytesLimit == 0 && _memoryLimit == 0) {
                // Zero means we impose a limit
                _memoryLimit = AspNetMemoryMonitor.ProcessPrivateBytesLimit;
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
                    s_pid = (uint) System.Diagnostics.Process.GetCurrentProcess().Id;
                    //s_pid = (uint) SafeNativeMethods.GetCurrentProcessId();

                 _pressureHigh = 100;
                _pressureMiddle = 90;
                _pressureLow = 80;
            }

             // convert <cache privateBytesPollTime/> to milliseconds
            s_pollInterval = (int)Math.Min(cacheSection.PrivateBytesPollTime.TotalMilliseconds, (double)Int32.MaxValue);

             // PerfCounter: Cache Percentage Process Memory Limit Used
            //    = memory used by this process / process memory limit at pressureHigh

             // Set private bytes limit in kilobytes becuase the counter is a DWORD
            //PerfCounters.SetCounter(AppPerfCounter.CACHE_PERCENT_PROC_MEM_LIMIT_USED_BASE, (int)(_memoryLimit >> KILOBYTE_SHIFT));

             Debug.Trace("CacheMemory", "CacheMemorySizePressure.ReadConfig: _pressureHigh=" + _pressureHigh + 
                        ", _pressureMiddle=" + _pressureMiddle + ", _pressureLow=" + _pressureLow);
        }

         int GetCurrentPressure() {
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

             // BUGBUG: need to update description for this counter, or deprecate and introduce a new one for this usage
            PerfCounters.SetCounter(AppPerfCounter.CACHE_PERCENT_PROC_MEM_LIMIT_USED, (int)(cacheSize >> KILOBYTE_SHIFT));

             int result = (int)(cacheSize * 100 / _memoryLimit);
            return result;
        }

         internal int GetPercentToTrim() {
            int gen2Count = GC.CollectionCount(2);
            int percent = 0;

             // has there been a Gen 2 Collection since the last trim?
            if (gen2Count != _lastTrimGen2Count) {
                if (IsAboveHighPressure()) {
                    long cacheSize = _cacheSizeSamples[_idx];
                    if (cacheSize > _memoryLimit) {
                        percent = Math.Min(100, (int)((cacheSize - _memoryLimit) * 100L / cacheSize));
                    }
#if PERF
                SafeNativeMethods.OutputDebugString(String.Format("CacheMemorySizePressure.GetPercentToTrim: percent={0:N}, lastTrimPercent={1:N}\n",
                                                    percent,
                                                    _lastTrimPercent));
#endif

                 }
            }

             return percent;
        }

         internal bool HasLimit() {
            return _memoryLimit != 0;
        }

     }
}