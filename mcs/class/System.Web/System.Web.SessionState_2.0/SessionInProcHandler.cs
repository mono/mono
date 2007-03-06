//
// System.Web.SessionState.SessionInProcHandler
//
// Authors:
//	Marek Habersack <grendello@gmail.com
//
// (C) 2006 Marek Habersack
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
		public ReaderWriterLock rwlock;
		public Int32 lockId;
		public int timeout;
		
		internal InProcSessionItem ()
		{
			this.locked = false;
			this.cookieless = false;
			this.items = null;
			this.lockedTime = DateTime.MinValue;
			this.expiresAt = DateTime.MinValue;
			this.rwlock = new ReaderWriterLock ();
			this.lockId = Int32.MinValue;
			this.timeout = 0;
		}
	}
	
	internal class SessionInProcHandler : SessionStateStoreProviderBase
	{
		private const string CachePrefix = "@@@InProc@";
		private const Int32 lockAcquireTimeout = 30000;
		
		CacheItemRemovedCallback removedCB;
		//NameValueCollection privateConfig;
		SessionStateItemExpireCallback expireCallback;

		public override SessionStateStoreData CreateNewStoreData (HttpContext context, int timeout)
		{
			return new SessionStateStoreData (new SessionStateItemCollection (),
							  SessionStateUtility.GetSessionStaticObjects(context),
							  timeout);
		}

		void InsertSessionItem (InProcSessionItem item, int timeout, string id)
		{
			HttpRuntime.Cache.InsertPrivate (id,
							 item,
							 null,
							 Cache.NoAbsoluteExpiration,
							 TimeSpan.FromMinutes (timeout),
							 CacheItemPriority.AboveNormal,
							 removedCB);
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
			
			Cache cache = HttpRuntime.Cache;
			string CacheId = CachePrefix + id;
			InProcSessionItem item = cache [CacheId] as InProcSessionItem;
			
			if (item == null)
				return null;
			
			try {
				item.rwlock.AcquireReaderLock (lockAcquireTimeout);
				if (item.locked) {
					locked = true;
					lockAge = DateTime.UtcNow.Subtract (item.lockedTime);
					lockId = item.lockId;
					return null;
				}
				item.rwlock.ReleaseReaderLock ();
				if (exclusive) {
					item.rwlock.AcquireWriterLock (lockAcquireTimeout);
					item.locked = true;
					item.lockedTime = DateTime.UtcNow;
					item.lockId++;
					lockId = item.lockId;
				}
				if (item.items == null) {
					actions = SessionStateActions.InitializeItem;
					item.items = new SessionStateItemCollection ();
				}
				return new SessionStateStoreData (item.items,
								  SessionStateUtility.GetSessionStaticObjects(context),
								  item.timeout);
			} catch {
				// we want such errors to be passed to the application.
				throw;
			} finally {
				if (item.rwlock.IsReaderLockHeld) 
					item.rwlock.ReleaseReaderLock ();
				if (item.rwlock.IsWriterLockHeld) 
					item.rwlock.ReleaseWriterLock ();
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
			// nothing to do here
		}
		
		public override void ReleaseItemExclusive (HttpContext context,
							   string id,
							   object lockId)
		{
			EnsureGoodId (id, true);
			string CacheId = CachePrefix + id;
			InProcSessionItem item = HttpRuntime.Cache [CacheId] as InProcSessionItem;
			
			if (item == null || lockId == null || lockId.GetType() != typeof(Int32) || item.lockId != (Int32)lockId)
				return;

			try {
				item.rwlock.AcquireWriterLock (lockAcquireTimeout);
				item.locked = false;
			} catch {
				throw;
			} finally {
				if (item.rwlock.IsWriterLockHeld)
					item.rwlock.ReleaseWriterLock ();
			}
		}
		
		public override void RemoveItem (HttpContext context,
						 string id,
						 object lockId,
						 SessionStateStoreData item)
		{
			EnsureGoodId (id, true);
			string CacheId = CachePrefix + id;
			Cache cache = HttpRuntime.Cache;
			InProcSessionItem inProcItem = cache [CacheId] as InProcSessionItem;

			if (inProcItem == null || lockId == null || lockId.GetType() != typeof(Int32) || inProcItem.lockId != (Int32)lockId)
				return;

			try {
				inProcItem.rwlock.AcquireWriterLock (lockAcquireTimeout);
				cache.Remove (CacheId);
			} catch {
				throw;
			} finally {
				if (inProcItem.rwlock.IsWriterLockHeld)
					inProcItem.rwlock.ReleaseWriterLock ();
			}
		}
		
		public override void ResetItemTimeout (HttpContext context, string id)
		{
			EnsureGoodId (id, true);
			string CacheId = CachePrefix + id;
			Cache cache = HttpRuntime.Cache;
			InProcSessionItem item = cache [CacheId] as InProcSessionItem;
			
			if (item == null)
				return;

			try {
				item.rwlock.AcquireWriterLock (lockAcquireTimeout);
				cache.Remove (CacheId);
				InsertSessionItem (item, item.timeout, CacheId);
			} catch {
				throw;
			} finally {
				if (item.rwlock.IsWriterLockHeld)
					item.rwlock.ReleaseWriterLock ();
			}
		}
		
		public override void SetAndReleaseItemExclusive (HttpContext context,
								 string id,
								 SessionStateStoreData item,
								 object lockId,
								 bool newItem)
		{
			EnsureGoodId (id, true);
			string CacheId = CachePrefix + id;
			Cache cache = HttpRuntime.Cache;
			InProcSessionItem inProcItem = cache [CacheId] as InProcSessionItem;
			
			if (newItem || inProcItem == null) {
				inProcItem = new InProcSessionItem ();
				inProcItem.timeout = item.Timeout;
				inProcItem.expiresAt = DateTime.UtcNow.AddMinutes (item.Timeout);
				if (lockId.GetType() == typeof(Int32))
					inProcItem.lockId = (Int32)lockId;
			} else {
				if (lockId == null || lockId.GetType() != typeof(Int32) || inProcItem.lockId != (Int32)lockId)
					return;
				cache.Remove (CacheId);
			}
			
			try {
				inProcItem.rwlock.AcquireWriterLock (lockAcquireTimeout);
				inProcItem.locked = false;
				inProcItem.items = item.Items;
				InsertSessionItem (inProcItem, item.Timeout, CacheId);
			} catch {
				throw;
			} finally {
				if (inProcItem.rwlock.IsWriterLockHeld)
					inProcItem.rwlock.ReleaseWriterLock ();
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
				if (value is SessionStateStoreData)
					expireCallback (key, (SessionStateStoreData)value);
				else if (value is InProcSessionItem) {
					InProcSessionItem item = (InProcSessionItem)value;
					expireCallback (key,
							new SessionStateStoreData (
								item.items,
								SessionStateUtility.GetSessionStaticObjects (HttpContext.Current),
								item.timeout));
				} else
					expireCallback (key, null);
			}
                }
	}
}
#endif
