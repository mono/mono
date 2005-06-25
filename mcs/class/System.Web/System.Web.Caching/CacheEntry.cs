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
	internal class CacheEntry {
		enum Flags {
			Removed	= 1,
			Public	= 2
		}

		internal static readonly byte NoBucketHash = byte.MaxValue;
		internal static readonly int NoIndexInBucket = int.MaxValue;

		CacheItemPriority _enumPriority;
		long _longHits;
		byte _byteExpiresBucket;
		int _intExpiresIndex;
		long _ticksExpires;	
		long _ticksSlidingExpiration;
		string _strKey;
		object _objItem;
		long _longMinHits;
		Flags _enumFlags;
		CacheDependency _objDependency;
		Cache _objCache;
		ReaderWriterLock _lock = new ReaderWriterLock();

		internal event CacheItemRemovedCallback _onRemoved;

		internal CacheEntry (Cache objManager, string strKey, object objItem,CacheDependency objDependency,
				CacheItemRemovedCallback eventRemove, DateTime dtExpires, TimeSpan tsSpan,
				long longMinHits, bool boolPublic, CacheItemPriority enumPriority )
		{
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
				_ticksExpires = DateTime.UtcNow.AddTicks (_ticksSlidingExpiration).Ticks;
			
			_objDependency = objDependency;
			if (_objDependency != null)
				// Add the entry to the cache dependency handler (we support multiple entries per handler)
				_objDependency.Changed += new CacheDependencyChangedHandler (OnChanged); 

			_longMinHits = longMinHits;
		}


		internal void OnChanged (object sender, CacheDependencyChangedArgs objDependency)
		{
			_objCache.Remove (_strKey, CacheItemRemovedReason.DependencyChanged);
		}

		internal void Close (CacheItemRemovedReason enumReason)
		{
			Delegate [] removedEvents = null;

			_lock.AcquireWriterLock(-1);
			try {
				// Check if the item already is removed
				if ((_enumFlags & Flags.Removed) != 0)
					return;

				_enumFlags |= Flags.Removed;

				if (_onRemoved != null)
					removedEvents = _onRemoved.GetInvocationList ();
			} finally {
				_lock.ReleaseWriterLock();
			}

			if (removedEvents != null) {
				// Call the delegate to tell that we are now removing the entry
				foreach (Delegate del in removedEvents) {
					CacheItemRemovedCallback removed = (CacheItemRemovedCallback) del;
					try {
						removed (_strKey, _objItem, enumReason);		
					} catch (Exception obj) {
						if (IsPublic)
							HttpApplicationFactory.SignalError (obj);
					}
				}
			}

			_lock.AcquireWriterLock(-1);
			try {
				// If we have a dependency, remove the entry
				if (_objDependency != null)
					_objDependency.Changed -= new CacheDependencyChangedHandler (OnChanged);
			} finally {
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
			get { return _byteExpiresBucket; }
			set { _byteExpiresBucket = value; }
		}

		internal int ExpiresIndex {
			get { return _intExpiresIndex; }
			set { _intExpiresIndex = value; }
		}
        
		internal long Expires {
			get { return _ticksExpires; }
			set { _ticksExpires = value; }
		}

		internal long SlidingExpiration {
			get { return _ticksSlidingExpiration; }
		}

		internal object Item {
			get { return _objItem; }
		}

		internal string Key {
			get { return _strKey; }
		}

		internal long Hits {
			get { return _longHits; }
		}

		internal long MinimumHits {
			get { return _longMinHits; }
		}

		internal CacheItemPriority Priority {
			get { return _enumPriority; }
		}

		internal bool IsPublic {
			get { return (_enumFlags & Flags.Public) != 0; }
		}

		internal void Hit ()
		{
			Interlocked.Increment (ref _longHits);
		}
	}
}

