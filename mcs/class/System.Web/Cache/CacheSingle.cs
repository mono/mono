using System.Collections;
using System.Threading;
using System.Web.Hosting;

namespace System.Web.Caching
{
  internal sealed class CacheSingle : CacheInternal
  {
    private static readonly TimeSpan INSERT_BLOCK_WAIT = new TimeSpan(0, 0, 10);
    private const int MAX_COUNT = 1073741823;
    private const int MIN_COUNT = 10;
    private Hashtable _entries;
    private CacheExpires _expires;
    private CacheUsage _usage;
    private object _lock;
    private int _disposed;
    private int _totalCount;
    private int _publicCount;
    private ManualResetEvent _insertBlock;
    private bool _useInsertBlock;
    private int _insertBlockCalls;
    private int _iSubCache;
    private CacheMultiple _cacheMultiple;

    internal CacheSingle(CacheCommon cacheCommon, CacheMultiple cacheMultiple, int iSubCache)
      : base(cacheCommon)
    {
      this._cacheMultiple = cacheMultiple;
      this._iSubCache = iSubCache;
      this._entries = new Hashtable((IEqualityComparer) CacheKeyComparer.GetInstance());
      this._expires = new CacheExpires(this);
      this._usage = new CacheUsage(this);
      this._lock = new object();
      this._insertBlock = new ManualResetEvent(true);
      cacheCommon.AddSRefTarget((object) new
      {
        _entries = this._entries,
        _expires = this._expires,
        _usage = this._usage
      });
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && Interlocked.Exchange(ref this._disposed, 1) == 0)
      {
        if (this._expires != null)
          this._expires.EnableExpirationTimer(false);
        CacheEntry[] cacheEntryArray = (CacheEntry[]) null;
        lock (this._lock)
        {
          cacheEntryArray = new CacheEntry[this._entries.Count];
          int num = 0;
          foreach (DictionaryEntry entry in this._entries)
            cacheEntryArray[num++] = (CacheEntry) entry.Value;
        }
        foreach (CacheKey cacheKey in cacheEntryArray)
          this.Remove(cacheKey, CacheItemRemovedReason.Removed);
        this._insertBlock.Set();
        this.ReleaseInsertBlock();
      }
      base.Dispose(disposing);
    }

    private ManualResetEvent UseInsertBlock()
    {
      while (this._disposed != 1)
      {
        int insertBlockCalls = this._insertBlockCalls;
        if (insertBlockCalls < 0)
          return (ManualResetEvent) null;
        if (Interlocked.CompareExchange(ref this._insertBlockCalls, insertBlockCalls + 1, insertBlockCalls) == insertBlockCalls)
          return this._insertBlock;
      }
      return (ManualResetEvent) null;
    }

    private void ReleaseInsertBlock()
    {
      if (Interlocked.Decrement(ref this._insertBlockCalls) >= 0)
        return;
      ManualResetEvent insertBlock = this._insertBlock;
      this._insertBlock = (ManualResetEvent) null;
      insertBlock.Close();
    }

    private void SetInsertBlock()
    {
      ManualResetEvent manualResetEvent = (ManualResetEvent) null;
      try
      {
        manualResetEvent = this.UseInsertBlock();
        if (manualResetEvent == null)
          return;
        manualResetEvent.Set();
      }
      finally
      {
        if (manualResetEvent != null)
          this.ReleaseInsertBlock();
      }
    }

    private void ResetInsertBlock()
    {
      ManualResetEvent manualResetEvent = (ManualResetEvent) null;
      try
      {
        manualResetEvent = this.UseInsertBlock();
        if (manualResetEvent == null)
          return;
        manualResetEvent.Reset();
      }
      finally
      {
        if (manualResetEvent != null)
          this.ReleaseInsertBlock();
      }
    }

    private bool WaitInsertBlock()
    {
      bool flag = false;
      ManualResetEvent manualResetEvent = (ManualResetEvent) null;
      try
      {
        manualResetEvent = this.UseInsertBlock();
        if (manualResetEvent != null)
          flag = manualResetEvent.WaitOne(CacheSingle.INSERT_BLOCK_WAIT, false);
      }
      finally
      {
        if (manualResetEvent != null)
          this.ReleaseInsertBlock();
      }
      return flag;
    }

    internal void BlockInsertIfNeeded()
    {
      if (!this._cacheCommon._cacheSizeMonitor.IsAboveHighPressure())
        return;
      this._useInsertBlock = true;
      this.ResetInsertBlock();
    }

    internal void UnblockInsert()
    {
      if (!this._useInsertBlock)
        return;
      this._useInsertBlock = false;
      this.SetInsertBlock();
    }

    internal override int PublicCount
    {
      get
      {
        return this._publicCount;
      }
    }

    internal override long TotalCount
    {
      get
      {
        return (long) this._totalCount;
      }
    }

    internal override IDictionaryEnumerator CreateEnumerator(bool getPrivateItems = false, CacheGetOptions options = CacheGetOptions.None)
    {
      Hashtable hashtable = new Hashtable(getPrivateItems ? this._totalCount - this._publicCount : this._publicCount);
      DateTime utcNow = DateTime.UtcNow;
      lock (this._lock)
      {
        foreach (DictionaryEntry entry in this._entries)
        {
          CacheEntry cacheEntry = (CacheEntry) entry.Value;
          if (cacheEntry.IsPublic == !getPrivateItems && cacheEntry.State == CacheEntry.EntryState.AddedToCache && (!this._cacheCommon._enableExpiration || utcNow <= cacheEntry.UtcExpires))
          {
            if (options == CacheGetOptions.ReturnCacheEntry)
              hashtable[(object) cacheEntry.Key] = (object) cacheEntry;
            else
              hashtable[(object) cacheEntry.Key] = cacheEntry.Value;
          }
        }
      }
      return hashtable.GetEnumerator();
    }

    internal override CacheEntry UpdateCache(CacheKey cacheKey, CacheEntry newEntry, bool replace, CacheItemRemovedReason removedReason, out object valueOld)
    {
      CacheEntry cacheEntry1 = (CacheEntry) null;
      CacheDependency cacheDependency = (CacheDependency) null;
      bool flag1 = false;
      bool flag2 = false;
      DateTime minValue = DateTime.MinValue;
      CacheEntry.EntryState entryState = CacheEntry.EntryState.NotInCache;
      bool flag3 = false;
      CacheItemRemovedReason reason = CacheItemRemovedReason.Removed;
      valueOld = (object) null;
      bool flag4 = !replace && newEntry == null;
      bool flag5 = !replace && newEntry != null;
      CacheEntry cacheEntry2;
      DateTime utcNow;
      while (true)
      {
        if (flag1)
        {
          this.UpdateCache(cacheKey, (CacheEntry) null, true, CacheItemRemovedReason.Expired, out valueOld);
          flag1 = false;
        }
        cacheEntry2 = (CacheEntry) null;
        utcNow = DateTime.UtcNow;
        if (this._useInsertBlock && newEntry != null && newEntry.HasUsage())
          this.WaitInsertBlock();
        bool lockTaken = false;
        if (!flag4)
          Monitor.Enter(this._lock, ref lockTaken);
        try
        {
          cacheEntry2 = (CacheEntry) this._entries[(object) cacheKey];
          if (cacheEntry2 != null)
          {
            entryState = cacheEntry2.State;
            if (this._cacheCommon._enableExpiration && cacheEntry2.UtcExpires < utcNow)
            {
              if (flag4)
              {
                if (entryState == CacheEntry.EntryState.AddedToCache)
                {
                  flag1 = true;
                  continue;
                }
                cacheEntry2 = (CacheEntry) null;
              }
              else
              {
                replace = true;
                removedReason = CacheItemRemovedReason.Expired;
              }
            }
            else
              flag2 = this._cacheCommon._enableExpiration && cacheEntry2.SlidingExpiration > TimeSpan.Zero;
          }
          if (!flag4)
          {
            if (replace && cacheEntry2 != null)
            {
              if (entryState != CacheEntry.EntryState.AddingToCache)
              {
                cacheEntry1 = cacheEntry2;
                cacheEntry1.State = CacheEntry.EntryState.RemovingFromCache;
                this._entries.Remove((object) cacheEntry1);
              }
              else if (newEntry == null)
                cacheEntry2 = (CacheEntry) null;
            }
            if (newEntry != null)
            {
              bool flag6 = true;
              if (cacheEntry2 != null && cacheEntry1 == null)
              {
                flag6 = false;
                reason = CacheItemRemovedReason.Removed;
              }
              if (flag6)
              {
                cacheDependency = newEntry.Dependency;
                if (cacheDependency != null && cacheDependency.HasChanged)
                {
                  flag6 = false;
                  reason = CacheItemRemovedReason.DependencyChanged;
                }
              }
              if (flag6)
              {
                newEntry.State = CacheEntry.EntryState.AddingToCache;
                this._entries.Add((object) newEntry, (object) newEntry);
                cacheEntry2 = !flag5 ? newEntry : (CacheEntry) null;
                break;
              }
              if (!flag5)
              {
                cacheEntry2 = (CacheEntry) null;
                flag3 = true;
              }
              else
                flag3 = cacheEntry2 == null;
              if (!flag3)
              {
                newEntry = (CacheEntry) null;
                break;
              }
              break;
            }
            break;
          }
          break;
        }
        finally
        {
          if (lockTaken)
            Monitor.Exit(this._lock);
        }
      }
      if (flag4)
      {
        if (cacheEntry2 != null)
        {
          if (flag2)
          {
            DateTime utcNewExpires = utcNow + cacheEntry2.SlidingExpiration;
            if (utcNewExpires - cacheEntry2.UtcExpires >= CacheExpires.MIN_UPDATE_DELTA || utcNewExpires < cacheEntry2.UtcExpires)
              this._expires.UtcUpdate(cacheEntry2, utcNewExpires);
          }
          this.UtcUpdateUsageRecursive(cacheEntry2, utcNow);
        }
        if (cacheKey.IsPublic)
        {
          PerfCounters.IncrementCounter(AppPerfCounter.API_CACHE_RATIO_BASE);
          if (cacheEntry2 != null)
            PerfCounters.IncrementCounter(AppPerfCounter.API_CACHE_HITS);
          else
            PerfCounters.IncrementCounter(AppPerfCounter.API_CACHE_MISSES);
        }
        PerfCounters.IncrementCounter(AppPerfCounter.TOTAL_CACHE_RATIO_BASE);
        if (cacheEntry2 != null)
          PerfCounters.IncrementCounter(AppPerfCounter.TOTAL_CACHE_HITS);
        else
          PerfCounters.IncrementCounter(AppPerfCounter.TOTAL_CACHE_MISSES);
      }
      else
      {
        int num1 = 0;
        int num2 = 0;
        int delta1 = 0;
        int delta2 = 0;
        if (cacheEntry1 != null)
        {
          if (cacheEntry1.InExpires())
            this._expires.Remove(cacheEntry1);
          if (cacheEntry1.InUsage())
            this._usage.Remove(cacheEntry1);
          cacheEntry1.State = CacheEntry.EntryState.RemovedFromCache;
          valueOld = cacheEntry1.Value;
          --num1;
          ++delta1;
          if (cacheEntry1.IsPublic)
          {
            --num2;
            ++delta2;
          }
        }
        if (newEntry != null)
        {
          if (flag3)
          {
            newEntry.State = CacheEntry.EntryState.RemovedFromCache;
            newEntry.Close(reason);
            newEntry = (CacheEntry) null;
          }
          else
          {
            if (this._cacheCommon._enableExpiration && newEntry.HasExpiration())
              this._expires.Add(newEntry);
            if (this._cacheCommon._enableMemoryCollection && newEntry.HasUsage() && (!newEntry.HasExpiration() || newEntry.SlidingExpiration > TimeSpan.Zero || newEntry.UtcExpires - utcNow >= CacheUsage.MIN_LIFETIME_FOR_USAGE))
              this._usage.Add(newEntry);
            newEntry.State = CacheEntry.EntryState.AddedToCache;
            ++num1;
            ++delta1;
            if (newEntry.IsPublic)
            {
              ++num2;
              ++delta2;
            }
          }
        }
        if (cacheEntry1 != null)
          cacheEntry1.Close(removedReason);
        if (newEntry != null)
        {
          newEntry.MonitorDependencyChanges();
          if (cacheDependency != null && cacheDependency.HasChanged)
            this.Remove((CacheKey) newEntry, CacheItemRemovedReason.DependencyChanged);
        }
        switch (num1)
        {
          case -1:
            Interlocked.Decrement(ref this._totalCount);
            PerfCounters.DecrementCounter(AppPerfCounter.TOTAL_CACHE_ENTRIES);
            break;
          case 1:
            Interlocked.Increment(ref this._totalCount);
            PerfCounters.IncrementCounter(AppPerfCounter.TOTAL_CACHE_ENTRIES);
            break;
        }
        switch (num2)
        {
          case -1:
            Interlocked.Decrement(ref this._publicCount);
            PerfCounters.DecrementCounter(AppPerfCounter.API_CACHE_ENTRIES);
            break;
          case 1:
            Interlocked.Increment(ref this._publicCount);
            PerfCounters.IncrementCounter(AppPerfCounter.API_CACHE_ENTRIES);
            break;
        }
        if (delta1 > 0)
          PerfCounters.IncrementCounterEx(AppPerfCounter.TOTAL_CACHE_TURNOVER_RATE, delta1);
        if (delta2 > 0)
          PerfCounters.IncrementCounterEx(AppPerfCounter.API_CACHE_TURNOVER_RATE, delta2);
      }
      return cacheEntry2;
    }

    private void UtcUpdateUsageRecursive(CacheEntry cacheEntry, DateTime utcNow)
    {
      if (cacheEntry == null || !(utcNow - cacheEntry.UtcLastUsageUpdate > CacheUsage.CORRELATED_REQUEST_TIMEOUT) && !(utcNow < cacheEntry.UtcLastUsageUpdate))
        return;
      cacheEntry.UtcLastUsageUpdate = utcNow;
      if (cacheEntry.InUsage())
        (this._cacheMultiple != null ? this._cacheMultiple.GetCacheSingle(cacheEntry.Key.GetHashCode()) : this)._usage.Update(cacheEntry);
      CacheDependency dependency = cacheEntry.Dependency;
      if (dependency == null)
        return;
      dependency.KeepDependenciesAlive();
    }

    internal override long TrimIfNecessary(int percent)
    {
      if (!this._cacheCommon._enableMemoryCollection)
        return 0;
      int num1 = 0;
      if (percent > 0)
        num1 = (int) ((long) this._totalCount * (long) percent / 100L);
      int num2 = this._totalCount - 1073741823;
      if (num1 < num2)
        num1 = num2;
      int num3 = this._totalCount - 10;
      if (num1 > num3)
        num1 = num3;
      if (num1 <= 0 || HostingEnvironment.ShutdownInitiated)
        return 0;
      int ocEntriesFlushed = 0;
      int publicEntriesFlushed = 0;
      int delta = 0;
      int num4 = 0;
      int totalCount = this._totalCount;
      try
      {
        num4 = this._expires.FlushExpiredItems(true);
        if (num4 < num1)
        {
          delta = this._usage.FlushUnderUsedItems(num1 - num4, ref publicEntriesFlushed, ref ocEntriesFlushed);
          num4 += delta;
        }
        if (delta > 0)
        {
          PerfCounters.IncrementCounterEx(AppPerfCounter.CACHE_TOTAL_TRIMS, delta);
          PerfCounters.IncrementCounterEx(AppPerfCounter.CACHE_API_TRIMS, publicEntriesFlushed);
          PerfCounters.IncrementCounterEx(AppPerfCounter.CACHE_OUTPUT_TRIMS, ocEntriesFlushed);
        }
      }
      catch
      {
      }
      return (long) num4;
    }

    internal override void EnableExpirationTimer(bool enable)
    {
      if (this._expires == null)
        return;
      this._expires.EnableExpirationTimer(enable);
    }
  }
}
