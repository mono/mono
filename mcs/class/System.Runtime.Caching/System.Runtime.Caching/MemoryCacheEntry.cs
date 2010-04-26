//
// MemoryCacheEntry.cs
//
// Authors:
//      Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2010 Novell, Inc. (http://novell.com/)
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.InteropServices;

namespace System.Runtime.Caching
{
	sealed class MemoryCacheEntry
	{
		object value;
		DateTimeOffset absoluteExpiration;
		Collection <ChangeMonitor> monitors;		
		CacheItemPriority priority;
		CacheEntryRemovedCallback removedCallback;
		TimeSpan slidingExpiration;
		CacheEntryUpdateCallback updateCallback;
		MemoryCache owner;
		long expiresAt;
		bool disabled;
		
		public bool Disabled {
			get { return disabled; }
			set {
				if (value) {
					this.value = null;
					this.Key = null;
					this.disabled = true;
				} else
					this.disabled = false;
			}
		}
		
		public bool IsExpired {
			get {
				if (absoluteExpiration != ObjectCache.InfiniteAbsoluteExpiration && absoluteExpiration.UtcTicks < DateTime.UtcNow.Ticks)
					return true;

				if (slidingExpiration != ObjectCache.NoSlidingExpiration && DateTime.UtcNow.Ticks - LastModified.Ticks > slidingExpiration.Ticks)
					return true;
			
				return false;
			}
		}

		public bool IsRemovable {
			get { return priority != CacheItemPriority.NotRemovable; }
		}

		public bool IsExpirable {
			get { return expiresAt > 0; }
		}
		
		public string Key {
			get; private set;
		}

		public DateTime LastModified {
			get; set;
		}

		public long ExpiresAt {
			get { return expiresAt; }
		}
		
		public object Value {
			get { return value; }
			set {
				this.value = value;
				LastModified = DateTime.UtcNow;
			}
		}

		public MemoryCacheEntry (MemoryCache owner, string key, object value)
		: this (owner, key, value, DateTimeOffset.MaxValue, null, CacheItemPriority.Default, null, TimeSpan.MaxValue, null)
		{
		}

		public MemoryCacheEntry (MemoryCache owner, string key, object value, DateTimeOffset absoluteExpiration, Collection <ChangeMonitor> monitors,
					 CacheItemPriority priority, CacheEntryRemovedCallback removedCallback, TimeSpan slidingExpiration,
					 CacheEntryUpdateCallback updateCallback)
		{
			if (value == null)
				throw new ArgumentNullException ("value");

			if (owner == null)
				throw new ArgumentNullException ("owner");

			this.owner = owner;
			this.Key = key;
			this.Value = value;
			this.absoluteExpiration = absoluteExpiration;
			this.monitors = monitors;
			this.priority = priority;
			this.removedCallback = removedCallback;
			this.slidingExpiration = slidingExpiration;
			this.updateCallback = updateCallback;
			this.LastModified = DateTime.UtcNow;

			if (absoluteExpiration != ObjectCache.InfiniteAbsoluteExpiration)
				expiresAt = absoluteExpiration.Ticks;
			else if (slidingExpiration != ObjectCache.NoSlidingExpiration)
				expiresAt = DateTime.Now.Ticks + slidingExpiration.Ticks;
			else
				expiresAt = 0;
		}

		public void Added ()
		{
			Disabled = false;
			if (monitors == null || monitors.Count == 0)
				return;

			foreach (ChangeMonitor monitor in monitors)
				monitor.NotifyOnChanged (OnMonitorChanged);
		}

		public override int GetHashCode ()
		{
			return Key.GetHashCode ();
		}
		
		void OnMonitorChanged (object state)
		{
			owner.Remove (this);
		}
		
		public void Removed (MemoryCache owner, CacheEntryRemovedReason reason)
		{
			if (removedCallback == null) {
				Disabled = true;
				return;
			}
			
			try {
				removedCallback (new CacheEntryRemovedArguments (owner, reason, new CacheItem (Key, Value)));
			} catch {
				// ignore - we don't care about the exceptions thrown inside the
				// handler
			} finally {
				Disabled = true;
			}
		}

		public void Updated (MemoryCache owner, CacheEntryRemovedReason reason)
		{
			if (updateCallback == null)
				return;
			
			try {
				var args = new CacheEntryUpdateArguments (owner, reason, Key, null);
				updateCallback (args);
			} catch {
				// ignore - we don't care about the exceptions thrown inside the
				// handler
			}
		}
		
		public void SetPolicy (CacheItemPolicy policy)
		{
			if (policy == null)
				return;
			
			absoluteExpiration = policy.AbsoluteExpiration;
			monitors = policy.ChangeMonitors;
			priority = policy.Priority;
			removedCallback = policy.RemovedCallback;
			slidingExpiration = policy.SlidingExpiration;
			updateCallback = policy.UpdateCallback;
		}
	}
}
