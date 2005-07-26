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

#if TARGET_J2EE
		internal void Destroy()
		{
			_arrEntries = null;
			_objExpires.Close();
		}
#endif

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

			objEntry = new CacheEntry (	this,
							strKey,
							objItem,
							objDependency,
							eventRemoveCallback,
							absolutExpiration,
							slidingExpiration,
							longHitRange,
							pub,
							enumPriority);

			_lockEntries.AcquireWriterLock (-1);
			try {
				_nItems++;
				if (_arrEntries.Contains (strKey)) {
					if (overwrite)
						objOldEntry = _arrEntries [strKey] as CacheEntry;
					else
						return null;
				}
				
				objEntry.Hit ();
				_arrEntries [strKey] = objEntry;

				// If we have any kind of expiration add into the CacheExpires
				// Do this under the lock so no-one can retrieve the objEntry
				// before it is fully initialized.
				if (objEntry.HasSlidingExpiration || objEntry.HasAbsoluteExpiration) {
					if (objEntry.HasSlidingExpiration)
						objEntry.Expires = DateTime.UtcNow.Ticks + objEntry.SlidingExpiration;

					_objExpires.Add (objEntry);
				}
			} finally {
				_lockEntries.ReleaseLock ();
			}

			if (objOldEntry != null) {
				if (objOldEntry.HasAbsoluteExpiration || objOldEntry.HasSlidingExpiration)
					_objExpires.Remove (objOldEntry);

				objOldEntry.Close (CacheItemRemovedReason.Removed);
			}

			return objEntry.Item;
		}
		
		public void Insert (string strKey, object objItem) {
			Add (strKey, objItem, null, NoAbsoluteExpiration, NoSlidingExpiration,
				CacheItemPriority.Default, null, true, true);
		}

		public void Insert (string strKey, object objItem, CacheDependency objDependency) {
			Add (strKey, objItem, objDependency, NoAbsoluteExpiration, NoSlidingExpiration,
				CacheItemPriority.Default, null, true, true);
		}

		public void Insert (string strKey, object objItem, CacheDependency objDependency,
							DateTime absolutExpiration, TimeSpan slidingExpiration) {

			Add (strKey, objItem, objDependency, absolutExpiration, slidingExpiration, 
				CacheItemPriority.Default, null, true, true);
		}

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
						CacheItemPriority enumPriority, CacheItemRemovedCallback eventRemoveCallback)
		{
			Add (strKey, objItem, objDependency, absolutExpiration, slidingExpiration, 
				enumPriority, eventRemoveCallback, false, true);
		}

		internal void InsertPrivate (string strKey, object objItem, CacheDependency objDependency)
		{
			Add (strKey, objItem, objDependency, NoAbsoluteExpiration, NoSlidingExpiration,
				CacheItemPriority.Default, null, false, true);
		}

		public object Remove (string strKey) {
			return Remove (strKey, CacheItemRemovedReason.Removed);
		}

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
				_nItems--;
			}
			finally {
				_lockEntries.ReleaseWriterLock ();
			}

			if (objEntry.HasAbsoluteExpiration || objEntry.HasSlidingExpiration)
				_objExpires.Remove (objEntry);

			objEntry.Close (enumReason);
			return objEntry.Item;
		}

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
				objEntry.Expires = ticksNow + objEntry.SlidingExpiration;
			}

			return objEntry;
		}

		public int Count {
			get { return _nItems; }
		}

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

