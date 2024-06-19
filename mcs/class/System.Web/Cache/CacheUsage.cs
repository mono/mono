using System.Threading;

namespace System.Web.Caching
{
  internal class CacheUsage
  {
    internal static readonly TimeSpan NEWADD_INTERVAL = new TimeSpan(0, 0, 10);
    internal static readonly TimeSpan CORRELATED_REQUEST_TIMEOUT = new TimeSpan(0, 0, 1);
    internal static readonly TimeSpan MIN_LIFETIME_FOR_USAGE = CacheUsage.NEWADD_INTERVAL;
    private const byte NUMBUCKETS = 5;
    private const int MAX_REMOVE = 1024;
    private readonly CacheSingle _cacheSingle;
    internal readonly UsageBucket[] _buckets;
    private int _inFlush;

    internal CacheUsage(CacheSingle cacheSingle)
    {
      this._cacheSingle = cacheSingle;
      this._buckets = new UsageBucket[5];
      for (byte bucket = 0; (int) bucket < this._buckets.Length; ++bucket)
        this._buckets[(int) bucket] = new UsageBucket(this, bucket);
    }

    internal CacheSingle CacheSingle
    {
      get
      {
        return this._cacheSingle;
      }
    }

    internal void Add(CacheEntry cacheEntry)
    {
      this._buckets[(int) cacheEntry.UsageBucket].AddCacheEntry(cacheEntry);
    }

    internal void Remove(CacheEntry cacheEntry)
    {
      byte usageBucket = cacheEntry.UsageBucket;
      if ((int) usageBucket == (int) byte.MaxValue)
        return;
      this._buckets[(int) usageBucket].RemoveCacheEntry(cacheEntry);
    }

    internal void Update(CacheEntry cacheEntry)
    {
      byte usageBucket = cacheEntry.UsageBucket;
      if ((int) usageBucket == (int) byte.MaxValue)
        return;
      this._buckets[(int) usageBucket].UpdateCacheEntry(cacheEntry);
    }

    internal int FlushUnderUsedItems(int toFlush, ref int publicEntriesFlushed, ref int ocEntriesFlushed)
    {
      int num1 = 0;
      if (Interlocked.Exchange(ref this._inFlush, 1) == 0)
      {
        try
        {
          foreach (UsageBucket bucket in this._buckets)
          {
            int num2 = bucket.FlushUnderUsedItems(toFlush - num1, false, ref publicEntriesFlushed, ref ocEntriesFlushed);
            num1 += num2;
            if (num1 >= toFlush)
              break;
          }
          if (num1 < toFlush)
          {
            foreach (UsageBucket bucket in this._buckets)
            {
              int num2 = bucket.FlushUnderUsedItems(toFlush - num1, true, ref publicEntriesFlushed, ref ocEntriesFlushed);
              num1 += num2;
              if (num1 >= toFlush)
                break;
            }
          }
        }
        finally
        {
          Interlocked.Exchange(ref this._inFlush, 0);
        }
      }
      return num1;
    }
  }
}
