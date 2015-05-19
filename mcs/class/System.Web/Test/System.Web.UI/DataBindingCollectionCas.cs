//
// DataBindingCollectionCas.cs 
//	- CAS unit tests for System.Web.UI.DataBindingCollection
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

using NUnit.Framework;

using System;
using System.Collections;
using System.Security.Permissions;
using System.Web;
using System.Web.UI;

namespace MonoCasTests.System.Web.UI {

	[TestFixture]
	[Category ("CAS")]
	public class DataBindingCollectionCas : AspNetHostingMinimal {

		private DataBinding db;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			db = new DataBinding ("property", typeof (string), "");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Deny_Unrestricted ()
		{
			DataBindingCollection dbc = new DataBindingCollection ();
			Assert.AreEqual (0, dbc.Count, "Count");
			Assert.IsFalse (dbc.IsReadOnly, "IsReadOnly");
			Assert.IsFalse (dbc.IsSynchronized, "IsSynchronized");
			dbc.Add (db);
			Assert.AreSame (db, dbc["property"], "this[string]");
			Assert.IsNotNull (dbc.RemovedBindings, "RemovedBindings");
			Assert.IsNotNull (dbc.SyncRoot, "SyncRoot");
			Assert.IsNotNull (dbc.GetEnumerator (), "GetEnumerator");
			dbc.CopyTo (new DataBinding[1], 0);
			dbc.Clear ();

			dbc.Add (db);
			dbc.Remove (db);

			dbc.Add (db);
			dbc.Remove ("property");
			dbc.Remove ("property", true);
			dbc.Changed += new EventHandler (Handler);
			Assert.IsFalse (dbc.Contains ("property"), "Contains");
			dbc.Changed -= new EventHandler (Handler);
		}

		private void Handler (object sender, EventArgs e)
		{
		}
		// LinkDemand

		public override Type Type {
			get { return typeof (DataBindingCollection); }
		}
	}
}
