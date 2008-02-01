//
// Check.cs
//
// Author:
//   Jb Evain (jbevain@novell.com)
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

namespace System.Linq {

	static class Check {

		public static void Source (object source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
		}

		public static void Source1AndSource2 (object source1,object source2)
		{
			if (source1 == null)
				throw new ArgumentNullException ("source1");
			if (source2 == null)
				throw new ArgumentNullException ("source2");
		}

		public static void SourceAndFuncAndSelector ( object source, object func, object selector)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (func == null)
				throw new ArgumentNullException ("func");
			if (selector == null)
				throw new ArgumentNullException ("selector");
		}


		public static void SourceAndFunc (object source, object func)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (func == null)
				throw new ArgumentNullException ("func");
		}

		public static void SourceAndSelector (object source, object selector)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (selector == null)
				throw new ArgumentNullException ("selector");
		}

		public static void SourceAndPredicate (object source, object predicate)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (predicate == null)
				throw new ArgumentNullException ("predicate");
		}

		public static void FirstAndSecond (object first, object second)
		{
			if (first == null)
				throw new ArgumentNullException ("first");
			if (second == null)
				throw new ArgumentNullException ("second");
		}

		public static void SourceAndKeySelector (object source, object keySelector)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (keySelector == null)
				throw new ArgumentNullException ("keySelector");
		}

		public static void SourceAndKeyElementSelectors (object source, object keySelector, object elementSelector)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (keySelector == null)
				throw new ArgumentNullException ("keySelector");
			if (elementSelector == null)
				throw new ArgumentNullException ("elementSelector");
		}
		public static void SourceAndKeyResultSelectors (object source, object keySelector, object resultSelector)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (keySelector == null)
				throw new ArgumentNullException ("keySelector");
			if (resultSelector == null)
				throw new ArgumentNullException ("resultSelector");
		}

		public static void SourceAndCollectionSelectorAndResultSelector (object source, object collectionSelector, object resultSelector)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (collectionSelector == null)
				throw new ArgumentNullException ("collectionSelector");
			if (resultSelector == null)
				throw new ArgumentNullException ("resultSelector");
		}

		public static void SourceAndCollectionSelectors (object source, object collectionSelector, object selector)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (collectionSelector == null)
				throw new ArgumentNullException ("collectionSelector");
			if (selector == null)
				throw new ArgumentNullException ("selector");
		}

		public static void JoinSelectors (object outer, object inner, object outerKeySelector, object innerKeySelector, object resultSelector)
		{
			if (outer == null)
				throw new ArgumentNullException ("outer");
			if (inner == null)
				throw new ArgumentNullException ("inner");
			if (outerKeySelector == null)
				throw new ArgumentNullException ("outerKeySelector");
			if (innerKeySelector == null)
				throw new ArgumentNullException ("innerKeySelector");
			if (resultSelector == null)
				throw new ArgumentNullException ("resultSelector");
		}

		public static void GroupBySelectors (object source, object keySelector, object elementSelector, object resultSelector)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (keySelector == null)
				throw new ArgumentNullException ("keySelector");
			if (elementSelector == null)
				throw new ArgumentNullException ("elementSelector");
			if (resultSelector == null)
				throw new ArgumentNullException ("resultSelector");
		}
	}
}
