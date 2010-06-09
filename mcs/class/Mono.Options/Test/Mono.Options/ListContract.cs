//
// IListContract.cs
//
// Author:
//   Jonathan Pryor  <jpryor@novell.com>
//
// Copyright (c) 2010 Novell, Inc. (http://www.novell.com)
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

using NUnit.Framework;

using Cadenza.Collections;
using Cadenza.Tests;

namespace Cadenza.Collections.Tests {

	public abstract class ListContract<T> : CollectionContract<T> {

		private IList<T> CreateList (IEnumerable<T> values)
		{
			return (IList<T>) CreateCollection (values);
		}

		[Test]
		public void IndexOf ()
		{
			var a = CreateValueA ();
			var b = CreateValueB ();

			var list = CreateList (new T[0]);

			Assert.AreEqual (-1,  list.IndexOf (a));

			try {
				list.Add (a);
				Assert.AreEqual (0,   list.IndexOf (a));

				list.Add (b);
				Assert.AreEqual (1,   list.IndexOf (b));

				list.Remove (a);
				Assert.AreEqual (-1,  list.IndexOf (a));
				Assert.AreEqual (0,   list.IndexOf (b));

				list.Remove (b);
				Assert.AreEqual (-1,  list.IndexOf (b));
			}
			catch (NotSupportedException) {
				Assert.IsTrue (list.IsReadOnly);
			}
		}

		[Test]
		public void Insert ()
		{
			var a = CreateValueA ();
			var b = CreateValueB ();

			var list = CreateList (new T[0]);

			try {
				AssertThrows<ArgumentOutOfRangeException>(() => list.Insert (-1, a));
				AssertThrows<ArgumentOutOfRangeException>(() => list.Insert (1, a));

				list.Insert (0, a);
				Assert.AreEqual (0, list.IndexOf (a));

				list.Insert (0, b);
				Assert.AreEqual (2, list.Count);
				Assert.AreEqual (0, list.IndexOf (b));
				Assert.AreEqual (1, list.IndexOf (a));
			}
			catch (NotSupportedException) {
				Assert.IsTrue (list.IsReadOnly);
			}
		}

		[Test]
		public void RemoveAt ()
		{
			var a = CreateValueA ();
			var b = CreateValueB ();

			var list = CreateList (new T [0]);

			try {
				AssertThrows<ArgumentOutOfRangeException>(() => list.RemoveAt (-1));
				AssertThrows<ArgumentOutOfRangeException>(() => list.RemoveAt (0));

				list.Add (a);
				Assert.AreEqual (1, list.Count);

				list.RemoveAt (0);
				Assert.AreEqual (0, list.Count);

				list.Add (a);
				list.Add (b);
				list.RemoveAt (0);
				Assert.AreEqual (1, list.Count);
				Assert.AreEqual (0, list.IndexOf (b));
			}
			catch (NotSupportedException) {
				Assert.IsTrue (list.IsReadOnly);
			}
		}

		[Test]
		public void Item ()
		{
			var a = CreateValueA ();
			var b = CreateValueB ();

			var list = CreateList (new []{a});

			Assert.AreEqual (a, list [0]);
			AssertThrows<ArgumentOutOfRangeException>(() => Ignore (list [-1]));

			try {
				AssertThrows<ArgumentOutOfRangeException>(() => list [-1] = a);
				AssertThrows<ArgumentOutOfRangeException>(() => list [1] = a);

				list [0] = b;
				Assert.AreEqual (1, list.Count);

				Assert.AreEqual (-1,  list.IndexOf (a));
				Assert.AreEqual (0,   list.IndexOf (b));
			}
			catch (NotSupportedException) {
				Assert.IsTrue (list.IsReadOnly);
			}
		}
	}
}

