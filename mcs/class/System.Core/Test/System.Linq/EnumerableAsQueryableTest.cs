//
// EnumerableAsQueryableTest.cs
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
using NUnit.Framework;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.Collections;

namespace MonoTests.System.Linq {

	[TestFixture]
	public class EnumerableAsQueryableTest {

		int [] _array;
		IQueryable<int> _src;

		[SetUp]
		public void MyTestCleanup ()
		{
			_array = new int [] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
			_src = _array.AsQueryable<int> ();
		}

		[Test]
		public void NewQueryableExpression ()
		{
			var queryable = _array.AsQueryable ();
			var expression = queryable.Expression;

			Assert.AreEqual (ExpressionType.Constant, expression.NodeType);

			var constant = (ConstantExpression) expression;

			Assert.AreEqual (queryable, constant.Value);
		}

		[Test]
		public void Aggregate ()
		{
		    Assert.AreEqual (_src.Aggregate<int> ((n, m) => n + m), _array.Aggregate<int> ((n, m) => n + m));
		}

		[Test]
		public void All ()
		{
		    Assert.AreEqual (_src.All<int> ((n) => n < 11), _array.All<int> ((n) => n < 11));
		    Assert.AreEqual (_src.All<int> ((n) => n < 10), _array.All<int> ((n) => n < 10));
		}

		[Test]
		public void Any ()
		{
			Assert.AreEqual (_src.Any<int> (i => i > 5), _array.Any<int> (i => i > 5));
		}

		[Test]
		public void Average ()
		{
			Assert.AreEqual (_src.Average<int> ((n) => 11), _array.Average<int> ((n) => 11));
		}

		[Test]
		public void Concat ()
		{
			Assert.AreEqual (_src.Concat<int> (_src).Count (), _array.Concat<int> (_src).Count ());
		}

		[Test]
		public void Contains ()
		{

			for (int i = 1; i < 20; ++i)
				Assert.AreEqual (_src.Contains<int> (i), _array.Contains<int> (i));
		}

		[Test]
		public void Count ()
		{
			Assert.AreEqual (_src.Count<int> (), _array.Count<int> ());
		}

		[Test]
		public void Distinct ()
		{
			Assert.AreEqual (_src.Distinct<int> ().Count (), _array.Distinct<int> ().Count ());
			Assert.AreEqual (_src.Distinct<int> (new CustomEqualityComparer ()).Count (), _array.Distinct<int> (new CustomEqualityComparer ()).Count ());
		}

		[Test]
		public void ElementAt ()
		{
			for (int i = 0; i < 10; ++i)
				Assert.AreEqual (_src.ElementAt<int> (i), _array.ElementAt<int> (i));
		}

		[Test]
		public void ElementAtOrDefault ()
		{
			for (int i = 0; i < 10; ++i)
				Assert.AreEqual (_src.ElementAtOrDefault<int> (i), _array.ElementAtOrDefault<int> (i));
			Assert.AreEqual (_src.ElementAtOrDefault<int> (100), _array.ElementAtOrDefault<int> (100));
		}

		[Test]
		public void Except ()
		{
			int [] except = { 1, 2, 3 };
			Assert.AreEqual (_src.Except<int> (except.AsQueryable ()).Count (), _array.Except<int> (except).Count ());
		}

		[Test]
		public void First ()
		{
			Assert.AreEqual (_src.First<int> (), _array.First<int> ());
		}

		[Test]
		public void FirstOrDefault ()
		{
			Assert.AreEqual (_src.FirstOrDefault<int> ((n) => n > 5), _array.FirstOrDefault<int> ((n) => n > 5));
			Assert.AreEqual (_src.FirstOrDefault<int> ((n) => n > 10), _array.FirstOrDefault<int> ((n) => n > 10));
		}

		[Test]
		public void GroupBy ()
		{
			IQueryable<IGrouping<bool, int>> grouping = _src.GroupBy<int, bool> ((n) => n > 5);
			Assert.AreEqual (grouping.Count(), 2);
			foreach (IGrouping<bool, int> group in grouping)
			{
				Assert.AreEqual(group.Count(), 5);
			}
		}

		[Test]
		public void Intersect ()
		{
			int [] subset = { 1, 2, 3 };
			int[] intersection = _src.Intersect<int> (subset.AsQueryable()).ToArray();
			Assert.AreEqual (subset, intersection);
		}

		[Test]
		public void Last ()
		{
			Assert.AreEqual (_src.Last<int> ((n) => n > 1), _array.Last<int> ((n) => n > 1));
		}

		[Test]
		public void LastOrDefault ()
		{
			Assert.AreEqual (_src.LastOrDefault<int> (), _array.LastOrDefault<int> ());
		}

		[Test]
		public void LongCount ()
		{
			Assert.AreEqual (_src.LongCount<int> (), _array.LongCount<int> ());
		}

		[Test]
		public void Max ()
		{
			Assert.AreEqual (_src.Max<int> (), _array.Max<int> ());
		}

		[Test]
		public void Min ()
		{
			Assert.AreEqual (_src.Min<int> (), _array.Min<int> ());
		}

		[Test]
		public void OfType ()
		{
			Assert.AreEqual (_src.OfType<int> ().Count (), _array.OfType<int> ().Count ());
		}

		[Test]
		public void OrderBy ()
		{
			int [] arr1 = _array.OrderBy<int, int> ((n) => n * -1).ToArray ();
			int [] arr2 = _src.OrderBy<int, int> ((n) => n * -1).ToArray ();
			Assert.AreEqual (arr1, arr2);
		}

		[Test]
		public void OrderByDescending ()
		{
			int [] arr1 = _array.OrderBy<int, int> ((n) => n).ToArray ();
			int [] arr2 = _src.OrderBy<int, int> ((n) => n).ToArray ();
			Assert.AreEqual (arr1, arr2);
		}

		[Test]
		public void Reverse ()
		{
			int [] arr1 = _array.Reverse<int> ().Reverse ().ToArray ();
			int [] arr2 = _src.Reverse<int> ().Reverse ().ToArray ();
			Assert.AreEqual (arr1, arr2);
		}

		[Test]
		public void Select ()
		{
			int [] arr1 = _array.Select<int, int> ((n) => n - 1).ToArray ();
			int [] arr2 = _src.Select<int, int> ((n) => n - 1).ToArray ();
			Assert.AreEqual (arr1, arr2);
		}

		[Test]
		public void SelectMany ()
		{
			int [] arr1 = _array.SelectMany<int, int> ((n) => new int [] { n, n, n }).ToArray ();
			int [] arr2 = _src.SelectMany<int, int> ((n) => new int [] { n, n, n }).ToArray ();
			Assert.AreEqual (arr1, arr2);
		}

		[Test]
		public void SequenceEqual ()
		{
			Assert.IsTrue (_src.SequenceEqual<int> (_src));
		}

		[Test]
		public void Single ()
		{
			Assert.AreEqual (_src.Single (n => n == 10), 10);
		}

		[Test]
		public void SingleOrDefault ()
		{
			Assert.AreEqual (_src.SingleOrDefault (n => n == 10), 10);
			Assert.AreEqual (_src.SingleOrDefault (n => n == 11), 0);
		}

		[Test]
		public void Skip ()
		{
			int [] arr1 = _array.Skip<int> (5).ToArray ();
			int [] arr2 = _src.Skip<int> (5).ToArray ();
			Assert.AreEqual (arr1, arr2);
		}

		[Test]
		public void SkipWhile ()
		{
			int[] arr1 = _src.SkipWhile<int> ((n) => n < 6).ToArray();
			int[] arr2 = _src.Skip<int> (5).ToArray();
			Assert.AreEqual (arr1, arr2);
		}

		[Test]
		public void Sum ()
		{
			Assert.AreEqual (_src.Sum<int> ((n) => n), _array.Sum<int> ((n) => n));
			Assert.AreEqual (_src.Sum<int> ((n) => n + 1), _array.Sum<int> ((n) => n + 1));
		}

		[Test]
		public void Take ()
		{
			int [] arr1 = _array.Take<int> (3).ToArray ();
			int [] arr2 = _src.Take<int> (3).ToArray ();
			Assert.AreEqual (arr1, arr2);
		}

		[Test]
		public void TakeWhile ()
		{
			int [] arr1 = _array.TakeWhile<int> (n => n < 6).ToArray ();
			int [] arr2 = _src.TakeWhile<int> (n => n < 6).ToArray ();
			Assert.AreEqual (arr1, arr2);
		}

		[Test]
		public void Union ()
		{
			int [] arr1 = _src.ToArray ();
			int[] arr2 = _src.Union (_src).ToArray ();
			Assert.AreEqual (arr1, arr2);

			int [] arr = { 11,12,13};
			Assert.AreEqual (_src.Union (arr).ToArray (), _array.Union (arr).ToArray ());
		}

		[Test]
		public void Where ()
		{
			int[] oddArray1 = _array.Where<int> ((n) => (n % 2) == 1).ToArray();
			int [] oddArray2 = _src.Where<int> ((n) => (n % 2) == 1).ToArray ();
			Assert.AreEqual (oddArray1, oddArray2);
		}

		[Test]
		public void UserExtensionMethod ()
		{
			BindingFlags extensionFlags = BindingFlags.Static | BindingFlags.Public;
			MethodInfo method = (from m in typeof (Ext).GetMethods (extensionFlags)
								 where (m.Name == "UserQueryableExt1" && m.GetParameters () [0].ParameterType.GetGenericTypeDefinition () == typeof (IQueryable<>))
								 select m).FirstOrDefault ().MakeGenericMethod (typeof (int));
			Expression<Func<int, int>> exp = i => i;
			Expression e = Expression.Equal (
									Expression.Constant ("UserEnumerableExt1"),
									Expression.Call (method, _src.Expression, Expression.Quote (exp)));
			Assert.AreEqual (_src.Provider.Execute<bool> (e), true, "UserQueryableExt1");

			method = (from m in typeof (Ext).GetMethods (extensionFlags)
							   where (m.Name == "UserQueryableExt2" && m.GetParameters () [0].ParameterType.GetGenericTypeDefinition () == typeof (IQueryable<>))
							   select m).FirstOrDefault ().MakeGenericMethod (typeof (int));
			e = Expression.Equal (
									Expression.Constant ("UserEnumerableExt2"),
									Expression.Call (method, _src.Expression, Expression.Quote (exp)));
			Assert.AreEqual (_src.Provider.Execute<bool> (e), true, "UserQueryableExt2");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void UserExtensionMethodNegative ()
		{
			BindingFlags extensionFlags = BindingFlags.Static | BindingFlags.Public;
			MethodInfo method = (from m in typeof (Ext).GetMethods (extensionFlags)
								 where (m.Name == "UserQueryableExt3" && m.GetParameters () [0].ParameterType.GetGenericTypeDefinition () == typeof (IQueryable<>))
								 select m).FirstOrDefault ().MakeGenericMethod (typeof (int));
			Expression<Func<int, int>> exp = i => i;
			Expression e = Expression.Call (method, _src.Expression, Expression.Quote (exp), Expression.Constant (10));
			_src.Provider.Execute (e);
		}

		[Test]
		public void NonGenericMethod () {
			BindingFlags extensionFlags = BindingFlags.Static | BindingFlags.Public;
			MethodInfo method = (from m in typeof (Ext).GetMethods (extensionFlags)
								 where (m.Name == "NonGenericMethod" && m.GetParameters () [0].ParameterType.GetGenericTypeDefinition () == typeof (IQueryable<>))
								 select m).FirstOrDefault ();

			Expression e = Expression.Call (method, _src.Expression);
			Assert.AreEqual (_src.Provider.Execute (e), "EnumerableNonGenericMethod", "NonGenericMethod");
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void InstantiatedGenericMethod () {
			BindingFlags extensionFlags = BindingFlags.Static | BindingFlags.Public;
			MethodInfo method = (from m in typeof (Ext).GetMethods (extensionFlags)
								 where (m.Name == "InstantiatedGenericMethod" && m.GetParameters () [0].ParameterType.GetGenericTypeDefinition () == typeof (IQueryable<>))
								 select m).FirstOrDefault ().MakeGenericMethod (typeof (int));

			Expression e = Expression.Call (method, _src.Expression, Expression.Constant(0));
			_src.Provider.Execute (e);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void NullEnumerable ()
		{
			IEnumerable<int> a = null;
			a.AsQueryable ();
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void NonGenericEnumerable1 ()
		{
			new MyEnum ().AsQueryable ();
		}

		[Test]
		public void NonGenericEnumerable2 ()
		{
			IEnumerable<int> nonGen = new int[] { 1, 2, 3 };
			Assert.IsTrue (nonGen.AsQueryable () is IQueryable<int>);
		}

		class Bar<T1, T2> : IEnumerable<T2> {

			public IEnumerator<T2> GetEnumerator ()
			{
				yield break;
			}

			IEnumerator IEnumerable.GetEnumerator ()
			{
				return GetEnumerator ();
			}
		}

		[Test]
		public void NonGenericAsQueryableInstantiateProperQueryable ()
		{
			IEnumerable bar = new Bar<int, string> ();
			IQueryable queryable = bar.AsQueryable ();

			Assert.IsInstanceOfType (typeof (IQueryable<string>), queryable);
		}
	}

	class MyEnum : IEnumerable
	{
		public IEnumerator GetEnumerator ()
		{
			throw new NotImplementedException ();
		}
	}

	class CustomEqualityComparer : IEqualityComparer<int> {

		public bool Equals (int x, int y)
		{
			return true;
		}

		public int GetHashCode (int obj)
		{
			return 0;
		}
	}

	public static class Ext {

		public static string UserQueryableExt1<T> (this IQueryable<T> e, Expression<Func<int, int>> ex)
		{
			return "UserQueryableExt1";
		}

		public static string UserQueryableExt2<T> (this IQueryable<T> e, Expression<Func<int, int>> ex)
		{
			return "UserQueryableExt2";
		}

		public static string UserQueryableExt3<T> (this IQueryable<T> e, Expression<Func<int, int>> ex, int dummy)
		{
			return "UserQueryableExt3";
		}

		public static string UserQueryableExt1<T> (this IEnumerable<T> e, Expression<Func<int, int>> ex)
		{
			return "UserEnumerableExt1";
		}

		public static string UserQueryableExt2<T> (this IEnumerable<T> e, Func<int, int> ex)
		{
			return "UserEnumerableExt2";
		}

		public static string NonGenericMethod (this IQueryable<int> iq)
		{
			return "QueryableNonGenericMethod";
		}

		public static string NonGenericMethod (this IEnumerable<int> iq)
		{
			return "EnumerableNonGenericMethod";
		}

		public static string InstantiatedGenericMethod<T> (this IQueryable<int> iq, T t)
		{
			return "QueryableInstantiatedGenericMethod";
		}

		public static string InstantiatedGenericMethod (this IEnumerable<int> ie, int t)
		{
			return "EnumerableInstantiatedGenericMethod";
		}
	}
}
