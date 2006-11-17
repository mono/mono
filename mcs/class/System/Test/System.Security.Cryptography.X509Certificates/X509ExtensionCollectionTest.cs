//
// X509ExtensionCollectionTest.cs - NUnit tests for X509ExtensionCollection
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using NUnit.Framework;

using System;
using System.Collections;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace MonoTests.System.Security.Cryptography.X509Certificates {

	[TestFixture]
	public class X509ExtensionCollectionTest {

		private X509ExtensionCollection empty;
		private X509Extension extn_empty;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			empty = new X509ExtensionCollection ();
			extn_empty = new X509Extension ("1.2", new byte[] { 0x05, 0x00 }, false);
		}

		[Test]
		public void Defaults ()
		{
			Assert.AreEqual (0, empty.Count, "Count");
			Assert.IsFalse (empty.IsSynchronized, "IsSynchronized");
			Assert.IsTrue (Object.ReferenceEquals (empty, empty.SyncRoot), "SyncRoot");
			Assert.AreEqual (typeof (X509ExtensionEnumerator), empty.GetEnumerator ().GetType (), "GetEnumerator");
			// IEnumerable
			IEnumerable e = (empty as IEnumerable);
			Assert.AreEqual (typeof (X509ExtensionEnumerator), e.GetEnumerator ().GetType (), "IEnumerable.GetEnumerator");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void Indexer_Int_Negative ()
		{
			Assert.IsNotNull (empty [-1]);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Indexer_Int_OutOfRange ()
		{
			Assert.IsNotNull (empty [0]);
		}

		[Test]
		public void Indexer_Int ()
		{
			X509ExtensionCollection c = new X509ExtensionCollection ();
			c.Add (extn_empty);
			Assert.AreEqual (extn_empty, c[0], "0");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Indexer_String_Null ()
		{
			Assert.IsNotNull (empty [null]);
		}

		[Test]
		public void Indexer_String ()
		{
			X509ExtensionCollection c = new X509ExtensionCollection ();
			c.Add (extn_empty);
			Assert.IsNull (c[String.Empty]);
			Assert.IsNull (c ["1.2.3"]);
			Assert.AreEqual (extn_empty, c["1.2"], "0");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Add_Null ()
		{
			empty.Add (null);
		}

		[Test]
		public void Add ()
		{
			X509ExtensionCollection c = new X509ExtensionCollection ();
			Assert.AreEqual (0, c.Count, "Count-0");
			c.Add (extn_empty);
			Assert.AreEqual (1, c.Count, "Count-1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CopyTo_Null ()
		{
			empty.CopyTo (null, 0);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void CopyTo_Negative ()
		{
			X509Extension[] array = new X509Extension[1];
			empty.CopyTo (array, -1);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void CopyTo_EmptyArray ()
		{
			X509Extension[] array = new X509Extension[0];
			empty.CopyTo (array, 0);
		}

		[Test]
		public void CopyTo_EmptyCollection ()
		{
			X509Extension[] array = new X509Extension[1];
			empty.CopyTo (array, 0);
		}

		[Test]
		public void CopyTo ()
		{
			X509ExtensionCollection c = new X509ExtensionCollection ();
			c.Add (extn_empty);
			X509Extension[] array = new X509Extension[1];
			c.CopyTo (array, 0);
			Assert.AreEqual (extn_empty, array[0], "0");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ICollection_CopyTo_Null ()
		{
			(empty as ICollection).CopyTo (null, 0);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ICollection_CopyTo_Negative ()
		{
			X509Extension[] array = new X509Extension[1];
			(empty as ICollection).CopyTo (array, -1);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ICollection_CopyTo_EmptyArray ()
		{
			X509Extension[] array = new X509Extension[0];
			(empty as ICollection).CopyTo (array, 0);
		}

		[Test]
		public void ICollection_CopyTo_EmptyCollection ()
		{
			X509Extension[] array = new X509Extension[1];
			(empty as ICollection).CopyTo (array, 0);
		}

		[Test]
		public void ICollection_CopyTo ()
		{
			X509ExtensionCollection c = new X509ExtensionCollection ();
			c.Add (extn_empty);
			X509Extension[] array = new X509Extension[1];
			(c as ICollection).CopyTo (array, 0);
			Assert.AreEqual (extn_empty, array[0], "0");
		}
	}
}

#endif
