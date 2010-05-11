//
// ConditionalWeakTable.cs
//
// Author:
//   Rodrigo Kumpera (rkumpera@novell.com)
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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

#if NET_4_0 || BOOTSTRAP_NET_4_0 || MOONLIGHT
using System;
using System.Collections;

namespace System.Runtime.CompilerServices
{
	internal struct Ephemeron
	{
		internal object key;
		internal object value;
	}

	/*
	TODO:
		The runtime need to inform the table about how many entries were expired.   
		Compact the table when there are too many tombstones.
		Rehash to a smaller size when there are too few entries.
		Change rehash condition check to use non-fp code.
		Look into using quatratic probing/double hashing to reduce clustering problems.
		Make reads and non-expanding writes (add/remove) lock free.
	*/
	public sealed class ConditionalWeakTable<TKey, TValue> 
		where TKey : class
		where TValue : class
	{
		const int INITIAL_SIZE = 13;
		const float LOAD_FACTOR = 0.7f;

		Ephemeron[] data;
		object _lock = new object ();
		int size;

		public delegate TValue CreateValueCallback (TKey key);

		public ConditionalWeakTable ()
		{
			data = new Ephemeron [INITIAL_SIZE];
			GC.register_ephemeron_array (data);
		}

		/*LOCKING: _lock must be held*/
		void Rehash () {
			uint newSize = (uint)Hashtable.ToPrime ((data.Length << 1) | 1);
			//Console.WriteLine ("--- resizing from {0} to {1}", data.Length, newSize);

			Ephemeron[] tmp = new Ephemeron [newSize];
			GC.register_ephemeron_array (tmp);
			size = 0;

			for (int i = 0; i < data.Length; ++i) {
				object key = data[i].key;
				object value = data[i].value;
				if (key == null || key == GC.EPHEMERON_TOMBSTONE)
					continue;

				int len = tmp.Length;
				int idx, initial_idx;
				int free_slot = -1;
	
				idx = initial_idx = RuntimeHelpers.GetHashCode (key) % len;
	
				do {
					object k = tmp [idx].key;
	
					//keys might be GC'd during Rehash
					if (k == null || k == GC.EPHEMERON_TOMBSTONE) {
						free_slot = idx;
						break;
					}
	
					if (++idx == len) //Wrap around
						idx = 0;
				} while (idx != initial_idx);
	
				tmp [free_slot].key = key;
				tmp [free_slot].value = value;
				++size;
			}
			data = tmp;
		}


		public void Add (TKey key, TValue value)
		{
			TValue tmp;
			if (key == default (TKey))
				throw new ArgumentNullException ("Null key", "key");

			lock (_lock) {
				if (size >= data.Length * LOAD_FACTOR)
					Rehash ();

				int len = data.Length;
				int idx,initial_idx;
				int free_slot = -1;

				idx = initial_idx = RuntimeHelpers.GetHashCode (key) % len;
				do {
					object k = data [idx].key;

					if (k == null) {
						if (free_slot == -1)
							free_slot = idx;
						break;
					} else if (k == GC.EPHEMERON_TOMBSTONE && free_slot == -1) { //Add requires us to check for dupes :(
						free_slot = idx;
					} else if (k == key) { 
						throw new ArgumentException ("Key already in the list", "key");
					}

					if (++idx == len) //Wrap around
						idx = 0;
				} while (idx != initial_idx);

				data [free_slot].key = key;
				data [free_slot].value = value;
				++size;
			}
		}

		public bool Remove (TKey key)
		{
			if (key == default (TKey))
				throw new ArgumentNullException ("Null key", "key");

			lock (_lock) {
				int len = data.Length;
				int idx, initial_idx;
				idx = initial_idx = RuntimeHelpers.GetHashCode (key) % len;
				do {
					object k = data[idx].key;
					if (k == key) {
						data [idx].key = GC.EPHEMERON_TOMBSTONE;
						data [idx].value = null;
						--size;
						return true;
					}
					if (k == null)
						break;
					if (++idx == len) //Wrap around
						idx = 0;
				} while (idx != initial_idx);
			}
			return false;
		}

		public bool TryGetValue (TKey key, out TValue value)
		{
			if (key == default (TKey))
				throw new ArgumentNullException ("Null key", "key");

			value = default (TValue);
			lock (_lock) {
				int len = data.Length;
				int idx, initial_idx;
				idx = initial_idx = RuntimeHelpers.GetHashCode (key) % len;
				do {
					object k = data [idx].key;
					if (k == key) {
						value = (TValue)data [idx].value;
						return true;
					}
					if (k == null)
						break;
					if (++idx == len) //Wrap around
						idx = 0;
				} while (idx != initial_idx);
			}
			return false;
		}

		public TValue GetOrCreateValue (TKey key)
		{
			return GetValue (key, k => Activator.CreateInstance<TValue> ());
		}

		public TValue GetValue (TKey key, CreateValueCallback createValueCallback)
		{
			if (key == default (TKey))
				throw new ArgumentNullException ("Null key", "key");
			if (createValueCallback == null)
				throw new ArgumentNullException ("Null create delegate", "createValueCallback");

			TValue res;

			lock (_lock) {
				if (TryGetValue (key, out res))
					return res;
	
				res = createValueCallback (key);
				Add (key, res);
			}

			return res;
		}
	}
}
#endif
