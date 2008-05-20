//
// Lookup<TKey, TElement>.cs
//
// Authors:
//	Alejandro Serrano "Serras" (trupill@yahoo.es)
//	Marek Safar  <marek.safar@gmail.com>
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

		Dictionary<TKey, IGrouping<TKey, TElement>> groups;

		internal Lookup (Dictionary<TKey, List<TElement>> groups)
		{
			this.groups = new Dictionary<TKey, IGrouping<TKey, TElement>> ();
			foreach (KeyValuePair<TKey, List<TElement>> group in groups)
				this.groups.Add (group.Key, new Grouping<TKey, TElement> (group.Key, group.Value));
		}

		public IEnumerable<TResult> ApplyResultSelector<TResult> (Func<TKey, IEnumerable<TElement>, TResult> selector)
		{
			foreach (IGrouping<TKey, TElement> group in groups.Values)
				yield return selector (group.Key, group);
		}

		public int Count
		{
			get { return groups.Count; }
		}

		public bool Contains (TKey key)
		{
			return groups.ContainsKey (key);
		}

		public IEnumerable<TElement> this [TKey key]
		{
			get { return groups [key]; }
		}

		public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator ()
		{
			return groups.Values.GetEnumerator ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return groups.Values.GetEnumerator ();
		}
	}
}
