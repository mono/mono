//
// Queryable.cs
//
// Author:
//   Jb Evain (jbevain@novell.com)
//
// (C) 2007 Novell, Inc. (http://www.novell.com)
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
using System.Reflection;

using NUnit.Framework;

namespace MonoTests.System.Linq {

	[TestFixture]
	public class QueryableTest {

		[Test] // #701187
		public void ConcatCastedQueryables ()
		{
			var bs = new List<B> { new B (), new B () }.AsQueryable ();
			var cs = new List<C> { new C (), new C () }.AsQueryable ();

			var concat = abs.Cast<A> ().Concat (acs.Cast<A> ());
			Assert.AreEqual (4, concat.Count ());
		}

		class A { }
		class B : A { }
		class C : A { }

		[Test]
		public void TestElementType ()
		{
			var data = new int [] { 1, 2, 3 };
			var queryable = data.AsQueryable ();

			Assert.AreEqual (typeof (int), queryable.ElementType);
		}

		[Test]
		public void TestCount ()
		{
			var q = CreateQueryable<string> ();

			q.Count ();

			AssertReceived (GetMethod ("Count", 0), q);
		}

		[Test]
		public void TestCountPredicate ()
		{
			var q = CreateQueryable<string> ();

			q.Count (s => true);

			AssertReceived (GetMethod ("Count", 1), q);
		}

		public static void AssertReceived<T> (MethodInfo method, MockQuery<T> query)
		{
			Expression expression = query.MockProvider.Received;

			MethodCallExpression call = expression as MethodCallExpression;

			Assert.IsNotNull (call, "Expected a MethodCallExpression");

			MethodInfo expected = method.MakeGenericMethod (typeof (T));

			Assert.AreEqual (expected, call.Method, "Expected method: " + expected);
		}

		public static MethodInfo GetMethod (string name, int parameters)
		{
			var methods = from m in typeof (Queryable).GetMethods ()
						  where m.Name == name && m.GetParameters ().Length == parameters + 1
						  select m;

			return methods.First ();
		}

		static MockQuery<T> CreateQueryable<T> ()
		{
			return new MockQuery<T> (new MockQueryProvider ());
		}

		public class MockQueryProvider : IQueryProvider {

			Expression received;

			public Expression Received {
				get { return received; }
			}

			public IQueryable<TElement> CreateQuery<TElement> (Expression expression)
			{
				throw new NotImplementedException ();
			}

			public IQueryable CreateQuery (Expression expression)
			{
				throw new NotImplementedException ();
			}

			public TResult Execute<TResult> (Expression expression)
			{
				received = expression;

				return default (TResult);
			}

			public object Execute (Expression expression)
			{
				throw new NotImplementedException ();
			}
		}

		public class MockQuery<T> : IQueryable<T> {

			MockQueryProvider provider;
			Expression expression;

			public Type ElementType {
				get { return typeof (T); }
			}

			public Expression Expression {
				get { return expression; }
			}

			public IQueryProvider Provider {
				get { return provider; }
			}

			public MockQueryProvider MockProvider {
				get { return provider; }
			}

			public MockQuery (MockQueryProvider provider)
			{
				this.provider = provider;
				this.expression = Expression.Constant (this);
			}

			public MockQuery (MockQueryProvider provider, Expression expression)
			{
				this.provider = provider;
				this.expression = expression;
			}

			public IEnumerator<T> GetEnumerator ()
			{
				throw new NotImplementedException ();
			}

			IEnumerator IEnumerable.GetEnumerator ()
			{
				return GetEnumerator ();
			}
		}

	}
}
