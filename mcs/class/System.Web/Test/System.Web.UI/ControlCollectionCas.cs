//
// ControlCollectionCas.cs 
//	- CAS unit tests for System.Web.UI.ControlCollection
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
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Web;
using System.Web.UI;

namespace MonoCasTests.System.Web.UI {

	[TestFixture]
	[Category ("CAS")]
	public class ControlCollectionCas : AspNetHostingMinimal {

		private Control control;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			control = new Control ();
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Deny_Unrestricted ()
		{
			// note: using the same control (as owner) to add results 
			// in killing the ms runtime with a stackoverflow - FDBK36722
			ControlCollection cc = new ControlCollection (new Control ());
			Assert.AreEqual (0, cc.Count, "Count");
			Assert.IsFalse (cc.IsReadOnly, "IsReadOnly");
			Assert.IsFalse (cc.IsSynchronized, "IsSynchronized");
			Assert.IsNotNull (cc.SyncRoot, "SyncRoot");

			cc.Add (control);
			Assert.IsNotNull (cc[0], "this[int]");
			cc.Clear ();
			cc.AddAt (0, control);
			Assert.IsTrue (cc.Contains (control), "Contains");

			cc.CopyTo (new Control[1], 0);
			Assert.IsNotNull (cc.GetEnumerator (), "GetEnumerator");
			Assert.AreEqual (0, cc.IndexOf (control), "IndexOf");
			cc.RemoveAt (0);
			cc.Remove (control);
		}

		// LinkDemand

		public override object CreateControl (SecurityAction action, AspNetHostingPermissionLevel level)
		{
			ConstructorInfo ci = this.Type.GetConstructor (new Type[1] { typeof (Control) });
			Assert.IsNotNull (ci, ".ctor(Control)");
			return ci.Invoke (new object[1] { control });
		}

		public override Type Type {
			get { return typeof (ControlCollection); }
		}
	}
}
