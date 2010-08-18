//
// ConcurrentLookup.cs
//
// Author:
//       Jérémie "Garuma" Laval <jeremie.laval@gmail.com>
//
// Copyright (c) 2010 Jérémie "Garuma" Laval
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

#if NET_4_0
using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace System.Linq.Parallel
{
	internal class ConcurrentLookup<TKey, TElement> : ILookup<TKey, TElement>
	{
		ConcurrentDictionary<TKey, IEnumerable<TElement>> dictionary;

		private class AddSlot
		{
			TElement element;

			internal AddSlot (TElement element)
			{
				this.element = element;
			}

			internal IEnumerable<TElement> AddMethod (TKey key)
			{
				List<TElement> list = new List<TElement> ();
				list.Add (element);

				return list;
			}

			internal IEnumerable<TElement> UpdateMethod (TKey key, IEnumerable<TElement> old)
			{
				ICollection<TElement> coll = (ICollection<TElement>)old;
				coll.Add (element);

				return coll;
			}
		}

		internal ConcurrentLookup (IEqualityComparer<TKey> comparer)
		{
			this.dictionary = new ConcurrentDictionary<TKey, IEnumerable<TElement>> (comparer);
		}

		internal void Add (TKey key, TElement element)
		{
			AddSlot slot = new AddSlot (element);
			dictionary.AddOrUpdate (key, slot.AddMethod, slot.UpdateMethod);
		}

		public bool Contains (TKey key)
		{
			return dictionary.ContainsKey (key);
		}

		public IEnumerable<TElement> this[TKey key] {
			get {
				return dictionary[key];
			}
		}

		public int Count {
			get {
				return dictionary.Count;
			}
		}
		
		public IList<TKey> Keys {
			get {
				return (IList<TKey>)dictionary.Keys;
			}
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return (IEnumerator)GetEnumeratorInternal ();
		}

		IEnumerator<IGrouping<TKey, TElement>> IEnumerable<IGrouping<TKey, TElement>>.GetEnumerator ()
		{
			return GetEnumeratorInternal ();
		}

		IEnumerator<IGrouping<TKey, TElement>> GetEnumeratorInternal ()
		{
			return dictionary.Select ((pair) => new ConcurrentGrouping<TKey, TElement> (pair.Key, pair.Value)).GetEnumerator ();
		}
	}
}

#endif
