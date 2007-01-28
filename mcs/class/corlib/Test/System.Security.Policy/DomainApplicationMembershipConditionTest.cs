//
// DomainApplicationMembershipConditionTest.cs -
//	NUnit Test Cases for DomainApplicationMembershipCondition
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

#if NET_2_0 && !TARGET_JVM

using NUnit.Framework;
using System;
using System.Reflection;
using System.Security;
using System.Security.Policy;

namespace MonoTests.System.Security.Policy {

	[TestFixture]
	public class DomainApplicationMembershipConditionTest {

		[Test]
		public void Constructor ()
		{
			DomainApplicationMembershipCondition domapp = new DomainApplicationMembershipCondition ();
			Assert.IsNotNull (domapp);
		}

		[Test]
		public void Check ()
		{
			DomainApplicationMembershipCondition domapp = new DomainApplicationMembershipCondition ();
			Evidence e = null;
			Assert.IsFalse (domapp.Check (e), "Check (null)");
			e = new Evidence ();
			Assert.IsFalse (domapp.Check (e), "Check (empty)");
			e.AddHost (new Zone (SecurityZone.MyComputer));
			Assert.IsFalse (domapp.Check (e), "Check (zone)");

			// TODO - more (non failing ;) tests
		}

		[Test]
		public void Copy ()
		{
			DomainApplicationMembershipCondition domapp = new DomainApplicationMembershipCondition ();
			DomainApplicationMembershipCondition copy = (DomainApplicationMembershipCondition)domapp.Copy ();
			Assert.AreEqual (domapp, copy, "Equals");
			Assert.IsFalse (Object.ReferenceEquals (domapp, copy), "ReferenceEquals");
		}

		[Test]
		public void Equals ()
		{
			DomainApplicationMembershipCondition domapp = new DomainApplicationMembershipCondition ();
			Assert.IsFalse (domapp.Equals (null), "Equals(null)");
			DomainApplicationMembershipCondition g2 = new DomainApplicationMembershipCondition ();
			Assert.IsTrue (domapp.Equals (g2), "Equals(g2)");
			Assert.IsTrue (g2.Equals (domapp), "Equals(domapp)");
			Assert.IsFalse (domapp.Equals (new object ()), "Equals (object)");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromXml_Null ()
		{
			DomainApplicationMembershipCondition domapp = new DomainApplicationMembershipCondition ();
			domapp.FromXml (null);
		}

		[Test]
		public void FromXml ()
		{
			DomainApplicationMembershipCondition domapp = new DomainApplicationMembershipCondition ();
			SecurityElement se = domapp.ToXml ();
			domapp.FromXml (se);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_InvalidTag ()
		{
			DomainApplicationMembershipCondition domapp = new DomainApplicationMembershipCondition ();
			SecurityElement se = domapp.ToXml ();
			se.Tag = "IMonoship";
			domapp.FromXml (se);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_WrongTagCase ()
		{
			DomainApplicationMembershipCondition domapp = new DomainApplicationMembershipCondition ();
			SecurityElement se = domapp.ToXml ();
			se.Tag = "IMEMBERSHIPCONDITION"; // instedomapp of IMembershipCondition
			domapp.FromXml (se);
		}

		[Test]
		public void FromXml_InvalidClass ()
		{
			DomainApplicationMembershipCondition domapp = new DomainApplicationMembershipCondition ();
			SecurityElement se = domapp.ToXml ();
			se.Attributes ["class"] = "Hello world";
			domapp.FromXml (se);
		}

		[Test]
		public void FromXml_NoClass ()
		{
			DomainApplicationMembershipCondition domapp = new DomainApplicationMembershipCondition ();
			SecurityElement se = domapp.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("version", se.Attribute ("version"));
			domapp.FromXml (w);
			// doesn't even care of the class attribute presence
		}

		[Test]
		public void FromXml_InvalidVersion ()
		{
			DomainApplicationMembershipCondition domapp = new DomainApplicationMembershipCondition ();
			SecurityElement se = domapp.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", se.Attribute ("class"));
			w.AddAttribute ("version", "2");
			domapp.FromXml (w);
			// doesn't seems to care about the version number!
		}

		[Test]
		public void FromXml_NoVersion ()
		{
			DomainApplicationMembershipCondition domapp = new DomainApplicationMembershipCondition ();
			SecurityElement se = domapp.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", se.Attribute ("class"));
			domapp.FromXml (w);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromXml_SecurityElementNull ()
		{
			DomainApplicationMembershipCondition domapp = new DomainApplicationMembershipCondition ();
			domapp.FromXml (null, PolicyLevel.CreateAppDomainLevel ());
		}

		[Test]
		public void FromXml_PolicyLevelNull ()
		{
			DomainApplicationMembershipCondition domapp = new DomainApplicationMembershipCondition ();
			SecurityElement se = domapp.ToXml ();
			domapp.FromXml (se, null);
		}

		[Test]
		public void GetHashCode_ ()
		{
			DomainApplicationMembershipCondition domapp = new DomainApplicationMembershipCondition ();
			Assert.AreEqual (-1, domapp.GetHashCode ());
			DomainApplicationMembershipCondition copy = (DomainApplicationMembershipCondition)domapp.Copy ();
			Assert.AreEqual (domapp.GetHashCode (), copy.GetHashCode ());
		}

		[Test]
		public void ToString_ ()
		{
			DomainApplicationMembershipCondition domapp = new DomainApplicationMembershipCondition ();
			Assert.AreEqual ("Domain", domapp.ToString ());
		}

		[Test]
		public void ToXml ()
		{
			DomainApplicationMembershipCondition domapp = new DomainApplicationMembershipCondition ();
			SecurityElement se = domapp.ToXml ();
			Assert.AreEqual ("IMembershipCondition", se.Tag, "Tag");
			Assert.IsTrue (se.Attribute ("class").StartsWith ("System.Security.Policy.DomainApplicationMembershipCondition"), "class");
			Assert.AreEqual ("1", se.Attribute ("version"), "version");
			Assert.AreEqual (se.ToString (), domapp.ToXml (null).ToString (), "ToXml(null)");
			Assert.AreEqual (se.ToString (), domapp.ToXml (PolicyLevel.CreateAppDomainLevel ()).ToString (), "ToXml(PolicyLevel)");
		}
	}
}

#endif
