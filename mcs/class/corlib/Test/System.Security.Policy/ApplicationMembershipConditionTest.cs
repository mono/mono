//
// ApplicationMembershipConditionTest.cs -
//	NUnit Test Cases for ApplicationMembershipCondition
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
using System.Reflection;
using System.Security;
using System.Security.Policy;

namespace MonoTests.System.Security.Policy {

	[TestFixture]
	public class ApplicationMembershipConditionTest {

		[Test]
		public void Constructor ()
		{
			ApplicationMembershipCondition app = new ApplicationMembershipCondition ();
			Assert.IsNotNull (app);
		}

		[Test]
		public void Check ()
		{
			ApplicationMembershipCondition app = new ApplicationMembershipCondition ();
			Evidence e = null;
			Assert.IsFalse (app.Check (e), "Check (null)");
			e = new Evidence ();
			Assert.IsFalse (app.Check (e), "Check (empty)");
			e.AddHost (new Zone (SecurityZone.MyComputer));
			Assert.IsFalse (app.Check (e), "Check (zone)");

			// TODO - more (non failing ;) tests
		}

		[Test]
		public void Copy ()
		{
			ApplicationMembershipCondition app = new ApplicationMembershipCondition ();
			ApplicationMembershipCondition copy = (ApplicationMembershipCondition)app.Copy ();
			Assert.AreEqual (app, copy, "Equals");
			Assert.IsFalse (Object.ReferenceEquals (app, copy), "ReferenceEquals");
		}

		[Test]
		public void Equals ()
		{
			ApplicationMembershipCondition app = new ApplicationMembershipCondition ();
			Assert.IsFalse (app.Equals (null), "Equals(null)");
			ApplicationMembershipCondition g2 = new ApplicationMembershipCondition ();
			Assert.IsTrue (app.Equals (g2), "Equals(g2)");
			Assert.IsTrue (g2.Equals (app), "Equals(app)");
			Assert.IsFalse (app.Equals (new object ()), "Equals (object)");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromXml_Null ()
		{
			ApplicationMembershipCondition app = new ApplicationMembershipCondition ();
			app.FromXml (null);
		}

		[Test]
		public void FromXml ()
		{
			ApplicationMembershipCondition app = new ApplicationMembershipCondition ();
			SecurityElement se = app.ToXml ();
			app.FromXml (se);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_InvalidTag ()
		{
			ApplicationMembershipCondition app = new ApplicationMembershipCondition ();
			SecurityElement se = app.ToXml ();
			se.Tag = "IMonoship";
			app.FromXml (se);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_WrongTagCase ()
		{
			ApplicationMembershipCondition app = new ApplicationMembershipCondition ();
			SecurityElement se = app.ToXml ();
			se.Tag = "IMEMBERSHIPCONDITION"; // insteapp of IMembershipCondition
			app.FromXml (se);
		}

		[Test]
		public void FromXml_InvalidClass ()
		{
			ApplicationMembershipCondition app = new ApplicationMembershipCondition ();
			SecurityElement se = app.ToXml ();
			se.Attributes ["class"] = "Hello world";
			app.FromXml (se);
		}

		[Test]
		public void FromXml_NoClass ()
		{
			ApplicationMembershipCondition app = new ApplicationMembershipCondition ();
			SecurityElement se = app.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("version", se.Attribute ("version"));
			app.FromXml (w);
			// doesn't even care of the class attribute presence
		}

		[Test]
		public void FromXml_InvalidVersion ()
		{
			ApplicationMembershipCondition app = new ApplicationMembershipCondition ();
			SecurityElement se = app.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", se.Attribute ("class"));
			w.AddAttribute ("version", "2");
			app.FromXml (w);
			// doesn't seems to care about the version number!
		}

		[Test]
		public void FromXml_NoVersion ()
		{
			ApplicationMembershipCondition app = new ApplicationMembershipCondition ();
			SecurityElement se = app.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", se.Attribute ("class"));
			app.FromXml (w);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromXml_SecurityElementNull ()
		{
			ApplicationMembershipCondition app = new ApplicationMembershipCondition ();
			app.FromXml (null, PolicyLevel.CreateAppDomainLevel ());
		}

		[Test]
		public void FromXml_NonBooleanLookAtDir ()
		{
			ApplicationMembershipCondition app = new ApplicationMembershipCondition ();
			SecurityElement se = app.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", se.Attribute ("class"));
			w.AddAttribute ("version", se.Attribute ("version"));
			w.AddAttribute ("LookAtDir", "Maybe"); // not (generally) a boolean ;)

			ApplicationMembershipCondition app2 = new ApplicationMembershipCondition ();
			app2.FromXml (w);

			se = app2.ToXml ();
			Assert.IsNull (se.Attribute ("LookAtDir"), "LookAtDir");
			// LookAtDir isn't part of the Equals computation
			Assert.IsTrue (app2.Equals (app), "Equals-1");
			Assert.IsTrue (app.Equals (app2), "Equals-2");

			ApplicationMembershipCondition app3 = (ApplicationMembershipCondition)app2.Copy ();
			se = app3.ToXml ();
			// LookAtDir isn't copied either
			Assert.AreEqual ("true", se.Attribute ("LookAtDir"), "Copy-LookAtDir");
		}

		[Test]
		public void FromXml_PolicyLevelNull ()
		{
			ApplicationMembershipCondition app = new ApplicationMembershipCondition ();
			SecurityElement se = app.ToXml ();
			app.FromXml (se, null);
		}

		[Test]
		public void GetHashCode_ ()
		{
			ApplicationMembershipCondition app = new ApplicationMembershipCondition ();
			Assert.AreEqual (-1, app.GetHashCode ());
			ApplicationMembershipCondition copy = (ApplicationMembershipCondition)app.Copy ();
			Assert.AreEqual (app.GetHashCode (), copy.GetHashCode ());
		}

		[Test]
		public void ToString_ ()
		{
			ApplicationMembershipCondition app = new ApplicationMembershipCondition ();
			Assert.AreEqual ("Application", app.ToString ());
		}

		[Test]
		public void ToXml ()
		{
			ApplicationMembershipCondition app = new ApplicationMembershipCondition ();
			SecurityElement se = app.ToXml ();
			Assert.AreEqual ("IMembershipCondition", se.Tag, "Tag");
			Assert.IsTrue (se.Attribute ("class").StartsWith ("System.Security.Policy.ApplicationMembershipCondition"), "class");
			Assert.AreEqual ("1", se.Attribute ("version"), "version");
			Assert.AreEqual ("true", se.Attribute ("LookAtDir"), "LookAtDir");
			Assert.AreEqual (se.ToString (), app.ToXml (null).ToString (), "ToXml(null)");
			Assert.AreEqual (se.ToString (), app.ToXml (PolicyLevel.CreateAppDomainLevel ()).ToString (), "ToXml(PolicyLevel)");
		}
	}
}

#endif
