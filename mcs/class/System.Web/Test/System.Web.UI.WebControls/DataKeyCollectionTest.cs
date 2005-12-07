//
// DataKeyCollectionTest.cs
//	- Unit tests for System.Web.UI.WebControls.DataKeyCollection
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
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

using System;
using System.Collections;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using NUnit.Framework;

namespace MonoTests.System.Web.UI.WebControls {

	[TestFixture]
	public class DataKeyCollectionTest {

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void Constructor_Null ()
		{
			DataKeyCollection dkc = new DataKeyCollection (null);
			Assert.IsNotNull (dkc, "ctor");
			Assert.IsFalse (dkc.IsReadOnly, "IsReadOnly");
			Assert.IsFalse (dkc.IsSynchronized, "IsSynchronized");
			Assert.IsTrue (Object.ReferenceEquals (dkc, dkc.SyncRoot), "SyncRoot");
			// unusable
			Assert.AreEqual (0, dkc.Count, "NRE");
		}

		[Test]
		public void Constructor_Empty ()
		{
			ArrayList al = new ArrayList ();
			DataKeyCollection dkc = new DataKeyCollection (al);
			Assert.AreEqual (0, dkc.Count, "Count0");
			Assert.IsFalse (dkc.IsReadOnly, "IsReadOnly");
			Assert.IsFalse (dkc.IsSynchronized, "IsSynchronized");
			Assert.IsTrue (Object.ReferenceEquals (dkc, dkc.SyncRoot), "SyncRoot");

			al.Add (String.Empty);
			Assert.AreEqual (1, dkc.Count, "Count++");
			// note: no add/insert/remove/...
			Assert.AreEqual (String.Empty, dkc[0], "[0]");

			al.Clear ();
			Assert.AreEqual (0, dkc.Count, "Count--");
			// we can add/remove from the original ArrayList
		}
	}
}
