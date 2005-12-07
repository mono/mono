//
// DataListItemCollectionTest.cs
//	- Unit tests for System.Web.UI.WebControls.DataListItemCollection
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
	public class DataListItemCollectionTest {

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void Constructor_Null ()
		{
			DataListItemCollection dlic = new DataListItemCollection (null);
			Assert.IsNotNull (dlic, "ctor");
			Assert.IsFalse (dlic.IsReadOnly, "IsReadOnly");
			Assert.IsFalse (dlic.IsSynchronized, "IsSynchronized");
			Assert.IsTrue (Object.ReferenceEquals (dlic, dlic.SyncRoot), "SyncRoot");
			// unusable
			Assert.AreEqual (0, dlic.Count, "NRE");
		}

		[Test]
		public void Constructor_Empty ()
		{
			ArrayList al = new ArrayList ();
			DataListItemCollection dlic = new DataListItemCollection (al);
			Assert.AreEqual (0, dlic.Count, "Count0");
			Assert.IsFalse (dlic.IsReadOnly, "IsReadOnly");
			Assert.IsFalse (dlic.IsSynchronized, "IsSynchronized");
			Assert.IsTrue (Object.ReferenceEquals (dlic, dlic.SyncRoot), "SyncRoot");

			al.Add (new DataListItem (0, ListItemType.Item));
			Assert.AreEqual (1, dlic.Count, "Count++");
			// note: no add/insert/remove/...
			Assert.IsNotNull (dlic[0], "[0]");

			al.Clear ();
			Assert.AreEqual (0, dlic.Count, "Count--");
			// we can add/remove from the original ArrayList
		}

		[Test]
		[ExpectedException (typeof (InvalidCastException))]
		public void Constructor_WrongType ()
		{
			ArrayList al = new ArrayList ();
			al.Add (String.Empty);
			// DataListItemCollection only deals with DataListItem so...

			DataListItemCollection dlic = new DataListItemCollection (al);
			Assert.AreEqual (1, dlic.Count, "Count0");

			// ... it chokes when accessing the string
			Assert.AreEqual (String.Empty, dlic[0], "[0]");
		}
	}
}
