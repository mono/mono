// This file implements the classes ExpiresEntryRef and CacheExpires missing from .NET reference source

using System.Threading;

namespace System.Runtime.Caching
{
	class ExpiresEntryRef
	{
		public static ExpiresEntryRef INVALID = new ExpiresEntryRef ();

		public bool IsInvalid {
			get { return this == INVALID; }
		}
	}

	class CacheExpiresHelper : ICacheEntryHelper
	{
		public int Compare(MemoryCacheEntry entry1, MemoryCacheEntry entry2)
		{
			return DateTime.Compare (entry1.UtcAbsExp , entry2.UtcAbsExp);
		}

		public DateTime GetDateTime (MemoryCacheEntry entry)
		{
			return entry.UtcAbsExp;
		}
	}

	class CacheExpires : CacheEntryCollection
	{

		public static TimeSpan MIN_UPDATE_DELTA = new TimeSpan (0, 0, 1);
		public static TimeSpan EXPIRATIONS_INTERVAL = new TimeSpan (0, 0, 20);
		public static CacheExpiresHelper helper = new CacheExpiresHelper ();

		Timer timer;

		public CacheExpires (MemoryCacheStore store)
			: base (store, helper)
		{
		}

		public void Add (MemoryCacheEntry entry)
		{
			entry.ExpiresEntryRef = new ExpiresEntryRef ();
			base.Add (entry);
		}

		public void Remove (MemoryCacheEntry entry)
		{
			base.Remove (entry);
			entry.ExpiresEntryRef = ExpiresEntryRef.INVALID;
		}

		public void UtcUpdate (MemoryCacheEntry entry, DateTime utcAbsExp)
		{
			base.Remove (entry);
			entry.UtcAbsExp = utcAbsExp;
			base.Add (entry);
		}

		public void EnableExpirationTimer (bool enable)
		{
			if (enable) {
				if (timer != null)
					return;

				var period = (int) EXPIRATIONS_INTERVAL.TotalMilliseconds;
				timer = new Timer ((o) => FlushExpiredItems (true), null, period, period);
			} else {
				timer.Dispose ();
				timer = null;
			}
		}

		public int FlushExpiredItems (bool blockInsert)
		{
			return base.FlushItems (DateTime.UtcNow, CacheEntryRemovedReason.Expired, blockInsert);
		}
	}
}