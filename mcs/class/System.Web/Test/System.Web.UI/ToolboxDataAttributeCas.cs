//
// ToolboxDataAttributeCas.cs 
//	- CAS unit tests for System.Web.UI.ToolboxDataAttribute
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
using System.ComponentModel.Design;
using System.Reflection;
using System.Security.Permissions;
using System.Web;
using System.Web.UI;

namespace MonoCasTests.System.Web.UI {

	[TestFixture]
	[Category ("CAS")]
	public class ToolboxDataAttributeCas : AspNetHostingMinimal {

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Deny_Unrestricted ()
		{
			ToolboxDataAttribute tda = new ToolboxDataAttribute (null);
			Assert.IsNull (tda.Data, "Data");
			Assert.IsTrue (tda.Equals (tda), "Equals");
			Assert.IsFalse (tda.GetHashCode () == ToolboxDataAttribute.Default.GetHashCode (), "GetHashCode");
#if NET_2_0
			// unexpected result as tda hash code is different from default
			// seems that null and String.Empty are both considered defaults...
			Assert.IsTrue (tda.IsDefaultAttribute (), "IsDefaultAttribute");
#else
			Assert.IsFalse (tda.IsDefaultAttribute (), "IsDefaultAttribute");
#endif
		}

		// LinkDemand

		public override object CreateControl (SecurityAction action, AspNetHostingPermissionLevel level)
		{
			ConstructorInfo ci = this.Type.GetConstructor (new Type[1] { typeof (string) });
			Assert.IsNotNull (ci, ".ctor(string)");
			return ci.Invoke (new object[1] { String.Empty });
		}

		public override Type Type {
			get { return typeof (ToolboxDataAttribute); }
		}
	}
}
