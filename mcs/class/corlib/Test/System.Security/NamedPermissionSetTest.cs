//
// NamedPermissionSetTest.cs - NUnit Test Cases for NamedPermissionSet
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using System;
using System.Security;
using System.Security.Permissions;

namespace MonoTests.System.Security {

	[TestFixture]
	public class NamedPermissionSetTest : Assertion {

		private static string name = "mono";
		private static string sentinel = "go mono!";

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConstructorNameNull () 
		{
			string s = null; // we don't want to confuse the compiler
			NamedPermissionSet nps = new NamedPermissionSet (s);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConstructorNameEmpty () 
		{
			NamedPermissionSet nps = new NamedPermissionSet ("");
		}

		[Test]
		public void Description () 
		{
			NamedPermissionSet nps = new NamedPermissionSet (name);
			// null by default (not empty)
			AssertNull ("Description", nps.Description);
			// is null-able (without exception)
			nps.Description = null;
			AssertNull ("Description(null)", nps.Description);
			nps.Description = sentinel;
			AssertEquals ("Description", sentinel, nps.Description);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void NameNull () 
		{
			NamedPermissionSet nps = new NamedPermissionSet (name);
			nps.Name = null;
			// strangely this isn't a ArgumentNullException (but so says the doc)
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void NameEmpty () 
		{
			NamedPermissionSet nps = new NamedPermissionSet (name);
			nps.Name = "";
		}

		[Test]
		public void Name () 
		{
			NamedPermissionSet nps = new NamedPermissionSet (name);
			nps.Name = sentinel;
			AssertEquals ("Name", sentinel, nps.Name);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromXmlNull () 
		{
			NamedPermissionSet nps = new NamedPermissionSet (name, PermissionState.None);
			nps.FromXml (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXmlInvalidPermission () 
		{
			NamedPermissionSet nps = new NamedPermissionSet (name, PermissionState.None);
			SecurityElement se = nps.ToXml ();
			// can't modify - so we create our own
			SecurityElement se2 = new SecurityElement ("InvalidPermissionSet", se.Text);
			se2.AddAttribute ("class", se.Attribute ("class"));
			se2.AddAttribute ("version", se.Attribute ("version"));
			se2.AddAttribute ("Name", se.Attribute ("Name"));
			nps.FromXml (se2);
		}

		[Test]
		// [ExpectedException (typeof (ArgumentException))]
		public void FromXmlWrongVersion () 
		{
			NamedPermissionSet nps = new NamedPermissionSet (name, PermissionState.None);
			SecurityElement se = nps.ToXml ();
			// can't modify - so we create our own
			SecurityElement se2 = new SecurityElement (se.Tag, se.Text);
			se2.AddAttribute ("class", se.Attribute ("class"));
			se2.AddAttribute ("version", "2");
			se2.AddAttribute ("Name", se.Attribute ("Name"));
			nps.FromXml (se2);
			// wow - here we accept a version 2 !!!
		}

		[Test]
		public void FromXml () 
		{
			NamedPermissionSet nps = new NamedPermissionSet (name, PermissionState.None);
			SecurityElement se = nps.ToXml ();
			AssertNotNull ("ToXml()", se);

			NamedPermissionSet nps2 = (NamedPermissionSet) nps.Copy ();
			nps2.FromXml (se);
			AssertEquals ("FromXml-Copy.Name", name, nps2.Name);
			// strangely it's empty when converted from XML (but null when created)
			AssertEquals ("FromXml-Copy.Description", "", nps2.Description);
			Assert ("FromXml-Copy.IsUnrestricted", !nps2.IsUnrestricted ()); 

			se.AddAttribute ("Description", sentinel);
			nps2.FromXml (se);
			AssertEquals ("FromXml-Add1.Name", name, nps2.Name);
			AssertEquals ("FromXml-Add1.Description", sentinel, nps2.Description);
			Assert ("FromXml-Add1.IsUnrestricted", !nps2.IsUnrestricted ()); 

			se.AddAttribute ("Unrestricted", "true");
			nps2.FromXml (se);
			AssertEquals ("FromXml-Add2.Name", name, nps2.Name);
			AssertEquals ("FromXml-Add2.Description", sentinel, nps2.Description);
			Assert ("FromXml-Add2.IsUnrestricted", nps2.IsUnrestricted ()); 
		}

		[Test]
		public void ToXmlNone () 
		{
			NamedPermissionSet ps = new NamedPermissionSet (name, PermissionState.None);
			ps.Description = sentinel;
			SecurityElement se = ps.ToXml ();
			Assert ("None.ToString().StartsWith", ps.ToString().StartsWith ("<PermissionSet"));
			AssertEquals ("None.class", "System.Security.NamedPermissionSet", (se.Attributes ["class"] as string));
			AssertEquals ("None.version", "1", (se.Attributes ["version"] as string));
			AssertEquals ("None.Name", name, (se.Attributes ["Name"] as string));
			AssertEquals ("None.Description", sentinel, (se.Attributes ["Description"] as string));
			AssertNull ("None.Unrestricted", (se.Attributes ["Unrestricted"] as string));
		}

		[Test]
		public void ToXmlUnrestricted () 
		{
			NamedPermissionSet ps = new NamedPermissionSet (name, PermissionState.Unrestricted);
			SecurityElement se = ps.ToXml ();
			Assert ("Unrestricted.ToString().StartsWith", ps.ToString().StartsWith ("<PermissionSet"));
			AssertEquals ("Unrestricted.class", "System.Security.NamedPermissionSet", (se.Attributes ["class"] as string));
			AssertEquals ("Unrestricted.version", "1", (se.Attributes ["version"] as string));
			AssertEquals ("Unrestricted.Name", name, (se.Attributes ["Name"] as string));
			AssertNull ("Unrestricted.Description", (se.Attributes ["Description"] as string));
			AssertEquals ("Unrestricted.Unrestricted", "true", (se.Attributes ["Unrestricted"] as string));
		}
	}
}
