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
	/// Implements a cache for Web applications and other. The difference from the MS.NET implementation is that we
	/// support to use the Cache object as cache in our applications.
	/// </summary>
	/// <remarks>
	/// The Singleton cache is created per application domain, and it remains valid as long as the application domain remains active. 
	/// </remarks>
	/// <example>
	/// Usage of the singleton cache:
	/// 
	/// Cache objManager = Cache.SingletonCache;
	/// 
	/// String obj = "tobecached";
	/// objManager.Add("kalle", obj);
	/// </example>
	public class Cache : System.Collections.IEnumerable, System.IDisposable
	{
		// Declarations

		// MS.NET Does only have the cache connected to the HttpRuntime and we don't have the HttpRuntime (yet)
		// HACK: [DHC] Don't construct a new cache unless it's actually used.
		//static Cache	objSingletonCache = new Cache();
		static Cache	objSingletonCache;

		private bool	_boolDisposed;

		// Helper objects
		private CacheExpires _objExpires;
		
		// The data storage 
		// Todo: Make a specialized storage for the cache entries?
		// todo: allow system to replace the storage?
		private System.Collections.Hashtable		_arrEntries;
		private System.Threading.ReaderWriterLock	_lockEntries;

		static private System.TimeSpan _datetimeOneYear = System.TimeSpan.FromDays(365);
		
		// Number of items in the cache
		private long	_longItems;

		// Constructor
		public Cache()
		{
			_boolDisposed = false;
			_longItems = 0;

			_lockEntries = new System.Threading.ReaderWriterLock();
			_arrEntries = new System.Collections.Hashtable();
			
			_objExpires = new CacheExpires(this);
		}

		// Public methods and properties

		// 
		/// <summary>
		/// Returns a static version of the cache. In MS.NET the cache is stored in the System.Web.HttpRuntime 
		///	but we keep it here as a singleton (right now anyway).
		/// </summary>
		public static Cache SingletonCache
		{
			get 
			{ 
				if (objSingletonCache == null)
				{
					// HACK: [DHC] Create the cache here instead of throwing.
					//throw new System.InvalidOperationException();
					objSingletonCache = new Cache();
				}

				return objSingletonCache;
			}
		}

		/// <summary>
		/// Used in the absoluteExpiration parameter in an Insert method call to indicate the item should never expire. This field is read-only.
		/// </summary>
		public static readonly System.DateTime NoAbsoluteExpiration	= System.DateTime.MaxValue;
		
		/// <summary>
		/// Used as the slidingExpiration parameter in an Insert method call to disable sliding expirations. This field is read-only.
		/// </summary>
		public static readonly System.TimeSpan NoSlidingExpiration	= System.TimeSpan.Zero;

		/// <summary>
		/// Internal method to create a enumerator and over all public items in the cache and is used by GetEnumerator method.
		/// </summary>
		/// <returns>Returns IDictionaryEnumerator that contains all public items in the cache</returns>
		private System.Collections.IDictionaryEnumerator CreateEnumerator()
		{
			System.Collections.Hashtable objTable;
				
			//Locking with 0 provides a non-expiring lock.
			_lockEntries.AcquireReaderLock(0);
			try 
			{
				// Create a new hashtable to return as collection of public items
				objTable = new System.Collections.Hashtable(_arrEntries.Count);

				foreach(System.Collections.DictionaryEntry objEntry in _arrEntries)
				{
					if (objEntry.Key != null)
					{
						// Check if this is a public entry
						if (((CacheEntry) objEntry.Value).TestFlag(CacheEntry.Flags.Public))
						{
							// Add to the collection
							objTable.Add(objEntry.Key, ((CacheEntry) objEntry.Value).Item);
						}
					}
				}
			}
			finally
			{
				_lockEntries.ReleaseReaderLock();
			}

			return objTable.GetEnumerator();
		}

		/// <summary>
		/// Implementation of IEnumerable interface and calls the GetEnumerator that returns
		/// IDictionaryEnumerator.
		/// </summary>
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <summary>
		/// Virtual override of the IEnumerable.GetEnumerator() method, returns a specialized enumerator.
		/// </summary>
		public System.Collections.IDictionaryEnumerator GetEnumerator()
		{
			return CreateEnumerator();
		}
		
		/// <summary>
		/// Touches a object in the cache. Used to update expire time and hit count.
		/// </summary>
		/// <param name="strKey">The identifier for the cache item to retrieve.</param>
		public void Touch(string strKey)
		{
			// Just touch the object
			Get(strKey);
		}

		/// <summary>
		/// Adds the specified item to the Cache object with dependencies, expiration and priority policies, and a 
		/// delegate you can use to notify your application when the inserted item is removed from the Cache.
		/// </summary>
		/// <param name="strKey">The cache key used to reference the item.</param>
		/// <param name="objItem">The item to be added to the cache.</param>
		/// <param name="objDependency">The file or cache key dependencies for the item. When any dependency changes, the object becomes invalid and is removed from the cache. If there are no dependencies, this paramter contains a null reference.</param>
		/// <param name="absolutExpiration">The time at which the added object expires and is removed from the cache. </param>
		/// <param name="slidingExpiration">The interval between the time the added object was last accessed and when that object expires. If this value is the equivalent of 20 minutes, the object expires and is removed from the cache 20 minutes after it is last accessed.</param>
		/// <param name="enumPriority">The relative cost of the object, as expressed by the CacheItemPriority enumeration. The cache uses this value when it evicts objects; objects with a lower cost are removed from the cache before objects with a higher cost.</param>
		/// <param name="enumPriorityDecay">The rate at which an object in the cache decays in importance. Objects that decay quickly are more likely to be removed.</param>
		/// <param name="eventRemoveCallback">A delegate that, if provided, is called when an object is removed from the cache. You can use this to notify applications when their objects are deleted from the cache.</param>
		/// <returns>The Object item added to the Cache.</returns>
		public object Add(string strKey, object objItem, CacheDependency objDependency, System.DateTime absolutExpiration, System.TimeSpan slidingExpiration, CacheItemPriority enumPriority, CacheItemPriorityDecay enumPriorityDecay, CacheItemRemovedCallback eventRemoveCallback)
		{
			if (_boolDisposed)
			{
				throw new System.ObjectDisposedException("System.Web.Cache");
			}

			if (strKey == null)
			{
				throw new System.ArgumentNullException("strKey");
			}

			if (objItem == null)
			{
				throw new System.ArgumentNullException("objItem");
			}

			if (slidingExpiration > _datetimeOneYear)
			{
				throw new System.ArgumentOutOfRangeException("slidingExpiration");
			}

			CacheEntry objEntry;
			CacheEntry objNewEntry;

			long longHitRange = 10000;

			// todo: check decay and make up the minHit range
			
			objEntry = new CacheEntry(this, strKey, objItem, objDependency, eventRemoveCallback, absolutExpiration, slidingExpiration, longHitRange, true, enumPriority);

			System.Threading.Interlocked.Increment(ref _longItems);

			// If we have any kind of expiration add into the CacheExpires class
			if (objEntry.HasSlidingExpiration || objEntry.HasAbsoluteExpiration)
			{
				// Add it to CacheExpires 
				_objExpires.Add(objEntry);
			}

			// Check and get the new item..
			objNewEntry = UpdateCache(strKey, objEntry, true, CacheItemRemovedReason.Removed);

			if (objNewEntry != null)
			{
				// Return added item
				return objEntry.Item;
			} 
			else
			{
				return null;
			}
		}

		/// <summary>
		///	Inserts an item into the Cache object with a cache key to reference its location and using default values 
		///	provided by the CacheItemPriority and CacheItemPriorityDecay enumerations.
		/// </summary>
		/// <param name="strKey">The cache key used to reference the item.</param>
		/// <param name="objItem">The item to be added to the cache.</param>
		public void Insert(string strKey, object objItem)
		{
			//HACK: [DHC] Use constants defined in Cache.
			//Add(strKey, objItem, null, System.DateTime.MaxValue, System.TimeSpan.Zero, CacheItemPriority.Default, CacheItemPriorityDecay.Default, null);
			Add(strKey, objItem, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.Default, CacheItemPriorityDecay.Default, null);
		}

		/// <summary>
		/// Inserts an object into the Cache that has file or key dependencies.
		/// </summary>
		/// <param name="strKey">The cache key used to reference the item.</param>
		/// <param name="objItem">The item to be added to the cache.</param>
		/// <param name="objDependency">The file or cache key dependencies for the item. When any dependency changes, the object becomes invalid and is removed from the cache. If there are no dependencies, this paramter contains a null reference.</param>
		public void Insert(string strKey, object objItem, CacheDependency objDependency)
		{
			//HACK: [DHC] Use constants defined in Cache.
			//Add(strKey, objItem, objDependency, System.DateTime.MaxValue, System.TimeSpan.Zero, CacheItemPriority.Default, CacheItemPriorityDecay.Default, null);
			Add(strKey, objItem, objDependency, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.Default, CacheItemPriorityDecay.Default, null);
		}

		/// <summary>
		/// Inserts an object into the Cache with dependencies and expiration policies.
		/// </summary>
		/// <param name="strKey">The cache key used to reference the item.</param>
		/// <param name="objItem">The item to be added to the cache.</param>
		/// <param name="objDependency">The file or cache key dependencies for the item. When any dependency changes, the object becomes invalid and is removed from the cache. If there are no dependencies, this paramter contains a null reference.</param>
		/// <param name="absolutExpiration">The time at which the added object expires and is removed from the cache. </param>
		/// <param name="slidingExpiration">The interval between the time the added object was last accessed and when that object expires. If this value is the equivalent of 20 minutes, the object expires and is removed from the cache 20 minutes after it is last accessed.</param>
		public void Insert(string strKey, object objItem, CacheDependency objDependency, System.DateTime absolutExpiration, System.TimeSpan slidingExpiration)
		{
			Add(strKey, objItem, objDependency, absolutExpiration, slidingExpiration, CacheItemPriority.Default, CacheItemPriorityDecay.Default, null);
		}
		
		/// <summary>
		/// Inserts an object into the Cache object with dependencies, expiration and priority policies, and a delegate 
		/// you can use to notify your application when the inserted item is removed from the Cache.
		/// </summary>
		/// <param name="strKey">The cache key used to reference the item.</param>
		/// <param name="objItem">The item to be added to the cache.</param>
		/// <param name="objDependency">The file or cache key dependencies for the item. When any dependency changes, the object becomes invalid and is removed from the cache. If there are no dependencies, this paramter contains a null reference.</param>
		/// <param name="absolutExpiration">The time at which the added object expires and is removed from the cache. </param>
		/// <param name="slidingExpiration">The interval between the time the added object was last accessed and when that object expires. If this value is the equivalent of 20 minutes, the object expires and is removed from the cache 20 minutes after it is last accessed.</param>
		/// <param name="enumPriority">The relative cost of the object, as expressed by the CacheItemPriority enumeration. The cache uses this value when it evicts objects; objects with a lower cost are removed from the cache before objects with a higher cost.</param>
		/// <param name="enumPriorityDecay">The rate at which an object in the cache decays in importance. Objects that decay quickly are more likely to be removed.</param>
		/// <param name="eventRemoveCallback">A delegate that, if provided, is called when an object is removed from the cache. You can use this to notify applications when their objects are deleted from the cache.</param>
		public void Insert(string strKey, object objItem, CacheDependency objDependency, System.DateTime absolutExpiration, System.TimeSpan slidingExpiration, CacheItemPriority enumPriority, CacheItemPriorityDecay enumPriorityDecay, CacheItemRemovedCallback eventRemoveCallback)
		{
			Add(strKey, objItem, objDependency, absolutExpiration, slidingExpiration, enumPriority, enumPriorityDecay, eventRemoveCallback);
		}

		/// <summary>
		/// Removes the specified item from the Cache object.
		/// </summary>
		/// <param name="strKey">The cache key for the cache item to remove.</param>
		/// <returns>The item removed from the Cache. If the value in the key parameter is not found, returns a null reference.</returns>
		public object Remove(string strKey)
		{
			return Remove(strKey, CacheItemRemovedReason.Removed);
		}

		/// <summary>
		/// Internal method that updates the cache, decremenents the number of existing items and call close on the cache entry. This method
		/// is also used from the ExpiresBuckets class to remove an item during GC flush.
		/// </summary>
		/// <param name="strKey">The cache key for the cache item to remove.</param>
		/// <param name="enumReason">Reason why the item is removed.</param>
		/// <returns>The item removed from the Cache. If the value in the key parameter is not found, returns a null reference.</returns>
		internal object Remove(string strKey, CacheItemRemovedReason enumReason)
		{
			CacheEntry objEntry = UpdateCache(strKey, null, true, enumReason);

			if (objEntry != null)
			{
				System.Threading.Interlocked.Decrement(ref _longItems);

				// Close the cache entry (calls the remove delegate)
				objEntry.Close(enumReason);

				return objEntry.Item;
			} else
			{
				return null;
			}
		}

		/// <summary>
		/// Retrieves the specified item from the Cache object.
		/// </summary>
		/// <param name="strKey">The identifier for the cache item to retrieve.</param>
		/// <returns>The retrieved cache item, or a null reference.</returns>
		public object Get(string strKey)
		{
			CacheEntry objEntry = UpdateCache(strKey, null, false, CacheItemRemovedReason.Expired);

			if (objEntry == null)
			{
				return null;
			} else
			{
				return objEntry.Item;
			}
		}

		/// <summary>
		/// Internal method used for removing, updating and adding CacheEntries into the cache.
		/// </summary>
		/// <param name="strKey">The identifier for the cache item to modify</param>
		/// <param name="objEntry">CacheEntry to use for overwrite operation, if this parameter is null and overwrite true the item is going to be removed</param>
		/// <param name="boolOverwrite">If true the objEntry parameter is used to overwrite the strKey entry</param>
		/// <param name="enumReason">Reason why an item was removed</param>
		/// <returns></returns>
		private CacheEntry UpdateCache(string strKey, CacheEntry objEntry, bool boolOverwrite, CacheItemRemovedReason enumReason)
		{
			if (_boolDisposed)
			{
				throw new System.ObjectDisposedException("System.Web.Cache", "Can't update item(s) in a disposed cache");
			}

			if (strKey == null)
			{
				throw new System.ArgumentNullException("System.Web.Cache");
			}

			long ticksNow = System.DateTime.Now.Ticks;
			long ticksExpires = long.MaxValue;

			bool boolGetItem = false;
			bool boolExpiried = false;
			bool boolWrite = false;
			bool boolRemoved = false;

			// Are we getting the item from the hashtable
			if (boolOverwrite == false && strKey.Length > 0 && objEntry == null)
			{
				boolGetItem = true;
			}

			// TODO: Optimize this method, move out functionality outside the lock
			_lockEntries.AcquireReaderLock(0);
			try 
			{	
				if (boolGetItem)
				{
					objEntry = (CacheEntry) _arrEntries[strKey];
					if (objEntry == null)
					{
						return null;
					}
				}

				if (objEntry != null)
				{
					// Check if we have expired
					if (objEntry.HasSlidingExpiration || objEntry.HasAbsoluteExpiration)
					{
						if (objEntry.Expires < ticksNow)
						{
							// We have expired, remove the item from the cache
							boolWrite = true;
							boolExpiried = true;
						} 
					} 
				}

				// Check if we going to modify the hashtable
				if (boolWrite || (boolOverwrite && !boolExpiried))
				{
					// Upgrade our lock to write
					System.Threading.LockCookie objCookie = _lockEntries.UpgradeToWriterLock(0);
					try 
					{
						// Check if we going to just modify an existing entry (or add)
						if (boolOverwrite && objEntry != null)
						{
							_arrEntries[strKey] = objEntry;
						} 
						else
						{
							// We need to remove the item, fetch the item first
							objEntry = (CacheEntry) _arrEntries[strKey];
							if (objEntry != null)
							{
								_arrEntries.Remove(strKey);
							}

							boolRemoved = true;
						}
					}
					finally
					{
						_lockEntries.DowngradeFromWriterLock(ref objCookie);
					}
				}

				// If the entry haven't expired or been removed update the info
				if (!boolExpiried && !boolRemoved)
				{
					// Update that we got a hit
					objEntry.Hits++;
					if (objEntry.HasSlidingExpiration)
					{
						ticksExpires = ticksNow + objEntry.SlidingExpiration;
					}
				}
			}
			finally
			{
				_lockEntries.ReleaseLock();

			}

			// If the item was removed we need to remove it from the CacheExpired class also
			if (boolRemoved)
			{
				if (objEntry != null)
				{
					if (objEntry.HasAbsoluteExpiration || objEntry.HasSlidingExpiration)
					{
						_objExpires.Remove(objEntry);
					}
				}

				// Return the entry, it's not up to the UpdateCache to call Close on the entry
				return objEntry;
			}

			// If we have sliding expiration and we have a correct hit, update the expiration manager
			if (objEntry.HasSlidingExpiration)
			{
				_objExpires.Update(objEntry, ticksExpires);
			}

			// Return the cache entry
			return objEntry;
		}

		/// <summary>
		/// Gets the number of items stored in the cache.
		/// </summary>
		public long Count
		{
			get 
			{
				return _longItems;
			}
		}

		/// <summary>
		/// Gets or sets the cache item at the specified key.
		/// </summary>
		public object this[string strKey]
		{
			get 
			{
				return Get(strKey);
			}

			set
			{
				Insert(strKey, value);
			}
		}

		/// <summary>
		/// Called to close the cache when the AppDomain is closing down or the GC has decided it's time to destroy the object.
		/// </summary>
		public void Dispose()
		{
			_boolDisposed = true;

			_lockEntries.AcquireReaderLock(0);
			try 
			{
				foreach(System.Collections.DictionaryEntry objEntry in _arrEntries)
				{
					if (objEntry.Key != null)
					{
						// Check if this is active
						if ( ((CacheEntry) objEntry.Value).TestFlag(CacheEntry.Flags.Removed) )
						{
							try 
							{
								((CacheEntry) objEntry.Value).Close(CacheItemRemovedReason.Removed);
							}
							catch (System.Exception objException)
							{
                                System.Diagnostics.Debug.Fail("System.Web.Cache.Dispose() Exception when closing cache entry", "Message: " + objException.Message + " Stack: " + objException.StackTrace + " Source:" + objException.Source);
							}
						}
					}
				}
				//HACK: dispose the expiring helper.
				_objExpires.Dispose();
			}
			finally
			{
				_lockEntries.ReleaseReaderLock();
			}
		}
	}
}
