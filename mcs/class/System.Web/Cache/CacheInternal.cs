using System.Collections;
using System.Web.Configuration;
using System.Web.Util;

namespace System.Web.Caching
{
  internal abstract class CacheInternal : IDisposable
  {
    internal const string PrefixFIRST = "A";
    internal const string PrefixResourceProvider = "A";
    internal const string PrefixMapPathVPPFile = "Bf";
    internal const string PrefixMapPathVPPDir = "Bd";
    internal const string PrefixOutputCache = "a";
    internal const string PrefixSqlCacheDependency = "b";
    internal const string PrefixMemoryBuildResult = "c";
    internal const string PrefixPathData = "d";
    internal const string PrefixHttpCapabilities = "e";
    internal const string PrefixMapPath = "f";
    internal const string PrefixHttpSys = "g";
    internal const string PrefixFileSecurity = "h";
    internal const string PrefixInProcSessionState = "j";
    internal const string PrefixStateApplication = "k";
    internal const string PrefixPartialCachingControl = "l";
    internal const string UNUSED = "m";
    internal const string PrefixAdRotator = "n";
    internal const string PrefixWebServiceDataSource = "o";
    internal const string PrefixLoadXPath = "p";
    internal const string PrefixLoadXml = "q";
    internal const string PrefixLoadTransform = "r";
    internal const string PrefixAspCompatThreading = "s";
    internal const string PrefixDataSourceControl = "u";
    internal const string PrefixValidationSentinel = "w";
    internal const string PrefixWebEventResource = "x";
    internal const string PrefixAssemblyPath = "y";
    internal const string PrefixBrowserCapsHash = "z";
    internal const string PrefixLAST = "z";
    protected CacheCommon _cacheCommon;
    internal int _refCount;
    private int _disposed;

    internal abstract int PublicCount { get; }

    internal abstract long TotalCount { get; }

    internal abstract IDictionaryEnumerator CreateEnumerator(bool getPrivateItems = false, CacheGetOptions options = CacheGetOptions.None);

    internal abstract CacheEntry UpdateCache(CacheKey cacheKey, CacheEntry newEntry, bool replace, CacheItemRemovedReason removedReason, out object valueOld);

    internal abstract long TrimIfNecessary(int percent);

    internal abstract void EnableExpirationTimer(bool enable);

    internal static CacheInternal Create()
    {
      CacheCommon cacheCommon = new CacheCommon();
      uint numProcessCpUs = (uint) SystemInfo.GetNumProcessCPUs();
      int numSingleCaches = 1;
      uint num = numProcessCpUs - 1U;
      while (num > 0U)
      {
        numSingleCaches <<= 1;
        num >>= 1;
      }
      CacheInternal cacheInternal = numSingleCaches != 1 ? (CacheInternal) new CacheMultiple(cacheCommon, numSingleCaches) : (CacheInternal) new CacheSingle(cacheCommon, (CacheMultiple) null, 0);
      cacheCommon.SetCacheInternal(cacheInternal);
      cacheCommon.ResetFromConfigSettings();
      return cacheInternal;
    }

    protected CacheInternal(CacheCommon cacheCommon)
    {
      this._cacheCommon = cacheCommon;
    }

    protected virtual void Dispose(bool disposing)
    {
      this._disposed = 1;
      this._cacheCommon.Dispose(disposing);
    }

    public void Dispose()
    {
      if (this._refCount > 0)
        return;
      this.Dispose(true);
    }

    internal bool IsDisposed
    {
      get
      {
        return this._disposed == 1;
      }
    }

    internal virtual void ReadCacheInternalConfig(CacheSection cacheSection)
    {
      this._cacheCommon.ReadCacheInternalConfig(cacheSection);
    }

    internal virtual long TrimCache(int percent)
    {
      return this._cacheCommon.CacheManagerThread(percent);
    }

    internal long ApproximateSize
    {
      get
      {
        return this._cacheCommon._srefMultiple.ApproximateSize;
      }
    }

    internal bool EnableExpiration
    {
      get
      {
        return this._cacheCommon._enableExpiration;
      }
    }

    internal object this[string key]
    {
      get
      {
        return this.Get(key);
      }
    }

    internal object Get(string key)
    {
      return this.DoGet(false, key, CacheGetOptions.None);
    }

    internal object Get(string key, CacheGetOptions getOptions)
    {
      return this.DoGet(false, key, getOptions);
    }

    internal object DoGet(bool isPublic, string key, CacheGetOptions getOptions)
    {
      object valueOld;
      CacheEntry cacheEntry = this.UpdateCache(new CacheKey(key, isPublic), (CacheEntry) null, false, CacheItemRemovedReason.Removed, out valueOld);
      if (cacheEntry == null)
        return (object) null;
      if ((getOptions & CacheGetOptions.ReturnCacheEntry) != CacheGetOptions.None)
        return (object) cacheEntry;
      return cacheEntry.Value;
    }

    internal object DoInsert(bool isPublic, string key, object value, CacheDependency dependencies, DateTime utcAbsoluteExpiration, TimeSpan slidingExpiration, CacheItemPriority priority, CacheItemRemovedCallback onRemoveCallback, bool replace)
    {
      using (dependencies)
      {
        CacheEntry newEntry = new CacheEntry(key, value, dependencies, onRemoveCallback, utcAbsoluteExpiration, slidingExpiration, priority, isPublic, this);
        object valueOld;
        CacheEntry cacheEntry = this.UpdateCache((CacheKey) newEntry, newEntry, replace, CacheItemRemovedReason.Removed, out valueOld);
        if (cacheEntry != null)
          return cacheEntry.Value;
        return (object) null;
      }
    }

    internal object Remove(string key)
    {
      return this.DoRemove(new CacheKey(key, false), CacheItemRemovedReason.Removed);
    }

    internal object Remove(CacheKey cacheKey, CacheItemRemovedReason reason)
    {
      return this.DoRemove(cacheKey, reason);
    }

    internal object DoRemove(CacheKey cacheKey, CacheItemRemovedReason reason)
    {
      object valueOld;
      this.UpdateCache(cacheKey, (CacheEntry) null, true, reason, out valueOld);
      return valueOld;
    }
  }
}
