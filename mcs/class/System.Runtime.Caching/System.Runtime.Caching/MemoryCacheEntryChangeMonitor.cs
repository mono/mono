//
// MemoryCacheEntryChangeMonitor.cs
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
using System.Collections.ObjectModel;
using System.Text;

namespace System.Runtime.Caching
{
	sealed class MemoryCacheEntryChangeMonitor : CacheEntryChangeMonitor
	{
		ReadOnlyCollection <string> cacheKeys;
		DateTimeOffset lastModified;
//		MemoryCache owner;
		string uniqueId;
		
		public override ReadOnlyCollection <string> CacheKeys {
			get { return cacheKeys; }
		}

		public override DateTimeOffset LastModified {
			get { return lastModified; }
		}

		public override string RegionName {
			get { return null; }
		}

		public override string UniqueId {
			get { return uniqueId; }
		}
		
		public MemoryCacheEntryChangeMonitor (MemoryCache owner, IEnumerable <string> keys)
		{
//			this.owner = owner;
			this.lastModified = DateTimeOffset.MinValue;

			MemoryCacheEntry mce;
			var sb = new StringBuilder ();
			var list = new List <string> ();
			foreach (string key in keys) {
				mce = owner.GetEntry (key);
				list.Add (key);
#if true
				// LAMESPEC: this is what's happening
				DateTimeOffset modtime;
				modtime = new DateTimeOffset (mce != null ? mce.LastModified : DateTime.MinValue);
				if (this.lastModified < modtime)
					this.lastModified = modtime;
#else
				// LAMESPEC: this is what is supposed to be happening
				if (mce == null) {
					OnChanged (null);
					sb.Append ("{0}{1:x}", key, DateTime.MinValue.Ticks);
					continue;
				}
				
				DateTime modtime = mce.LastModified;
				if (this.lastModtime < modtime)
					this.lastModtime = new DateTimeOffset (modtime);
#endif
				sb.AppendFormat ("{0}{1:X}", key, modtime.Ticks);
			}
			this.uniqueId = sb.ToString ();
			this.cacheKeys = new ReadOnlyCollection <string> (list);

			// TODO: hook up to change events on MemoryCache
		}

		protected override void Dispose (bool disposing)
		{
		}
	}
}
