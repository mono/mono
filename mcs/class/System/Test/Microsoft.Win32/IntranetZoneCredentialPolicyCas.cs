//
// IntranetZoneCredentialPolicyCas.cs 
//	- CAS unit tests for Microsoft.Win32.IntranetZoneCredentialPolicy
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
using Microsoft.Win32;

using MonoTests.Microsoft.Win32;

namespace MonoCasTests.Microsoft.Win32 {

	[TestFixture]
	[Category ("CAS")]
	public class IntranetZoneCredentialPolicyCas {

		private IntranetZoneCredentialPolicyTest unit;


		[SetUp]
		public virtual void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");

			// execute IntranetZoneCredentialPolicy ctor at fulltrust
			unit = new IntranetZoneCredentialPolicyTest ();
			unit.FixtureSetUp ();
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlPolicy = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Constructor_Deny_ControlPolicy ()
		{
			new IntranetZoneCredentialPolicy ();
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, ControlPolicy = true)]
		public void Constructor_PermitOnly_ControlPolicy ()
		{
			new IntranetZoneCredentialPolicy ();
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlPolicy = true)]
		public void UnitTestReuse ()
		{
			unit.NullRequest ();
			unit.NullCredential ();
			unit.NullModule ();
			unit.Localhost ();
			unit.LocalhostWithoutWebRequest ();
			unit.LocalhostWithoutCredentials ();
			unit.LocalhostWithoutModule ();
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlPolicy = true)]
		[ExpectedException (typeof (SecurityException))]
		public void LinkDemand_Deny_ControlPolicy ()
		{
			ConstructorInfo ci = typeof (IntranetZoneCredentialPolicy).GetConstructor (new Type[0]);
			Assert.IsNotNull (ci, "default .ctor");
			try {
				ci.Invoke (null);
			}
			catch (TargetInvocationException tie) {
				// same as directly calling the ctor
				throw tie.InnerException;
			}
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, ControlPolicy = true)]
		public void LinkDemand_PermitOnly_ControlPolicy ()
		{
			ConstructorInfo ci = typeof (IntranetZoneCredentialPolicy).GetConstructor (new Type[0]);
			Assert.IsNotNull (ci, "default .ctor");
			Assert.IsNotNull (ci.Invoke (null), "invoke");
		}
	}
}

