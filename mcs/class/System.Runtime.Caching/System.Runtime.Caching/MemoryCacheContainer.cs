//
// MemoryCacheContainer.cs
//
// Authors:
//      Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2010 Novell, Inc. (http://novell.com/)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Caching.Configuration;
using System.Threading;

namespace System.Runtime.Caching
{
	sealed class MemoryCacheContainer : IDisposable
	{
		const int DEFAULT_LRU_LOWER_BOUND = 10;
		
		ReaderWriterLockSlim cache_lock = new ReaderWriterLockSlim ();
		
		SortedDictionary <string, MemoryCacheEntry> cache;
		MemoryCache owner;
		MemoryCachePerformanceCounters perfCounters;
		MemoryCacheEntryPriorityQueue timedItems;
		MemoryCacheLRU lru;
		Timer expirationTimer;
		
		public int ID {
			get; private set;
		}

		public long Count {
			get { return (long)cache.Count; }
		}
		
		public MemoryCacheContainer (MemoryCache owner, int id, MemoryCachePerformanceCounters perfCounters)
		{
			if (owner == null)
				throw new ArgumentNullException ("owner");
			
			this.owner = owner;
			this.ID = id;
			this.perfCounters = perfCounters;
			cache = new SortedDictionary <string, MemoryCacheEntry> ();
			lru = new MemoryCacheLRU (this, DEFAULT_LRU_LOWER_BOUND);
		}

		bool ExpireIfNeeded (string key, MemoryCacheEntry entry, bool needsLock = true, CacheEntryRemovedReason reason = CacheEntryRemovedReason.Expired)
		{
			bool locked = false;
			
			try {
				if (entry.IsExpired) {
					if (needsLock) {
						cache_lock.EnterWriteLock ();
						locked = true;
					}
					
					cache.Remove (key);
					perfCounters.Decrement (MemoryCachePerformanceCounters.CACHE_ENTRIES);
					entry.Removed (owner, CacheEntryRemovedReason.Expired);
					
					return true;
				}
			} finally {
				if (locked)
					cache_lock.ExitWriteLock ();
			}
			
			return false;
		}

		public void Dispose ()
		{
			if (expirationTimer != null) {
				expirationTimer.Dispose ();
				expirationTimer = null;
			}
		}
		
		public void CopyEntries (IDictionary dict)
		{
			bool locked = false;
			try {
				cache_lock.EnterWriteLock ();
				locked = true;

				MemoryCacheEntry entry;
				foreach (var de in cache) {
					entry = de.Value;

					if (entry.IsExpired)
						continue;

					dict.Add (de.Key, entry.Value);
				}
			} finally {
				if (locked)
					cache_lock.ExitWriteLock ();
			}
		}
		
		public bool ContainsKey (string key)
		{
			bool readLocked = false;
			try {
				cache_lock.EnterUpgradeableReadLock ();
				readLocked = true;

				MemoryCacheEntry entry;
				if (cache.TryGetValue (key, out entry)) {
					if (ExpireIfNeeded (key, entry))
						return false;

					return true;
				}

				return false;
			} finally {
				if (readLocked)
					cache_lock.ExitUpgradeableReadLock ();
			}
		}

		// NOTE: this method _MUST_ be called with the write lock held
		void AddToCache (string key, MemoryCacheEntry entry, bool update = false)
		{
			if (update)
				cache [key] = entry;
			else
				cache.Add (key, entry);
			lru.Update (entry);
			entry.Added ();
			if (!update)
				perfCounters.Increment (MemoryCachePerformanceCounters.CACHE_ENTRIES);
			
			if (entry.IsExpirable)
				UpdateExpirable (entry);
		}

		// NOTE: this method _MUST_ be called with the write lock held
		void UpdateExpirable (MemoryCacheEntry entry)
		{
			if (timedItems == null)
				timedItems = new MemoryCacheEntryPriorityQueue ();

			timedItems.Enqueue (entry);

			if (expirationTimer == null)
				expirationTimer = new Timer (RemoveExpiredItems, null, owner.TimerPeriod, owner.TimerPeriod);
		}

		void RemoveExpiredItems (object state)
		{
			DoRemoveExpiredItems (true);
		}

		long DoRemoveExpiredItems (bool needLock)
		{
			long count = 0;
			bool locked = false;
			try {
				if (needLock) {
					cache_lock.EnterWriteLock ();
					locked = true;
				}

				if (timedItems == null)
					return 0;
				
				long now = DateTime.Now.Ticks;
				MemoryCacheEntry entry = timedItems.Peek ();

				while (entry != null) {
					if (entry.Disabled) {
						timedItems.Dequeue ();
						entry = timedItems.Peek ();
						continue;
					}

					if (entry.ExpiresAt > now)
						break;

					timedItems.Dequeue ();
					count++;
					DoRemoveEntry (entry, true, entry.Key, CacheEntryRemovedReason.Expired);
					entry = timedItems.Peek ();
				}

				if (entry == null) {
					timedItems = null;
					expirationTimer.Dispose ();
					expirationTimer = null;
				}

				return count;
			} finally {
				if (locked)
					cache_lock.ExitWriteLock ();
			}
		}
		
		public object AddOrGetExisting (string key, object value, CacheItemPolicy policy)
		{
			bool readLocked = false, writeLocked = false;
			try {
				cache_lock.EnterUpgradeableReadLock ();
				readLocked = true;

				MemoryCacheEntry entry;
				if (cache.TryGetValue (key, out entry) && entry != null) {
					perfCounters.Increment (MemoryCachePerformanceCounters.CACHE_HITS);
					return entry.Value;
				}
				perfCounters.Increment (MemoryCachePerformanceCounters.CACHE_MISSES);
				
				cache_lock.EnterWriteLock ();
				writeLocked = true;

				if (policy == null)
					entry = new MemoryCacheEntry (owner, key, value);
				else
					entry = new MemoryCacheEntry (owner, key, value,
								      policy.AbsoluteExpiration,
								      policy.ChangeMonitors,
								      policy.Priority,
								      policy.RemovedCallback,
								      policy.SlidingExpiration,
								      policy.UpdateCallback);

				AddToCache (key, entry);
				return null;
			} finally {
				if (writeLocked)
					cache_lock.ExitWriteLock ();
				if (readLocked)
					cache_lock.ExitUpgradeableReadLock ();
			}
		}

		public MemoryCacheEntry GetEntry (string key)
		{
			bool locked = false;
			try {
				cache_lock.EnterReadLock ();
				locked = true;

				MemoryCacheEntry entry;
				if (cache.TryGetValue (key, out entry)) {
					if (ExpireIfNeeded (key, entry))
						return null;
					
					return entry;
				}
				
				return null;
			} finally {
				if (locked)
					cache_lock.ExitReadLock ();
			}
		}
		
		public object Get (string key)
		{
			bool readLocked = false;
			try {
				cache_lock.EnterUpgradeableReadLock ();
				readLocked = true;

				MemoryCacheEntry entry;
				if (cache.TryGetValue (key, out entry)) {
					if (ExpireIfNeeded (key, entry))
						return null;
					
					perfCounters.Increment (MemoryCachePerformanceCounters.CACHE_HITS);
					return entry.Value;
				}

				perfCounters.Increment (MemoryCachePerformanceCounters.CACHE_MISSES);
				return null;
			} finally {
				if (readLocked)
					cache_lock.ExitUpgradeableReadLock ();
			}
		}

		public object Remove (string key, bool needLock = true, bool updateLRU = true)
		{
			bool writeLocked = false, readLocked = false;
			try {
				if (needLock) {
					cache_lock.EnterUpgradeableReadLock ();
					readLocked = true;
				}

				MemoryCacheEntry entry;
				if (!cache.TryGetValue (key, out entry))
					return null;

				if (needLock) {
					cache_lock.EnterWriteLock ();
					writeLocked = true;
				}
				
				object ret = entry.Value;
				DoRemoveEntry (entry, updateLRU, key);
				return ret;
			} finally {
				if (writeLocked)
					cache_lock.ExitWriteLock ();
				if (readLocked)
					cache_lock.ExitUpgradeableReadLock ();
			}
		}
		
		// NOTE: this must be called with the write lock held
		void DoRemoveEntry (MemoryCacheEntry entry, bool updateLRU = true, string key = null, CacheEntryRemovedReason reason = CacheEntryRemovedReason.Removed)
		{
			if (key == null)
				key = entry.Key;

			cache.Remove (key);
			if (updateLRU)
				lru.Remove (entry);
			perfCounters.Decrement (MemoryCachePerformanceCounters.CACHE_ENTRIES);
			entry.Removed (owner, reason);
		}
		
		public void Set (string key, object value, CacheItemPolicy policy)
		{
			bool locked = false;
			try {
				cache_lock.EnterWriteLock ();
				locked = true;

				MemoryCacheEntry mce;
				bool update = false;
				if (cache.TryGetValue (key, out mce)) {
					if (mce != null) {
						perfCounters.Increment (MemoryCachePerformanceCounters.CACHE_HITS);
						mce.Value = value;
						mce.SetPolicy (policy);
						if (mce.IsExpirable)
							UpdateExpirable (mce);
						lru.Update (mce);
						return;
					}

					update = true;
				}
				perfCounters.Increment (MemoryCachePerformanceCounters.CACHE_MISSES);
				if (policy != null)
					mce = new MemoryCacheEntry (owner, key, value,
								    policy.AbsoluteExpiration,
								    policy.ChangeMonitors,
								    policy.Priority,
								    policy.RemovedCallback,
								    policy.SlidingExpiration,
								    policy.UpdateCallback);
				else
					mce = new MemoryCacheEntry (owner, key, value);
				AddToCache (key, mce, update);
			} finally {
				if (locked)
					cache_lock.ExitWriteLock ();
			}
		}

		public long Trim (int percent)
		{
			int count = cache.Count;
			if (count == 0)
				return 0;

			long goal = (long)((count * percent) / 100);
			bool locked = false;
			long ret = 0;

			try {
				cache_lock.EnterWriteLock ();
				locked = true;
				ret = DoRemoveExpiredItems (false);

				goal -= ret;
				if (goal > 0)
					ret += lru.Trim (goal);
			} finally {
				if (locked)
					cache_lock.ExitWriteLock ();
			}
			
			return ret;
		}
	}
}
