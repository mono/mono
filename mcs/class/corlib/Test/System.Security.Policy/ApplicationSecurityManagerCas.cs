//
// ApplicationSecurityManagerCas.cs -
//	CAS unit tests for System.Security.Policy.ApplicationSecurityManager
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
using System.Collections;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;

namespace MonoCasTests.System.Security.Policy {

	[TestFixture]
	[Category ("CAS")]
	public class ApplicationSecurityManagerCas {

		private string defaultTrustManagerTypeName;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			defaultTrustManagerTypeName = ApplicationSecurityManager.ApplicationTrustManager.GetType ().AssemblyQualifiedName;
		}

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlPolicy = true)]
		[ExpectedException (typeof (SecurityException))]
		public void ApplicationTrustManager_DenyControlPolicy ()
		{
			Assert.IsNotNull (ApplicationSecurityManager.ApplicationTrustManager);
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, ControlPolicy = true)]
		public void ApplicationTrustManager_PermitOnlyControlPolicy ()
		{
			Assert.IsNotNull (ApplicationSecurityManager.ApplicationTrustManager);
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlPolicy = true)]
		[ExpectedException (typeof (SecurityException))]
		public void UserApplicationTrusts_DenyControlPolicy ()
		{
			// no security requirements documented
			Assert.AreEqual (0, ApplicationSecurityManager.UserApplicationTrusts.Count);
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, ControlPolicy = true)]
		public void UserApplicationTrusts_PermitOnlyControlPolicy ()
		{
			// no security requirements documented
			Assert.IsNotNull (ApplicationSecurityManager.UserApplicationTrusts);
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (NullReferenceException))]
		public void DetermineApplicationTrust_DenyUnrestricted ()
		{
			// documented as requiring ControlPolicy and ControlEvidence
			// possibly a linkdemand as only ControlPolicy seems check by the default
			// IApplicationTrustManager
			ApplicationSecurityManager.DetermineApplicationTrust (null, null);
		}

		// default trust manager

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlPolicy = true)]
		[ExpectedException (typeof (SecurityException))]
		public void DefaultTrustManager_DetermineApplicationTrust_DenyControlPolicy ()
		{
			ApplicationSecurityManager.ApplicationTrustManager.DetermineApplicationTrust (null, null);
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, ControlPolicy = true)]
		[ExpectedException (typeof (ArgumentNullException))]
		public void DefaultTrustManager_DetermineApplicationTrust_PermitOnlyControlPolicy ()
		{
			ApplicationSecurityManager.ApplicationTrustManager.DetermineApplicationTrust (null, null);
		}

		private void CheckXml (SecurityElement se)
		{
			Assert.AreEqual (defaultTrustManagerTypeName, se.Attribute ("class"), "class");
			Assert.AreEqual ("1", se.Attribute ("version"), "version");
			Assert.AreEqual (2, se.Attributes.Count, "Count");
			Assert.IsNull (se.Children, "Children");
		}

		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		private void CheckFromXml (IApplicationTrustManager atm)
		{
			SecurityElement se = new SecurityElement ("IApplicationTrustManager");
			se.AddAttribute ("class", defaultTrustManagerTypeName);
			se.AddAttribute ("version", "1");
			atm.FromXml (se);
			// accepted
			CheckXml (atm.ToXml ());
		}

		[Test]
		public void DefaultTrustManager_FromXml ()
		{
			CheckFromXml (ApplicationSecurityManager.ApplicationTrustManager);
		}

		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void CheckToXml (IApplicationTrustManager atm)
		{
			CheckXml (atm.ToXml ());
		}

		[Test]
		public void DefaultTrustManager_ToXml ()
		{
			CheckToXml (ApplicationSecurityManager.ApplicationTrustManager);
		}
	}
}

#endif
