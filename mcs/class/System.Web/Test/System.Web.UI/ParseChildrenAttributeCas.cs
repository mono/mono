//
// ParseChildrenAttributeCas.cs 
//	- CAS unit tests for System.Web.UI.ParseChildrenAttribute
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
	public class ParseChildrenAttributeCas : AspNetHostingMinimal {

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Ctor_Deny_Unrestricted ()
		{
			ParseChildrenAttribute pca = new ParseChildrenAttribute ();
			Assert.IsFalse (pca.ChildrenAsProperties, "ChildrenAsProperties");
			Assert.AreEqual (String.Empty, pca.DefaultProperty, "DefaultProperty");
			Assert.IsTrue (pca.Equals (pca), "Equals");
			Assert.IsTrue (pca.IsDefaultAttribute (), "IsDefaultAttribute");
			// this throws a NullReferenceException on MS 2.0 beta2
			// Assert.IsTrue (pca.GetHashCode () != 0, "GetHashCode"); // likely
#if NET_2_0
			Assert.AreEqual (typeof (Control), pca.ChildControlType, "ChildControlType");
#endif
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void CtorBool_Deny_Unrestricted ()
		{
			ParseChildrenAttribute pca = new ParseChildrenAttribute (true);
			Assert.IsTrue (pca.ChildrenAsProperties, "ChildrenAsProperties");
			Assert.AreEqual (String.Empty, pca.DefaultProperty, "DefaultProperty");
			Assert.IsTrue (pca.Equals (pca), "Equals");
			Assert.IsFalse (pca.IsDefaultAttribute (), "IsDefaultAttribute");
			Assert.IsTrue (pca.GetHashCode () != 0, "GetHashCode"); // likely
#if NET_2_0
			Assert.AreEqual (typeof (Control), pca.ChildControlType, "ChildControlType");
#endif
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void CtorBoolString_Deny_Unrestricted ()
		{
			ParseChildrenAttribute pca = new ParseChildrenAttribute (true, "mono");
			Assert.IsTrue (pca.ChildrenAsProperties, "ChildrenAsProperties");
			Assert.AreEqual ("mono", pca.DefaultProperty, "DefaultProperty");
			Assert.IsTrue (pca.Equals (pca), "Equals");
			Assert.IsFalse (pca.IsDefaultAttribute (), "IsDefaultAttribute");
			Assert.IsTrue (pca.GetHashCode () != 0, "GetHashCode"); // likely
#if NET_2_0
			Assert.AreEqual (typeof (Control), pca.ChildControlType, "ChildControlType");
#endif
		}

#if NET_2_0
		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void CtorType_Deny_Unrestricted ()
		{
			ParseChildrenAttribute pca = new ParseChildrenAttribute (typeof (string));
			Assert.IsFalse (pca.ChildrenAsProperties, "ChildrenAsProperties");
			Assert.AreEqual (String.Empty, pca.DefaultProperty, "DefaultProperty");
			Assert.IsTrue (pca.Equals (pca), "Equals");
			Assert.IsFalse (pca.IsDefaultAttribute (), "IsDefaultAttribute");
			Assert.IsTrue (pca.GetHashCode () != 0, "GetHashCode"); // likely
			Assert.AreEqual (typeof (string), pca.ChildControlType, "ChildControlType");
		}
#endif

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Static_Deny_Unrestricted ()
		{
			Assert.IsNotNull (ParseChildrenAttribute.Default, "Default");
#if NET_2_0
			Assert.IsNotNull (ParseChildrenAttribute.ParseAsChildren, "ParseAsChildren");
			Assert.IsNotNull (ParseChildrenAttribute.ParseAsProperties, "ParseAsProperties");
#endif
		}

		// LinkDemand

		public override Type Type {
			get { return typeof (ParseChildrenAttribute); }
		}
	}
}
