//
// A simple LRU cache
//
// Authors:
//   Miguel de Icaza (miguel@gnome.org)
//   Andres G. Aragoneses (andres@7digital.com)
//
// Copyright 2010 Miguel de Icaza
// Copyright 2013 7digital Media Ltd.
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

namespace System.Web.Configuration {

	class LruCache<TKey, TValue> {
		Dictionary<TKey, LinkedListNode <TValue>> dict;
		Dictionary<LinkedListNode<TValue>, TKey> revdict;
		LinkedList<TValue> list;
		int entry_limit = DEFAULT_ENTRY_LIMIT;

		const int DEFAULT_ENTRY_LIMIT = 100;
		const string CACHE_SIZE_OVERRIDING_KEY = "MONO_ASPNET_WEBCONFIG_CACHESIZE";

		bool eviction_warning_shown;
		int evictions;
		bool size_overriden;

		public LruCache ()
		{
			int override_limit;
			if (int.TryParse (Environment.GetEnvironmentVariable (CACHE_SIZE_OVERRIDING_KEY), out override_limit)) {
				size_overriden = true;
				entry_limit = override_limit;
				Console.WriteLine ("WebConfigurationManager's LRUcache Size overriden to: {0} (via {1})", override_limit, CACHE_SIZE_OVERRIDING_KEY);
			}
			dict = new Dictionary<TKey, LinkedListNode<TValue>> ();
			revdict = new Dictionary<LinkedListNode<TValue>, TKey> ();
			list = new LinkedList<TValue> ();
		}

		//for debugging: public int Count { get { return dict.Count; } }

		void Evict ()
		{
			var last = list.Last;
			if (last == null)
				return;

			var key = revdict [last];

			dict.Remove (key);
			revdict.Remove (last);
			list.RemoveLast ();
			DisposeValue (last.Value);
			evictions++;

			if (!eviction_warning_shown && (evictions >= entry_limit)) {
				Console.WriteLine ("WARNING: WebConfigurationManager's LRUcache evictions count reached its max size");
				eviction_warning_shown = true;
				if (!size_overriden)
					Console.WriteLine ("Cache Size: {0} (overridable via {1})", entry_limit, CACHE_SIZE_OVERRIDING_KEY);
			}
		}

		public void Clear ()
		{
			foreach (var element in list) {
				DisposeValue (element);
			}

			dict.Clear ();
			revdict.Clear ();
			list.Clear ();
			eviction_warning_shown = false;
			evictions = 0;
		}

		void DisposeValue (TValue value)
		{
			if (value is IDisposable) {
				((IDisposable)value).Dispose ();
			}
		}

		public bool TryGetValue (TKey key, out TValue value)
		{
			LinkedListNode<TValue> node;

			if (dict.TryGetValue (key, out node)){
				list.Remove (node);
				list.AddFirst (node);

				value = node.Value;
				return true;
			}
			value = default (TValue);
			return false;
		}

		public void Add (TKey key, TValue value)
		{
			LinkedListNode<TValue> node;

			if (dict.TryGetValue (key, out node)){

				// If we already have a key, move it to the front
				list.Remove (node);
				list.AddFirst (node);

				// Remove the old value
				DisposeValue (node.Value);

				node.Value = value;
				return;
			}

			if (dict.Count >= entry_limit)
				Evict ();

			// Adding new node
			node = new LinkedListNode<TValue> (value);
			list.AddFirst (node);
			dict [key] = node;
			revdict [node] = key;
		}

		public override string ToString ()
		{
			return "LRUCache dict={0} revdict={1} list={2}";
		}
	}
}
