// This file implements the classes UsageEntryRef and CacheUsage missing from .NET reference source

namespace System.Runtime.Caching {
	class UsageEntryRef {
		public static UsageEntryRef INVALID = new UsageEntryRef ();

		public bool IsInvalid {
			get { return this == INVALID; }
		}
	}

	class CacheUsageHelper : ICacheEntryHelper
	{
		public int Compare(MemoryCacheEntry entry1, MemoryCacheEntry entry2)
		{
			return DateTime.Compare (entry1.UtcLastUpdateUsage , entry2.UtcLastUpdateUsage);
		}

		public DateTime GetDateTime (MemoryCacheEntry entry)
		{
			return entry.UtcLastUpdateUsage;
		}
	}

	class CacheUsage : CacheEntryCollection {

		public static TimeSpan CORRELATED_REQUEST_TIMEOUT = new TimeSpan (0, 0, 10);
		public static TimeSpan MIN_LIFETIME_FOR_USAGE = new TimeSpan (0, 0, 10);
		public static CacheUsageHelper helper = new CacheUsageHelper ();

		public CacheUsage (MemoryCacheStore store)
			: base (store, helper)
		{
		}

		public void Add (MemoryCacheEntry entry)
		{
			entry.UtcLastUpdateUsage = DateTime.UtcNow;
			entry.UsageEntryRef = new UsageEntryRef ();
			base.Add (entry);
		}

		public void Remove (MemoryCacheEntry entry)
		{
			base.Remove (entry);
			entry.UsageEntryRef = UsageEntryRef.INVALID;
		}

		public void Update (MemoryCacheEntry entry)
		{
			base.Remove (entry);
			entry.UtcLastUpdateUsage = DateTime.UtcNow;
			base.Add (entry);
		}

		public int FlushUnderUsedItems (int count)
		{
			return base.FlushItems (DateTime.MaxValue, CacheEntryRemovedReason.Evicted, true, count);
		}
	}
}