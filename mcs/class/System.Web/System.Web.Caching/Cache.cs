// 
// System.Web.Caching
//
// Author:
//   Patrik Torstensson
//   Daniel Cazzulino [DHC] (dcazzulino@users.sf.net)
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

using System;
using System.Collections;
using System.Threading;

namespace System.Web.Caching {
	public sealed class Cache : IEnumerable {

		public static readonly DateTime NoAbsoluteExpiration = DateTime.MaxValue;
		public static readonly TimeSpan NoSlidingExpiration = TimeSpan.Zero;

		// Helper objects
		private CacheExpires _objExpires;

		// The data storage 
		private Hashtable _arrEntries;
		private ReaderWriterLock _lockEntries;
		private int _nItems;
		
		static private TimeSpan _datetimeOneYear = TimeSpan.FromDays (365);


		public Cache () {
			_nItems = 0;

			_lockEntries = new ReaderWriterLock ();
			_arrEntries = new Hashtable ();

			_objExpires = new CacheExpires (this);
		}

		/// <summary>
		/// Internal method to create a enumerator and over all public
		/// items in the cache and is used by GetEnumerator method.
		/// </summary>
		/// <returns>
		/// Returns IDictionaryEnumerator that contains all public items in the cache
		/// </returns>
		private IDictionaryEnumerator CreateEnumerator () {
			Hashtable objTable;

			//Locking with -1 provides a non-expiring lock.
			_lockEntries.AcquireReaderLock (-1);
			try {
				// Create a new hashtable to return as collection of public items
				objTable = new Hashtable (_arrEntries.Count);

				foreach (DictionaryEntry objEntry in _arrEntries) {
					if (objEntry.Key == null)
						continue;

					CacheEntry entry = (CacheEntry) objEntry.Value;
					if (entry.IsPublic)
						objTable.Add (objEntry.Key, entry.Item);
				}
			} finally {
				_lockEntries.ReleaseReaderLock ();
			}

			return objTable.GetEnumerator ();
		}


		IEnumerator IEnumerable.GetEnumerator () {
			return GetEnumerator ();
		}

		public IDictionaryEnumerator GetEnumerator () {
			return CreateEnumerator ();
		}

		internal void Touch(string strKey) {
			GetEntry (strKey);
		}

		/// <summary>
		/// Adds the specified item to the Cache object with
		/// dependencies, expiration and priority policies, and a
		/// delegate you can use to notify your application when the
		/// inserted item is removed from the Cache.
		/// </summary>
		/// <param name="strKey">The cache key used to reference the item.</param>
		/// <param name="objItem">The item to be added to the cache.</param>
		/// <param name="objDependency">
		/// The file or cache key dependencies for the item. When any
		/// dependency changes, the object becomes invalid and is removed
		/// from the cache. If there are no dependencies, this paramter
		/// contains a null reference.
		/// </param>
		/// <param name="absolutExpiration">
		/// The time at which the added object expires and is removed from the cache.
		/// </param>
		/// <param name="slidingExpiration">
		/// The interval between the time the added object was last
		/// accessed and when that object expires. If this value is the
		/// equivalent of 20 minutes, the object expires and is removed
		/// from the cache 20 minutes after it is last accessed.
		/// </param>
		/// <param name="enumPriority">
		/// The relative cost of the object, as expressed by the
		/// CacheItemPriority enumeration. The cache uses this value when
		/// it evicts objects; objects with a lower cost are removed from
		/// the cache before objects with a higher cost.
		/// </param>
		/// <param name="eventRemoveCallback">
		/// A delegate that, if provided, is called when an object is
		/// removed from the cache. You can use this to notify
		/// applications when their objects are deleted from the
		/// cache.
		/// </param>
		/// <returns>The Object item added to the Cache.</returns>
		public object Add (string strKey, object objItem, CacheDependency objDependency,
							DateTime absolutExpiration, TimeSpan slidingExpiration, 
							CacheItemPriority enumPriority, CacheItemRemovedCallback eventRemoveCallback) {
			
			return Add (strKey, objItem, objDependency, absolutExpiration,
				slidingExpiration, enumPriority, eventRemoveCallback, true, false);
		}

		private object Add (string strKey,
			object objItem,
			CacheDependency objDependency,
			DateTime absolutExpiration,
			TimeSpan slidingExpiration,
			CacheItemPriority enumPriority,
			CacheItemRemovedCallback eventRemoveCallback,
			bool pub,
			bool overwrite) {

			if (strKey == null)
				throw new ArgumentNullException ("strKey");

			if (objItem == null)
				throw new ArgumentNullException ("objItem");

			if (slidingExpiration > _datetimeOneYear)
				throw new ArgumentOutOfRangeException ("slidingExpiration");

			CacheEntry objEntry;
			CacheEntry objOldEntry = null;

			long longHitRange = 10000;

			// todo: check decay and make up the minHit range

			objEntry = new CacheEntry (this,
										strKey,
										objItem,
										objDependency,
										eventRemoveCallback,
										absolutExpiration,
										slidingExpiration,
										longHitRange,
										pub,
										enumPriority);

			Interlocked.Increment (ref _nItems);

			_lockEntries.AcquireWriterLock (-1);
			try {
				if (_arrEntries.Contains (strKey)) {
					if (overwrite)
						objOldEntry = _arrEntries [strKey] as CacheEntry;
					else
						return null;
				}
				
				objEntry.Hit ();
				_arrEntries [strKey] = objEntry;
			} finally {
				_lockEntries.ReleaseLock ();
			}

			if (objOldEntry != null) {
				if (objOldEntry.HasAbsoluteExpiration || objOldEntry.HasSlidingExpiration)
					_objExpires.Remove (objOldEntry);

				objOldEntry.Close (CacheItemRemovedReason.Removed);
			}

			// If we have any kind of expiration add into the CacheExpires class
			if (objEntry.HasSlidingExpiration || objEntry.HasAbsoluteExpiration) {
				if (objEntry.HasSlidingExpiration)
					objEntry.Expires = DateTime.UtcNow.Ticks + objEntry.SlidingExpiration;

				_objExpires.Add (objEntry);
			}

			return objEntry.Item;
		}
		
		/// <summary>
		/// Inserts an item into the Cache object with a cache key to
		/// reference its location and using default values provided by
		/// the CacheItemPriority enumeration.
		/// </summary>
		/// <param name="strKey">The cache key used to reference the item.</param>
		/// <param name="objItem">The item to be added to the cache.</param>
		public void Insert (string strKey, object objItem) {
			Add (strKey, objItem, null, NoAbsoluteExpiration, NoSlidingExpiration,
				CacheItemPriority.Default, null, true, true);
		}

		/// <summary>
		/// Inserts an object into the Cache that has file or key dependencies.
		/// </summary>
		/// <param name="strKey">The cache key used to reference the item.</param>
		/// <param name="objItem">The item to be added to the cache.</param>
		/// <param name="objDependency">
		/// The file or cache key dependencies for the item. When any
		/// dependency changes, the object becomes invalid and is removed
		/// from the cache. If there are no dependencies, this paramter
		/// contains a null reference.
		/// </param>
		public void Insert (string strKey, object objItem, CacheDependency objDependency) {
			Add (strKey, objItem, objDependency, NoAbsoluteExpiration, NoSlidingExpiration,
				CacheItemPriority.Default, null, true, true);
		}

		/// <summary>
		/// Inserts an object into the Cache with dependencies and expiration policies.
		/// </summary>
		/// <param name="strKey">The cache key used to reference the item.</param>
		/// <param name="objItem">The item to be added to the cache.</param>
		/// <param name="objDependency">
		/// The file or cache key dependencies for the item. When any
		/// dependency changes, the object becomes invalid and is removed
		/// from the cache. If there are no dependencies, this paramter
		/// contains a null reference.
		/// </param>
		/// <param name="absolutExpiration">
		/// The time at which the added object expires and is removed from the cache.
		/// </param>
		/// <param name="slidingExpiration">The interval between the
		/// time the added object was last accessed and when that object
		/// expires. If this value is the equivalent of 20 minutes, the
		/// object expires and is removed from the cache 20 minutes after
		/// it is last accessed.
		/// </param>
		public void Insert (string strKey, object objItem, CacheDependency objDependency,
							DateTime absolutExpiration, TimeSpan slidingExpiration) {

			Add (strKey, objItem, objDependency, absolutExpiration, slidingExpiration, 
				CacheItemPriority.Default, null, true, true);
		}

		/// <summary>
		/// Inserts an object into the Cache object with dependencies,
		/// expiration and priority policies, and a delegate you can use
		/// to notify your application when the inserted item is removed
		/// from the Cache.
		/// </summary>
		/// <param name="strKey">The cache key used to reference the item.</param>
		/// <param name="objItem">The item to be added to the cache.</param>
		/// <param name="objDependency">
		/// The file or cache key dependencies for the item. When any
		/// dependency changes, the object becomes invalid and is removed
		/// from the cache. If there are no dependencies, this paramter
		/// contains a null reference.
		/// </param>
		/// <param name="absolutExpiration">
		/// The time at which the added object expires and is removed from the cache.
		/// </param>
		/// <param name="slidingExpiration">
		/// The interval between the time the added object was last
		/// accessed and when that object expires. If this value is the
		/// equivalent of 20 minutes, the object expires and is removed
		/// from the cache 20 minutes after it is last accessed.
		/// </param>
		/// <param name="enumPriority">The relative cost of the object,
		/// as expressed by the CacheItemPriority enumeration. The cache
		/// uses this value when it evicts objects; objects with a lower
		/// cost are removed from the cache before objects with a higher
		/// cost.
		/// </param>
		/// <param name="eventRemoveCallback">A delegate that, if
		/// provided, is called when an object is removed from the cache.
		/// You can use this to notify applications when their objects
		/// are deleted from the cache.
		/// </param>
		public void Insert (string strKey, object objItem, CacheDependency objDependency,
							DateTime absolutExpiration, TimeSpan slidingExpiration,
							CacheItemPriority enumPriority, CacheItemRemovedCallback eventRemoveCallback) {

			Add (strKey, objItem, objDependency, absolutExpiration, slidingExpiration, 
				enumPriority, eventRemoveCallback, true, true);
		}

		// Called from other internal System.Web methods to add non-public objects into
		// cache, like output cache etc
		internal void InsertPrivate (string strKey, object objItem, CacheDependency objDependency,
									DateTime absolutExpiration, TimeSpan slidingExpiration,
									CacheItemPriority enumPriority, CacheItemRemovedCallback eventRemoveCallback) {

			Add (strKey, objItem, objDependency, absolutExpiration, slidingExpiration, 
				enumPriority, eventRemoveCallback, false, true);
		}

		/// <summary>
		/// Removes the specified item from the Cache object.
		/// </summary>
		/// <param name="strKey">The cache key for the cache item to remove.</param>
		/// <returns>
		/// The item removed from the Cache. If the value in the key
		/// parameter is not found, returns a null reference.
		/// </returns>
		public object Remove (string strKey) {
			return Remove (strKey, CacheItemRemovedReason.Removed);
		}

		/// <summary>
		/// Internal method that updates the cache, decremenents the
		/// number of existing items and call close on the cache entry.
		/// This method is also used from the ExpiresBuckets class to
		/// remove an item during GC flush.
		/// </summary>
		/// <param name="strKey">The cache key for the cache item to remove.</param>
		/// <param name="enumReason">Reason why the item is removed.</param>
		/// <returns>
		/// The item removed from the Cache. If the value in the key
		/// parameter is not found, returns a null reference.
		/// </returns>
		internal object Remove (string strKey, CacheItemRemovedReason enumReason) {
			CacheEntry objEntry = null;

			if (strKey == null)
				throw new ArgumentNullException ("strKey");

			_lockEntries.AcquireWriterLock (-1);
			try {
				objEntry = _arrEntries [strKey] as CacheEntry;
				if (null == objEntry)
					return null;

				_arrEntries.Remove (strKey);
			}
			finally {
				_lockEntries.ReleaseWriterLock ();
			}

			if (objEntry.HasAbsoluteExpiration || objEntry.HasSlidingExpiration)
				_objExpires.Remove (objEntry);

			objEntry.Close (enumReason);

			Interlocked.Decrement (ref _nItems);

			return objEntry.Item;
		}

		/// <summary>
		/// Retrieves the specified item from the Cache object.
		/// </summary>
		/// <param name="strKey">The identifier for the cache item to retrieve.</param>
		/// <returns>The retrieved cache item, or a null reference.</returns>
		public object Get (string strKey) {
			CacheEntry objEntry = GetEntry (strKey);

			if (objEntry == null)
				return null;

			return objEntry.Item;
		}

		internal CacheEntry GetEntry (string strKey) {
			CacheEntry objEntry = null;
			long ticksNow = DateTime.UtcNow.Ticks;
			
			if (strKey == null)
				throw new ArgumentNullException ("strKey");

			_lockEntries.AcquireReaderLock (-1);
			try {
				objEntry = _arrEntries [strKey] as CacheEntry;
				if (null == objEntry)
					return null;
			}
			finally {
				_lockEntries.ReleaseReaderLock ();
			}

			if (objEntry.HasSlidingExpiration || objEntry.HasAbsoluteExpiration) {
				if (objEntry.Expires < ticksNow) {
					Remove (strKey, CacheItemRemovedReason.Expired);
					return null;
				}
			} 

			objEntry.Hit ();
			if (objEntry.HasSlidingExpiration) {
				long ticksExpires = ticksNow + objEntry.SlidingExpiration;

				_objExpires.Update (objEntry, ticksExpires);
				objEntry.Expires = ticksExpires;
			}

			return objEntry;
		}

		/// <summary>
		/// Gets the number of items stored in the cache.
		/// </summary>
		public int Count {
			get { return _nItems; }
		}

		/// <summary>
		/// Gets or sets the cache item at the specified key.
		/// </summary>
		public object this [string strKey] {
			get {
				return Get (strKey);
			}

			set {
				Insert (strKey, value);
			}
		}
	}
}

