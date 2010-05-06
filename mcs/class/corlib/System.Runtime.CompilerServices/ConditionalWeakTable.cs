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

namespace System.Runtime.CompilerServices
{
	internal struct Ephemeron
	{
		internal object key;
		internal object value;
	}

	public sealed class ConditionalWeakTable<TKey, TValue> 
		where TKey : class
		where TValue : class
	{
		Ephemeron[] data;
		object _lock = new object ();
		int size;

		public delegate TValue CreateValueCallback (TKey key);

		public ConditionalWeakTable ()
		{
			data = new Ephemeron[16];
			GC.register_ephemeron_array (data);
		}

		public void Add (TKey key, TValue value)
		{
			TValue tmp;
			if (key == default (TKey))
				throw new ArgumentNullException ("Null key", "key");

			lock (_lock) {
				if (TryGetValue (key, out tmp))
					throw new ArgumentException ("Key already in the list", "key");
			
				if (size >= data.Length) {
					Array.Resize (ref data, size * 2);
					//Console.WriteLine ("--registering");
					GC.register_ephemeron_array (data);
				}
	
				data [size].key = key;
				data [size].value = value;
				++size;
			}
		}

		public bool Remove (TKey key)
		{
			if (key == default (TKey))
				throw new ArgumentNullException ("Null key", "key");

			lock (_lock) {
				for (int i = 0; i < size; ++i) {
					if (data [i].key == key) {
						size--;
						data [i].key = data [size].key;
						data [i].value = data [size].value;
	
						data [size].key = data [size].value = null;
						return true; 
					}
				}
			}
			return false;
		}

		public bool TryGetValue (TKey key, out TValue value)
		{
			if (key == default (TKey))
				throw new ArgumentNullException ("Null key", "key");

			value = default (TValue);
			lock (_lock) {
				for (int i = 0; i < size; ++i) {
					var k = data [i].key;
					var v = data [i].value;
					
					if (key == k) {
						value = (TValue)v;
						return true;
					}
				}
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
