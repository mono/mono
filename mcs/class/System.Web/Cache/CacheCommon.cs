using System.Diagnostics;
using System.Threading;
using System.Web.Configuration;
using System.Web.Util;

namespace System.Web.Caching
{
  internal class CacheCommon
  {
    private object _timerLock = new object();
    private int _currentPollInterval = 30000;
    internal const int MEMORYSTATUS_INTERVAL_5_SECONDS = 5000;
    internal const int MEMORYSTATUS_INTERVAL_30_SECONDS = 30000;
    internal CacheInternal _cacheInternal;
    protected internal CacheSizeMonitor _cacheSizeMonitor;
    private DisposableGCHandleRef<Timer> _timerHandleRef;
    internal int _inCacheManagerThread;
    internal bool _enableMemoryCollection;
    internal bool _enableExpiration;
    internal bool _internalConfigRead;
    internal SRefMultiple _srefMultiple;
    private int _disposed;

    internal CacheCommon()
    {
      this._srefMultiple = new SRefMultiple();
      this._cacheSizeMonitor = new CacheSizeMonitor(this._srefMultiple);
      this._enableMemoryCollection = true;
      this._enableExpiration = true;
    }

    internal void Dispose(bool disposing)
    {
      if (!disposing || Interlocked.Exchange(ref this._disposed, 1) != 0)
        return;
      this.EnableCacheMemoryTimer(false);
      this._cacheSizeMonitor.Dispose();
    }

    internal void AddSRefTarget(object o)
    {
      this._srefMultiple.AddSRefTarget(o);
    }

    internal void SetCacheInternal(CacheInternal cacheInternal)
    {
      this._cacheInternal = cacheInternal;
    }

    internal void ReadCacheInternalConfig(CacheSection cacheSection)
    {
      if (this._internalConfigRead)
        return;
      lock (this)
      {
        if (this._internalConfigRead)
          return;
        this._internalConfigRead = true;
        if (cacheSection == null)
          return;
        this._enableMemoryCollection = !cacheSection.DisableMemoryCollection;
        this._enableExpiration = !cacheSection.DisableExpiration;
        this._cacheSizeMonitor.ReadConfig(cacheSection);
        this._currentPollInterval = CacheSizeMonitor.PollInterval;
        this.ResetFromConfigSettings();
      }
    }

    internal void ResetFromConfigSettings()
    {
      this.EnableCacheMemoryTimer(this._enableMemoryCollection);
      this._cacheInternal.EnableExpirationTimer(this._enableExpiration);
    }

    internal void EnableCacheMemoryTimer(bool enable)
    {
      lock (this._timerLock)
      {
        if (enable)
        {
          if (this._timerHandleRef == null)
            this._timerHandleRef = new DisposableGCHandleRef<Timer>(new Timer(new TimerCallback(this.CacheManagerTimerCallback), (object) null, this._currentPollInterval, this._currentPollInterval));
          else
            this._timerHandleRef.Target.Change(this._currentPollInterval, this._currentPollInterval);
        }
        else
        {
          DisposableGCHandleRef<Timer> timerHandleRef = this._timerHandleRef;
          if (timerHandleRef != null)
          {
            if (Interlocked.CompareExchange<DisposableGCHandleRef<Timer>>(ref this._timerHandleRef, (DisposableGCHandleRef<Timer>) null, timerHandleRef) == timerHandleRef)
              timerHandleRef.Dispose();
          }
        }
      }
      if (enable)
        return;
      while (this._inCacheManagerThread != 0)
        Thread.Sleep(100);
    }

    private void AdjustTimer()
    {
      lock (this._timerLock)
      {
        if (this._timerHandleRef == null)
          return;
        if (this._cacheSizeMonitor.IsAboveHighPressure())
        {
          if (this._currentPollInterval <= 5000)
            return;
          this._currentPollInterval = 5000;
          this._timerHandleRef.Target.Change(this._currentPollInterval, this._currentPollInterval);
        }
        else if (this._cacheSizeMonitor.PressureLast > this._cacheSizeMonitor.PressureLow / 2)
        {
          int num = Math.Min(CacheSizeMonitor.PollInterval, 30000);
          if (this._currentPollInterval == num)
            return;
          this._currentPollInterval = num;
          this._timerHandleRef.Target.Change(this._currentPollInterval, this._currentPollInterval);
        }
        else
        {
          if (this._currentPollInterval == CacheSizeMonitor.PollInterval)
            return;
          this._currentPollInterval = CacheSizeMonitor.PollInterval;
          this._timerHandleRef.Target.Change(this._currentPollInterval, this._currentPollInterval);
        }
      }
    }

    private void CacheManagerTimerCallback(object state)
    {
      this.CacheManagerThread(0);
    }

    internal long CacheManagerThread(int minPercent)
    {
      if (Interlocked.Exchange(ref this._inCacheManagerThread, 1) != 0)
        return 0;
      try
      {
        if (this._timerHandleRef == null)
          return 0;
        this._cacheSizeMonitor.Update();
        this.AdjustTimer();
        int percent = Math.Max(minPercent, this._cacheSizeMonitor.GetPercentToTrim());
        long totalCount = this._cacheInternal.TotalCount;
        Stopwatch stopwatch = Stopwatch.StartNew();
        long trimCount = this._cacheInternal.TrimIfNecessary(percent);
        stopwatch.Stop();
        if (percent > 0 && trimCount > 0L)
          this._cacheSizeMonitor.SetTrimStats(stopwatch.Elapsed.Ticks, totalCount, trimCount);
        return trimCount;
      }
      finally
      {
        Interlocked.Exchange(ref this._inCacheManagerThread, 0);
      }
    }
  }
}
