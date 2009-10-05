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
using System.Security.Permissions;
#if NET_2_0
using System.Collections.Generic;
#endif
using System.Web.Configuration;

namespace System.Web.Caching
{
	// CAS - no InheritanceDemand here as the class is sealed
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public sealed class Cache: IEnumerable
	{
		public static readonly DateTime NoAbsoluteExpiration = DateTime.MaxValue;
		public static readonly TimeSpan NoSlidingExpiration = TimeSpan.Zero;
		
#if NET_2_0 && SYSTEMCORE_DEP
		ReaderWriterLockSlim cacheLock;
#else
		ReaderWriterLock cacheLock;
#endif

#if NET_2_0
		Dictionary <string, CacheItem> cache;
#else
		Hashtable cache;
#endif
		CacheItemPriorityQueue timedItems;
		Timer expirationTimer;
		long expirationTimerPeriod = 0;
		Cache dependencyCache;
#if NET_2_0
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
#else
		bool DisableExpiration {
			get { return false; }
		}		
#endif
		
		public Cache ()
		{
#if NET_2_0 && SYSTEMCORE_DEP
			cacheLock = new ReaderWriterLockSlim ();
#else
			cacheLock = new ReaderWriterLock ();
#endif

#if NET_2_0
			cache = new Dictionary <string, CacheItem> (StringComparer.Ordinal);
#else
			cache = new Hashtable ();
#endif
		}

		public int Count {
			get { return cache.Count; }
		}
		
		public object this [string key] {
			get { return Get (key); }
			set { Insert (key, value); }
		}

		CacheItem GetCacheItem (string key)
		{
			if (key == null)
				return null;
			
			CacheItem ret;
#if NET_2_0
			if (cache.TryGetValue (key, out ret))
				return ret;
#else
			ret = cache [key] as CacheItem;
			if (ret != null)
				return ret;
#endif
			return null;
		}

		CacheItem RemoveCacheItem (string key)
		{
			if (key == null)
				return null;

			CacheItem ret = null;
#if NET_2_0
			if (!cache.TryGetValue (key, out ret))
				return null;
#else
			ret = cache [key] as CacheItem;
			if (ret == null)
				return null;
#endif
			ret.Disabled = true;
			cache.Remove (key);
			
			return ret;
		}
		
		public object Add (string key, object value, CacheDependency dependencies, DateTime absoluteExpiration, TimeSpan slidingExpiration, CacheItemPriority priority, CacheItemRemovedCallback onRemoveCallback)
		{
			if (key == null)
				throw new ArgumentNullException ("key");

			bool locked = false;
			try {
#if NET_2_0 && SYSTEMCORE_DEP
				cacheLock.EnterWriteLock ();
#else
				cacheLock.AcquireWriterLock (-1);
#endif
				locked = true;
				CacheItem it = GetCacheItem (key);

				if (it != null)
					return it.Value;
				Insert (key, value, dependencies, absoluteExpiration, slidingExpiration, priority, onRemoveCallback, false);
			} finally {
				if (locked) {
#if NET_2_0 && SYSTEMCORE_DEP
					cacheLock.ExitWriteLock ();
#else
					cacheLock.ReleaseWriterLock ();
#endif
				}
			}
				
			return null;
		}
		
		public object Get (string key)
		{
			bool locked = false;
			try {
#if NET_2_0 && SYSTEMCORE_DEP
				cacheLock.EnterUpgradeableReadLock ();
#else
				cacheLock.AcquireReaderLock (-1);
#endif
				locked = true;
				CacheItem it = GetCacheItem (key);
				if (it == null)
					return null;
				
				if (it.Dependency != null && it.Dependency.HasChanged) {
#if !NET_2_0
					LockCookie lc = default (LockCookie);
#endif
					try {
#if NET_2_0
						cacheLock.EnterWriteLock ();
#else
						lc = cacheLock.UpgradeToWriterLock (-1);
#endif
						Remove (it.Key, CacheItemRemovedReason.DependencyChanged, false);
					} finally {
#if NET_2_0
						cacheLock.ExitWriteLock ();
#else
						cacheLock.DowngradeFromWriterLock (ref lc);
#endif
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
#if !NET_2_0
						LockCookie lc = default (LockCookie);
#endif
						try {
#if NET_2_0
							cacheLock.EnterWriteLock ();
#else
							lc = cacheLock.UpgradeToWriterLock (-1);
#endif
							Remove (key, CacheItemRemovedReason.Expired, false);
						} finally {
#if NET_2_0
							cacheLock.ExitWriteLock ();
#else
							cacheLock.DowngradeFromWriterLock (ref lc);
#endif
						}

						return null;
					}
				}
				
				return it.Value;
			} finally {
				if (locked) {
#if NET_2_0 && SYSTEMCORE_DEP
					cacheLock.ExitUpgradeableReadLock ();
#else
					cacheLock.ReleaseReaderLock ();
#endif
				}
			}
		}
		
		public void Insert (string key, object value)
		{
			Insert (key, value, null, NoAbsoluteExpiration, NoSlidingExpiration, CacheItemPriority.Normal, null, true);
		}
		
		public void Insert (string key, object value, CacheDependency dependencies)
		{
			Insert (key, value, dependencies, NoAbsoluteExpiration, NoSlidingExpiration, CacheItemPriority.Normal, null, true);
		}
		
		public void Insert (string key, object value, CacheDependency dependencies, DateTime absoluteExpiration, TimeSpan slidingExpiration)
		{
			Insert (key, value, dependencies, absoluteExpiration, slidingExpiration, CacheItemPriority.Normal, null, true);
		}
		
		public void Insert (string key, object value, CacheDependency dependencies, DateTime absoluteExpiration, TimeSpan slidingExpiration,
				    CacheItemPriority priority, CacheItemRemovedCallback onRemoveCallback)
		{
			Insert (key, value, dependencies, absoluteExpiration, slidingExpiration, CacheItemPriority.Normal, onRemoveCallback, true);
		}

		void Insert (string key, object value, CacheDependency dependencies, DateTime absoluteExpiration, TimeSpan slidingExpiration,
			     CacheItemPriority priority, CacheItemRemovedCallback onRemoveCallback, bool doLock)
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
			SetItemTimeout (ci, absoluteExpiration, slidingExpiration, onRemoveCallback, key, doLock);
		}
		
		internal void SetItemTimeout (string key, DateTime absoluteExpiration, TimeSpan slidingExpiration, bool doLock)
		{
			CacheItem ci = null;
			bool locked = false;
			
			try {
				if (doLock) {
#if NET_2_0 && SYSTEMCORE_DEP
					cacheLock.EnterWriteLock ();
#else
					cacheLock.AcquireWriterLock (-1);
#endif
					locked = true;
				}
				
				ci = GetCacheItem (key);
				if (ci != null)
					SetItemTimeout (ci, absoluteExpiration, slidingExpiration, ci.OnRemoveCallback, null, false);
			} finally {
				if (locked) {
#if NET_2_0 && SYSTEMCORE_DEP
					cacheLock.ExitWriteLock ();
#else
					cacheLock.ReleaseWriterLock ();
#endif
				}
			}
		}

		void SetItemTimeout (CacheItem ci, DateTime absoluteExpiration, TimeSpan slidingExpiration, CacheItemRemovedCallback onRemoveCallback,
				     string key, bool doLock)
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
			
			bool locked = false;
			try {
				if (doLock) {
#if NET_2_0 && SYSTEMCORE_DEP
					cacheLock.EnterWriteLock ();
#else
					cacheLock.AcquireWriterLock (-1);
#endif
					locked = true;
				}
				
				if (ci.Timer != null) {
					ci.Timer.Dispose ();
					ci.Timer = null;
				}

				if (key != null)
					cache [key] = ci;
				
				ci.LastChange = DateTime.Now;
				if (!disableExpiration && ci.AbsoluteExpiration != NoAbsoluteExpiration)
					EnqueueTimedItem (ci);
			} finally {
				if (locked) {
#if NET_2_0 && SYSTEMCORE_DEP
					cacheLock.ExitWriteLock ();
#else
					cacheLock.ReleaseWriterLock ();
#endif
				}
			}
		}

		// MUST be called with cache lock held
		void EnqueueTimedItem (CacheItem item)
		{
			long remaining = Math.Max (0, (long)(item.AbsoluteExpiration - DateTime.Now).TotalMilliseconds);
			item.ExpiresAt = item.AbsoluteExpiration.Ticks;
			
			if (timedItems == null)
				timedItems = new CacheItemPriorityQueue ();
			
			if (remaining > 4294967294)
				// Maximum due time for timer
				// Item will expire properly anyway, as the timer will be
				// rescheduled for the item's expiration time once that item is
				// bubbled to the top of the priority queue.
				expirationTimerPeriod = 4294967294;
			else
				expirationTimerPeriod = remaining;
			
			if (expirationTimer == null)
				expirationTimer = new Timer (new TimerCallback (ExpireItems), null, expirationTimerPeriod, expirationTimerPeriod);
			else if (expirationTimerPeriod > remaining)
				expirationTimer.Change (expirationTimerPeriod, expirationTimerPeriod);
			
			timedItems.Enqueue (item);
		}

		public object Remove (string key)
		{
			return Remove (key, CacheItemRemovedReason.Removed, true);
		}
		
		object Remove (string key, CacheItemRemovedReason reason, bool doLock)
		{
			CacheItem it = null;
			bool locked = false;
			try {
				if (doLock) {
#if NET_2_0 && SYSTEMCORE_DEP
					cacheLock.EnterWriteLock ();
#else
					cacheLock.AcquireWriterLock (-1);
#endif
					locked = true;
				}
				
				it = RemoveCacheItem (key);
			} finally {
				if (locked) {
#if NET_2_0 && SYSTEMCORE_DEP
					cacheLock.ExitWriteLock ();
#else
					cacheLock.ReleaseWriterLock ();
#endif
				}
			}

			if (it != null) {
				Timer t = it.Timer;
				if (t != null)
					t.Dispose ();
				
				if (it.Dependency != null) {
#if NET_2_0
					it.Dependency.SetCache (null);
#endif
					it.Dependency.DependencyChanged -= new EventHandler (OnDependencyChanged);
					it.Dependency.Dispose ();
				}
				if (it.OnRemoveCallback != null) {
					try {
						it.OnRemoveCallback (key, it.Value, reason);
					} catch {
						//TODO: anything to be done here?
					}
				}
				object ret = it.Value;
				it.Value = null;
				it.Key = null;
				it.Dependency = null;
				it.OnRemoveCallback = null;

				return ret;
			} else
				return null;
		}

		// Used when shutting down the application so that
		// session_end events are sent for all sessions.
		internal void InvokePrivateCallbacks ()
		{
			CacheItemRemovedReason reason = CacheItemRemovedReason.Removed;
			bool locked = false;
			try {
#if NET_2_0 && SYSTEMCORE_DEP
				cacheLock.EnterReadLock ();
#else
				cacheLock.AcquireReaderLock (-1);
#endif
				locked = true;
				foreach (string key in cache.Keys) {
					CacheItem item = GetCacheItem (key);
					if (item.Disabled)
						continue;
					
					if (item != null && item.OnRemoveCallback != null) {
						try {
							item.OnRemoveCallback (key, item.Value, reason);
						} catch {
							//TODO: anything to be done here?
						}
					}
				}
			}  finally {
				if (locked) {
#if NET_2_0 && SYSTEMCORE_DEP
					cacheLock.ExitReadLock ();
#else
					cacheLock.ReleaseReaderLock ();
#endif
				}
			}
		}

		public IDictionaryEnumerator GetEnumerator ()
		{
			ArrayList list = new ArrayList ();
			bool locked = false;
			try {
#if NET_2_0 && SYSTEMCORE_DEP
				cacheLock.EnterReadLock ();
#else
				cacheLock.AcquireReaderLock (-1);
#endif
				locked = true;
#if NET_2_0
				foreach (CacheItem it in cache.Values)
					list.Add (it);
#else
				list.AddRange (cache.Values);
#endif
			} finally {
				if (locked) {
#if NET_2_0 && SYSTEMCORE_DEP
					cacheLock.ExitReadLock ();
#else
					cacheLock.ReleaseReaderLock ();
#endif
				}
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

		void ExpireItems (object data)
		{
			DateTime now = DateTime.Now;
			CacheItem item = timedItems.Peek ();

			while (item != null) {
				if (!item.Disabled && item.ExpiresAt > now.Ticks)
					break;
				if (item.Disabled)
					continue;
				
				item = timedItems.Dequeue ();
				Remove (item.Key, CacheItemRemovedReason.Expired, true);
				item = timedItems.Peek ();
			}

			if (item != null) {
				long remaining = Math.Max (0, (long)(item.AbsoluteExpiration - now).TotalMilliseconds);
				if (expirationTimerPeriod > remaining) {
					expirationTimerPeriod = remaining;
					expirationTimer.Change (expirationTimerPeriod, expirationTimerPeriod);
				}
				return;
			}
			
			expirationTimer.Change (Timeout.Infinite, Timeout.Infinite);
			expirationTimerPeriod = 0;
		}
		
		void ItemExpired(object cacheItem) {
			CacheItem ci = (CacheItem)cacheItem;
			ci.Timer.Dispose();
			ci.Timer = null;

			Remove (ci.Key, CacheItemRemovedReason.Expired, true);
		}
		
		internal void CheckDependencies ()
		{
			IList list;
			bool locked = false;
			try {
#if NET_2_0 && SYSTEMCORE_DEP
				cacheLock.EnterWriteLock ();
#else
				cacheLock.AcquireWriterLock (-1);
#endif
				locked = true;
#if NET_2_0
				list = new List <CacheItem> ();
				foreach (CacheItem it in cache.Values)
					list.Add (it);
#else
				list = new ArrayList ();
				((ArrayList)list).AddRange (cache.Values);
#endif
			
				foreach (CacheItem it in list) {
					if (it.Dependency != null && it.Dependency.HasChanged)
						Remove (it.Key, CacheItemRemovedReason.DependencyChanged, false);
				}
			} finally {
				if (locked) {
#if NET_2_0 && SYSTEMCORE_DEP
					cacheLock.ExitWriteLock ();
#else
					cacheLock.ReleaseWriterLock ();
#endif
				}
			}
		}
		
		internal DateTime GetKeyLastChange (string key)
		{
			bool locked = false;
			try {
#if NET_2_0 && SYSTEMCORE_DEP
				cacheLock.EnterReadLock ();
#else
				cacheLock.AcquireReaderLock (-1);
#endif
				locked = true;
				CacheItem it = GetCacheItem (key);

				if (it == null)
					return DateTime.MaxValue;
				
				return it.LastChange;
			} finally {
				if (locked) {
#if NET_2_0 && SYSTEMCORE_DEP
					cacheLock.ExitReadLock ();
#else
					cacheLock.ReleaseReaderLock ();
#endif
				}
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
