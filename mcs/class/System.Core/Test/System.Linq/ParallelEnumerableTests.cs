// ParallelEnumerableTests.cs
//
// Copyright (c) 2008 Jérémie "Garuma" Laval
//
// Based on Enumerable test suite by Jb Evain (jbevain@novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//

#if NET_4_0

using System;
using System.Threading;
using System.Linq;

using System.Collections;
using System.Collections.Generic;

using NUnit.Framework;

namespace MonoTests.System.Linq
{
	internal static class AsParallelHelper
	{
		internal static ParallelQuery<T> AsReallyParallel<T> (this IEnumerable<T> source)
		{
			return source.AsParallel ().WithExecutionMode (ParallelExecutionMode.ForceParallelism);
		}
	}

	[TestFixtureAttribute]
	public class ParallelEnumerableTests
	{
		IEnumerable<int> baseEnumerable;
		
		[SetUpAttribute]
		public void Setup ()
		{
			baseEnumerable = Enumerable.Range(1, 1000);
		}

		static void AreEquivalent (IEnumerable syncEnumerable, IEnumerable async_resEnumerable)
		{
			Assert.That (async_resEnumerable, new NUnit.Framework.Constraints.CollectionEquivalentConstraint (syncEnumerable));
		}
		
		static void AssertAreSame<T> (IEnumerable<T> expected, IEnumerable<T> actual)
		{
			if (expected == null) {
				Assert.IsNull (actual);
				return;
			}

			Assert.IsNotNull (actual);
			int index = -1;

			IEnumerator<T> ee = expected.GetEnumerator ();
			IEnumerator<T> ea = actual.GetEnumerator ();

			while (ee.MoveNext ()) {
				Assert.IsTrue (ea.MoveNext (), "'" + ee.Current + "' expected at index '"+ ++index + "'.");
				Assert.AreEqual (ee.Current, ea.Current, "at index '" + index + "'");
			}

			if (ea.MoveNext ())
				Assert.Fail ("Unexpected element: " + ea.Current);
		}
		
		public static void AssertException<T> (Action action) where T : Exception
		{
			try {
				action ();
			}
			catch (T) {
				return;
			}
			Assert.Fail ("Expected: " + typeof (T).Name);
		}

		static void AssertAreSame<K, V> (K expectedKey, IEnumerable<V> expectedValues, IGrouping<K, V> actual)
		{
			if (expectedValues == null) {
				Assert.IsNull (actual);
				return;
			}

			Assert.IsNotNull (actual);

			Assert.AreEqual (expectedKey, actual.Key);

			var ee = expectedValues.GetEnumerator ();
			var ea = actual.GetEnumerator ();

			while (ee.MoveNext ()) {
				Assert.IsTrue (ea.MoveNext (), "'" + ee.Current + "' expected.");
				Assert.AreEqual (ee.Current, ea.Current);
			}

			if (ea.MoveNext ())
				Assert.Fail ("Unexpected element: " + ee.Current);
		}

		static void AssertAreSame<K, V> (IDictionary<K, IEnumerable<V>> expected, IEnumerable<IGrouping<K, V>> actual)
		{
			if (expected == null) {
				Assert.IsNull (actual);
				return;
			}

			Assert.IsNotNull (actual);

			var ee = expected.GetEnumerator ();
			var ea = actual.GetEnumerator ();

			while (ee.MoveNext ()) {
				Assert.IsTrue (ea.MoveNext (), "'" + ee.Current.Key + "' expected.");
				AssertAreSame (ee.Current.Key, ee.Current.Value, ea.Current);
			}

			if (ea.MoveNext ())
				Assert.Fail ("Unexpected element: " + ee.Current.Key);
		}

		static void AssertAreSame<K, V> (IDictionary<K, IEnumerable<V>> expected, ILookup<K, V> actual)
		{
			if (expected == null) {
				Assert.IsNull (actual);
				return;
			}

			Assert.IsNotNull (actual);

			var ee = expected.GetEnumerator ();
			var ea = actual.GetEnumerator ();

			while (ee.MoveNext ()) {
				Assert.IsTrue (ea.MoveNext (), "'" + ee.Current.Key + "' expected.");
				AssertAreSame (ee.Current.Key, ee.Current.Value, ea.Current);
			}

			if (ea.MoveNext ())
				Assert.Fail ("Unexpected element: " + ee.Current.Key);
		}

		static void AssertAreSame<K, V> (IDictionary<K, V> expected, IDictionary<K, V> actual)
		{
			if (expected == null) {
				Assert.IsNull (actual);
				return;
			}

			Assert.IsNotNull (actual);

			var ee = expected.GetEnumerator ();
			var ea = actual.GetEnumerator ();

			while (ee.MoveNext ()) {
				Assert.IsTrue (ea.MoveNext (), "'" + ee.Current.Key + ", " + ee.Current.Value + "' expected.");
				Assert.AreEqual (ee.Current.Key, ea.Current.Key);
				Assert.AreEqual (ee.Current.Value, ea.Current.Value);
			}

			if (ea.MoveNext ())
				Assert.Fail ("Unexpected element: " + ee.Current.Key + ", " + ee.Current.Value);
		}

		[Test]
		public void SelectTestCase ()
		{
			ParallelTestHelper.Repeat (() => {
				IEnumerable<int> sync  = baseEnumerable.Select (i => i * i);
				IEnumerable<int> async_res = baseEnumerable.AsParallel ().Select (i => i * i);
				
				AreEquivalent(sync, async_res);
			});
		}
			
		[Test]
		public void WhereTestCase ()
		{
			ParallelTestHelper.Repeat (() => {
				IEnumerable<int> sync  = baseEnumerable.Where(i => i % 2 == 0);
				IEnumerable<int> async_res = baseEnumerable.AsParallel().Where(i => i % 2 == 0);
				
				AreEquivalent(sync, async_res);
			});
		}
		
		[Test]
		public void CountTestCase ()
		{
			ParallelTestHelper.Repeat (() => {
				int sync  = baseEnumerable.Count();
				int async_res = baseEnumerable.AsParallel().Count();
				
				Assert.AreEqual(sync, async_res, "#1");
			});
		}
		
		[Test]
		public void AggregateTestCase ()
		{
			ParallelTestHelper.Repeat (() => {
				ParallelQuery<int> range = ParallelEnumerable.Repeat (5, 2643);
				double average = range.Aggregate(() => new double[2],
				                                 (acc, elem) => { acc[0] += elem; acc[1]++; return acc; },
				(acc1, acc2) => { acc1[0] += acc2[0]; acc1[1] += acc2[1]; return acc1; },
				acc => acc[0] / acc[1]);
				
				Assert.AreEqual(5.0, average, "#1");
			});
		}
		
		[Test]
		public void TestSimpleExcept ()
		{
			ParallelTestHelper.Repeat (() => {
				int [] first = {0, 1, 2, 3, 4, 5};
				int [] second = {2, 4, 6};
				int [] result = {0, 1, 3, 5};
	
				AreEquivalent (result, first.AsReallyParallel ().Except (second.AsParallel ()));
			});
		}

		[Test]
		public void TestSimpleIntersect ()
		{
			ParallelTestHelper.Repeat (() => {
				int [] first = {0, 1, 2, 3, 4, 5};
				int [] second = {2, 4, 6};
				int [] result = {2, 4};
	
				AreEquivalent (result, first.AsReallyParallel ().Intersect (second.AsParallel ()));
			});
		}

		[Test]
		public void TestSimpleUnion ()
		{
			ParallelTestHelper.Repeat (() => {
				int [] first = {0, 1, 2, 3, 4, 5};
				int [] second = {2, 4, 6};
				int [] result = {0, 1, 2, 3, 4, 5, 6};
				
				AreEquivalent (result, first.AsReallyParallel ().Union (second.AsParallel ()));
			});
		}

		[Test]
		public void TestBigUnion ()
		{
			ParallelTestHelper.Repeat (() => {
				int [] first = Enumerable.Range (1, 10000).ToArray ();
				int [] second = Enumerable.Range (323, 757).ToArray ();

				AreEquivalent (first, first.AsReallyParallel ().Union (second.AsParallel ()));
			});
		}

		[Test]
		public void TestBigIntersect ()
		{
			ParallelTestHelper.Repeat (() => {
				int [] first = Enumerable.Range (1, 10000).ToArray ();
				int [] second = Enumerable.Range (323, 757).ToArray ();

				AreEquivalent (second, first.AsReallyParallel ().Intersect (second.AsParallel ()));
			});
		}
		
		class Foo {}
		class Bar : Foo {}

		[Test]
		public void TestCast ()
		{
			Bar a = new Bar ();
			Bar b = new Bar ();
			Bar c = new Bar ();

			Foo [] foos = new Foo [] {a, b, c};
			Bar [] result = new Bar [] {a, b, c};

			AreEquivalent (result, foos.AsReallyParallel ().Cast<Bar> ());
		}
		
		[Test]
		public void TestSkip ()
		{
			int [] data = {0, 1, 2, 3, 4, 5};
			int [] result = {3, 4, 5};

			AssertAreSame (result, data.AsReallyParallel ().AsOrdered ().Skip (3).ToArray ());
		}
		
		[Test]
		public void TestSkipIterating ()
		{
			int [] data = {0, 1, 2, 3, 4, 5};
			int [] result = {3, 4, 5};

			AssertAreSame (result, data.AsReallyParallel ().AsOrdered ().Skip (3));
		}

		[Test]
		public void TestSkipWhile ()
		{
			int [] data = {0, 1, 2, 3, 4, 5};
			int [] result = {3, 4, 5};

			AssertAreSame (result, data.AsReallyParallel ().AsOrdered ().SkipWhile (i => i < 3));
		}

		[Test]
		public void TestTake ()
		{
			int [] data = {0, 1, 2, 3, 4, 5};
			int [] result = {0, 1, 2};

			AssertAreSame (result, data.AsReallyParallel ().AsOrdered ().Take (3));
		}

		[Test]
		public void TestTakeWhile ()
		{
			int [] data = {0, 1, 2, 3, 4, 5};
			int [] result = {0, 1, 2};

			AssertAreSame (result, data.AsReallyParallel ().AsOrdered ().TakeWhile (i => i < 3));
		}

		[Test]
		public void SelectManyTest ()
		{
			IEnumerable<int> initial = Enumerable.Range (1, 50);
			IEnumerable<int> expected = initial.SelectMany ((i) => Enumerable.Range (1, i));

			ParallelTestHelper.Repeat (() => {
					var actual = initial.AsReallyParallel ().SelectMany ((i) => Enumerable.Range (1, i));
					AreEquivalent (expected, actual);
				});
		}

		[Test]
		public void SelectManyOrderedTest ()
		{
			IEnumerable<int> initial = Enumerable.Range (1, 50);
			IEnumerable<int> expected = initial.SelectMany ((i) => Enumerable.Range (1, i));

			ParallelTestHelper.Repeat (() => {
					var actual = initial.AsReallyParallel ().AsOrdered ().SelectMany ((i) => Enumerable.Range (1, i));
					AssertAreSame (expected, actual);
				});
		}
		
		[Test]
		public void TestLast ()
		{
			int [] data = {1, 2, 3};

			Assert.AreEqual (3, data.AsReallyParallel ().AsOrdered ().Last ());
		}

		[Test]
		public void TestLastOrDefault ()
		{
			int [] data = {};

			Assert.AreEqual (default (int), data.AsReallyParallel ().AsOrdered ().LastOrDefault ());
		}

		[Test]
		public void TestFirst ()
		{
			int [] data = {1, 2, 3};

			Assert.AreEqual (1, data.AsReallyParallel ().AsOrdered ().First ());
		}

		[Test]
		public void TestFirstOrDefault ()
		{
			int [] data = {};

			Assert.AreEqual (default (int), data.AsReallyParallel ().AsOrdered ().FirstOrDefault ());
		}
		
		[Test]
		public void TestReverse ()
		{
			int [] data = {0, 1, 2, 3, 4};
			int [] result = {4, 3, 2, 1, 0};

			AssertAreSame (result, ((IEnumerable<int>)data).Select ((i) => i).AsReallyParallel ().AsOrdered ().Reverse ());
			AssertAreSame (result, ParallelEnumerable.Range (0, 5).AsReallyParallel ().AsOrdered ().Reverse ());
		}
		
		[Test]
		public void TestOrderBy ()
		{
			ParallelTestHelper.Repeat (() => {
				int [] array = { 14, 53, 3, 9, 11, 14, 5, 32, 2 };
				
				var q = array.AsReallyParallel ().OrderBy ((i) => i);
				AssertIsOrdered (q, array.Length);
			});
		}

		class Baz {
			string name;
			int age;

			public string Name
			{
				get {
					if (string.IsNullOrEmpty (name))
						return Age.ToString ();

					return name + " (" + Age + ")";
				}
			}

			public int Age
			{
				get { return age + 1; }
			}

			public Baz (string name, int age)
			{
				this.name = name;
				this.age = age;
			}

			public override int GetHashCode ()
			{
				return this.Age ^ this.Name.GetHashCode ();
			}

			public override bool Equals (object obj)
			{
				Baz b = obj as Baz;
				if (b == null)
					return false;

				return b.Age == this.Age && b.Name == this.Name;
			}

			public override string ToString ()
			{
				return this.Name;
			}
		}

		static IEnumerable<Baz> CreateBazCollection ()
		{
			return new [] {
				new Baz ("jb", 25),
				new Baz ("ana", 20),
				new Baz ("reg", 28),
				new Baz ("ro", 25),
				new Baz ("jb", 7),
			};
		}

		[Test]
		public void TestOrderByAgeAscendingTheByNameDescending ()
		{
			ParallelTestHelper.Repeat (() => {
				var q = from b in CreateBazCollection ().AsReallyParallel ()
						orderby b.Age ascending, b.Name descending
						select b;
	
				var expected = new [] {
					new Baz ("jb", 7),
					new Baz ("ana", 20),
					new Baz ("ro", 25),
					new Baz ("jb", 25),
					new Baz ("reg", 28),
				};

				AssertAreSame (expected, q);
			});
		}

		class Data {
			public int ID { get; set; }
			public string Name { get; set; }

			public override string ToString ()
			{
				return ID + " " + Name;
			}
		}

		IEnumerable<Data> CreateData ()
		{
			return new [] {
				new Data { ID = 10, Name = "bcd" },
				new Data { ID = 20, Name = "Abcd" },
				new Data { ID = 20, Name = "Ab" },
				new Data { ID = 10, Name = "Zyx" },
			};
		}

		[Test]
		public void TestOrderByIdDescendingThenByNameAscending ()
		{
			ParallelTestHelper.Repeat (() => {
				var q = from d in CreateData ().AsReallyParallel ()
					orderby d.ID descending, d.Name ascending
						select d;
				
				var list = new List<Data> (q);
				
				Assert.AreEqual ("Ab", list [0].Name);
				Assert.AreEqual ("Abcd", list [1].Name);
				Assert.AreEqual ("bcd", list [2].Name);
				Assert.AreEqual ("Zyx", list [3].Name);
			});
		}

		static void AssertIsOrdered (IEnumerable<int> e, int count)
		{
			int f = int.MinValue;
			int c = 0;
			
			foreach (int i in e) {
				Assert.IsTrue (f <= i, string.Format ("{0} <= {1}", f, i));
				f = i;
				c++;
			}
			
			Assert.AreEqual (count, c);
		}
		
		
		[Test]
		public void ElementAtTestCase()
		{
			//ParallelTestHelper.Repeat (() => {
					Assert.AreEqual(1, baseEnumerable.AsReallyParallel ().AsOrdered ().ElementAt(0), "#1");
					Assert.AreEqual(51, baseEnumerable.AsReallyParallel ().AsOrdered ().ElementAt(50), "#2");
					Assert.AreEqual(489, baseEnumerable.AsReallyParallel ().AsOrdered ().ElementAt(488), "#3");
			//});
		}

		[Test]
		public void TestJoin ()
		{
			int num = 100;
			Tuple<int, int>[] outer = Enumerable.Range (1, 50).Select ((i) => Tuple.Create (i, num - 2 * i)).ToArray ();
			Tuple<int, int>[] inner = Enumerable.Range (1, 50).Reverse ().Select ((i) => Tuple.Create (i, 2 * i)).ToArray ();

			IEnumerable<int> expected = outer.Join (inner, (e) => e.Item1, (e) => e.Item1, (e1, e2) => e1.Item2 + e2.Item2, EqualityComparer<int>.Default);

			ParallelTestHelper.Repeat (() => {
				ParallelQuery<int> actual = outer.AsReallyParallel ().Join (inner.AsParallel (),
				                                                            (e) => e.Item1,
				                                                            (e) => e.Item1,
				                                                            (e1, e2) => e1.Item2 + e2.Item2,
				                                                            EqualityComparer<int>.Default);
				AreEquivalent (expected, actual);
			});
		}

		[Test]
		public void SmallJoinTest ()
		{
			var items = new [] { 1, 2, 3 };
			var items2 = new [] { 1, 2, 3, 4 };
			var actual = items.AsReallyParallel ().Join (items2.AsReallyParallel (), i => i, i => i, (e1, e2) => e1 + e2);
			AreEquivalent (new[] { 2, 4, 6 }, actual);
		}

		[Test]
		[Category ("NotWorking")] // Deadlocks randomly
		public void TestGroupBy ()
		{
			int num = 100;
			Tuple<int, int>[] source = Enumerable.Range (0, num).Select ((i) => Tuple.Create (i / 10, i)).ToArray ();

			ParallelTestHelper.Repeat (() => {
				ParallelQuery<IGrouping<int, int>> actual = source.AsReallyParallel ().GroupBy ((e) => e.Item1, (e) => e.Item2, EqualityComparer<int>.Default);
				foreach (var group in actual) {
					Assert.IsTrue (group.Key >= 0);
					Assert.IsTrue (group.Key < num / 10);

					int count = 0;
					foreach (var e in group) {
						count++;
						Assert.IsTrue (e >= group.Key * 10);
						Assert.IsTrue (e <  (group.Key + 1) * 10);
					}

					Assert.AreEqual (10, count, "count");
				}
			});
		}
		
		[Test]
		public void TakeTestCase()
		{
			ParallelTestHelper.Repeat (() => {
				ParallelQuery<int> async_res = baseEnumerable.AsReallyParallel ().AsOrdered ().Take(800);
				IEnumerable<int> sync = baseEnumerable.Take(800);

				AreEquivalent(sync, async_res);

				async_res = baseEnumerable.AsReallyParallel ().AsOrdered ().Take(100);
				sync = baseEnumerable.Take(100);

				AreEquivalent(sync, async_res);
			});
		}

		[TestAttribute]
		public void UnorderedTakeTestCase()
		{
			ParallelTestHelper.Repeat (() => {
				ParallelQuery<int> async_res = baseEnumerable.AsReallyParallel ().Take(800);
				IEnumerable<int> sync = baseEnumerable.Take (800);

				Assert.AreEqual (sync.Count (), async_res.Count (), "#1");

				async_res = baseEnumerable.AsReallyParallel ().Take(100);
				sync = baseEnumerable.Take(100);

				Assert.AreEqual (sync.Count (), async_res.Count (), "#2");
			});
		}
		
		[Test]
		public void SkipTestCase()
		{
			ParallelTestHelper.Repeat (() => {
				ParallelQuery<int> async_res = baseEnumerable.AsReallyParallel ().AsOrdered().Skip (800);
				IEnumerable<int> sync = baseEnumerable.Skip (800);
				
				AreEquivalent (sync, async_res);
			});
		}

		[Test]
		public void SkipTestCaseSmall ()
		{
			ParallelTestHelper.Repeat (() => {
				var async_res = baseEnumerable.AsReallyParallel ().Skip(100);
				var sync = baseEnumerable.Skip(100);
				
				Assert.AreEqual (sync.Count (), async_res.Count ());
			});
		}

		[Test]
		public void ZipTestCase()
		{
			ParallelTestHelper.Repeat (() => {
				ParallelQuery<int> async_res1 = ParallelEnumerable.Range(0, 10000);
				ParallelQuery<int> async_res2 = ParallelEnumerable.Repeat(1, 10000).Zip(async_res1, (e1, e2) => e1 + e2);
				
				int[] expected = Enumerable.Range (1, 10000).ToArray ();
				AreEquivalent(expected, Enumerable.ToArray (async_res2));
			});
		}
		
		[Test]
		public void Range ()
		{
			ParallelTestHelper.Repeat (() => {
				IEnumerable<int> sync  = Enumerable.Range(1, 1000);
				IEnumerable<int> async_res = ParallelEnumerable.Range(1, 1000);
				
				AreEquivalent (sync, async_res);
			});
		}
	
		[Test]
		public void Range_StartOffset ()
		{
			ParallelTestHelper.Repeat (() => {
				IEnumerable<int> sync  = Enumerable.Range (30, 10);
				IEnumerable<int> async_res = ParallelEnumerable.Range (30, 10);
				
				AreEquivalent (sync, async_res);
			});
		}

		[Test]
		public void RepeatTestCase ()
		{
			ParallelTestHelper.Repeat (() => {
				IEnumerable<int> sync  = Enumerable.Repeat(1, 1000);
				IEnumerable<int> async_res = ParallelEnumerable.Repeat(1, 1000);
				
				AreEquivalent (sync, async_res);
			});
		}
		
		[Test]
		public void TestSum ()
		{
			int [] data = {1, 2, 3, 4};

			Assert.AreEqual (10, data.AsReallyParallel ().Sum ());
		}

		[Test]
		public void SumOnEmpty ()
		{
			int [] data = {};

			Assert.AreEqual (0, data.AsReallyParallel ().Sum ());
		}

		[Test]
		public void TestMax ()
		{
			int [] data = {1, 3, 5, 2};

			Assert.AreEqual (5, data.AsReallyParallel ().Max ());
		}

		[Test]
		public void TestMin ()
		{
			int [] data = {3, 5, 2, 6, 1, 7};

			Assert.AreEqual (1, data.AsReallyParallel ().Min ());
		}
		
		[Test]
		public void TestToListOrdered ()
		{
			int [] data = { 2, 3, 5 };

			var list = data.AsParallel().AsOrdered().WithExecutionMode (ParallelExecutionMode.ForceParallelism).ToList ();

			AssertAreSame (data, list);
			AssertIsOrdered (list, data.Length);

			Assert.AreEqual (typeof (List<int>), list.GetType ());
		}

		[Test]
		public void TestToArrayOrdered ()
		{
			ICollection<int> coll = new List<int> ();
			coll.Add (0);
			coll.Add (1);
			coll.Add (2);

			int [] result = {0, 1, 2};

			var array = coll.AsReallyParallel ().AsOrdered().ToArray ();

			AssertAreSame (result, array);
			AssertIsOrdered (array, result.Length);

			Assert.AreEqual (typeof (int []), array.GetType ());

			array = Enumerable.Range (1, 100).Select ((i) => i).AsReallyParallel ().AsOrdered().ToArray ();
			result = Enumerable.Range (1, 100).ToArray ();

			AssertAreSame (result, array);
			AssertIsOrdered (array, result.Length);

			Assert.AreEqual (typeof (int []), array.GetType ());

		}

		[Test]
		public void TestToList ()
		{
			int [] data = {3, 5, 2};

			var list = data.AsReallyParallel ().ToList ();

			AreEquivalent (data, list);

			Assert.AreEqual (typeof (List<int>), list.GetType ());
		}

		[Test]
		public void TestToArray ()
		{
			ICollection<int> coll = new List<int> ();
			coll.Add (0);
			coll.Add (1);
			coll.Add (2);

			int [] result = {0, 1, 2};

			var array = coll.AsReallyParallel ().ToArray ();

			AreEquivalent (result, array);

			Assert.AreEqual (typeof (int []), array.GetType ());
		}
		
		
		[Test]
		public void TestAverageOnInt32 ()
		{
			Assert.AreEqual (23.25, (new int [] { 24, 7, 28, 34 }).Average ());
		}

		[Test]
		public void TestAverageOnInt64 ()
		{
			Assert.AreEqual (23.25, (new long [] { 24, 7, 28, 34 }).Average ());
		}
		
		
		[Test]
		public void AnyArgumentNullTest ()
		{
			string [] data = { "2", "1", "5", "3", "4" };


			// Any<TSource> ()
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).AsReallyParallel ().Any (); });

			// Any<TSource> (Func<TSource, bool>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).AsReallyParallel ().Any (x => true); });
			AssertException<ArgumentNullException> (delegate () { data.AsReallyParallel ().Any ((Func<string, bool>) null); });
		}

		[Test]
		public void AnyTest ()
		{
			int [] data = { 5, 2, 3, 1, 6 };
			int [] empty = { };


			// Any<TSource> ()
			Assert.IsTrue (data.AsReallyParallel ().Any ());
			Assert.IsFalse (empty.AsReallyParallel ().Any ());

			// Any<TSource> (Func<TSource, bool>)
			Assert.IsTrue (data.AsReallyParallel ().Any (x => x == 5));
			Assert.IsFalse (data.AsReallyParallel ().Any (x => x == 9));
			Assert.IsFalse (empty.AsReallyParallel ().Any (x => true));
		}

		
		[Test]
		public void AllArgumentNullTest ()
		{
			string [] data = { "2", "1", "5", "3", "4" };

			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).AsReallyParallel ().All (x => true); });
			AssertException<ArgumentNullException> (delegate () { data.AsReallyParallel ().All ((Func<string, bool>) null); });
		}

		[Test]
		public void AllTest ()
		{
			int [] data = { 5, 2, 3, 1, 6 };
			int [] empty = { };

			Assert.IsTrue (data.AsReallyParallel ().All (x => true));
			Assert.IsFalse (data.AsReallyParallel ().All (x => x != 1));
			Assert.IsTrue (empty.AsReallyParallel ().All (x => false));
		}

		[Test]
		public void SequenceEqualsTest ()
		{
			var data1 = new int[] { 1, 2, 3 };
			var data2 = new int[] { 1, 2, 4 };
			var data3 = new int[] { 1, 2, 3, 4 };

			Assert.IsTrue (data1.AsReallyParallel ().SequenceEqual (data1.AsReallyParallel ()));
			Assert.IsTrue (data2.AsReallyParallel ().SequenceEqual (data2.AsReallyParallel ()));
			Assert.IsTrue (data3.AsReallyParallel ().SequenceEqual (data3.AsReallyParallel ()));
			Assert.IsFalse (data1.AsReallyParallel ().SequenceEqual (data2.AsReallyParallel ()));
			Assert.IsFalse (data1.AsReallyParallel ().SequenceEqual (data3.AsReallyParallel ()));
			Assert.IsFalse (data2.AsReallyParallel ().SequenceEqual (data3.AsReallyParallel ()));
		}

		[Test]
		public void ContainsTest ()
		{
			var data1 = new int[] { 1, 2, 3 };
			var data2 = new int[] { 1, 2, 4 };
			var data3 = new int[] { 1, 2, 3, 4 };

			Assert.IsTrue (data1.AsReallyParallel ().Contains (3));
			Assert.IsFalse (data2.AsReallyParallel ().Contains (3));
			Assert.IsTrue (data3.AsReallyParallel ().Contains (3));
			Assert.IsFalse (data3.AsReallyParallel ().Contains (5));
			Assert.IsTrue (data2.AsReallyParallel ().Contains (2));
		}
	}
}

#endif
