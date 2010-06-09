//
// IEnumerableContract.cs
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

	// NOTE:  when adding new tests to this type, add them to the
	//        RunAllTests() method as well.
	//        RunAllTests() is used by IDictionaryContract<T>.Keys()/.Values()
	//        to test the behavior of the .Keys/.Values read-only collections.
	//
	// NOTE:  No test may use [ExpectedException]; use Assert.Throws<T> instead.
	public abstract class CollectionContract<T> : BaseRocksFixture {

		protected abstract ICollection<T> CreateCollection (IEnumerable<T> values);
		protected abstract T CreateValueA ();
		protected abstract T CreateValueB ();
		protected abstract T CreateValueC ();

		public void RunAllTests ()
		{
			Add ();
			Clear ();
			Contains ();
			CopyTo_Exceptions ();
			CopyTo_SequenceComparison ();
			CopyTo ();
			Ctor_CopySequence ();
			Ctor_Initial_Count_Is_Zero ();
			Remove ();
		}

		[Test]
		public void Ctor_Initial_Count_Is_Zero ()
		{
			var c = CreateCollection (new T [0]);
			Assert.AreEqual (0, c.Count);
		}

		[Test]
		public void Ctor_CopySequence ()
		{
			var c = CreateCollection (new []{CreateValueA (), CreateValueB (), CreateValueC ()});
			Assert.AreEqual (3, c.Count);
		}

		[Test]
		public void Add ()
		{
			var c = CreateCollection (new T [0]);
			var n = c.Count;
			try {
				c.Add (CreateValueA ());
				Assert.AreEqual (n+1, c.Count);
			}
			catch (NotSupportedException) {
				Assert.IsTrue (c.IsReadOnly);
			}
		}

		[Test]
		public void Clear ()
		{
			var c = CreateCollection (new []{CreateValueA ()});
			try {
				c.Clear ();
				Assert.AreEqual (0, c.Count);
			}
			catch (NotSupportedException) {
				Assert.IsTrue (c.IsReadOnly);
			}
		}

		[Test]
		public void Contains ()
		{
			var a = CreateValueA ();
			var b = CreateValueB ();

			var c = CreateCollection (new []{a, b});
			Assert.IsTrue (c.Contains (a));
			Assert.IsTrue (c.Contains (b));
			Assert.IsFalse (c.Contains (CreateValueC ()));
		}

		[Test]
		public void CopyTo_Exceptions ()
		{
			var c = CreateCollection (new []{CreateValueA (), CreateValueB (), CreateValueC ()});
			AssertThrows<ArgumentNullException>(() => c.CopyTo (null, 0));
			AssertThrows<ArgumentOutOfRangeException>(() => c.CopyTo (new T [3], -1));
			var d = new T[5];
			// not enough space from d[3..d.Length-1] to hold c.Count elements.
			AssertThrows<ArgumentException>(() => c.CopyTo (d, 3));
			AssertThrows<ArgumentException>(() => c.CopyTo (new T [0], 0));
		}

		// can fail for IDictionary<TKey,TValue> implementations; override if appropriate.
		[Test]
		public virtual void CopyTo_SequenceComparison ()
		{
			var a = CreateValueA ();
			var b = CreateValueB ();
			var c = CreateValueC ();

			var coll = CreateCollection (new []{a, b, c});
			var d = new T [5];
			coll.CopyTo (d, 1);
			Assert.IsTrue (new []{
				default (T), a, b, c, default (T),
			}.SequenceEqual (d));
		}

		[Test]
		public void CopyTo ()
		{
			var a = CreateValueA ();
			var b = CreateValueB ();
			var c = CreateValueC ();

			var coll = CreateCollection (new []{a, b, c});
			var d = new T [5];
			coll.CopyTo (d, 1);
			Assert.IsTrue (Array.IndexOf (d, a) >= 0);
			Assert.IsTrue (Array.IndexOf (d, b) >= 0);
			Assert.IsTrue (Array.IndexOf (d, c) >= 0);
		}

		[Test]
		public void Remove ()
		{
			var a = CreateValueA ();
			var b = CreateValueB ();
			var c = CreateValueC ();

			var coll = CreateCollection (new []{a, b});
			int n = coll.Count;
			try {
				Assert.IsFalse (coll.Remove (c));
				Assert.AreEqual (n, coll.Count);
				Assert.IsTrue (coll.Remove (a));
				Assert.AreEqual (n-1, coll.Count);
			}
			catch (NotSupportedException) {
				Assert.IsTrue (coll.IsReadOnly);
			}
		}
	}
}

