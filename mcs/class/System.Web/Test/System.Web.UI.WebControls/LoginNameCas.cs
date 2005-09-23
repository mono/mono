//
// LoginNameCas.cs 
//	- CAS unit tests for System.Web.UI.WebControls.LoginName
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

#if NET_2_0

using NUnit.Framework;

using System;
using System.Security;
using System.Security.Permissions;
using System.Web;
using System.Web.UI.WebControls;

using MonoTests.System.Web.UI.WebControls;

namespace MonoCasTests.System.Web.UI.WebControls {

	[TestFixture]
	[Category ("CAS")]
	public class LoginNameCas : AspNetHostingMinimal {

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Deny_Unrestricted ()
		{
			LoginNameTest unit = new LoginNameTest ();
			unit.DefaultProperties ();
			unit.SetOriginalProperties ();
			unit.CleanProperties ();
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlPrincipal = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Deny_ControlPrincipal ()
		{
			LoginNameTest unit = new LoginNameTest ();
			unit.CacheIdentity ();
			// other unit tests fails for the same reason, i.e.
			// setting the Page.Context.User property
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, ControlPrincipal = true)]
		public void PermitOnly_ControlPrincipal ()
		{
			LoginNameTest unit = new LoginNameTest ();
			unit.CacheIdentity ();
			unit.Render_Anonymous_NoPrincipal ();
			unit.Render_Anonymous_IPrincipal ();
			unit.Render_User ();
			unit.Render_UnauthenticatedUser ();
			unit.Render_NoPage ();
			unit.Render_StringFormat ();
			unit.Render_StringFormat_Empty ();
			unit.Render_StringFormat_NoVar ();
		}

		// LinkDemand

		public override Type Type {
			get { return typeof (LoginName); }
		}
	}
}

#endif
