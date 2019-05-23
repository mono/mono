//
// SecurityExceptionCas.cs - CAS unit tests for 
//	System.Security.SecurityException
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
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;

namespace MonoCasTests.System.Security {

	[TestFixture]
	[Category ("CAS")]
	public class SecurityExceptionCas {

		private SecurityException se;

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");

			se = new SecurityException ();
		}


		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void GetAction ()
		{
			SecurityAction sa = se.Action;
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void SetAction ()
		{
			se.Action = SecurityAction.RequestRefuse;
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, ControlEvidence = true, ControlPolicy = true)]
		public void GetDemanded_Pass ()
		{
			object o = se.Demanded;
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlEvidence = true)]
		[ExpectedException (typeof (SecurityException))]
		public void GetDemanded_Fail_ControlEvidence ()
		{
			object o = se.Demanded;
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlPolicy = true)]
		[ExpectedException (typeof (SecurityException))]
		public void GetDemanded_Fail_ControlPolicy ()
		{
			object o = se.Demanded;
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void SetDemanded ()
		{
			se.Demanded = new PermissionSet (PermissionState.None);
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, ControlEvidence = true, ControlPolicy = true)]
		public void GetDenySetInstance_Pass ()
		{
			object o = se.DenySetInstance;
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlEvidence = true)]
		[ExpectedException (typeof (SecurityException))]
		public void GetDenySetInstance_Fail_ControlEvidence ()
		{
			object o = se.DenySetInstance;
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlPolicy = true)]
		[ExpectedException (typeof (SecurityException))]
		public void GetDenySetInstance_Fail_ControlPolicy ()
		{
			object o = se.DenySetInstance;
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void SetDenySetInstance ()
		{
			se.DenySetInstance = new PermissionSet (PermissionState.None);
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, ControlEvidence = true, ControlPolicy = true)]
		public void GetFailedAssemblyInfo_Pass ()
		{
			AssemblyName an = se.FailedAssemblyInfo;
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlEvidence = true)]
		[ExpectedException (typeof (SecurityException))]
		public void GetFailedAssemblyInfo_Fail_ControlEvidence ()
		{
			AssemblyName an = se.FailedAssemblyInfo;
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlPolicy = true)]
		[ExpectedException (typeof (SecurityException))]
		public void GetFailedAssemblyInfo_Fail_ControlPolicy ()
		{
			AssemblyName an = se.FailedAssemblyInfo;
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void SetFailedAssemblyInfo ()
		{
			se.FailedAssemblyInfo = null;
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, ControlEvidence = true, ControlPolicy = true)]
		public void GetFirstPermissionThatFailed_Pass ()
		{
			IPermission p = se.FirstPermissionThatFailed;
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlEvidence = true)]
		[ExpectedException (typeof (SecurityException))]
		public void GetFirstPermissionThatFailed_Fail_ControlEvidence ()
		{
			IPermission p = se.FirstPermissionThatFailed;
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlPolicy = true)]
		[ExpectedException (typeof (SecurityException))]
		public void GetFirstPermissionThatFailed_Fail_ControlPolicy ()
		{
			IPermission p = se.FirstPermissionThatFailed;
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void SetFirstPermissionThatFailed ()
		{
			se.FirstPermissionThatFailed = null;
		}
		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, ControlEvidence = true, ControlPolicy = true)]
		public void GetGrantedSet_Pass ()
		{
			string s = se.GrantedSet;
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlEvidence = true)]
		[ExpectedException (typeof (SecurityException))]
		public void GetGrantedSet_Fail_ControlEvidence ()
		{
			string s = se.GrantedSet;
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlPolicy = true)]
		[ExpectedException (typeof (SecurityException))]
		public void GetGrantedSet_Fail_ControlPolicy ()
		{
			string s = se.GrantedSet;
		}
		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void SetGrantedSet ()
		{
			se.GrantedSet = String.Empty;
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, ControlEvidence = true, ControlPolicy = true)]
		public void GetMethod_Pass ()
		{
			MethodInfo mi = se.Method;
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlEvidence = true)]
		[ExpectedException (typeof (SecurityException))]
		public void GetMethod_Fail_ControlEvidence ()
		{
			MethodInfo mi = se.Method;
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlPolicy = true)]
		[ExpectedException (typeof (SecurityException))]
		public void GetMethod_Fail_ControlPolicy ()
		{
			MethodInfo mi = se.Method;
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void SetMethod ()
		{
			se.Method = null;
		}
		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, ControlEvidence = true, ControlPolicy = true)]
		public void GetPermissionState_Pass ()
		{
			string s = se.PermissionState;
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlEvidence = true)]
		[ExpectedException (typeof (SecurityException))]
		public void GetPermissionState_Fail_ControlEvidence ()
		{
			string s = se.PermissionState;
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlPolicy = true)]
		[ExpectedException (typeof (SecurityException))]
		public void GetPermissionState_Fail_ControlPolicy ()
		{
			string s = se.PermissionState;
		}
		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void SetPermissionState ()
		{
			se.PermissionState = String.Empty;
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, ControlEvidence = true, ControlPolicy = true)]
		public void GetPermitOnlySetInstance_Pass ()
		{
			object s = se.PermitOnlySetInstance;
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlEvidence = true)]
		[ExpectedException (typeof (SecurityException))]
		public void GetPermitOnlySetInstance_Fail_ControlEvidence ()
		{
			object s = se.PermitOnlySetInstance;
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlPolicy = true)]
		[ExpectedException (typeof (SecurityException))]
		public void GetPermitOnlySetInstance_Fail_ControlPolicy ()
		{
			object s = se.PermitOnlySetInstance;
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void SetPermitOnlySetInstance ()
		{
			se.PermitOnlySetInstance = null;
		}
		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, ControlEvidence = true, ControlPolicy = true)]
		public void GetRefusedSet_Pass ()
		{
			string s = se.RefusedSet;
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlEvidence = true)]
		[ExpectedException (typeof (SecurityException))]
		public void GetRefusedSet_Fail_ControlEvidence ()
		{
			string s = se.RefusedSet;
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlPolicy = true)]
		[ExpectedException (typeof (SecurityException))]
		public void GetRefusedSet_Fail_ControlPolicy ()
		{
			string s = se.RefusedSet;
		}
		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void SetRefusedSet ()
		{
			se.RefusedSet = String.Empty;
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, ControlEvidence = true, ControlPolicy = true)]
		public void GetUrl_Pass ()
		{
			string s = se.Url;
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlEvidence = true)]
		[ExpectedException (typeof (SecurityException))]
		public void GetUrl_Fail_ControlEvidence ()
		{
			string s = se.Url;
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlPolicy = true)]
		[ExpectedException (typeof (SecurityException))]
		public void GetUrl_Fail_ControlPolicy ()
		{
			string s = se.Url;
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void SetUrl ()
		{
			se.Url = "http://www.example.com/";
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void GetZone ()
		{
			SecurityZone sz = se.Zone;
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void SetZone ()
		{
			se.Zone = SecurityZone.Untrusted;
		}
		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void ToString_Empty ()
		{
			string s = se.ToString ();
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void ToString_WithSuppliedSensitiveInformation1 ()
		{
			// here we supply something *sensitive* as the state
			string sensitive = "*SENSITIVE*";
			SecurityException se = new SecurityException ("message", typeof (Object), sensitive);
			// and we don't expect it to be shown in the output
			Assert.IsTrue (se.ToString ().IndexOf (sensitive) == -1, sensitive);
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, ControlEvidence = true, ControlPolicy = true)]
		public void ToString_WithSuppliedSensitiveInformation2 ()
		{
			// here we supply something *sensitive* as the state
			string sensitive = "*SENSITIVE*";
			SecurityException se = new SecurityException ("message", typeof (Object), sensitive);
			// and we EXPECT it to be shown in the output 
			// as we pass the security checks for PermissionState property
			Assert.IsFalse (se.ToString ().IndexOf (sensitive) == -1, sensitive);
		}

		[Test]
		public void GetObjectData ()
		{
			SecurityException se = new SecurityException ("message", typeof (string), "state");
			SerializationInfo info = new SerializationInfo (typeof (SecurityException), new FormatterConverter ());
			se.GetObjectData (info, new StreamingContext (StreamingContextStates.All));
			Assert.AreEqual ("state", info.GetValue ("PermissionState", typeof (string)), "PermissionState");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SerializationException))]
		public void GetObjectData_Deny_Unrestricted ()
		{
			SecurityException se = new SecurityException ("message", typeof (string), "state");
			SerializationInfo info = new SerializationInfo (typeof (SecurityException), new FormatterConverter ());
			se.GetObjectData (info, new StreamingContext (StreamingContextStates.All));
			// "PermissionState" hasn't been serialized because it's access was restricted
			info.GetValue ("PermissionState", typeof (string));
		}
	}
}
