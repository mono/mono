//
// GacMembershipConditionTest.cs - NUnit Test Cases for GacMembershipCondition
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

#if NET_2_0

using NUnit.Framework;
using System;
using System.Security;
using System.Security.Policy;

namespace MonoTests.System.Security.Policy {

	[TestFixture]
	public class GacMembershipConditionTest	{

		[Test]
		public void Constructor ()
		{
			GacMembershipCondition gac = new GacMembershipCondition ();
			Assert.IsNotNull (gac);
		}

		[Test]
		public void Check ()
		{
			GacMembershipCondition gac = new GacMembershipCondition ();
			Evidence e = null;
			Assert.IsFalse (gac.Check (e), "Check (null)");
			e = new Evidence ();
			Assert.IsFalse (gac.Check (e), "Check (empty)");
			e.AddHost (new Zone (SecurityZone.MyComputer));
			Assert.IsFalse (gac.Check (e), "Check (zone)");
			GacInstalled g = new GacInstalled ();
			e.AddAssembly (g);
			Assert.IsFalse (gac.Check (e), "Check (gac-assembly)");
			e.AddHost (g);
			Assert.IsTrue (gac.Check (e), "Check (gac-host)");
		}

		[Test]
		public void Copy ()
		{
			GacMembershipCondition gac = new GacMembershipCondition ();
			GacMembershipCondition copy = (GacMembershipCondition) gac.Copy ();
			Assert.AreEqual (gac, copy, "Equals");
			Assert.IsFalse (Object.ReferenceEquals (gac, copy), "ReferenceEquals");
		}

		[Test]
		public void Equals ()
		{
			GacMembershipCondition gac = new GacMembershipCondition ();
			Assert.IsFalse (gac.Equals (null), "Equals(null)");
			GacMembershipCondition g2 = new GacMembershipCondition ();
			Assert.IsTrue (gac.Equals (g2), "Equals(g2)");
			Assert.IsTrue (g2.Equals (gac), "Equals(gac)");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromXml_Null () 
		{
			GacMembershipCondition gac = new GacMembershipCondition ();
			gac.FromXml (null);
		}

		[Test]
		public void FromXml ()
		{
			GacMembershipCondition gac = new GacMembershipCondition ();
			SecurityElement se = gac.ToXml ();
			gac.FromXml (se);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_InvalidTag ()
		{
			GacMembershipCondition gac = new GacMembershipCondition ();
			SecurityElement se = gac.ToXml ();
			se.Tag = "IMonoship";
			gac.FromXml (se);
		}

		[Test]
		public void FromXml_InvalidClass ()
		{
			GacMembershipCondition gac = new GacMembershipCondition ();
			SecurityElement se = gac.ToXml ();
			se.Attributes ["class"] = "Hello world";
			gac.FromXml (se);
		}

		[Test]
		public void FromXml_NoClass ()
		{
			GacMembershipCondition gac = new GacMembershipCondition ();
			SecurityElement se = gac.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("version", se.Attribute ("version"));
			gac.FromXml (w);
			// doesn't even care of the class attribute presence
		}

		[Test]
		public void FromXml_InvalidVersion ()
		{
			GacMembershipCondition gac = new GacMembershipCondition ();
			SecurityElement se = gac.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", se.Attribute ("class"));
			w.AddAttribute ("version", "2");
			gac.FromXml (w);
		}

		[Test]
		public void FromXml_NoVersion ()
		{
			GacMembershipCondition gac = new GacMembershipCondition ();
			SecurityElement se = gac.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", se.Attribute ("class"));
			gac.FromXml (w);
		}

		[Test]
#if MOBILE
		[Ignore]
#endif
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromXml_SecurityElementNull ()
		{
			GacMembershipCondition gac = new GacMembershipCondition ();
			gac.FromXml (null, PolicyLevel.CreateAppDomainLevel ());
		}

		[Test]
		public void FromXml_PolicyLevelNull ()
		{
			GacMembershipCondition gac = new GacMembershipCondition ();
			SecurityElement se = gac.ToXml ();
			gac.FromXml (se, null);
		}

		[Test]
		public void GetHashCode_ ()
		{
			GacMembershipCondition gac = new GacMembershipCondition ();
			Assert.AreEqual (0, gac.GetHashCode ());
		}

		[Test]
		public void ToString_ ()
		{
			GacMembershipCondition gac = new GacMembershipCondition ();
			Assert.AreEqual ("GAC", gac.ToString ());
		}

		[Test]
#if MOBILE
		[Ignore]
#endif
		public void ToXml ()
		{
			GacMembershipCondition gac = new GacMembershipCondition ();
			SecurityElement se = gac.ToXml ();
			Assert.AreEqual ("IMembershipCondition", se.Tag, "Tag");
			Assert.IsTrue (se.Attribute ("class").StartsWith ("System.Security.Policy.GacMembershipCondition"), "class");
			Assert.AreEqual ("1", se.Attribute ("version"), "version");
			Assert.AreEqual (se.ToString (), gac.ToXml (null).ToString (), "ToXml(null)");
			Assert.AreEqual (se.ToString (), gac.ToXml (PolicyLevel.CreateAppDomainLevel ()).ToString (), "ToXml(PolicyLevel)");
		}
	}
}

#endif
