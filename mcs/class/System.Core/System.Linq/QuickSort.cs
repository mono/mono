//
// QuickSort.cs
//
// Authors:
//   Alejandro Serrano "Serras" (trupill@yahoo.es)
//   Marek Safar  <marek.safar@gmail.com>
//   Jb Evain (jbevain@novell.com)
//
// (C) 2007 - 2008 Novell, Inc. (http://www.novell.com)
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

namespace System.Linq {

	class QuickSort<TElement> {

		TElement [] elements;
		int [] indexes;
		SortContext<TElement> context;

		QuickSort (IEnumerable<TElement> source, SortContext<TElement> context)
		{
			this.elements = source.ToArray ();
			this.indexes = CreateIndexes (elements.Length);
			this.context = context;
		}

		static int [] CreateIndexes (int length)
		{
			var indexes = new int [length];
			for (int i = 0; i < length; i++)
				indexes [i] = i;

			return indexes;
		}

		void PerformSort ()
		{
			// If the source contains just zero or one element, there's no need to sort
			if (elements.Length <= 1)
				return;

			context.Initialize (elements);

			// Then sorts the elements according to the collected
			// key values and the selected ordering
			Array.Sort<int> (indexes, context);
		}

		public static IEnumerable<TElement> Sort (IEnumerable<TElement> source, SortContext<TElement> context)
		{
			var sorter = new QuickSort<TElement> (source, context);

			sorter.PerformSort ();

			for (int i = 0; i < sorter.elements.Length; i++)
				yield return sorter.elements [sorter.indexes [i]];
		}
	}
}
