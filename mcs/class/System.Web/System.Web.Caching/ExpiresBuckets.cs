// 
// System.Web.Caching
//
// Author:
//   Patrik Torstensson (Patrik.Torstensson@labs2.com)
//
// (C) Copyright Patrik Torstensson, 2001
//
namespace System.Web.Caching
{
	/// <summary>
	/// Responsible for holding a cache entry in the linked list bucket.
	/// </summary>
	public struct ExpiresEntry
	{
		public CacheEntry	_objEntry;
		public long			_ticksExpires;
		public int			_intNext;
	}

	/// <summary>
	/// Holds cache entries that has a expiration in a bucket list.
	/// </summary>
	public class ExpiresBucket
	{
		private static int MIN_ENTRIES = 16;
		
		private byte	_byteID;
		private int		_intSize;
		private int		_intCount;
		private	int		_intNext;

		private Cache	_objManager;

		private ExpiresEntry [] _arrEntries;

		/// <summary>
		/// Constructs a new bucket.
		/// </summary>
		/// <param name="bucket">Current bucket ID.</param>
		/// <param name="objManager">Cache manager reponsible for the item(s) in the expires bucket.</param>
		public ExpiresBucket(byte bucket, Cache objManager) 
		{
			_objManager = objManager;
			Initialize(bucket);
		}

		/// <summary>
		/// Initializes the expires bucket, creates a linked list of MIN_ENTRIES.
		/// </summary>
		/// <param name="bucket">Bucket ID.</param>
		private void Initialize(byte bucket)
		{
			_byteID = bucket;
			_intNext = 0;
			_intCount = 0;

			_arrEntries = new ExpiresEntry[MIN_ENTRIES];
			_intSize = MIN_ENTRIES;

			int intPos = 0;
			do 
			{
				_arrEntries[intPos]._intNext = intPos + 1;
				_arrEntries[intPos]._ticksExpires = System.DateTime.MaxValue.Ticks;
				
				intPos++;
			} while (intPos < _intSize);

			_arrEntries[_intSize - 1]._intNext = -1;
		}

		/// <summary>
		/// Expands the bucket linked array list.
		/// </summary>
		private void Expand() 
		{
			ExpiresEntry [] arrData;
			int intPos = 0;
			int intOldSize;

			lock(this)	
			{
				intOldSize = _intSize;
				_intSize *= 2;

				// Create a new array and copy the old data into the new array
				arrData = new ExpiresEntry[_intSize];
				do 
				{
					arrData[intPos] = _arrEntries[intPos];
					intPos++;
				} while (intPos < intOldSize);

				_intNext = intPos;

				// Initialize the "new" positions.
				do 
				{
					arrData[intPos]._intNext = intPos + 1;
					intPos++;
				} while (intPos < _intSize);

				arrData[_intSize - 1]._intNext = -1;

				_arrEntries = arrData;
			}
		}

		/// <summary>
		/// Adds a cache entry into the expires bucket.
		/// </summary>
		/// <param name="objEntry">Cache Entry object to be added.</param>
		public void Add(CacheEntry objEntry)
		{
			if (_intNext == -1) 
			{
				Expand();
			}
			
			lock(this)	
			{
				_arrEntries[_intNext]._ticksExpires =  objEntry.Expires;
				_arrEntries[_intNext]._objEntry = objEntry;

				_intNext = _arrEntries[_intNext]._intNext;
			
				_intCount++;
			}
		}

		/// <summary>
		/// Removes a cache entry from the expires bucket.
		/// </summary>
		/// <param name="objEntry">Cache entry to be removed.</param>
		public void Remove(CacheEntry objEntry)
		{
			lock(this)
			{
				// Check if this is our bucket
				if (objEntry.ExpiresIndex != _byteID) return;
				if (objEntry.ExpiresIndex == System.Int32.MaxValue) return;
				if (_arrEntries.Length < objEntry.ExpiresIndex) return;

				_intCount--;

				_arrEntries[objEntry.ExpiresIndex]._objEntry.ExpiresBucket = byte.MaxValue;
				_arrEntries[objEntry.ExpiresIndex]._objEntry.ExpiresIndex = int.MaxValue;
				_arrEntries[objEntry.ExpiresIndex]._objEntry = null;
				_intNext = _arrEntries[objEntry.ExpiresIndex]._intNext;
			}			
		}

		/// <summary>
		/// Updates a cache entry in the expires bucket, this is called during a hit of an item if the 
		/// cache item has a sliding expiration. The function is responsible for updating the cache
		/// entry.
		/// </summary>
		/// <param name="objEntry">Cache entry to update.</param>
		/// <param name="ticksExpires">New expiration value for the cache entry.</param>
		public void Update(CacheEntry objEntry, long ticksExpires)
		{
			lock(this)
			{
				// Check if this is our bucket
				if (objEntry.ExpiresIndex != _byteID) return;
				if (objEntry.ExpiresIndex == System.Int32.MaxValue) return;
				if (_arrEntries.Length < objEntry.ExpiresIndex) return;

				_arrEntries[objEntry.ExpiresIndex]._ticksExpires = ticksExpires;
				_arrEntries[objEntry.ExpiresIndex]._objEntry.Expires = ticksExpires;
			}
		}

		/// <summary>
		/// Flushes all cache entries that has expired and removes them from the cache manager.
		/// </summary>
		public void FlushExpiredItems()
		{
			ExpiresEntry objEntry;
			CacheEntry [] arrCacheEntries;
			
			int		intCachePos;
			int		intPos;
			long	ticksNow;
	
			ticksNow = System.DateTime.Now.Ticks;

			intCachePos = 0;

			// Lookup all items that needs to be removed, this is done in a two part
			// operation to minimize the locking time.
			lock (this)
			{
				arrCacheEntries = new CacheEntry[_intSize];

				intPos = 0;
				do 
				{
					objEntry = _arrEntries[intPos];
					if (objEntry._objEntry != null)
					{
						if (objEntry._ticksExpires < ticksNow)
						{
							arrCacheEntries[intCachePos++] = objEntry._objEntry;

							objEntry._objEntry.ExpiresBucket = byte.MaxValue;
							objEntry._objEntry.ExpiresIndex = int.MaxValue;
							objEntry._objEntry = null;
							_intNext = objEntry._intNext;
						}
					}
					
					intPos++;
				} while (intPos < _intSize);
			}

			// If we have any entries to remove, go ahead and call the cache manager remove.
			if (intCachePos > 0)
			{
				intPos = 0;
				do 
				{
					_objManager.Remove(arrCacheEntries[intPos].Key, CacheItemRemovedReason.Expired);

					intPos++;
				} while (intPos < intCachePos);
			}
		}

		/// <summary>
		/// Returns the current size of the expires bucket.
		/// </summary>
		public int Size
		{
			get 
			{ 
				return _arrEntries.Length;
			}
		}

		/// <summary>
		/// Returns number of items in the bucket.
		/// </summary>
		public int Count
		{
			get 
			{
				return  _intCount;
			}
		}
	}
}
