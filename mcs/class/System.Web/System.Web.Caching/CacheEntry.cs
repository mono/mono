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
	/// Class responsible for representing a cache entry.
	/// </summary>
	internal class CacheEntry
	{
		/// <summary>
		/// Defines the status of the current cache entry
		/// </summary>
		public enum Flags
		{
			Removed	= 0,
			Public	= 1
		}

		private CacheItemPriority		_enumPriority;
        
		private long	_longHits;

		private byte	_byteExpiresBucket;
		private int		_intExpiresIndex;

		private	long	_ticksExpires;	
		private long	_ticksSlidingExpiration;

		private	string	_strKey;
		private	object	_objItem;

		private long	_longMinHits;

		private Flags	_enumFlags;
		
		private CacheDependency		_objDependency;
		private Cache				_objCache;

		/// <summary>
		/// The item is not placed in a bucket. [DHC]
		/// </summary>
		internal static readonly byte NoBucketHash = byte.MaxValue;
		
		/// <summary>
		/// The item is not placed in a bucket. [DHC]
		/// </summary>
		internal static readonly int NoIndexInBucket = int.MaxValue;

		/// <summary>
		/// Lock for syncronized operations. [DHC]
		/// </summary>
		System.Threading.ReaderWriterLock _lock = new System.Threading.ReaderWriterLock();

		/// <summary>
		/// Constructs a new cache entry
		/// </summary>
		/// <param name="strKey">The cache key used to reference the item.</param>
		/// <param name="objItem">The item to be added to the cache.</param>
		/// <param name="objDependency">The file or cache key dependencies for the item. When any dependency changes, the object becomes invalid and is removed from the cache. If there are no dependencies, this paramter contains a null reference.</param>
		/// <param name="dtExpires">The time at which the added object expires and is removed from the cache. </param>
		/// <param name="tsSpan">The interval between the time the added object was last accessed and when that object expires. If this value is the equivalent of 20 minutes, the object expires and is removed from the cache 20 minutes after it is last accessed.</param>
		/// <param name="longMinHits">Used to detect and control if the item should be flushed due to under usage</param>
		/// <param name="boolPublic">Defines if the item is public or not</param>
		/// <param name="enumPriority">The relative cost of the object, as expressed by the CacheItemPriority enumeration. The cache uses this value when it evicts objects; objects with a lower cost are removed from the cache before objects with a higher cost.</param>
		internal CacheEntry(	Cache objManager, string strKey, object objItem, CacheDependency objDependency, CacheItemRemovedCallback eventRemove, 
			System.DateTime dtExpires, System.TimeSpan tsSpan, long longMinHits, bool boolPublic, CacheItemPriority enumPriority )
		{
			if (boolPublic)
			{
				SetFlag(Flags.Public);
			}

			_strKey = strKey;
			_objItem = objItem;
			_objCache = objManager;
			
			_onRemoved += eventRemove;

			_enumPriority = enumPriority;

			_ticksExpires = dtExpires.Ticks;

			_ticksSlidingExpiration = tsSpan.Ticks;

			// If we have a sliding expiration it overrides the absolute expiration (MS behavior)
			// This is because sliding expiration causes the absolute expiration to be 
			// moved after each period, and the absolute expiration is the value used 
			// for all expiration calculations.
			//HACK: [DHC] Use constants defined in Cache.
			//if (tsSpan.Ticks != System.TimeSpan.Zero.Ticks)
			if (tsSpan.Ticks != Cache.NoSlidingExpiration.Ticks)
			{
				_ticksExpires = System.DateTime.Now.AddTicks(_ticksSlidingExpiration).Ticks;
			}
			
			_objDependency = objDependency;
			if (_objDependency != null)
			{
				// Add the entry to the cache dependency handler (we support multiple entries per handler)
				_objDependency.Changed += new CacheDependencyChangedHandler (OnChanged); 
			}

			_longMinHits = longMinHits;
		}

		internal event CacheItemRemovedCallback _onRemoved;

		internal void OnChanged (object sender, CacheDependencyChangedArgs objDependency)
		{
			_objCache.Remove (_strKey, CacheItemRemovedReason.DependencyChanged);
		}

		/// <summary>
		/// Cleans up the cache entry, removes the cache dependency and calls the remove delegate.
		/// </summary>
		/// <param name="enumReason">The reason why the cache entry are going to be removed</param>
		internal void Close(CacheItemRemovedReason enumReason)
		{	
			//HACK: optimized locks. [DHC]
			_lock.AcquireWriterLock(0);
			try
			{
				// Check if the item already is removed
				if (TestFlag(Flags.Removed))
				{
					return;
				}

				SetFlag(Flags.Removed);

				if (_onRemoved != null)
				{
					// Call the delegate to tell that we are now removing the entry
					try 
					{
						_onRemoved(_strKey, _objItem, enumReason);		
					}
					catch (System.Exception objException)
					{
						System.Diagnostics.Debug.Fail("System.Web.CacheEntry.Close() Exception when calling remove delegate", "Message: " + objException.Message + " Stack: " + objException.StackTrace + " Source:" + objException.Source);
					}
				}

				// If we have a dependency, remove the entry
				if (_objDependency != null)
				{
					_objDependency.Changed -= new CacheDependencyChangedHandler (OnChanged);
				}
			}
			finally
			{
				_lock.ReleaseWriterLock();
			}
		}

		/// <summary>
		/// Tests a specific flag is set or not.
		/// </summary>
		/// <param name="oFlag">Flag to test agains</param>
		/// <returns>Returns true if the flag is set.</returns>
		internal bool TestFlag(Flags oFlag)
		{
			_lock.AcquireReaderLock(0);
			try
			{
				if ((_enumFlags & oFlag) != 0)
				{
					return true;
				} 
			}
			finally
			{
				_lock.ReleaseReaderLock();
			}

			return false;
		}

		/// <summary>
		/// Sets a specific flag.
		/// </summary>
		/// <param name="oFlag">Flag to set.</param>
		internal void SetFlag(Flags oFlag)
		{
			_lock.AcquireWriterLock(0);
			try
			{
				_enumFlags |= oFlag;
			}
			finally
			{
				_lock.ReleaseWriterLock	();
			}
		}

		/// <summary>
		/// Returns true if the object has minimum hit usage flushing enabled.
		/// </summary>
		internal bool HasUsage
		{
			get { 
				if (_longMinHits == System.Int64.MaxValue) 
				{
					return false;
				}

				return true;
			}
		}

		/// <summary>
		/// Returns true if the entry has absolute expiration.
		/// </summary>
		internal bool HasAbsoluteExpiration
		{
			get 
			{ 
				//HACK: [DHC] Use constant defined in Cache.
				//if (_ticksExpires == System.DateTime.MaxValue.Ticks) 
				if (_ticksExpires == Cache.NoAbsoluteExpiration.Ticks) 
				{
					return false;
				}

				return true;
			}
		}

		/// <summary>
		/// Returns true if the entry has sliding expiration enabled.
		/// </summary>
		internal bool HasSlidingExpiration
		{
			get 
			{ 
				//HACK: [DHC] Use constants defined in Cache.
				//if (_ticksSlidingExpiration == System.TimeSpan.Zero.Ticks) 
				if (_ticksSlidingExpiration == Cache.NoSlidingExpiration.Ticks) 
				{
					return false;
				}

				return true;
			}
		}
		
		/// <summary>
		/// Gets and sets the current expires bucket the entry is active in.
		/// </summary>
		internal byte ExpiresBucket
		{
			get 
			{ 
				_lock.AcquireReaderLock(0);
				try
				{
					return _byteExpiresBucket; 
				}
				finally
				{
					_lock.ReleaseReaderLock();
				}
			}
			set 
			{ 
				_lock.AcquireWriterLock(0);
				try
				{
					_byteExpiresBucket = value; 
				}
				finally
				{
					_lock.ReleaseWriterLock	();
				}
			}
		}

		/// <summary>
		/// Gets and sets the current index in the expires bucket of the current cache entry.
		/// </summary>
		internal int ExpiresIndex
		{
			get 
			{ 
				_lock.AcquireReaderLock(0);
				try
				{
					return _intExpiresIndex; 
				}
				finally
				{
					_lock.ReleaseReaderLock();
				}
			}
			
			set 
			{ 
				_lock.AcquireWriterLock(0);
				try
				{
					_intExpiresIndex = value; 
				}
				finally
				{
					_lock.ReleaseWriterLock();
				}
			}
		}
        
		/// <summary>
		/// Gets and sets the expiration of the cache entry.
		/// </summary>
		internal long Expires
		{
			get 
			{ 
				_lock.AcquireReaderLock(0);
				try
				{
					return _ticksExpires; 
				}
				finally
				{
					_lock.ReleaseReaderLock();
				}
			}
			set 
			{ 
				_lock.AcquireWriterLock(0);
				try
				{
					_ticksExpires = value; 
				}
				finally
				{
					_lock.ReleaseWriterLock();
				}
			}
		}

		/// <summary>
		/// Gets the sliding expiration value. The return value is in ticks (since 0/0-01 in 100nanosec)
		/// </summary>
		internal long SlidingExpiration
		{
			get 
			{ 
				return _ticksSlidingExpiration; 
			}
		}

		/// <summary>
		/// Returns the current cached item.
		/// </summary>
		internal object Item
		{
			get
			{
				return _objItem; 
			}
		}

		/// <summary>
		/// Returns the current cache identifier.
		/// </summary>
		internal string Key
		{
			get 
			{ 
				return _strKey; 
			}
		}

		/// <summary>
		/// Gets and sets the current number of hits on the cache entry.
		/// </summary>
		internal long Hits
		{
			// todo: Could be optimized by using interlocked methods..
			get 
			{
				_lock.AcquireReaderLock(0);
				try
				{
					return _longHits; 
				}
				finally
				{
					_lock.ReleaseReaderLock();
				}
			}
			set 
			{ 
				_lock.AcquireWriterLock(0);
				try
				{
					_longHits = value; 
				}
				finally
				{
					_lock.ReleaseWriterLock();
				}
			}
		}

		/// <summary>
		/// Returns minimum hits for the usage flushing rutine.
		/// </summary>
		internal long MinimumHits
		{
			get 
			{ 
				return _longMinHits; 
			}
		}

		/// <summary>
		/// Returns the priority of the cache entry.
		/// </summary>
		internal CacheItemPriority Priority
		{
			get 
			{ 
				return _enumPriority; 
			}
		}
	}
}
