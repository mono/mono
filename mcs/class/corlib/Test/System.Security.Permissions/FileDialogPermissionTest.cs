//
// FileDialogPermissionTest.cs - NUnit Test Cases for FileDialogPermission
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

namespace MonoTests.System.Security.Permissions {

	[TestFixture]
	public class FileDialogPermissionTest : Assertion {

		private static string className = "System.Security.Permissions.FileDialogPermission, ";

		[Test]
		public void PermissionStateNone () 
		{
			FileDialogPermission p = new FileDialogPermission (PermissionState.None);
			AssertNotNull ("FileDialogPermission(PermissionState.None)", p);
			Assert ("IsUnrestricted", !p.IsUnrestricted ());
			FileDialogPermission copy = (FileDialogPermission) p.Copy ();
			AssertEquals ("Copy.IsUnrestricted", p.IsUnrestricted (), copy.IsUnrestricted ());
			SecurityElement se = p.ToXml ();
			Assert ("ToXml-class", (se.Attributes ["class"] as string).StartsWith (className));
			AssertEquals ("ToXml-version", "1", (se.Attributes ["version"] as string));
		}

		[Test]
		public void PermissionStateUnrestricted () 
		{
			FileDialogPermission p = new FileDialogPermission (PermissionState.Unrestricted);
			AssertNotNull ("FileDialogPermission(PermissionState.Unrestricted)", p);
			Assert ("IsUnrestricted", p.IsUnrestricted ());
			FileDialogPermission copy = (FileDialogPermission) p.Copy ();
			AssertEquals ("Copy.IsUnrestricted", p.IsUnrestricted (), copy.IsUnrestricted ());
			SecurityElement se = p.ToXml ();
			AssertEquals ("ToXml-Unrestricted", "true", (se.Attributes ["Unrestricted"] as string));
		}

		[Test]
		public void Derestricted () 
		{
			FileDialogPermission p = new FileDialogPermission (PermissionState.Unrestricted);
			Assert ("IsUnrestricted", p.IsUnrestricted ());
			p.Access = FileDialogPermissionAccess.None;
			Assert ("IsUnrestricted", !p.IsUnrestricted ());
		}

		[Test]
		public void None () 
		{
			FileDialogPermission p = new FileDialogPermission (FileDialogPermissionAccess.None);
			Assert ("IsUnrestricted", !p.IsUnrestricted ());
			SecurityElement se = p.ToXml ();
			AssertNull ("ToXml-Access=None", (se.Attributes ["Access"] as string));
		}

		[Test]
		public void Open () 
		{
			FileDialogPermission p = new FileDialogPermission (FileDialogPermissionAccess.Open);
			Assert ("IsUnrestricted", !p.IsUnrestricted ());
			SecurityElement se = p.ToXml ();
			AssertEquals ("ToXml-Access=Open", "Open", (se.Attributes ["Access"] as string));
		}

		[Test]
		public void Save () 
		{
			FileDialogPermission p = new FileDialogPermission (FileDialogPermissionAccess.Save);
			Assert ("IsUnrestricted", !p.IsUnrestricted ());
			SecurityElement se = p.ToXml ();
			AssertEquals ("ToXml-Access=Save", "Save", (se.Attributes ["Access"] as string));
		}

		[Test]
		public void OpenSave () 
		{
			FileDialogPermission p = new FileDialogPermission (FileDialogPermissionAccess.OpenSave);
			Assert ("IsUnrestricted", p.IsUnrestricted ());
			SecurityElement se = p.ToXml ();
			AssertEquals ("ToXml-Unrestricted", "true", (se.Attributes ["Unrestricted"] as string));
		}

		[Test]
		public void Access () 
		{
			FileDialogPermission p = new FileDialogPermission (PermissionState.None);
			Assert ("Access(default).IsUnrestricted", !p.IsUnrestricted ());
			p.Access = FileDialogPermissionAccess.None;
			Assert ("Access(None).IsUnrestricted", !p.IsUnrestricted ());
			p.Access = FileDialogPermissionAccess.Open;
			Assert ("Access(Open).IsUnrestricted", !p.IsUnrestricted ());
			p.Access = FileDialogPermissionAccess.Save;
			Assert ("Access(Save).IsUnrestricted", !p.IsUnrestricted ());
			p.Access = FileDialogPermissionAccess.OpenSave;
			Assert ("Access(OpenSave).IsUnrestricted", p.IsUnrestricted ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromXmlNull () 
		{
			FileDialogPermission p = new FileDialogPermission (PermissionState.None);
			p.FromXml (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXmlInvalidPermission () 
		{
			FileDialogPermission p = new FileDialogPermission (PermissionState.None);
			SecurityElement se = p.ToXml ();
			// can't modify - so we create our own
			SecurityElement se2 = new SecurityElement ("IInvalidPermission", se.Text);
			se2.AddAttribute ("class", se.Attribute ("class"));
			se2.AddAttribute ("version", se.Attribute ("version"));
			p.FromXml (se2);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXmlWrongVersion () 
		{
			FileDialogPermission p = new FileDialogPermission (PermissionState.None);
			SecurityElement se = p.ToXml ();
			// can't modify - so we create our own
			SecurityElement se2 = new SecurityElement (se.Tag, se.Text);
			se2.AddAttribute ("class", se.Attribute ("class"));
			se2.AddAttribute ("version", "2");
			p.FromXml (se2);
		}

		[Test]
		public void FromXml () 
		{
			FileDialogPermission p = new FileDialogPermission (PermissionState.None);
			SecurityElement se = p.ToXml ();
			AssertNotNull ("ToXml()", se);

			FileDialogPermission p2 = (FileDialogPermission) p.Copy ();
			p2.FromXml (se);
			AssertEquals ("FromXml-None", FileDialogPermissionAccess.None, p2.Access);

			se.AddAttribute ("Access", "Open");
			p2.FromXml (se);
			AssertEquals ("FromXml-Open", FileDialogPermissionAccess.Open, p2.Access);

			se = p.ToXml ();
			se.AddAttribute ("Access", "Save");
			p2.FromXml (se);
			AssertEquals ("FromXml-Save", FileDialogPermissionAccess.Save, p2.Access);

			se = p.ToXml ();
			se.AddAttribute ("Unrestricted", "true");
			p2.FromXml (se);
			Assert ("FromXml-Unrestricted", p2.IsUnrestricted ());
		}

		[Test]
		public void UnionWithNull () 
		{
			FileDialogPermission p1 = new FileDialogPermission (FileDialogPermissionAccess.Open);
			FileDialogPermission p2 = null;
			FileDialogPermission p3 = (FileDialogPermission) p1.Union (p2);
			AssertEquals ("P1 U null == P1", p1.ToXml ().ToString (), p3.ToXml ().ToString ());
		}

		[Test]
		public void UnionWithUnrestricted () 
		{
			FileDialogPermission p1 = new FileDialogPermission (PermissionState.Unrestricted);
			FileDialogPermission p2 = new FileDialogPermission (FileDialogPermissionAccess.Open);
			FileDialogPermission p3 = (FileDialogPermission) p1.Union (p2);
			Assert ("Unrestricted U P2 == Unrestricted", p3.IsUnrestricted ());
			p3 = (FileDialogPermission) p2.Union (p1);
			Assert ("P2 U Unrestricted == Unrestricted", p3.IsUnrestricted ());
		}

		[Test]
		public void Union () 
		{
			FileDialogPermission p1 = new FileDialogPermission (FileDialogPermissionAccess.Open);
			FileDialogPermission p2 = new FileDialogPermission (FileDialogPermissionAccess.Save);
			FileDialogPermission p3 = (FileDialogPermission) p1.Union (p2);
			FileDialogPermission p4 = new FileDialogPermission (FileDialogPermissionAccess.OpenSave);
			AssertEquals ("P1 U P2 == P1+2", p3.ToXml ().ToString (), p4.ToXml ().ToString ());
			Assert ("P1+2==Unrestricted", p3.IsUnrestricted ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void UnionWithBadPermission () 
		{
			FileDialogPermission p1 = new FileDialogPermission (FileDialogPermissionAccess.Open);
			EnvironmentPermission ep2 = new EnvironmentPermission (PermissionState.Unrestricted);
			FileDialogPermission p3 = (FileDialogPermission) p1.Union (ep2);
		}

		[Test]
		public void IntersectWithNull () 
		{
			FileDialogPermission p1 = new FileDialogPermission (FileDialogPermissionAccess.Open);
			FileDialogPermission p2 = null;
			FileDialogPermission p3 = (FileDialogPermission) p1.Intersect (p2);
			AssertNull ("P1 N null == null", p3);
		}

		[Test]
		public void IntersectWithUnrestricted () 
		{
			FileDialogPermission p1 = new FileDialogPermission (PermissionState.Unrestricted);
			FileDialogPermission p2 = new FileDialogPermission (FileDialogPermissionAccess.Open);
			FileDialogPermission p3 = (FileDialogPermission) p1.Intersect (p2);
			Assert ("Unrestricted N P2 == P2", !p3.IsUnrestricted ());
			AssertEquals ("Unrestricted N EP2 == EP2", p2.ToXml ().ToString (), p3.ToXml ().ToString ());
			p3 = (FileDialogPermission) p2.Intersect (p1);
			Assert ("P2 N Unrestricted == P2", !p3.IsUnrestricted ());
			AssertEquals ("P2 N Unrestricted == P2", p2.ToXml ().ToString (), p3.ToXml ().ToString ());
		}

		[Test]
		public void Intersect () 
		{
			// no intersection
			FileDialogPermission p1 = new FileDialogPermission (FileDialogPermissionAccess.Open);
			FileDialogPermission p2 = new FileDialogPermission (FileDialogPermissionAccess.Save);
			FileDialogPermission p3 = (FileDialogPermission) p1.Intersect (p2);
			AssertNull ("EP1 N EP2 == null", p3);
			// intersection in open
			FileDialogPermission p4 = new FileDialogPermission (FileDialogPermissionAccess.Open);
			p3 = (FileDialogPermission) p4.Intersect (p1);
			AssertEquals ("Intersect-Open", FileDialogPermissionAccess.Open, p3.Access);
			// intersection in save
			FileDialogPermission p5 = new FileDialogPermission (FileDialogPermissionAccess.Save);		
			p3 = (FileDialogPermission) p5.Intersect (p2);
			AssertEquals ("Intersect-Save", FileDialogPermissionAccess.Save, p3.Access);
			// intersection in open and save
			FileDialogPermission p6 = new FileDialogPermission (FileDialogPermissionAccess.OpenSave);
			FileDialogPermission p7 = new FileDialogPermission (FileDialogPermissionAccess.OpenSave);
			p3 = (FileDialogPermission) p6.Intersect (p7);
			AssertEquals ("Intersect-AllAccess-OpenSave", FileDialogPermissionAccess.OpenSave, p3.Access);
			Assert ("Intersect-OpenSave-Unrestricted", p3.IsUnrestricted ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void IntersectWithBadPermission () 
		{
			FileDialogPermission p1 = new FileDialogPermission (FileDialogPermissionAccess.Open);
			EnvironmentPermission ep2 = new EnvironmentPermission (PermissionState.Unrestricted);
			FileDialogPermission p3 = (FileDialogPermission) p1.Intersect (ep2);
		}

		[Test]
		public void IsSubsetOfNull () 
		{
			FileDialogPermission p1 = new FileDialogPermission (FileDialogPermissionAccess.Open);
			Assert ("IsSubsetOf(null)", !p1.IsSubsetOf (null));
		}

		[Test]
		public void IsSubsetOfUnrestricted () 
		{
			FileDialogPermission p1 = new FileDialogPermission (PermissionState.Unrestricted);
			FileDialogPermission p2 = new FileDialogPermission (FileDialogPermissionAccess.Open);
			FileDialogPermission p3 = new FileDialogPermission (PermissionState.Unrestricted);
			Assert ("Unrestricted.IsSubsetOf()", !p1.IsSubsetOf (p2));
			Assert ("IsSubsetOf(Unrestricted)", p2.IsSubsetOf (p1));
			Assert ("Unrestricted.IsSubsetOf(Unrestricted)", p1.IsSubsetOf (p3));
		}

		[Test]
		public void IsSubsetOf () 
		{
			FileDialogPermission p1 = new FileDialogPermission (FileDialogPermissionAccess.Open);
			FileDialogPermission p2 = new FileDialogPermission (FileDialogPermissionAccess.Save);
			Assert ("IsSubsetOf(nosubset1)", !p1.IsSubsetOf (p2));
			Assert ("IsSubsetOf(nosubset2)", !p2.IsSubsetOf (p1));
			FileDialogPermission p3 = new FileDialogPermission (FileDialogPermissionAccess.OpenSave);
			Assert ("IsSubsetOf(OpenSave)", p1.IsSubsetOf (p3));
			Assert ("OpenSave.IsSubsetOf()", !p3.IsSubsetOf (p1));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void IsSubsetOfBadPermission () 
		{
			FileDialogPermission p1 = new FileDialogPermission (FileDialogPermissionAccess.Open);
			EnvironmentPermission ep2 = new EnvironmentPermission (PermissionState.Unrestricted);
			Assert ("IsSubsetOf(EnvironmentPermission)", p1.IsSubsetOf (ep2));
		}
	}
}
