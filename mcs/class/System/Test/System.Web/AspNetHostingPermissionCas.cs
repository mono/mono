//
// AspNetHostingPermissionCas.cs 
//	- CAS unit tests for System.Web.AspNetHostingPermission
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
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Web;

namespace MonoCasTests.System.Web {

	[TestFixture]
	[Category ("CAS")]
	public class AspNetHostingPermissionCas {

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}

		private void CommonTests (AspNetHostingPermission p)
		{
			Assert.IsNotNull (p.Copy (), "Copy");
			SecurityElement se = p.ToXml ();
			Assert.IsNotNull (se, "ToXml");
			p.FromXml (se);
			Assert.IsNotNull (p.Intersect (p), "Intersect");
			Assert.IsTrue (p.IsSubsetOf (p), "IsSubsetOf");
			Assert.IsNotNull (p.Union (p), "Union");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void ConstructorLevel_Deny_Unrestricted ()
		{
			AspNetHostingPermission p = new AspNetHostingPermission (AspNetHostingPermissionLevel.Unrestricted);
			Assert.AreEqual (AspNetHostingPermissionLevel.Unrestricted, p.Level, "Level");
			Assert.IsTrue (p.IsUnrestricted (), "IsUnrestricted");
			CommonTests (p);
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void ConstructorState_Deny_Unrestricted ()
		{
			AspNetHostingPermission p = new AspNetHostingPermission (PermissionState.None);
			Assert.AreEqual (AspNetHostingPermissionLevel.None, p.Level, "Level");
			Assert.IsFalse (p.IsUnrestricted (), "IsUnrestricted");
			CommonTests (p);
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void LinkDemand_Deny_Unrestricted ()
		{
			Type[] types = new Type[1] { typeof (PermissionState) };
			ConstructorInfo ci = typeof (AspNetHostingPermission).GetConstructor (types);
			Assert.IsNotNull (ci, ".ctor(PermissionState)");
			Assert.IsNotNull (ci.Invoke (new object[1] { PermissionState.None }), "invoke");
		}
	}
}
