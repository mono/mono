//
// Tests for System.Web.UI.WebControls.WebParts.ConnectionInterfacesCollectoinTest.cs 
//
// Author:
//	Chris Toshok (toshok@novell.com)
//

//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System;
using System.Collections;
using NUnit.Framework;
using CIC = System.Web.UI.WebControls.WebParts.ConnectionInterfaceCollection;

namespace MonoTests.System.Web.UI.WebControls.WebParts
{
	[TestFixture]
	public class ConnectionInterfaceCollectionTest
	{

		[Test]
		public void Empty ()
		{
			CIC col = CIC.Empty;
			Assert.AreEqual (0, col.Count, "A1");
		}

		[Test]
		public void Ctor0 ()
		{
			CIC col = new CIC ();
			Assert.AreEqual (0, col.Count, "A1");
		}

		[Test]
		public void Ctor1()
		{
			ArrayList a = new ArrayList ();

			a.Add (typeof (string));
			a.Add (typeof (object));

			CIC col = new CIC (a);

			Assert.AreEqual (2, col.Count, "A1");
			Assert.IsTrue   (col.Contains (typeof (string)), "A2");
			Assert.IsTrue   (col.Contains (typeof (object)), "A3");
			Assert.AreEqual (0, col.IndexOf (typeof (string)), "A4");
			Assert.AreEqual (1, col.IndexOf (typeof (object)), "A5");
		}

		[Test]
		public void Ctor1_dup ()
		{
			ArrayList a = new ArrayList ();

			a.Add (typeof (string));
			a.Add (typeof (string));

			CIC col = new CIC (a);

			Assert.AreEqual (2, col.Count, "A1");
			Assert.IsTrue   (col.Contains (typeof (string)), "A2");
			Assert.AreEqual (0, col.IndexOf (typeof (string)), "A3");
		}

		[Test]
		public void Ctor2 ()
		{
			ArrayList a = new ArrayList ();

			a.Add (typeof (string));
			a.Add (typeof (object));

			CIC col1 = new CIC (a);

			a = new ArrayList ();
			a.Add (typeof (int));
			a.Add (typeof (short));

			CIC col2 = new CIC (col1, a);
			
			Assert.AreEqual (4, col2.Count, "A1");
			Assert.IsTrue   (col2.Contains (typeof (string)), "A2");
			Assert.IsTrue   (col2.Contains (typeof (object)), "A3");
			Assert.IsTrue   (col2.Contains (typeof (int)),    "A4");
			Assert.IsTrue   (col2.Contains (typeof (short)),  "A5");
			Assert.AreEqual (0, col2.IndexOf (typeof (string)), "A6");
			Assert.AreEqual (1, col2.IndexOf (typeof (object)), "A7");
			Assert.AreEqual (2, col2.IndexOf (typeof (int)),    "A8");
			Assert.AreEqual (3, col2.IndexOf (typeof (short)),  "A9");
		}

		[Test]
		public void Ctor2_dup ()
		{
			ArrayList a = new ArrayList ();

			a.Add (typeof (string));
			a.Add (typeof (object));

			CIC col1 = new CIC (a);

			a = new ArrayList ();
			a.Add (typeof (string));
			a.Add (typeof (object));

			CIC col2 = new CIC (col1, a);
			
			Assert.AreEqual (4, col2.Count, "A1");
			Assert.IsTrue   (col2.Contains (typeof (string)), "A2");
			Assert.IsTrue   (col2.Contains (typeof (object)), "A3");
			Assert.AreEqual (0, col2.IndexOf (typeof (string)), "A4");
			Assert.AreEqual (1, col2.IndexOf (typeof (object)), "A5");
		}

	}
}
#endif
