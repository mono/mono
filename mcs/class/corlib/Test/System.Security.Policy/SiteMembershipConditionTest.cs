//
// SiteMembershipConditionTest.cs - NUnit Test Cases for SiteMembershipCondition
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2004 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using System;
using System.Collections;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;

namespace MonoTests.System.Security.Policy {

	[TestFixture]
	public class SiteMembershipConditionTest : Assertion {

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void SiteMembershipCondition_Null () 
		{
			SiteMembershipCondition smc = new SiteMembershipCondition (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void SiteMembershipCondition_Empty () 
		{
			SiteMembershipCondition smc = new SiteMembershipCondition (String.Empty);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void SiteMembershipCondition_FileUrl () 
		{
			SiteMembershipCondition smc = new SiteMembershipCondition ("file://mono/index.html");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void SiteMembershipCondition_FullUrlWithPort () 
		{
			SiteMembershipCondition smc = new SiteMembershipCondition ("http://www.go-mono.com:8080/index.html");
		}

		[Test]
		public void SiteMembershipCondition_GoMonoWebSite () 
		{
			SiteMembershipCondition smc = new SiteMembershipCondition ("www.go-mono.com");
			AssertEquals ("Site", "www.go-mono.com", smc.Site);
			AssertEquals ("ToString", "Site - www.go-mono.com", smc.ToString ());

			SiteMembershipCondition smc2 = (SiteMembershipCondition) smc.Copy ();
			AssertEquals ("Copy.Site", smc.Site, smc2.Site);
			AssertEquals ("Copy.GetHashCode", smc.GetHashCode (), smc2.GetHashCode ());

			SecurityElement se = smc2.ToXml ();
			SiteMembershipCondition smc3 = new SiteMembershipCondition ("*");
			smc3.FromXml (se);
			AssertEquals ("ToXml/FromXml", smc.Site, smc3.Site);

			Assert ("Equals", smc.Equals (smc2));
			SiteMembershipCondition smc4 = new SiteMembershipCondition ("go-mono.com");
			Assert ("!Equals", !smc.Equals (smc4));
		}

		[Test]
		public void Site_AllGoMonoSite () 
		{
			SiteMembershipCondition smc = new SiteMembershipCondition ("*.go-mono.com");
			AssertEquals ("Site", "*.go-mono.com", smc.Site);
			AssertEquals ("ToString", "Site - *.go-mono.com", smc.ToString ());

			SiteMembershipCondition smc2 = (SiteMembershipCondition) smc.Copy ();
			AssertEquals ("Copy.Site", smc.Site, smc2.Site);
			AssertEquals ("Copy.GetHashCode", smc.GetHashCode (), smc2.GetHashCode ());

			SecurityElement se = smc2.ToXml ();
			SiteMembershipCondition smc3 = new SiteMembershipCondition ("*");
			smc3.FromXml (se);
			AssertEquals ("ToXml/FromXml", smc.Site, smc3.Site);

			Assert ("Equals", smc.Equals (smc2));
			SiteMembershipCondition smc4 = new SiteMembershipCondition ("go-mono.com");
			Assert ("!Equals", !smc.Equals (smc4));
		}

		[Test]
		public void CheckNull () 
		{
			SiteMembershipCondition smc = new SiteMembershipCondition ("*.go-mono.com");
			Assert ("Check(null)", !smc.Check (null));
		}

		[Test]
		public void CheckPositive () 
		{
			SiteMembershipCondition smc = new SiteMembershipCondition ("*.go-mono.com");
			Evidence e = new Evidence ();
			e.AddHost (new Site ("*.go-mono.com"));
			Assert ("Check(+)", smc.Check (e));
		}

		[Test]
		public void CheckPositive_Partial () 
		{
			SiteMembershipCondition smc = new SiteMembershipCondition ("*.go-mono.com");
			Evidence e = new Evidence ();
			e.AddHost (new Site ("www.go-mono.com"));
			Assert ("Check(+-)", smc.Check (e));
		}

		[Test]
		public void CheckNegative () 
		{
			SiteMembershipCondition smc = new SiteMembershipCondition ("*.go-mono.com");
			Evidence e = new Evidence ();
			e.AddHost (new Site ("*.go-mono.org"));
			Assert ("Check(-)", !smc.Check (e));
		}

		[Test]
		public void CheckNegative_NoSiteEvidence () 
		{
			SiteMembershipCondition smc = new SiteMembershipCondition ("*.go-mono.com");
			Evidence e = new Evidence ();
			e.AddHost (new Zone (SecurityZone.MyComputer));
			Assert ("Check(?)", !smc.Check (e));
		}

		[Test]
		public void EqualsCaseSensitive () 
		{
			SiteMembershipCondition smc1 = new SiteMembershipCondition ("*.go-mono.com");
			SiteMembershipCondition smc2 = new SiteMembershipCondition ("*.Go-Mono.com");
			Assert ("CaseSensitive", smc1.Equals (smc2));
		}

		[Test]
		public void EqualsNull () 
		{
			SiteMembershipCondition smc = new SiteMembershipCondition ("*.go-mono.com");
			Assert ("EqualsNull", !smc.Equals (null));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromXmlNull () 
		{
			SiteMembershipCondition smc = new SiteMembershipCondition ("*.go-mono.com");
			smc.FromXml (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXmlInvalid () 
		{
			SiteMembershipCondition smc = new SiteMembershipCondition ("*.go-mono.com");
			SecurityElement se = smc.ToXml ();
			se.Tag = "IMonoship";
			smc.FromXml (se);
		}

		[Test]
		public void FromXmlPolicyLevel () 
		{
			SiteMembershipCondition smc = new SiteMembershipCondition ("*.go-mono.com");
			SecurityElement se = smc.ToXml ();
			// is it accepted for all policy levels ?
			IEnumerator e = SecurityManager.PolicyHierarchy ();
			while (e.MoveNext ()) {
				PolicyLevel pl = e.Current as PolicyLevel;
				SiteMembershipCondition spl = new SiteMembershipCondition ("*");
				spl.FromXml (se, pl);
				Assert ("FromXml(PolicyLevel='" + pl.Label + "')", spl.Equals (smc));
			}
			// yes!
		}

		[Test]
		public void ToXmlNull () 
		{
			SiteMembershipCondition smc = new SiteMembershipCondition ("*.go-mono.com");
			// no ArgumentNullException here
			SecurityElement se = smc.ToXml (null);
			AssertNotNull ("ToXml(null)", se);
		}

		[Test]
		public void ToXmlPolicyLevel () 
		{
			SiteMembershipCondition smc = new SiteMembershipCondition ("*.go-mono.com");
			SecurityElement se = smc.ToXml ();
			string s = smc.ToXml ().ToString ();
			// is it accepted for all policy levels ?
			IEnumerator e = SecurityManager.PolicyHierarchy ();
			while (e.MoveNext ()) {
				PolicyLevel pl = e.Current as PolicyLevel;
				SiteMembershipCondition spl = new SiteMembershipCondition ("*");
				spl.FromXml (se, pl);
				AssertEquals ("ToXml(PolicyLevel='" + pl.Label + "')", s, spl.ToXml (pl).ToString ());
			}
			// yes!
		}

		[Test]
		public void ToFromXmlRoundTrip () 
		{
			SiteMembershipCondition smc1 = new SiteMembershipCondition ("*.go-mono.com");
			SecurityElement se = smc1.ToXml ();

			SiteMembershipCondition smc2 = new SiteMembershipCondition ("*");
			smc2.FromXml (se);

			AssertEquals ("ToFromXmlRoundTrip", smc1.GetHashCode (), smc2.GetHashCode ());
		}
	}
}
