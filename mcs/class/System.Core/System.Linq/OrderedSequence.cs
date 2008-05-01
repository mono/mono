//
// OrderedSequence.cs
//
// Authors:
//	Alejandro Serrano "Serras" (trupill@yahoo.es)
//	Marek Safar  <marek.safar@gmail.com>
//	Jb Evain  <jbevain@novell.com>
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

	class OrderedSequence<TElement, TKey> : OrderedEnumerable<TElement> {

		OrderedEnumerable<TElement> parent;

		Func<TElement, TKey> selector;
		IComparer<TKey> comparer;
		SortDirection direction;

		internal OrderedSequence (IEnumerable<TElement> source, Func<TElement, TKey> key_selector, IComparer<TKey> comparer, SortDirection direction)
			: base (source)
		{
			this.selector = key_selector;
			this.comparer = comparer ?? Comparer<TKey>.Default;
			this.direction = direction;
		}

		internal OrderedSequence (OrderedEnumerable<TElement> parent, IEnumerable<TElement> source, Func<TElement, TKey> keySelector, IComparer<TKey> comparer, SortDirection direction)
			: this (source, keySelector, comparer, direction)
		{
			this.parent = parent;
		}

		public override SortContext<TElement> CreateContext (SortContext<TElement> current)
		{
			SortContext<TElement> context = new SortSequenceContext<TElement, TKey> (selector, comparer, direction, current);

			if (parent != null)
				return parent.CreateContext (context);

			return context;
		}

		protected override IEnumerable<TElement> Sort (IEnumerable<TElement> source)
		{
			return QuickSort<TElement>.Sort (source, CreateContext (null));
		}
	}
}
