//
// PersistenceModeAttributeCas.cs 
//	- CAS unit tests for System.Web.UI.PersistenceModeAttribute
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
using System.Security.Permissions;
using System.Web;
using System.Web.UI;

namespace MonoCasTests.System.Web.UI {

	[TestFixture]
	[Category ("CAS")]
	public class PersistenceModeAttributeCas : AspNetHostingMinimal {

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Deny_Unrestricted ()
		{
			PersistenceModeAttribute pma = new PersistenceModeAttribute (PersistenceMode.Attribute);
			Assert.AreEqual (PersistenceMode.Attribute, pma.Mode, "Mode");
			Assert.IsTrue (pma.Equals (pma), "Equals");
			Assert.IsTrue (pma.IsDefaultAttribute (), "IsDefaultAttribute");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Static_Deny_Unrestricted ()
		{
			Assert.IsNotNull (PersistenceModeAttribute.Default, "Default");
			Assert.IsNotNull (PersistenceModeAttribute.Attribute, "Attribute");
			Assert.IsNotNull (PersistenceModeAttribute.EncodedInnerDefaultProperty, "EncodedInnerDefaultProperty");
			Assert.IsNotNull (PersistenceModeAttribute.InnerDefaultProperty, "InnerDefaultProperty");
			Assert.IsNotNull (PersistenceModeAttribute.InnerProperty, "InnerProperty");
		}

		// LinkDemand

		public override object CreateControl (SecurityAction action, AspNetHostingPermissionLevel level)
		{
			ConstructorInfo ci = this.Type.GetConstructor (new Type[1] { typeof (PersistenceMode) });
			Assert.IsNotNull (ci, ".ctor(PersistenceMode)");
			return ci.Invoke (new object[1] { PersistenceMode.Attribute });
		}

		public override Type Type {
			get { return typeof (PersistenceModeAttribute); }
		}
	}
}
