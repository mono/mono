//
// System.Web.SessionState.RemoteStateServer
//
// Author(s):
//  Jackson Harper (jackson@ximian.com)
//  Gonzalo Paniagua (gonzalo@ximian.com)
//  Marek Habersack (grendello@gmail.com)
//
// (C) 2003-2006 Novell, Inc (http://www.novell.com)
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
using System.Web.Caching;

namespace System.Web.SessionState {
	class LockableStateServerItem
	{
		public StateServerItem item;
		public ReaderWriterLock rwlock;

		public LockableStateServerItem (StateServerItem item)
		{
			this.item = item;
			this.rwlock = new ReaderWriterLock ();
		}
	}
	
	internal class RemoteStateServer : MarshalByRefObject {
		const Int32 lockAcquireTimeout = 30000;
		Cache cache;
		
		internal RemoteStateServer ()
		{
			cache = new Cache ();
		}

		void Insert (string id, LockableStateServerItem item)
		{
			cache.Insert (id, item, null, Cache.NoAbsoluteExpiration, new TimeSpan (0, item.item.Timeout, 0));
		}

		LockableStateServerItem Retrieve (string id)
		{
			return cache [id] as LockableStateServerItem;
		}

		internal void CreateUninitializedItem (string id, int timeout)
		{
			StateServerItem item = new StateServerItem (timeout);
			item.Action = SessionStateActions.InitializeItem;
			LockableStateServerItem cacheItem = new LockableStateServerItem (item);
			Insert (id, cacheItem);
		}
		
		internal StateServerItem GetItem (string id,
						  out bool locked,
						  out TimeSpan lockAge,
						  out object lockId,
						  out SessionStateActions actions,
						  bool exclusive)
		{
			locked = false;
			lockAge = TimeSpan.MinValue;
			lockId = Int32.MinValue;
			actions = SessionStateActions.None;
			
			LockableStateServerItem item = Retrieve (id);
			if (item == null || item.item.IsAbandoned ())
				return null;
			
			try {
				item.rwlock.AcquireReaderLock (lockAcquireTimeout);
				if (item.item.Locked) {
					locked = true;
					lockAge = DateTime.UtcNow.Subtract (item.item.LockedTime);
					lockId = item.item.LockId;
					return null;
				}

				item.rwlock.ReleaseReaderLock ();
				if (exclusive) {
					item.rwlock.AcquireWriterLock (lockAcquireTimeout);
					item.item.Locked = true;
					item.item.LockedTime = DateTime.UtcNow;
					item.item.LockId++;
					lockId = item.item.LockId;
				}
			} catch {
				throw;
			} finally {
				if (item.rwlock.IsReaderLockHeld)
					item.rwlock.ReleaseReaderLock ();
				
				if (item.rwlock.IsWriterLockHeld)
					item.rwlock.ReleaseWriterLock ();
			}
			
			actions = item.item.Action;
			return item.item;
		}

		internal void Remove (string id, object lockid)
		{
			cache.Remove (id);
		}

		internal void ResetItemTimeout (string id)
		{
			LockableStateServerItem item = Retrieve (id);
			if (item == null)
				return;
			item.item.Touch ();
		}

		internal void ReleaseItemExclusive (string id, object lockId)
		{
			LockableStateServerItem item = Retrieve (id);
			if (item == null || item.item.LockId != (Int32)lockId)
				return;
			
			try {
				item.rwlock.AcquireWriterLock (lockAcquireTimeout);
				item.item.Locked = false;
			} catch {
				throw;
			} finally {
				if (item.rwlock.IsWriterLockHeld)
					item.rwlock.ReleaseWriterLock ();
			}
		}
		
		internal void SetAndReleaseItemExclusive (string id, byte [] collection_data, byte [] sobjs_data,
							  object lockId, int timeout, bool newItem)
		{
			LockableStateServerItem item = Retrieve (id);
			bool fresh = false;
			
			if (newItem || item == null) {
				item = new LockableStateServerItem (new StateServerItem (collection_data, sobjs_data, timeout));
				item.item.LockId = (Int32)lockId;
				fresh = true;
			} else {
				if (item.item.LockId != (Int32)lockId)
					return;
				Remove (id, lockId);
			}

			try {
				item.rwlock.AcquireWriterLock (lockAcquireTimeout);
				item.item.Locked = false;
				if (!fresh) {
					item.item.CollectionData = collection_data;
					item.item.StaticObjectsData = sobjs_data;
				}
				Insert (id, item);
			} catch {
				throw;
			} finally {
				if (item.rwlock.IsWriterLockHeld)
					item.rwlock.ReleaseWriterLock ();
			}
		}
		
		public override object InitializeLifetimeService ()
		{
			return null; // just in case...
		}
	}
}
