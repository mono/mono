using System.Collections;
using System.Threading;
using System.Web.Util;

namespace System.Web.Caching
{
  internal class CacheMultiple : CacheInternal
  {
    private int _disposed;
    private DisposableGCHandleRef<CacheSingle>[] _cachesRefs;
    private int _cacheIndexMask;

    internal CacheMultiple(CacheCommon cacheCommon, int numSingleCaches)
      : base(cacheCommon)
    {
      this._cacheIndexMask = numSingleCaches - 1;
      this._cachesRefs = new DisposableGCHandleRef<CacheSingle>[numSingleCaches];
      for (int iSubCache = 0; iSubCache < numSingleCaches; ++iSubCache)
        this._cachesRefs[iSubCache] = new DisposableGCHandleRef<CacheSingle>(new CacheSingle(cacheCommon, this, iSubCache));
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && Interlocked.Exchange(ref this._disposed, 1) == 0)
      {
        foreach (DisposableGCHandleRef<CacheSingle> cachesRef in this._cachesRefs)
          cachesRef.Target.Dispose();
      }
      base.Dispose(disposing);
    }

    internal override int PublicCount
    {
      get
      {
        int num = 0;
        foreach (DisposableGCHandleRef<CacheSingle> cachesRef in this._cachesRefs)
          num += cachesRef.Target.PublicCount;
        return num;
      }
    }

    internal override long TotalCount
    {
      get
      {
        long num = 0;
        foreach (DisposableGCHandleRef<CacheSingle> cachesRef in this._cachesRefs)
          num += cachesRef.Target.TotalCount;
        return num;
      }
    }

    internal override IDictionaryEnumerator CreateEnumerator(bool getPrivateItems = false, CacheGetOptions options = CacheGetOptions.None)
    {
      IDictionaryEnumerator[] enumerators = new IDictionaryEnumerator[this._cachesRefs.Length];
      int index = 0;
      for (int length = this._cachesRefs.Length; index < length; ++index)
        enumerators[index] = this._cachesRefs[index].Target.CreateEnumerator(getPrivateItems, options);
      return (IDictionaryEnumerator) new AggregateEnumerator(enumerators);
    }

    internal CacheSingle GetCacheSingle(int hashCode)
    {
      if (hashCode < 0)
        hashCode = hashCode == int.MinValue ? 0 : -hashCode;
      return this._cachesRefs[hashCode & this._cacheIndexMask].Target;
    }

    internal override CacheEntry UpdateCache(CacheKey cacheKey, CacheEntry newEntry, bool replace, CacheItemRemovedReason removedReason, out object valueOld)
    {
      return this.GetCacheSingle(cacheKey.Key.GetHashCode()).UpdateCache(cacheKey, newEntry, replace, removedReason, out valueOld);
    }

    internal override long TrimIfNecessary(int percent)
    {
      long num = 0;
      foreach (DisposableGCHandleRef<CacheSingle> cachesRef in this._cachesRefs)
        num += cachesRef.Target.TrimIfNecessary(percent);
      return num;
    }

    internal override void EnableExpirationTimer(bool enable)
    {
      foreach (DisposableGCHandleRef<CacheSingle> cachesRef in this._cachesRefs)
        cachesRef.Target.EnableExpirationTimer(enable);
    }
  }
}
