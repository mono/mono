// Decompiled with JetBrains decompiler
// Type: System.Web.Caching.AspNetCache
// Assembly: System.Web, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// MVID: 0C6883DC-A0C9-4219-BD64-501FA87809D2
// Assembly location: C:\Windows\Microsoft.NET\Framework64\v4.0.30319\System.Web.dll

using System.Collections;
using System.Collections.Specialized;
using System.Threading;
using System.Web.Configuration;

namespace System.Web.Caching
{
  internal sealed class AspNetCache : CacheStoreProvider
  {
    private static CacheInsertOptions DefaultInsertOptions = new CacheInsertOptions();
    private bool _isPublic = true;
    internal CacheInternal _cacheInternal;
    private bool _isDisposed;

    public AspNetCache()
    {
      this._cacheInternal = CacheInternal.Create();
      Interlocked.Exchange(ref this._cacheInternal._refCount, 1);
    }

    internal AspNetCache(bool isPublic)
    {
      this._isPublic = isPublic;
      this._cacheInternal = CacheInternal.Create();
      Interlocked.Exchange(ref this._cacheInternal._refCount, 1);
    }

    internal AspNetCache(AspNetCache cache, bool isPublic)
    {
      this._isPublic = isPublic;
      this._cacheInternal = cache._cacheInternal;
      Interlocked.Increment(ref this._cacheInternal._refCount);
    }

    public override long ItemCount
    {
      get
      {
        if (this._isPublic)
          return (long) this._cacheInternal.PublicCount;
        return this._cacheInternal.TotalCount - (long) this._cacheInternal.PublicCount;
      }
    }

    public override long SizeInBytes
    {
      get
      {
        return this._cacheInternal.ApproximateSize;
      }
    }

    public override void Initialize(string name, NameValueCollection config)
    {
      bool result = this._isPublic;
      if (bool.TryParse(config["isPublic"], out result))
        this._isPublic = result;
      this._cacheInternal.ReadCacheInternalConfig(RuntimeConfig.GetAppConfig().Cache);
    }

    public override object Add(string key, object item, CacheInsertOptions options)
    {
      CacheInsertOptions cacheInsertOptions = options ?? AspNetCache.DefaultInsertOptions;
      return this._cacheInternal.DoInsert(this._isPublic, key, item, cacheInsertOptions.Dependencies, cacheInsertOptions.AbsoluteExpiration, cacheInsertOptions.SlidingExpiration, cacheInsertOptions.Priority, cacheInsertOptions.OnRemovedCallback, false);
    }

    public override object Get(string key)
    {
      return this._cacheInternal.DoGet(this._isPublic, key, CacheGetOptions.None);
    }

    public override void Insert(string key, object item, CacheInsertOptions options)
    {
      CacheInsertOptions cacheInsertOptions = options ?? AspNetCache.DefaultInsertOptions;
      this._cacheInternal.DoInsert(this._isPublic, key, item, cacheInsertOptions.Dependencies, cacheInsertOptions.AbsoluteExpiration, cacheInsertOptions.SlidingExpiration, cacheInsertOptions.Priority, cacheInsertOptions.OnRemovedCallback, true);
    }

    public override object Remove(string key)
    {
      return this.Remove(key, CacheItemRemovedReason.Removed);
    }

    public override object Remove(string key, CacheItemRemovedReason reason)
    {
      return this._cacheInternal.Remove(new CacheKey(key, this._isPublic), reason);
    }

    public override long Trim(int percent)
    {
      return this._cacheInternal.TrimCache(percent);
    }

    public override bool AddDependent(string key, CacheDependency dependency, out DateTime utcLastUpdated)
    {
      CacheEntry cacheEntry = (CacheEntry) this._cacheInternal.DoGet(this._isPublic, key, CacheGetOptions.ReturnCacheEntry);
      if (cacheEntry != null)
      {
        utcLastUpdated = cacheEntry.UtcCreated;
        cacheEntry.AddDependent(dependency);
        if (cacheEntry.State == CacheEntry.EntryState.AddedToCache)
          return true;
      }
      utcLastUpdated = DateTime.MinValue;
      return false;
    }

    public override void RemoveDependent(string key, CacheDependency dependency)
    {
      CacheEntry cacheEntry = (CacheEntry) this._cacheInternal.DoGet(this._isPublic, key, CacheGetOptions.ReturnCacheEntry);
      if (cacheEntry == null)
        return;
      cacheEntry.RemoveDependent(dependency);
    }

    public override IDictionaryEnumerator GetEnumerator()
    {
      return this._cacheInternal.CreateEnumerator(!this._isPublic, CacheGetOptions.None);
    }

    public override bool Equals(object obj)
    {
      AspNetCache aspNetCache = obj as AspNetCache;
      if (aspNetCache != null)
        return this._cacheInternal == aspNetCache._cacheInternal;
      return base.Equals(obj);
    }

    public override int GetHashCode()
    {
      return base.GetHashCode();
    }

    public override void Dispose()
    {
      if (this._isDisposed)
        return;
      lock (this)
      {
        if (this._isDisposed)
          return;
        this._isDisposed = true;
        Interlocked.Decrement(ref this._cacheInternal._refCount);
        this._cacheInternal.Dispose();
      }
    }
  }
}
