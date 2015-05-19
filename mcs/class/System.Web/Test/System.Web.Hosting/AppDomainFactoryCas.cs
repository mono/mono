//
// AppDomainFactoryCas.cs 
//	- CAS unit tests for System.Web.Hosting.AppDomainFactory
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
using System.Web.Hosting;

namespace MonoCasTests.System.Web.Hosting {

	[TestFixture]
	[Category ("CAS")]
	public class AppDomainFactoryCas : AspNetHostingMinimal {

		private AppDomainFactory adf;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			// we're at full trust here
			adf = new AppDomainFactory ();
		}

		// test ctor

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Constructor_Deny_UnmanagedCode ()
		{
			new AppDomainFactory ();
		}

		[Test]
		[AspNetHostingPermission (SecurityAction.Deny, Level = AspNetHostingPermissionLevel.Minimal)]
		[ExpectedException (typeof (SecurityException))]
		public void Constructor_Deny_AspNetHostingPermission ()
		{
			new AppDomainFactory ();
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, UnmanagedCode = true)]
		[AspNetHostingPermission (SecurityAction.PermitOnly, Level = AspNetHostingPermissionLevel.Minimal)]
		public void Constructor_PermitOnly_UnmanagedCode ()
		{
			new AppDomainFactory ();
		}

		// Create isn't protected (so the Demands aren't on the class)

		private void Create ()
		{
			try {
				adf.Create (null, null, null, null, null, 0);
			}
			catch (NullReferenceException) {
				// MS
			}
			catch (NotImplementedException) {
				// Mono
			}
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		public void Create_Deny_UnmanagedCode ()
		{
			Create ();
		}

		[Test]
		[AspNetHostingPermission (SecurityAction.Deny, Level = AspNetHostingPermissionLevel.Minimal)]
		public void Create_Deny_AspNetHostingPermission ()
		{
			Create ();
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Create_Deny_Unrestricted ()
		{
			Create ();
		}

		// test for LinkDemand on class

		[SecurityPermission (SecurityAction.Assert, UnmanagedCode = true)]
		public override object CreateControl (SecurityAction action, AspNetHostingPermissionLevel level)
		{
			// don't let UnmanagedCode mess up the results
			return base.CreateControl (action, level);
		}

		public override Type Type {
			get { return typeof (AppDomainFactory); }
		}
	}
}
