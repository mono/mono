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

using System;
using System.Collections;
using System.Threading;

namespace System.Web.Caching
{
	/// <summary>
	/// Implements a cache for Web applications and other. The difference
	/// from the MS.NET implementation is that we / support to use the Cache
	/// object as cache in our applications.
	/// </summary>
	/// <remarks>
	/// The Singleton cache is created per application domain, and it
	/// remains valid as long as the application domain remains active. 
	/// </remarks>
	/// <example>
	/// Usage of the singleton cache:
	/// 
	/// Cache objManager = Cache.SingletonCache;
	/// 
	/// String obj = "tobecached";
	/// objManager.Add("kalle", obj);
	/// </example>
	public sealed class Cache : IEnumerable
	{
		// Declarations

		/// <summary>
		/// Used in the absoluteExpiration parameter in an Insert
		/// method call to indicate the item should never expire. This
		/// field is read-only.
		/// </summary>
		public static readonly DateTime NoAbsoluteExpiration = DateTime.MaxValue;

		/// <summary>
		/// Used as the slidingExpiration parameter in an Insert method
		/// call to disable sliding expirations. This field is read-only.
		/// </summary>
		public static readonly TimeSpan NoSlidingExpiration = TimeSpan.Zero;

		// Helper objects
		CacheExpires _objExpires;

		// The data storage 
		// Todo: Make a specialized storage for the cache entries?
		// todo: allow system to replace the storage?
		Hashtable _arrEntries;
		ReaderWriterLock _lockEntries;

		static TimeSpan _datetimeOneYear = TimeSpan.FromDays (365);

		int _nItems; // Number of items in the cache

		public Cache ()
		{
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

		private IDictionaryEnumerator CreateEnumerator ()
		{
			Hashtable objTable;

			//Locking with 0 provides a non-expiring lock.
			_lockEntries.AcquireReaderLock (0);
			try {
				// Create a new hashtable to return as collection of public items
				objTable = new Hashtable (_arrEntries.Count);

				foreach (DictionaryEntry objEntry in _arrEntries) {
					if (objEntry.Key == null)
						continue;

					CacheEntry entry = (CacheEntry) objEntry.Value;
					if (entry.TestFlag (CacheEntry.Flags.Public))
						objTable.Add (objEntry.Key, entry.Item);
				}
			} finally {
				_lockEntries.ReleaseReaderLock ();
			}

			return objTable.GetEnumerator ();
		}

		/// <summary>
		/// Implementation of IEnumerable interface and calls the GetEnumerator that returns
		/// IDictionaryEnumerator.
		/// </summary>
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		/// <summary>
		/// Virtual override of the IEnumerable.GetEnumerator() method,
		/// returns a specialized enumerator.
		/// </summary>
		public IDictionaryEnumerator GetEnumerator ()
		{
			return CreateEnumerator ();
		}

		/// <summary>
		/// Touches a object in the cache. Used to update expire time and hit count.
		/// </summary>
		/// <param name="strKey">The identifier for the cache item to retrieve.</param>
		internal void Touch(string strKey)
		{
			Get (strKey);
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
		public object Add (string strKey,
				   object objItem,
				   CacheDependency objDependency,
				   DateTime absolutExpiration,
				   TimeSpan slidingExpiration,
				   CacheItemPriority enumPriority,
				   CacheItemRemovedCallback eventRemoveCallback)
		{
			if (strKey == null)
				throw new ArgumentNullException ("strKey");

			if (objItem == null)
				throw new ArgumentNullException ("objItem");

			if (slidingExpiration > _datetimeOneYear)
				throw new ArgumentOutOfRangeException ("slidingExpiration");

			CacheEntry objEntry;
			CacheEntry objNewEntry;

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
						   true,
						   enumPriority);

			Interlocked.Increment (ref _nItems);

			// If we have any kind of expiration add into the CacheExpires class
			if (objEntry.HasSlidingExpiration || objEntry.HasAbsoluteExpiration)
				_objExpires.Add (objEntry);

			// Check and get the new item..
			objNewEntry = UpdateCache (strKey, objEntry, true, CacheItemRemovedReason.Removed);

			if (objNewEntry == null)
				return null;

			return objEntry.Item;
		}

		/// <summary>
		/// Inserts an item into the Cache object with a cache key to
		/// reference its location and using default values provided by
		/// the CacheItemPriority enumeration.
		/// </summary>
		/// <param name="strKey">The cache key used to reference the item.</param>
		/// <param name="objItem">The item to be added to the cache.</param>
		public void Insert (string strKey, object objItem)
		{
			Add (strKey,
			     objItem,
			     null,
			     NoAbsoluteExpiration,
			     NoSlidingExpiration,
			     CacheItemPriority.Default,
			     null);
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
		public void Insert (string strKey, object objItem, CacheDependency objDependency)
		{
			Add (strKey,
			     objItem,
			     objDependency,
			     NoAbsoluteExpiration,
			     NoSlidingExpiration,
			     CacheItemPriority.Default,
			     null);
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
		public void Insert (string strKey,
				    object objItem,
				    CacheDependency objDependency,
				    DateTime absolutExpiration,
				    TimeSpan slidingExpiration)
		{
			Add (strKey,
			     objItem,
			     objDependency,
			     absolutExpiration,
			     slidingExpiration,
			     CacheItemPriority.Default,
			     null);
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
		public void Insert (string strKey,
				    object objItem,
				    CacheDependency objDependency,
				    DateTime absolutExpiration,
				    TimeSpan slidingExpiration,
				    CacheItemPriority enumPriority,
				    CacheItemRemovedCallback eventRemoveCallback)
		{
			Add (strKey,
			     objItem,
			     objDependency,
			     absolutExpiration,
			     slidingExpiration,
			     enumPriority,
			     eventRemoveCallback);
		}

		/// <summary>
		/// Removes the specified item from the Cache object.
		/// </summary>
		/// <param name="strKey">The cache key for the cache item to remove.</param>
		/// <returns>
		/// The item removed from the Cache. If the value in the key
		/// parameter is not found, returns a null reference.
		/// </returns>
		public object Remove (string strKey)
		{
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
		internal object Remove (string strKey, CacheItemRemovedReason enumReason)
		{
			CacheEntry objEntry = UpdateCache (strKey, null, true, enumReason);
			if (objEntry == null)
				return null;

			Interlocked.Decrement (ref _nItems);

			// Close the cache entry (calls the remove delegate)
			objEntry.Close (enumReason);

			return objEntry.Item;
		}

		/// <summary>
		/// Retrieves the specified item from the Cache object.
		/// </summary>
		/// <param name="strKey">The identifier for the cache item to retrieve.</param>
		/// <returns>The retrieved cache item, or a null reference.</returns>
		public object Get (string strKey)
		{
			CacheEntry objEntry = UpdateCache (strKey, null, false, CacheItemRemovedReason.Expired);

			if (objEntry == null)
				return null;

			return objEntry.Item;
		}

		/// <summary>
		/// Internal method used for removing, updating and adding CacheEntries into the cache.
		/// </summary>
		/// <param name="strKey">The identifier for the cache item to modify</param>
		/// <param name="objEntry">
		/// CacheEntry to use for overwrite operation, if this
		/// parameter is null and overwrite true the item is going to be
		/// removed
		/// </param>
		/// <param name="boolOverwrite">
		/// If true the objEntry parameter is used to overwrite the
		/// strKey entry
		/// </param>
		/// <param name="enumReason">Reason why an item was removed</param>
		/// <returns></returns>
		private CacheEntry UpdateCache (string strKey,
						CacheEntry objEntry,
						bool boolOverwrite,
						CacheItemRemovedReason enumReason)
		{
			if (strKey == null)
				throw new ArgumentNullException ("strKey");

			long ticksNow = DateTime.Now.Ticks;
			long ticksExpires = long.MaxValue;

			bool boolGetItem = false;
			bool boolExpiried = false;
			bool boolWrite = false;
			bool boolRemoved = false;

			// Are we getting the item from the hashtable
			if (boolOverwrite == false && strKey.Length > 0 && objEntry == null)
				boolGetItem = true;

			// TODO: Optimize this method, move out functionality outside the lock
			_lockEntries.AcquireReaderLock (0);
			try {
				if (boolGetItem) {
					objEntry = (CacheEntry) _arrEntries [strKey];
					if (objEntry == null)
						return null;
				}

				if (objEntry != null) {
					// Check if we have expired
					if (objEntry.HasSlidingExpiration || objEntry.HasAbsoluteExpiration) {
						if (objEntry.Expires < ticksNow) {
							// We have expired, remove the item from the cache
							boolWrite = true;
							boolExpiried = true;
						} 
					} 
				}

				// Check if we going to modify the hashtable
				if (boolWrite || (boolOverwrite && !boolExpiried)) {
					// Upgrade our lock to write
					Threading.LockCookie objCookie = _lockEntries.UpgradeToWriterLock (0);
					try {
						// Check if we going to just modify an existing entry (or add)
						if (boolOverwrite && objEntry != null) {
							_arrEntries [strKey] = objEntry;
						} else {
							// We need to remove the item, fetch the item first
							objEntry = (CacheEntry) _arrEntries [strKey];
							if (objEntry != null)
								_arrEntries.Remove (strKey);

							boolRemoved = true;
						}
					} finally {
						_lockEntries.DowngradeFromWriterLock (ref objCookie);
					}
				}

				// If the entry haven't expired or been removed update the info
				if (!boolExpiried && !boolRemoved) {
					// Update that we got a hit
					objEntry.Hits++;
					if (objEntry.HasSlidingExpiration)
						ticksExpires = ticksNow + objEntry.SlidingExpiration;
				}
			} finally {
				_lockEntries.ReleaseLock ();
			}

			// If the item was removed we need to remove it from the CacheExpired class also
			if (boolRemoved) {
				if (objEntry != null) {
					if (objEntry.HasAbsoluteExpiration || objEntry.HasSlidingExpiration)
						_objExpires.Remove (objEntry);
				}

				// Return the entry, it's not up to the UpdateCache to call Close on the entry
				return objEntry;
			}

			// If we have sliding expiration and we have a correct hit, update the expiration manager
			if (objEntry.HasSlidingExpiration)
				_objExpires.Update (objEntry, ticksExpires);

			// Return the cache entry
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

