//
// System.Web.Caching.Cache
//
// Author(s):
//  Lluis Sanchez (lluis@ximian.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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

namespace System.Web.Caching
{
	// CAS - no InheritanceDemand here as the class is sealed
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public sealed class Cache: IEnumerable
	{
#if NET_2_0
		Dictionary <string, CacheItem> cache;
#else
		Hashtable cache;
#endif
		Cache dependencyCache;
		public static readonly DateTime NoAbsoluteExpiration = DateTime.MaxValue;
		public static readonly TimeSpan NoSlidingExpiration = TimeSpan.Zero;
		
		public Cache ()
		{
#if NET_2_0
			cache = new Dictionary <string, CacheItem> ();
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
		
		public object Add (string key, object value, CacheDependency dependencies, DateTime absoluteExpiration, TimeSpan slidingExpiration, CacheItemPriority priority, CacheItemRemovedCallback onRemoveCallback)
		{
			if (key == null)
				throw new ArgumentNullException ("key");

			lock (cache) {
				CacheItem it;

#if NET_2_0
				cache.TryGetValue (key, out it);
#else
				it = (CacheItem) cache [key];
#endif
				if (it != null)
					return it.Value;
				Insert (key, value, dependencies, absoluteExpiration, slidingExpiration, priority, onRemoveCallback, false);
			}
			return null;
		}
		
		public object Get (string key)
		{
			lock (cache) {
				CacheItem it;
#if NET_2_0
				if (!cache.TryGetValue (key, out it))
					return null;
#else
				it = (CacheItem) cache [key];
				if (it == null)
					return null;
#endif
				
				if (it.Dependency != null && it.Dependency.HasChanged) {
					Remove (it.Key, CacheItemRemovedReason.DependencyChanged, false);
					return null;
				}

				if (it.SlidingExpiration != NoSlidingExpiration) {
					it.AbsoluteExpiration = DateTime.Now + it.SlidingExpiration;
					// Cast to long is ok since we know that sliding expiration
					// is less than 365 days (31536000000ms)
					it.Timer.Change ((long)it.SlidingExpiration.TotalMilliseconds, Timeout.Infinite);
				} else if (DateTime.Now >= it.AbsoluteExpiration) {
					Remove (key, CacheItemRemovedReason.Expired, false);
					return null;
				}

				return it.Value;
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

			try {
				if (doLock)
					Monitor.Enter (cache);
#if NET_2_0
				cache.TryGetValue (key, out ci);
#else
				ci = (CacheItem) cache [key];
#endif

				if (ci != null)
					SetItemTimeout (ci, absoluteExpiration, slidingExpiration, ci.OnRemoveCallback, null, false);
			} finally {
				if (doLock)
					Monitor.Exit (cache);
			}
		}

		void SetItemTimeout (CacheItem ci, DateTime absoluteExpiration, TimeSpan slidingExpiration, CacheItemRemovedCallback onRemoveCallback,
				     string key, bool doLock)
		{
			ci.SlidingExpiration = slidingExpiration;
			if (slidingExpiration != NoSlidingExpiration)
				ci.AbsoluteExpiration = DateTime.Now + slidingExpiration;
			else
				ci.AbsoluteExpiration = absoluteExpiration;			

			ci.OnRemoveCallback = onRemoveCallback;
			
			try {
				if (doLock)
					Monitor.Enter (cache);
				
				if (ci.Timer != null) {
					ci.Timer.Dispose ();
					ci.Timer = null;
				}

				if (key != null)
					cache [key] = ci;
				
				ci.LastChange = DateTime.Now;
				if (ci.AbsoluteExpiration != NoAbsoluteExpiration) {
					long remaining = Math.Max (0, (long)(ci.AbsoluteExpiration - DateTime.Now).TotalMilliseconds);
					if (remaining > 4294967294L)
						// Maximum due time for timer
						// Item will expire properly anyway, as the timer will be
						// rescheduled for the item's expiration time once that item is
						// bubbled to the top of the priority queue.
						remaining = 4294967294L;
					ci.Timer = new Timer (new TimerCallback (ItemExpired), ci, remaining, Timeout.Infinite);
				}
			} finally {
				if (doLock)
					Monitor.Exit (cache);
			}
		}
		
		public object Remove (string key)
		{
			return Remove (key, CacheItemRemovedReason.Removed, true);
		}
		
		object Remove (string key, CacheItemRemovedReason reason, bool doLock)
		{
			CacheItem it = null;

			try {
				if (doLock)
					Monitor.Enter (cache);
#if NET_2_0
				cache.TryGetValue (key, out it);
#else
				it = (CacheItem) cache [key];
#endif
				cache.Remove (key);
			} finally {
				if (doLock)
					Monitor.Exit (cache);
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
				return it.Value;
			} else
				return null;
		}

		// Used when shutting down the application so that
		// session_end events are sent for all sessions.
		internal void InvokePrivateCallbacks ()
		{
			CacheItemRemovedReason reason = CacheItemRemovedReason.Removed;
			lock (cache) {
				foreach (string key in cache.Keys) {
					CacheItem item;
#if NET_2_0
					cache.TryGetValue (key, out item);
#else
					item = (CacheItem) cache [key];
#endif

					if (item != null && item.OnRemoveCallback != null) {
						try {
							item.OnRemoveCallback (key, item.Value, reason);
						} catch {
							//TODO: anything to be done here?
						}
					}
				}
			}
		}

		public IDictionaryEnumerator GetEnumerator ()
		{
			ArrayList list = new ArrayList ();
			lock (cache) {
#if NET_2_0
				foreach (CacheItem it in cache.Values)
					list.Add (it);
#else
				list.AddRange (cache.Values);
#endif
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
		
		void ItemExpired(object cacheItem) {
			CacheItem ci = (CacheItem)cacheItem;
			long remaining = Math.Max (0, (long)(ci.AbsoluteExpiration - DateTime.Now).TotalMilliseconds);
			if (remaining > 4294967294L)
				remaining = 4294967294L;

			if (remaining <= 0) {
				ci.Timer.Dispose();
				ci.Timer = null;

				Remove (ci.Key, CacheItemRemovedReason.Expired, true);
			}

			ci.Timer.Change (remaining, Timeout.Infinite);
		}
		
		internal void CheckDependencies ()
		{
			ArrayList list;
			lock (cache) {
				list = new ArrayList ();
#if NET_2_0
				foreach (CacheItem it in cache.Values)
					list.Add (it);
#else
				list.AddRange (cache.Values);
#endif
			
				foreach (CacheItem it in list) {
					if (it.Dependency != null && it.Dependency.HasChanged)
						Remove (it.Key, CacheItemRemovedReason.DependencyChanged, false);
				}
			}
		}
		
		internal DateTime GetKeyLastChange (string key)
		{
			lock (cache) {
				CacheItem it;
#if NET_2_0
				if (!cache.TryGetValue (key, out it))
					return DateTime.MaxValue;
#else
				it = (CacheItem) cache [key];
#endif
				if (it == null)
					return DateTime.MaxValue;
				
				return it.LastChange;
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

	sealed class CacheItem
	{
		public object Value;
		public string Key;
		public CacheDependency Dependency;
		public DateTime AbsoluteExpiration;
		public TimeSpan SlidingExpiration;
		public CacheItemPriority Priority;
		public CacheItemRemovedCallback OnRemoveCallback;
		public DateTime LastChange;
		public Timer Timer;
	}
		
	sealed class CacheItemEnumerator: IDictionaryEnumerator
	{
		ArrayList list;
		int pos = -1;
		
		public CacheItemEnumerator (ArrayList list)
		{
			this.list = list;
		}
		
		CacheItem Item {
			get {
				if (pos < 0 || pos >= list.Count)
					throw new InvalidOperationException ();
				return (CacheItem) list [pos];
			}
		}
		
		public DictionaryEntry Entry {
			get { return new DictionaryEntry (Item.Key, Item.Value); }
		}
		
		public object Key {
			get { return Item.Key; }
		}
		
		public object Value {
			get { return Item.Value; }
		}
		
		public object Current {
			get { return Entry; }
		}
		
		public bool MoveNext ()
		{
			return (++pos < list.Count);
		}
		
		public void Reset ()
		{
			pos = -1;
		}
	}
}
