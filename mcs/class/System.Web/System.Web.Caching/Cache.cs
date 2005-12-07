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

namespace System.Web.Caching
{
	// CAS - no InheritanceDemand here as the class is sealed
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public sealed class Cache: IEnumerable
	{
		Hashtable cache;
		Timer timer;
		bool needsTimer;
		
		public static readonly DateTime NoAbsoluteExpiration = DateTime.MaxValue;
		public static readonly TimeSpan NoSlidingExpiration = TimeSpan.Zero;
		
		public Cache ()
		{
			cache = new Hashtable ();
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
				CacheItem it = (CacheItem) cache [key];
				if (it != null)
					return it.Value;
				Insert (key, value, dependencies, absoluteExpiration, slidingExpiration, priority, onRemoveCallback);
			}
			return null;
		}
		
		public object Get (string key)
		{
			lock (cache) {
				CacheItem it = (CacheItem) cache [key];
				if (it == null)
					return null;
				if (it.SlidingExpiration != NoSlidingExpiration)
					it.AbsoluteExpiration = DateTime.Now + it.SlidingExpiration;
				else if (DateTime.Now >= it.AbsoluteExpiration) {
					Remove (key, CacheItemRemovedReason.Expired);
					return null;
				}
				return it.Value;
			}
		}
		
		public void Insert (string key, object value)
		{
			Insert (key, value, null, NoAbsoluteExpiration, NoSlidingExpiration, CacheItemPriority.Normal, null);
		}
		
		public void Insert (string key, object value, CacheDependency dependencies)
		{
			Insert (key, value, dependencies, NoAbsoluteExpiration, NoSlidingExpiration, CacheItemPriority.Normal, null);
		}
		
		public void Insert (string key, object value, CacheDependency dependencies, DateTime absoluteExpiration, TimeSpan slidingExpiration)
		{
			Insert (key, value, dependencies, absoluteExpiration, slidingExpiration, CacheItemPriority.Normal, null);
		}
		
		public void Insert (string key, object value, CacheDependency dependencies, DateTime absoluteExpiration, TimeSpan slidingExpiration, CacheItemPriority priority, CacheItemRemovedCallback onRemoveCallback)
		{
			InsertInternal (key, value, dependencies, absoluteExpiration, slidingExpiration, priority, onRemoveCallback, false);
		}
		
		internal void InsertPrivate (string key, object value, CacheDependency dependencies)
		{
			InsertInternal (key, value, dependencies, NoAbsoluteExpiration, NoSlidingExpiration, CacheItemPriority.Normal, null, true);
		}
		
		internal void InsertPrivate (string key, object value, CacheDependency dependencies, DateTime absoluteExpiration, TimeSpan slidingExpiration, CacheItemPriority priority, CacheItemRemovedCallback onRemoveCallback)
		{
			InsertInternal (key, value, dependencies, absoluteExpiration, slidingExpiration, priority, onRemoveCallback, true);
		}
		
		void InsertInternal (string key, object value, CacheDependency dependency, DateTime absoluteExpiration, TimeSpan slidingExpiration, CacheItemPriority priority, CacheItemRemovedCallback onRemoveCallback, bool isprivate)
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
			ci.Private = isprivate;
			
			if (dependency != null) {
				ci.Dependency = dependency;
				dependency.DependencyChanged += new EventHandler (OnDependencyChanged);
				dependency.SetCache (this);
			}
			ci.SlidingExpiration = slidingExpiration;
			if (slidingExpiration != NoSlidingExpiration)
				ci.AbsoluteExpiration = DateTime.Now + slidingExpiration;
			else
				ci.AbsoluteExpiration = absoluteExpiration;
			
			ci.Priority = priority;
			ci.OnRemoveCallback = onRemoveCallback;
			
			lock (cache) {
				cache [key] = ci;
				ci.LastChange = DateTime.Now;
				if (ci.AbsoluteExpiration != NoAbsoluteExpiration && timer == null) {
					timer = new Timer (new TimerCallback (TimerRun), null, 60000, 60000);
					needsTimer = true;
				}
			}
		}
		
		public object Remove (string key)
		{
			return Remove (key, CacheItemRemovedReason.Removed);
		}
		
		object Remove (string key, CacheItemRemovedReason reason)
		{
			lock (cache) {
				CacheItem it = (CacheItem) cache [key];
				if (it != null) {
					if (it.Dependency != null) {
						it.Dependency.DependencyChanged -= new EventHandler (OnDependencyChanged);
						it.Dependency.Dispose ();
					}
					cache.Remove (key);
					if (it.OnRemoveCallback != null) {
						try {
							it.OnRemoveCallback (key, it.Value, reason);
						} catch {
							//TODO: anything to be done here?
						}
					}
					return it.Value;
				}
				else
					return null;
			}
		}

		// Used when shutting down the application so that
		// session_end events are sent for all sessions.
		internal void InvokePrivateCallbacks ()
		{
			CacheItemRemovedReason reason = CacheItemRemovedReason.Removed;
			lock (cache) {
				foreach (string key in cache.Keys) {
					CacheItem item = (CacheItem) cache [key];
					if (item == null || false == item.Private)
						continue;

					if (item.OnRemoveCallback != null) {
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
				foreach (CacheItem it in cache.Values)
					if (!it.Private)
						list.Add (it);
			}
			return new CacheItemEnumerator (list);
		}
		
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}
		
		void TimerRun (object ob)
		{
			CheckExpiration ();
		}
		
		void OnDependencyChanged (object o, EventArgs a)
		{
			CheckExpiration ();
		}
		
		internal void CheckExpiration ()
		{
			ArrayList list;
			lock (cache) {
				list = new ArrayList ();
				list.AddRange (cache.Values);
				needsTimer = false;
			}
			
			DateTime now = DateTime.Now;
			foreach (CacheItem it in list) {
				if (it.Dependency != null && it.Dependency.HasChanged)
					Remove (it.Key, CacheItemRemovedReason.DependencyChanged);
				else if (it.AbsoluteExpiration != NoAbsoluteExpiration) {
					if (now >= it.AbsoluteExpiration)
						Remove (it.Key, CacheItemRemovedReason.Expired);
					else 
						needsTimer = true;
				}
			}
			
			lock (cache) {
				if (!needsTimer && timer != null) {
					timer.Dispose ();
					timer = null;
				}
			}
		}
		
		internal DateTime GetKeyLastChange (string key)
		{
			lock (cache) {
				CacheItem it = (CacheItem) cache [key];
				if (it == null) return DateTime.MaxValue;
				return it.LastChange;
			}
		}
	}

	class CacheItem
	{
		public object Value;
		public string Key;
		public CacheDependency Dependency;
		public DateTime AbsoluteExpiration;
		public TimeSpan SlidingExpiration;
		public CacheItemPriority Priority;
		public CacheItemRemovedCallback OnRemoveCallback;
		public DateTime LastChange;
		public bool Private;
	}
		
	class CacheItemEnumerator: IDictionaryEnumerator
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
