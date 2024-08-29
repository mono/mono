namespace System.Web.Caching
{
  public class CacheInsertOptions
  {
    public CacheDependency Dependencies { get; set; }

    public DateTime AbsoluteExpiration { get; set; } = Cache.NoAbsoluteExpiration;

    public TimeSpan SlidingExpiration { get; set; } = Cache.NoSlidingExpiration;

    public CacheItemPriority Priority { get; set; } = CacheItemPriority.Normal;

    public CacheItemRemovedCallback OnRemovedCallback { get; set; }
  }
}