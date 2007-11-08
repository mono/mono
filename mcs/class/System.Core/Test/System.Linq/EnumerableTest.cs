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

		[Test]
		public void TestSequenceEqual ()
		{
			int [] first = {0, 1, 2, 3, 4, 5};
			int [] second = {0, 1, 2};
			int [] third = {0, 1, 2, 3, 4, 5};

			Assert.IsFalse (first.SequenceEqual (second));
			Assert.IsTrue (first.SequenceEqual (third));
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
