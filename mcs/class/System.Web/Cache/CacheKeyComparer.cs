using System.Collections;

namespace System.Web.Caching
{
  internal sealed class CacheKeyComparer : IEqualityComparer
  {
    private static CacheKeyComparer s_comparerInstance;

    internal static CacheKeyComparer GetInstance()
    {
      if (CacheKeyComparer.s_comparerInstance == null)
        CacheKeyComparer.s_comparerInstance = new CacheKeyComparer();
      return CacheKeyComparer.s_comparerInstance;
    }

    private CacheKeyComparer()
    {
    }

    bool IEqualityComparer.Equals(object x, object y)
    {
      return this.Compare(x, y) == 0;
    }

    private int Compare(object x, object y)
    {
      CacheKey cacheKey1 = (CacheKey) x;
      CacheKey cacheKey2 = (CacheKey) y;
      if (cacheKey1.IsPublic)
      {
        if (cacheKey2.IsPublic)
          return string.Compare(cacheKey1.Key, cacheKey2.Key, StringComparison.Ordinal);
        return 1;
      }
      if (!cacheKey2.IsPublic)
        return string.Compare(cacheKey1.Key, cacheKey2.Key, StringComparison.Ordinal);
      return -1;
    }

    int IEqualityComparer.GetHashCode(object obj)
    {
      return ((CacheKey) obj).GetHashCode();
    }
  }
}
