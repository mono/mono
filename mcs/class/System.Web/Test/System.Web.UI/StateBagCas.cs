//
// StateBagCas.cs - CAS unit tests for System.Web.UI.StateBag
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
	public class StateBagCas : AspNetHostingMinimal {

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Deny_Unrestricted ()
		{
			StateBag bag = new StateBag (true);
			Assert.IsNotNull (bag.Add ("key", "value"), "Add");
			Assert.AreEqual (1, bag.Count, "Count");
			Assert.IsNotNull (bag.GetEnumerator (), "GetEnumerator");
			bag.SetItemDirty ("key", true);
			Assert.IsTrue (bag.IsItemDirty ("key"), "IsItemDirty");
			bag.Remove ("key");

			bag.Clear ();
			bag["key"] = "value";
			Assert.IsNotNull (bag["key"], "this[string]");
			Assert.IsNotNull (bag.Keys, "Keys");
			Assert.IsNotNull (bag.Values, "Values");
#if NET_2_0
			bag.SetDirty (true);
#endif
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void IStateManager_Deny_Unrestricted ()
		{
			IStateManager sm = new StateBag ();
			Assert.IsFalse (sm.IsTrackingViewState, "IsTrackingViewState");
			object state = sm.SaveViewState ();
			sm.LoadViewState (state);
			sm.TrackViewState ();
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void IDictionary_Deny_Unrestricted ()
		{
			IDictionary d = new StateBag ();
			d.Add ("key", "value");
			Assert.IsTrue (d.Contains ("key"), "Contains");
			Assert.AreEqual (1, d.Count, "Count");
			d.Remove ("key");
			d["key"] = "value";
			Assert.AreEqual ("value", d["key"], "this[string]");
			d.Clear ();
			Assert.IsFalse (d.IsFixedSize, "IsFixedSize");
			Assert.IsFalse (d.IsReadOnly, "IsReadOnly");

			ICollection c = (d as ICollection);
			Assert.IsFalse (c.IsSynchronized, "IsSynchronized");
			Assert.IsNotNull (c.SyncRoot, "SyncRoot");
		}

		// LinkDemand

		public override Type Type {
			get { return typeof (StateBag); }
		}
	}
}
