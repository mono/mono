using System.Threading;
using System.Web.Configuration;
using System.Web.Hosting;

namespace System.Web.Caching
{
  internal sealed class CacheSizeMonitor
  {
    private DateTime _lastTrimTime = DateTime.MinValue;
    private int _lastTrimGen2Count = -1;
    private const int SAMPLE_COUNT = 2;
    private const int HISTORY_COUNT = 6;
    private const int MEGABYTE_SHIFT = 20;
    private const int KILOBYTE_SHIFT = 10;
    private static uint s_pid;
    private static int s_pollInterval;
    private long[] _cacheSizeSamples;
    private DateTime[] _cacheSizeSampleTimes;
    private int _idx;
    private SRefMultiple _sizedRef;
    private int _gen2Count;
    private long _memoryLimit;
    private int _pressureHigh;
    private int _pressureMiddle;
    private int _pressureLow;
    private int _i0;
    private int[] _pressureHist;
    private int _pressureTotal;
    private int _pressureAvg;
    private long _lastTrimDurationTicks;
    private int _lastTrimPercent;
    private long _totalCountBeforeTrim;
    private long _lastTrimCount;

    internal CacheSizeMonitor(SRefMultiple sizedRef)
    {
      this._sizedRef = sizedRef;
      this._gen2Count = GC.CollectionCount(2);
      this._cacheSizeSamples = new long[2];
      this._cacheSizeSampleTimes = new DateTime[2];
      this._pressureHigh = 99;
      this._pressureMiddle = 98;
      this._pressureLow = 97;
      this.InitHistory();
    }

    internal static int PollInterval
    {
      get
      {
        return CacheSizeMonitor.s_pollInterval;
      }
    }

    internal int PressureLast
    {
      get
      {
        return this._pressureHist[this._i0];
      }
    }

    internal int PressureAvg
    {
      get
      {
        return this._pressureAvg;
      }
    }

    internal int PressureHigh
    {
      get
      {
        return this._pressureHigh;
      }
    }

    internal int PressureLow
    {
      get
      {
        return this._pressureLow;
      }
    }

    internal int PressureMiddle
    {
      get
      {
        return this._pressureMiddle;
      }
    }

    internal bool IsAboveHighPressure()
    {
      return this.PressureLast >= this.PressureHigh;
    }

    internal bool IsAboveMediumPressure()
    {
      return this.PressureLast > this.PressureMiddle;
    }

    private void InitHistory()
    {
      int currentPressure = this.GetCurrentPressure();
      this._pressureHist = new int[6];
      for (int index = 0; index < 6; ++index)
      {
        this._pressureHist[index] = currentPressure;
        this._pressureTotal += currentPressure;
      }
      this._pressureAvg = currentPressure;
    }

    internal void Update()
    {
      int currentPressure = this.GetCurrentPressure();
      this._i0 = (this._i0 + 1) % 6;
      this._pressureTotal -= this._pressureHist[this._i0];
      this._pressureTotal += currentPressure;
      this._pressureHist[this._i0] = currentPressure;
      this._pressureAvg = this._pressureTotal / 6;
    }

    internal void SetTrimStats(long trimDurationTicks, long totalCountBeforeTrim, long trimCount)
    {
      this._lastTrimDurationTicks = trimDurationTicks;
      int num = GC.CollectionCount(2);
      if (num != this._lastTrimGen2Count)
      {
        this._lastTrimTime = DateTime.UtcNow;
        this._totalCountBeforeTrim = totalCountBeforeTrim;
        this._lastTrimCount = trimCount;
      }
      else
        this._lastTrimCount += trimCount;
      this._lastTrimGen2Count = num;
      this._lastTrimPercent = (int) (this._lastTrimCount * 100L / this._totalCountBeforeTrim);
    }

    internal void Dispose()
    {
      SRefMultiple sizedRef = this._sizedRef;
      if (sizedRef == null || Interlocked.CompareExchange<SRefMultiple>(ref this._sizedRef, (SRefMultiple) null, sizedRef) != sizedRef)
        return;
      sizedRef.Dispose();
    }

    internal void ReadConfig(CacheSection cacheSection)
    {
      long privateBytesLimit = cacheSection.PrivateBytesLimit;
      //this._memoryLimit = AspNetMemoryMonitor.ConfiguredProcessMemoryLimit;
      this._memoryLimit = 0;

      //if (privateBytesLimit == 0L && this._memoryLimit == 0L)
      //  this._memoryLimit = AspNetMemoryMonitor.ProcessPrivateBytesLimit;
      //else if (privateBytesLimit != 0L && this._memoryLimit != 0L)
      //  this._memoryLimit = Math.Min(this._memoryLimit, privateBytesLimit);
      //else if (privateBytesLimit != 0L)
      //  this._memoryLimit = privateBytesLimit;
      if (this._memoryLimit > 0L)
      {
        //if ((int) CacheSizeMonitor.s_pid == 0)
        //  CacheSizeMonitor.s_pid = (uint) SafeNativeMethods.GetCurrentProcessId();
        this._pressureHigh = 100;
        this._pressureMiddle = 90;
        this._pressureLow = 80;
      }
      CacheSizeMonitor.s_pollInterval = (int) Math.Min(cacheSection.PrivateBytesPollTime.TotalMilliseconds, (double) int.MaxValue);
      //PerfCounters.SetCounter(AppPerfCounter.CACHE_PERCENT_PROC_MEM_LIMIT_USED_BASE, (int) (this._memoryLimit >> 10));
    }

    private int GetCurrentPressure()
    {
      int num1 = GC.CollectionCount(2);
      SRefMultiple sizedRef = this._sizedRef;
      if (num1 != this._gen2Count && sizedRef != null)
      {
        this._gen2Count = num1;
        this._idx ^= 1;
        this._cacheSizeSampleTimes[this._idx] = DateTime.UtcNow;
        this._cacheSizeSamples[this._idx] = sizedRef.ApproximateSize;
      }
      if (this._memoryLimit <= 0L)
        return 0;
      long num2 = this._cacheSizeSamples[this._idx];
      if (num2 > this._memoryLimit)
        num2 = this._memoryLimit;
      //PerfCounters.SetCounter(AppPerfCounter.CACHE_PERCENT_PROC_MEM_LIMIT_USED, (int) (num2 >> 10));
      return (int) (num2 * 100L / this._memoryLimit);
    }

    internal int GetPercentToTrim()
    {
      int num1 = GC.CollectionCount(2);
      int num2 = 0;
      if (num1 != this._lastTrimGen2Count && this.IsAboveHighPressure())
      {
        long cacheSizeSample = this._cacheSizeSamples[this._idx];
        if (cacheSizeSample > this._memoryLimit)
          num2 = Math.Min(100, (int) ((cacheSizeSample - this._memoryLimit) * 100L / cacheSizeSample));
      }
      return num2;
    }

    internal bool HasLimit()
    {
      return (ulong) this._memoryLimit > 0UL;
    }
  }
}
