using System;
using System.Globalization;
using System.Web;
using System.Web.Util;
using System.Threading;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Debug = System.Web.Util.Debug;

//
// Welcome to the CacheManager class, CM for short.  CM monitors private bytes for the
// worker process.  If the Private Bytes limit is about to be exceeded, CM will trim
// the cache (as necessary), and induce a GC to prevent the process from recycling.
//
// A timer thread is used to monitor Private Bytes.  The interval is adjusted depending
// on the current memory pressure.  The maximum interval is every 2 minutes, and the
// minimum interval is every 5 seconds.
//

namespace System.Web.Hosting {
    internal class CacheManager: IDisposable {
        const int      HIGH_FREQ_INTERVAL_S                     = 5;
        const int      HIGH_FREQ_INTERVAL_MS                    = 5 * Msec.ONE_SECOND;
        const int      MEDIUM_FREQ_INTERVAL_S                   = 30;
        const int      MEDIUM_FREQ_INTERVAL_MS                  = 30 * Msec.ONE_SECOND;
        const int      LOW_FREQ_INTERVAL_S                      = 120;
        const int      LOW_FREQ_INTERVAL_MS                     = 120 * Msec.ONE_SECOND;
        const int      MEGABYTE_SHIFT                           = 20;
        const long     MEGABYTE                                 = 1L << MEGABYTE_SHIFT; // 1048576
        const int      SAMPLE_COUNT                             = 2;
        const int      DELTA_SAMPLE_COUNT                       = 10;

        private        ApplicationManager _appManager;

        private        long       _totalCacheSize;
        private        long       _trimDurationTicks;
        private        int        _lastTrimPercent              = 10; // starts at 10, but changes to fit workload
        private        long       _inducedGCMinInterval         = TimeSpan.TicksPerSecond * 5; // starts at 5 seconds, but changes to fit workload
        private        DateTime   _inducedGCFinishTime          = DateTime.MinValue;
        private        long       _inducedGCDurationTicks;
        private        int        _inducedGCCount;
        private        long       _inducedGCPostPrivateBytes;
        private        long       _inducedGCPrivateBytesChange;

        private        int        _currentPollInterval          = MEDIUM_FREQ_INTERVAL_MS;
        private        DateTime   _timerSuspendTime             = DateTime.MinValue;
        private        int        _inPBytesMonitorThread;
        private        Timer      _timer;
        private        Object     _timerLock                    = new object();

        private        long       _limit; // the "effective" worker process Private Bytes limit
        private        long       _highPressureMark;
        private        long       _mediumPressureMark;
        private        long       _lowPressureMark;
        private        long[]     _deltaSamples; // a history of the increase in private bytes per second
        private        int        _idxDeltaSamples;
        private        long       _maxDelta; // the maximum expected increase in private bytes per second
        private        long       _minMaxDelta;  // _maxDelta must always be at least this large
        private        long[]     _samples; // a history of the sample values (private bytes for the process)
        private        DateTime[] _sampleTimes; // time at which samples were taken
        private        int        _idx;

        private        bool       _useGetProcessMemoryInfo;
        private        uint       _pid;
        private        bool       _disposed;

        private CacheManager() {}

        internal CacheManager(ApplicationManager appManager, long privateBytesLimit) {
#if PERF
            SafeNativeMethods.OutputDebugString(String.Format("Creating CacheManager with PrivateBytesLimit = {0:N}\n", privateBytesLimit));
#endif
            // don't create timer if there's no memory limit
            if (privateBytesLimit <= 0) {
                return;
            }

            _appManager = appManager;
            _limit = privateBytesLimit;

            _pid = (uint) SafeNativeMethods.GetCurrentProcessId();
            
            // the initial expected maximum increase in private bytes is 2MB per second per CPU
            _minMaxDelta = 2 * MEGABYTE * SystemInfo.GetNumProcessCPUs();
            AdjustMaxDeltaAndPressureMarks(_minMaxDelta);

            _samples = new long[SAMPLE_COUNT];
            _sampleTimes = new DateTime[SAMPLE_COUNT];
            _useGetProcessMemoryInfo = (VersionInfo.ExeName == "w3wp");
            _deltaSamples = new long[DELTA_SAMPLE_COUNT];
            
            // start timer with initial poll interval
            _timer = new Timer(new TimerCallback(this.PBytesMonitorThread), null, _currentPollInterval, _currentPollInterval);
        }


        void Adjust() {
            // not thread-safe, only invoke from timer callback
            Debug.Assert(_inPBytesMonitorThread == 1);

            Debug.Assert(SAMPLE_COUNT == 2);
            // current sample
            long s2 = _samples[_idx];
            // previous sample
            long s1 = _samples[_idx ^ 1];

            // adjust _maxDelta and pressure marks
            if (s2 > s1 && s1 > 0) {
                // current time
                DateTime d2 = _sampleTimes[_idx];
                // previous time
                DateTime d1 = _sampleTimes[_idx ^ 1];

                long numBytes = s2 - s1;
                long numSeconds = (long)Math.Round(d2.Subtract(d1).TotalSeconds);
                if (numSeconds > 0) {
                    long delta = numBytes / numSeconds;
                    _deltaSamples[_idxDeltaSamples] = delta;
                    _idxDeltaSamples = (_idxDeltaSamples + 1) % DELTA_SAMPLE_COUNT;
                    // update rate of change in private bytes and pressure marks
                    AdjustMaxDeltaAndPressureMarks(delta);
                }
            }

            lock (_timerLock) {
                if (_timer == null) {
                    return;
                }

                // adjust timer frequency
                if (s2 > _mediumPressureMark) {
                    if (_currentPollInterval > HIGH_FREQ_INTERVAL_MS) {
                        _currentPollInterval = HIGH_FREQ_INTERVAL_MS;
                        _timer.Change(_currentPollInterval, _currentPollInterval);
                    }
                }
                else if (s2 > _lowPressureMark) {
                    if (_currentPollInterval > MEDIUM_FREQ_INTERVAL_MS) {
                        _currentPollInterval = MEDIUM_FREQ_INTERVAL_MS;
                        _timer.Change(_currentPollInterval, _currentPollInterval);
                    }
                }
                else {
                    if (_currentPollInterval != LOW_FREQ_INTERVAL_MS) {
                        _currentPollInterval = LOW_FREQ_INTERVAL_MS;
                        _timer.Change(_currentPollInterval, _currentPollInterval);
                    }
                }
            }
        }

        void AdjustMaxDeltaAndPressureMarks(long delta) {
            // not thread-safe...only invoke from ctor or timer callback
            Debug.Assert(_inPBytesMonitorThread == 1 || _timer == null);

            // The value of _maxDelta is the largest rate of change we've seen, 
            // but it is reduced if the rate is now consistently less than what
            // it once was.
            long newMaxDelta = _maxDelta;
            if (delta > newMaxDelta) {
                // set maxDelta to the current rate of change
                newMaxDelta = delta;
            }
            else {
                // if _maxDelta is at least four times larger than every sample rate in the history,
                // then reduce _maxDelta
                bool reduce = true;
                long maxDelta = _maxDelta / 4;
                foreach (long rate in _deltaSamples) {
                    if (rate > maxDelta) {
                        reduce = false;
                        break;
                    }
                }
                if (reduce) {
                    newMaxDelta = maxDelta * 2;
                }
            }

            // ensure that maxDelta is sufficiently large so that the _highPressureMark is sufficiently
            // far away from the memory limit
            newMaxDelta = Math.Max(newMaxDelta, _minMaxDelta);
            
            // Do we have a new maxDelta?  If so, adjust it and pressure marks.
            if (_maxDelta != newMaxDelta) {
                // adjust _maxDelta
                _maxDelta = newMaxDelta;
                // instead of using _maxDelta, use twice _maxDelta since recycling is
                // expensive and the real delta fluctuates
                _highPressureMark = Math.Max(_limit * 9 / 10, _limit - (_maxDelta * 2 * HIGH_FREQ_INTERVAL_S));
                _lowPressureMark =  Math.Max(_limit * 6 / 10, _limit - (_maxDelta * 2 * LOW_FREQ_INTERVAL_S));
                _mediumPressureMark = Math.Max((_highPressureMark + _lowPressureMark) / 2 , _limit - (_maxDelta * 2 * MEDIUM_FREQ_INTERVAL_S));
                _mediumPressureMark = Math.Min(_highPressureMark , _mediumPressureMark);

#if PERF
                SafeNativeMethods.OutputDebugString(String.Format("CacheManager.AdjustMaxDeltaAndPressureMarks:  _highPressureMark={0:N}, _mediumPressureMark={1:N}, _lowPressureMark={2:N}, _maxDelta={3:N}\n", _highPressureMark, _mediumPressureMark, _lowPressureMark, _maxDelta));
#endif

#if DBG
                Debug.Trace("CacheMemory", "AdjustMaxDeltaAndPressureMarks "
                            + "delta=" + delta
                            + ", _maxDelta=" + _maxDelta
                            + ", _highPressureMark=" + _highPressureMark
                            + ", _mediumPressureMark=" + _mediumPressureMark
                            + ", _lowPressureMark=" + _lowPressureMark);
#endif

            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", Justification="Need to call GC.Collect.")]
        private void CollectInfrequently(long privateBytes) {
            // not thread-safe, only invoke from timer callback
            Debug.Assert(_inPBytesMonitorThread == 1);

            // The Server GC on x86 can traverse ~200mb per CPU per second, and the maximum heap size
            // is about 3400mb, so the worst case scenario on x86 would take about 8 seconds to collect
            // on a dual CPU box. 
            //
            // The Server GC on x64 can traverse ~300mb per CPU per second, so a 6000 MB heap will take
            // about 10 seconds to collect on a dual CPU box.  The worst case scenario on x64 would make 
            // you want to return your hardware for a refund.

            long timeSinceInducedGC = DateTime.UtcNow.Subtract(_inducedGCFinishTime).Ticks;
            bool infrequent = (timeSinceInducedGC > _inducedGCMinInterval);

            // if we haven't collected recently, or if the trim percent is low (less than 50%), 
            // we need to collect again
            if (infrequent || _lastTrimPercent < 50) {

                // if we're inducing GC too frequently, increase the trim percentage, but don't go above 50%
                if (!infrequent) {
                    _lastTrimPercent = Math.Min(50, _lastTrimPercent + 10);
                }
                // if we're inducing GC infrequently, we may want to decrease the trim percentage
                else if (_lastTrimPercent > 10 && timeSinceInducedGC > 2 * _inducedGCMinInterval) {
                    _lastTrimPercent = Math.Max(10, _lastTrimPercent - 10);
                }
                int percent = (_totalCacheSize > 0) ? _lastTrimPercent : 0;
                long trimmedOrExpired = 0;
                if (percent > 0) {
                    Stopwatch sw1 = Stopwatch.StartNew();
                    trimmedOrExpired = _appManager.TrimCaches(percent);
                    sw1.Stop();
                    _trimDurationTicks = sw1.Elapsed.Ticks;
                }

                // 

                if (trimmedOrExpired == 0 || _appManager.ShutdownInProgress) {
                    return;
                }

                // collect and record statistics
                Stopwatch sw2 = Stopwatch.StartNew();
                GC.Collect();
                sw2.Stop();

                _inducedGCCount++; // only used for debugging
                _inducedGCFinishTime = DateTime.UtcNow;
                _inducedGCDurationTicks = sw2.Elapsed.Ticks;
                _inducedGCPostPrivateBytes = NextSample();
                _inducedGCPrivateBytesChange = privateBytes - _inducedGCPostPrivateBytes;
                // target 3.3% Time in GC, but don't induce a GC more than once every 5 seconds
                // Notes on calculation below:  If G is duration of garbage collection and T is duration 
                // between starting the next collection, then G/T is % Time in GC.  If we target 3.3%,
                // then G/T = 3.3% = 33/1000, so T = G * 1000/33.                
                _inducedGCMinInterval = Math.Max(_inducedGCDurationTicks * 1000 / 33, 5 * TimeSpan.TicksPerSecond);
                // no more frequently than every 60 seconds if change is less than 1%
                if (_inducedGCPrivateBytesChange * 100 <= privateBytes) {
                    _inducedGCMinInterval = Math.Max(_inducedGCMinInterval, 60 * TimeSpan.TicksPerSecond);
                }
#if DBG
                Debug.Trace("CacheMemory", "GC.COLLECT STATS "
                            + "TrimCaches(" + percent + ")"
                            + ", trimDurationSeconds=" + (_trimDurationTicks/TimeSpan.TicksPerSecond)
                            + ", trimmedOrExpired=" + trimmedOrExpired
                            + ", #secondsSinceInducedGC=" + (timeSinceInducedGC/TimeSpan.TicksPerSecond)
                            + ", InducedGCCount=" + _inducedGCCount
                            + ", gcDurationSeconds=" + (_inducedGCDurationTicks/TimeSpan.TicksPerSecond)
                            + ", PrePrivateBytes=" + privateBytes
                            + ", PostPrivateBytes=" + _inducedGCPostPrivateBytes
                            + ", PrivateBytesChange=" + _inducedGCPrivateBytesChange
                            + ", gcMinIntervalSeconds=" + (_inducedGCMinInterval/TimeSpan.TicksPerSecond));
#endif

#if PERF
                SafeNativeMethods.OutputDebugString("  ** COLLECT **: "
                            + percent + "%, "
                            + (_trimDurationTicks/TimeSpan.TicksPerSecond) + " seconds"
                            + ", infrequent=" + infrequent
                            + ", removed=" + trimmedOrExpired
                            + ", sinceIGC=" + (timeSinceInducedGC/TimeSpan.TicksPerSecond)
                            + ", IGCCount=" + _inducedGCCount
                            + ", IGCDuration=" + (_inducedGCDurationTicks/TimeSpan.TicksPerSecond)
                            + ", preBytes=" + privateBytes
                            + ", postBytes=" + _inducedGCPostPrivateBytes
                            + ", byteChange=" + _inducedGCPrivateBytesChange
                            + ", IGCMinInterval=" + (_inducedGCMinInterval/TimeSpan.TicksPerSecond) + "\n");
#endif

            }
        }

        internal long GetUpdatedTotalCacheSize(long sizeUpdate) {
            if (sizeUpdate != 0) {
                long totalSize = Interlocked.Add(ref _totalCacheSize, sizeUpdate);
#if PERF
                SafeNativeMethods.OutputDebugString("CacheManager.GetUpdatedTotalCacheSize:"
                                                    + " _totalCacheSize= " + totalSize
                                                    + ", sizeUpdate=" + sizeUpdate + "\n");
#endif

                return totalSize;
            }
            else {
                return _totalCacheSize;
            }
        }

        public void Dispose() {
            _disposed = true;
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                // managed and unmanaged resources can be touched/released
                DisposeTimer();
            }
            else {
                // the finalizer is calling, so don't touch managed state
            }
        }

        private void DisposeTimer() {
            lock (_timerLock) {
                if (_timer != null) {
                    _timer.Dispose();
                    _timer = null;
                }
            }
        }

        private void PBytesMonitorThread(object state) {
            // callbacks are queued and can unleash all at once, so concurrent invocations must be prevented
            if (Interlocked.Exchange(ref _inPBytesMonitorThread, 1) != 0)
                return;

            try {
                if (_disposed) {
                    return;
                }

#if DBG
                Debug.Trace("CacheMemory", "\r\n\r\n***BEG** PBytesMonitorThread " + DateTime.Now.ToString("T", CultureInfo.InvariantCulture));
#endif
                // get another sample
                long privateBytes = NextSample();
                
                // adjust frequency of timer and pressure marks after the sample is captured
                Adjust();

                if (privateBytes > _highPressureMark) {
                    // induce a GC if necessary
                    CollectInfrequently(privateBytes);
                }

#if DBG
                Debug.Trace("CacheMemory", "**END** PBytesMonitorThread " 
                            + "privateBytes=" + privateBytes
                            + ", _highPressureMark=" + _highPressureMark);
#endif

            }
            finally {
                Interlocked.Exchange(ref _inPBytesMonitorThread, 0);
            }
        }

        private long NextSample() {
            // not thread-safe, only invoke from timer callback
            Debug.Assert(_inPBytesMonitorThread == 1);

            // NtQuerySystemInformation is a very expensive call. A new function 
            // exists on XP Pro and later versions of the OS and it performs much 
            // better. The name of that function is GetProcessMemoryInfo. For hosting
            // scenarios where a larger number of w3wp.exe instances are running, we 
            // want to use the new API (VSWhidbey 417366).
            long privateBytes;
            if (_useGetProcessMemoryInfo) {
                long privatePageCount;
                UnsafeNativeMethods.GetPrivateBytesIIS6(out privatePageCount, true /*nocache*/);
                privateBytes = privatePageCount;
            }
            else {
                uint    dummy;
                uint    privatePageCount = 0;
                // this is a very expensive call
                UnsafeNativeMethods.GetProcessMemoryInformation(_pid, out privatePageCount, out dummy, true /*nocache*/);
                privateBytes = (long)privatePageCount << MEGABYTE_SHIFT;
            }
        
            // increment the index (it's either 1 or 0)
            Debug.Assert(SAMPLE_COUNT == 2);
            _idx = _idx ^ 1;
            // remember the sample time
            _sampleTimes[_idx] = DateTime.UtcNow;
            // remember the sample value
            _samples[_idx] = privateBytes;

#if PERF
            SafeNativeMethods.OutputDebugString(String.Format("CacheManager.NextSample:  privateBytes={0:N}, _highPresureMark={1:N}\n", privateBytes, _highPressureMark));
#endif

            return privateBytes;
        }
    }
}
