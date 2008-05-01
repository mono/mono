using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;


namespace MonoTests.System.Linq
{

	[TestFixture]
	public class QueryableProviderTest
	{
		QueryProvider _provider;

		Query<int> _src;

		int [] _array = { 1, 2, 3 };
		int [] _otherArray = { 0, 2 };

		public QueryableProviderTest ()
		{
			_provider = new QueryProvider ();
			_src = new Query<int> (_provider, _array);

		}

		[SetUp]
		public void MyTestCleanup ()
		{
			_provider.Init ();
		}

		[Test]
		public void TestAggregate ()
		{
			_src.Aggregate<int> ((n, m) => n + m);
			Assert.AreEqual (StatusEnum.Execute, _provider.Status);

		}

		[Test]
		public void TestAll ()
		{
			_src.All<int> ((n) => true);
			Assert.AreEqual (StatusEnum.Execute, _provider.Status);
		}

		[Test]
		public void TestAny ()
		{
			_src.Any<int> ();
			Assert.AreEqual (StatusEnum.Execute, _provider.Status);
		}

		[Test]
		public void TestAverage ()
		{
			_src.Average<int> ((n) => n);
			Assert.AreEqual (StatusEnum.Execute, _provider.Status);
		}

		[Test]
		public void TestCast ()
		{
			_src.Cast<int> ();
			Assert.AreEqual (StatusEnum.CreateQuery, _provider.Status);
		}

		[Test]
		public void TestConcat ()
		{
			_src.Concat<int> (_otherArray);
			Assert.AreEqual (StatusEnum.CreateQuery, _provider.Status);
		}

		[Test]
		public void TestContains ()
		{
			_src.Contains<int> (3);
			Assert.AreEqual (StatusEnum.Execute, _provider.Status);
		}


		[Test]
		public void TestCount ()
		{
			_src.Count<int> ();
			Assert.AreEqual (StatusEnum.Execute, _provider.Status);
		}

		[Test]
		public void TestDefaultIfEmpty ()
		{
			_src.DefaultIfEmpty<int> (0);
			Assert.AreEqual (StatusEnum.CreateQuery, _provider.Status);
		}

		[Test]
		public void TestDistinct ()
		{
			_src.Distinct<int> ();
			Assert.AreEqual (StatusEnum.CreateQuery, _provider.Status);
		}

		[Test]
		public void TestElementAt ()
		{
			_src.ElementAt<int> (1);
			Assert.AreEqual (StatusEnum.Execute, _provider.Status);
		}

		[Test]
		public void TestElementAtOrDefault ()
		{
			_src.ElementAtOrDefault<int> (1);
			Assert.AreEqual (StatusEnum.Execute, _provider.Status);
		}

		[Test]
		public void TestExcept ()
		{
			_src.Except<int> (_otherArray);
			Assert.AreEqual (StatusEnum.CreateQuery, _provider.Status);
		}

		[Test]
		public void TestFirst ()
		{
			_src.First<int> ();
			Assert.AreEqual (StatusEnum.Execute, _provider.Status);
		}

		[Test]
		public void TestFirstOrDefault ()
		{
			_src.FirstOrDefault<int> ((n) => n > 1);
			Assert.AreEqual (StatusEnum.Execute, _provider.Status);
		}

		[Test]
		public void TestGroupBy ()
		{
			_src.GroupBy<int, bool> ((n) => n > 2);
			Assert.AreEqual (StatusEnum.CreateQuery, _provider.Status);
		}

		[Test]
		public void TestGroupJoin ()
		{
			_src.GroupJoin<int, int, bool, int> (_otherArray, (n) => n > 1, (n) => n > 1, (n, col) => n);
			Assert.AreEqual (StatusEnum.CreateQuery, _provider.Status);
		}

		[Test]
		public void TestIntersect ()
		{
			_src.Intersect<int> (_otherArray);
			Assert.AreEqual (StatusEnum.CreateQuery, _provider.Status);
		}

		[Test]
		public void TestJoin ()
		{
			_src.Join<int, int, int, int> (_otherArray, (n) => n, (n => n), (n, m) => n + m);
			Assert.AreEqual (StatusEnum.CreateQuery, _provider.Status);
		}

		[Test]
		public void TestLast ()
		{
			_src.Last<int> ((n) => n > 1);
			Assert.AreEqual (StatusEnum.Execute, _provider.Status);
		}

		[Test]
		public void TestLastOrDefault ()
		{
			_src.LastOrDefault<int> ();
			Assert.AreEqual (StatusEnum.Execute, _provider.Status);
		}

		[Test]
		public void TestLongCount ()
		{
			_src.LongCount<int> ();
			Assert.AreEqual (StatusEnum.Execute, _provider.Status);
		}

		[Test]
		public void TestMax ()
		{
			_src.Max<int> ();
			Assert.AreEqual (StatusEnum.Execute, _provider.Status);
		}

		[Test]
		public void TestMin ()
		{
			_src.Min<int> ();
			Assert.AreEqual (StatusEnum.Execute, _provider.Status);
		}

		[Test]
		public void TestOfType ()
		{
			_src.OfType<int> ();
			Assert.AreEqual (StatusEnum.CreateQuery, _provider.Status);
		}

		[Test]
		public void TestOrderBy ()
		{
			_src.OrderBy<int, bool> ((n) => n > 1);
			Assert.AreEqual (StatusEnum.CreateQuery, _provider.Status);
		}

		[Test]
		public void TestOrderByDescending ()
		{
			_src.OrderByDescending<int, bool> ((n) => n > 1);
			Assert.AreEqual (StatusEnum.CreateQuery, _provider.Status);
		}

		[Test]
		public void TestReverse ()
		{
			_src.Reverse<int> ();
			Assert.AreEqual (StatusEnum.CreateQuery, _provider.Status);
		}

		[Test]
		public void TestSelect ()
		{
			_src.Select<int, int> ((n) => n);
			Assert.AreEqual (StatusEnum.CreateQuery, _provider.Status);
		}

		[Test]
		public void TestSelectMany ()
		{
			_src.SelectMany<int, int> ((n) => new int [] { n });
			Assert.AreEqual (StatusEnum.CreateQuery, _provider.Status);
		}

		[Test]
		public void TestSequenceEqual ()
		{
			_src.SequenceEqual<int> (_otherArray);
			Assert.AreEqual (StatusEnum.Execute, _provider.Status);
		}

		[Test]
		public void TestSingle ()
		{
			(new Query<int> (_provider, new int [] { 1 })).Single<int> ();
			Assert.AreEqual (StatusEnum.Execute, _provider.Status);
		}

		[Test]
		public void TestSingleOrDefault ()
		{
			(new Query<int> (_provider, new int [] { 1 })).SingleOrDefault<int> ();
			Assert.AreEqual (StatusEnum.Execute, _provider.Status);
		}

		[Test]
		public void TestSkip ()
		{
			_src.Skip<int> (1);
			Assert.AreEqual (StatusEnum.CreateQuery, _provider.Status);
		}

		[Test]
		public void TestSkipWhile ()
		{
			_src.SkipWhile<int> ((n) => n > 1);
			Assert.AreEqual (StatusEnum.CreateQuery, _provider.Status);
		}

		[Test]
		public void TestSum ()
		{
			_src.Sum<int> ((n) => n);
			Assert.AreEqual (StatusEnum.Execute, _provider.Status);
		}

		[Test]
		public void TestTake ()
		{
			_src.Take<int> (3);
			Assert.AreEqual (StatusEnum.CreateQuery, _provider.Status);
		}


		[Test]
		public void TestTakeWhile ()
		{
			_src.TakeWhile<int> ((n) => n < 2);
			Assert.AreEqual (StatusEnum.CreateQuery, _provider.Status);
		}

		[Test]
		public void TestThenBy ()
		{
			_src.ThenBy<int, bool> ((n) => n < 2);
			Assert.AreEqual (StatusEnum.CreateQuery, _provider.Status);
		}

		[Test]
		public void TestThenByDescending ()
		{
			_src.ThenByDescending<int, bool> ((n) => n < 2);
			Assert.AreEqual (StatusEnum.CreateQuery, _provider.Status);
		}

		[Test]
		public void TestUnion ()
		{
			_src.Union<int> (_otherArray);
			Assert.AreEqual (StatusEnum.CreateQuery, _provider.Status);
		}

		[Test]
		public void TestWhere ()
		{
			_src.Where<int> ((n) => true);
			Assert.AreEqual (StatusEnum.CreateQuery, _provider.Status);
		}

		public class Query<T> : IQueryable<T>, IQueryable, IEnumerable<T>, IEnumerable, IOrderedQueryable<T>, IOrderedQueryable
		{
			IQueryProvider provider;

			Expression expression;

			IEnumerable<T> _context;

			public Query (IQueryProvider provider, IEnumerable<T> context)
			{
				_context = context;
				this.provider = provider;
				this.expression = Expression.Constant (this);
			}

			Expression IQueryable.Expression
			{

				get { return this.expression; }

			}



			Type IQueryable.ElementType
			{

				get { return typeof (T); }

			}


			IQueryProvider IQueryable.Provider
			{

				get { return this.provider; }

			}


			public IEnumerator<T> GetEnumerator ()
			{
				throw new NotImplementedException ();
			}

			IEnumerator IEnumerable.GetEnumerator ()
			{
				throw new NotImplementedException ();
			}

		}

		public enum StatusEnum { NotInitilized, Execute, CreateQuery }

		public class QueryProvider : IQueryProvider
		{

			private StatusEnum _status = StatusEnum.NotInitilized;

			public StatusEnum Status
			{
				get { return _status; }
				set { _status = value; }
			}

			public void Init ()
			{
				_status = StatusEnum.NotInitilized;
			}

			public QueryProvider ()
			{
				Init ();
			}

			#region IQueryProvider Members

			IQueryable<S> IQueryProvider.CreateQuery<S> (Expression expression)
			{
				Status = StatusEnum.CreateQuery;
				return null;
			}

			IQueryable IQueryProvider.CreateQuery (Expression expression)
			{
				Status = StatusEnum.CreateQuery;
				return null;

			}

			S IQueryProvider.Execute<S> (Expression expression)
			{
				Status = StatusEnum.Execute;
				return default (S);
			}



			object IQueryProvider.Execute (Expression expression)
			{
				Status = StatusEnum.Execute;
				return null;

			}

			#endregion
		}
	}
}
