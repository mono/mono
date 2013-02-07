//
// AllMembershipConditionTest.cs - NUnit Test Cases for AllMembershipCondition
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
using System.Security;
using System.Security.Policy;

namespace MonoTests.System.Security.Policy {

	[TestFixture]
	public class AllMembershipConditionTest {

		[Test]
		public void Constructor ()
		{
			AllMembershipCondition all = new AllMembershipCondition ();
			Assert.IsNotNull (all);
		}

		[Test]
		public void Check ()
		{
			AllMembershipCondition all = new AllMembershipCondition ();
			Evidence e = null;
			Assert.IsTrue (all.Check (e), "Check (null)");
			e = new Evidence ();
			Assert.IsTrue (all.Check (e), "Check (empty)");
			e.AddHost (new Zone (SecurityZone.MyComputer));
			Assert.IsTrue (all.Check (e), "Check (zone)");
			Url u = new Url ("http://www.go-mono.com/");
			e.AddAssembly (u);
			Assert.IsTrue (all.Check (e), "Check (all-assembly)");
			Site s = new Site ("www.go-mono.com");
			e.AddHost (s);
			Assert.IsTrue (all.Check (e), "Check (all-host)");
		}

		[Test]
		public void Copy ()
		{
			AllMembershipCondition all = new AllMembershipCondition ();
			AllMembershipCondition copy = (AllMembershipCondition)all.Copy ();
			Assert.AreEqual (all, copy, "Equals");
			Assert.IsFalse (Object.ReferenceEquals (all, copy), "ReferenceEquals");
		}

		[Test]
		public void Equals ()
		{
			AllMembershipCondition all = new AllMembershipCondition ();
			Assert.IsFalse (all.Equals (null), "Equals(null)");
			AllMembershipCondition g2 = new AllMembershipCondition ();
			Assert.IsTrue (all.Equals (g2), "Equals(g2)");
			Assert.IsTrue (g2.Equals (all), "Equals(all)");
			Assert.IsFalse (all.Equals (new object ()), "Equals (object)");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromXml_Null ()
		{
			AllMembershipCondition all = new AllMembershipCondition ();
			all.FromXml (null);
		}

		[Test]
		public void FromXml ()
		{
			AllMembershipCondition all = new AllMembershipCondition ();
			SecurityElement se = all.ToXml ();
			all.FromXml (se);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_InvalidTag ()
		{
			AllMembershipCondition all = new AllMembershipCondition ();
			SecurityElement se = all.ToXml ();
			se.Tag = "IMonoship";
			all.FromXml (se);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_WrongTagCase ()
		{
			AllMembershipCondition all = new AllMembershipCondition ();
			SecurityElement se = all.ToXml ();
			se.Tag = "IMEMBERSHIPCONDITION"; // instead of IMembershipCondition
			all.FromXml (se);
		}

		[Test]
		public void FromXml_InvalidClass ()
		{
			AllMembershipCondition all = new AllMembershipCondition ();
			SecurityElement se = all.ToXml ();
			se.Attributes ["class"] = "Hello world";
			all.FromXml (se);
		}

		[Test]
		public void FromXml_NoClass ()
		{
			AllMembershipCondition all = new AllMembershipCondition ();
			SecurityElement se = all.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("version", se.Attribute ("version"));
			all.FromXml (w);
			// doesn't even care of the class attribute presence
		}

		[Test]
		public void FromXml_InvalidVersion ()
		{
			AllMembershipCondition all = new AllMembershipCondition ();
			SecurityElement se = all.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", se.Attribute ("class"));
			w.AddAttribute ("version", "2");
			all.FromXml (w);
			// doesn't seems to care about the version number!
		}

		[Test]
		public void FromXml_NoVersion ()
		{
			AllMembershipCondition all = new AllMembershipCondition ();
			SecurityElement se = all.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", se.Attribute ("class"));
			all.FromXml (w);
		}

		[Test]
#if MOBILE
		[Ignore]
#endif
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromXml_SecurityElementNull ()
		{
			AllMembershipCondition all = new AllMembershipCondition ();
			all.FromXml (null, PolicyLevel.CreateAppDomainLevel ());
		}

		[Test]
		public void FromXml_PolicyLevelNull ()
		{
			AllMembershipCondition all = new AllMembershipCondition ();
			SecurityElement se = all.ToXml ();
			all.FromXml (se, null);
		}

		[Test]
		public void GetHashCode_ ()
		{
			AllMembershipCondition all = new AllMembershipCondition ();
			AllMembershipCondition copy = (AllMembershipCondition)all.Copy ();
			Assert.AreEqual (all.GetHashCode (), copy.GetHashCode ());
		}

		[Test]
		public void ToString_ ()
		{
			AllMembershipCondition all = new AllMembershipCondition ();
			Assert.AreEqual ("All code", all.ToString ());
		}

		[Test]
#if MOBILE
		[Ignore]
#endif
		public void ToXml ()
		{
			AllMembershipCondition all = new AllMembershipCondition ();
			SecurityElement se = all.ToXml ();
			Assert.AreEqual ("IMembershipCondition", se.Tag, "Tag");
			Assert.IsTrue (se.Attribute ("class").StartsWith ("System.Security.Policy.AllMembershipCondition"), "class");
			Assert.AreEqual ("1", se.Attribute ("version"), "version");
			Assert.AreEqual (se.ToString (), all.ToXml (null).ToString (), "ToXml(null)");
			Assert.AreEqual (se.ToString (), all.ToXml (PolicyLevel.CreateAppDomainLevel ()).ToString (), "ToXml(PolicyLevel)");
		}
	}
}
