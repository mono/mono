using System.Collections;
using System.Collections.Specialized;
using System.Configuration.Provider;

namespace System.Web.Caching
{
  public abstract class CacheStoreProvider : ProviderBase, IDisposable
  {
    public abstract long ItemCount { get; }

    public abstract long SizeInBytes { get; }

    public new abstract void Initialize(string name, NameValueCollection config);

    public abstract object Add(string key, object item, CacheInsertOptions options);

    public abstract object Get(string key);

    public abstract void Insert(string key, object item, CacheInsertOptions options);

    public abstract object Remove(string key);

    public abstract object Remove(string key, CacheItemRemovedReason reason);

    public abstract long Trim(int percent);

    public abstract bool AddDependent(string key, CacheDependency dependency, out DateTime utcLastUpdated);

    public abstract void RemoveDependent(string key, CacheDependency dependency);

    public abstract void Dispose();

    public abstract IDictionaryEnumerator GetEnumerator();
  }
}
