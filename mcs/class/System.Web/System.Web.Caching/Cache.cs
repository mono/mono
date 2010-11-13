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
		Dictionary <string, CacheItem> cache;		
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
			cacheLock = new ReaderWriterLockSlim ();
			cache = new Dictionary <string, CacheItem> (StringComparer.Ordinal);
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
			if (cache.TryGetValue (key, out ret))
				return ret;
			return null;
		}

		// Must ALWAYS be called with the cache lock held
		CacheItem RemoveCacheItem (string key)
		{
			if (key == null)
				return null;

			CacheItem ret = null;
			if (!cache.TryGetValue (key, out ret))
				return null;
			if (timedItems != null)
				timedItems.OnItemDisable (ret);
			
			ret.Disabled = true;
			cache.Remove (key);
			
			return ret;
		}

		void RemoveOldItemsIfNecessary ()
		{
			if (cache.Count < HIGH_WATER_MARK)
				return;
			
			ThreadPool.QueueUserWorkItem (delegate {
				try {
					DoRemoveOldItemsIfNecessary ();
				} catch (Exception ex) {
					Console.Error.WriteLine ("Exception while attempting to purge old cache items:");
					Console.Error.WriteLine (ex);
				}
			});
		}

		void DoRemoveOldItemsIfNecessary ()
		{
			ICollection <CacheItem> values = null;
			try {
				cacheLock.EnterWriteLock ();
				int count = cache.Count;
				values = cache.Values;

				if (RemoveOldItems (values.Where (item => item.Priority == CacheItemPriority.Low).OrderBy (item => item.LastChange).ToArray <CacheItem> (), ref count))
					return;
				
				if (RemoveOldItems (values.Where (item => item.Priority == CacheItemPriority.Normal).OrderBy (item => item.LastChange).ToArray <CacheItem> (), ref count))
					return;
				
				if (RemoveOldItems (values.Where (item => item.Priority == CacheItemPriority.AboveNormal).OrderBy (item => item.LastChange).ToArray <CacheItem> (), ref count))
					return;
				
				if (RemoveOldItems (values.Where (item => item.Priority == CacheItemPriority.High).OrderBy (item => item.LastChange).ToArray <CacheItem> (), ref count))
					return;
			} finally {
				// See comment at the top of the file, above cacheLock declaration
				cacheLock.ExitWriteLock ();
				values = null;
				GC.Collect ();
			}
		}

		bool RemoveOldItems (CacheItem[] byPriority, ref int count)
		{			
			if (byPriority == null || byPriority.Length == 0)
				return false;

			foreach (CacheItem item in byPriority) {
				if (item.Disabled)
					continue;

				if (count < LOW_WATER_MARK)
					break;

				Remove (item.Key, CacheItemRemovedReason.Underused, false, true);
				count--;
			}
			Array.Clear (byPriority, 0, byPriority.Length);
			byPriority = null;			

			return count < LOW_WATER_MARK;
		}
		
		public object Add (string key, object value, CacheDependency dependencies, DateTime absoluteExpiration, TimeSpan slidingExpiration, CacheItemPriority priority, CacheItemRemovedCallback onRemoveCallback)
		{
			if (key == null)
				throw new ArgumentNullException ("key");
			
			try {
				cacheLock.EnterWriteLock ();
				CacheItem it = GetCacheItem (key);

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
				CacheItem it = GetCacheItem (key);
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
				
				ci = GetCacheItem (key);
				if (ci != null)
					SetItemTimeout (ci, absoluteExpiration, slidingExpiration, ci.OnRemoveCallback, null, key, false);
				else
					RemoveOldItemsIfNecessary ();
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

				if (key != null)
					cache [key] = ci;
				
				ci.LastChange = DateTime.Now;
				if (!disableExpiration && ci.AbsoluteExpiration != NoAbsoluteExpiration) {
					ci.IsTimedItem = true;
					EnqueueTimedItem (ci);
				}
			} finally {
				if (doLock) {
					// See comment at the top of the file, above cacheLock declaration
					cacheLock.ExitWriteLock ();
				}
			}
			RemoveOldItemsIfNecessary ();
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
			else
				expirationTimer.Change (expirationTimerPeriod, expirationTimerPeriod);
			
			timedItems.Enqueue (item);
		}

		public object Remove (string key)
		{
			return Remove (key, CacheItemRemovedReason.Removed, true, true);
		}
		
		object Remove (string key, CacheItemRemovedReason reason, bool doLock, bool invokeCallback)
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

			RemoveOldItemsIfNecessary ();
			return ret;
		}

		// Used when shutting down the application so that
		// session_end events are sent for all sessions.
		internal void InvokePrivateCallbacks ()
		{
			CacheItemRemovedReason reason = CacheItemRemovedReason.Removed;
			try {
				cacheLock.EnterReadLock ();
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
				// See comment at the top of the file, above cacheLock declaration
				cacheLock.ExitReadLock ();
			}
		}

		public IDictionaryEnumerator GetEnumerator ()
		{
			ArrayList list = new ArrayList ();
			try {
				cacheLock.EnterReadLock ();
				foreach (CacheItem it in cache.Values)
					list.Add (it);
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
			CacheItem item = timedItems.Peek ();

			try {
				cacheLock.EnterWriteLock ();

				while (item != null) {
					if (!item.Disabled && item.ExpiresAt > now.Ticks)
						break;
					if (item.Disabled) {
						item = timedItems.Dequeue ();
						continue;
					}

					item = timedItems.Dequeue ();
					if (item != null) {
						if (!NeedsUpdate (item, CacheItemUpdateReason.Expired, false))
							Remove (item.Key, CacheItemRemovedReason.Expired, false, true);
					}
					item = timedItems.Peek ();
				}
			} finally {
				// See comment at the top of the file, above cacheLock declaration
				cacheLock.ExitWriteLock ();
			}

			if (item != null) {
				long remaining = Math.Max (0, (long)(item.AbsoluteExpiration - now).TotalMilliseconds);
				if (expirationTimerPeriod != remaining && remaining > 0) {
					expirationTimerPeriod = remaining;
					expirationTimer.Change (expirationTimerPeriod, expirationTimerPeriod);
				}
				return;
			}

			expirationTimer.Change (Timeout.Infinite, Timeout.Infinite);
			expirationTimerPeriod = 0;
		}
		
		internal void CheckDependencies ()
		{
			IList list;
			try {
				cacheLock.EnterWriteLock ();
				list = new List <CacheItem> ();
				foreach (CacheItem it in cache.Values)
					list.Add (it);
			
				foreach (CacheItem it in list) {
					if (it.Dependency != null && it.Dependency.HasChanged && !NeedsUpdate (it, CacheItemUpdateReason.DependencyChanged, false))
						Remove (it.Key, CacheItemRemovedReason.DependencyChanged, false, true);
				}
			} finally {
				// See comment at the top of the file, above cacheLock declaration
				cacheLock.ExitWriteLock ();
			}
		}
		
		internal DateTime GetKeyLastChange (string key)
		{
			try {
				cacheLock.EnterReadLock ();
				CacheItem it = GetCacheItem (key);

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
