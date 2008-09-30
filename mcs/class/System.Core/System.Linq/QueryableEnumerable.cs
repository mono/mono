//
// QueryableEnumerable<TElement>.cs
//
// Authors:
//	Roei Erez (roeie@mainsoft.com)
//	Jb Evain (jbevain@novell.com)
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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
using System.Linq;
using System.Linq.Expressions;

namespace System.Linq {

	interface IQueryableEnumerable : IQueryable {
		IEnumerable GetEnumerable ();
	}

	interface IQueryableEnumerable<TElement> : IQueryableEnumerable, IQueryable<TElement>, IOrderedQueryable<TElement> {
	}

	class QueryableEnumerable<TElement> : IQueryableEnumerable<TElement>, IQueryProvider {

		Expression expression;
		IEnumerable<TElement> enumerable;

		public Type ElementType {
			get { return typeof (TElement); }
		}

		public Expression Expression {
			get { return expression; }
		}

		public IQueryProvider Provider {
			get { return this; }
		}

		public QueryableEnumerable (IEnumerable<TElement> enumerable)
		{
			this.expression = Expression.Constant (this);
			this.enumerable = enumerable;
		}

		public QueryableEnumerable (Expression expression)
		{
			this.expression = expression;
		}

		public IEnumerable GetEnumerable ()
		{
			return enumerable;
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		public IEnumerator<TElement> GetEnumerator ()
		{
			return Execute<IEnumerable<TElement>> (expression).GetEnumerator ();
		}

		public IQueryable CreateQuery (Expression expression)
		{
			return (IQueryable) Activator.CreateInstance (
				typeof (QueryableEnumerable<>).MakeGenericType (
					expression.Type.GetFirstGenericArgument ()), expression);
		}

		public object Execute (Expression expression)
		{
			var lambda = Expression.Lambda (TransformQueryable (expression));
			return lambda.Compile ().DynamicInvoke ();
		}

		static Expression TransformQueryable (Expression expression)
		{
			return new QueryableTransformer ().Transform (expression);
		}

		public IQueryable<TElem> CreateQuery<TElem> (Expression expression)
		{
			return new QueryableEnumerable<TElem> (expression);
		}

		public TResult Execute<TResult> (Expression expression)
		{
			var lambda = Expression.Lambda<Func<TResult>> (TransformQueryable (expression));
			return lambda.Compile ().Invoke ();
		}
	}
}
