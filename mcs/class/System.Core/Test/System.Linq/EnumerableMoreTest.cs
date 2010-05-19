//
// EnumerableMoreTest.cs
//
// Author:
//  Andreas Noever <andreas.noever@gmail.com>
//
// (C) 2007 Andreas Noever
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
using NUnit.Framework;


namespace MonoTests.System.Linq {

	[TestFixture]
	public class EnumerableMoreTest {

		class BigEnumerable : IEnumerable<int> {
			public readonly ulong Count;
			public BigEnumerable (ulong Count)
			{
				this.Count = Count;
			}


			#region IEnumerable<int> Members

			public IEnumerator<int> GetEnumerator ()
			{
				return new BigEnumerator (this);
			}

			#endregion

			#region IEnumerable Members

			IEnumerator IEnumerable.GetEnumerator ()
			{
				throw new NotImplementedException ();
			}

			#endregion
		}

		class BigEnumerator : IEnumerator<int> {
			BigEnumerable Parent;
			private ulong current;

			public BigEnumerator (BigEnumerable parent)
			{
				Parent = parent;
			}

			public int Current
			{
				get { return 3; }
			}

			public void Dispose ()
			{
			}

			object IEnumerator.Current
			{
				get { throw new NotImplementedException (); }
			}

			public bool MoveNext ()
			{
				if (current == Parent.Count)
					return false;
				current++;
				return true;
			}

			public void Reset ()
			{
				throw new NotImplementedException ();
			}

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

		[Test]
		public void FirstArgumentNullTest ()
		{
			string [] data = { "2", "1", "5", "3", "4" };


			// First<TSource> ()
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).First (); });

			// First<TSource> (Func<TSource, bool>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).First ((x => true)); });
			AssertException<ArgumentNullException> (delegate () { data.First ((Func<string, bool>) null); });
		}

		[Test]
		public void FirstTest ()
		{
			int [] data = { 2, 1, 5, 3, 4 };
			int [] empty = { };

			// First<TSource> ()
			Assert.AreEqual (2, data.First ());
			AssertException<InvalidOperationException> (delegate () { empty.First (); });

			// First<TSource> (Func<TSource, bool>)
			Assert.AreEqual (5, data.First (x => x == 5));
			AssertException<InvalidOperationException> (delegate () { empty.First (x => x == 5); });
			AssertException<InvalidOperationException> (delegate () { data.First (x => x == 6); });
		}

		[Test]
		public void FirstOrDefaultArgumentNullTest ()
		{
			string [] data = { "2", "1", "5", "3", "4" };


			// FirstOrDefault<TSource> ()
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).FirstOrDefault (); });

			// FirstOrDefault<TSource> (Func<string, bool>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).FirstOrDefault ((x => true)); });
			AssertException<ArgumentNullException> (delegate () { data.FirstOrDefault ((Func<string, bool>) null); });
		}

		[Test]
		public void FirstOrDefaultTest ()
		{
			int [] data = { 2, 1, 5, 3, 4 };
			int [] empty = { };


			// FirstOrDefault<TSource> ()
			Assert.AreEqual (2, data.FirstOrDefault ());
			Assert.AreEqual (0, empty.FirstOrDefault ());

			// FirstOrDefault<TSource> (Func<TSource, bool>)
			Assert.AreEqual (5, data.FirstOrDefault (x => x == 5));
			Assert.AreEqual (0, empty.FirstOrDefault (x => x == 5));
			Assert.AreEqual (0, data.FirstOrDefault (x => x == 6));

		}

		[Test]
		public void LastArgumentNullTest ()
		{
			string [] data = { "2", "1", "5", "3", "4" };


			// Last<TSource> ()
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Last (); });

			// Last<TSource> (Func<TSource, bool>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Last (x => true); });
			AssertException<ArgumentNullException> (delegate () { data.Last ((Func<string, bool>) null); });
		}

		[Test]
		public void LastTest ()
		{
			int [] data = { 2, 1, 1, 3, 4, 5 };
			int [] empty = { };

			// Last<TSource> ()
			Assert.AreEqual (5, data.Last ());
			AssertException<InvalidOperationException> (delegate () { empty.Last (); });

			// Last<TSource> (Func<TSource, bool>)
			Assert.AreEqual (4, data.Last (x => x < 5));
			AssertException<InvalidOperationException> (delegate () { empty.Last (x => x == 5); });
			AssertException<InvalidOperationException> (delegate () { data.Last (x => x == 6); });
		}

		[Test]
		public void LastOrDefaultArgumentNullTest ()
		{
			string [] data = { "2", "1", "5", "3", "4" };


			// LastOrDefault<TSource> ()
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).LastOrDefault (); });

			// LastOrDefault<TSource> (Func<TSource, bool>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).LastOrDefault (x => true); });
			AssertException<ArgumentNullException> (delegate () { data.LastOrDefault ((Func<string, bool>) null); });
		}

		[Test]
		public void LastOrDefaultTest ()
		{
			int [] data = { 2, 1, 5, 3, 4 };
			int [] empty = { };


			// LastOrDefault<TSource> ()
			Assert.AreEqual (4, data.LastOrDefault ());
			Assert.AreEqual (0, empty.LastOrDefault ());

			// LastOrDefault<TSource> (Func<TSource, bool>)
			Assert.AreEqual (3, data.LastOrDefault (x => x < 4));
			Assert.AreEqual (0, empty.LastOrDefault (x => x == 5));
			Assert.AreEqual (0, data.LastOrDefault (x => x == 6));
		}

		[Test]
		public void SingleArgumentNullTest ()
		{
			string [] data = { "2", "1", "5", "3", "4" };


			// Single<TSource> ()
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Single (); });

			// Single<TSource> (Func<TSource, bool>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Single ((x => true)); });
			AssertException<ArgumentNullException> (delegate () { data.Single ((Func<string, bool>) null); });
		}

		[Test]
		public void SingleTest ()
		{
			int [] data = { 2 };
			int [] data2 = { 2, 3, 5 };
			int [] empty = { };


			// Single<TSource> ()
			Assert.AreEqual (2, data.Single ());
			AssertException<InvalidOperationException> (delegate () { data2.Single (); });
			AssertException<InvalidOperationException> (delegate () { empty.Single (); });

			// Single<TSource> (Func<TSource, bool>)
			Assert.AreEqual (5, data2.Single (x => x == 5));
			AssertException<InvalidOperationException> (delegate () { data2.Single (x => false); });
			AssertException<InvalidOperationException> (delegate () { data2.Single (x => true); });
			AssertException<InvalidOperationException> (delegate () { empty.Single (x => true); });
		}

		[Test]
		public void SingleOrDefaultArgumentNullTest ()
		{
			string [] data = { "2", "1", "5", "3", "4" };


			// SingleOrDefault<TSource> ()
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).SingleOrDefault (); });

			// SingleOrDefault<TSource> (Func<TSource, bool>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).SingleOrDefault (x => true); });
			AssertException<ArgumentNullException> (delegate () { data.SingleOrDefault ((Func<string, bool>) null); });
		}

		[Test]
		public void SingleOrDefaultTest ()
		{
			int [] data = { 2 };
			int [] data2 = { 2, 3, 5 };
			int [] empty = { };


			// SingleOrDefault<TSource> ()
			Assert.AreEqual (2, data.SingleOrDefault ());
			Assert.AreEqual (0, empty.SingleOrDefault ());
			AssertException<InvalidOperationException> (delegate () { data2.SingleOrDefault (); });


			// SingleOrDefault<TSource> (Func<TSource, bool>)
			Assert.AreEqual (3, data2.SingleOrDefault (x => x == 3));
			Assert.AreEqual (0, data2.SingleOrDefault (x => false));
			AssertException<InvalidOperationException> (delegate () { data2.SingleOrDefault (x => true); });
		}

		[Test]
		public void ElementAtArgumentNullTest ()
		{
			//string [] data = { "2", "1", "5", "3", "4" };


			// ElementAt<TSource> (int)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).ElementAt (0); });
		}

		[Test]
		public void ElementAtTest ()
		{
			int [] data = { 2, 3, 4, 5 };

			// ElementAt<string> (int)
			Assert.AreEqual (2, data.ElementAt (0));
			Assert.AreEqual (4, data.ElementAt (2));
			AssertException<ArgumentOutOfRangeException> (delegate () { data.ElementAt (-1); });
			AssertException<ArgumentOutOfRangeException> (delegate () { data.ElementAt (4); });
			AssertException<ArgumentOutOfRangeException> (delegate () { data.ElementAt (6); });
		}

		[Test]
		public void ElementAtOrDefaultArgumentNullTest ()
		{
			//string [] data = { "2", "1", "5", "3", "4" };


			// ElementAtOrDefault<TSource> (int)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).ElementAtOrDefault (0); });
		}

		[Test]
		public void ElementAtOrDefaultTest ()
		{
			int [] data = { 2, 3, 4, 5 };
			int [] empty = { };


			// ElementAtOrDefault<TSource> (int)
			Assert.AreEqual (2, data.ElementAtOrDefault (0));
			Assert.AreEqual (4, data.ElementAtOrDefault (2));
			Assert.AreEqual (0, data.ElementAtOrDefault (-1));
			Assert.AreEqual (0, data.ElementAtOrDefault (4));
			Assert.AreEqual (0, empty.ElementAtOrDefault (4));
		}

		[Test]
		public void EmptyTest ()
		{
			IEnumerable<string> empty = Enumerable.Empty<string> ();
			Assert.IsFalse (empty.GetEnumerator ().MoveNext ());

		}

		[Test]
		public void AnyArgumentNullTest ()
		{
			string [] data = { "2", "1", "5", "3", "4" };


			// Any<TSource> ()
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Any (); });

			// Any<TSource> (Func<TSource, bool>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Any (x => true); });
			AssertException<ArgumentNullException> (delegate () { data.Any ((Func<string, bool>) null); });
		}

		[Test]
		public void AnyTest ()
		{
			int [] data = { 5, 2, 3, 1, 6 };
			int [] empty = { };


			// Any<TSource> ()
			Assert.IsTrue (data.Any ());
			Assert.IsFalse (empty.Any ());

			// Any<TSource> (Func<TSource, bool>)
			Assert.IsTrue (data.Any (x => x == 5));
			Assert.IsFalse (data.Any (x => x == 9));
			Assert.IsFalse (empty.Any (x => true));
		}

		[Test]
		public void AllArgumentNullTest ()
		{
			string [] data = { "2", "1", "5", "3", "4" };


			// All<TSource> (Func<TSource, bool>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).All (x => true); });
			AssertException<ArgumentNullException> (delegate () { data.All ((Func<string, bool>) null); });
		}

		[Test]
		public void AllTest ()
		{
			int [] data = { 5, 2, 3, 1, 6 };
			int [] empty = { };

			// All<TSource> (Func<TSource, bool>)
			Assert.IsTrue (data.All (x => true));
			Assert.IsFalse (data.All (x => x != 1));
			Assert.IsTrue (empty.All (x => false));
		}

		[Test]
		public void CountArgumentNullTest ()
		{
			string [] data = { "2", "1", "5", "3", "4" };


			// Count<TSource> ()
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Count (); });

			// Count<TSource> (Func<TSource, bool>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Count (x => true); });
			AssertException<ArgumentNullException> (delegate () { data.Count ((Func<string, bool>) null); });
		}

		[Test]
		public void CountTest ()
		{
			int [] data = { 5, 2, 3, 1, 6 };

			// Count<TSource> ()
			Assert.AreEqual (5, data.Count ());

			// Count<TSource> (Func<TSource, bool>)
			Assert.AreEqual (3, data.Count (x => x < 5));
		}


		//[Test]
		public void CountOverflowTest ()
		{
			//BigEnumerable data = new BigEnumerable ((ulong) int.MaxValue + 1);

			// Count<TSource> ()
			//AssertException<OverflowException> (delegate () { data.Count (); });

			// Count<TSource> (Func<TSource, bool>)
			//AssertException<OverflowException> (delegate () { data.Count (x => 3 == x); });

			// Documentation error: http://msdn2.microsoft.com/en-us/library/bb535181.aspx
			// An exception is only rasied if count > int.MaxValue. Not if source contains more than int.MaxValue elements.
			// AssertException<OverflowException> (delegate () { data.Count (x => 5 == x); });
		}

		[Test]
		public void LongCountArgumentNullTest ()
		{
			string [] data = { "2", "1", "5", "3", "4" };


			// LongCount<TSource> ()
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).LongCount (); });

			// LongCount<TSource> (Func<TSource, bool>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).LongCount (x => true); });
			AssertException<ArgumentNullException> (delegate () { data.LongCount ((Func<string, bool>) null); });
		}

		[Test]
		public void LongCountTest ()
		{
			int [] data = { 5, 2, 3, 1, 6 };

			//TODO: Overflow test...

			// LongCount<TSource> ()
			Assert.AreEqual (5, data.LongCount ());
			Assert.AreEqual (5, Enumerable.Range (0, 5).LongCount ());

			// LongCount<TSource> (Func<TSource, bool>)
			Assert.AreEqual (3, data.LongCount (x => x < 5));
		}

		[Test]
		public void ContainsArgumentNullTest ()
		{
			//string [] data = { "2", "1", "5", "3", "4" };


			// Contains<TSource> (TSource)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Contains ("2"); });

			// Contains<TSource> (TSource, IEqualityComparer<TSource>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Contains ("2", (IEqualityComparer<string>) EqualityComparer<string>.Default); });
		}

		[Test]
		public void ContainsTest ()
		{
			int [] data = { 5, 2, 3, 1, 6 };


			// Contains<TSource> (TSource)
			Assert.IsTrue (data.Contains (2));
			Assert.IsFalse (data.Contains (0));

			// Contains<TSource> (TSource, IEqualityComparer<TSource>)
			Assert.IsTrue (data.Contains (2, EqualityComparer<int>.Default));
			Assert.IsFalse (data.Contains (0, EqualityComparer<int>.Default));
		}

		[Test]
		public void AggregateArgumentNullTest ()
		{
			string [] data = { "2", "1", "5", "3", "4" };


			// Aggregate<TSource> (Func<TSource, TSource, TSource>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Aggregate ((x, y) => "test"); });
			AssertException<ArgumentNullException> (delegate () { data.Aggregate ((Func<string, string, string>) null); });

			// Aggregate<TSource,TAccumulate> (TAccumulate, Func<TAccumulate, TSource, TAccumulate>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Aggregate ("initial", (x, y) => "test"); });
			AssertException<ArgumentNullException> (delegate () { data.Aggregate ("initial", (Func<string, string, string>) null); });

			// Aggregate<TSource,TAccumulate,TResult> (TAccumulate, Func<TAccumulate, TSource, TAccumulate>, Func<TAccumulate, TResult>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Aggregate ("initial", (x, y) => "test", x => "test"); });
			AssertException<ArgumentNullException> (delegate () { data.Aggregate ("initial", (Func<string, string, string>) null, x => "test"); });
			AssertException<ArgumentNullException> (delegate () { data.Aggregate ("initial", (x, y) => "test", (Func<string, string>) null); });
		}

		[Test]
		public void AggregateTest ()
		{
			string [] data = { "2", "1", "5", "3", "4" };
			string [] empty = { };

			// Aggregate<TSource> (Func<TSource, TSource, TSource>)
			Assert.AreEqual ("21534", data.Aggregate ((x, y) => x + y));
			AssertException<InvalidOperationException> (delegate () { empty.Aggregate ((x, y) => x + y); }); //only this overload throws

			// Aggregate<TSource,TAccumulate> (TAccumulate, Func<TAccumulate, TSource, TAccumulate>)
			Assert.AreEqual ("initial21534", (data.Aggregate ("initial", (x, y) => x + y)));

			// Aggregate<TSource,TAccumulate,TResult> (TAccumulate, Func<TAccumulate, TSource, TAccumulate>, Func<TAccumulate, TResult>)
			Assert.AreEqual ("INITIAL21534", data.Aggregate ("initial", (x, y) => x + y, (x => x.ToUpper ())));
		}

		[Test]
		public void SumArgumentNullTest ()
		{
			string [] data = { "2", "1", "5", "3", "4" };


			// Sum<TSource> (Func<TSource, int>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Sum<string> ((Func<string, int>) (x => 0)); });
			AssertException<ArgumentNullException> (delegate () { data.Sum<string> ((Func<string, int>) null); });

			// Sum<TSource> (Func<TSource, Nullable<int>>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Sum<string> ((Func<string, Nullable<int>>) (x => (int?) 0)); });
			AssertException<ArgumentNullException> (delegate () { data.Sum<string> ((Func<string, Nullable<int>>) null); });

			// Sum<TSource> (Func<TSource, Int64>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Sum<string> ((Func<string, Int64>) (x => 0L)); });
			AssertException<ArgumentNullException> (delegate () { data.Sum<string> ((Func<string, Int64>) null); });

			// Sum<TSource> (Func<TSource, Nullable<Int64>>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Sum<string> ((Func<string, Nullable<Int64>>) (x => (int?) 0L)); });
			AssertException<ArgumentNullException> (delegate () { data.Sum<string> ((Func<string, Nullable<Int64>>) null); });

			// Sum<TSource> (Func<TSource, Single>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Sum<string> ((Func<string, Single>) (x => 0f)); });
			AssertException<ArgumentNullException> (delegate () { data.Sum<string> ((Func<string, Single>) null); });

			// Sum<TSource> (Func<TSource, Nullable<Single>>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Sum<string> ((Func<string, Nullable<Single>>) (x => (int?) 0f)); });
			AssertException<ArgumentNullException> (delegate () { data.Sum<string> ((Func<string, Nullable<Single>>) null); });

			// Sum<TSource> (Func<TSource, Double>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Sum<string> ((Func<string, Double>) (x => 0d)); });
			AssertException<ArgumentNullException> (delegate () { data.Sum<string> ((Func<string, Double>) null); });

			// Sum<TSource> (Func<TSource, Nullable<Double>>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Sum<string> ((Func<string, Nullable<Double>>) (x => (int?) 0d)); });
			AssertException<ArgumentNullException> (delegate () { data.Sum<string> ((Func<string, Nullable<Double>>) null); });

			// Sum<TSource> (Func<TSource, Decimal>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Sum<string> ((Func<string, Decimal>) (x => 0m)); });
			AssertException<ArgumentNullException> (delegate () { data.Sum<string> ((Func<string, Decimal>) null); });

			// Sum<TSource> (Func<TSource, Nullable<Decimal>>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Sum<string> ((Func<string, Nullable<Decimal>>) (x => (int?) 0m)); });
			AssertException<ArgumentNullException> (delegate () { data.Sum<string> ((Func<string, Nullable<Decimal>>) null); });

			// Sum (IEnumerable<int>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<int>) null).Sum (); });

			// Sum (IEnumerable<int?>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<Nullable<int>>) null).Sum (); });

			// Sum (IEnumerable<long>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<Int64>) null).Sum (); });

			// Sum (IEnumerable<long?>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<Nullable<Int64>>) null).Sum (); });

			// Sum (IEnumerable<float>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<Single>) null).Sum (); });

			// Sum (IEnumerable<float?>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<Nullable<Single>>) null).Sum (); });

			// Sum (IEnumerable<double>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<Double>) null).Sum (); });

			// Sum (IEnumerable<double?>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<Nullable<Double>>) null).Sum (); });

			// Sum (IEnumerable<decimal>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<Decimal>) null).Sum (); });

			// Sum (IEnumerable<decimal?>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<Nullable<Decimal>>) null).Sum (); });
		}

		[Test]
		public void SumTest ()
		{
			string [] data = { "2", "3", "5", "5" };

			//TODO: OverflowException

			// Sum<TSource> (Func<TSource, int>)
			Assert.AreEqual (15, ((IEnumerable<string>) data).Sum<string> ((Func<string, int>) (x => int.Parse (x))));
			Assert.AreEqual (0, Enumerable.Empty<string> ().Sum<string> ((Func<string, int>) (x => int.Parse (x))));

			// Sum<TSource> (Func<TSource, Nullable<int>>)
			Assert.AreEqual (15, ((IEnumerable<string>) data).Sum<string> ((Func<string, Nullable<int>>) (x => (int?) int.Parse (x))));
			Assert.AreEqual (0, Enumerable.Empty<string> ().Sum<string> ((Func<string, Nullable<int>>) (x => (int?) int.Parse (x))));

			// Sum<TSource> (Func<TSource, Int64>)
			Assert.AreEqual (15, ((IEnumerable<string>) data).Sum<string> ((Func<string, Int64>) (x => int.Parse (x))));
			Assert.AreEqual (0, Enumerable.Empty<string> ().Sum<string> ((Func<string, Int64>) (x => int.Parse (x))));

			// Sum<TSource> (Func<TSource, Nullable<Int64>>)
			Assert.AreEqual (15, ((IEnumerable<string>) data).Sum<string> ((Func<string, Nullable<Int64>>) (x => (int?) int.Parse (x))));
			Assert.AreEqual (0, Enumerable.Empty<string> ().Sum<string> ((Func<string, Nullable<Int64>>) (x => (int?) int.Parse (x))));

			// Sum<TSource> (Func<TSource, Single>)
			Assert.AreEqual (15, ((IEnumerable<string>) data).Sum<string> ((Func<string, Single>) (x => int.Parse (x))));
			Assert.AreEqual (0, Enumerable.Empty<string> ().Sum<string> ((Func<string, Single>) (x => int.Parse (x))));

			// Sum<TSource> (Func<TSource, Nullable<Single>>)
			Assert.AreEqual (15, ((IEnumerable<string>) data).Sum<string> ((Func<string, Nullable<Single>>) (x => (int?) int.Parse (x))));
			Assert.AreEqual (0, Enumerable.Empty<string> ().Sum<string> ((Func<string, Nullable<Single>>) (x => (int?) int.Parse (x))));

			// Sum<TSource> (Func<TSource, Double>)
			Assert.AreEqual (15, ((IEnumerable<string>) data).Sum<string> ((Func<string, Double>) (x => int.Parse (x))));
			Assert.AreEqual (0, Enumerable.Empty<string> ().Sum<string> ((Func<string, Double>) (x => int.Parse (x))));

			// Sum<TSource> (Func<TSource, Nullable<Double>>)
			Assert.AreEqual (15, ((IEnumerable<string>) data).Sum<string> ((Func<string, Nullable<Double>>) (x => (int?) int.Parse (x))));
			Assert.AreEqual (0, Enumerable.Empty<string> ().Sum<string> ((Func<string, Nullable<Double>>) (x => (int?) int.Parse (x))));

			// Sum<TSource> (Func<TSource, Decimal>)
			Assert.AreEqual (15, ((IEnumerable<string>) data).Sum<string> ((Func<string, Decimal>) (x => int.Parse (x))));
			Assert.AreEqual (0, Enumerable.Empty<string> ().Sum<string> ((Func<string, Decimal>) (x => int.Parse (x))));

			// Sum<TSource> (Func<TSource, Nullable<Decimal>>)
			Assert.AreEqual (15, ((IEnumerable<string>) data).Sum<string> ((Func<string, Nullable<Decimal>>) (x => (int?) int.Parse (x))));
			Assert.AreEqual (0, Enumerable.Empty<string> ().Sum<string> ((Func<string, Nullable<Decimal>>) (x => (int?) int.Parse (x))));

			// Sum<> ()
			Assert.AreEqual (6, ((IEnumerable<int>) new int [] { 1, 2, 3 }).Sum ());
			Assert.AreEqual (0, Enumerable.Empty<int> ().Sum ());

			// Sum<> ()
			Assert.AreEqual (6, ((IEnumerable<Nullable<int>>) new int? [] { 1, 2, 3 }).Sum ());
			Assert.AreEqual (0, Enumerable.Empty<int?> ().Sum ());

			// Sum<> ()
			Assert.AreEqual (6, ((IEnumerable<Int64>) new long [] { 1, 2, 3 }).Sum ());
			Assert.AreEqual (0, Enumerable.Empty<long> ().Sum ());

			// Sum<> ()
			Assert.AreEqual (6, ((IEnumerable<Nullable<Int64>>) new long? [] { 1, 2, 3 }).Sum ());
			Assert.AreEqual (0, Enumerable.Empty<long?> ().Sum ());

			// Sum<> ()
			Assert.AreEqual (6, ((IEnumerable<Single>) new float [] { 1, 2, 3 }).Sum ());
			Assert.AreEqual (0, Enumerable.Empty<float> ().Sum ());

			// Sum<> ()
			Assert.AreEqual (6, ((IEnumerable<Nullable<Single>>) new float? [] { 1, 2, 3 }).Sum ());
			Assert.AreEqual (0, Enumerable.Empty<float?> ().Sum ());

			// Sum<> ()
			Assert.AreEqual (6, ((IEnumerable<Double>) new double [] { 1, 2, 3 }).Sum ());
			Assert.AreEqual (0, Enumerable.Empty<double> ().Sum ());

			// Sum<> ()
			Assert.AreEqual (6, ((IEnumerable<Nullable<Double>>) new double? [] { 1, 2, 3 }).Sum ());
			Assert.AreEqual (0, Enumerable.Empty<double?> ().Sum ());

			// Sum<> ()
			Assert.AreEqual (6, ((IEnumerable<Decimal>) new decimal [] { 1, 2, 3 }).Sum ());
			Assert.AreEqual (0, Enumerable.Empty<decimal> ().Sum ());

			// Sum<> ()
			Assert.AreEqual (6, ((IEnumerable<Nullable<Decimal>>) new decimal? [] { 1, 2, 3 }).Sum ());
			Assert.AreEqual (0, Enumerable.Empty<decimal?> ().Sum ());
		}

		[Test]
		public void MinArgumentNullTest ()
		{
			string [] data = { "2", "1", "5", "3", "4" };


			// Min<TSource> ()
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Min<string> (); });

			// Min<TSource> (Func<TSource, int>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Min<string> ((Func<string, int>) (x => 0)); });
			AssertException<ArgumentNullException> (delegate () { data.Min<string> ((Func<string, int>) null); });

			// Min<TSource> (Func<TSource, Nullable<int>>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Min<string> ((Func<string, Nullable<int>>) (x => (int?) 0)); });
			AssertException<ArgumentNullException> (delegate () { data.Min<string> ((Func<string, Nullable<int>>) null); });

			// Min<TSource> (Func<TSource, Int64>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Min<string> ((Func<string, Int64>) (x => 0L)); });
			AssertException<ArgumentNullException> (delegate () { data.Min<string> ((Func<string, Int64>) null); });

			// Min<TSource> (Func<TSource, Nullable<Int64>>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Min<string> ((Func<string, Nullable<Int64>>) (x => (int?) 0L)); });
			AssertException<ArgumentNullException> (delegate () { data.Min<string> ((Func<string, Nullable<Int64>>) null); });

			// Min<TSource> (Func<TSource, Single>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Min<string> ((Func<string, Single>) (x => 0f)); });
			AssertException<ArgumentNullException> (delegate () { data.Min<string> ((Func<string, Single>) null); });

			// Min<TSource> (Func<TSource, Nullable<Single>>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Min<string> ((Func<string, Nullable<Single>>) (x => (int?) 0f)); });
			AssertException<ArgumentNullException> (delegate () { data.Min<string> ((Func<string, Nullable<Single>>) null); });

			// Min<TSource> (Func<TSource, Double>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Min<string> ((Func<string, Double>) (x => 0d)); });
			AssertException<ArgumentNullException> (delegate () { data.Min<string> ((Func<string, Double>) null); });

			// Min<TSource> (Func<TSource, Nullable<Double>>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Min<string> ((Func<string, Nullable<Double>>) (x => (int?) 0d)); });
			AssertException<ArgumentNullException> (delegate () { data.Min<string> ((Func<string, Nullable<Double>>) null); });

			// Min<TSource> (Func<TSource, Decimal>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Min<string> ((Func<string, Decimal>) (x => 0m)); });
			AssertException<ArgumentNullException> (delegate () { data.Min<string> ((Func<string, Decimal>) null); });

			// Min<TSource> (Func<TSource, Nullable<Decimal>>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Min<string> ((Func<string, Nullable<Decimal>>) (x => (int?) 0m)); });
			AssertException<ArgumentNullException> (delegate () { data.Min<string> ((Func<string, Nullable<Decimal>>) null); });

			// Min<TSource,TSource> (Func<TSource, string>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Min<string, string> ((Func<string, string>) (x => "test")); });
			AssertException<ArgumentNullException> (delegate () { data.Min<string, string> ((Func<string, string>) null); });

			// Min<> ()
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<int>) null).Min (); });

			// Min<> ()
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<Nullable<int>>) null).Min (); });

			// Min<> ()
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<Int64>) null).Min (); });

			// Min<> ()
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<Nullable<Int64>>) null).Min (); });

			// Min<> ()
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<Single>) null).Min (); });

			// Min<> ()
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<Nullable<Single>>) null).Min (); });

			// Min<> ()
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<Double>) null).Min (); });

			// Min<> ()
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<Nullable<Double>>) null).Min (); });

			// Min<> ()
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<Decimal>) null).Min (); });

			// Min<> ()
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<Nullable<Decimal>>) null).Min (); });
		}

		[Test]
		public void MinTest ()
		{
			string [] data = { "2", "1", "5", "3", "4" };


			// Min<TSource> ()
			Assert.AreEqual ("1", ((IEnumerable<string>) data).Min<string> ());


			// Min<TSource> (Func<TSource, int>)
			Assert.AreEqual (1, ((IEnumerable<string>) data).Min<string> ((Func<string, int>) (x => int.Parse (x))));

			// Min<TSource> (Func<TSource, Nullable<int>>)
			Assert.AreEqual (1, ((IEnumerable<string>) data).Min<string> ((Func<string, Nullable<int>>) (x => (int?) int.Parse (x))));

			// Min<TSource> (Func<TSource, Int64>)
			Assert.AreEqual (1, ((IEnumerable<string>) data).Min<string> ((Func<string, Int64>) (x => int.Parse (x))));

			// Min<TSource> (Func<TSource, Nullable<Int64>>)
			Assert.AreEqual (1, ((IEnumerable<string>) data).Min<string> ((Func<string, Nullable<Int64>>) (x => (int?) int.Parse (x))));

			// Min<TSource> (Func<TSource, Single>)
			Assert.AreEqual (1, ((IEnumerable<string>) data).Min<string> ((Func<string, Single>) (x => int.Parse (x))));

			// Min<TSource> (Func<TSource, Nullable<Single>>)
			Assert.AreEqual (1, ((IEnumerable<string>) data).Min<string> ((Func<string, Nullable<Single>>) (x => (int?) int.Parse (x))));

			// Min<TSource> (Func<TSource, Double>)
			Assert.AreEqual (1, ((IEnumerable<string>) data).Min<string> ((Func<string, Double>) (x => int.Parse (x))));

			// Min<TSource> (Func<TSource, Nullable<Double>>)
			Assert.AreEqual (1, ((IEnumerable<string>) data).Min<string> ((Func<string, Nullable<Double>>) (x => (int?) int.Parse (x))));

			// Min<TSource> (Func<TSource, Decimal>)
			Assert.AreEqual (1, ((IEnumerable<string>) data).Min<string> ((Func<string, Decimal>) (x => int.Parse (x))));

			// Min<TSource> (Func<TSource, Nullable<Decimal>>)
			Assert.AreEqual (1, ((IEnumerable<string>) data).Min<string> ((Func<string, Nullable<Decimal>>) (x => (int?) int.Parse (x))));

			// Min<TSource,TSource> (Func<TSource, TSource>)
			Assert.AreEqual ("1", ((IEnumerable<string>) data).Min<string, string> ((Func<string, string>) (x => x)));

			// Min<> ()
			Assert.AreEqual (2, ((IEnumerable<int>) new int [] { 2, 3, 4 }).Min ());

			// Min<> ()
			Assert.AreEqual (2, ((IEnumerable<Nullable<int>>) new int? [] { 2, 3, 4 }).Min ());

			// Min<> ()
			Assert.AreEqual (2, ((IEnumerable<Int64>) new long [] { 2, 3, 4 }).Min ());

			// Min<> ()
			Assert.AreEqual (2, ((IEnumerable<Nullable<Int64>>) new long? [] { 2, 3, 4 }).Min ());

			// Min<> ()
			Assert.AreEqual (2, ((IEnumerable<Single>) new float [] { 2, 3, 4 }).Min ());

			// Min<> ()
			Assert.AreEqual (2, ((IEnumerable<Nullable<Single>>) new float? [] { 2, 3, 4 }).Min ());

			// Min<> ()
			Assert.AreEqual (2, ((IEnumerable<Double>) new double [] { 2, 3, 4 }).Min ());

			// Min<> ()
			Assert.AreEqual (2, ((IEnumerable<Nullable<Double>>) new double? [] { 2, 3, 4 }).Min ());

			// Min<> ()
			Assert.AreEqual (2, ((IEnumerable<Decimal>) new decimal [] { 2, 3, 4 }).Min ());

			// Min<> ()
			Assert.AreEqual (2, ((IEnumerable<Nullable<Decimal>>) new decimal? [] { 2, 3, 4 }).Min ());
		}

		[Test]
		public void MaxArgumentNullTest ()
		{
			string [] data = { "2", "1", "5", "3", "4" };


			// Max<TSource> ()
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Max<string> (); });

			// Max<TSource> (Func<TSource, int>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Max<string> ((Func<string, int>) (x => 0)); });
			AssertException<ArgumentNullException> (delegate () { data.Max<string> ((Func<string, int>) null); });

			// Max<TSource> (Func<TSource, Nullable<int>>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Max<string> ((Func<string, Nullable<int>>) (x => (int?) 0)); });
			AssertException<ArgumentNullException> (delegate () { data.Max<string> ((Func<string, Nullable<int>>) null); });

			// Max<TSource> (Func<TSource, Int64>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Max<string> ((Func<string, Int64>) (x => 0L)); });
			AssertException<ArgumentNullException> (delegate () { data.Max<string> ((Func<string, Int64>) null); });

			// Max<TSource> (Func<TSource, Nullable<Int64>>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Max<string> ((Func<string, Nullable<Int64>>) (x => (int?) 0L)); });
			AssertException<ArgumentNullException> (delegate () { data.Max<string> ((Func<string, Nullable<Int64>>) null); });

			// Max<TSource> (Func<TSource, Single>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Max<string> ((Func<string, Single>) (x => 0f)); });
			AssertException<ArgumentNullException> (delegate () { data.Max<string> ((Func<string, Single>) null); });

			// Max<TSource> (Func<TSource, Nullable<Single>>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Max<string> ((Func<string, Nullable<Single>>) (x => (int?) 0f)); });
			AssertException<ArgumentNullException> (delegate () { data.Max<string> ((Func<string, Nullable<Single>>) null); });

			// Max<TSource> (Func<TSource, Double>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Max<string> ((Func<string, Double>) (x => 0d)); });
			AssertException<ArgumentNullException> (delegate () { data.Max<string> ((Func<string, Double>) null); });

			// Max<TSource> (Func<TSource, Nullable<Double>>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Max<string> ((Func<string, Nullable<Double>>) (x => (int?) 0d)); });
			AssertException<ArgumentNullException> (delegate () { data.Max<string> ((Func<string, Nullable<Double>>) null); });

			// Max<TSource> (Func<TSource, Decimal>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Max<string> ((Func<string, Decimal>) (x => 0m)); });
			AssertException<ArgumentNullException> (delegate () { data.Max<string> ((Func<string, Decimal>) null); });

			// Max<TSource> (Func<TSource, Nullable<Decimal>>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Max<string> ((Func<string, Nullable<Decimal>>) (x => (int?) 0m)); });
			AssertException<ArgumentNullException> (delegate () { data.Max<string> ((Func<string, Nullable<Decimal>>) null); });

			// Max<TSource,TSource> (Func<TSource, TSource>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Max<string, string> ((Func<string, string>) (x => "test")); });
			AssertException<ArgumentNullException> (delegate () { data.Max<string, string> ((Func<string, string>) null); });

			// Max<> ()
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<int>) null).Max (); });

			// Max<> ()
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<Nullable<int>>) null).Max (); });

			// Max<> ()
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<Int64>) null).Max (); });

			// Max<> ()
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<Nullable<Int64>>) null).Max (); });

			// Max<> ()
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<Double>) null).Max (); });

			// Max<> ()
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<Nullable<Double>>) null).Max (); });

			// Max<> ()
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<Single>) null).Max (); });

			// Max<> ()
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<Nullable<Single>>) null).Max (); });

			// Max<> ()
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<Decimal>) null).Max (); });

			// Max<> ()
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<Nullable<Decimal>>) null).Max (); });
		}

		[Test]
		public void MaxTest ()
		{
			string [] data = { "2", "1", "5", "3", "4" };


			// Max<string> ()
			Assert.AreEqual ("5", ((IEnumerable<string>) data).Max<string> ());

			// Max<TSource> (Func<TSource, int>)
			Assert.AreEqual (5, ((IEnumerable<string>) data).Max<string> ((Func<string, int>) (x => int.Parse (x))));

			// Max<TSource> (Func<TSource, Nullable<int>>)
			Assert.AreEqual (5, ((IEnumerable<string>) data).Max<string> ((Func<string, Nullable<int>>) (x => (int?) int.Parse (x))));

			// Max<TSource> (Func<TSource, Int64>)
			Assert.AreEqual (5, ((IEnumerable<string>) data).Max<string> ((Func<string, Int64>) (x => int.Parse (x))));

			// Max<TSource> (Func<TSource, Nullable<Int64>>)
			Assert.AreEqual (5, ((IEnumerable<string>) data).Max<string> ((Func<string, Nullable<Int64>>) (x => (int?) int.Parse (x))));

			// Max<TSource> (Func<TSource, Single>)
			Assert.AreEqual (5, ((IEnumerable<string>) data).Max<string> ((Func<string, Single>) (x => int.Parse (x))));

			// Max<TSource> (Func<TSource, Nullable<Single>>)
			Assert.AreEqual (5, ((IEnumerable<string>) data).Max<string> ((Func<string, Nullable<Single>>) (x => (int?) int.Parse (x))));

			// Max<TSource> (Func<TSource, Double>)
			Assert.AreEqual (5, ((IEnumerable<string>) data).Max<string> ((Func<string, Double>) (x => int.Parse (x))));

			// Max<TSource> (Func<TSource, Nullable<Double>>)
			Assert.AreEqual (5, ((IEnumerable<string>) data).Max<string> ((Func<string, Nullable<Double>>) (x => (int?) int.Parse (x))));

			// Max<TSource> (Func<TSource, Decimal>)
			Assert.AreEqual (5, ((IEnumerable<string>) data).Max<string> ((Func<string, Decimal>) (x => int.Parse (x))));

			// Max<TSource> (Func<TSource, Nullable<Decimal>>)
			Assert.AreEqual (5, ((IEnumerable<string>) data).Max<string> ((Func<string, Nullable<Decimal>>) (x => (int?) int.Parse (x))));

			// Max<TSource,TSource> (Func<TSource, TSource>)
			Assert.AreEqual ("5", ((IEnumerable<string>) data).Max<string, string> ((Func<string, string>) (x => x)));

			// Max<> ()
			Assert.AreEqual (4, ((IEnumerable<int>) new int [] { 2, 3, 4 }).Max ());

			// Max<> ()
			Assert.AreEqual (4, ((IEnumerable<Nullable<int>>) new int? [] { 2, 3, 4 }).Max ());

			// Max<> ()
			Assert.AreEqual (4, ((IEnumerable<Int64>) new long [] { 2, 3, 4 }).Max ());

			// Max<> ()
			Assert.AreEqual (4, ((IEnumerable<Nullable<Int64>>) new long? [] { 2, 3, 4 }).Max ());

			// Max<> ()
			Assert.AreEqual (4, ((IEnumerable<Single>) new float [] { 2, 3, 4 }).Max ());

			// Max<> ()
			Assert.AreEqual (4, ((IEnumerable<Nullable<Single>>) new float? [] { 2, 3, 4 }).Max ());

			// Max<> ()
			Assert.AreEqual (4, ((IEnumerable<Double>) new double [] { 2, 3, 4 }).Max ());

			// Max<> ()
			Assert.AreEqual (4, ((IEnumerable<Nullable<Double>>) new double? [] { 2, 3, 4 }).Max ());

			// Max<> ()
			Assert.AreEqual (4, ((IEnumerable<Decimal>) new decimal [] { 2, 3, 4 }).Max ());

			// Max<> ()
			Assert.AreEqual (4, ((IEnumerable<Nullable<Decimal>>) new decimal? [] { 2, 3, 4 }).Max ());
		}

		[Test]
		public void AverageArgumentNullTest ()
		{
			string [] data = { "2", "1", "5", "3", "4" };


			// Average<TSource> (Func<TSource, int>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Average<string> ((Func<string, int>) (x => 0)); });
			AssertException<ArgumentNullException> (delegate () { data.Average<string> ((Func<string, int>) null); });

			// Average<TSource> (Func<TSource, Nullable<int>>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Average<string> ((Func<string, Nullable<int>>) (x => (int?) 0)); });
			AssertException<ArgumentNullException> (delegate () { data.Average<string> ((Func<string, Nullable<int>>) null); });

			// Average<TSource> (Func<TSource, Int64>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Average<string> ((Func<string, Int64>) (x => 0L)); });
			AssertException<ArgumentNullException> (delegate () { data.Average<string> ((Func<string, Int64>) null); });

			// Average<TSource> (Func<TSource, Nullable<Int64>>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Average<string> ((Func<string, Nullable<Int64>>) (x => (int?) 0L)); });
			AssertException<ArgumentNullException> (delegate () { data.Average<string> ((Func<string, Nullable<Int64>>) null); });

			// Average<TSource> (Func<TSource, Single>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Average<string> ((Func<string, Single>) (x => 0f)); });
			AssertException<ArgumentNullException> (delegate () { data.Average<string> ((Func<string, Single>) null); });

			// Average<TSource> (Func<TSource, Nullable<Single>>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Average<string> ((Func<string, Nullable<Single>>) (x => (int?) 0f)); });
			AssertException<ArgumentNullException> (delegate () { data.Average<string> ((Func<string, Nullable<Single>>) null); });

			// Average<TSource> (Func<TSource, Double>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Average<string> ((Func<string, Double>) (x => 0d)); });
			AssertException<ArgumentNullException> (delegate () { data.Average<string> ((Func<string, Double>) null); });

			// Average<TSource> (Func<TSource, Nullable<Double>>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Average<string> ((Func<string, Nullable<Double>>) (x => (int?) 0d)); });
			AssertException<ArgumentNullException> (delegate () { data.Average<string> ((Func<string, Nullable<Double>>) null); });

			// Average<TSource> (Func<TSource, Decimal>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Average<string> ((Func<string, Decimal>) (x => 0m)); });
			AssertException<ArgumentNullException> (delegate () { data.Average<string> ((Func<string, Decimal>) null); });

			// Average<TSource> (Func<TSource, Nullable<Decimal>>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Average<string> ((Func<string, Nullable<Decimal>>) (x => (int?) 0m)); });
			AssertException<ArgumentNullException> (delegate () { data.Average<string> ((Func<string, Nullable<Decimal>>) null); });

			// Average<> ()
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<int>) null).Average (); });

			// Average<> ()
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<Nullable<int>>) null).Average (); });

			// Average<> ()
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<Int64>) null).Average (); });

			// Average<> ()
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<Nullable<Int64>>) null).Average (); });

			// Average<> ()
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<Single>) null).Average (); });

			// Average<> ()
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<Nullable<Single>>) null).Average (); });

			// Average<> ()
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<Double>) null).Average (); });

			// Average<> ()
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<Nullable<Double>>) null).Average (); });

			// Average<> ()
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<Decimal>) null).Average (); });

			// Average<> ()
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<Nullable<Decimal>>) null).Average (); });
		}

		[Test]
		public void AverageTest ()
		{
			string [] data = { "2", "1", "5", "3", "4" };
			string [] empty = { };

			// Average<string> (Func<string, int>)
			Assert.AreEqual (3, ((IEnumerable<string>) data).Average<string> ((Func<string, int>) (x => int.Parse (x))));
			AssertException<InvalidOperationException> (delegate () { empty.Average ((Func<string, int>) (x => int.Parse (x))); });

			// Average<TSource> (Func<TSource, Nullable<int>>)
			Assert.AreEqual (3, ((IEnumerable<string>) data).Average<string> ((Func<string, Nullable<int>>) (x => (int?) int.Parse (x))));

			// Average<TSource> (Func<TSource, Int64>)
			Assert.AreEqual (3, ((IEnumerable<string>) data).Average<string> ((Func<string, long>) (x => int.Parse (x))));
			AssertException<InvalidOperationException> (delegate () { empty.Average ((Func<string, long>) (x => int.Parse (x))); });

			// Average<TSource> (Func<TSource, Nullable<Int64>>)
			Assert.AreEqual (3, ((IEnumerable<string>) data).Average<string> ((Func<string, long?>) (x => (int?) int.Parse (x))));

			// Average<TSource> (Func<TSource, Single>)
			Assert.AreEqual (3, ((IEnumerable<string>) data).Average<string> ((Func<string, float>) (x => int.Parse (x))));
			AssertException<InvalidOperationException> (delegate () { empty.Average ((Func<string, float>) (x => int.Parse (x))); });

			// Average<TSource> (Func<TSource, Nullable<Single>>)
			Assert.AreEqual (3, ((IEnumerable<string>) data).Average<string> ((Func<string, float?>) (x => (int?) int.Parse (x))));

			// Average<TSource> (Func<TSource, Double>)
			Assert.AreEqual (3, ((IEnumerable<string>) data).Average<string> ((Func<string, double>) (x => int.Parse (x))));
			AssertException<InvalidOperationException> (delegate () { empty.Average ((Func<string, double>) (x => int.Parse (x))); });

			// Average<TSource> (Func<TSource, Nullable<Double>>)
			Assert.AreEqual (3, ((IEnumerable<string>) data).Average<string> ((Func<string, double?>) (x => (int?) int.Parse (x))));

			// Average<TSource> (Func<TSource, Decimal>)
			Assert.AreEqual (3, ((IEnumerable<string>) data).Average<string> ((Func<string, decimal>) (x => int.Parse (x))));
			AssertException<InvalidOperationException> (delegate () { empty.Average ((Func<string, decimal>) (x => int.Parse (x))); });

			// Average<TSource> (Func<TSource, Nullable<Decimal>>)
			Assert.AreEqual (3, ((IEnumerable<string>) data).Average<string> ((Func<string, decimal?>) (x => (int?) int.Parse (x))));


			// Average<> ()
			Assert.AreEqual (3, ((IEnumerable<int>) new int [] { 2, 3, 4 }).Average ());
			AssertException<InvalidOperationException> (delegate () { new int [0].Average (); });

			// Average<> ()
			Assert.AreEqual (3, ((IEnumerable<Nullable<int>>) new int? [] { 2, 3, 4 }).Average ());

			// Average<> ()
			Assert.AreEqual (3, ((IEnumerable<Int64>) new long [] { 2, 3, 4 }).Average ());
			AssertException<InvalidOperationException> (delegate () { new long [0].Average (); });

			// Average<> ()
			Assert.AreEqual (3, ((IEnumerable<Nullable<Int64>>) new long? [] { 2, 3, 4 }).Average ());

			// Average<> ()
			Assert.AreEqual (3, ((IEnumerable<Single>) new float [] { 2, 3, 4 }).Average ());
			AssertException<InvalidOperationException> (delegate () { new float [0].Average (); });

			// Average<> ()
			Assert.AreEqual (3, ((IEnumerable<Nullable<Single>>) new float? [] { 2, 3, 4 }).Average ());

			// Average<> ()
			Assert.AreEqual (3, ((IEnumerable<Double>) new double [] { 2, 3, 4 }).Average ());
			AssertException<InvalidOperationException> (delegate () { new double [0].Average (); });

			// Average<> ()
			Assert.AreEqual (3, ((IEnumerable<Nullable<Double>>) new double? [] { 2, 3, 4 }).Average ());

			// Average<> ()
			Assert.AreEqual (3, ((IEnumerable<Decimal>) new decimal [] { 2, 3, 4 }).Average ());
			AssertException<InvalidOperationException> (delegate () { new decimal [0].Average (); });

			// Average<> ()
			Assert.AreEqual (3, ((IEnumerable<Nullable<Decimal>>) new decimal? [] { 2, 3, 4 }).Average ());
		}

		[Test]
		public void WhereArgumentNullTest ()
		{
			string [] data = { "2", "1", "5", "3", "4" };

			// Where<TSource> (Func<TSource, bool>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Where (x => true); });
			AssertException<ArgumentNullException> (delegate () { data.Where ((Func<string, bool>) null); });

			// Where<TSource> (Func<TSource, int, bool>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Where ((x, y) => true); });
			AssertException<ArgumentNullException> (delegate () { data.Where ((Func<string, int, bool>) null); });
		}

		[Test]
		public void WhereTest ()
		{
			int [] data = { 2, 1, 5, 3, 4 };
			int [] expected1 = { 2, 1 };
			int [] expected2 = { 2 };

			// Where<TSource> (Func<TSource, bool>)
			AssertAreSame (expected1, data.Where (x => x < 3));

			// Where<TSource> (Func<TSource, int, bool>)
			AssertAreSame (expected2, data.Where ((x, y) => x < 3 && y != 1));
		}

		[Test]
		public void SelectArgumentNullTest ()
		{
			string [] data = { "2", "1", "5", "3", "4" };


			// Select<TSource,TResult> (Func<TSource, TResult>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Select (x => "test"); });
			AssertException<ArgumentNullException> (delegate () { data.Select ((Func<string, string>) null); });

			// Select<TSource,TResult> (Func<TSource, int, TResult>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Select ((x, y) => "test"); });
			AssertException<ArgumentNullException> (delegate () { data.Select ((Func<string, int, string>) null); });
		}

		[Test]
		public void SelectTest ()
		{
			string [] data = { "2", "1", "5", "3", "4" };
			string [] expected1 = { "2x", "1x", "5x", "3x", "4x" };
			string [] expected2 = { "2x0", "1x1", "5x2", "3x3", "4x4" };


			// Select<TSource,TResult> (Func<TSource, TResult>)
			AssertAreSame (expected1, data.Select<string, string> (x => x + "x"));

			// Select<TSource,TResult> (Func<TSource, int, TResult>)
			AssertAreSame (expected2, data.Select<string, string> ((x, y) => x + "x" + y));
		}

		[Test]
		public void SelectManyArgumentNullTest ()
		{
			string [] data = { "2", "1", "5", "3", "4" };


			// SelectMany<TSource,TResult> (Func<TSource, IEnumerable<TResult>>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).SelectMany (x => data); });
			AssertException<ArgumentNullException> (delegate () { data.SelectMany ((Func<string, IEnumerable<string>>) null); });

			// SelectMany<TSource,TResult> (Func<TSource, int, IEnumerable<TResult>>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).SelectMany ((x, y) => data); });
			AssertException<ArgumentNullException> (delegate () { data.SelectMany ((Func<string, int, IEnumerable<string>>) null); });

			// SelectMany<TSource,TCollection,TResult> (Func<string, int, IEnumerable<TCollection>>, Func<TSource, TCollection, TResult>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).SelectMany ((x, y) => data, (x, y) => "test"); });
			AssertException<ArgumentNullException> (delegate () { data.SelectMany ((Func<string, int, IEnumerable<string>>) null, (x, y) => "test"); });
			AssertException<ArgumentNullException> (delegate () { data.SelectMany ((x, y) => data, (Func<string, string, string>) null); });

			// SelectMany<TSource,TCollection,TResult> (Func<TSource, IEnumerable<TCollection>>, Func<TSource, TCollection, TResult>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).SelectMany (x => data, (x, y) => "test"); });
			AssertException<ArgumentNullException> (delegate () { data.SelectMany ((Func<string, IEnumerable<string>>) null, (x, y) => "test"); });
			AssertException<ArgumentNullException> (delegate () { data.SelectMany (x => data, (Func<string, string, string>) null); });
		}

		[Test]
		public void SelectManyTest ()
		{
			string [] data = { "0", "1" };
			string [] expected = { "0", "00", "1", "11" };

			// SelectMany<TSource,TResult> (Func<TSource, IEnumerable<TResult>>)
			AssertAreSame (expected, ((IEnumerable<string>) data).SelectMany (x => new string [] { x, x + x }));

			// SelectMany<TSource,TResult> (Func<TSource, int, IEnumerable<TResult>>)
			AssertAreSame (expected, ((IEnumerable<string>) data).SelectMany ((x, y) => new string [] { x, x + y }));

			// SelectMany<TSource,TCollection,TResult> (Func<string, int, IEnumerable<TCollection>>, Func<TSource, TCollection, TResult>)
			AssertAreSame (expected, ((IEnumerable<string>) data).SelectMany ((x, y) => new string [] { x, x + y }, (x, y) => y));

			// SelectMany<TSource,TCollection,TResult> (Func<TSource, IEnumerable<TCollection>>, Func<TSource, TCollection, TResult>)
			AssertAreSame (expected, ((IEnumerable<string>) data).SelectMany (x => new string [] { x, x + x }, (x, y) => y));
		}

		[Test]
		public void TakeArgumentNullTest ()
		{
			//string [] data = { "2", "1", "5", "3", "4" };


			// Take<TSource> (int)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Take (0); });
		}

		[Test]
		public void TakeTest ()
		{
			int [] data = { 2, 1, 5, 3, 1 };
			int [] expected = { 2, 1 };
			int [] empty = { };

			// Take<TSource> (int)
			AssertAreSame (expected, data.Take (2));
			AssertAreSame (empty, data.Take (-2));
		}

		[Test]
		public void TakeWhileArgumentNullTest ()
		{
			string [] data = { "2", "1", "5", "3", "4" };


			// TakeWhile<TSource> (Func<TSource, bool>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).TakeWhile (x => true); });
			AssertException<ArgumentNullException> (delegate () { data.TakeWhile ((Func<string, bool>) null); });

			// TakeWhile<TSource> (Func<TSource, int, bool>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).TakeWhile ((x, y) => true); });
			AssertException<ArgumentNullException> (delegate () { data.TakeWhile ((Func<string, int, bool>) null); });
		}

		[Test]
		public void TakeWhileTest ()
		{
			int [] data = { 2, 1, 5, 3, 1 };
			int [] expected = { 2, 1 };


			// TakeWhile<TSource> (Func<TSource, bool>)
			AssertAreSame (expected, data.TakeWhile (x => x != 5));

			// TakeWhile<TSource> (Func<TSource, int, bool>)
			AssertAreSame (expected, data.TakeWhile ((x, y) => y != 2));
		}

		[Test]
		public void SkipArgumentNullTest ()
		{
			//string [] data = { "2", "1", "5", "3", "4" };


			// Skip<TSource> (int)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Skip (0); });
		}

		[Test]
		public void SkipTest ()
		{
			int [] data = { 2, 1, 5, 3, 1 };
			int [] expected = { 5, 3, 1 };

			// Skip<string> (TSource)
			AssertAreSame (expected, data.Skip (2));
			AssertAreSame (data, data.Skip (-2));
		}

		[Test]
		public void SkipWhileArgumentNullTest ()
		{
			string [] data = { "2", "1", "5", "3", "4" };


			// SkipWhile<TSource> (Func<TSource, bool>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).SkipWhile (x => true); });
			AssertException<ArgumentNullException> (delegate () { data.SkipWhile ((Func<string, bool>) null); });

			// SkipWhile<TSource> (Func<TSource, int, bool>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).SkipWhile ((x, y) => true); });
			AssertException<ArgumentNullException> (delegate () { data.SkipWhile ((Func<string, int, bool>) null); });
		}

		[Test]
		public void SkipWhileTest ()
		{
			int [] data = { 2, 1, 5, 3, 1 };
			int [] expected = { 5, 3, 1 };



			// SkipWhile<TSource> (Func<TSource, bool>)
			AssertAreSame (expected, data.SkipWhile (x => x != 5));

			// SkipWhile<TSource> (Func<TSource, int, bool>)
			AssertAreSame (expected, data.SkipWhile ((x, y) => y != 2));
		}

		[Test]
		public void JoinArgumentNullTest ()
		{
			string [] data = { "2", "1", "5", "3", "4" };


			// Join<TOuter,TInner,TKey,TResult> (IEnumerable<TInner>, Func<TOuter, TKey>, Func<TInner, TKey>, Func<TOuter, TInner, TResult>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Join (data, x => "test", x => "test", (x, y) => "test"); });
			AssertException<ArgumentNullException> (delegate () { data.Join ((IEnumerable<string>) null, x => "test", x => "test", (x, y) => "test"); });
			AssertException<ArgumentNullException> (delegate () { data.Join (data, (Func<string, string>) null, x => "test", (x, y) => "test"); });
			AssertException<ArgumentNullException> (delegate () { data.Join (data, x => "test", (Func<string, string>) null, (x, y) => "test"); });
			AssertException<ArgumentNullException> (delegate () { data.Join (data, x => "test", x => "test", (Func<string, string, string>) null); });

			// Join<TOuter,TInner,TKey,TResult> (IEnumerable<TInner>, Func<TOuter, TKey>, Func<TInner, TKey>, Func<TOuter, TInner, TResult>, IEqualityComparer<string>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Join (data, x => "test", x => "test", (x, y) => "test", EqualityComparer<string>.Default); });
			AssertException<ArgumentNullException> (delegate () { data.Join ((IEnumerable<string>) null, x => "test", x => "test", (x, y) => "test", EqualityComparer<string>.Default); });
			AssertException<ArgumentNullException> (delegate () { data.Join (data, (Func<string, string>) null, x => "test", (x, y) => "test", EqualityComparer<string>.Default); });
			AssertException<ArgumentNullException> (delegate () { data.Join (data, x => "test", (Func<string, string>) null, (x, y) => "test", EqualityComparer<string>.Default); });
			AssertException<ArgumentNullException> (delegate () { data.Join (data, x => "test", x => "test", (Func<string, string, string>) null, EqualityComparer<string>.Default); });
		}

		[Test]
		public void JoinTest ()
		{
			string [] dataOuter1 = { "2", "1", "5", "3", "4" };
			string [] dataInner1 = { "7", "3", "5", "8", "9" };
			string [] expected1 = { "55", "33" };

			string [] dataOuter2 = { "2", "1", "3", "4" };
			string [] dataInner2 = { "7", "5", "8", "9" };
			string [] expected2 = { };


			// Join<TOuter,TInner,TKey,TResult> (IEnumerable<TInner>, Func<TOuter, TKey>, Func<TInner, TKey>, Func<TOuter, TInner, TResult>)
			AssertAreSame (expected1, dataOuter1.Join (dataInner1, x => x, x => x, (x, y) => x + y));
			AssertAreSame (expected2, dataOuter2.Join (dataInner2, x => x, x => x, (x, y) => x + y));

			// Join<TOuter,TInner,TKey,TResult> (IEnumerable<TInner>, Func<TOuter, TKey>, Func<TInner, TKey>, Func<TOuter, TInner, TResult>, IEqualityComparer<string>)
			AssertAreSame (expected1, dataOuter1.Join (dataInner1, x => x, x => x, (x, y) => x + y, EqualityComparer<string>.Default));
			AssertAreSame (expected2, dataOuter2.Join (dataInner2, x => x, x => x, (x, y) => x + y, EqualityComparer<string>.Default));
		}

		[Test]
		public void GroupJoinArgumentNullTest ()
		{
			string [] data = { "2", "1", "5", "3", "4" };


			// GroupJoin<TOuter,TInner,TKey,TResult> (IEnumerable<TInner>, Func<TOuter, TKey>, Func<TInner, TKey>, Func<TOuter, IEnumerable<TInner>, TResult>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).GroupJoin (data, x => "test", x => "test", (x, y) => "test"); });
			AssertException<ArgumentNullException> (delegate () { data.GroupJoin ((IEnumerable<string>) null, x => "test", x => "test", (x, y) => "test"); });
			AssertException<ArgumentNullException> (delegate () { data.GroupJoin (data, (Func<string, string>) null, x => "test", (x, y) => "test"); });
			AssertException<ArgumentNullException> (delegate () { data.GroupJoin (data, x => "test", (Func<string, string>) null, (x, y) => "test"); });
			AssertException<ArgumentNullException> (delegate () { data.GroupJoin (data, x => "test", x => "test", (Func<string, IEnumerable<string>, string>) null); });

			// GroupJoin<TOuter,TInner,TKey,TResult> (IEnumerable<TInner>, Func<TOuter, TKey>, Func<TInner, TKey>, Func<TOuter, IEnumerable<TInner>, TResult, IEqualityComparer<TKey>>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).GroupJoin (data, x => "test", x => "test", (x, y) => "test", EqualityComparer<string>.Default); });
			AssertException<ArgumentNullException> (delegate () { data.GroupJoin ((IEnumerable<string>) null, x => "test", x => "test", (x, y) => "test", EqualityComparer<string>.Default); });
			AssertException<ArgumentNullException> (delegate () { data.GroupJoin (data, (Func<string, string>) null, x => "test", (x, y) => "test", EqualityComparer<string>.Default); });
			AssertException<ArgumentNullException> (delegate () { data.GroupJoin (data, x => "test", (Func<string, string>) null, (x, y) => "test", EqualityComparer<string>.Default); });
			AssertException<ArgumentNullException> (delegate () { data.GroupJoin (data, x => "test", x => "test", (Func<string, IEnumerable<string>, string>) null, EqualityComparer<string>.Default); });
		}

		[Test]
		public void GroupJoinTest ()
		{
			string [] dataOuter1 = { "2", "1", "5", "3", "4" };
			string [] dataInner1 = { "7", "3", "5", "3", "9" };
			string [] expected1 = { "2", "1", "55", "333", "4" };

			string [] dataOuter2 = { "2", "1", "5", "8", "4" };
			string [] dataInner2 = { "7", "3", "6", "3", "9" };
			string [] expected2 = { "2", "1", "5", "8", "4" };


			// GroupJoin<TOuter,TInner,TKey,TResult> (IEnumerable<TInner>, Func<TOuter, TKey>, Func<TInner, TKey>, Func<TOuter, IEnumerable<TInner>, TResult>)
			AssertAreSame (expected1, (dataOuter1.GroupJoin (dataInner1, x => x, x => x, (x, y) => { foreach (var s in y) x += s; return x; })));
			AssertAreSame (expected2, (dataOuter2.GroupJoin (dataInner2, x => x, x => x, (x, y) => { foreach (var s in y) x += s; return x; })));

			// GroupJoin<TOuter,TInner,TKey,TResult> (IEnumerable<TInner>, Func<TOuter, TKey>, Func<TInner, TKey>, Func<TOuter, IEnumerable<TInner>, TResult, IEqualityComparer<TKey>>)
			AssertAreSame (expected1, dataOuter1.GroupJoin (dataInner1, x => x, x => x, (x, y) => { foreach (var s in y) x += s; return x; }, EqualityComparer<string>.Default));
			AssertAreSame (expected2, dataOuter2.GroupJoin (dataInner2, x => x, x => x, (x, y) => { foreach (var s in y) x += s; return x; }, EqualityComparer<string>.Default));
		}

		[Test]
		public void OrderByArgumentNullTest ()
		{
			string [] data = { "2", "1", "5", "3", "4" };


			// OrderBy<TSource,TKey> (Func<TSource, TKey>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).OrderBy (x => "test"); });
			AssertException<ArgumentNullException> (delegate () { data.OrderBy ((Func<string, string>) null); });

			// OrderBy<TSource,TKey> (Func<TSource, TKey>, IComparer<string>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).OrderBy (x => "test", Comparer<string>.Default); });
			AssertException<ArgumentNullException> (delegate () { data.OrderBy ((Func<string, string>) null, Comparer<string>.Default); });
		}

		[Test]
		public void OrderByTest ()
		{
			int [] data = { 2, 1, 5, 3, 4 };
			int [] expected = { 1, 2, 3, 4, 5 };


			// OrderBy<TSource,TKey> (Func<TSource, TKey>)
			AssertAreSame (expected, data.OrderBy (x => x));

			// OrderBy<TSource,TKey> (Func<TSource, TKey>, IComparer<string>)
			AssertAreSame (expected, data.OrderBy (x => x, Comparer<int>.Default));
		}

		[Test]
		public void OrderByDescendingArgumentNullTest ()
		{
			string [] data = { "2", "1", "5", "3", "4" };


			// OrderByDescending<TSource,TKey> (Func<TSource, TKey>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).OrderByDescending (x => "test"); });
			AssertException<ArgumentNullException> (delegate () { data.OrderByDescending ((Func<string, string>) null); });

			// OrderByDescending<TSource,TKey> (Func<TSource, TKey>, IComparer<string>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).OrderByDescending (x => "test", Comparer<string>.Default); });
			AssertException<ArgumentNullException> (delegate () { data.OrderByDescending ((Func<string, string>) null, Comparer<string>.Default); });
		}

		[Test]
		public void OrderByDescendingTest ()
		{
			int [] data = { 2, 1, 5, 3, 4 };
			int [] expected = { 5, 4, 3, 2, 1 };


			// OrderByDescending<TSource,TKey> (Func<TSource, TKey>)
			AssertAreSame (expected, data.OrderByDescending (x => x));

			// OrderByDescending<TSource,TKey> (Func<TSource, TKey>, IComparer<string>)
			AssertAreSame (expected, data.OrderByDescending (x => x, Comparer<int>.Default));
		}

		[Test]
		public void ThenByArgumentNullTest ()
		{
			string [] data = { "2", "1", "5", "3", "4" };


			// ThenBy<TSource,TKey> (Func<TSource, TKey>)
			AssertException<ArgumentNullException> (delegate () {
				((IOrderedEnumerable<string>) null).ThenBy (x => "test");
			});
			AssertException<ArgumentNullException> (delegate () {
				data.OrderBy (x => x).ThenBy ((Func<string, string>) null);
			});

			// ThenBy<TSource,TKey> (Func<TSource, TKey>, IComparer<string>)
			AssertException<ArgumentNullException> (delegate () {

				((IOrderedEnumerable<string>) null).ThenBy (x => "test", Comparer<string>.Default);
			});
			AssertException<ArgumentNullException> (delegate () {

				data.OrderBy (x => x).ThenBy ((Func<string, string>) null, Comparer<string>.Default);
			});
		}

		[Test]
		public void ThenByTest ()
		{
			int [] data = { 2, 1, 5, 3, 4 };
			int [] expected = { 1, 2, 3, 4, 5 };


			// ThenBy<TSource,TKey> (Func<TSource, TKey>)
			AssertAreSame (expected, data.OrderBy (x => x).ThenBy (x => x));

			// ThenBy<TSource,TKey> (Func<TSource, TKey>, IComparer<string>)
			AssertAreSame (expected, data.OrderBy (x => x).ThenBy (x => x, Comparer<int>.Default));
		}

		[Test]
		public void ThenByDescendingArgumentNullTest ()
		{
			string [] data = { "2", "1", "5", "3", "4" };


			// ThenByDescending<TSource,TKey> (Func<TSource, TKey>)
			AssertException<ArgumentNullException> (delegate () {
				((IOrderedEnumerable<string>) null).ThenByDescending (x => "test");
			});
			AssertException<ArgumentNullException> (delegate () {
				data.OrderBy (x => x).ThenByDescending ((Func<string, string>) null);
			});

			// ThenByDescending<TSource,TKey> (Func<TSource, TKey>, IComparer<string>)
			AssertException<ArgumentNullException> (delegate () {

				((IOrderedEnumerable<string>) null).ThenByDescending (x => "test", Comparer<string>.Default);
			});
			AssertException<ArgumentNullException> (delegate () {

				data.OrderBy (x => x).ThenByDescending ((Func<string, string>) null, Comparer<string>.Default);
			});
		}

		[Test]
		public void ThenByDescendingTest ()
		{
			int [] data = { 2, 1, 5, 3, 4 };
			int [] expected = { 5, 4, 3, 2, 1 };


			// ThenByDescending<TSource,TKey> (Func<TSource, TKey>)
			AssertAreSame (expected, data.OrderBy (x => 0).ThenByDescending (x => x));

			// ThenByDescending<TSource,TKey> (Func<TSource, TKey>, IComparer<string>)
			AssertAreSame (expected, data.OrderBy (x => 0).ThenByDescending (x => x, Comparer<int>.Default));
		}

		[Test]
		public void GroupByArgumentNullTest ()
		{
			string [] data = { "2", "1", "5", "3", "4" };


			// GroupBy<string,string> (Func<string, string>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).GroupBy<string, string> ((Func<string, string>) (x => "test")); });
			AssertException<ArgumentNullException> (delegate () { data.GroupBy<string, string> ((Func<string, string>) null); });

			// GroupBy<string,string> (Func<string, string>, IEqualityComparer<string>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).GroupBy<string, string> ((Func<string, string>) (x => "test"), (IEqualityComparer<string>) EqualityComparer<string>.Default); });
			AssertException<ArgumentNullException> (delegate () { data.GroupBy<string, string> ((Func<string, string>) null, (IEqualityComparer<string>) EqualityComparer<string>.Default); });

			// GroupBy<string,string,string> (Func<string, string>, Func<string, string>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).GroupBy<string, string, string> ((Func<string, string>) (x => "test"), (Func<string, string>) (x => "test")); });
			AssertException<ArgumentNullException> (delegate () { data.GroupBy<string, string, string> ((Func<string, string>) null, (Func<string, string>) (x => "test")); });
			AssertException<ArgumentNullException> (delegate () { data.GroupBy<string, string, string> ((Func<string, string>) (x => "test"), (Func<string, string>) null); });

			// GroupBy<string,string,string> (Func<string, string>, Func<string, string>, IEqualityComparer<string>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).GroupBy<string, string, string> ((Func<string, string>) (x => "test"), (Func<string, string>) (x => "test"), (IEqualityComparer<string>) EqualityComparer<string>.Default); });
			AssertException<ArgumentNullException> (delegate () { data.GroupBy<string, string, string> ((Func<string, string>) null, (Func<string, string>) (x => "test"), (IEqualityComparer<string>) EqualityComparer<string>.Default); });
			AssertException<ArgumentNullException> (delegate () { data.GroupBy<string, string, string> ((Func<string, string>) (x => "test"), (Func<string, string>) null, (IEqualityComparer<string>) EqualityComparer<string>.Default); });

			// GroupBy<string,string,string> (Func<string, string>, Func<string, IEnumerable<string>, string>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).GroupBy<string, string, string> ((Func<string, string>) (x => "test"), (Func<string, IEnumerable<string>, string>) ((x, y) => "test")); });
			AssertException<ArgumentNullException> (delegate () { data.GroupBy<string, string, string> ((Func<string, string>) null, (Func<string, IEnumerable<string>, string>) ((x, y) => "test")); });
			AssertException<ArgumentNullException> (delegate () { data.GroupBy<string, string, string> ((Func<string, string>) (x => "test"), (Func<string, IEnumerable<string>, string>) null); });

			// GroupBy<string,string,string,string> (Func<string, string>, Func<string, string>, Func<string, IEnumerable<string>, string>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).GroupBy<string, string, string, string> ((Func<string, string>) (x => "test"), (Func<string, string>) (x => "test"), (Func<string, IEnumerable<string>, string>) ((x, y) => "test")); });
			AssertException<ArgumentNullException> (delegate () { data.GroupBy<string, string, string, string> ((Func<string, string>) null, (Func<string, string>) (x => "test"), (Func<string, IEnumerable<string>, string>) ((x, y) => "test")); });
			AssertException<ArgumentNullException> (delegate () { data.GroupBy<string, string, string, string> ((Func<string, string>) (x => "test"), (Func<string, string>) null, (Func<string, IEnumerable<string>, string>) ((x, y) => "test")); });
			AssertException<ArgumentNullException> (delegate () { data.GroupBy<string, string, string, string> ((Func<string, string>) (x => "test"), (Func<string, string>) (x => "test"), (Func<string, IEnumerable<string>, string>) null); });

			// GroupBy<string,string,string> (Func<string, string>, Func<string, IEnumerable<string>, string>, IEqualityComparer<string>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).GroupBy<string, string, string> ((Func<string, string>) (x => "test"), (Func<string, IEnumerable<string>, string>) ((x, y) => "test"), (IEqualityComparer<string>) EqualityComparer<string>.Default); });
			AssertException<ArgumentNullException> (delegate () { data.GroupBy<string, string, string> ((Func<string, string>) null, (Func<string, IEnumerable<string>, string>) ((x, y) => "test"), (IEqualityComparer<string>) EqualityComparer<string>.Default); });
			AssertException<ArgumentNullException> (delegate () { data.GroupBy<string, string, string> ((Func<string, string>) (x => "test"), (Func<string, IEnumerable<string>, string>) null, (IEqualityComparer<string>) EqualityComparer<string>.Default); });

			// GroupBy<string,string,string,string> (Func<string, string>, Func<string, string>, Func<string, IEnumerable<string>, string>, IEqualityComparer<string>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).GroupBy<string, string, string, string> ((Func<string, string>) (x => "test"), (Func<string, string>) (x => "test"), (Func<string, IEnumerable<string>, string>) ((x, y) => "test"), (IEqualityComparer<string>) EqualityComparer<string>.Default); });
			AssertException<ArgumentNullException> (delegate () { data.GroupBy<string, string, string, string> ((Func<string, string>) null, (Func<string, string>) (x => "test"), (Func<string, IEnumerable<string>, string>) ((x, y) => "test"), (IEqualityComparer<string>) EqualityComparer<string>.Default); });
			AssertException<ArgumentNullException> (delegate () { data.GroupBy<string, string, string, string> ((Func<string, string>) (x => "test"), (Func<string, string>) null, (Func<string, IEnumerable<string>, string>) ((x, y) => "test"), (IEqualityComparer<string>) EqualityComparer<string>.Default); });
			AssertException<ArgumentNullException> (delegate () { data.GroupBy<string, string, string, string> ((Func<string, string>) (x => "test"), (Func<string, string>) (x => "test"), (Func<string, IEnumerable<string>, string>) null, (IEqualityComparer<string>) EqualityComparer<string>.Default); });
		}

		[Test]
		public void GroupByTest ()
		{
			string [] data = { "2", "1", "5", "3", "4", "3" };

			Dictionary<string, IEnumerable<string>> expected = new Dictionary<string, IEnumerable<string>> ();
			expected.Add ("2", new List<string> () { "2" });
			expected.Add ("1", new List<string> () { "1" });
			expected.Add ("5", new List<string> () { "5" });
			expected.Add ("3", new List<string> () { "3", "3" });
			expected.Add ("4", new List<string> () { "4" });

			Dictionary<string, IEnumerable<string>> expected2 = new Dictionary<string, IEnumerable<string>> ();
			expected2.Add ("2", new List<string> () { "22" });
			expected2.Add ("1", new List<string> () { "11" });
			expected2.Add ("5", new List<string> () { "55" });
			expected2.Add ("3", new List<string> () { "33", "33" });
			expected2.Add ("4", new List<string> () { "44" });

			string [] expected3 = new string [] { "22", "11", "55", "333", "44" };

			// GroupBy<int,int> (Func<int, int>)
			AssertAreSame (expected, data.GroupBy (x => x));

			// GroupBy<int,int> (Func<int, int>, IEqualityComparer<int>)
			AssertAreSame (expected, data.GroupBy (x => x, EqualityComparer<string>.Default));

			// GroupBy<int,int,int> (Func<int, int>, Func<int, int>)
			AssertAreSame (expected2, data.GroupBy (x => x, x => x + x));

			// GroupBy<int,int,int> (Func<int, int>, Func<int, int>, IEqualityComparer<int>)
			AssertAreSame (expected2, data.GroupBy (x => x, x => x + x, EqualityComparer<string>.Default));

			// GroupBy<int,int,int> (Func<int, int>, Func<int, IEnumerable<int>, int>)
			AssertAreSame (expected3, data.GroupBy (x => x, (x, y) => { foreach (var s in y) x += s; return x; }));

			// GroupBy<int,int,int,int> (Func<int, int>, Func<int, int>, Func<int, IEnumerable<int>, int>)
			AssertAreSame (expected3, data.GroupBy (x => x, x => x, (x, y) => { foreach (var s in y) x += s; return x; }));

			// GroupBy<int,int,int> (Func<int, int>, Func<int, IEnumerable<int>, int>, IEqualityComparer<int>)
			AssertAreSame (expected3, data.GroupBy (x => x, (x, y) => { foreach (var s in y) x += s; return x; }, EqualityComparer<string>.Default));

			// GroupBy<int,int,int,int> (Func<int, int>, Func<int, int>, Func<int, IEnumerable<int>, int>, IEqualityComparer<int>)
			AssertAreSame (expected3, data.GroupBy (x => x, x => x, (x, y) => { foreach (var s in y) x += s; return x; }, EqualityComparer<string>.Default));
		}


		class Data {

			public int Number;
			public string String;

			public Data (int number, string str)
			{
				Number = number;
				String = str;
			}
		}

		[Test]
		public void GroupByLastNullGroup ()
		{
			var values = new List<Data> ();

			values.Add (new Data (0, "a"));
			values.Add (new Data (1, "a"));
			values.Add (new Data (2, "b"));
			values.Add (new Data (3, "b"));
			values.Add (new Data (4, null));

			var groups = values.GroupBy (d => d.String);

			Assert.AreEqual (3, groups.Count ());
		}

		[Test]
		public void ConcatArgumentNullTest ()
		{
			string [] data = { "2", "1", "5", "3", "4" };

			// Concat<TSource> (IEnumerable<TSource>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Concat (data); });
			AssertException<ArgumentNullException> (delegate () { data.Concat ((IEnumerable<string>) null); });
		}

		[Test]
		public void ConcatTest ()
		{
			int [] data1 = { 2, 1, 5, 3, 4 };
			int [] data2 = { 1, 2, 3, 4, 5 };
			int [] expected = { 2, 1, 5, 3, 4, 1, 2, 3, 4, 5 };


			// Concat<TSource> (IEnumerable<TSource>)
			AssertAreSame (expected, data1.Concat (data2));
		}

		[Test]
		public void DistinctArgumentNullTest ()
		{
			//string [] data = { "2", "1", "5", "3", "4" };


			// Distinct<TSource> ()
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Distinct (); });

			// Distinct<TSource> (IEqualityComparer<TSource>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Distinct (EqualityComparer<string>.Default); });
		}

		[Test]
		public void DistinctTest ()
		{
			int [] data = { 2, 1, 5, 3, 4, 2, 5, 3, 1, 8 };
			int [] expected = { 2, 1, 5, 3, 4, 8 };


			// Distinct<TSource> ()
			AssertAreSame (expected, data.Distinct ());

			// Distinct<iTSourcent> (IEqualityComparer<TSource>)
			AssertAreSame (expected, data.Distinct (EqualityComparer<int>.Default));
		}

		[Test]
		public void UnionArgumentNullTest ()
		{
			string [] data = { "2", "1", "5", "3", "4" };


			// Union<TSource> (IEnumerable<TSource>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Union (data); });
			AssertException<ArgumentNullException> (delegate () { data.Union ((IEnumerable<string>) null); });

			// Union<TSource> (IEnumerable<TSource>, IEqualityComparer<TSource>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Union (data, EqualityComparer<string>.Default); });
			AssertException<ArgumentNullException> (delegate () { data.Union ((IEnumerable<string>) null, EqualityComparer<string>.Default); });
		}

		[Test]
		public void UnionTest ()
		{
			int [] data1 = { 2, 1, 5, 7, 3, 4 };
			int [] data2 = { 1, 2, 3, 8, 4, 5 };
			int [] expected = { 2, 1, 5, 7, 3, 4, 8 };

			// Union<TSource> (IEnumerable<TSource>)
			AssertAreSame (expected, data1.Union (data2));

			// Union<TSource> (IEnumerable<TSource>, IEqualityComparer<TSource>)
			AssertAreSame (expected, data1.Union (data2, EqualityComparer<int>.Default));
		}

		[Test]
		public void IntersectArgumentNullTest ()
		{
			string [] data = { "2", "1", "5", "3", "4" };


			// Intersect<TSource> (IEnumerable<TSource>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Intersect (data); });
			AssertException<ArgumentNullException> (delegate () { data.Intersect ((IEnumerable<string>) null); });

			// Intersect<TSource> (IEnumerable<TSource>, IEqualityComparer<TSource>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Intersect (data, EqualityComparer<string>.Default); });
			AssertException<ArgumentNullException> (delegate () { data.Intersect ((IEnumerable<string>) null, EqualityComparer<string>.Default); });
		}

		[Test]
		public void IntersectTest ()
		{
			int [] data1 = { 2, 1, 5, 7, 3, 4 };
			int [] data2 = { 1, 2, 3, 8, 4, 5 };
			int [] expected = { 2, 1, 5, 3, 4 };


			// Intersect<TSource> (IEnumerable<TSource>)
			AssertAreSame (expected, data1.Intersect (data2));

			// Intersect<TSource> (IEnumerable<TSource>, IEqualityComparer<TSource>)
			AssertAreSame (expected, data1.Intersect (data2, EqualityComparer<int>.Default));
		}

		[Test]
		public void ExceptArgumentNullTest ()
		{
			string [] data = { "2", "1", "5", "3", "4" };


			// Except<TSource> (IEnumerable<TSource>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Except (data); });
			AssertException<ArgumentNullException> (delegate () { data.Except ((IEnumerable<string>) null); });

			// Except<TSource> (IEnumerable<string>, IEqualityComparer<TSource>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Except (data, EqualityComparer<string>.Default); });
			AssertException<ArgumentNullException> (delegate () { data.Except ((IEnumerable<string>) null, EqualityComparer<string>.Default); });
		}

		[Test]
		public void ExceptTest ()
		{
			int [] data1 = { 2, 1, 5, 7, 3, 4 };
			int [] data2 = { 1, 2, 3, 8, 4, 5 };
			int [] expected = { 7 };


			// Except<TSource> (IEnumerable<TSource>)
			AssertAreSame (expected, data1.Except (data2));

			// Except<TSource> (IEnumerable<TSource>, IEqualityComparer<TSource>)
			AssertAreSame (expected, data1.Except (data2, EqualityComparer<int>.Default));
		}

		[Test]
		public void ReverseArgumentNullTest ()
		{
			//string [] data = { "2", "1", "5", "3", "4" };


			// Reverse<TSource> ()
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).Reverse (); });
		}

		[Test]
		public void ReverseTest ()
		{
			int [] data = { 2, 1, 5, 7, 3, 4 };
			int [] expected = { 4, 3, 7, 5, 1, 2 };



			// Reverse<TSource> ()
			AssertAreSame (expected, data.Reverse ());
		}

		[Test]
		public void SequenceEqualArgumentNullTest ()
		{
			string [] data = { "2", "1", "5", "3", "4" };


			// SequenceEqual<TSource> (IEnumerable<TSource>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).SequenceEqual (data); });
			AssertException<ArgumentNullException> (delegate () { data.SequenceEqual ((IEnumerable<string>) null); });

			// SequenceEqual<TSource> (IEnumerable<TSource>, IEqualityComparer<TSource>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).SequenceEqual (data, EqualityComparer<string>.Default); });
			AssertException<ArgumentNullException> (delegate () { data.SequenceEqual ((IEnumerable<string>) null, EqualityComparer<string>.Default); });
		}

		[Test]
		public void SequenceEqualTest ()
		{
			int [] data1 = { 2, 1, 5, 7, 3, 4 };
			int [] data2 = { 2, 1, 5, 7, 3, 4 };
			int [] data3 = { 2, 1, 5, 7, 3, 4, 5 };
			int [] data4 = { 2, 1, 5, 7, 3 };
			int [] data5 = { 2, 1, 5, 8, 3, 4 };


			// SequenceEqual<TSource> (IEnumerable<TSource>)
			Assert.IsTrue (data1.SequenceEqual (data2));
			Assert.IsFalse (data1.SequenceEqual (data3));
			Assert.IsFalse (data1.SequenceEqual (data4));
			Assert.IsFalse (data1.SequenceEqual (data5));

			// SequenceEqual<TSource> (IEnumerable<TSource>, IEqualityComparer<TSource>)
			Assert.IsTrue (data1.SequenceEqual (data2, EqualityComparer<int>.Default));
			Assert.IsFalse (data1.SequenceEqual (data3, EqualityComparer<int>.Default));
			Assert.IsFalse (data1.SequenceEqual (data4, EqualityComparer<int>.Default));
			Assert.IsFalse (data1.SequenceEqual (data5, EqualityComparer<int>.Default));
		}

		[Test]
		public void AsEnumerableArgumentNullTest ()
		{
			//string [] data = { "2", "1", "5", "3", "4" };

		}

		[Test]
		public void AsEnumerableTest ()
		{
			int [] data = { 2, 1, 5, 7, 3, 4 };


			// AsEnumerable<TSource> ()
			Assert.AreSame (data, data.AsEnumerable ());
		}

		[Test]
		public void ToArrayArgumentNullTest ()
		{
			//string [] data = { "2", "1", "5", "3", "4" };


			// ToArray<TSource> ()
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).ToArray (); });
		}

		[Test]
		public void ToArrayTest ()
		{
			int [] data = { 2, 3, 4, 5 };
			int [] expected = { 2, 3, 4, 5 };


			// ToArray<TSource> ()
			AssertAreSame (expected, data.ToArray ());
		}

		[Test]
		public void ToListArgumentNullTest ()
		{
			//string [] data = { "2", "1", "5", "3", "4" };


			// ToList<string> ()
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).ToList (); });
		}

		[Test]
		public void ToListTest ()
		{
			int [] data = { 2, 4, 5, 1 };
			int [] expected = { 2, 4, 5, 1 };


			// ToList<int> ()
			AssertAreSame (expected, data.ToList ());
		}

		[Test]
		public void ToDictionaryArgumentNullTest ()
		{
			string [] data = { "2", "1", "5", "3", "4" };


			// ToDictionary<TSource,TKey> (Func<TSource, TKey>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).ToDictionary (x => "test"); });
			AssertException<ArgumentNullException> (delegate () { data.ToDictionary ((Func<string, string>) null); });

			// ToDictionary<TSource,TKey> (Func<TSource, TKey>, IEqualityComparer<TKey>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).ToDictionary (x => "test", EqualityComparer<string>.Default); });
			AssertException<ArgumentNullException> (delegate () { data.ToDictionary ((Func<string, string>) null, EqualityComparer<string>.Default); });

			// ToDictionary<TSource,TKey,TElement> (Func<TSource, TKey>, Func<TSource, TElement>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).ToDictionary (x => "test", x => "test"); });
			AssertException<ArgumentNullException> (delegate () { data.ToDictionary ((Func<string, string>) null, x => "test"); });
			AssertException<ArgumentNullException> (delegate () { data.ToDictionary (x => "test", (Func<string, string>) null); });

			// ToDictionary<TSource,TKey,TElement> (Func<TSource, TKey>, Func<TSource, TElement>, IEqualityComparer<TKey>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).ToDictionary (x => "test", x => "test", EqualityComparer<string>.Default); });
			AssertException<ArgumentNullException> (delegate () { data.ToDictionary ((Func<string, string>) null, x => "test", EqualityComparer<string>.Default); });
			AssertException<ArgumentNullException> (delegate () { data.ToDictionary (x => "test", (Func<string, string>) null, EqualityComparer<string>.Default); });
		}

		[Test]
		public void ToDictionaryTest ()
		{
			string [] data = { "2", "1", "5", "3", "4" };
			Dictionary<string, string> expected = new Dictionary<string, string> ();
			expected.Add ("k2", "2");
			expected.Add ("k1", "1");
			expected.Add ("k5", "5");
			expected.Add ("k3", "3");
			expected.Add ("k4", "4");


			// ToDictionary<TSource,TKey> (Func<TSource, TKey>)
			AssertAreSame (expected, ((IEnumerable<string>) data).ToDictionary (x => "k" + x));
			AssertException<ArgumentException> (delegate () { data.ToDictionary (x => "key"); });

			// ToDictionary<TSource,TKey> (Func<TSource, TKey>, IEqualityComparer<TKey>)
			AssertAreSame (expected, ((IEnumerable<string>) data).ToDictionary (x => "k" + x, EqualityComparer<string>.Default));
			AssertException<ArgumentException> (delegate () { data.ToDictionary (x => "key", EqualityComparer<string>.Default); });

			// ToDictionary<TSource,TKey,TElement> (Func<TSource, TKey>, Func<TSource, TElement>)
			AssertAreSame (expected, ((IEnumerable<string>) data).ToDictionary (x => "k" + x, x => x));
			AssertException<ArgumentException> (delegate () { data.ToDictionary (x => "key", x => x); });

			// ToDictionary<TSource,TKey,TElement> (Func<TSource, TKey>, Func<TSource, TElement>, IEqualityComparer<TKey>)
			AssertAreSame (expected, ((IEnumerable<string>) data).ToDictionary (x => "k" + x, x => x, EqualityComparer<string>.Default));
			AssertException<ArgumentException> (delegate () { data.ToDictionary (x => "key", x => x, EqualityComparer<string>.Default); });
		}

		[Test]
		public void ToLookupArgumentNullTest ()
		{
			string [] data = { "2", "1", "5", "3", "4" };


			// ToLookup<string,string> (Func<string, string>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).ToLookup<string, string> ((Func<string, string>) (x => "test")); });
			AssertException<ArgumentNullException> (delegate () { data.ToLookup<string, string> ((Func<string, string>) null); });

			// ToLookup<string,string> (Func<string, string>, IEqualityComparer<string>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).ToLookup<string, string> ((Func<string, string>) (x => "test"), (IEqualityComparer<string>) EqualityComparer<string>.Default); });
			AssertException<ArgumentNullException> (delegate () { data.ToLookup<string, string> ((Func<string, string>) null, (IEqualityComparer<string>) EqualityComparer<string>.Default); });

			// ToLookup<string,string,string> (Func<string, string>, Func<string, string>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).ToLookup<string, string, string> ((Func<string, string>) (x => "test"), (Func<string, string>) (x => "test")); });
			AssertException<ArgumentNullException> (delegate () { data.ToLookup<string, string, string> ((Func<string, string>) null, (Func<string, string>) (x => "test")); });
			AssertException<ArgumentNullException> (delegate () { data.ToLookup<string, string, string> ((Func<string, string>) (x => "test"), (Func<string, string>) null); });

			// ToLookup<string,string,string> (Func<string, string>, Func<string, string>, IEqualityComparer<string>)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).ToLookup<string, string, string> ((Func<string, string>) (x => "test"), (Func<string, string>) (x => "test"), (IEqualityComparer<string>) EqualityComparer<string>.Default); });
			AssertException<ArgumentNullException> (delegate () { data.ToLookup<string, string, string> ((Func<string, string>) null, (Func<string, string>) (x => "test"), (IEqualityComparer<string>) EqualityComparer<string>.Default); });
			AssertException<ArgumentNullException> (delegate () { data.ToLookup<string, string, string> ((Func<string, string>) (x => "test"), (Func<string, string>) null, (IEqualityComparer<string>) EqualityComparer<string>.Default); });
		}

		[Test]
		public void ToLookupTest ()
		{
			string [] data = { "23", "12", "55", "42", "41" };
			Dictionary<string, IEnumerable<string>> expected = new Dictionary<string, IEnumerable<string>> ();
			expected.Add ("2", new List<string> () { "23" });
			expected.Add ("1", new List<string> () { "12" });
			expected.Add ("5", new List<string> () { "55" });
			expected.Add ("4", new List<string> () { "42", "41" });

			Assert.AreEqual (expected.Count, ((IEnumerable<string>)data).ToLookup ((x => x [0].ToString ())).Count);
			
			// ToLookup<string,string> (Func<string, string>)
			AssertAreSame (expected, ((IEnumerable<string>) data).ToLookup ((x => x [0].ToString ())));

			// ToLookup<string,string> (Func<string, string>, IEqualityComparer<string>)
			AssertAreSame (expected, ((IEnumerable<string>) data).ToLookup (x => x [0].ToString (), EqualityComparer<string>.Default));

			// ToLookup<string,string,string> (Func<string, string>, Func<string, string>)
			AssertAreSame (expected, ((IEnumerable<string>) data).ToLookup (x => x [0].ToString (), x => x));

			// ToLookup<string,string,string> (Func<string, string>, Func<string, string>, IEqualityComparer<string>)
			AssertAreSame (expected, ((IEnumerable<string>) data).ToLookup (x => x [0].ToString (), x => x, EqualityComparer<string>.Default));
		}
		
		[Test]
		public void ToLookupNullKeyTest ()
		{
			string[] strs = new string[] { "one", null, "two", null, "three" };
			
			int i = 0;
			var l = strs.ToLookup (s => (s == null) ? null : "numbers", s => (s == null) ? (++i).ToString() : s);
			
			Assert.AreEqual (2, l.Count);
			Assert.AreEqual (2, l [null].Count());
			Assert.IsTrue (l [null].Contains ("1"));
			Assert.IsTrue (l [null].Contains ("2"));
			
			Assert.AreEqual (3, l ["numbers"].Count());
			Assert.IsTrue (l ["numbers"].Contains ("one"));
			Assert.IsTrue (l ["numbers"].Contains ("two"));
			Assert.IsTrue (l ["numbers"].Contains ("three"));
		}

		[Test]
		public void DefaultIfEmptyArgumentNullTest ()
		{
			//string [] data = { "2", "1", "5", "3", "4" };


			// DefaultIfEmpty<string> ()
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).DefaultIfEmpty<string> (); });

			// DefaultIfEmpty<string> (string)
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable<string>) null).DefaultIfEmpty<string> ((string) "default"); });
		}

		[Test]
		public void DefaultIfEmptyTest ()
		{
			string [] data = { "2", "1", "5", "3", "4" };
			string [] empty = { };
			string [] default1 = { null };
			string [] default2 = { "default" };


			// DefaultIfEmpty<string> ()
			AssertAreSame (data, data.DefaultIfEmpty ());
			AssertAreSame (default1, empty.DefaultIfEmpty ());

			// DefaultIfEmpty<string> (string)
			AssertAreSame (data, data.DefaultIfEmpty ("default"));
			AssertAreSame (default2, empty.DefaultIfEmpty ("default"));
		}

		[Test]
		public void OfTypeArgumentNullTest ()
		{
			//string [] data = { "2", "1", "5", "3", "4" };


			// OfType<string> ()
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable) null).OfType<string> (); });
		}

		[Test]
		public void OfTypeTest ()
		{
			object [] data = { "2", 2, "1", "5", "3", "4" };
			string [] expected = { "2", "1", "5", "3", "4" };


			// OfType<string> ()
			AssertAreSame (expected, data.OfType<string> ());
		}

		[Test]
		public void CastArgumentNullTest ()
		{
			//string [] data = { "2", "1", "5", "3", "4" };


			// Cast<string> ()
			AssertException<ArgumentNullException> (delegate () { ((IEnumerable) null).Cast<string> (); });
		}

		[Test]
		public void CastTest ()
		{
			object [] data = { 1, 2, 3 };
			int [] expected = { 1, 2, 3 };


			// Cast<string> ()
			AssertAreSame (expected, data.Cast<int> ());
			AssertException<InvalidCastException> (delegate () { data.Cast<IEnumerable> ().GetEnumerator ().MoveNext (); });
			data.Cast<IEnumerable> ();
		}

		[Test]
		public void RangeArgumentNullTest ()
		{
			//string [] data = { "2", "1", "5", "3", "4" };

		}

		[Test]
		public void RangeTest ()
		{
			int [] expected = { 2, 3, 4, 5 };

			// Range<> (int)
			AssertAreSame (expected, Enumerable.Range (2, 4));
			AssertException<ArgumentOutOfRangeException> (delegate () { Enumerable.Range (2, -3); });
			AssertException<ArgumentOutOfRangeException> (delegate () { Enumerable.Range (int.MaxValue - 5, 7); });
			Enumerable.Range (int.MaxValue - 5, 6);
		}
	}
}
