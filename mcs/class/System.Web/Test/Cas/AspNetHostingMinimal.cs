//
// AspNetHostingPermissionHelper.cs - CAS unit tests helper for System.Web
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
using System.Security;
using System.Security.Permissions;
using System.Web;

namespace MonoCasTests {

	public abstract class AspNetHostingMinimal : AspNetHostingPermissionHelper {

		[Test]
		[AspNetHostingPermission (SecurityAction.PermitOnly, Level = AspNetHostingPermissionLevel.Unrestricted)]
		public void PermitOnly_Unrestricted ()
		{
			Assert.IsNotNull (CreateControl ());
		}

		[Test]
		[AspNetHostingPermission (SecurityAction.PermitOnly, Level = AspNetHostingPermissionLevel.High)]
		public void PermitOnly_High ()
		{
			Assert.IsNotNull (CreateControl ());
		}

		[Test]
		[AspNetHostingPermission (SecurityAction.PermitOnly, Level = AspNetHostingPermissionLevel.Medium)]
		public void PermitOnly_Medium ()
		{
			Assert.IsNotNull (CreateControl ());
		}

		[Test]
		[AspNetHostingPermission (SecurityAction.PermitOnly, Level = AspNetHostingPermissionLevel.Low)]
		public void PermitOnly_Low ()
		{
			Assert.IsNotNull (CreateControl ());
		}

		[Test]
		[AspNetHostingPermission (SecurityAction.PermitOnly, Level = AspNetHostingPermissionLevel.Minimal)]
		public void PermitOnly_Minimal ()
		{
			Assert.IsNotNull (CreateControl ());
		}

		[Test]
		[AspNetHostingPermission (SecurityAction.PermitOnly, Level = AspNetHostingPermissionLevel.None)]
		[ExpectedException (typeof (SecurityException))]
		public void PermitOnly_None ()
		{
			CreateControl ();
		}

		[Test]
		[AspNetHostingPermission (SecurityAction.Deny, Level = AspNetHostingPermissionLevel.Unrestricted)]
		[ExpectedException (typeof (SecurityException))]
		public void Deny_Unrestricted ()
		{
			CreateControl ();
		}

		[Test]
		[AspNetHostingPermission (SecurityAction.Deny, Level = AspNetHostingPermissionLevel.High)]
		[ExpectedException (typeof (SecurityException))]
		public void Deny_High ()
		{
			CreateControl ();
		}

		[Test]
		[AspNetHostingPermission (SecurityAction.Deny, Level = AspNetHostingPermissionLevel.Medium)]
		[ExpectedException (typeof (SecurityException))]
		public void Deny_Medium ()
		{
			CreateControl ();
		}

		[Test]
		[AspNetHostingPermission (SecurityAction.Deny, Level = AspNetHostingPermissionLevel.Low)]
		[ExpectedException (typeof (SecurityException))]
		public void Deny_Low ()
		{
			CreateControl ();
		}

		[Test]
		[AspNetHostingPermission (SecurityAction.Deny, Level = AspNetHostingPermissionLevel.Minimal)]
		[ExpectedException (typeof (SecurityException))]
		public void Deny_Minimal ()
		{
			CreateControl ();
		}

		[Test]
		[AspNetHostingPermission (SecurityAction.Deny, Level = AspNetHostingPermissionLevel.None)]
		public void Deny_None ()
		{
			Assert.IsNotNull (CreateControl ());
		}
	}
}
