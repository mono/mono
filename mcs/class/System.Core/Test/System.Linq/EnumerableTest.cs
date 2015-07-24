//
// EnumerableTest.cs
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
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using NUnit.Framework;

namespace MonoTests.System.Linq {

	[TestFixture]
	public class EnumerableTest {

		[Test]
		public void TestSimpleExcept ()
		{
			int [] first = {0, 1, 2, 3, 4, 5};
			int [] second = {2, 4, 6};
			int [] result = {0, 1, 3, 5};

			AssertAreSame (result, first.Except (second));
		}

		[Test]
		public void TestSimpleIntersect ()
		{
			int [] first = {0, 1, 2, 3, 4, 5};
			int [] second = {2, 4, 6};
			int [] result = {2, 4};

			AssertAreSame (result, first.Intersect (second));
		}

		[Test]
		public void TestSimpleUnion ()
		{
			int [] first = {0, 1, 2, 3, 4, 5};
			int [] second = {2, 4, 6};
			int [] result = {0, 1, 2, 3, 4, 5, 6};

			AssertAreSame (result, first.Union (second));
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

			AssertAreSame (result, foos.Cast<Bar> ());
		}

		class Bingo : IEnumerable<int>, IEnumerable<string> {

			IEnumerator<int> IEnumerable<int>.GetEnumerator ()
			{
				yield return 42;
				yield return 12;
			}

			IEnumerator<string> IEnumerable<string>.GetEnumerator ()
			{
				yield return "foo";
				yield return "bar";
			}

			public IEnumerator GetEnumerator ()
			{
				return (this as IEnumerable<int>).GetEnumerator ();
			}
		}

		[Test]
		public void TestCastToImplementedType ()
		{
			var ints = new int [] { 42, 12 };
			var strs = new string [] { "foo", "bar" };

			var bingo = new Bingo ();

			AssertAreSame (ints, bingo.Cast<int> ());
			AssertAreSame (strs, bingo.Cast<string> ());
		}

		[Test]
		public void TestLast ()
		{
			int [] data = {1, 2, 3};

			Assert.AreEqual (3, data.Last ());
		}

		[Test]
		public void TestLastOrDefault ()
		{
			int [] data = {};

			Assert.AreEqual (default (int), data.LastOrDefault ());
		}

		[Test]
		public void TestFirst ()
		{
			int [] data = {1, 2, 3};

			Assert.AreEqual (1, data.First ());
		}

		[Test]
		public void TestFirstOrDefault ()
		{
			int [] data = {};

			Assert.AreEqual (default (int), data.FirstOrDefault ());
		}

		[Test]
		public void TestSequenceEqual ()
		{
			int [] first = {0, 1, 2, 3, 4, 5};
			int [] second = {0, 1, 2};
			int [] third = {0, 1, 2, 3, 4, 5};

			Assert.IsFalse (first.SequenceEqual (second));
			Assert.IsTrue (first.SequenceEqual (third));
		}

		[Test]
		public void TestSkip ()
		{
			int [] data = {0, 1, 2, 3, 4, 5};
			int [] result = {3, 4, 5};

			AssertAreSame (result, data.Skip (3));
		}

		[Test]
		public void TestSkipWhile ()
		{
			int [] data = {0, 1, 2, 3, 4, 5};
			int [] result = {3, 4, 5};

			AssertAreSame (result, data.SkipWhile (i => i < 3));
		}

		[Test]
		public void TestTake ()
		{
			int [] data = {0, 1, 2, 3, 4, 5};
			int [] result = {0, 1, 2};

			AssertAreSame (result, data.Take (3));
		}

		[Test]
		public void TestTakeWhile ()
		{
			int [] data = {0, 1, 2, 3, 4, 5};
			int [] result = {0, 1, 2};

			AssertAreSame (result, data.TakeWhile (i => i < 3));
		}

		[Test]
		public void TestSelect ()
		{
			int [] data = {0, 1, 2, 3, 4, 5, 6, 7, 8, 9};
			int [] result = {1, 3, 5, 7, 9};

			AssertAreSame (result, data.Where (i => i % 2 != 0));
		}

		[Test]
		public void TestReverse ()
		{
			int [] data = {0, 1, 2, 3, 4};
			int [] result = {4, 3, 2, 1, 0};

			AssertAreSame (result, data.Reverse ());
			AssertAreSame (result, Enumerable.Range (0, 5).Reverse ());
		}

		[Test]
		public void ReverseArrays ()
		{
			int[] source = { 1, 2, 3 };

			var query = source.Reverse ();
			using (var enumerator = query.GetEnumerator ()) {
				enumerator.MoveNext ();
				Assert.AreEqual (3, enumerator.Current);

				source [1] = 42;
				enumerator.MoveNext ();
				Assert.AreEqual (2, enumerator.Current);

				enumerator.MoveNext ();
				Assert.AreEqual (1, enumerator.Current);
			}
		}

		[Test]
		public void TestSum ()
		{
			int [] data = {1, 2, 3, 4};

			Assert.AreEqual (10, data.Sum ());
		}

		[Test]
		public void SumOnEmpty ()
		{
			int [] data = {};

			Assert.AreEqual (0, data.Sum ());
		}

		[Test]
		public void TestMax ()
		{
			int [] data = {1, 3, 5, 2};

			Assert.AreEqual (5, data.Max ());
		}

		[Test]
		public void TestMaxNullableInt32 ()
		{
			int? [] data = { null, null, null };

			Assert.IsNull (data.Max (x => -x));

			data = new int? [] { null, 1, 2 };

			Assert.AreEqual (-1, data.Max (x => -x));
		}

		[Test]
		public void TestMin ()
		{
			int [] data = {3, 5, 2, 6, 1, 7};

			Assert.AreEqual (1, data.Min ());
		}

		[Test]
		public void TestMinNullableInt32 ()
		{
			int? [] data = { null, null, null };

			Assert.IsNull (data.Min(x => -x));

			data = new int? [] { null, 1, 2 };

			Assert.AreEqual (-2, data.Min (x => -x));
		}

		[Test]
		public void TestMinStringEmpty ()
		{
			Assert.IsNull ((new string [0]).Min ());
		}

		[Test]
		public void TestMaxStringEmpty ()
		{
			Assert.IsNull ((new string [0]).Max ());
		}

		[Test]
		public void TestToList ()
		{
			int [] data = {3, 5, 2};

			var list = data.ToList ();

			AssertAreSame (data, list);

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

			var array = coll.ToArray ();

			AssertAreSame (result, array);

			Assert.AreEqual (typeof (int []), array.GetType ());
		}

		[Test]
		public void TestIntersect ()
		{
			int [] left = { 1, 1 }, right = { 1, 1 };
			int [] result = { 1 };

			AssertAreSame (result, left.Intersect (right));
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
		public void TestAverageInt32 ()
		{
			// This does not overflow, computation is done with longs
			var x = new int [] { Int32.MaxValue, Int32.MaxValue };
			Assert.AreEqual ((double) Int32.MaxValue, x.Average ());
		}
		
		[Test]
		public void TestAverageOverflowOnInt64 ()
		{
			try {
				var x = new long [] { Int64.MaxValue, Int64.MaxValue };
				x.Average ();
				Assert.Fail ("#1");
			} catch (OverflowException) {
			}
		}

		[Test]
		public void TestAverageOnLongNullable ()
		{
			List<long?> list = new List<long?> ();
			list.Add (2);
			list.Add (3);
			Assert.AreEqual (2.5d, list.Average ());
		}

		[Test]
		public void TestRange ()
		{
			AssertAreSame (new [] {1, 2, 3, 4}, Enumerable.Range (1, 4));
			AssertAreSame (new [] {0, 1, 2, 3}, Enumerable.Range (0, 4));
		}

		[Test]
		public void SingleValueOfMaxInt32 ()
		{
			AssertAreSame (new [] { int.MaxValue }, Enumerable.Range(int.MaxValue, 1));
		}

		[Test]
		public void EmptyRangeStartingAtMinInt32 ()
		{
			AssertAreSame (new int [0], Enumerable.Range(int.MinValue, 0));
		}

		static void AssertThrows<T> (Action action) where T : Exception
		{
			try {
				action ();
				Assert.Fail ();
			} catch (T) {
			} catch {
				Assert.Fail ();
			}
		}

		[Test]
		public void TestTakeTakesProperNumberOfItems ()
		{
			var stream = new MemoryStream (new byte [] { 1, 2, 3, 4, 0 });

			Assert.AreEqual (0, stream.Position);

			foreach (byte b in AsEnumerable (stream).Take (2))
				;

			Assert.AreEqual (2, stream.Position);
		}

		static IEnumerable<byte> AsEnumerable (Stream stream)
		{
			byte b;
			while ((b = (byte) stream.ReadByte ()) >= 0)
				yield return b;
		}

		[Test]
		public void TestOrderBy ()
		{
				int [] array = { 14, 53, 3, 9, 11, 14, 5, 32, 2 };
				var q = from i in array
						orderby i
						select i;
				AssertIsOrdered (q);
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
			var q = from b in CreateBazCollection ()
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
			var q = from d in CreateData ()
					orderby d.ID descending, d.Name ascending
					select d;

			var list = new List<Data> (q);

			Assert.AreEqual ("Ab", list [0].Name);
			Assert.AreEqual ("Abcd", list [1].Name);
			Assert.AreEqual ("bcd", list [2].Name);
			Assert.AreEqual ("Zyx", list [3].Name);
		}

		[Test]
		public void TestOrderByDescendingStability ()
		{
			var data = new [] {
				new { Key = true, Value = 1 },
				new { Key = false, Value = 2},
				new { Key = true, Value = 3},
				new { Key = false, Value = 4},
				new { Key = true, Value = 5},
				new { Key = false, Value = 6},
				new { Key = true, Value = 7},
				new { Key = false, Value = 8},
				new { Key = true, Value = 9},
				new { Key = false, Value = 10},
			};

			var expected = new [] {
				new { Key = true, Value = 1 },
				new { Key = true, Value = 3},
				new { Key = true, Value = 5},
				new { Key = true, Value = 7},
				new { Key = true, Value = 9},
				new { Key = false, Value = 2},
				new { Key = false, Value = 4},
				new { Key = false, Value = 6},
				new { Key = false, Value = 8},
				new { Key = false, Value = 10},
			};

			AssertAreSame (expected, data.OrderByDescending (x => x.Key));
		}

		static void AssertIsOrdered (IEnumerable<int> e)
		{
				int f = int.MinValue;
				foreach(int i in e) {
						Assert.IsTrue (f <= i);
						f = i;
				}
		}

		static void AssertAreSame<T> (IEnumerable<T> expected, IEnumerable<T> actual)
		{
			if (expected == null) {
				Assert.IsNull (actual);
				return;
			}

			Assert.IsNotNull (actual);

			IEnumerator<T> ee = expected.GetEnumerator ();
			IEnumerator<T> ea = actual.GetEnumerator ();

			while (ee.MoveNext ()) {
				Assert.IsTrue (ea.MoveNext (), "'" + ee.Current + "' expected.");
				Assert.AreEqual (ee.Current, ea.Current);
			}

			if (ea.MoveNext ())
				Assert.Fail ("Unexpected element: " + ea.Current);
		}
	}
}
