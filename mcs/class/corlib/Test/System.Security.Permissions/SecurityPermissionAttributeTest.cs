//
// SecurityPermissionAttributeTest.cs - NUnit Test Cases for SecurityPermissionAttribute
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using System;
using System.Security;
using System.Security.Permissions;

namespace MonoTests.System.Security.Permissions {

	[TestFixture]
	public class SecurityPermissionAttributeTest : Assertion {

		[Test]
		public void Default () 
		{
			SecurityPermissionAttribute a = new SecurityPermissionAttribute (SecurityAction.Assert);
			Assert ("Assertion", !a.Assertion);
			Assert ("ControlAppDomain", !a.ControlAppDomain);
			Assert ("ControlDomainPolicy", !a.ControlDomainPolicy);
			Assert ("ControlEvidence", !a.ControlEvidence);
			Assert ("ControlPolicy", !a.ControlPolicy);
			Assert ("ControlPrincipal", !a.ControlPrincipal);
			Assert ("ControlThread", !a.ControlThread);
			Assert ("Execution", !a.Execution);
			Assert ("Infrastructure", !a.Infrastructure);
			Assert ("RemotingConfiguration", !a.RemotingConfiguration);
			Assert ("SerializationFormatter", !a.SerializationFormatter);
			Assert ("SkipVerification", !a.SkipVerification);
			Assert ("UnmanagedCode", !a.UnmanagedCode);
			
			AssertEquals ("Flags", SecurityPermissionFlag.NoFlags, a.Flags);
			AssertEquals ("TypeId", a.ToString (), a.TypeId.ToString ());
			Assert ("Unrestricted", !a.Unrestricted);

			SecurityPermission perm = (SecurityPermission) a.CreatePermission ();
			AssertEquals ("CreatePermission.Flags", SecurityPermissionFlag.NoFlags, perm.Flags);
		}

		[Test]
		public void Action () 
		{
			SecurityPermissionAttribute a = new SecurityPermissionAttribute (SecurityAction.Assert);
			AssertEquals ("Action=Assert", SecurityAction.Assert, a.Action);
			a.Action = SecurityAction.Demand;
			AssertEquals ("Action=Demand", SecurityAction.Demand, a.Action);
			a.Action = SecurityAction.Deny;
			AssertEquals ("Action=Deny", SecurityAction.Deny, a.Action);
			a.Action = SecurityAction.InheritanceDemand;
			AssertEquals ("Action=InheritanceDemand", SecurityAction.InheritanceDemand, a.Action);
			a.Action = SecurityAction.LinkDemand;
			AssertEquals ("Action=LinkDemand", SecurityAction.LinkDemand, a.Action);
			a.Action = SecurityAction.PermitOnly;
			AssertEquals ("Action=PermitOnly", SecurityAction.PermitOnly, a.Action);
			a.Action = SecurityAction.RequestMinimum;
			AssertEquals ("Action=RequestMinimum", SecurityAction.RequestMinimum, a.Action);
			a.Action = SecurityAction.RequestOptional;
			AssertEquals ("Action=RequestOptional", SecurityAction.RequestOptional, a.Action);
			a.Action = SecurityAction.RequestRefuse;
			AssertEquals ("Action=RequestRefuse", SecurityAction.RequestRefuse, a.Action);
		}

		private SecurityPermissionAttribute Empty () 
		{
			SecurityPermissionAttribute a = new SecurityPermissionAttribute (SecurityAction.Assert);
			a.Assertion = false;
			a.ControlAppDomain = false;
			a.ControlDomainPolicy = false;
			a.ControlEvidence = false;
			a.ControlPolicy = false;
			a.ControlPrincipal = false;
			a.ControlThread = false;
			a.Execution = false;
			a.Infrastructure = false;
			a.RemotingConfiguration = false;
			a.SerializationFormatter = false;
			a.SkipVerification = false;
			a.UnmanagedCode = false;
			AssertEquals ("Flags", SecurityPermissionFlag.NoFlags, a.Flags);
			return a;
		}

		[Test]
		public void Assertion () 
		{
			SecurityPermissionAttribute a = Empty ();
			a.Assertion = true;
			AssertEquals ("Flags=Assertion", SecurityPermissionFlag.Assertion, a.Flags);
		}

		[Test]
		public void ControlAppDomain () 
		{
			SecurityPermissionAttribute a = Empty ();
			a.ControlAppDomain = true;
			AssertEquals ("Flags=ControlAppDomain", SecurityPermissionFlag.ControlAppDomain, a.Flags);
		}

		[Test]
		public void ControlDomainPolicy () 
		{
			SecurityPermissionAttribute a = Empty ();
			a.ControlDomainPolicy = true;
			AssertEquals ("Flags=ControlDomainPolicy", SecurityPermissionFlag.ControlDomainPolicy, a.Flags);
		}

		[Test]
		public void ControlEvidence () 
		{
			SecurityPermissionAttribute a = Empty ();
			a.ControlEvidence = true;
			AssertEquals ("Flags=ControlEvidence", SecurityPermissionFlag.ControlEvidence, a.Flags);
		}

		[Test]
		public void ControlPolicy () 
		{
			SecurityPermissionAttribute a = Empty ();
			a.ControlPolicy = true;
			AssertEquals ("Flags=ControlPolicy", SecurityPermissionFlag.ControlPolicy, a.Flags);
		}

		[Test]
		public void ControlPrincipal () 
		{
			SecurityPermissionAttribute a = Empty ();
			a.ControlPrincipal = true;
			AssertEquals ("Flags=ControlPrincipal", SecurityPermissionFlag.ControlPrincipal, a.Flags);
		}

		[Test]
		public void ControlThread ()
		{
			SecurityPermissionAttribute a = Empty ();
			a.ControlThread = true;
			AssertEquals ("Flags=ControlThread", SecurityPermissionFlag.ControlThread, a.Flags);
		}

		[Test]
		public void Execution () 
		{
			SecurityPermissionAttribute a = Empty ();
			a.Execution = true;
			AssertEquals ("Flags=Execution", SecurityPermissionFlag.Execution, a.Flags);
		}

		[Test]
		public void Infrastructure () 
		{
			SecurityPermissionAttribute a = Empty ();
			a.Infrastructure = true;
			AssertEquals ("Flags=Infrastructure", SecurityPermissionFlag.Infrastructure, a.Flags);
		}

		[Test]
		public void RemotingConfiguration () 
		{
			SecurityPermissionAttribute a = Empty ();
			a.RemotingConfiguration = true;
			AssertEquals ("Flags=RemotingConfiguration", SecurityPermissionFlag.RemotingConfiguration, a.Flags);
		}

		[Test]
		public void SerializationFormatter () 
		{
			SecurityPermissionAttribute a = Empty ();
			a.SerializationFormatter = true;
			AssertEquals ("Flags=SerializationFormatter", SecurityPermissionFlag.SerializationFormatter, a.Flags);
		}

		[Test]
		public void SkipVerification () 
		{
			SecurityPermissionAttribute a = Empty ();
			a.SkipVerification = true;
			AssertEquals ("Flags=SkipVerification", SecurityPermissionFlag.SkipVerification, a.Flags);
		}

		[Test]
		public void UnmanagedCode () 
		{
			SecurityPermissionAttribute a = Empty ();
			a.UnmanagedCode = true;
			AssertEquals ("Flags=UnmanagedCode", SecurityPermissionFlag.UnmanagedCode, a.Flags);
		}

		[Test]
		public void Unrestricted () 
		{
			SecurityPermissionAttribute a = new SecurityPermissionAttribute (SecurityAction.Assert);
			a.Unrestricted = true;
			AssertEquals ("Unrestricted", SecurityPermissionFlag.NoFlags, a.Flags);

			SecurityPermission perm = (SecurityPermission) a.CreatePermission ();
			AssertEquals ("CreatePermission.Flags", SecurityPermissionFlag.AllFlags, perm.Flags);
		}
	}
}
