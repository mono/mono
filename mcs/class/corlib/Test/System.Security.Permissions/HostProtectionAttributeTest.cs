//
// HostProtectionAttributeTest.cs - NUnit Test Cases for HostProtectionAttribute
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
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

#if NET_2_0

using NUnit.Framework;
using System;
using System.Security;
using System.Security.Permissions;

namespace MonoTests.System.Security.Permissions {

	[TestFixture]
#if MOBILE
	[Ignore]
#endif
	public class HostProtectionAttributeTest {

		private void DefaultTests (HostProtectionAttribute hpa)
		{
			Assert.AreEqual (SecurityAction.LinkDemand, hpa.Action, "Action");
			Assert.AreEqual (HostProtectionResource.None, hpa.Resources, "Resources");
			Assert.IsFalse (hpa.ExternalProcessMgmt, "ExternalProcessMgmt");
			Assert.IsFalse (hpa.ExternalThreading, "ExternalThreading");
			Assert.IsFalse (hpa.MayLeakOnAbort, "MayLeakOnAbort");
			Assert.IsFalse (hpa.SecurityInfrastructure, "SecurityInfrastructure");
			Assert.IsFalse (hpa.SelfAffectingProcessMgmt, "SelfAffectingProcessMgmt");
			Assert.IsFalse (hpa.SelfAffectingThreading, "SelfAffectingThreading");
			Assert.IsFalse (hpa.SharedState, "SharedState");
			Assert.IsFalse (hpa.Synchronization, "Synchronization");
			Assert.IsFalse (hpa.UI, "UI");
			Assert.IsFalse (hpa.Unrestricted, "Unrestricted");
			IPermission p = hpa.CreatePermission ();
			Assert.AreEqual ("System.Security.Permissions.HostProtectionPermission", p.GetType ().ToString (), "CreatePermission");
			Assert.IsTrue ((p is IUnrestrictedPermission), "IUnrestrictedPermission");
		}

		[Test]
#if MOBILE
		[Ignore]
#endif
		public void HostProtectionAttribute_Empty ()
		{
			// note: normally security attributes don't have an empty constructor
			HostProtectionAttribute hpa = new HostProtectionAttribute ();
			DefaultTests (hpa);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void HostProtectionAttribute_Assert ()
		{
			new HostProtectionAttribute (SecurityAction.Assert);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void HostProtectionAttribute_Demand ()
		{
			new HostProtectionAttribute (SecurityAction.Demand);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void HostProtectionAttribute_Deny ()
		{
			new HostProtectionAttribute (SecurityAction.Deny);
		}

		[Test]
#if MOBILE
		[Ignore]
#endif
		[ExpectedException (typeof (ArgumentException))]
		public void HostProtectionAttribute_InheritanceDemand ()
		{
			new HostProtectionAttribute (SecurityAction.InheritanceDemand);
		}

		[Test]
		public void HostProtectionAttribute_LinkDemand ()
		{
			HostProtectionAttribute hpa = new HostProtectionAttribute (SecurityAction.LinkDemand);
			DefaultTests (hpa);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void HostProtectionAttribute_PermitOnly ()
		{
			new HostProtectionAttribute (SecurityAction.PermitOnly);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void HostProtectionAttribute_RequestMinimum ()
		{
			new HostProtectionAttribute (SecurityAction.RequestMinimum);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void HostProtectionAttribute_RequestOptional ()
		{
			new HostProtectionAttribute (SecurityAction.RequestOptional);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void HostProtectionAttribute_RequestRefuse ()
		{
			new HostProtectionAttribute (SecurityAction.RequestRefuse);
		}

		private HostProtectionAttribute Empty () 
		{
			HostProtectionAttribute a = new HostProtectionAttribute ();
			a.Synchronization = false;
			a.SharedState = false;
			a.ExternalProcessMgmt = false;
			a.SelfAffectingProcessMgmt = false;
			a.ExternalThreading = false;
			a.SelfAffectingThreading = false;
			a.SecurityInfrastructure = false;
			a.UI = false;
			a.MayLeakOnAbort = false;
			Assert.AreEqual (HostProtectionResource.None, a.Resources, "Resources=None");
			return a;
		}

		[Test]
		public void Synchronization () 
		{
			HostProtectionAttribute a = Empty ();
			a.Synchronization = true;
			Assert.AreEqual (HostProtectionResource.Synchronization, a.Resources, "Resources=Synchronization");
			a.Synchronization = false;
			Assert.AreEqual (HostProtectionResource.None, a.Resources, "Resources=None");
		}

		[Test]
		public void SharedState () 
		{
			HostProtectionAttribute a = Empty ();
			a.SharedState = true;
			Assert.AreEqual (HostProtectionResource.SharedState, a.Resources, "Resources=SharedState");
			a.SharedState = false;
			Assert.AreEqual (HostProtectionResource.None, a.Resources, "Resources=None");
		}

		[Test]
		public void ExternalProcessMgmt () 
		{
			HostProtectionAttribute a = Empty ();
			a.ExternalProcessMgmt = true;
			Assert.AreEqual (HostProtectionResource.ExternalProcessMgmt, a.Resources, "Resources=ExternalProcessMgmt");
			a.ExternalProcessMgmt = false;
			Assert.AreEqual (HostProtectionResource.None, a.Resources, "Resources=None");
		}

		[Test]
		public void SelfAffectingProcessMgmt () 
		{
			HostProtectionAttribute a = Empty ();
			a.SelfAffectingProcessMgmt = true;
			Assert.AreEqual (HostProtectionResource.SelfAffectingProcessMgmt, a.Resources, "Resources=SelfAffectingProcessMgmt");
			a.SelfAffectingProcessMgmt = false;
			Assert.AreEqual (HostProtectionResource.None, a.Resources, "Resources=None");
		}

		[Test]
		public void ExternalThreading () 
		{
			HostProtectionAttribute a = Empty ();
			a.ExternalThreading = true;
			Assert.AreEqual (HostProtectionResource.ExternalThreading, a.Resources, "Resources=ExternalThreading");
			a.ExternalThreading = false;
			Assert.AreEqual (HostProtectionResource.None, a.Resources, "Resources=None");
		}

		[Test]
		public void SelfAffectingThreading () 
		{
			HostProtectionAttribute a = Empty ();
			a.SelfAffectingThreading = true;
			Assert.AreEqual (HostProtectionResource.SelfAffectingThreading, a.Resources, "Resources=SelfAffectingThreading");
			a.SelfAffectingThreading = false;
			Assert.AreEqual (HostProtectionResource.None, a.Resources, "Resources=None");
		}

		[Test]
		public void SecurityInfrastructure () 
		{
			HostProtectionAttribute a = Empty ();
			a.SecurityInfrastructure = true;
			Assert.AreEqual (HostProtectionResource.SecurityInfrastructure, a.Resources, "Resources=SecurityInfrastructure");
			a.SecurityInfrastructure = false;
			Assert.AreEqual (HostProtectionResource.None, a.Resources, "Resources=None");
		}

		[Test]
		public void UI () 
		{
			HostProtectionAttribute a = Empty ();
			a.UI = true;
			Assert.AreEqual (HostProtectionResource.UI, a.Resources, "Resources=UI");
			a.UI = false;
			Assert.AreEqual (HostProtectionResource.None, a.Resources, "Resources=None");
		}

		[Test]
		public void MayLeakOnAbort () 
		{
			HostProtectionAttribute a = Empty ();
			a.MayLeakOnAbort = true;
			Assert.AreEqual (HostProtectionResource.MayLeakOnAbort, a.Resources, "Resources=MayLeakOnAbort");
			a.MayLeakOnAbort = false;
			Assert.AreEqual (HostProtectionResource.None, a.Resources, "Resources=None");
		}

		[Test]
		public void Properties () 
		{
			HostProtectionAttribute hpa = new HostProtectionAttribute (SecurityAction.LinkDemand);
			HostProtectionResource expected = HostProtectionResource.None;
			Assert.AreEqual (expected, hpa.Resources, "None");
			Assert.IsFalse (hpa.Unrestricted, "Unrestricted-1");

			hpa.ExternalProcessMgmt = true;
			expected |= HostProtectionResource.ExternalProcessMgmt;
			Assert.AreEqual (expected, hpa.Resources, "+ExternalProcessMgmt");
			Assert.IsFalse (hpa.Unrestricted, "Unrestricted-2");

			hpa.ExternalThreading = true;
			expected |= HostProtectionResource.ExternalThreading;
			Assert.AreEqual (expected, hpa.Resources, "+ExternalThreading");
			Assert.IsFalse (hpa.Unrestricted, "Unrestricted-3");

			hpa.MayLeakOnAbort = true;
			expected |= HostProtectionResource.MayLeakOnAbort;
			Assert.AreEqual (expected, hpa.Resources, "+MayLeakOnAbort");
			Assert.IsFalse (hpa.Unrestricted, "Unrestricted-4");

			hpa.SecurityInfrastructure = true;
			expected |= HostProtectionResource.SecurityInfrastructure;
			Assert.AreEqual (expected, hpa.Resources, "+SecurityInfrastructure");
			Assert.IsFalse (hpa.Unrestricted, "Unrestricted-5");

			hpa.SelfAffectingProcessMgmt = true;
			expected |= HostProtectionResource.SelfAffectingProcessMgmt;
			Assert.AreEqual (expected, hpa.Resources, "+SelfAffectingProcessMgmt");
			Assert.IsFalse (hpa.Unrestricted, "Unrestricted-6");

			hpa.SelfAffectingThreading = true;
			expected |= HostProtectionResource.SelfAffectingThreading;
			Assert.AreEqual (expected, hpa.Resources, "+SelfAffectingThreading");
			Assert.IsFalse (hpa.Unrestricted, "Unrestricted-7");

			hpa.SharedState = true;
			expected |= HostProtectionResource.SharedState;
			Assert.AreEqual (expected, hpa.Resources, "+SharedState");
			Assert.IsFalse (hpa.Unrestricted, "Unrestricted-8");

			hpa.Synchronization = true;
			expected |= HostProtectionResource.Synchronization;
			Assert.AreEqual (expected, hpa.Resources, "+Synchronization");
			Assert.IsFalse (hpa.Unrestricted, "Unrestricted-9");

			hpa.UI = true;
			expected |= HostProtectionResource.UI;
			Assert.AreEqual (expected, hpa.Resources, "+UI");

			Assert.IsFalse (hpa.Unrestricted, "Unrestricted");

			hpa.ExternalProcessMgmt = false;
			expected &= ~HostProtectionResource.ExternalProcessMgmt;
			Assert.AreEqual (expected, hpa.Resources, "-ExternalProcessMgmt");
			Assert.IsFalse (hpa.Unrestricted, "Unrestricted-10");

			hpa.ExternalThreading = false;
			expected &= ~HostProtectionResource.ExternalThreading;
			Assert.AreEqual (expected, hpa.Resources, "+ExternalThreading");
			Assert.IsFalse (hpa.Unrestricted, "Unrestricted-11");

			hpa.MayLeakOnAbort = false;
			expected &= ~HostProtectionResource.MayLeakOnAbort;
			Assert.AreEqual (expected, hpa.Resources, "+MayLeakOnAbort");
			Assert.IsFalse (hpa.Unrestricted, "Unrestricted-12");

			hpa.SecurityInfrastructure = false;
			expected &= ~HostProtectionResource.SecurityInfrastructure;
			Assert.AreEqual (expected, hpa.Resources, "+SecurityInfrastructure");
			Assert.IsFalse (hpa.Unrestricted, "Unrestricted-13");

			hpa.SelfAffectingProcessMgmt = false;
			expected &= ~HostProtectionResource.SelfAffectingProcessMgmt;
			Assert.AreEqual (expected, hpa.Resources, "+SelfAffectingProcessMgmt");
			Assert.IsFalse (hpa.Unrestricted, "Unrestricted-14");

			hpa.SelfAffectingThreading = false;
			expected &= ~HostProtectionResource.SelfAffectingThreading;
			Assert.AreEqual (expected, hpa.Resources, "+SelfAffectingThreading");
			Assert.IsFalse (hpa.Unrestricted, "Unrestricted-15");

			hpa.SharedState = false;
			expected &= ~HostProtectionResource.SharedState;
			Assert.AreEqual (expected, hpa.Resources, "+SharedState");
			Assert.IsFalse (hpa.Unrestricted, "Unrestricted-16");

			hpa.Synchronization = false;
			expected &= ~HostProtectionResource.Synchronization;
			Assert.AreEqual (expected, hpa.Resources, "+Synchronization");
			Assert.IsFalse (hpa.Unrestricted, "Unrestricted-17");

			hpa.UI = false;
			expected &= ~HostProtectionResource.UI;
			Assert.AreEqual (expected, hpa.Resources, "+UI");
			Assert.IsFalse (hpa.Unrestricted, "Unrestricted-18");
		}

		[Test]
		public void Attributes ()
		{
			Type t = typeof (HostProtectionAttribute);
			Assert.IsTrue (t.IsSerializable, "IsSerializable");

			object [] attrs = t.GetCustomAttributes (typeof (AttributeUsageAttribute), false);
			Assert.AreEqual (1, attrs.Length, "AttributeUsage");
			AttributeUsageAttribute aua = (AttributeUsageAttribute)attrs [0];
			Assert.IsTrue (aua.AllowMultiple, "AllowMultiple");
			Assert.IsFalse (aua.Inherited, "Inherited");
			AttributeTargets at = (AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Delegate);
			Assert.AreEqual (at, aua.ValidOn, "ValidOn");
		}
	}
}

#endif
