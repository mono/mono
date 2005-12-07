//
// PassportAuthenticationEventArgsCas.cs 
//	- CAS unit tests for System.Web.Security.PassportAuthenticationEventArgs
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
	public class PassportAuthenticationEventArgsCas : AspNetHostingMinimal {

		private HttpContext context;
		private PassportAuthenticationEventArgs paea;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			context = new HttpContext (null);
			paea = new PassportAuthenticationEventArgs (null, context);
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void All_Get_Deny_Unrestricted ()
		{
			Assert.IsNotNull (paea.Context, "Context");
			Assert.IsNull (paea.Identity, "Identity");
			Assert.IsNull (paea.User, "User");
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlPrincipal = true)]
		[ExpectedException (typeof (SecurityException))]
		public void User_Set_Deny_ControlPrincipal ()
		{
			paea.User = new GenericPrincipal (new GenericIdentity ("me"), null);
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, ControlPrincipal = true)]
		public void User_Set_PermitOnly_ControlPrincipal ()
		{
			Assert.IsNull (paea.Context.User, "Context.User-before");
			Assert.IsNull (paea.Identity, "Identity-before");
			Assert.IsNull (paea.User, "User-before");
			paea.User = new GenericPrincipal (new GenericIdentity ("me"), null);
			Assert.IsNull (paea.Context.User, "Context.User-after");
			Assert.IsNull (paea.Identity, "Identity-after");
			Assert.IsNotNull (paea.User, "User-after");
		}

		// LinkDemand

		public override object CreateControl (SecurityAction action, AspNetHostingPermissionLevel level)
		{
			ConstructorInfo ci = this.Type.GetConstructor (new Type[2] { typeof (PassportIdentity), typeof (HttpContext) });
			Assert.IsNotNull (ci, ".ctor(PassportIdentity,HttpContext)");
			return ci.Invoke (new object[2] { null, context });
		}

		public override Type Type {
			get { return typeof (PassportAuthenticationEventArgs); }
		}
	}
}
