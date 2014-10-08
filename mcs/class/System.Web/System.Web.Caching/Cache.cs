//
// System.Web.Caching.Cache
//
// Author(s):
//  Lluis Sanchez (lluis@ximian.com)
//  Marek Habersack <mhabersack@novell.com>
//
// (C) 2005-2009 Novell, Inc (http://novell.com)
//
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

using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Web.Configuration;

namespace System.Web.Caching
{
	// CAS - no InheritanceDemand here as the class is sealed
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public sealed class Cache: IEnumerable
	{
		const int LOW_WATER_MARK = 10000; // Target number of items if high water mark is reached
		const int HIGH_WATER_MARK = 15000; // We start collection after exceeding this count
		
		public static readonly DateTime NoAbsoluteExpiration = DateTime.MaxValue;
		public static readonly TimeSpan NoSlidingExpiration = TimeSpan.Zero;

		// cacheLock will be released in the code below without checking whether it was
		// actually acquired. The API doesn't offer a reliable way to check whether the lock
		// is being held by the current thread and since Mono does't implement CER
		// (Constrained Execution Regions -
		// http://msdn.microsoft.com/en-us/library/ms228973.aspx) currently, we have no
		// reliable way of recording the information that the lock has been successfully
		// acquired.
		// It can happen that a Thread.Abort occurs while acquiring the lock and the lock
		// isn't  actually held. In this case the attempt to release a lock will throw an
		// exception. It's better than a race of setting a boolean flag  after acquiring the
		// lock and then relying upon it here to release it - that may cause a deadlock
		// should we fail to release the lock  which was successfully acquired but
		// Thread.Abort happened right after that during the stloc instruction to set the
		// boolean flag. Once CERs are supported we can use the boolean flag reliably.
		ReaderWriterLockSlim cacheLock;
		CacheItemLRU cache;
		CacheItemPriorityQueue timedItems;
		Timer expirationTimer;
		long expirationTimerPeriod = 0;
		Cache dependencyCache;
		bool? disableExpiration;
		long privateBytesLimit = -1;
		long percentagePhysicalMemoryLimit = -1;
		
		bool DisableExpiration {
			get {
				if (disableExpiration == null) {
					var cs = WebConfigurationManager.GetWebApplicationSection ("system.web/caching/cache") as CacheSection;
					if (cs == null)
						disableExpiration = false;
					else
						disableExpiration = (bool)cs.DisableExpiration;
				}

				return (bool)disableExpiration;
			}
		}

		public long EffectivePrivateBytesLimit {
			get {
				if (privateBytesLimit == -1) {
					var cs = WebConfigurationManager.GetWebApplicationSection ("system.web/caching/cache") as CacheSection;
					if (cs == null)
						privateBytesLimit = 0;
					else
						privateBytesLimit = cs.PrivateBytesLimit;

					if (privateBytesLimit == 0) {
						// http://blogs.msdn.com/tmarq/archive/2007/06/25/some-history-on-the-asp-net-cache-memory-limits.aspx
						// TODO: calculate
						privateBytesLimit = 734003200;
					}
				}

				return privateBytesLimit;
			}
		}

		public long EffectivePercentagePhysicalMemoryLimit {
			get {
				if (percentagePhysicalMemoryLimit == -1) {
					var cs = WebConfigurationManager.GetWebApplicationSection ("system.web/caching/cache") as CacheSection;
					if (cs == null)
						percentagePhysicalMemoryLimit = 0;
					else
						percentagePhysicalMemoryLimit = cs.PercentagePhysicalMemoryUsedLimit;

					if (percentagePhysicalMemoryLimit == 0) {
						// http://blogs.msdn.com/tmarq/archive/2007/06/25/some-history-on-the-asp-net-cache-memory-limits.aspx
						// TODO: calculate
						percentagePhysicalMemoryLimit = 97;
					}
				}

				return percentagePhysicalMemoryLimit;
			}
		}
		
		public Cache ()
		{
			cacheLock = new ReaderWriterLockSlim (LockRecursionPolicy.SupportsRecursion);
			cache = new CacheItemLRU (this, HIGH_WATER_MARK, LOW_WATER_MARK);
		}

		public int Count {
			get { return cache.Count; }
		}
		
		public object this [string key] {
			get { return Get (key); }
			set { Insert (key, value); }
		}

		// Must ALWAYS be called with the cache write lock held
		CacheItem RemoveCacheItem (string key)
		{
			if (key == null)
				return null;

			CacheItem ret = cache [key];
			if (ret == null)
				return null;
			
			if (timedItems != null)
				timedItems.OnItemDisable (ret);
			
			ret.Disabled = true;
			cache.Remove (key);
			
			return ret;
		}
		
		public object Add (string key, object value, CacheDependency dependencies, DateTime absoluteExpiration, TimeSpan slidingExpiration, CacheItemPriority priority, CacheItemRemovedCallback onRemoveCallback)
		{
			if (key == null)
				throw new ArgumentNullException ("key");
			
			try {
				cacheLock.EnterWriteLock ();
				CacheItem it = cache [key];

				if (it != null)
					return it.Value;
				Insert (key, value, dependencies, absoluteExpiration, slidingExpiration, priority, onRemoveCallback, null, false);
			} finally {
				// See comment at the top of the file, above cacheLock declaration
				cacheLock.ExitWriteLock ();
			}
				
			return null;
		}
		
		public object Get (string key)
		{
			try {
				cacheLock.EnterUpgradeableReadLock ();
				CacheItem it = cache [key];
				if (it == null)
					return null;
				
				if (it.Dependency != null && it.Dependency.HasChanged) {
					try {
						cacheLock.EnterWriteLock ();
						if (!NeedsUpdate (it, CacheItemUpdateReason.DependencyChanged, false))
							Remove (it.Key, CacheItemRemovedReason.DependencyChanged, false, true);
					} finally {
						// See comment at the top of the file, above cacheLock declaration
						cacheLock.ExitWriteLock ();
					}
					
					return null;
				}

				if (!DisableExpiration) {
					if (it.SlidingExpiration != NoSlidingExpiration) {
						it.AbsoluteExpiration = DateTime.Now + it.SlidingExpiration;
						// Cast to long is ok since we know that sliding expiration
						// is less than 365 days (31536000000ms)
						long remaining = (long)it.SlidingExpiration.TotalMilliseconds;
						it.ExpiresAt = it.AbsoluteExpiration.Ticks;
						
						if (expirationTimer != null && (expirationTimerPeriod == 0 || expirationTimerPeriod > remaining)) {
							expirationTimerPeriod = remaining;
							expirationTimer.Change (expirationTimerPeriod, expirationTimerPeriod);
						}
					
					} else if (DateTime.Now >= it.AbsoluteExpiration) {
						try {
							cacheLock.EnterWriteLock ();
							if (!NeedsUpdate (it, CacheItemUpdateReason.Expired, false))
								Remove (key, CacheItemRemovedReason.Expired, false, true);
						} finally {
							// See comment at the top of the file, above cacheLock declaration
							cacheLock.ExitWriteLock ();
						}

						return null;
					}
				}
				
				return it.Value;
			} finally {
				// See comment at the top of the file, above cacheLock declaration
				cacheLock.ExitUpgradeableReadLock ();
			}
		}
		
		public void Insert (string key, object value)
		{
			Insert (key, value, null, NoAbsoluteExpiration, NoSlidingExpiration, CacheItemPriority.Normal, null, null, true);
		}
		
		public void Insert (string key, object value, CacheDependency dependencies)
		{
			Insert (key, value, dependencies, NoAbsoluteExpiration, NoSlidingExpiration, CacheItemPriority.Normal, null, null, true);
		}
		
		public void Insert (string key, object value, CacheDependency dependencies, DateTime absoluteExpiration, TimeSpan slidingExpiration)
		{
			Insert (key, value, dependencies, absoluteExpiration, slidingExpiration, CacheItemPriority.Normal, null, null, true);
		}

		public void Insert (string key, object value, CacheDependency dependencies, DateTime absoluteExpiration, TimeSpan slidingExpiration,
				    CacheItemUpdateCallback onUpdateCallback)
		{
			Insert (key, value, dependencies, absoluteExpiration, slidingExpiration, CacheItemPriority.Normal, null, onUpdateCallback, true);
		}
		
		public void Insert (string key, object value, CacheDependency dependencies, DateTime absoluteExpiration, TimeSpan slidingExpiration,
				    CacheItemPriority priority, CacheItemRemovedCallback onRemoveCallback)
		{
			Insert (key, value, dependencies, absoluteExpiration, slidingExpiration, priority, onRemoveCallback, null, true);
		}

		void Insert (string key, object value, CacheDependency dependencies, DateTime absoluteExpiration, TimeSpan slidingExpiration,
			     CacheItemPriority priority, CacheItemRemovedCallback onRemoveCallback, CacheItemUpdateCallback onUpdateCallback, bool doLock)
		{
			if (key == null)
				throw new ArgumentNullException ("key");
			if (value == null)
				throw new ArgumentNullException ("value");
			if (slidingExpiration < TimeSpan.Zero || slidingExpiration > TimeSpan.FromDays (365))
				throw new ArgumentNullException ("slidingExpiration");
			if (absoluteExpiration != NoAbsoluteExpiration && slidingExpiration != NoSlidingExpiration)
				throw new ArgumentException ("Both absoluteExpiration and slidingExpiration are specified");
				
			CacheItem ci = new CacheItem ();
			ci.Value = value;
			ci.Key = key;
			
			if (dependencies != null) {
				ci.Dependency = dependencies;
				dependencies.DependencyChanged += new EventHandler (OnDependencyChanged);
				dependencies.SetCache (DependencyCache);
			}

			ci.Priority = priority;
			SetItemTimeout (ci, absoluteExpiration, slidingExpiration, onRemoveCallback, onUpdateCallback, key, doLock);
		}
		
		internal void SetItemTimeout (string key, DateTime absoluteExpiration, TimeSpan slidingExpiration, bool doLock)
		{
			CacheItem ci = null;			
			try {
				if (doLock)
					cacheLock.EnterWriteLock ();
				
				ci = cache [key];
				if (ci != null)
					SetItemTimeout (ci, absoluteExpiration, slidingExpiration, ci.OnRemoveCallback, null, key, false);
			} finally {
				if (doLock) {
					// See comment at the top of the file, above cacheLock declaration
					cacheLock.ExitWriteLock ();
				}
			}
		}

		void SetItemTimeout (CacheItem ci, DateTime absoluteExpiration, TimeSpan slidingExpiration, CacheItemRemovedCallback onRemoveCallback,
				     CacheItemUpdateCallback onUpdateCallback, string key, bool doLock)
		{
			bool disableExpiration = DisableExpiration;

			if (!disableExpiration) {
				ci.SlidingExpiration = slidingExpiration;
				if (slidingExpiration != NoSlidingExpiration)
					ci.AbsoluteExpiration = DateTime.Now + slidingExpiration;
				else
					ci.AbsoluteExpiration = absoluteExpiration;			
			}
			
			ci.OnRemoveCallback = onRemoveCallback;
			ci.OnUpdateCallback = onUpdateCallback;
			
			try {
				if (doLock)
					cacheLock.EnterWriteLock ();

				if (key != null) {
					cache [key] = ci;
					cache.EvictIfNecessary ();
				}
				
				ci.LastChange = DateTime.Now;
				if (!disableExpiration && ci.AbsoluteExpiration != NoAbsoluteExpiration) {
					bool enqueue;
					if (ci.IsTimedItem) {
						enqueue = UpdateTimedItem (ci);
						if (!enqueue)
							UpdateTimerPeriod (ci);
					} else
						enqueue = true;

					if (enqueue) {
						ci.IsTimedItem = true;
						EnqueueTimedItem (ci);
					}
					
				}
			} finally {
				if (doLock) {
					// See comment at the top of the file, above cacheLock declaration
					cacheLock.ExitWriteLock ();
				}
			}
		}

		// MUST be called with cache lock held
		bool UpdateTimedItem (CacheItem item)
		{
			if (timedItems == null)
				return true;

			item.ExpiresAt = item.AbsoluteExpiration.Ticks;
			return !timedItems.Update (item);
		}

		// MUST be called with cache lock held
		void UpdateTimerPeriod (CacheItem item)
		{
			if (timedItems == null)
				timedItems = new CacheItemPriorityQueue ();

			long remaining = Math.Max (0, (long)(item.AbsoluteExpiration - DateTime.Now).TotalMilliseconds);
			item.ExpiresAt = item.AbsoluteExpiration.Ticks;
			
			if (remaining > 4294967294)
				// Maximum due time for timer
				// Item will expire properly anyway, as the timer will be
				// rescheduled for the item's expiration time once that item is
				// bubbled to the top of the priority queue.
				remaining = 4294967294;

			if (expirationTimer != null && expirationTimerPeriod <= remaining)
				return;
			expirationTimerPeriod = remaining;

			if (expirationTimer == null)
				expirationTimer = new Timer (new TimerCallback (ExpireItems), null, expirationTimerPeriod, expirationTimerPeriod);
			else
				expirationTimer.Change (expirationTimerPeriod, expirationTimerPeriod);
		}
		
		// MUST be called with cache lock held
		void EnqueueTimedItem (CacheItem item)
		{
			UpdateTimerPeriod (item);
			timedItems.Enqueue (item);
		}

		public object Remove (string key)
		{
			return Remove (key, CacheItemRemovedReason.Removed, true, true);
		}
		
		internal object Remove (string key, CacheItemRemovedReason reason, bool doLock, bool invokeCallback)
		{
			CacheItem it = null;
			try {
				if (doLock)
					cacheLock.EnterWriteLock ();
				
				it = RemoveCacheItem (key);
			} finally {
				if (doLock) {
					// See comment at the top of the file, above cacheLock declaration
					cacheLock.ExitWriteLock ();
				}
			}

			object ret = null;
			if (it != null) {
				if (it.Dependency != null) {
					it.Dependency.SetCache (null);
					it.Dependency.DependencyChanged -= new EventHandler (OnDependencyChanged);
					it.Dependency.Dispose ();
				}
				if (invokeCallback && it.OnRemoveCallback != null) {
					try {
						it.OnRemoveCallback (key, it.Value, reason);
					} catch {
						//TODO: anything to be done here?
					}
				}
				ret = it.Value;
				it.Value = null;
				it.Key = null;
				it.Dependency = null;
				it.OnRemoveCallback = null;
				it.OnUpdateCallback = null;
				it = null;
			}

			return ret;
		}

		// Used when shutting down the application so that
		// session_end events are sent for all sessions.
		internal void InvokePrivateCallbacks ()
		{
			try {
				cacheLock.EnterReadLock ();
				cache.InvokePrivateCallbacks ();
			}  finally {
				// See comment at the top of the file, above cacheLock declaration
				cacheLock.ExitReadLock ();
			}
		}

		public IDictionaryEnumerator GetEnumerator ()
		{
			List <CacheItem> list = null;
			try {
				cacheLock.EnterReadLock ();
				list = cache.ToList ();
			} finally {
				// See comment at the top of the file, above cacheLock declaration
				cacheLock.ExitReadLock ();
			}
			
			return new CacheItemEnumerator (list);
		}
		
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}
		
		void OnDependencyChanged (object o, EventArgs a)
		{
			CheckDependencies ();
		}

		bool NeedsUpdate (CacheItem item, CacheItemUpdateReason reason, bool needLock)
		{
			try {
				if (needLock)
					cacheLock.EnterWriteLock ();
				
				if (item == null || item.OnUpdateCallback == null)
					return false;

				object expensiveObject;
				CacheDependency dependency;
				DateTime absoluteExpiration;
				TimeSpan slidingExpiration;
				string key = item.Key;
				CacheItemUpdateCallback updateCB = item.OnUpdateCallback;
				
				updateCB (key, reason, out expensiveObject, out dependency, out absoluteExpiration, out slidingExpiration);
				if (expensiveObject == null)
					return false;

				CacheItemPriority priority = item.Priority;
				CacheItemRemovedCallback removeCB = item.OnRemoveCallback;
				CacheItemRemovedReason whyRemoved;

				switch (reason) {
					case CacheItemUpdateReason.Expired:
						whyRemoved = CacheItemRemovedReason.Expired;
						break;

					case CacheItemUpdateReason.DependencyChanged:
						whyRemoved = CacheItemRemovedReason.DependencyChanged;
						break;

					default:
						whyRemoved = CacheItemRemovedReason.Removed;
						break;
				}
				
				Remove (key, whyRemoved, false, false);
				Insert (key, expensiveObject, dependency, absoluteExpiration, slidingExpiration, priority, removeCB, updateCB, false);
				
				return true;
			} catch (Exception) {
				return false;
			} finally {
				if (needLock) {
					// See comment at the top of the file, above cacheLock declaration
					cacheLock.ExitWriteLock ();
				}
			}
		}
		
		void ExpireItems (object data)
		{
			DateTime now = DateTime.Now;
			CacheItem item = null;

			expirationTimer.Change (Timeout.Infinite, Timeout.Infinite);
			try {
				cacheLock.EnterWriteLock ();
				while (true) {
					item = timedItems.Peek ();
					
					if (item == null) {
						if (timedItems.Count == 0)
							break;
						
						timedItems.Dequeue ();
						continue;
					}
						
					if (!item.Disabled && item.ExpiresAt > now.Ticks)
						break;
					
					if (item.Disabled) {
						item = timedItems.Dequeue ();
						continue;
					}

					item = timedItems.Dequeue ();
					if (item != null)
						if (!NeedsUpdate (item, CacheItemUpdateReason.Expired, false))
							Remove (item.Key, CacheItemRemovedReason.Expired, false, true);
				}
			} finally {
				// See comment at the top of the file, above cacheLock declaration
				cacheLock.ExitWriteLock ();
			}

			if (item != null) {
				long remaining = Math.Max (0, (long)(item.AbsoluteExpiration - now).TotalMilliseconds);
				if (remaining > 0 && (expirationTimerPeriod == 0 || expirationTimerPeriod > remaining)) {
					expirationTimerPeriod = remaining;
					expirationTimer.Change (expirationTimerPeriod, expirationTimerPeriod);
					return;
				}
				if (expirationTimerPeriod > 0)
					return;
			}

			expirationTimer.Change (Timeout.Infinite, Timeout.Infinite);
			expirationTimerPeriod = 0;
		}
		
		internal void CheckDependencies ()
		{
			try {
				cacheLock.EnterWriteLock ();
				List <CacheItem> list = cache.SelectItems (it => {
					if (it == null)
						return false;
					if (it.Dependency != null && it.Dependency.HasChanged && !NeedsUpdate (it, CacheItemUpdateReason.DependencyChanged, false))
						return true;
					return false;
				});
				
				foreach (CacheItem it in list)
					Remove (it.Key, CacheItemRemovedReason.DependencyChanged, false, true);
				list.Clear ();
				list.TrimExcess ();
			} finally {
				// See comment at the top of the file, above cacheLock declaration
				cacheLock.ExitWriteLock ();
			}
		}
		
		internal DateTime GetKeyLastChange (string key)
		{
			try {
				cacheLock.EnterReadLock ();
				CacheItem it = cache [key];

				if (it == null)
					return DateTime.MaxValue;
				
				return it.LastChange;
			} finally {
				// See comment at the top of the file, above cacheLock declaration
				cacheLock.ExitReadLock ();
			}
		}

		internal Cache DependencyCache {
			get {
				if (dependencyCache == null)
					return this;

				return dependencyCache;
			}
			set { dependencyCache = value; }
		}
	}
}
