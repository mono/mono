//
// FormsAuthenticationEventArgsCas.cs 
//	- CAS unit tests for System.Web.Security.FormsAuthenticationEventArgs
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
using System.Security.Principal;
using System.Web;
using System.Web.Security;

namespace MonoCasTests.System.Web.Security {

	[TestFixture]
	[Category ("CAS")]
	public class FormsAuthenticationEventArgsCas : AspNetHostingMinimal {

		private HttpContext context;
		private FormsAuthenticationEventArgs faea;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			context = new HttpContext (null);
			faea = new FormsAuthenticationEventArgs (context);
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void All_Get_Deny_Unrestricted ()
		{
			Assert.IsNotNull (faea.Context, "Context");
			Assert.IsNull (faea.User, "User");
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlPrincipal = true)]
		[ExpectedException (typeof (SecurityException))]
		public void User_Set_Deny_ControlPrincipal ()
		{
			faea.User = new GenericPrincipal (new GenericIdentity ("me"), null);
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, ControlPrincipal = true)]
		public void User_Set_PermitOnly_ControlPrincipal ()
		{
			Assert.IsNull (faea.Context.User, "Context.User-before");
			Assert.IsNull (faea.User, "User-before");
			faea.User = new GenericPrincipal (new GenericIdentity ("me"), null);
			Assert.IsNull (faea.Context.User, "Context.User-after");
			Assert.IsNotNull (faea.User, "User-after");
		}

		// LinkDemand

		public override object CreateControl (SecurityAction action, AspNetHostingPermissionLevel level)
		{
			ConstructorInfo ci = this.Type.GetConstructor (new Type[1] { typeof (HttpContext) });
			Assert.IsNotNull (ci, ".ctor(HttpContext)");
			return ci.Invoke (new object[1] { context });
		}

		public override Type Type {
			get { return typeof (DefaultAuthenticationEventArgs); }
		}
	}
}
