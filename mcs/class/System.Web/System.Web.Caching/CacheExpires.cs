// 
// System.Web.Caching
//
// Author:
//   Patrik Torstensson (Patrik.Torstensson@labs2.com)
// Changes:
//   Daniel Cazzulino [DHC] (dcazzulino@users.sf.net)
//
// (C) Copyright Patrik Torstensson, 2001
//
namespace System.Web.Caching
{
	/// <summary>
	/// Class responsible for handling time based flushing of entries in the cache. The class creates
	/// and manages 60 buckets each holding every item that expires that minute. The bucket calculated
	/// for an entry is one minute more than the timeout just to make sure that the item end up in the
	/// bucket where it should be flushed.
	/// </summary>
	internal class CacheExpires : System.IDisposable
	{
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
		private System.Threading.Timer _objTimer;
		private Cache _objManager;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="objManager">The cache manager, used when flushing items in a bucket.</param>
		internal CacheExpires(Cache objManager)
		{
			_objManager = objManager;
			Initialize();
		}

		/// <summary>
		/// Initializes the class.
		/// </summary>
		private void Initialize()
		{
			// Create one bucket per minute
			_arrBuckets = new ExpiresBucket[60];

			byte bytePos = 0;
			do 
			{
				_arrBuckets[bytePos] = new ExpiresBucket(bytePos, _objManager);
				bytePos++;
			} while (bytePos < 60);

			// GC Bucket controller
			_intFlush = System.DateTime.Now.Minute - 1;
			_objTimer = new System.Threading.Timer(new System.Threading.TimerCallback(GarbageCleanup), null, 10000, 60000);
		}

		/// <summary>
		/// Adds a Cache entry to the correct flush bucket.
		/// </summary>
		/// <param name="objEntry">Cache entry to add.</param>
		internal void Add (CacheEntry objEntry)
		{
			lock(this) 
			{
				// If the entry doesn't have a expires time we assume that the entry is due to expire now.
				if (objEntry.Expires == 0) 
				{
					objEntry.Expires = System.DateTime.Now.Ticks;
				}

                _arrBuckets[GetHashBucket(objEntry.Expires)].Add(objEntry);
			}
		}

		internal void Remove(CacheEntry objEntry)
		{
			lock(this) 
			{
				// If the entry doesn't have a expires time we assume that the entry is due to expire now.
				if (objEntry.Expires == 0) 
				{
					objEntry.Expires = System.DateTime.Now.Ticks;
				}

				_arrBuckets[GetHashBucket(objEntry.Expires)].Remove(objEntry);
			}
		}

		internal void Update(CacheEntry objEntry, long ticksExpires)
		{
			lock(this) 
			{
				// If the entry doesn't have a expires time we assume that the entry is due to expire now.
				if (objEntry.Expires == 0) 
				{
					objEntry.Expires = System.DateTime.Now.Ticks;
				}

				_arrBuckets[GetHashBucket(objEntry.Expires)].Update(objEntry, ticksExpires);
			}		
		}

		internal void GarbageCleanup(object State)
		{
			ExpiresBucket	objBucket;

			lock(this)
			{
				// Do cleanup of the bucket 
				objBucket = _arrBuckets[(++_intFlush) % 60];
			}	

			// Flush expired items in the current bucket (defined by _intFlush)
			objBucket.FlushExpiredItems();
		}

		private int GetHashBucket(long ticks)
		{
			// Get bucket to add expire item into, add one minute to the bucket just to make sure that we get it in the bucket gc
			return (int) (((((ticks + 60000) % _ticksPerCycle) / _ticksPerBucket) + 1) % 60);
		}

		/// <summary>
		/// Called by the cache for cleanup.
		/// </summary>
		public void Dispose()
		{
			lock(this)
			{
				// Cleanup the internal timer
				if (_objTimer != null)
				{
					_objTimer.Dispose();
					_objTimer = null;
				}
			}
		}
	}
}
