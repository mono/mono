// 
// System.Web.Caching
//
// Author:
//   Patrik Torstensson
//   Daniel Cazzulino (dcazzulino@users.sf.net)
//

using System;
using System.Collections;
using System.Threading;

namespace System.Web.Caching {
	/// <summary>
	/// Class responsible for handling time based flushing of entries in the cache. The class creates
	/// and manages 60 buckets each holding every item that expires that minute. The bucket calculated
	/// for an entry is one minute more than the timeout just to make sure that the item end up in the
	/// bucket where it should be flushed.
	/// </summary>
	internal class CacheExpires : IDisposable {
		static int	_intFlush;
		/// <summary>
		/// 1 bucket == 1 minute == 10M ticks (1 second) * 60
		/// </summary>
		static long _ticksPerBucket = 600000000;
		/// <summary>
		/// 1 cycle == 1 hour
		/// </summary>
		static long _ticksPerCycle = _ticksPerBucket * 60;

		private ExpiresBucket[] _arrBuckets;
		private Timer _objTimer;
		private Cache _objManager;

		private object _lockObj = new object ();

		internal CacheExpires (Cache objManager) {
			_objManager = objManager;
			Initialize();
		}

		private void Initialize () {
			// Create one bucket per minute
			_arrBuckets = new ExpiresBucket [60];

			byte bytePos = 0;
			do {
				_arrBuckets [bytePos] = new ExpiresBucket (bytePos, _objManager);
				bytePos++;
			} while (bytePos < 60);

			// GC Bucket controller
			_intFlush = System.DateTime.Now.Minute - 1;
			_objTimer = new System.Threading.Timer (new System.Threading.TimerCallback (GarbageCleanup), null, 10000, 60000);
		}

		/// <summary>
		/// Adds a Cache entry to the correct flush bucket.
		/// </summary>
		/// <param name="objEntry">Cache entry to add.</param>
		internal void Add (CacheEntry objEntry) {
			long now = DateTime.Now.Ticks;
			if (objEntry.Expires < now)
				objEntry.Expires = now;

			_arrBuckets [GetHashBucket (objEntry.Expires)].Add (objEntry);
		}

		internal void Remove (CacheEntry objEntry) {
			if (objEntry.ExpiresBucket != CacheEntry.NoBucketHash)
				_arrBuckets [objEntry.ExpiresBucket].Remove (objEntry);
		}

		internal void Update (CacheEntry objEntry, long ticksExpires) {
			// If the entry doesn't have a expires time we assume that the entry is due to expire now.
			int oldBucket = objEntry.ExpiresBucket;
			int newBucket = GetHashBucket (ticksExpires);

			if (oldBucket == CacheEntry.NoBucketHash)
				return;

			// Check if we need to move the item
			if (oldBucket != newBucket) {
				_arrBuckets [oldBucket].Remove (objEntry);
				objEntry.Expires = ticksExpires;
				_arrBuckets [newBucket].Add (objEntry);
			} else
				_arrBuckets [oldBucket].Update (objEntry, ticksExpires);
		}

		internal void GarbageCleanup (object State) {
			ExpiresBucket objBucket;
			int bucket;

			// We lock here if FlushExpiredItems take time
			lock (_lockObj) {
				bucket = (++_intFlush) % 60;
			}

			// Flush expired items in the current bucket (defined by _intFlush)
			_arrBuckets [bucket].FlushExpiredItems ();
		}

		private int GetHashBucket (long ticks) {
			// Get bucket to add expire item into, add one minute to the bucket just to make sure that we get it in the bucket gc
			return (int) (((((ticks + 60000) % _ticksPerCycle) / _ticksPerBucket) + 1) % 60);
		}

		/// <summary>
		/// Called by the cache for cleanup.
		/// </summary>
		public void Dispose () {
			// Cleanup the internal timer
			if (_objTimer != null) {
				_objTimer.Dispose();
				_objTimer = null;
			}
		}
	}
}
