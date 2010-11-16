//
// A simple LRU cache used for tracking the cache items
//
// Authors:
//   Miguel de Icaza (miguel@gnome.org)
//
// Copyright 2010 Miguel de Icaza
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
// 
using System;
using System.Collections.Generic;

namespace System.Web.Caching
{
	sealed class CacheItemLRU
	{
		public delegate bool SelectItemsQualifier (CacheItem item);
		
		Dictionary<string, LinkedListNode <CacheItem>> dict;
		Dictionary<LinkedListNode<CacheItem>, string> revdict;
		LinkedList<CacheItem> list;
		Cache owner;
		
		// High/Low water mark is here to avoid situations when we hit a limit, evict an
		// entry, add another one and have to evict again because the limit was hit. When we
		// hit the high water limit, we evict until we reach the low water mark to avoid the
		// situation.
		int highWaterMark, lowWaterMark;
		bool needsEviction;

		public int Count {
			get { return dict.Count; }
		}

		public CacheItemLRU (Cache owner, int highWaterMark, int lowWaterMark)
		{
			list = new LinkedList<CacheItem> ();
			dict = new Dictionary<string, LinkedListNode<CacheItem>> (StringComparer.Ordinal);
			revdict = new Dictionary<LinkedListNode<CacheItem>, string> ();
			
			this.highWaterMark = highWaterMark;
			this.lowWaterMark = lowWaterMark;
			this.owner = owner;
		}

		public bool TryGetValue (string key, out CacheItem value)
		{
			LinkedListNode <CacheItem> item;
			
			if (dict.TryGetValue (key, out item)) {
				value = item.Value;
				return true;
			}
			value = null;
			return false;
		}

		// Must ALWAYS be called with the owner's write lock held
		public void EvictIfNecessary ()
		{
			if (!needsEviction)
				return;

			for (int i = dict.Count; i > lowWaterMark; i--) {
				var key = revdict [list.Last];

				owner.Remove (key, CacheItemRemovedReason.Underused, false, true);
			}
		}

		// Must ALWAYS be called with the owner's read lock held
		public void InvokePrivateCallbacks ()
		{
			foreach (var de in dict) {
				CacheItem item = de.Value.Value;
				if (item == null || item.Disabled)
					continue;
				
				if (item.OnRemoveCallback != null) {
					try {
						item.OnRemoveCallback (de.Key, item.Value, CacheItemRemovedReason.Removed);
					} catch {
						//TODO: anything to be done here?
					}
				}
			}
		}

		// Must ALWAYS be called with the owner's write lock held
		public List <CacheItem> SelectItems (SelectItemsQualifier qualifier)
		{
			var ret = new List <CacheItem> ();

			foreach (LinkedListNode <CacheItem> node in dict.Values) {
				CacheItem item = node.Value;
				
				if (qualifier (item))
					ret.Add (item);
			}

			return ret;
		}
		
		// Must ALWAYS be called with the owner's read lock held
		public List <CacheItem> ToList ()
		{
			var ret = new List <CacheItem> ();

			if (dict.Count == 0)
				return ret;

			foreach (LinkedListNode <CacheItem> node in dict.Values)
				ret.Add (node.Value);

			return ret;
		}
		
		public void Remove (string key)
		{
			if (key == null)
				return;
			
			LinkedListNode <CacheItem> node;
			if (!dict.TryGetValue (key, out node))
				return;

			CacheItem item = node.Value;
			dict.Remove (key);

			if (item == null || item.Priority != CacheItemPriority.NotRemovable) {
				revdict.Remove (node);
				list.Remove (node);
			}
		}
		
		public CacheItem this [string key] {
			get {
				if (key == null)
					return null;
				
				LinkedListNode<CacheItem> node;
				CacheItem item;
				
				if (dict.TryGetValue (key, out node)){
					item = node.Value;
					if (item == null || item.Priority != CacheItemPriority.NotRemovable) {
						list.Remove (node);
						list.AddFirst (node);
					}
					
					return item;
				}

				return null;
			}

			set {
				LinkedListNode<CacheItem> node;
	
				if (dict.TryGetValue (key, out node)){
					// If we already have a key, move it to the front
					list.Remove (node);
					if (value == null || value.Priority != CacheItemPriority.NotRemovable)
						list.AddFirst (node);
					else
						revdict.Remove (node);
					
					node.Value = value;
					return;
				}
				needsEviction = dict.Count >= highWaterMark;
				
				// Adding new node
				node = new LinkedListNode<CacheItem> (value);
				if (value == null || value.Priority != CacheItemPriority.NotRemovable) {
					list.AddFirst (node);
					revdict [node] = key;
				}
				
				dict [key] = node;
			}
		}
	}
}
