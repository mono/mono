//
// SortedSetTest.cs
//
// Author:
//	Jb Evain <jbevain@novell.com>
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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

#if NET_4_0

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using NUnit.Framework;

namespace MonoTests.System.Collections.Generic
{
	[TestFixture]
	public class SortedSetTest
	{
		[Test]
		public void CtorNullComparer ()
		{
			var set = new SortedSet<int> ((IComparer<int>) null);
			Assert.AreEqual (Comparer<int>.Default, set.Comparer);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CtorNullCollection ()
		{
			new SortedSet<int> (null as IEnumerable<int>);
		}

		[Test]
		public void CtorDefault ()
		{
			var set = new SortedSet<int> ();
			Assert.IsNotNull (set.Comparer);
		}

		[Test]
		public void Add ()
		{
			var set = new SortedSet<int> ();
			Assert.AreEqual (0, set.Count);
			Assert.IsTrue (set.Add (2));
			Assert.IsTrue (set.Add (4));
			Assert.IsTrue (set.Add (3));
			Assert.AreEqual (3, set.Count);
			Assert.IsFalse (set.Add (2));
		}

		[Test]
		public void Remove ()
		{
			var set = new SortedSet<int> ();
			Assert.IsTrue (set.Add (2));
			Assert.IsTrue (set.Add (4));
			Assert.AreEqual (2, set.Count);
			Assert.IsTrue (set.Remove (4));
			Assert.IsTrue (set.Remove (2));
			Assert.AreEqual (0, set.Count);
			Assert.IsFalse (set.Remove (4));
			Assert.IsFalse (set.Remove (2));
		}

		[Test]
		public void Clear ()
		{
			var set = new SortedSet<int> { 2, 3, 4, 5 };
			Assert.AreEqual (4, set.Count);
			set.Clear ();
			Assert.AreEqual (0, set.Count);
		}

		[Test]
		public void Contains ()
		{
			var set = new SortedSet<int> { 2, 3, 4, 5 };
			Assert.IsTrue (set.Contains (4));
			Assert.IsFalse (set.Contains (7));
		}

		[Test]
		public void GetEnumerator ()
		{
			var set = new SortedSet<int> { 5, 3, 1, 2, 6, 4  };
			Assert.IsTrue (set.SequenceEqual (new [] { 1, 2, 3, 4, 5, 6 }));
		}

		[Test]
		public void Reverse ()
		{
			var set = new SortedSet<int> { 5, 3, 1, 2, 6, 4  };
			var reversed = set.Reverse ();
			Assert.IsTrue (reversed.SequenceEqual (new [] { 6, 5, 4, 3, 2, 1 }));
		}

		[Test]
		public void RemoveWhere ()
		{
			var set = new SortedSet<int> { 1, 2, 3, 4, 5, 6 };
			Assert.AreEqual (3, set.RemoveWhere (i => i % 2 == 0));
			Assert.AreEqual (3, set.Count);
			Assert.IsTrue (set.SequenceEqual (new [] { 1, 3, 5 }));

		}

		[Test]
		public void Max ()
		{
			var set = new SortedSet<int> { 1, 3, 12, 9 };
			Assert.AreEqual (12, set.Max);
		}

		[Test]
		public void Min ()
		{
			var set = new SortedSet<int> { 2, 3, 1, 9 };
			Assert.AreEqual (1, set.Min);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetViewBetweenLowerBiggerThanUpper ()
		{
			var set = new SortedSet<int> { 1, 2, 3, 4, 5, 6 };
			set.GetViewBetween (4, 2);
		}

		[Test]
		public void GetView ()
		{
			var set = new SortedSet<int> { 1, 3, 5, 7, 9 };
			var view = set.GetViewBetween (3, 7);

			Assert.IsTrue (view.SequenceEqual (new [] { 3, 5, 7 }));
		}

		[Test]
		public void ViewAdd ()
		{
			var set = new SortedSet<int> { 1, 3, 5, 7 };
			var view = set.GetViewBetween (3, 5);

			Assert.IsTrue (view.Add (4));
			Assert.IsTrue (view.Contains (4));
			Assert.IsTrue (set.Contains (4));

			Assert.IsFalse (view.Add (5));
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ViewAddOutOfRange ()
		{
			var set = new SortedSet<int> { 1, 3, 5, 7 };
			var view = set.GetViewBetween (3, 5);

			view.Add (7);
		}

		[Test]
		public void ViewContains ()
		{
			var set = new SortedSet<int> { 1, 3, 5, 7, 9 };
			var view = set.GetViewBetween (3, 7);

			Assert.IsFalse (view.Contains (4));
			Assert.IsTrue (view.Contains (3));
			Assert.IsTrue (view.Contains (5));
		}

		[Test]
		public void ViewRemove ()
		{
			var set = new SortedSet<int> { 1, 3, 5, 7, 9 };
			var view = set.GetViewBetween (3, 7);

			Assert.IsTrue (view.Remove (3));
			Assert.IsFalse (view.Contains (3));
			Assert.IsFalse (set.Contains (3));
			Assert.IsFalse (view.Remove (9));
			Assert.IsTrue (set.Contains (9));
		}

		[Test]
		public void ViewClear ()
		{
			var set = new SortedSet<int> { 1, 3, 5, 7, 9 };
			var view = set.GetViewBetween (3, 7);

			view.Clear ();

			Assert.AreEqual (0, view.Count);
			Assert.IsTrue (set.SequenceEqual (new [] { 1, 9 }));
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ViewGetViewLowerOutOfRange ()
		{
			var set = new SortedSet<int> { 1, 3, 5, 7, 9 };
			var view = set.GetViewBetween (3, 7);
			view.GetViewBetween (2, 5);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ViewGetViewUpperOutOfRange ()
		{
			var set = new SortedSet<int> { 1, 3, 5, 7, 9 };
			var view = set.GetViewBetween (3, 7);
			view.GetViewBetween (5, 9);
		}

		[Test]
		public void ViewGetView ()
		{
			var set = new SortedSet<int> { 1, 3, 5, 7, 9 };
			var view = set.GetViewBetween (3, 7);
			view = view.GetViewBetween (4, 6);

			Assert.IsTrue (view.SequenceEqual (new [] { 5 }));
		}

		void EmptySubView (SortedSet<int> set)
		{
			var view = set.GetViewBetween (-20, -15);
			Assert.AreEqual (0, view.Count);
			Assert.AreEqual (0, view.Min);
			Assert.AreEqual (0, view.Max);

			view = set.GetViewBetween (15, 20);
			Assert.AreEqual (0, view.Count);
			Assert.AreEqual (0, view.Min);
			Assert.AreEqual (0, view.Max);
		}

		[Test]
		public void EmptySubView ()
		{
			EmptySubView (new SortedSet<int> ());
			EmptySubView (new SortedSet<int> { 1, 3, 5, 7, 9 });
			EmptySubView (new SortedSet<int> { -40, 40 });
			EmptySubView (new SortedSet<int> { -40, -10, 10, 40 });
		}

		[Test]
		public void ViewMin ()
		{
			var set = new SortedSet<int> { 1, 3, 5, 7, 9 };

			var view = set.GetViewBetween (4, 8);
			Assert.AreEqual (5, view.Min);

			view = set.GetViewBetween (-2, 4);
			Assert.AreEqual (1, view.Min);

			view = set.GetViewBetween (1, 9);
			Assert.AreEqual (1, view.Min);
		}

		[Test]
		public void ViewMax ()
		{
			var set = new SortedSet<int> { 1, 3, 5, 7, 9 };

			var view = set.GetViewBetween (4, 8);
			Assert.AreEqual (7, view.Max);

			view = set.GetViewBetween (4, 55);
			Assert.AreEqual (9, view.Max);

			view = set.GetViewBetween (1, 9);
			Assert.AreEqual (9, view.Max);
		}

		[Test]
		public void ViewCount ()
		{
			var set = new SortedSet<int> { 1, 3, 4, 5, 6, 7, 8, 9 };
			var view = set.GetViewBetween (4, 8);

			Assert.AreEqual (5, view.Count);
			set.Remove (5);
			Assert.AreEqual (4, view.Count);
			set.Add (10);
			Assert.AreEqual (4, view.Count);
			set.Add (6);
			Assert.AreEqual (4, view.Count);
			set.Add (5);
			Assert.AreEqual (5, view.Count);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void IntersectWith_Null ()
		{
			var set = new SortedSet<int> ();
			set.IntersectWith (null);
		}

		[Test]
		public void IntersectWith ()
		{
			var set = new SortedSet<int> { 1, 3, 5, 7, 9 };
			set.IntersectWith (new [] { 5, 7, 3, 7, 11, 7, 5, 2 });
			Assert.IsTrue (set.SequenceEqual (new [] { 3, 5, 7 }));
		}

		[Test]
		public void ViewIntersectWith ()
		{
			var set = new SortedSet<int> { 1, 3, 5, 7, 9 };
			var view = set.GetViewBetween (4, 8);
			view.IntersectWith (new [] { 1, 5, 9 });
			Assert.IsTrue (view.SequenceEqual (new [] { 5 }));
			Assert.IsTrue (set.SequenceEqual (new [] { 1, 3, 5, 9 }));
			view.IntersectWith (new [] { 1, 2 });
			Assert.IsTrue (view.SequenceEqual (new int [] {}));
			Assert.IsTrue (set.SequenceEqual (new [] { 1, 3, 9 }));
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void UnionWith_Null ()
		{
			var set = new SortedSet<int> ();
			set.UnionWith (null);
		}

		[Test]
		public void UnionWith ()
		{
			var set = new SortedSet<int> { 1, 3, 5, 7, 9 };
			set.UnionWith (new [] { 5, 7, 3, 7, 11, 7, 5, 2 });
			Assert.IsTrue (set.SequenceEqual (new [] { 1, 2, 3, 5, 7, 9, 11 }));
		}

		[Test]
		public void ViewUnionWith ()
		{
			var set = new SortedSet<int> { 1, 3, 5, 7, 9 };
			var view = set.GetViewBetween (4, 8);
			view.UnionWith (new [] { 4, 5, 6, 6, 4 });
			Assert.IsTrue (view.SequenceEqual (new [] { 4, 5, 6, 7 }));
			Assert.IsTrue (set.SequenceEqual (new [] { 1, 3, 4, 5, 6, 7, 9 }));
		}

		[Test, ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ViewUnionWith_oor ()
		{
			var set = new SortedSet<int> { 1, 3, 5, 7, 9 };
			var view = set.GetViewBetween (4, 8);
			view.UnionWith (new [] {1});
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void ExceptWith_Null ()
		{
			var set = new SortedSet<int> ();
			set.ExceptWith (null);
		}

		[Test]
		public void ExceptWith ()
		{
			var set = new SortedSet<int> { 1, 3, 5, 7, 9 };
			set.ExceptWith (new [] { 5, 7, 3, 7, 11, 7, 5, 2 });
			Assert.IsTrue (set.SequenceEqual (new [] { 1, 9 }));
		}

		[Test]
		public void ExceptWithItself ()
		{
			var set = new SortedSet<int> (new [] { 1, 5 });
			set.ExceptWith (set);
			Assert.AreEqual (0, set.Count);
		}

		[Test]
		public void ViewExceptWith ()
		{
			var set = new SortedSet<int> { 1, 3, 5, 7, 9 };
			var view = set.GetViewBetween (4, 8);
			view.ExceptWith (new [] { 4, 5, 6, 6, 4 });
			Assert.IsTrue (view.SequenceEqual (new [] { 7 }));
			Assert.IsTrue (set.SequenceEqual (new [] { 1, 3, 7, 9 }));
			view.ExceptWith (new [] { 1, 2 });
			Assert.IsTrue (view.SequenceEqual (new [] { 7 }));
			Assert.IsTrue (set.SequenceEqual (new [] { 1, 3, 7, 9 }));
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void SymmetricExceptWith_Null ()
		{
			var set = new SortedSet<int> ();
			set.SymmetricExceptWith (null);
		}

		[Test]
		public void SymmetricExceptWith ()
		{
			var set = new SortedSet<int> { 1, 3, 5, 7, 9 };
			set.SymmetricExceptWith (new [] { 5, 7, 3, 7, 11, 7, 5, 2 });
			Assert.IsTrue (set.SequenceEqual (new [] { 1, 2, 9, 11 }));
		}

		[Test]
		public void SymetricExceptWithItself ()
		{
			var set = new SortedSet<int> (new [] { 1, 5 });
			set.SymmetricExceptWith (set);
			Assert.AreEqual (0, set.Count);
		}

		[Test]
		public void ViewSymmetricExceptWith ()
		{
			var set = new SortedSet<int> { 1, 3, 5, 7, 9 };
			var view = set.GetViewBetween (4, 8);
			view.SymmetricExceptWith (new [] { 4, 5, 6, 6, 4 });
			Assert.IsTrue (view.SequenceEqual (new [] { 4, 6, 7 }));
			Assert.IsTrue (set.SequenceEqual (new [] { 1, 3, 4, 6, 7, 9 }));
		}

		[Test, ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ViewSymmetricExceptWith_oor ()
		{
			var set = new SortedSet<int> { 1, 3, 5, 7, 9 };
			var view = set.GetViewBetween (4, 8);
			view.SymmetricExceptWith (new [] {2});
		}

		void do_test_e (SortedSet<int> s1, IEnumerable<int> s2, bool o, bool se = false, bool psb = false, bool psu = false)
		{
			bool sb = false, su = false;
			if (se)
				sb = su = true;
			if (psb)
				sb = true;
			if (psu)
				su = true;

			Assert.IsTrue (!su || !psb);
			Assert.IsTrue (!sb || !psu);

			// actual tests
			Assert.AreEqual (o, s1.Overlaps (s2));
			Assert.AreEqual (se, s1.SetEquals (s2));
			Assert.AreEqual (sb, s1.IsSubsetOf (s2));
			Assert.AreEqual (su, s1.IsSupersetOf (s2));
			Assert.AreEqual (psb, s1.IsProperSubsetOf (s2));
			Assert.AreEqual (psu, s1.IsProperSupersetOf (s2));
		}

		void do_test (SortedSet<int> s1, SortedSet<int> s2, bool o = false, bool se = false, bool psb = false, bool psu = false)
		{
			if (s1.Count != 0 && s2.Count != 0 && (se || psb || psu))
				o = true;
			do_test_e (s1, s2, o, se, psb, psu);
			do_test_e (s2, s1, o, se, psu, psb);
		}

		[Test]
		public void TestSetCompares ()
		{
			var empty = new SortedSet<int> ();
			var zero = new SortedSet<int> { 0 };
			var one = new SortedSet<int> { 1 };
			var two = new SortedSet<int> { 2 };
			var bit = new SortedSet<int> { 0, 1 };
			var trit = new SortedSet<int> { 0, 1, 2 };
			var odds = new SortedSet<int> { 1, 3, 5, 7, 9 };
			var evens = new SortedSet<int> { 2, 4, 6, 8 };
			var digits = new SortedSet<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
			var squares = new SortedSet<int> { 0, 1, 4, 9 };

			var non_prime_odd_digit = odds.GetViewBetween (8, 42);
			var non_trit = digits.GetViewBetween (3, 42);

			do_test (empty, empty, se: true);
			do_test (empty, zero, psb: true);
			do_test (empty, digits, psb: true);
			do_test (zero, zero, se: true);
			do_test (zero, one);
			do_test (zero, bit, psb: true);
			do_test (zero, trit, psb: true);
			do_test (one, bit, psb: true);
			do_test (one, trit, psb: true);
			do_test (two, bit);
			do_test (two, trit, psb: true);
			do_test (odds, squares, o: true);
			do_test (evens, squares, o: true);
			do_test (odds, digits, psb: true);
			do_test (evens, digits, psb: true);
			do_test (squares, digits, psb: true);
			do_test (digits, digits, se: true);
			do_test_e (digits, squares.Concat (evens.Concat (odds)), o: true, se: true);
			do_test (non_prime_odd_digit, digits, psb: true);
			do_test_e (non_prime_odd_digit, new [] { 9 }, o: true, se: true);
			do_test (non_trit, digits, psb: true);
			do_test (trit, non_trit);
			do_test_e (digits, trit.Concat (non_trit), o: true, se: true);
			do_test_e (non_trit, new [] { 3, 4, 5, 6, 7, 8, 9 }, o: true, se: true);
			do_test (digits.GetViewBetween (0, 2), trit, se: true);
		}
	}
}

#endif
