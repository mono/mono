//
// UrlTest.cs - NUnit Test Cases for Url
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
	public class UrlTest : Assertion {

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Url_Null () 
		{
			Url u = new Url (null);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void Url_Empty () 
		{
			Url u = new Url (String.Empty);
		}

		[Test]
		public void Url_NoProtocol () 
		{
			Url u = new Url ("index.html");
			AssertEquals ("Value", "file://INDEX.HTML", u.Value);
		}

		[Test]
		public void Url_WellKnownProtocol () 
		{
			Url u = new Url ("file://mono/index.html");
			AssertEquals ("file.Value", "file://MONO/INDEX.HTML", u.Value);

			u = new Url ("ftp://www.go-mono.com");
			AssertEquals ("ftp.Value", "ftp://www.go-mono.com/", u.Value);
			
			u = new Url ("http://www.go-mono.com");
			AssertEquals ("http.Value", "http://www.go-mono.com/", u.Value);

			u = new Url ("https://www.go-mono.com");
			AssertEquals ("https.Value", "https://www.go-mono.com/", u.Value);
		}

		[Test]
		public void Url_UnknownProtocol () 
		{
			Url u = new Url ("mono://www.go-mono.com");
			AssertEquals ("mono.Value", "mono://www.go-mono.com/", u.Value);
		}

		[Test]
		public void Url_RelativePath () 
		{
			Url u = new Url ("http://www.go-mono.com/path/../newpath/index.html");
			AssertEquals ("Value", "http://www.go-mono.com/path/../newpath/index.html", u.Value);
		}

		[Test]
		public void Url_GoMonoWebUrl () 
		{
			Url u = new Url ("http://www.go-mono.com");
			AssertEquals ("Value", "http://www.go-mono.com/", u.Value);
			AssertEquals ("ToString", "<System.Security.Policy.Url version=\"1\">" + Environment.NewLine + "   <Url>http://www.go-mono.com/</Url>" + Environment.NewLine + "</System.Security.Policy.Url>" + Environment.NewLine, u.ToString ());

			Url u2 = (Url) u.Copy ();
			AssertEquals ("Copy.Value", u.Value, u2.Value);
			AssertEquals ("Copy.GetHashCode", u.GetHashCode (), u2.GetHashCode ());

			UrlIdentityPermission uip = (UrlIdentityPermission) u.CreateIdentityPermission (null);
			AssertEquals ("CreateIdentityPermission", u.Value, uip.Url);

			Assert ("Equals", u.Equals (u2));
			Url u3 = new Url ("go-mono.com");
			Assert ("!Equals", !u.Equals (u3));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Url_InvalidSite () 
		{
			Url u = new Url ("http://www.go-mono.*");
		}

		[Test]
		public void EqualsCaseSensitive () 
		{
			Url u1 = new Url ("http://www.go-mono.com");
			Url u2 = new Url ("http://www.Go-Mono.com");
			Assert ("CaseSensitive", u1.Equals (u2));
		}

		[Test]
		public void EqualsPartial () 
		{
			Url u1 = new Url ("http://www.go-mono.com/index.html");
			Url u2 = new Url ("http://www.go-mono.com/*");
			Assert ("Partial:1-2", !u1.Equals (u2));
			Assert ("Partial:2-1", !u2.Equals (u1));
		}

		[Test]
		public void EqualsNull () 
		{
			Url u = new Url ("http://www.go-mono.com");
			Assert ("EqualsNull", !u.Equals (null));
		}

		[Test]
		public void Url_LoneStar () 
		{
			Url u = new Url ("*");
#if NET_2_0
			AssertEquals ("Value", "*", u.Value);
			AssertEquals ("ToString", "<System.Security.Policy.Url version=\"1\">" + Environment.NewLine + "<Url>*</Url>" + Environment.NewLine + "</System.Security.Policy.Url>" + Environment.NewLine, u.ToString ());
#else
			AssertEquals ("Value", "file://*", u.Value);
			AssertEquals ("ToString", "<System.Security.Policy.Url version=\"1\">" + Environment.NewLine + "   <Url>file://*</Url>" + Environment.NewLine + "</System.Security.Policy.Url>" + Environment.NewLine, u.ToString ());
#endif
			Url u2 = (Url) u.Copy ();
			AssertEquals ("Copy.Value", u.Value, u2.Value);
			AssertEquals ("Copy.GetHashCode", u.GetHashCode (), u2.GetHashCode ());

			UrlIdentityPermission uip = (UrlIdentityPermission) u.CreateIdentityPermission (null);
			AssertEquals ("CreateIdentityPermission", u.Value, uip.Url);

			Assert ("Equals", u.Equals (u2));
			Url u3 = new Url ("index.html");
			Assert ("!Equals(*)", !u.Equals (u3));

			u2 = new Url ("file://*");
			AssertEquals ("Value-file://*", "file://*", u2.Value);
			Assert ("Equals-file://*", u.Equals (u2));
		}
	}
}
