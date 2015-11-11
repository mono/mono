//
// CacheEntryCollection.cs
//
// Authors:
//       Marcos Henrih (marcos.henrich@xamarin.com)
//
// Copyright 2014 Xamarin, Inc (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System.Collections;
using System.Collections.Generic;
using System.Timers;

namespace System.Runtime.Caching
{
	interface ICacheEntryHelper : IComparer<MemoryCacheEntry>
	{
		DateTime GetDateTime (MemoryCacheEntry entry);
	}

	class CacheEntryCollection
	{
		protected MemoryCacheStore store;
		private ICacheEntryHelper helper;
		private SortedSet <MemoryCacheEntry> entries;

		protected CacheEntryCollection (MemoryCacheStore store, ICacheEntryHelper helper)
		{
			this.store = store;
			this.helper = helper;
			entries = new SortedSet <MemoryCacheEntry> (helper);
		}

		protected void Add (MemoryCacheEntry entry)
		{
			lock (entries) {
				entries.Add (entry);
			}
		}

		protected void Remove (MemoryCacheEntry entry)
		{
			lock (entries) {
				entries.Remove (entry);
			}
		}

		protected int FlushItems (DateTime limit, CacheEntryRemovedReason reason, bool blockInsert, int count = int.MaxValue)
		{
			var flushedItems = 0;
			if (blockInsert)
				store.BlockInsert ();

			foreach (var entry in entries) {
				if (helper.GetDateTime (entry) > limit || flushedItems >= count)
					break;

				flushedItems++;
			}

			for (var f = 0; f < flushedItems; f++)
				store.Remove (entries.Min, null, reason);

			if (blockInsert)
				store.UnblockInsert ();

			return flushedItems;
		}
	}
}