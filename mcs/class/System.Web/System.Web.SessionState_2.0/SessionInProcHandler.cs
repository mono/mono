//
// System.Web.SessionState.SessionInProcHandler
//
// Authors:
//	Marek Habersack <grendello@gmail.com>
//
// (C) 2006 Marek Habersack
// (C) 2010 Novell, Inc (http://novell.com/)
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
#if NET_2_0
using System;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Web.Caching;
using System.Web.Configuration;
using System.Threading;

namespace System.Web.SessionState
{
	internal sealed class InProcSessionItem
	{
		public bool locked;
		public bool cookieless;
		public ISessionStateItemCollection items;
		public DateTime lockedTime;
		public DateTime expiresAt;
		public ReaderWriterLockSlim rwlock;
		public Int32 lockId;
		public int timeout;
		public bool resettingTimeout;
		public HttpStaticObjectsCollection staticItems;
		
		internal InProcSessionItem ()
		{
			this.locked = false;
			this.cookieless = false;
			this.items = null;
			this.staticItems = null;
			this.lockedTime = DateTime.MinValue;
			this.expiresAt = DateTime.MinValue;
			this.rwlock = new ReaderWriterLockSlim ();
			this.lockId = Int32.MinValue;
			this.timeout = 0;
			this.resettingTimeout = false;
		}

		public void Dispose ()
		{
			if (rwlock != null) {
				rwlock.Dispose ();
				rwlock = null;
			}
			staticItems = null;
			if (items != null)
				items.Clear ();
			items = null;
		}
		
		~InProcSessionItem ()
		{
			Dispose ();
		}
	}
	
	internal class SessionInProcHandler : SessionStateStoreProviderBase
	{
		const string CachePrefix = "@@@InProc@";
		const int CachePrefixLength = 10;
		
		const Int32 lockAcquireTimeout = 30000;
		
		CacheItemRemovedCallback removedCB;
		//NameValueCollection privateConfig;
		SessionStateItemExpireCallback expireCallback;
		HttpStaticObjectsCollection staticObjects;
		
		public override SessionStateStoreData CreateNewStoreData (HttpContext context, int timeout)
		{
			return new SessionStateStoreData (new SessionStateItemCollection (),
							  staticObjects, timeout);
		}

		void InsertSessionItem (InProcSessionItem item, int timeout, string id)
		{
			if (item == null || String.IsNullOrEmpty (id))
				return;

			HttpRuntime.InternalCache.Insert (id,
							  item,
							  null,
							  Cache.NoAbsoluteExpiration,
							  TimeSpan.FromMinutes (timeout),
							  CacheItemPriority.AboveNormal,
							  removedCB);
		}

		void UpdateSessionItemTimeout (int timeout, string id)
		{
			if (String.IsNullOrEmpty (id))
				return;

			HttpRuntime.InternalCache.SetItemTimeout (id, Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes (timeout), true);
		}
		
		public override void CreateUninitializedItem (HttpContext context, string id, int timeout)
		{
			EnsureGoodId (id, true);
			InProcSessionItem item = new InProcSessionItem ();
			item.expiresAt = DateTime.UtcNow.AddMinutes (timeout);
			item.timeout = timeout;
			InsertSessionItem (item, timeout, CachePrefix + id);
		}
		
		public override void Dispose ()
		{
		}
		
		public override void EndRequest (HttpContext context)
		{
			if (staticObjects != null) {
				staticObjects.GetObjects ().Clear ();
				staticObjects = null;
			}
		}

		SessionStateStoreData GetItemInternal (HttpContext context,
						       string id,
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

			if (id == null)
				return null;
			
			Cache cache = HttpRuntime.InternalCache;
			string CacheId = CachePrefix + id;
			InProcSessionItem item = cache [CacheId] as InProcSessionItem;
			
			if (item == null)
				return null;

			bool readLocked = false, writeLocked = false;
			try {
				if (item.rwlock.TryEnterUpgradeableReadLock (lockAcquireTimeout))
					readLocked = true;
				else
					throw new ApplicationException ("Failed to acquire lock");
				
				if (item.locked) {
					locked = true;
					lockAge = DateTime.UtcNow.Subtract (item.lockedTime);
					lockId = item.lockId;
					return null;
				}
				
				if (exclusive) {
					if (item.rwlock.TryEnterWriteLock (lockAcquireTimeout))
						writeLocked = true;
					else
						throw new ApplicationException ("Failed to acquire lock");
					item.locked = true;
					item.lockedTime = DateTime.UtcNow;
					item.lockId++;
					lockId = item.lockId;
				}
				if (item.items == null) {
					actions = SessionStateActions.InitializeItem;
					item.items = new SessionStateItemCollection ();
				}
				if (item.staticItems == null)
					item.staticItems = staticObjects;
				
				return new SessionStateStoreData (item.items,
								  item.staticItems,
								  item.timeout);
			} catch {
				// we want such errors to be passed to the application.
				throw;
			} finally {
				if (writeLocked)
					item.rwlock.ExitWriteLock ();
				if (readLocked)
					item.rwlock.ExitUpgradeableReadLock ();
			}
		}
		
		public override SessionStateStoreData GetItem (HttpContext context,
							       string id,
							       out bool locked,
							       out TimeSpan lockAge,
							       out object lockId,
							       out SessionStateActions actions)
		{
			EnsureGoodId (id, false);
			return GetItemInternal (context, id, out locked, out lockAge, out lockId, out actions, false);
		}
		
		public override SessionStateStoreData GetItemExclusive (HttpContext context,
									string id,
									out bool locked,
									out TimeSpan lockAge,
									out object lockId,
									out SessionStateActions actions)
		{
			EnsureGoodId (id, false);
			return GetItemInternal (context, id, out locked, out lockAge, out lockId, out actions, true);
		}

		public override void Initialize (string name, NameValueCollection config)
		{
			if (String.IsNullOrEmpty (name))
				name = "Session InProc handler";
			removedCB = new CacheItemRemovedCallback (OnSessionRemoved);
			//privateConfig = config;
			base.Initialize (name, config);
		}
		
		public override void InitializeRequest (HttpContext context)
		{
			staticObjects = HttpApplicationFactory.ApplicationState.SessionObjects.Clone ();
		}
		
		public override void ReleaseItemExclusive (HttpContext context,
							   string id,
							   object lockId)
		{
			EnsureGoodId (id, true);
			string CacheId = CachePrefix + id;
			InProcSessionItem item = HttpRuntime.InternalCache [CacheId] as InProcSessionItem;
			
			if (item == null || lockId == null || lockId.GetType() != typeof(Int32) || item.lockId != (Int32)lockId)
				return;

			bool locked = false;
			ReaderWriterLockSlim itemLock = null;
			
			try {
				itemLock = item.rwlock;
				if (itemLock != null && itemLock.TryEnterWriteLock (lockAcquireTimeout))
					locked = true;
				else
					throw new ApplicationException ("Failed to acquire lock");
				item.locked = false;
			} catch {
				throw;
			} finally {
				if (locked && itemLock != null)
					itemLock.ExitWriteLock ();
			}
		}
		
		public override void RemoveItem (HttpContext context,
						 string id,
						 object lockId,
						 SessionStateStoreData item)
		{
			EnsureGoodId (id, true);
			string CacheId = CachePrefix + id;
			Cache cache = HttpRuntime.InternalCache;
			InProcSessionItem inProcItem = cache [CacheId] as InProcSessionItem;

			if (inProcItem == null || lockId == null || lockId.GetType() != typeof(Int32) || inProcItem.lockId != (Int32)lockId)
				return;

			bool locked = false;
			ReaderWriterLockSlim itemLock = null;
			
			try {
				itemLock = inProcItem.rwlock;
				if (itemLock != null && itemLock.TryEnterWriteLock (lockAcquireTimeout))
					locked = true;
				else
					throw new ApplicationException ("Failed to acquire lock after");
				cache.Remove (CacheId);
			} catch {
				throw;
			} finally {
				if (locked)
					itemLock.ExitWriteLock ();
			}
		}
		
		public override void ResetItemTimeout (HttpContext context, string id)
		{
			EnsureGoodId (id, true);
			string CacheId = CachePrefix + id;
			Cache cache = HttpRuntime.InternalCache;
			InProcSessionItem item = cache [CacheId] as InProcSessionItem;
			
			if (item == null)
				return;

			bool locked = false;
			ReaderWriterLockSlim itemLock = null;

			try {
				itemLock = item.rwlock;
				if (itemLock != null && itemLock.TryEnterWriteLock (lockAcquireTimeout))
					locked = true;
				else
					throw new ApplicationException ("Failed to acquire lock after");
				item.resettingTimeout = true;
				UpdateSessionItemTimeout (item.timeout, CacheId);
			} catch {
				throw;
			} finally {
				if (locked && itemLock != null)
					itemLock.ExitWriteLock ();
			}
		}

		/* In certain situations the 'item' parameter passed to SetAndReleaseItemExclusive
		   may be null. The issue was reported in bug #333898, but the reporter cannot
		   provide a test case that triggers the issue. Added work around the problem
		   in the way that should have the least impact on the rest of the code. If 'item'
		   is null, then the new session item is created without the items and staticItems
		   collections - they will be initialized to defaults when retrieving the session
		   item. This is not a correct fix, but since there is no test case this is the best
		   what can be done right now.
		*/
		public override void SetAndReleaseItemExclusive (HttpContext context,
								 string id,
								 SessionStateStoreData item,
								 object lockId,
								 bool newItem)
		{
			EnsureGoodId (id, true);
			string CacheId = CachePrefix + id;
			Cache cache = HttpRuntime.InternalCache;
			InProcSessionItem inProcItem = cache [CacheId] as InProcSessionItem;
			ISessionStateItemCollection itemItems = null;
			int itemTimeout = 20;
			HttpStaticObjectsCollection itemStaticItems = null;

			if (item != null) {
				itemItems = item.Items;
				itemTimeout = item.Timeout;
				itemStaticItems = item.StaticObjects;
			}
			
			if (newItem || inProcItem == null) {
				inProcItem = new InProcSessionItem ();
				inProcItem.timeout = itemTimeout;
				inProcItem.expiresAt = DateTime.UtcNow.AddMinutes (itemTimeout);
				if (lockId.GetType() == typeof(Int32))
					inProcItem.lockId = (Int32)lockId;
			} else {
				if (lockId == null || lockId.GetType() != typeof(Int32) || inProcItem.lockId != (Int32)lockId)
					return;
				inProcItem.resettingTimeout = true;
				cache.Remove (CacheId);
			}

			bool locked = false;
			ReaderWriterLockSlim itemLock = null;
			try {
				itemLock = inProcItem.rwlock;
				if (itemLock != null && itemLock.TryEnterWriteLock (lockAcquireTimeout))
					locked = true;
				else if (itemLock != null)
					throw new ApplicationException ("Failed to acquire lock");
				if (inProcItem.resettingTimeout)
				 	UpdateSessionItemTimeout (itemTimeout, CacheId);
				else {
					inProcItem.locked = false;
					inProcItem.items = itemItems;
					inProcItem.staticItems = itemStaticItems;
					InsertSessionItem (inProcItem, itemTimeout, CacheId);
				}
			} catch {
				throw;
			} finally {
				if (locked && itemLock != null)
					itemLock.ExitWriteLock ();
			}
		}
		
		public override bool SetItemExpireCallback (SessionStateItemExpireCallback expireCallback)
		{
			this.expireCallback = expireCallback;
			return true;
		}

		void EnsureGoodId (string id, bool throwOnNull)
		{
			if (id == null)
				if (throwOnNull)
					throw new HttpException ("Session ID is invalid");
				else
					return;
			
			if (id.Length > SessionIDManager.SessionIDMaxLength)
				throw new HttpException ("Session ID too long");
		}

		void OnSessionRemoved (string key, object value, CacheItemRemovedReason reason)
                {
			if (expireCallback != null) {
				if (key.StartsWith (CachePrefix, StringComparison.OrdinalIgnoreCase))
					key = key.Substring (CachePrefixLength);
				
				if (value is SessionStateStoreData)
					expireCallback (key, (SessionStateStoreData)value);
				else if (value is InProcSessionItem) {
					InProcSessionItem item = (InProcSessionItem)value;
					if (item.resettingTimeout) {
						item.resettingTimeout = false;
						return;
					}
					
					expireCallback (key,
							new SessionStateStoreData (
								item.items,
								item.staticItems,
								item.timeout));
					item.Dispose ();
				} else
					expireCallback (key, null);
			} else if (value is InProcSessionItem) {
				InProcSessionItem item = (InProcSessionItem)value;
				if (item.resettingTimeout) {
					item.resettingTimeout = false;
					return;
				}
				
				item.Dispose ();
			}
                }
	}
}
#endif
