//
// ApplicationDirectoryMembershipConditionTest.cs -
//	NUnit Test Cases for ApplicationDirectoryMembershipCondition
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
using System.Reflection;
using System.Security;
using System.Security.Policy;

namespace MonoTests.System.Security.Policy {

	[TestFixture]
	public class ApplicationDirectoryMembershipConditionTest {

		[Test]
		public void Constructor ()
		{
			ApplicationDirectoryMembershipCondition ad = new ApplicationDirectoryMembershipCondition ();
			Assert.IsNotNull (ad);
		}

		[Test]
		public void Check ()
		{
			ApplicationDirectoryMembershipCondition ad = new ApplicationDirectoryMembershipCondition ();
			Evidence e = null;
			Assert.IsFalse (ad.Check (e), "Check (null)");
			e = new Evidence ();
			Assert.IsFalse (ad.Check (e), "Check (empty)");
			e.AddHost (new Zone (SecurityZone.MyComputer));
			Assert.IsFalse (ad.Check (e), "Check (zone)");

			string codebase = Assembly.GetExecutingAssembly ().CodeBase;
			Url u = new Url (codebase);
			ApplicationDirectory adir = new ApplicationDirectory (codebase);

			e.AddHost (u);
			Assert.IsFalse (ad.Check (e), "Check (url-host)"); // not enough
			e.AddAssembly (adir);
			Assert.IsFalse (ad.Check (e), "Check (url-host+adir-assembly)");

			e = new Evidence ();
			e.AddHost (adir);
			Assert.IsFalse (ad.Check (e), "Check (adir-host)"); // not enough
			e.AddAssembly (u);
			Assert.IsFalse (ad.Check (e), "Check (url-assembly+adir-host)");

			e = new Evidence ();
			e.AddHost (u);
			e.AddHost (adir);
			Assert.IsTrue (ad.Check (e), "Check (url+adir host)"); // both!!
		}

		[Test]
		public void Copy ()
		{
			ApplicationDirectoryMembershipCondition ad = new ApplicationDirectoryMembershipCondition ();
			ApplicationDirectoryMembershipCondition copy = (ApplicationDirectoryMembershipCondition)ad.Copy ();
			Assert.AreEqual (ad, copy, "Equals");
			Assert.IsFalse (Object.ReferenceEquals (ad, copy), "ReferenceEquals");
		}

		[Test]
		public void Equals ()
		{
			ApplicationDirectoryMembershipCondition ad = new ApplicationDirectoryMembershipCondition ();
			Assert.IsFalse (ad.Equals (null), "Equals(null)");
			ApplicationDirectoryMembershipCondition g2 = new ApplicationDirectoryMembershipCondition ();
			Assert.IsTrue (ad.Equals (g2), "Equals(g2)");
			Assert.IsTrue (g2.Equals (ad), "Equals(ad)");
			Assert.IsFalse (ad.Equals (new object ()), "Equals (object)");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromXml_Null ()
		{
			ApplicationDirectoryMembershipCondition ad = new ApplicationDirectoryMembershipCondition ();
			ad.FromXml (null);
		}

		[Test]
		public void FromXml ()
		{
			ApplicationDirectoryMembershipCondition ad = new ApplicationDirectoryMembershipCondition ();
			SecurityElement se = ad.ToXml ();
			ad.FromXml (se);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_InvalidTag ()
		{
			ApplicationDirectoryMembershipCondition ad = new ApplicationDirectoryMembershipCondition ();
			SecurityElement se = ad.ToXml ();
			se.Tag = "IMonoship";
			ad.FromXml (se);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_WrongTagCase ()
		{
			ApplicationDirectoryMembershipCondition ad = new ApplicationDirectoryMembershipCondition ();
			SecurityElement se = ad.ToXml ();
			se.Tag = "IMEMBERSHIPCONDITION"; // instead of IMembershipCondition
			ad.FromXml (se);
		}

		[Test]
		public void FromXml_InvalidClass ()
		{
			ApplicationDirectoryMembershipCondition ad = new ApplicationDirectoryMembershipCondition ();
			SecurityElement se = ad.ToXml ();
			se.Attributes ["class"] = "Hello world";
			ad.FromXml (se);
		}

		[Test]
		public void FromXml_NoClass ()
		{
			ApplicationDirectoryMembershipCondition ad = new ApplicationDirectoryMembershipCondition ();
			SecurityElement se = ad.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("version", se.Attribute ("version"));
			ad.FromXml (w);
			// doesn't even care of the class attribute presence
		}

		[Test]
		public void FromXml_InvalidVersion ()
		{
			ApplicationDirectoryMembershipCondition ad = new ApplicationDirectoryMembershipCondition ();
			SecurityElement se = ad.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", se.Attribute ("class"));
			w.AddAttribute ("version", "2");
			ad.FromXml (w);
			// doesn't seems to care about the version number!
		}

		[Test]
		public void FromXml_NoVersion ()
		{
			ApplicationDirectoryMembershipCondition ad = new ApplicationDirectoryMembershipCondition ();
			SecurityElement se = ad.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", se.Attribute ("class"));
			ad.FromXml (w);
		}

		[Test]
#if MOBILE
		[Ignore]
#endif
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromXml_SecurityElementNull ()
		{
			ApplicationDirectoryMembershipCondition ad = new ApplicationDirectoryMembershipCondition ();
			ad.FromXml (null, PolicyLevel.CreateAppDomainLevel ());
		}

		[Test]
		public void FromXml_PolicyLevelNull ()
		{
			ApplicationDirectoryMembershipCondition ad = new ApplicationDirectoryMembershipCondition ();
			SecurityElement se = ad.ToXml ();
			ad.FromXml (se, null);
		}

		[Test]
		public void GetHashCode_ ()
		{
			ApplicationDirectoryMembershipCondition ad = new ApplicationDirectoryMembershipCondition ();
			ApplicationDirectoryMembershipCondition copy = (ApplicationDirectoryMembershipCondition)ad.Copy ();
			Assert.AreEqual (ad.GetHashCode (), copy.GetHashCode ());
		}

		[Test]
		public void ToString_ ()
		{
			ApplicationDirectoryMembershipCondition ad = new ApplicationDirectoryMembershipCondition ();
			Assert.AreEqual ("ApplicationDirectory", ad.ToString ());
		}

		[Test]
#if MOBILE
		[Ignore]
#endif
		public void ToXml ()
		{
			ApplicationDirectoryMembershipCondition ad = new ApplicationDirectoryMembershipCondition ();
			SecurityElement se = ad.ToXml ();
			Assert.AreEqual ("IMembershipCondition", se.Tag, "Tag");
			Assert.IsTrue (se.Attribute ("class").StartsWith ("System.Security.Policy.ApplicationDirectoryMembershipCondition"), "class");
			Assert.AreEqual ("1", se.Attribute ("version"), "version");
			Assert.AreEqual (se.ToString (), ad.ToXml (null).ToString (), "ToXml(null)");
			Assert.AreEqual (se.ToString (), ad.ToXml (PolicyLevel.CreateAppDomainLevel ()).ToString (), "ToXml(PolicyLevel)");
		}
	}
}
