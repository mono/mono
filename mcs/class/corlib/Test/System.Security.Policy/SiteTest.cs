//
// SiteTest.cs - NUnit Test Cases for Site
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2004 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using System;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;

namespace MonoTests.System.Security.Policy {

	[TestFixture]
	public class SiteTest : Assertion {

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Site_Null () 
		{
			Site s = new Site (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Site_Empty () 
		{
			Site s = new Site (String.Empty);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Site_FileUrl () 
		{
			Site s = new Site ("file://mono/index.html");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Site_AllGoMono () 
		{
			Site s = new Site ("http://*.go-mono.com");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Site_FullUrlWithPort () 
		{
			Site s = new Site ("http://www.go-mono.com:8080/index.html");
		}

		[Test]
		public void Site_GoMonoWebSite () 
		{
			Site s = new Site ("www.go-mono.com");
			AssertEquals ("Name", "www.go-mono.com", s.Name);
			AssertEquals ("ToString", "<System.Security.Policy.Site version=\"1\">" + Environment.NewLine + "   <Name>www.go-mono.com</Name>" + Environment.NewLine + "</System.Security.Policy.Site>" + Environment.NewLine, s.ToString ());

			Site s2 = (Site) s.Copy ();
			AssertEquals ("Copy.Name", s.Name, s2.Name);
			AssertEquals ("Copy.GetHashCode", s.GetHashCode (), s2.GetHashCode ());

			SiteIdentityPermission sip = (SiteIdentityPermission) s.CreateIdentityPermission (null);
			AssertEquals ("CreateIdentityPermission", s.Name, sip.Site);

			Assert ("Equals", s.Equals (s2));
			Site s3 = new Site ("go-mono.com");
			Assert ("!Equals", !s.Equals (s3));
		}

		[Test]
		public void Site_AllGoMonoSite () 
		{
			Site s = new Site ("*.go-mono.com");
			AssertEquals ("Name", "*.go-mono.com", s.Name);
			AssertEquals ("ToString", "<System.Security.Policy.Site version=\"1\">" + Environment.NewLine + "   <Name>*.go-mono.com</Name>" + Environment.NewLine + "</System.Security.Policy.Site>" + Environment.NewLine, s.ToString ());

			Site s2 = (Site) s.Copy ();
			AssertEquals ("Copy.Name", s.Name, s2.Name);
			AssertEquals ("Copy.GetHashCode", s.GetHashCode (), s2.GetHashCode ());

			SiteIdentityPermission sip = (SiteIdentityPermission) s.CreateIdentityPermission (null);
			AssertEquals ("CreateIdentityPermission", s.Name, sip.Site);

			Assert ("Equals", s.Equals (s2));
			Site s3 = new Site ("go-mono.com");
			Assert ("!Equals", !s.Equals (s3));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Site_GoMonoAllTLD () 
		{
			Site s = new Site ("www.go-mono.*");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Site_TwoStars () 
		{
			Site s = new Site ("*.*.go-mono.com");
		}

		[Test]
		public void EqualsCaseSensitive () {
			Site s1 = new Site ("*.go-mono.com");
			Site s2 = new Site ("*.Go-Mono.com");
			Assert ("CaseSensitive", s1.Equals (s2));
		}

		[Test]
		public void EqualsPartial () 
		{
			Site s1 = new Site ("www.go-mono.com");
			Site s2 = new Site ("*.go-mono.com");
			Assert ("Partial:1-2", !s1.Equals (s2));
			Assert ("Partial:2-1", !s2.Equals (s1));
		}

		[Test]
		public void EqualsNull () 
		{
			Site s1 = new Site ("*.go-mono.com");
			Assert ("EqualsNull", !s1.Equals (null));
		}

		[Test]
		public void Site_LoneStar () 
		{
			Site s = new Site ("*");
			AssertEquals ("Name", "*", s.Name);
			AssertEquals ("ToString", "<System.Security.Policy.Site version=\"1\">" + Environment.NewLine + "   <Name>*</Name>" + Environment.NewLine + "</System.Security.Policy.Site>" + Environment.NewLine, s.ToString ());

			Site s2 = (Site) s.Copy ();
			AssertEquals ("Copy.Name", s.Name, s2.Name);
			AssertEquals ("Copy.GetHashCode", s.GetHashCode (), s2.GetHashCode ());

			SiteIdentityPermission sip = (SiteIdentityPermission) s.CreateIdentityPermission (null);
			AssertEquals ("CreateIdentityPermission", s.Name, sip.Site);

			Assert ("Equals", s.Equals (s2));
			Site s3 = new Site ("go-mono.com");
			Assert ("!Equals", !s.Equals (s3));
		}

		[Test]
		public void AllChars () 
		{
			for (int i=0; i < 256; i++) {
				bool actual = false;
				char c = Convert.ToChar (i);
				try {
					Site s = new Site (Convert.ToString (c));
					actual = true;
					// Console.WriteLine ("GOOD: {0} - {1}", i, c);
				}
				catch {
					// Console.WriteLine ("FAIL: {0} - {1}", i, c);
				}
				bool result = ((i == 42)		// *
					|| (i == 45)			// -
					|| (i >= 47 && i <= 57)		// /,0-9
					|| (i >= 64 && i <= 90)		// @,A-Z
					|| (i == 95)			// _
					|| (i >= 97 && i <= 122));	// a-z
				Assert ("#"+i, (actual == result));
			}
		}
	}
}
