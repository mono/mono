//
// EnumerableQuery<T>.cs
//
// Authors:
//  Marek Safar  <marek.safar@gmail.com>
//  Jb Evain  <jbevain@novell.com>
//
// Copyright (C) 2009 Novell, Inc (http://www.novell.com)
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

#if NET_4_0 || MOONLIGHT

using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace System.Linq
{
	public class EnumerableQuery<T> : EnumerableQuery, IOrderedQueryable<T>, IQueryable<T>, IQueryProvider
	{
		QueryableEnumerable<T> queryable;

		public Type ElementType {
			get { return queryable.ElementType; }
		}

		public Expression Expression {
			get { return queryable.Expression; }
		}

		public IQueryProvider Provider {
			get { return queryable; }
		}

		public EnumerableQuery (Expression expression)
		{
			queryable = new QueryableEnumerable<T> (expression);
		}

		public EnumerableQuery (IEnumerable<T> enumerable)
		{
			queryable = new QueryableEnumerable<T> (enumerable);
		}

		public IEnumerable GetEnumerable ()
		{
			return queryable.GetEnumerable ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return queryable.GetEnumerator ();
		}

		public IEnumerator<T> GetEnumerator ()
		{
			return queryable.GetEnumerator ();
		}

		public IQueryable CreateQuery (Expression expression)
		{
			return queryable.CreateQuery (expression);
		}

		public object Execute (Expression expression)
		{
			return queryable.Execute (expression);
		}

		public IQueryable<TElem> CreateQuery<TElem> (Expression expression)
		{
			return new EnumerableQuery<TElem> (expression);
		}

		public TResult Execute<TResult> (Expression expression)
		{
			return queryable.Execute<TResult> (expression);
		}

		public override string ToString ()
		{
			return queryable.ToString ();
		}
	}
}

#endif
