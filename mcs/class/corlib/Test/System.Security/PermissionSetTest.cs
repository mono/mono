//
// PermissionSetTest.cs - NUnit Test Cases for PermissionSet
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
	public class PermissionSetTest : Assertion {

		[Test]
		public void PermissionStateNone () 
		{
			PermissionSet ps = new PermissionSet (PermissionState.None);
			Assert ("PermissionStateNone.IsUnrestricted", !ps.IsUnrestricted ());
			Assert ("PermissionStateNone.IsEmpty", ps.IsEmpty ());
			Assert ("PermissionStateNone.IsReadOnly", !ps.IsReadOnly);
			AssertEquals ("PermissionStateNone.ToXml().ToString()==ToString()", ps.ToXml ().ToString (), ps.ToString ());
		}

		[Test]
		public void PermissionStateUnrestricted () 
		{
			PermissionSet ps = new PermissionSet (PermissionState.Unrestricted);
			Assert ("PermissionStateUnrestricted.IsUnrestricted", ps.IsUnrestricted ());
			Assert ("PermissionStateUnrestricted.IsEmpty", !ps.IsEmpty ());
			Assert ("PermissionStateUnrestricted.IsReadOnly", !ps.IsReadOnly);
			AssertEquals ("PermissionStateUnrestricted.ToXml().ToString()==ToString()", ps.ToXml ().ToString (), ps.ToString ());
		}

		[Test]
		public void PermissionSetNull () 
		{
			// no exception is thrown
			PermissionSet ps = new PermissionSet (null);
			Assert ("PermissionStateNull.IsUnrestricted", ps.IsUnrestricted ());
			Assert ("PermissionStateNull.IsEmpty", !ps.IsEmpty ());
			Assert ("PermissionStateNull.IsReadOnly", !ps.IsReadOnly);
			AssertEquals ("PermissionStateNull.ToXml().ToString()==ToString()", ps.ToXml ().ToString (), ps.ToString ());
		}

		[Test]
		public void PermissionSetPermissionSet () 
		{
			FileDialogPermission fdp = new FileDialogPermission (FileDialogPermissionAccess.Open);
			PermissionSet ps1 = new PermissionSet (PermissionState.None);
			ps1.AddPermission (fdp);
			Assert ("ps1.IsEmpty", !ps1.IsEmpty ());

			PermissionSet ps = new PermissionSet (ps1);
			Assert ("PermissionSetPermissionSet.IsUnrestricted", !ps.IsUnrestricted ());
			Assert ("PermissionSetPermissionSet.IsEmpty", !ps.IsEmpty ());
			Assert ("PermissionSetPermissionSet.IsReadOnly", !ps.IsReadOnly);
			AssertEquals ("PermissionSetPermissionSet.ToXml().ToString()==ToString()", ps.ToXml ().ToString (), ps.ToString ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromXmlNull () 
		{
			PermissionSet ps = new PermissionSet (PermissionState.None);
			ps.FromXml (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXmlInvalidPermission () 
		{
			PermissionSet ps = new PermissionSet (PermissionState.None);
			SecurityElement se = ps.ToXml ();
			// can't modify - so we create our own
			SecurityElement se2 = new SecurityElement ("InvalidPermissionSet", se.Text);
			se2.AddAttribute ("class", se.Attribute ("class"));
			se2.AddAttribute ("version", se.Attribute ("version"));
			ps.FromXml (se2);
		}

		[Test]
		// [ExpectedException (typeof (ArgumentException))]
		public void FromXmlWrongVersion () 
		{
			PermissionSet ps = new PermissionSet (PermissionState.None);
			SecurityElement se = ps.ToXml ();
			// can't modify - so we create our own
			SecurityElement se2 = new SecurityElement (se.Tag, se.Text);
			se2.AddAttribute ("class", se.Attribute ("class"));
			se2.AddAttribute ("version", "2");
			ps.FromXml (se2);
			// wow - here we accept a version 2 !!!
		}

		[Test]
		public void FromXmlEmpty () 
		{
			PermissionSet ps = new PermissionSet (PermissionState.None);
			SecurityElement se = ps.ToXml ();
			AssertNotNull ("Empty.ToXml()", se);
			AssertEquals ("Empty.Count", 0, ps.Count);

			PermissionSet ps2 = (PermissionSet) ps.Copy ();
			ps2.FromXml (se);
			Assert ("FromXml-Copy.IsUnrestricted", !ps2.IsUnrestricted ()); 

			se.AddAttribute ("Unrestricted", "true");
			ps2.FromXml (se);
			Assert ("FromXml-Unrestricted.IsUnrestricted", ps2.IsUnrestricted ());
		}


		[Test]
		public void FromXmlOne () 
		{
			FileDialogPermission fdp = new FileDialogPermission (FileDialogPermissionAccess.Open);
			PermissionSet ps1 = new PermissionSet (PermissionState.None);
			ps1.AddPermission (fdp);
			Assert ("ps1.IsEmpty", !ps1.IsEmpty ());

			PermissionSet ps = new PermissionSet (ps1);
			SecurityElement se = ps.ToXml ();
			AssertNotNull ("One.ToXml()", se);
			AssertEquals ("One.Count", 1, ps.Count);

			PermissionSet ps2 = (PermissionSet) ps.Copy ();
			ps2.FromXml (se);
			Assert ("FromXml-Copy.IsUnrestricted", !ps2.IsUnrestricted ()); 
			AssertEquals ("Copy.Count", 1, ps2.Count);

			se.AddAttribute ("Unrestricted", "true");
			ps2.FromXml (se);
			Assert ("FromXml-Unrestricted.IsUnrestricted", ps2.IsUnrestricted ());
			// IPermission not shown in XML but still present in Count
			AssertEquals ("Unrestricted.Count", 1, ps2.Count);
		}

		[Test]
		public void ToXmlNone () 
		{
			PermissionSet ps = new PermissionSet (PermissionState.None);
			SecurityElement se = ps.ToXml ();
			Assert ("None.ToString().StartsWith", ps.ToString().StartsWith ("<PermissionSet"));
			AssertEquals ("None.class", "System.Security.PermissionSet", (se.Attributes ["class"] as string));
			AssertEquals ("None.version", "1", (se.Attributes ["version"] as string));
			AssertNull ("None.Unrestricted", (se.Attributes ["Unrestricted"] as string));
		}

		[Test]
		public void ToXmlUnrestricted () 
		{
			PermissionSet ps = new PermissionSet (PermissionState.Unrestricted);
			SecurityElement se = ps.ToXml ();
			Assert ("Unrestricted.ToString().StartsWith", ps.ToString().StartsWith ("<PermissionSet"));
			AssertEquals ("Unrestricted.class", "System.Security.PermissionSet", (se.Attributes ["class"] as string));
			AssertEquals ("Unrestricted.version", "1", (se.Attributes ["version"] as string));
			AssertEquals ("Unrestricted.Unrestricted", "true", (se.Attributes ["Unrestricted"] as string));
		}
	}
}
