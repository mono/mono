//
// UrlMembershipConditionTest.cs - NUnit Test Cases for UrlMembershipCondition
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2004 Motus Technologies Inc. (http://www.motus.com)
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
	public class UrlMembershipConditionTest {

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
			UrlMembershipCondition umc = new UrlMembershipCondition ("http://www.example.com:8080/index.html");
		}

		[Test]
		public void UrlMembershipCondition_GoMonoWebUrl () 
		{
			UrlMembershipCondition umc = new UrlMembershipCondition ("http://www.example.com/");
			Assert.AreEqual ("http://www.example.com/", umc.Url, "Url");
			Assert.AreEqual ("Url - http://www.example.com/", umc.ToString (), "ToString");

			UrlMembershipCondition umc2 = (UrlMembershipCondition) umc.Copy ();
			Assert.AreEqual (umc.Url, umc2.Url, "Copy.Url");
			Assert.AreEqual (umc.GetHashCode (), umc2.GetHashCode (), "Copy.GetHashCode");

			SecurityElement se = umc2.ToXml ();
			UrlMembershipCondition umc3 = new UrlMembershipCondition ("*");
			umc3.FromXml (se);
			Assert.AreEqual (umc.Url, umc3.Url, "ToXml/FromXml");

			Assert.IsTrue (umc.Equals (umc2), "Equals");
			UrlMembershipCondition umc4 = new UrlMembershipCondition ("http://www.example.com");
			// note that a last slash is added to Url - so it's equal
			Assert.IsTrue (umc.Equals (umc4), "Equals-AutoAddedLastSlash");
		}

		[Test]
		public void Url_AllGoMonoUrl () 
		{
			UrlMembershipCondition umc = new UrlMembershipCondition ("http://www.example.com/*");
			Assert.AreEqual ("http://www.example.com/*", umc.Url, "Url");
			Assert.AreEqual ("Url - http://www.example.com/*", umc.ToString (), "ToString");

			UrlMembershipCondition umc2 = (UrlMembershipCondition) umc.Copy ();
			Assert.AreEqual (umc.Url, umc2.Url, "Copy.Url");
			Assert.AreEqual (umc.GetHashCode (), umc2.GetHashCode (), "Copy.GetHashCode");

			SecurityElement se = umc2.ToXml ();
			UrlMembershipCondition umc3 = new UrlMembershipCondition ("*");
			umc3.FromXml (se);
			Assert.AreEqual (umc.Url, umc3.Url, "ToXml/FromXml");

			Assert.IsTrue (umc.Equals (umc2), "Equals");
			UrlMembershipCondition umc4 = new UrlMembershipCondition ("http://www.example.com/");
			Assert.IsTrue (umc.Equals (umc4), "Equals-*");
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
			UrlMembershipCondition umc = new UrlMembershipCondition ("www.example.com");
			Assert.AreEqual ("www.example.com", umc.Url, "Url");
			Assert.AreEqual ("Url - www.example.com", umc.ToString (), "ToString");
		}

		[Test]
		public void Url_WellKnownProtocol () 
		{
			UrlMembershipCondition umc = new UrlMembershipCondition ("http://www.example.com");
			Assert.AreEqual ("http://www.example.com", umc.Url, "http-Url");
			Assert.AreEqual ("Url - http://www.example.com", umc.ToString (), "http-ToString");
			umc = new UrlMembershipCondition ("https://www.example.com");
			Assert.AreEqual ("https://www.example.com", umc.Url, "https-Url");
			Assert.AreEqual ("Url - https://www.example.com", umc.ToString (), "https-ToString");

			umc = new UrlMembershipCondition ("ftp://www.example.com");
			Assert.AreEqual ("ftp://www.example.com", umc.Url, "ftp-Url");
			Assert.AreEqual ("Url - ftp://www.example.com", umc.ToString (), "ftp-ToString");

			umc = new UrlMembershipCondition ("file://www.example.com");
			Assert.AreEqual ("file://www.example.com", umc.Url, "file-Url");
			Assert.AreEqual ("Url - file://www.example.com", umc.ToString (), "file-ToString");
		}

		[Test]
		public void Url_UnknownProtocol () 
		{
			UrlMembershipCondition umc = new UrlMembershipCondition ("mono://www.example.com");
			Assert.AreEqual ("mono://www.example.com", umc.Url, "Url");
			Assert.AreEqual ("Url - mono://www.example.com", umc.ToString (), "ToString");
		}

		[Test]
		public void Url_RelativePath () 
		{
			UrlMembershipCondition umc = new UrlMembershipCondition ("http://www.example.com/path/../newpath/index.html");
			Assert.AreEqual ("http://www.example.com/path/../newpath/index.html", umc.Url, "Url");
			Assert.AreEqual ("Url - http://www.example.com/path/../newpath/index.html", umc.ToString (), "ToString");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Url_Null () 
		{
			UrlMembershipCondition umc = new UrlMembershipCondition ("ftp://www.example.com");
			umc.Url = null;
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void Url_Empty () 
		{
			UrlMembershipCondition umc = new UrlMembershipCondition ("ftp://www.example.com");
			umc.Url = String.Empty;
		}

		[Test]
		public void Check () 
		{
			UrlMembershipCondition umc = new UrlMembershipCondition ("http://www.example.com");

			Evidence e = null;
			Assert.IsFalse (umc.Check (e), "Check(null)");

			e = new Evidence ();
			Assert.IsFalse (umc.Check (e), "Check(empty)");

			e.AddHost (new Zone (SecurityZone.MyComputer));
			Assert.IsFalse (umc.Check (e), "Check(zone)");

			Url u = new Url ("http://www.example.com");
			e.AddAssembly (u);
			Assert.IsFalse (umc.Check (e), "Check(url-assembly)");
			e.AddHost (u);
			Assert.IsTrue (umc.Check (e), "Check(url-host)");
		}

		[Test]
		public void CheckPositive_Partial () 
		{
			UrlMembershipCondition umc = new UrlMembershipCondition ("http://www.example.com/*");
			Evidence e = new Evidence ();
			e.AddHost (new Url ("http://www.example.com/index.html"));
			Assert.IsTrue (umc.Check (e), "Check(+-)");
		}

		[Test]
		public void CheckNegative () 
		{
			UrlMembershipCondition umc = new UrlMembershipCondition ("http://www.example.com");
			Evidence e = new Evidence ();
			e.AddHost (new Url ("http://www.go-mono.org"));
			Assert.IsFalse (umc.Check (e), "Check(-)");
		}

		[Test]
		public void CheckMultipleEvidences () 
		{
			UrlMembershipCondition umc = new UrlMembershipCondition ("http://www.example.com");
			Evidence e = new Evidence ();
			e.AddHost (new Url ("http://www.go-mono.org"));	// the bad
			e.AddHost (new Url ("http://www.example.com"));	// the good
			e.AddHost (new Zone (SecurityZone.MyComputer));	// and the ugly (couldn't resist ;)
			Assert.IsTrue (umc.Check (e), "Check(n)");
			// check all Url evidence (i.e. do not stop at the first Url evidence)
		}

		[Test]
		public void EqualsCaseSensitive_Http () 
		{
			UrlMembershipCondition umc1 = new UrlMembershipCondition ("http://www.example.com");
			UrlMembershipCondition umc2 = new UrlMembershipCondition ("http://www.Example.com");
			Assert.IsTrue (umc1.Equals (umc2), "CaseSensitive");
		}

		[Test]
		public void EqualsCaseSensitive_File () 
		{
			UrlMembershipCondition umc1 = new UrlMembershipCondition ("file://MONO");
			UrlMembershipCondition umc2 = new UrlMembershipCondition ("file://mono");
			Assert.IsTrue (umc1.Equals (umc2), "CaseSensitive");
		}

		[Test]
		public void EqualsNull () 
		{
			UrlMembershipCondition umc = new UrlMembershipCondition ("http://www.example.com");
			Assert.IsFalse (umc.Equals (null), "EqualsNull");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromXml_Null () 
		{
			UrlMembershipCondition umc = new UrlMembershipCondition ("http://www.example.com");
			umc.FromXml (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_InvalidTag () 
		{
			UrlMembershipCondition umc = new UrlMembershipCondition ("http://www.example.com");
			SecurityElement se = umc.ToXml ();
			se.Tag = "IMonoship";
			umc.FromXml (se);
		}

		[Test]
		public void FromXml_InvalidClass ()
		{
			UrlMembershipCondition umc = new UrlMembershipCondition ("http://www.example.com");
			SecurityElement se = umc.ToXml ();
			se.Attributes ["class"] = "Hello world";
			umc.FromXml (se);
		}

		[Test]
		public void FromXml_NoClass ()
		{
			UrlMembershipCondition umc = new UrlMembershipCondition ("http://www.example.com");
			SecurityElement se = umc.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("version", se.Attribute ("version"));
			umc.FromXml (w);
			// doesn't even care of the class attribute presence
		}

		[Test]
		public void FromXml_InvalidVersion ()
		{
			UrlMembershipCondition umc = new UrlMembershipCondition ("http://www.example.com");
			SecurityElement se = umc.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", se.Attribute ("class"));
			w.AddAttribute ("version", "2");
			w.AddAttribute ("Url", se.Attribute ("Url"));
			umc.FromXml (w);
		}

		[Test]
		public void FromXml_NoVersion ()
		{
			UrlMembershipCondition umc = new UrlMembershipCondition ("http://www.example.com");
			SecurityElement se = umc.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", se.Attribute ("class"));
			umc.FromXml (w);
		}

		[Test]
#if MOBILE
		[ExpectedException (typeof (NotSupportedException))]
#endif
		public void FromXml_PolicyLevel () 
		{
			UrlMembershipCondition umc = new UrlMembershipCondition ("http://www.example.com");
			SecurityElement se = umc.ToXml ();
			// is it accepted for all policy levels ?
			IEnumerator e = SecurityManager.PolicyHierarchy ();
			while (e.MoveNext ()) {
				PolicyLevel pl = e.Current as PolicyLevel;
				UrlMembershipCondition spl = new UrlMembershipCondition ("*");
				spl.FromXml (se, pl);
				Assert.IsTrue (spl.Equals (umc), "FromXml(PolicyLevel='" + pl.Label + "')");
			}
			// yes!
		}

		[Test]
		public void ToXml_Null () 
		{
			UrlMembershipCondition umc = new UrlMembershipCondition ("http://www.example.com");
			// no ArgumentNullException here
			SecurityElement se = umc.ToXml (null);
			Assert.IsNotNull (se, "ToXml(null)");
		}

		[Test]
#if MOBILE
		[ExpectedException (typeof (NotSupportedException))]
#endif
		public void ToXml_PolicyLevel () 
		{
			UrlMembershipCondition umc = new UrlMembershipCondition ("http://www.example.com");
			SecurityElement se = umc.ToXml ();
			string s = umc.ToXml ().ToString ();
			// is it accepted for all policy levels ?
			IEnumerator e = SecurityManager.PolicyHierarchy ();
			while (e.MoveNext ()) {
				PolicyLevel pl = e.Current as PolicyLevel;
				UrlMembershipCondition spl = new UrlMembershipCondition ("*");
				spl.FromXml (se, pl);
				Assert.AreEqual (s, spl.ToXml (pl).ToString (), "ToXml(PolicyLevel='" + pl.Label + "')");
			}
			// yes!
		}

		[Test]
		public void ToFromXmlRoundTrip () 
		{
			UrlMembershipCondition umc1 = new UrlMembershipCondition ("http://www.example.com");
			SecurityElement se = umc1.ToXml ();

			UrlMembershipCondition umc2 = new UrlMembershipCondition ("*");
			umc2.FromXml (se);

			Assert.AreEqual (umc1.GetHashCode (), umc2.GetHashCode (), "ToFromXmlRoundTrip");
		}
	}
}
