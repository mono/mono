// 
// System.Web.Caching
//
// Author:
//   Patrik Torstensson
//   Daniel Cazzulino (dcazzulino@users.sf.net)
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
using System.Threading;

namespace System.Web.Caching {
	/// <summary>
	/// Class responsible for representing a cache entry.
	/// </summary>
	internal class CacheEntry {
		internal enum Flags {
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

		internal static readonly byte NoBucketHash = byte.MaxValue;
		internal static readonly int NoIndexInBucket = int.MaxValue;

		internal event CacheItemRemovedCallback _onRemoved;

		private ReaderWriterLock _lock = new ReaderWriterLock();

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
		internal CacheEntry (Cache objManager, string strKey, object objItem, CacheDependency objDependency, CacheItemRemovedCallback eventRemove, 
			System.DateTime dtExpires, System.TimeSpan tsSpan, long longMinHits, bool boolPublic, CacheItemPriority enumPriority ) {
			if (boolPublic)
				_enumFlags |= Flags.Public;

			_strKey = strKey;
			_objItem = objItem;
			_objCache = objManager;
			
			_onRemoved += eventRemove;

			_enumPriority = enumPriority;

			_ticksExpires = dtExpires.ToUniversalTime ().Ticks;

			_ticksSlidingExpiration = tsSpan.Ticks;

			// If we have a sliding expiration it overrides the absolute expiration (MS behavior)
			// This is because sliding expiration causes the absolute expiration to be 
			// moved after each period, and the absolute expiration is the value used 
			// for all expiration calculations.
			if (tsSpan.Ticks != Cache.NoSlidingExpiration.Ticks)
				_ticksExpires = System.DateTime.UtcNow.AddTicks(_ticksSlidingExpiration).Ticks;
			
			_objDependency = objDependency;
			if (_objDependency != null)
				// Add the entry to the cache dependency handler (we support multiple entries per handler)
				_objDependency.Changed += new CacheDependencyChangedHandler (OnChanged); 

			_longMinHits = longMinHits;
		}


		internal void OnChanged (object sender, CacheDependencyChangedArgs objDependency) {
			_objCache.Remove (_strKey, CacheItemRemovedReason.DependencyChanged);
		}

		/// <summary>
		/// Cleans up the cache entry, removes the cache dependency and calls the remove delegate.
		/// </summary>
		/// <param name="enumReason">The reason why the cache entry are going to be removed</param>
		internal void Close(CacheItemRemovedReason enumReason) {	
			Delegate [] removedEvents = null;

			_lock.AcquireWriterLock(-1);
			try {
				// Check if the item already is removed
				if ((_enumFlags & Flags.Removed) != 0)
					return;

				_enumFlags |= Flags.Removed;

				if (_onRemoved != null)
					removedEvents = _onRemoved.GetInvocationList ();
			}
			finally {
				_lock.ReleaseWriterLock();
			}

			if (removedEvents != null) {
				// Call the delegate to tell that we are now removing the entry
				if ((_enumFlags & Flags.Public) != 0) {
					foreach (Delegate del in removedEvents) {
						CacheItemRemovedCallback removed = (CacheItemRemovedCallback) del;
						try {
							removed (_strKey, _objItem, enumReason);		
						}
						catch (System.Exception obj) {
							HttpApplicationFactory.SignalError (obj);
						}
					}
				} 
				else {
					foreach (Delegate del in removedEvents) {
						CacheItemRemovedCallback removed = (CacheItemRemovedCallback) del;
						try {
							removed (_strKey, _objItem, enumReason);		
						}
						catch (Exception) {
						}
					}
				}
			}

			_lock.AcquireWriterLock(-1);
			try {
				// If we have a dependency, remove the entry
				if (_objDependency != null)
					_objDependency.Changed -= new CacheDependencyChangedHandler (OnChanged);
			}
			finally {
				_lock.ReleaseWriterLock();
			}
		}
	
		internal bool HasUsage {
			get { 
				if (_longMinHits == System.Int64.MaxValue)
					return false;

				return true;
			}
		}

		internal bool HasAbsoluteExpiration {
			get { 
				if (_ticksExpires == Cache.NoAbsoluteExpiration.Ticks) 
					return false;

				return true;
			}
		}

		internal bool HasSlidingExpiration {
			get { 
				if (_ticksSlidingExpiration == Cache.NoSlidingExpiration.Ticks) 
					return false;

				return true;
			}
		}
		
		internal byte ExpiresBucket {
			get { 
				return _byteExpiresBucket; 
			}
			set { 
				_byteExpiresBucket = value; 
			}
		}

		internal int ExpiresIndex {
			get { 
				return _intExpiresIndex; 
			}
			
			set { 
				_intExpiresIndex = value; 
			}
		}
        
		internal long Expires {
			get { 
				return _ticksExpires; 
			}
			set { 
				_ticksExpires = value; 
			}
		}

		internal long SlidingExpiration {
			get { 
				return _ticksSlidingExpiration; 
			}
		}

		internal object Item {
			get {
				return _objItem; 
			}
		}

		internal string Key {
			get { 
				return _strKey; 
			}
		}

		internal long Hits {
			get {
				return _longHits; 
			}
		}

		internal long MinimumHits {
			get { 
				return _longMinHits; 
			}
		}

		internal CacheItemPriority Priority {
			get { 
				return _enumPriority; 
			}
		}

		internal bool IsPublic {
			get {
				return (_enumFlags & Flags.Public) != 0;
			}
		}

		internal void Hit () {
			Interlocked.Increment (ref _longHits);
		}
	}
}
