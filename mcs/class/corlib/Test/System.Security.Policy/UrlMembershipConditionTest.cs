//
// UrlMembershipConditionTest.cs - NUnit Test Cases for UrlMembershipCondition
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
	public class UrlMembershipConditionTest : Assertion {

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void UrlMembershipCondition_Null () 
		{
			UrlMembershipCondition umc = new UrlMembershipCondition (null);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void UrlMembershipCondition_Empty () 
		{
			UrlMembershipCondition umc = new UrlMembershipCondition (String.Empty);
		}

		[Test]
		public void UrlMembershipCondition_FileUrl () 
		{
			UrlMembershipCondition umc = new UrlMembershipCondition ("file://mono/index.html");
		}

		[Test]
		public void UrlMembershipCondition_FullUrlWithPort () 
		{
			UrlMembershipCondition umc = new UrlMembershipCondition ("http://www.go-mono.com:8080/index.html");
		}

		[Test]
		public void UrlMembershipCondition_GoMonoWebUrl () 
		{
			UrlMembershipCondition umc = new UrlMembershipCondition ("http://www.go-mono.com/");
			AssertEquals ("Url", "http://www.go-mono.com/", umc.Url);
			AssertEquals ("ToString", "Url - http://www.go-mono.com/", umc.ToString ());

			UrlMembershipCondition umc2 = (UrlMembershipCondition) umc.Copy ();
			AssertEquals ("Copy.Url", umc.Url, umc2.Url);
			AssertEquals ("Copy.GetHashCode", umc.GetHashCode (), umc2.GetHashCode ());

			SecurityElement se = umc2.ToXml ();
			UrlMembershipCondition umc3 = new UrlMembershipCondition ("*");
			umc3.FromXml (se);
			AssertEquals ("ToXml/FromXml", umc.Url, umc3.Url);

			Assert ("Equals", umc.Equals (umc2));
			UrlMembershipCondition umc4 = new UrlMembershipCondition ("http://www.go-mono.com");
			// note that a last slash is added to Url - so it's equal
			Assert ("Equals-AutoAddedLastSlash", umc.Equals (umc4));
		}

		[Test]
		public void Url_AllGoMonoUrl () 
		{
			UrlMembershipCondition umc = new UrlMembershipCondition ("http://www.go-mono.com/*");
			AssertEquals ("Url", "http://www.go-mono.com/*", umc.Url);
			AssertEquals ("ToString", "Url - http://www.go-mono.com/*", umc.ToString ());

			UrlMembershipCondition umc2 = (UrlMembershipCondition) umc.Copy ();
			AssertEquals ("Copy.Url", umc.Url, umc2.Url);
			AssertEquals ("Copy.GetHashCode", umc.GetHashCode (), umc2.GetHashCode ());

			SecurityElement se = umc2.ToXml ();
			UrlMembershipCondition umc3 = new UrlMembershipCondition ("*");
			umc3.FromXml (se);
			AssertEquals ("ToXml/FromXml", umc.Url, umc3.Url);

			Assert ("Equals", umc.Equals (umc2));
			UrlMembershipCondition umc4 = new UrlMembershipCondition ("http://www.go-mono.com/");
			Assert ("Equals-*", umc.Equals (umc4));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Url_InvalidSite ()
		{
			UrlMembershipCondition umc = new UrlMembershipCondition ("http://www.go-mono.*");
		}

		[Test]
		public void Url_NoProtocol () 
		{
			UrlMembershipCondition umc = new UrlMembershipCondition ("www.go-mono.com");
			// note: no last slash here
			AssertEquals ("Url", "file://WWW.GO-MONO.COM", umc.Url);
			AssertEquals ("ToString", "Url - file://WWW.GO-MONO.COM", umc.ToString ());
		}

		[Test]
		public void Url_WellKnownProtocol () 
		{
			UrlMembershipCondition umc = new UrlMembershipCondition ("http://www.go-mono.com");
			AssertEquals ("Url", "http://www.go-mono.com/", umc.Url);
			AssertEquals ("ToString", "Url - http://www.go-mono.com/", umc.ToString ());

			umc = new UrlMembershipCondition ("https://www.go-mono.com");
			AssertEquals ("Url", "https://www.go-mono.com/", umc.Url);
			AssertEquals ("ToString", "Url - https://www.go-mono.com/", umc.ToString ());

			umc = new UrlMembershipCondition ("ftp://www.go-mono.com");
			AssertEquals ("Url", "ftp://www.go-mono.com/", umc.Url);
			AssertEquals ("ToString", "Url - ftp://www.go-mono.com/", umc.ToString ());

			umc = new UrlMembershipCondition ("file://www.go-mono.com");
			AssertEquals ("Url", "file://WWW.GO-MONO.COM", umc.Url);
			AssertEquals ("ToString", "Url - file://WWW.GO-MONO.COM", umc.ToString ());
		}

		[Test]
		public void Url_UnknownProtocol () 
		{
			UrlMembershipCondition umc = new UrlMembershipCondition ("mono://www.go-mono.com");
			AssertEquals ("Url", "mono://www.go-mono.com/", umc.Url);
			AssertEquals ("ToString", "Url - mono://www.go-mono.com/", umc.ToString ());
		}

		[Test]
		public void Url_RelativePath () 
		{
			UrlMembershipCondition umc = new UrlMembershipCondition ("http://www.go-mono.com/path/../newpath/index.html");
			AssertEquals ("Url", "http://www.go-mono.com/path/../newpath/index.html", umc.Url);
			AssertEquals ("ToString", "Url - http://www.go-mono.com/path/../newpath/index.html", umc.ToString ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Url_Null () 
		{
			UrlMembershipCondition umc = new UrlMembershipCondition ("ftp://www.go-mono.com");
			umc.Url = null;
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void Url_Empty () 
		{
			UrlMembershipCondition umc = new UrlMembershipCondition ("ftp://www.go-mono.com");
			umc.Url = String.Empty;
		}

		[Test]
		public void CheckNull () 
		{
			UrlMembershipCondition umc = new UrlMembershipCondition ("http://www.go-mono.com");
			Assert ("Check(null)", !umc.Check (null));
		}

		[Test]
		public void CheckPositive () 
		{
			UrlMembershipCondition umc = new UrlMembershipCondition ("http://www.go-mono.com");
			Evidence e = new Evidence ();
			e.AddHost (new Url ("http://www.go-mono.com"));
			Assert ("Check(+)", umc.Check (e));
		}

		[Test]
		public void CheckPositive_Partial () 
		{
			UrlMembershipCondition umc = new UrlMembershipCondition ("http://www.go-mono.com/*");
			Evidence e = new Evidence ();
			e.AddHost (new Url ("http://www.go-mono.com/index.html"));
			Assert ("Check(+-)", umc.Check (e));
		}

		[Test]
		public void CheckNegative () 
		{
			UrlMembershipCondition umc = new UrlMembershipCondition ("http://www.go-mono.com");
			Evidence e = new Evidence ();
			e.AddHost (new Url ("http://www.go-mono.org"));
			Assert ("Check(-)", !umc.Check (e));
		}

		[Test]
		public void CheckNegative_NoUrlEvidence () 
		{
			UrlMembershipCondition umc = new UrlMembershipCondition ("http://www.go-mono.com");
			Evidence e = new Evidence ();
			e.AddHost (new Zone (SecurityZone.MyComputer));
			Assert ("Check(?)", !umc.Check (e));
		}

		[Test]
		public void CheckMultipleEvidences () 
		{
			UrlMembershipCondition umc = new UrlMembershipCondition ("http://www.go-mono.com");
			Evidence e = new Evidence ();
			e.AddHost (new Url ("http://www.go-mono.org"));	// the bad
			e.AddHost (new Url ("http://www.go-mono.com"));	// the good
			e.AddHost (new Zone (SecurityZone.MyComputer));	// and the ugly (couldn't resist ;)
			Assert ("Check(n)", umc.Check (e));
			// check all Url evidence (i.e. do not stop at the first Url evidence)
		}

		[Test]
		public void EqualsCaseSensitive_Http () 
		{
			UrlMembershipCondition umc1 = new UrlMembershipCondition ("http://www.go-mono.com");
			UrlMembershipCondition umc2 = new UrlMembershipCondition ("http://www.Go-Mono.com");
			Assert ("CaseSensitive", umc1.Equals (umc2));
		}

		[Test]
		public void EqualsCaseSensitive_File () 
		{
			UrlMembershipCondition umc1 = new UrlMembershipCondition ("file://MONO");
			UrlMembershipCondition umc2 = new UrlMembershipCondition ("file://mono");
			Assert ("CaseSensitive", umc1.Equals (umc2));
		}

		[Test]
		public void EqualsNull () 
		{
			UrlMembershipCondition umc = new UrlMembershipCondition ("http://www.go-mono.com");
			Assert ("EqualsNull", !umc.Equals (null));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromXmlNull () 
		{
			UrlMembershipCondition umc = new UrlMembershipCondition ("http://www.go-mono.com");
			umc.FromXml (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXmlInvalid () 
		{
			UrlMembershipCondition umc = new UrlMembershipCondition ("http://www.go-mono.com");
			SecurityElement se = umc.ToXml ();
			se.Tag = "IMonoship";
			umc.FromXml (se);
		}

		[Test]
		public void FromXmlPolicyLevel () 
		{
			UrlMembershipCondition umc = new UrlMembershipCondition ("http://www.go-mono.com");
			SecurityElement se = umc.ToXml ();
			// is it accepted for all policy levels ?
			IEnumerator e = SecurityManager.PolicyHierarchy ();
			while (e.MoveNext ()) {
				PolicyLevel pl = e.Current as PolicyLevel;
				UrlMembershipCondition spl = new UrlMembershipCondition ("*");
				spl.FromXml (se, pl);
				Assert ("FromXml(PolicyLevel='" + pl.Label + "')", spl.Equals (umc));
			}
			// yes!
		}

		[Test]
		public void ToXmlNull () 
		{
			UrlMembershipCondition umc = new UrlMembershipCondition ("http://www.go-mono.com");
			// no ArgumentNullException here
			SecurityElement se = umc.ToXml (null);
			AssertNotNull ("ToXml(null)", se);
		}

		[Test]
		public void ToXmlPolicyLevel () 
		{
			UrlMembershipCondition umc = new UrlMembershipCondition ("http://www.go-mono.com");
			SecurityElement se = umc.ToXml ();
			string s = umc.ToXml ().ToString ();
			// is it accepted for all policy levels ?
			IEnumerator e = SecurityManager.PolicyHierarchy ();
			while (e.MoveNext ()) {
				PolicyLevel pl = e.Current as PolicyLevel;
				UrlMembershipCondition spl = new UrlMembershipCondition ("*");
				spl.FromXml (se, pl);
				AssertEquals ("ToXml(PolicyLevel='" + pl.Label + "')", s, spl.ToXml (pl).ToString ());
			}
			// yes!
		}

		[Test]
		public void ToFromXmlRoundTrip () 
		{
			UrlMembershipCondition umc1 = new UrlMembershipCondition ("http://www.go-mono.com");
			SecurityElement se = umc1.ToXml ();

			UrlMembershipCondition umc2 = new UrlMembershipCondition ("*");
			umc2.FromXml (se);

			AssertEquals ("ToFromXmlRoundTrip", umc1.GetHashCode (), umc2.GetHashCode ());
		}
	}
}
