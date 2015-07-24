// This file implements the classes UsageEntryRef and CacheUsage missing from .NET reference source

namespace System.Runtime.Caching {
	class UsageEntryRef {
		public static UsageEntryRef INVALID = new UsageEntryRef ();

		public bool IsInvalid {
			get { return this == INVALID; }
		}

		// This is used to compare MemoryCacheEntry that have the same UtcLastUpdateUsage.
		public int DateTimeIndex {
			get; set;
		}
	}

	class CacheUsageHelper : ICacheEntryHelper
	{
		public int Compare(MemoryCacheEntry entry1, MemoryCacheEntry entry2)
		{
			var ret = DateTime.Compare (entry1.UtcLastUpdateUsage , entry2.UtcLastUpdateUsage);
			if (ret == 0)
				return entry1.UsageEntryRef.DateTimeIndex - entry2.UsageEntryRef.DateTimeIndex;

			return ret;
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

		public DateTime prevDateTime;
		public int dateTimeIndex;

		public CacheUsage (MemoryCacheStore store)
			: base (store, helper)
		{
		}

		public new void Add (MemoryCacheEntry entry)
		{
			var now = DateTime.UtcNow;
			if (now == prevDateTime)
				dateTimeIndex++;
			else
				dateTimeIndex = 0;

			prevDateTime = now;

			entry.UtcLastUpdateUsage = now;
			entry.UsageEntryRef = new UsageEntryRef ();
			entry.UsageEntryRef.DateTimeIndex = dateTimeIndex;
			base.Add (entry);
		}

		public new void Remove (MemoryCacheEntry entry)
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