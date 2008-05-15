//
// QueryableEnumerable<TElement>.cs
//
// Authors:
//	Roei Erez (roeie@mainsoft.com)
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace System.Linq
{
	internal class QueryableEnumerable<TElement> : IQueryable<TElement>, IQueryProvider, IOrderedQueryable<TElement>
	{		
		Expression expression;

		public QueryableEnumerable (Expression expression) {
			this.expression = expression;
		}

		public Type ElementType {
			get { return expression.Type; }
		}

		public Expression Expression {
			get { return expression; }
		}

		public IQueryProvider Provider {
			get { return this; }
		}

		public System.Collections.IEnumerator GetEnumerator () 
		{			
			return ((IEnumerable<TElement>)this).GetEnumerator ();
		}

		IEnumerator<TElement> IEnumerable<TElement>.GetEnumerator () 
		{
			return Execute<IEnumerable<TElement>> (Expression).GetEnumerator ();
		}

		public IQueryable CreateQuery (System.Linq.Expressions.Expression expression) 
		{
			return (IQueryable) Activator.CreateInstance (
				typeof (QueryableEnumerable<>).MakeGenericType (expression.Type.GetGenericArguments()[0]), expression);			
		}

		public object Execute (System.Linq.Expressions.Expression expression) 
		{
			QueryableTransformer visitor = new QueryableTransformer ();
			Expression body = visitor.Transform (expression);
			LambdaExpression lambda = Expression.Lambda (body);			
			return lambda.Compile ().DynamicInvoke();
		}

		public IQueryable<TElem> CreateQuery<TElem> (System.Linq.Expressions.Expression expression) 
		{
			return new QueryableEnumerable<TElem> (expression);
		}

		public TResult Execute<TResult> (System.Linq.Expressions.Expression expression) 
		{
			QueryableTransformer visitor = new QueryableTransformer ();
			Expression body = visitor.Transform (expression);
			Expression<Func<TResult>> lambda = Expression.Lambda<Func<TResult>> (body);
			return lambda.Compile () ();
		}
	}
}
