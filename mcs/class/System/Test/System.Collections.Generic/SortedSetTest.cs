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

		[Test]
		public void ViewMin ()
		{
			var set = new SortedSet<int> { 1, 3, 5, 7, 9 };
			var view = set.GetViewBetween (4, 8);

			Assert.AreEqual (5, view.Min);
		}

		[Test]
		public void ViewMax ()
		{
			var set = new SortedSet<int> { 1, 3, 5, 7, 9 };
			var view = set.GetViewBetween (4, 8);

			Assert.AreEqual (7, view.Max);
		}
	}
}

#endif
