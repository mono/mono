//
// SiteMembershipConditionTest.cs - 
//	NUnit Test Cases for SiteMembershipCondition
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
	public class SiteMembershipConditionTest {

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
			Assert.AreEqual ("www.go-mono.com", smc.Site, "Site");
			Assert.AreEqual ("Site - www.go-mono.com", smc.ToString (), "ToString");

			SiteMembershipCondition smc2 = (SiteMembershipCondition) smc.Copy ();
			Assert.AreEqual (smc.Site, smc2.Site, "Copy.Site");
			Assert.AreEqual (smc.GetHashCode (), smc2.GetHashCode (), "Copy.GetHashCode");

			SecurityElement se = smc2.ToXml ();
			SiteMembershipCondition smc3 = new SiteMembershipCondition ("*");
			smc3.FromXml (se);
			Assert.AreEqual (smc.Site, smc3.Site, "ToXml/FromXml");

			Assert.IsTrue (smc.Equals (smc2), "Equals");
			SiteMembershipCondition smc4 = new SiteMembershipCondition ("go-mono.com");
			Assert.IsFalse (smc.Equals (smc4), "!Equals");
		}

		[Test]
		public void Site_AllGoMonoSite () 
		{
			SiteMembershipCondition smc = new SiteMembershipCondition ("*.go-mono.com");
			Assert.AreEqual ("*.go-mono.com", smc.Site, "Site");
			Assert.AreEqual ("Site - *.go-mono.com", smc.ToString (), "ToString");

			SiteMembershipCondition smc2 = (SiteMembershipCondition) smc.Copy ();
			Assert.AreEqual (smc.Site, smc2.Site, "Copy.Site");
			Assert.AreEqual (smc.GetHashCode (), smc2.GetHashCode (), "Copy.GetHashCode");

			SecurityElement se = smc2.ToXml ();
			SiteMembershipCondition smc3 = new SiteMembershipCondition ("*");
			smc3.FromXml (se);
			Assert.AreEqual (smc.Site, smc3.Site, "ToXml/FromXml");

			Assert.IsTrue (smc.Equals (smc2), "Equals");
			SiteMembershipCondition smc4 = new SiteMembershipCondition ("go-mono.com");
			Assert.IsFalse (smc.Equals (smc4), "!Equals");
		}

		[Test]
		public void Check () 
		{
			SiteMembershipCondition smc = new SiteMembershipCondition ("*.go-mono.com");

			Evidence e = null;
			Assert.IsFalse (smc.Check (e), "Check(null)");
			e = new Evidence ();
			Assert.IsFalse (smc.Check (e), "Check (empty)");
			e.AddHost (new Zone (SecurityZone.MyComputer));
			Assert.IsFalse (smc.Check (e), "Check (zone)");
			
			Site s = new Site ("*.go-mono.com");
			e.AddAssembly (s);
			Assert.IsFalse (smc.Check (e), "Check (site-assembly)");
			e.AddHost (s);
			Assert.IsTrue (smc.Check (e), "Check (site-host)");

			e = new Evidence ();
			e.AddHost (new Site ("www.go-mono.com"));
			Assert.IsTrue (smc.Check (e), "Check(+-)");

			e = new Evidence ();
			e.AddHost (new Site ("*.go-mono.org"));
			Assert.IsFalse (smc.Check (e), "Check(-)");
		}

		[Test]
		public void Equals () 
		{
			SiteMembershipCondition smc1 = new SiteMembershipCondition ("*.go-mono.com");
			Assert.IsFalse (smc1.Equals (null), "Null");
			SiteMembershipCondition smc2 = new SiteMembershipCondition ("*.Go-Mono.com");
			Assert.IsTrue (smc1.Equals (smc2), "CaseSensitive");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromXml_Null () 
		{
			SiteMembershipCondition smc = new SiteMembershipCondition ("*.go-mono.com");
			smc.FromXml (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_InvalidTag () 
		{
			SiteMembershipCondition smc = new SiteMembershipCondition ("*.go-mono.com");
			SecurityElement se = smc.ToXml ();
			se.Tag = "IMonoship";
			smc.FromXml (se);
		}

		[Test]
		public void FromXml_InvalidClass ()
		{
			SiteMembershipCondition smc = new SiteMembershipCondition ("*.go-mono.com");
			SecurityElement se = smc.ToXml ();
			se.Attributes ["class"] = "Hello world";
			smc.FromXml (se);
		}

		[Test]
		public void FromXml_NoClass ()
		{
			SiteMembershipCondition smc = new SiteMembershipCondition ("*.go-mono.com");
			SecurityElement se = smc.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("version", se.Attribute ("version"));
			smc.FromXml (w);
			// doesn't even care of the class attribute presence
		}

		[Test]
		public void FromXml_InvalidVersion ()
		{
			SiteMembershipCondition smc = new SiteMembershipCondition ("*.go-mono.com");
			SecurityElement se = smc.ToXml ();
			se.Attributes ["version"] = "2";
			smc.FromXml (se);
		}

		[Test]
		public void FromXml_NoVersion ()
		{
			SiteMembershipCondition smc = new SiteMembershipCondition ("*.go-mono.com");
			SecurityElement se = smc.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", se.Attribute ("class"));
			smc.FromXml (w);
		}

		[Test]
#if MOBILE
		[ExpectedException (typeof (NotSupportedException))]
#endif
		public void FromXml_PolicyLevel () 
		{
			SiteMembershipCondition smc = new SiteMembershipCondition ("*.go-mono.com");
			SecurityElement se = smc.ToXml ();
			// is it accepted for all policy levels ?
			IEnumerator e = SecurityManager.PolicyHierarchy ();
			while (e.MoveNext ()) {
				PolicyLevel pl = e.Current as PolicyLevel;
				SiteMembershipCondition spl = new SiteMembershipCondition ("*");
				spl.FromXml (se, pl);
				Assert.IsTrue (spl.Equals (smc), "FromXml(PolicyLevel='" + pl.Label + "')");
			}
			// yes!
		}

		[Test]
		public void ToXml_Null () 
		{
			SiteMembershipCondition smc = new SiteMembershipCondition ("*.go-mono.com");
			// no ArgumentNullException here
			SecurityElement se = smc.ToXml (null);
			Assert.IsNotNull (se, "ToXml(null)");
		}

		[Test]
#if MOBILE
		[ExpectedException (typeof (NotSupportedException))]
#endif
		public void ToXml_PolicyLevel () 
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
				Assert.AreEqual (s, spl.ToXml (pl).ToString (), "ToXml(PolicyLevel='" + pl.Label + "')");
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

			Assert.AreEqual (smc1.GetHashCode (), smc2.GetHashCode (), "ToFromXmlRoundTrip");
		}
	}
}
