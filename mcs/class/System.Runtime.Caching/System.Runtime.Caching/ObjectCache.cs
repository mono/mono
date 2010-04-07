//
// ObjectCache.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime;

namespace System.Runtime.Caching
{
	public abstract class ObjectCache : IEnumerable<KeyValuePair<string, object>>, IEnumerable
	{
		public static readonly DateTimeOffset InfiniteAbsoluteExpiration;
		public static readonly TimeSpan NoSlidingExpiration;
		
		public static IServiceProvider Host {
			[TargetedPatchingOptOut ("Performance critical to inline this type of method across NGen image boundaries")]
			get; set;
		}
		
		public abstract DefaultCacheCapabilities DefaultCacheCapabilities { get; }
		public abstract object this [string key] { get; set; }
		public abstract string Name { get; }
		
		[TargetedPatchingOptOut ("Performance critical to inline this type of method across NGen image boundaries")]
		protected ObjectCache ()
		{
		}
		
		public virtual bool Add (CacheItem item, CacheItemPolicy policy)
		{
			throw new NotImplementedException ();
		}
		
		public virtual bool Add (string key, object value, CacheItemPolicy policy, string regionName)
		{
			throw new NotImplementedException ();
		}
		
		public virtual bool Add (string key, object value, DateTimeOffset absoluteExpiration, string regionName)
		{
			throw new NotImplementedException ();
		}
		
		public abstract CacheItem AddOrGetExisting (CacheItem value, CacheItemPolicy policy);
		public abstract object AddOrGetExisting (string key, object value, CacheItemPolicy policy, string regionName);
		public abstract object AddOrGetExisting (string key, object value, DateTimeOffset absoluteExpiration, string regionName);
		public abstract bool Contains (string key, string regionName);
		public abstract CacheEntryChangeMonitor CreateCacheEntryChangeMonitor (IEnumerable <string> keys, string regionName);
		public abstract object Get (string key, string regionName);
		public abstract CacheItem GetCacheItem (string key, string regionName);
		public abstract long GetCount (string regionName);
		protected abstract IEnumerator <KeyValuePair <string, object>> GetEnumerator ();
		public abstract IDictionary <string, object> GetValues (IEnumerable <string> keys, string regionName);
		
		[TargetedPatchingOptOut ("Performance critical to inline this type of method across NGen image boundaries")]
		public virtual IDictionary <string, object> GetValues (string regionName, params string[] keys)
		{
			throw new NotImplementedException ();
		}
		
		public abstract object Remove (string key, string regionName);
		public abstract void Set (CacheItem item, CacheItemPolicy policy);
		public abstract void Set (string key, object value, CacheItemPolicy policy, string regionName);
		public abstract void Set (string key, object value, DateTimeOffset absoluteExpiration, string regionName);
		
		[TargetedPatchingOptOut ("Performance critical to inline this type of method across NGen image boundaries")]
		IEnumerator <KeyValuePair <string,object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator ()
		{
			throw new NotImplementedException ();
		}
		
		IEnumerator IEnumerable.GetEnumerator ()
		{
			throw new NotImplementedException ();
		}
	}
}
