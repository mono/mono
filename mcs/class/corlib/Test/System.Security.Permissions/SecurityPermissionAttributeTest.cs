//
// SecurityPermissionAttributeTest.cs -
//	NUnit Test Cases for SecurityPermissionAttribute
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

namespace MonoTests.System.Security.Permissions {

	[TestFixture]
#if MOBILE
	[Ignore]
#endif
	public class SecurityPermissionAttributeTest {

		[Test]
		public void Default () 
		{
			SecurityPermissionAttribute a = new SecurityPermissionAttribute (SecurityAction.Assert);
			Assert.IsFalse (a.Assertion, "Assertion");
#if NET_2_0
			Assert.IsFalse (a.BindingRedirects, "BindingRedirects");
#endif
			Assert.IsFalse (a.ControlAppDomain, "ControlAppDomain");
			Assert.IsFalse (a.ControlDomainPolicy, "ControlDomainPolicy");
			Assert.IsFalse (a.ControlEvidence, "ControlEvidence");
			Assert.IsFalse (a.ControlPolicy, "ControlPolicy");
			Assert.IsFalse (a.ControlPrincipal, "ControlPrincipal");
			Assert.IsFalse (a.ControlThread, "ControlThread");
			Assert.IsFalse (a.Execution, "Execution");
			Assert.IsFalse (a.Infrastructure, "Infrastructure");
			Assert.IsFalse (a.RemotingConfiguration, "RemotingConfiguration");
			Assert.IsFalse (a.SerializationFormatter, "SerializationFormatter");
			Assert.IsFalse (a.SkipVerification, "SkipVerification");
			Assert.IsFalse (a.UnmanagedCode, "UnmanagedCode");

			Assert.AreEqual (SecurityPermissionFlag.NoFlags, a.Flags, "Flags");
			Assert.AreEqual (a.ToString (), a.TypeId.ToString (), "TypeId");
			Assert.IsFalse (a.Unrestricted, "Unrestricted");

			SecurityPermission perm = (SecurityPermission) a.CreatePermission ();
			Assert.AreEqual (SecurityPermissionFlag.NoFlags, perm.Flags, "CreatePermission.Flags");
		}

		[Test]
		public void Action () 
		{
			SecurityPermissionAttribute a = new SecurityPermissionAttribute (SecurityAction.Assert);
			Assert.AreEqual (SecurityAction.Assert, a.Action, "Action=Assert");
			a.Action = SecurityAction.Demand;
			Assert.AreEqual (SecurityAction.Demand, a.Action, "Action=Demand");
			a.Action = SecurityAction.Deny;
			Assert.AreEqual (SecurityAction.Deny, a.Action, "Action=Deny");
			a.Action = SecurityAction.InheritanceDemand;
			Assert.AreEqual (SecurityAction.InheritanceDemand, a.Action, "Action=InheritanceDemand");
			a.Action = SecurityAction.LinkDemand;
			Assert.AreEqual (SecurityAction.LinkDemand, a.Action, "Action=LinkDemand");
			a.Action = SecurityAction.PermitOnly;
			Assert.AreEqual (SecurityAction.PermitOnly, a.Action, "Action=PermitOnly");
			a.Action = SecurityAction.RequestMinimum;
			Assert.AreEqual (SecurityAction.RequestMinimum, a.Action, "Action=RequestMinimum");
			a.Action = SecurityAction.RequestOptional;
			Assert.AreEqual (SecurityAction.RequestOptional, a.Action, "Action=RequestOptional");
			a.Action = SecurityAction.RequestRefuse;
			Assert.AreEqual (SecurityAction.RequestRefuse, a.Action, "Action=RequestRefuse");
		}

		[Test]
		public void Action_Invalid ()
		{
			SecurityPermissionAttribute a = new SecurityPermissionAttribute ((SecurityAction)Int32.MinValue);
			// no validation in attribute
		}

		private SecurityPermissionAttribute Empty () 
		{
			SecurityPermissionAttribute a = new SecurityPermissionAttribute (SecurityAction.Assert);
			a.Assertion = false;
#if NET_2_0
			a.BindingRedirects = false;
#endif
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
			Assert.AreEqual (SecurityPermissionFlag.NoFlags, a.Flags, "Flags");
			return a;
		}

		[Test]
		public void Assertion () 
		{
			SecurityPermissionAttribute a = Empty ();
			a.Assertion = true;
			Assert.AreEqual (SecurityPermissionFlag.Assertion, a.Flags, "Flags=Assertion");
			a.Assertion = false;
			Assert.AreEqual (SecurityPermissionFlag.NoFlags, a.Flags, "Flags=NoFlags");
		}
#if NET_2_0
		[Test]
		public void BindingRedirects ()
		{
			SecurityPermissionAttribute a = Empty ();
			a.BindingRedirects = true;
			Assert.AreEqual (SecurityPermissionFlag.BindingRedirects, a.Flags, "Flags=BindingRedirects");
			a.BindingRedirects = false;
			Assert.AreEqual (SecurityPermissionFlag.NoFlags, a.Flags, "Flags=NoFlags");
		}
#endif
		[Test]
		public void ControlAppDomain () 
		{
			SecurityPermissionAttribute a = Empty ();
			a.ControlAppDomain = true;
			Assert.AreEqual (SecurityPermissionFlag.ControlAppDomain, a.Flags, "Flags=ControlAppDomain");
			a.ControlAppDomain = false;
			Assert.AreEqual (SecurityPermissionFlag.NoFlags, a.Flags, "Flags=NoFlags");
		}

		[Test]
		public void ControlDomainPolicy () 
		{
			SecurityPermissionAttribute a = Empty ();
			a.ControlDomainPolicy = true;
			Assert.AreEqual (SecurityPermissionFlag.ControlDomainPolicy, a.Flags, "Flags=ControlDomainPolicy");
			a.ControlDomainPolicy = false;
			Assert.AreEqual (SecurityPermissionFlag.NoFlags, a.Flags, "Flags=NoFlags");
		}

		[Test]
		public void ControlEvidence () 
		{
			SecurityPermissionAttribute a = Empty ();
			a.ControlEvidence = true;
			Assert.AreEqual (SecurityPermissionFlag.ControlEvidence, a.Flags, "Flags=ControlEvidence");
			a.ControlEvidence = false;
			Assert.AreEqual (SecurityPermissionFlag.NoFlags, a.Flags, "Flags=NoFlags");
		}

		[Test]
		public void ControlPolicy () 
		{
			SecurityPermissionAttribute a = Empty ();
			a.ControlPolicy = true;
			Assert.AreEqual (SecurityPermissionFlag.ControlPolicy, a.Flags, "Flags=ControlPolicy");
			a.ControlPolicy = false;
			Assert.AreEqual (SecurityPermissionFlag.NoFlags, a.Flags, "Flags=NoFlags");
		}

		[Test]
		public void ControlPrincipal () 
		{
			SecurityPermissionAttribute a = Empty ();
			a.ControlPrincipal = true;
			Assert.AreEqual (SecurityPermissionFlag.ControlPrincipal, a.Flags, "Flags=ControlPrincipal");
			a.ControlPrincipal = false;
			Assert.AreEqual (SecurityPermissionFlag.NoFlags, a.Flags, "Flags=NoFlags");
		}

		[Test]
		public void ControlThread ()
		{
			SecurityPermissionAttribute a = Empty ();
			a.ControlThread = true;
			Assert.AreEqual (SecurityPermissionFlag.ControlThread, a.Flags, "Flags=ControlThread");
			a.ControlThread = false;
			Assert.AreEqual (SecurityPermissionFlag.NoFlags, a.Flags, "Flags=NoFlags");
		}

		[Test]
		public void Execution () 
		{
			SecurityPermissionAttribute a = Empty ();
			a.Execution = true;
			Assert.AreEqual (SecurityPermissionFlag.Execution, a.Flags, "Flags=Execution");
			a.Execution = false;
			Assert.AreEqual (SecurityPermissionFlag.NoFlags, a.Flags, "Flags=NoFlags");
		}

		[Test]
		public void Infrastructure () 
		{
			SecurityPermissionAttribute a = Empty ();
			a.Infrastructure = true;
			Assert.AreEqual (SecurityPermissionFlag.Infrastructure, a.Flags, "Flags=Infrastructure");
			a.Infrastructure = false;
			Assert.AreEqual (SecurityPermissionFlag.NoFlags, a.Flags, "Flags=NoFlags");
		}

		[Test]
		public void RemotingConfiguration () 
		{
			SecurityPermissionAttribute a = Empty ();
			a.RemotingConfiguration = true;
			Assert.AreEqual (SecurityPermissionFlag.RemotingConfiguration, a.Flags, "Flags=RemotingConfiguration");
			a.RemotingConfiguration = false;
			Assert.AreEqual (SecurityPermissionFlag.NoFlags, a.Flags, "Flags=NoFlags");
		}

		[Test]
		public void SerializationFormatter () 
		{
			SecurityPermissionAttribute a = Empty ();
			a.SerializationFormatter = true;
			Assert.AreEqual (SecurityPermissionFlag.SerializationFormatter, a.Flags, "Flags=SerializationFormatter");
			a.SerializationFormatter = false;
			Assert.AreEqual (SecurityPermissionFlag.NoFlags, a.Flags, "Flags=NoFlags");
		}

		[Test]
		public void SkipVerification () 
		{
			SecurityPermissionAttribute a = Empty ();
			a.SkipVerification = true;
			Assert.AreEqual (SecurityPermissionFlag.SkipVerification, a.Flags, "Flags=SkipVerification");
			a.SkipVerification = false;
			Assert.AreEqual (SecurityPermissionFlag.NoFlags, a.Flags, "Flags=NoFlags");
		}

		[Test]
		public void UnmanagedCode () 
		{
			SecurityPermissionAttribute a = Empty ();
			a.UnmanagedCode = true;
			Assert.AreEqual (SecurityPermissionFlag.UnmanagedCode, a.Flags, "Flags=UnmanagedCode");
			a.UnmanagedCode = false;
			Assert.AreEqual (SecurityPermissionFlag.NoFlags, a.Flags, "Flags=NoFlags");
		}

		[Test]
		public void Unrestricted () 
		{
			SecurityPermissionAttribute a = new SecurityPermissionAttribute (SecurityAction.Assert);
			a.Unrestricted = true;
			Assert.AreEqual (SecurityPermissionFlag.NoFlags, a.Flags, "Unrestricted");

			SecurityPermission perm = (SecurityPermission) a.CreatePermission ();
			Assert.AreEqual (SecurityPermissionFlag.AllFlags, perm.Flags, "CreatePermission.Flags");
		}

		[Test]
		public void Flags ()
		{
			SecurityPermissionAttribute a = new SecurityPermissionAttribute (SecurityAction.Assert);
			a.Flags = SecurityPermissionFlag.Assertion;
			Assert.IsTrue (a.Assertion, "Assertion");
#if NET_2_0
			a.Flags |= SecurityPermissionFlag.BindingRedirects;
			Assert.IsTrue (a.BindingRedirects, "BindingRedirects");
#endif
			a.Flags |= SecurityPermissionFlag.ControlAppDomain;
			Assert.IsTrue (a.ControlAppDomain, "ControlAppDomain");
			a.Flags |= SecurityPermissionFlag.ControlDomainPolicy;
			Assert.IsTrue (a.ControlDomainPolicy, "ControlDomainPolicy");
			a.Flags |= SecurityPermissionFlag.ControlEvidence;
			Assert.IsTrue (a.ControlEvidence, "ControlEvidence");
			a.Flags |= SecurityPermissionFlag.ControlPolicy;
			Assert.IsTrue (a.ControlPolicy, "ControlPolicy");
			a.Flags |= SecurityPermissionFlag.ControlPrincipal;
			Assert.IsTrue (a.ControlPrincipal, "ControlPrincipal");
			a.Flags |= SecurityPermissionFlag.ControlThread;
			Assert.IsTrue (a.ControlThread, "ControlThread");
			a.Flags |= SecurityPermissionFlag.Execution;
			Assert.IsTrue (a.Execution, "Execution");
			a.Flags |= SecurityPermissionFlag.Infrastructure;
			Assert.IsTrue (a.Infrastructure, "Infrastructure");
			a.Flags |= SecurityPermissionFlag.RemotingConfiguration;
			Assert.IsTrue (a.RemotingConfiguration, "RemotingConfiguration");
			a.Flags |= SecurityPermissionFlag.SerializationFormatter;
			Assert.IsTrue (a.SerializationFormatter, "SerializationFormatter");
			a.Flags |= SecurityPermissionFlag.SkipVerification;
			Assert.IsTrue (a.SkipVerification, "SkipVerification");
			a.Flags |= SecurityPermissionFlag.UnmanagedCode;

			Assert.IsTrue (a.UnmanagedCode, "UnmanagedCode");
			Assert.IsFalse (a.Unrestricted, "Unrestricted");

			a.Flags &= ~SecurityPermissionFlag.Assertion;
			Assert.IsFalse (a.Assertion, "Assertion-False");
#if NET_2_0
			a.Flags &= ~SecurityPermissionFlag.BindingRedirects;
			Assert.IsFalse (a.BindingRedirects, "BindingRedirects-False");
#endif
			a.Flags &= ~SecurityPermissionFlag.ControlAppDomain;
			Assert.IsFalse (a.ControlAppDomain, "ControlAppDomain-False");
			a.Flags &= ~SecurityPermissionFlag.ControlDomainPolicy;
			Assert.IsFalse (a.ControlDomainPolicy, "ControlDomainPolicy-False");
			a.Flags &= ~SecurityPermissionFlag.ControlEvidence;
			Assert.IsFalse (a.ControlEvidence, "ControlEvidence-False");
			a.Flags &= ~SecurityPermissionFlag.ControlPolicy;
			Assert.IsFalse (a.ControlPolicy, "ControlPolicy-False");
			a.Flags &= ~SecurityPermissionFlag.ControlPrincipal;
			Assert.IsFalse (a.ControlPrincipal, "ControlPrincipal-False");
			a.Flags &= ~SecurityPermissionFlag.ControlThread;
			Assert.IsFalse (a.ControlThread, "ControlThread-False");
			a.Flags &= ~SecurityPermissionFlag.Execution;
			Assert.IsFalse (a.Execution, "Execution-False");
			a.Flags &= ~SecurityPermissionFlag.Infrastructure;
			Assert.IsFalse (a.Infrastructure, "Infrastructure-False");
			a.Flags &= ~SecurityPermissionFlag.RemotingConfiguration;
			Assert.IsFalse (a.RemotingConfiguration, "RemotingConfiguration-False");
			a.Flags &= ~SecurityPermissionFlag.SerializationFormatter;
			Assert.IsFalse (a.SerializationFormatter, "SerializationFormatter-False");
			a.Flags &= ~SecurityPermissionFlag.SkipVerification;
			Assert.IsFalse (a.SkipVerification, "SkipVerification-False");
			a.Flags &= ~SecurityPermissionFlag.UnmanagedCode;
		}

		[Test]
		public void Attributes ()
		{
			Type t = typeof (SecurityPermissionAttribute);
			Assert.IsTrue (t.IsSerializable, "IsSerializable");

			object[] attrs = t.GetCustomAttributes (typeof (AttributeUsageAttribute), false);
			Assert.AreEqual (1, attrs.Length, "AttributeUsage");
			AttributeUsageAttribute aua = (AttributeUsageAttribute)attrs [0];
			Assert.IsTrue (aua.AllowMultiple, "AllowMultiple");
			Assert.IsFalse (aua.Inherited, "Inherited");
			AttributeTargets at = (AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method);
			Assert.AreEqual (at, aua.ValidOn, "ValidOn");
		}
	}
}
