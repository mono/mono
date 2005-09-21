//
// AspNetHostingNone.cs - CAS unit tests helper for System.Web
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

	public abstract class AspNetHostingNone : AspNetHostingPermissionHelper {

		[Test]
		[AspNetHostingPermission (SecurityAction.PermitOnly, Level = AspNetHostingPermissionLevel.Unrestricted)]
		public void LinkDemand_PermitOnly_Unrestricted ()
		{
			Assert.IsNotNull (CreateControl (SecurityAction.PermitOnly, AspNetHostingPermissionLevel.Unrestricted));
		}

		[Test]
		[AspNetHostingPermission (SecurityAction.PermitOnly, Level = AspNetHostingPermissionLevel.High)]
		public void LinkDemand_PermitOnly_High ()
		{
			Assert.IsNotNull (CreateControl (SecurityAction.PermitOnly, AspNetHostingPermissionLevel.High));
		}

		[Test]
		[AspNetHostingPermission (SecurityAction.PermitOnly, Level = AspNetHostingPermissionLevel.Medium)]
		public void LinkDemand_PermitOnly_Medium ()
		{
			Assert.IsNotNull (CreateControl (SecurityAction.PermitOnly, AspNetHostingPermissionLevel.Medium));
		}

		[Test]
		[AspNetHostingPermission (SecurityAction.PermitOnly, Level = AspNetHostingPermissionLevel.Low)]
		public void LinkDemand_PermitOnly_Low ()
		{
			Assert.IsNotNull (CreateControl (SecurityAction.PermitOnly, AspNetHostingPermissionLevel.Low));
		}

		[Test]
		[AspNetHostingPermission (SecurityAction.PermitOnly, Level = AspNetHostingPermissionLevel.Minimal)]
		public void LinkDemand_PermitOnly_Minimal ()
		{
			Assert.IsNotNull (CreateControl (SecurityAction.PermitOnly, AspNetHostingPermissionLevel.Minimal));
		}

		[Test]
		[AspNetHostingPermission (SecurityAction.PermitOnly, Level = AspNetHostingPermissionLevel.None)]
		public void LinkDemand_PermitOnly_None ()
		{
			CreateControl (SecurityAction.PermitOnly, AspNetHostingPermissionLevel.None);
		}

		[Test]
		[AspNetHostingPermission (SecurityAction.Deny, Level = AspNetHostingPermissionLevel.Unrestricted)]
		public void LinkDemand_Deny_Unrestricted ()
		{
			CreateControl (SecurityAction.Deny, AspNetHostingPermissionLevel.Unrestricted);
		}

		[Test]
		[AspNetHostingPermission (SecurityAction.Deny, Level = AspNetHostingPermissionLevel.High)]
		public void LinkDemand_Deny_High ()
		{
			CreateControl (SecurityAction.Deny, AspNetHostingPermissionLevel.High);
		}

		[Test]
		[AspNetHostingPermission (SecurityAction.Deny, Level = AspNetHostingPermissionLevel.Medium)]
		public void LinkDemand_Deny_Medium ()
		{
			CreateControl (SecurityAction.Deny, AspNetHostingPermissionLevel.Medium);
		}

		[Test]
		[AspNetHostingPermission (SecurityAction.Deny, Level = AspNetHostingPermissionLevel.Low)]
		public void LinkDemand_Deny_Low ()
		{
			CreateControl (SecurityAction.Deny, AspNetHostingPermissionLevel.Low);
		}

		[Test]
		[AspNetHostingPermission (SecurityAction.Deny, Level = AspNetHostingPermissionLevel.Minimal)]
		public void LinkDemand_Deny_Minimal ()
		{
			CreateControl (SecurityAction.Deny, AspNetHostingPermissionLevel.Minimal);
		}

		[Test]
		[AspNetHostingPermission (SecurityAction.Deny, Level = AspNetHostingPermissionLevel.None)]
		public void LinkDemand_Deny_None ()
		{
			Assert.IsNotNull (CreateControl (SecurityAction.Deny, AspNetHostingPermissionLevel.None));
		}
	}
}
