//
// Object.cs: C# extension methods on object.
//
// Author:
//   Jonathan Pryor  <jpryor@novell.com>
//   leppie  (http://xacc.wordpress.com/)
//
// Copyright (c) 2008-2009 Novell, Inc. (http://www.novell.com)
// Copyright (c) 2009 leppie (http://xacc.wordpress.com/)
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
using System.Linq;
using System.Linq.Expressions;

namespace Mono.Rocks {

	static class Check {
		public static void ChildrenSelector (object childrenSelector)
		{
			if (childrenSelector == null)
				throw new ArgumentNullException ("childrenSelector");
		}

		public static void Destination (object destination)
		{
			if (destination == null)
				throw new ArgumentNullException ("destination");
		}

		public static void Self (object self)
		{
			if (self == null)
				throw new ArgumentNullException ("self");
		}

		public static void ValueSelector (object valueSelector)
		{
			if (valueSelector == null)
				throw new ArgumentNullException ("valueSelector");
		}
	}

	public static class ObjectRocks {

		#region Tree Traversal Methods

		/*
		 * Tree Traversal Methods courtesy of:
		 * http://xacc.wordpress.com/2009/03/05/tree-traversal-extension-methods/
		 */

		public static IEnumerable<TResult> TraverseDepthFirst<TSource, TResult>(
				this TSource self,
				Func<TSource, TResult> valueSelector,
				Func<TSource, IEnumerable<TSource>> childrenSelector)
		{
			return self.TraverseDepthFirstWithParent (valueSelector, childrenSelector)
				.Select(x => x.Value);
		}

		public static IEnumerable<KeyValuePair<TSource, TResult>> TraverseDepthFirstWithParent<TSource, TResult>(
				this TSource self,
				Func<TSource, TResult> valueSelector,
				Func<TSource, IEnumerable<TSource>> childrenSelector)
		{
			return self.TraverseDepthFirstWithParent (default (TSource), valueSelector, childrenSelector);
		}

		static IEnumerable<KeyValuePair<TSource, TResult>> TraverseDepthFirstWithParent<TSource, TResult>(
				this TSource self,
				TSource parent,
				Func<TSource, TResult> valueSelector,
				Func<TSource, IEnumerable<TSource>> childrenSelector)
		{
			Check.Self (self);
			Check.ValueSelector (valueSelector);
			Check.ChildrenSelector (childrenSelector);

			return CreateTraverseDepthFirstWithParentIterator (self, parent, valueSelector, childrenSelector);
		}

		static IEnumerable<KeyValuePair<TSource, TResult>> CreateTraverseDepthFirstWithParentIterator<TSource, TResult>(
				this TSource self,
				TSource parent,
				Func<TSource, TResult> valueSelector,
				Func<TSource, IEnumerable<TSource>> childrenSelector)
		{
			yield return new KeyValuePair<TSource, TResult>(parent, valueSelector (self));

			foreach (var c in childrenSelector (self))
			{
				foreach (var item in c.TraverseDepthFirstWithParent(c, valueSelector, childrenSelector))
				{
					yield return item;
				}
			}
		}
		#endregion
	}
}

