//
// System.Drawing.Printing.Margins unit tests
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
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
using System.Drawing;
using System.Drawing.Printing;
using System.Security.Permissions;
using NUnit.Framework;

namespace MonoTests.System.Drawing.Printing {

	[TestFixture]
	[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
	public class MarginsTest {

		[Test]
		public void CtorDefault ()
		{
			Margins m = new Margins ();
			Assert.AreEqual (100, m.Left, "Left");
			Assert.AreEqual (100, m.Top, "Top");
			Assert.AreEqual (100, m.Right, "Right");
			Assert.AreEqual (100, m.Bottom, "Bottom");
			Assert.AreEqual ("[Margins Left=100 Right=100 Top=100 Bottom=100]", m.ToString (), "ToString");
			Margins clone = (Margins) m.Clone ();
			Assert.AreEqual (m, clone, "clone");
#if NET_2_0
			Assert.IsTrue (m == clone, "==");
			Assert.IsFalse (m != clone, "!=");
#endif
		}

		[Test]
		public void Ctor4Int ()
		{
			Margins m1 = new Margins (Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue);
			Assert.AreEqual (Int32.MaxValue, m1.Left, "Left");
			Assert.AreEqual (Int32.MaxValue, m1.Top, "Top");
			Assert.AreEqual (Int32.MaxValue, m1.Right, "Right");
			Assert.AreEqual (Int32.MaxValue, m1.Bottom, "Bottom");
			// right smaller than left
			Margins m2 = new Margins (Int32.MaxValue, 0, 10, 20);
			// bottom smaller than top
			Margins m3 = new Margins (10, 20, Int32.MaxValue, 0);
			Assert.IsFalse (m2.GetHashCode () == m3.GetHashCode (), "GetHashCode");
#if NET_2_0
			Assert.IsTrue (m1 != m2, "m1 != m2");
			Assert.IsFalse (m1 == m2, "m1 == m2");
#endif
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Ctor_BadLeft ()
		{
			new Margins (-1, 0, 0, 0); 
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Ctor_BadRight ()
		{
			new Margins (0, Int32.MinValue, 0, 0);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Ctor_BadTop ()
		{
			new Margins (0, 0, Int32.MinValue, 0);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Ctor_BadBottom ()
		{
			new Margins (0, 0, 0, -1);
		}

		[Test]
		public void Equals ()
		{
			Margins m = new Margins ();
			Assert.IsTrue (m.Equals (m), "Equals(m)");
			Assert.IsFalse (m.Equals (null), "Equals(null)");
		}

		[Test]
		public void OperatorsWithNulls ()
		{
			Margins m1 = null;
			Margins m2 = null;
#if NET_2_0
			Assert.IsTrue (m1 == m2, "null==null");
			Assert.IsFalse (m1 != m2, "null!=null");
#endif
		}
	}
}
