//
// CacheEntryUpdateArguments.cs
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

namespace System.Runtime.Caching
{
	public class CacheEntryUpdateArguments
	{
		public string Key { get; private set; }
		public string RegionName { get; private set; }
		public CacheEntryRemovedReason RemovedReason { get; private set; }
		public ObjectCache Source { get; private set; }
		public CacheItem UpdatedCacheItem { get; set; }
		public CacheItemPolicy UpdatedCacheItemPolicy { get; set; }
		
		public CacheEntryUpdateArguments (ObjectCache source, CacheEntryRemovedReason reason, string key, string regionName)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			if (key == null)
				throw new ArgumentNullException ("key");

			this.Key = key;
			this.RegionName = regionName;
			this.RemovedReason = reason;
			this.Source = source;	
		}
	}
}
