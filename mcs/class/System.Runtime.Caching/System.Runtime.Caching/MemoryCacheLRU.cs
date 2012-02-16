//
// MemoryCacheLRU.cs
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

namespace System.Runtime.Caching
{
	// NOTE: all the public methods in this assume that the owner's write lock is held
	sealed class MemoryCacheLRU
	{
//		int trimLowerBound;
		Dictionary <int, LinkedListNode <MemoryCacheEntry>> index;
		LinkedList <MemoryCacheEntry> lru;
		MemoryCacheContainer owner;
		
		public MemoryCacheLRU (MemoryCacheContainer owner, int trimLowerBound)
		{
//			this.trimLowerBound = trimLowerBound;
			index = new Dictionary <int, LinkedListNode <MemoryCacheEntry>> ();
			lru = new LinkedList <MemoryCacheEntry> ();
			this.owner = owner;
		}

		public void Update (MemoryCacheEntry entry)
		{
			if (entry == null)
				return;

			int hash = entry.GetHashCode ();
			LinkedListNode <MemoryCacheEntry> node;
			
			if (!index.TryGetValue (hash, out node)) {
				node = new LinkedListNode <MemoryCacheEntry> (entry);
				index.Add (hash, node);
			} else {
				lru.Remove (node);
				node.Value = entry;
			}
			
			lru.AddLast (node);
		}

		public void Remove (MemoryCacheEntry entry)
		{
			if (entry == null)
				return;
			
			int hash = entry.GetHashCode ();
			LinkedListNode <MemoryCacheEntry> node;
			
			if (index.TryGetValue (hash, out node)) {
				lru.Remove (node);
				index.Remove (hash);
			}
		}

		public long Trim (long upTo)
		{
			int count = index.Count;
			if (count <= 10)
				return 0;

			// The list is used below to reproduce .NET's behavior which selects the
			// entries using the LRU order, but it removes them from the cache in the
			// MRU order
			var toremove = new List <MemoryCacheEntry> ((int)upTo);
			long removed = 0;
			MemoryCacheEntry entry;
			
			while (upTo > removed && count > 10) {
				entry = lru.First.Value;
				toremove.Insert (0, entry);
				Remove (entry);
				removed++;
				count--;
			}

			foreach (MemoryCacheEntry e in toremove)
				owner.Remove (e.Key, false);
			
			return removed;
		}
	}
}
