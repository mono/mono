//
// ZoneMembershipConditionTest.cs -
//	NUnit Test Cases for ZoneMembershipCondition
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

using NUnit.Framework;
using System;
using System.Collections;
using System.Security;
using System.Security.Policy;

namespace MonoTests.System.Security.Policy {

	[TestFixture]
	public class ZoneMembershipConditionTest {

		static Evidence allEmpty;
		static Evidence hostInternet;
		static Evidence hostIntranet;
		static Evidence hostMyComputer;
		static Evidence hostNoZone;
		static Evidence hostTrusted;
		static Evidence hostUntrusted;
		static Evidence hostOther;
		static Evidence assemblyInternet;
		static Evidence assemblyIntranet;
		static Evidence assemblyMyComputer;
		static Evidence assemblyNoZone;
		static Evidence assemblyTrusted;
		static Evidence assemblyUntrusted;
		static Evidence assemblyOther;
		static object wrongEvidence;

		private Evidence CreateHostEvidence (object o)
		{
			Evidence e = new Evidence ();
			e.AddHost (o);
			return e;
		}

		private Evidence CreateAssemblyEvidence (object o)
		{
			Evidence e = new Evidence ();
			e.AddAssembly (o);
			return e;
		}

		[TestFixtureSetUp]
		public void FixtureSetUp () 
		{
			wrongEvidence = new Site ("test");
			allEmpty = new Evidence ();
			hostInternet = CreateHostEvidence (new Zone (SecurityZone.Internet));
			hostIntranet = CreateHostEvidence (new Zone (SecurityZone.Intranet));
			hostMyComputer = CreateHostEvidence (new Zone (SecurityZone.MyComputer));
			hostNoZone = CreateHostEvidence (new Zone (SecurityZone.NoZone));
			hostTrusted = CreateHostEvidence (new Zone (SecurityZone.Trusted));
			hostUntrusted = CreateHostEvidence (new Zone (SecurityZone.Untrusted));
			hostOther = CreateHostEvidence (wrongEvidence);
			assemblyInternet = CreateAssemblyEvidence (new Zone (SecurityZone.Internet));
			assemblyIntranet = CreateAssemblyEvidence (new Zone (SecurityZone.Intranet));
			assemblyMyComputer = CreateAssemblyEvidence (new Zone (SecurityZone.MyComputer));
			assemblyNoZone = CreateAssemblyEvidence (new Zone (SecurityZone.NoZone));
			assemblyTrusted = CreateAssemblyEvidence (new Zone (SecurityZone.Trusted));
			assemblyUntrusted = CreateAssemblyEvidence (new Zone (SecurityZone.Untrusted));
			assemblyOther = CreateAssemblyEvidence (wrongEvidence);
		}

		private ZoneMembershipCondition BasicTest (SecurityZone zone)
		{
			ZoneMembershipCondition zmc = new ZoneMembershipCondition (zone);
			Assert.AreEqual (zone, zmc.SecurityZone, "SecurityZone");
			Assert.IsFalse (zmc.Check (null), "Check(null)");
			Assert.IsFalse (zmc.Check (allEmpty), "Check(empty)");
			Assert.IsFalse (zmc.Check (hostOther), "Check(hostOther)");
			Assert.IsFalse (zmc.Check (assemblyOther), "Check(assemblyOther)");

			ZoneMembershipCondition copy = (ZoneMembershipCondition) zmc.Copy ();
			Assert.IsTrue (zmc.Equals (copy), "Equals-1");
			Assert.IsTrue (copy.Equals (zmc), "Equals-2");
			Assert.IsFalse (Object.ReferenceEquals (zmc, copy), "!ReferenceEquals");
			Assert.IsFalse (zmc.Equals (null), "Equals-3");
			Assert.IsFalse (zmc.Equals (wrongEvidence), "Equals-4");

			SecurityElement se = zmc.ToXml ();
			copy.FromXml (se);
			Assert.IsTrue (zmc.Equals (copy), "Equals-5");
			Assert.AreEqual (se.ToString (), zmc.ToXml (null).ToString (), "Equals-6");

			Assert.IsTrue (zmc.ToString ().StartsWith ("Zone - "), "ToString-1");
			Assert.IsTrue (zmc.ToString ().EndsWith (zmc.SecurityZone.ToString ()), "ToString-2");

#if NET_2_0
			Assert.AreEqual (zmc.SecurityZone.GetHashCode (), zmc.GetHashCode (), "GetHashCode");
#endif

			return zmc; // for further tests
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ZoneMembershipCondition_Invalid ()
		{
			ZoneMembershipCondition zmc = new ZoneMembershipCondition ((SecurityZone)128);
		}

		[Test]
		public void ZoneMembershipCondition_Internet ()
		{
			ZoneMembershipCondition zmc = BasicTest (SecurityZone.Internet);
			Assert.IsTrue (zmc.Check (hostInternet), "Check(hostInternet)");
			Assert.IsFalse (zmc.Check (hostIntranet), "Check(hostIntranet)");
			Assert.IsFalse (zmc.Check (hostMyComputer), "Check(hostMyComputer)");
			Assert.IsFalse (zmc.Check (hostNoZone), "Check(hostNoZone)");
			Assert.IsFalse (zmc.Check (hostTrusted), "Check(hostTrusted)");
			Assert.IsFalse (zmc.Check (hostUntrusted), "Check(hostUntrusted)");
			Assert.IsFalse (zmc.Check (assemblyInternet), "Check(assemblyInternet)");
			Assert.IsFalse (zmc.Check (assemblyIntranet), "Check(assemblyIntranet)");
			Assert.IsFalse (zmc.Check (assemblyMyComputer), "Check(assemblyMyComputer)");
			Assert.IsFalse (zmc.Check (assemblyNoZone), "Check(assemblyNoZone)");
			Assert.IsFalse (zmc.Check (assemblyTrusted), "Check(assemblyTrusted)");
			Assert.IsFalse (zmc.Check (assemblyUntrusted), "Check(assemblyUntrusted)");
		}

		[Test]
		public void ZoneMembershipCondition_Intranet ()
		{
			ZoneMembershipCondition zmc = BasicTest (SecurityZone.Intranet);
			Assert.IsFalse (zmc.Check (hostInternet), "Check(hostInternet)");
			Assert.IsTrue (zmc.Check (hostIntranet), "Check(hostIntranet)");
			Assert.IsFalse (zmc.Check (hostMyComputer), "Check(hostMyComputer)");
			Assert.IsFalse (zmc.Check (hostNoZone), "Check(hostNoZone)");
			Assert.IsFalse (zmc.Check (hostTrusted), "Check(hostTrusted)");
			Assert.IsFalse (zmc.Check (hostUntrusted), "Check(hostUntrusted)");
			Assert.IsFalse (zmc.Check (assemblyInternet), "Check(assemblyInternet)");
			Assert.IsFalse (zmc.Check (assemblyIntranet), "Check(assemblyIntranet)");
			Assert.IsFalse (zmc.Check (assemblyMyComputer), "Check(assemblyMyComputer)");
			Assert.IsFalse (zmc.Check (assemblyNoZone), "Check(assemblyNoZone)");
			Assert.IsFalse (zmc.Check (assemblyTrusted), "Check(assemblyTrusted)");
			Assert.IsFalse (zmc.Check (assemblyUntrusted), "Check(assemblyUntrusted)");
		}

		[Test]
		public void ZoneMembershipCondition_MyComputer ()
		{
			ZoneMembershipCondition zmc = BasicTest (SecurityZone.MyComputer);
			Assert.IsFalse (zmc.Check (hostInternet), "Check(hostInternet)");
			Assert.IsFalse (zmc.Check (hostIntranet), "Check(hostIntranet)");
			Assert.IsTrue (zmc.Check (hostMyComputer), "Check(hostMyComputer)");
			Assert.IsFalse (zmc.Check (hostNoZone), "Check(hostNoZone)");
			Assert.IsFalse (zmc.Check (hostTrusted), "Check(hostTrusted)");
			Assert.IsFalse (zmc.Check (hostUntrusted), "Check(hostUntrusted)");
			Assert.IsFalse (zmc.Check (assemblyInternet), "Check(assemblyInternet)");
			Assert.IsFalse (zmc.Check (assemblyIntranet), "Check(assemblyIntranet)");
			Assert.IsFalse (zmc.Check (assemblyMyComputer), "Check(assemblyMyComputer)");
			Assert.IsFalse (zmc.Check (assemblyNoZone), "Check(assemblyNoZone)");
			Assert.IsFalse (zmc.Check (assemblyTrusted), "Check(assemblyTrusted)");
			Assert.IsFalse (zmc.Check (assemblyUntrusted), "Check(assemblyUntrusted)");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ZoneMembershipCondition_NoZone ()
		{
			ZoneMembershipCondition zmc = new ZoneMembershipCondition (SecurityZone.NoZone);
		}

		[Test]
		public void ZoneMembershipCondition_Trusted ()
		{
			ZoneMembershipCondition zmc = BasicTest (SecurityZone.Trusted);
			Assert.IsFalse (zmc.Check (hostInternet), "Check(hostInternet)");
			Assert.IsFalse (zmc.Check (hostIntranet), "Check(hostIntranet)");
			Assert.IsFalse (zmc.Check (hostMyComputer), "Check(hostMyComputer)");
			Assert.IsFalse (zmc.Check (hostNoZone), "Check(hostNoZone)");
			Assert.IsTrue (zmc.Check (hostTrusted), "Check(hostTrusted)");
			Assert.IsFalse (zmc.Check (hostUntrusted), "Check(hostUntrusted)");
			Assert.IsFalse (zmc.Check (assemblyInternet), "Check(assemblyInternet)");
			Assert.IsFalse (zmc.Check (assemblyIntranet), "Check(assemblyIntranet)");
			Assert.IsFalse (zmc.Check (assemblyMyComputer), "Check(assemblyMyComputer)");
			Assert.IsFalse (zmc.Check (assemblyNoZone), "Check(assemblyNoZone)");
			Assert.IsFalse (zmc.Check (assemblyTrusted), "Check(assemblyTrusted)");
			Assert.IsFalse (zmc.Check (assemblyUntrusted), "Check(assemblyUntrusted)");
		}

		[Test]
		public void ZoneMembershipCondition_Untrusted ()
		{
			ZoneMembershipCondition zmc = BasicTest (SecurityZone.Untrusted);
			Assert.IsFalse (zmc.Check (hostInternet), "Check(hostInternet)");
			Assert.IsFalse (zmc.Check (hostIntranet), "Check(hostIntranet)");
			Assert.IsFalse (zmc.Check (hostMyComputer), "Check(hostMyComputer)");
			Assert.IsFalse (zmc.Check (hostNoZone), "Check(hostNoZone)");
			Assert.IsFalse (zmc.Check (hostTrusted), "Check(hostTrusted)");
			Assert.IsTrue (zmc.Check (hostUntrusted), "Check(hostUntrusted)");
			Assert.IsFalse (zmc.Check (assemblyInternet), "Check(assemblyInternet)");
			Assert.IsFalse (zmc.Check (assemblyIntranet), "Check(assemblyIntranet)");
			Assert.IsFalse (zmc.Check (assemblyMyComputer), "Check(assemblyMyComputer)");
			Assert.IsFalse (zmc.Check (assemblyNoZone), "Check(assemblyNoZone)");
			Assert.IsFalse (zmc.Check (assemblyTrusted), "Check(assemblyTrusted)");
			Assert.IsFalse (zmc.Check (assemblyUntrusted), "Check(assemblyUntrusted)");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void SecurityZone_NoZone ()
		{
			ZoneMembershipCondition zmc = new ZoneMembershipCondition (SecurityZone.MyComputer);
			zmc.SecurityZone = SecurityZone.NoZone;
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void SecurityZone_Invalid ()
		{
			ZoneMembershipCondition zmc = new ZoneMembershipCondition (SecurityZone.MyComputer);
			zmc.SecurityZone = (SecurityZone)128;
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromXml_Null ()
		{
			ZoneMembershipCondition zmc = new ZoneMembershipCondition (SecurityZone.MyComputer);
			zmc.FromXml (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_InvalidTag ()
		{
			ZoneMembershipCondition zmc = new ZoneMembershipCondition (SecurityZone.MyComputer);
			SecurityElement se = zmc.ToXml ();
			se.Tag = "IMonoship";
			zmc.FromXml (se);
		}

		[Test]
		public void FromXml_InvalidClass ()
		{
			ZoneMembershipCondition zmc = new ZoneMembershipCondition (SecurityZone.MyComputer);
			SecurityElement se = zmc.ToXml ();
			se.Attributes ["class"] = "Hello world";
			zmc.FromXml (se);
		}

		[Test]
		public void FromXml_NoClass ()
		{
			ZoneMembershipCondition zmc = new ZoneMembershipCondition (SecurityZone.MyComputer);
			SecurityElement se = zmc.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("version", se.Attribute ("version"));
			zmc.FromXml (w);
			// doesn't even care of the class attribute presence
		}

		[Test]
		public void FromXml_InvalidVersion ()
		{
			ZoneMembershipCondition zmc = new ZoneMembershipCondition (SecurityZone.MyComputer);
			SecurityElement se = zmc.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", se.Attribute ("class"));
			w.AddAttribute ("version", "2");
			w.AddAttribute ("Zone", se.Attribute ("Zone"));
			zmc.FromXml (w);
		}

		[Test]
		public void FromXml_NoVersion ()
		{
			ZoneMembershipCondition zmc = new ZoneMembershipCondition (SecurityZone.MyComputer);
			SecurityElement se = zmc.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", se.Attribute ("class"));
			zmc.FromXml (w);
		}

		[Test]
#if MOBILE
		[ExpectedException (typeof (NotSupportedException))]
#endif
		public void FromXml_PolicyLevel ()
		{
			ZoneMembershipCondition zmc = new ZoneMembershipCondition (SecurityZone.MyComputer);
			SecurityElement se = zmc.ToXml ();
			// is it accepted for all policy levels ?
			IEnumerator e = SecurityManager.PolicyHierarchy ();
			while (e.MoveNext ()) {
				PolicyLevel pl = e.Current as PolicyLevel;
				ZoneMembershipCondition spl = new ZoneMembershipCondition (SecurityZone.Internet);
				spl.FromXml (se, pl);
				Assert.IsTrue (spl.Equals (zmc), "FromXml(PolicyLevel='" + pl.Label + "')");
			}
			// yes!
		}

		[Test]
		public void ToXml_Null ()
		{
			ZoneMembershipCondition zmc = new ZoneMembershipCondition (SecurityZone.MyComputer);
			// no ArgumentNullException here
			SecurityElement se = zmc.ToXml (null);
			Assert.IsNotNull (se, "ToXml(null)");
		}

		[Test]
#if MOBILE
		[ExpectedException (typeof (NotSupportedException))]
#endif
		public void ToXml_PolicyLevel ()
		{
			ZoneMembershipCondition zmc = new ZoneMembershipCondition (SecurityZone.MyComputer);
			SecurityElement se = zmc.ToXml ();
			string s = zmc.ToXml ().ToString ();
			// is it accepted for all policy levels ?
			IEnumerator e = SecurityManager.PolicyHierarchy ();
			while (e.MoveNext ()) {
				PolicyLevel pl = e.Current as PolicyLevel;
				ZoneMembershipCondition spl = new ZoneMembershipCondition (SecurityZone.Internet);
				spl.FromXml (se, pl);
				Assert.AreEqual (s, spl.ToXml (pl).ToString (), "ToXml(PolicyLevel='" + pl.Label + "')");
			}
			// yes!
		}
	}
}
