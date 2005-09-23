//
// LoginCas.cs - CAS unit tests for System.Web.UI.WebControls.Login
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
	public class LoginCas : AspNetHostingMinimal {

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Deny_Unrestricted ()
		{
			LoginTest unit = new LoginTest ();
			unit.ReadOnlyFields ();
			unit.DefaultProperties ();
			unit.AssignToDefaultProperties ();
			unit.NullProperties ();
			unit.BorderPadding ();
			unit.FailureAction_All ();
			unit.LoginButtonType_All ();
			unit.Orientation_All ();
			unit.TextLayout_All ();
			unit.SaveViewState ();
			unit.OnBubbleEvent_Cancel_OnLoggingIn ();
			unit.OnLoggingIn_False ();
			unit.OnLoggingIn_True ();
			unit.OnLoginError ();
			unit.OnLoggedIn ();
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void OnAuthenticate_True_Deny_Unrestricted ()
		{
			LoginTest unit = new LoginTest ();
			try {
				unit.OnAuthenticate_True ();
			}
			catch (TypeInitializationException) {
				// ms 2.0 - depending on the test run-order the HttpRuntime may throw
			}
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, ControlPrincipal = true)]
		public void PermitOnly_ControlPrincipal ()
		{
			LoginTest unit = new LoginTest ();
			try {
				unit.OnBubbleEvent ();
				unit.OnAuthenticate_False ();
			}
			catch (TypeInitializationException) {
				// ms 2.0 - depending on the test run-order the HttpRuntime may throw
			}
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlPrincipal = true)]
		public void OnBubbleEvent_Deny_ControlPrincipal ()
		{
			LoginTest unit = new LoginTest ();
			unit.OnBubbleEvent ();
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlPrincipal = true)]
		public void OnAuthenticate_False_Deny_ControlPrincipal ()
		{
			LoginTest unit = new LoginTest ();
			unit.OnAuthenticate_False ();
		}

		// LinkDemand

		public override Type Type {
			get { return typeof (Login); }
		}
	}
}

#endif
