//
// SecurityManagerCas.cs - CAS unit tests for System.Security.SecurityManager
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
using System.Collections;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;

namespace MonoCasTests.System.Security {

	[TestFixture]
	[Category ("CAS")]
	public class SecurityManagerCas {

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}

		[Test]
		public void IsGranted_Null ()
		{
			// null is always granted
			Assert.IsTrue (SecurityManager.IsGranted (null));
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlPolicy = true)]
		[ExpectedException (typeof (SecurityException))]
		public void CheckExecutionRights_DenyControlPolicy ()
		{
			SecurityManager.CheckExecutionRights = SecurityManager.CheckExecutionRights;
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, ControlPolicy = true)]
		public void CheckExecutionRights_PermitOnlyControlPolicy ()
		{
			SecurityManager.CheckExecutionRights = SecurityManager.CheckExecutionRights;
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlPolicy = true)]
#if NET_2_0
		// it seems that this was removed in 2.0 - maybe because you can't turn CAS off ?!?
#else
		[ExpectedException (typeof (SecurityException))]
#endif
		public void SecurityEnabled_DenyControlPolicy ()
		{
			SecurityManager.SecurityEnabled = false;
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, ControlPolicy = true)]
		public void SecurityEnabled_PermitOnlyControlPolicy ()
		{
			SecurityManager.SecurityEnabled = SecurityManager.SecurityEnabled;
		}

		// identities permission are unrestricted since 2.0
		// the Deny shows that IsGranted only checks for assembly 
		// granted set (and not the stack modifiers)

#if NET_2_0
		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void IsGranted_GacIdentityPermission ()
		{
			GacIdentityPermission gip = new GacIdentityPermission ();
			Assert.IsTrue (SecurityManager.IsGranted (gip));
		}
#endif
		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void IsGranted_ZoneIdentityPermission ()
		{
			ZoneIdentityPermission zip = new ZoneIdentityPermission (SecurityZone.Internet);
#if NET_2_0
			Assert.IsTrue (SecurityManager.IsGranted (zip));
#else
			Assert.IsFalse (SecurityManager.IsGranted (zip));
#endif
		}

		[Test]
		[ExpectedException (typeof (PolicyException))]
		public void ResolvePolicy_Evidence_AllNull ()
		{
			Assert.IsTrue (SecurityManager.CheckExecutionRights, "CheckExecutionRights");
			PermissionSet denied = null;
			// null (2nd) is missing the Execution right
			SecurityManager.ResolvePolicy (null, null, null, null, out denied);
		}

		[Test]
		[ExpectedException (typeof (PolicyException))]
		public void ResolvePolicy_Evidence_MinExec ()
		{
			Assert.IsTrue (SecurityManager.CheckExecutionRights, "CheckExecutionRights");
			PermissionSet ps = new PermissionSet (PermissionState.None);
			ps.AddPermission (new SecurityPermission (SecurityPermissionFlag.Execution));
			PermissionSet denied = null;
			SecurityManager.ResolvePolicy (null, ps, null, null, out denied);
			// the security manager doesn't try the optional permissions to find the execution right
		}

		[Test]
		[ExpectedException (typeof (PolicyException))]
		public void ResolvePolicy_Evidence_MinNullExecOpt ()
		{
			Assert.IsTrue (SecurityManager.CheckExecutionRights, "CheckExecutionRights");
			PermissionSet ps = new PermissionSet (PermissionState.None);
			ps.AddPermission (new SecurityPermission (SecurityPermissionFlag.Execution));
			PermissionSet denied = null;
			// null (2nd) is missing the Execution right
			SecurityManager.ResolvePolicy (null, null, ps, null, out denied);
		}
	}
}
