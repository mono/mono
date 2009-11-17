//
// Lookup<TKey, TElement>.cs
//
// Authors:
//	Alejandro Serrano "Serras" (trupill@yahoo.es)
//	Marek Safar  <marek.safar@gmail.com>
//	Jb Evain  <jbevain@novell.com>
// 	Eric Maupin  <me@ermau.com>
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
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

namespace System.Linq {

	public class Lookup<TKey, TElement> : IEnumerable<IGrouping<TKey, TElement>>, ILookup<TKey, TElement> {

		IGrouping<TKey, TElement> nullGrouping;
		Dictionary<TKey, IGrouping<TKey, TElement>> groups;

		public int Count {
			get { return (nullGrouping == null) ? groups.Count : groups.Count + 1; }
		}

		public IEnumerable<TElement> this [TKey key] {
			get {
				if (key == null && nullGrouping != null)
					return nullGrouping;
				else if (key != null) {
					IGrouping<TKey, TElement> group;
					if (groups.TryGetValue (key, out group))
						return group;
				}
				
				return new TElement [0];
			}
		}

		internal Lookup (Dictionary<TKey, List<TElement>> lookup, IEnumerable<TElement> nullKeyElements)
		{
			groups = new Dictionary<TKey, IGrouping<TKey, TElement>> (lookup.Comparer);
			foreach (var slot in lookup)
				groups.Add (slot.Key, new Grouping<TKey, TElement> (slot.Key, slot.Value));
			
			if (nullKeyElements != null)
				nullGrouping = new Grouping<TKey, TElement> (default (TKey), nullKeyElements);
		}

		public IEnumerable<TResult> ApplyResultSelector<TResult> (Func<TKey, IEnumerable<TElement>, TResult> selector)
		{
			if (nullGrouping != null)
				yield return selector (nullGrouping.Key, nullGrouping);
			
			foreach (var group in groups.Values)
				yield return selector (group.Key, group);
		}

		public bool Contains (TKey key)
		{	
			return (key != null) ? groups.ContainsKey (key) : nullGrouping != null; 
		}

		public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator ()
		{
			if (nullGrouping != null)
				yield return nullGrouping;
			
			foreach (var g in groups.Values)
				yield return g;
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}
	}
}
