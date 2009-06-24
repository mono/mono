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
	public class FileDialogPermissionTest {

		private static string className = "System.Security.Permissions.FileDialogPermission, ";

		[Test]
		public void PermissionStateNone () 
		{
			FileDialogPermission p = new FileDialogPermission (PermissionState.None);
			Assert.IsNotNull (p, "FileDialogPermission(PermissionState.None)");
			Assert.IsTrue (!p.IsUnrestricted (), "IsUnrestricted");
			FileDialogPermission copy = (FileDialogPermission) p.Copy ();
			Assert.AreEqual (p.IsUnrestricted (), copy.IsUnrestricted (), "Copy.IsUnrestricted");
			SecurityElement se = p.ToXml ();
			Assert.IsTrue ((se.Attributes ["class"] as string).StartsWith (className), "ToXml-class");
			Assert.AreEqual ("1", (se.Attributes ["version"] as string), "ToXml-version");
		}

		[Test]
		public void PermissionStateUnrestricted () 
		{
			FileDialogPermission p = new FileDialogPermission (PermissionState.Unrestricted);
			Assert.IsNotNull (p, "FileDialogPermission(PermissionState.Unrestricted)");
			Assert.IsTrue (p.IsUnrestricted (), "IsUnrestricted");
			FileDialogPermission copy = (FileDialogPermission) p.Copy ();
			Assert.AreEqual (p.IsUnrestricted (), copy.IsUnrestricted (), "Copy.IsUnrestricted");
			SecurityElement se = p.ToXml ();
			Assert.AreEqual ("true", (se.Attributes ["Unrestricted"] as string), "ToXml-Unrestricted");
		}

		[Test]
		public void Derestricted () 
		{
			FileDialogPermission p = new FileDialogPermission (PermissionState.Unrestricted);
			Assert.IsTrue (p.IsUnrestricted (), "IsUnrestricted");
			p.Access = FileDialogPermissionAccess.None;
			Assert.IsTrue (!p.IsUnrestricted (), "IsUnrestricted");
		}

		[Test]
		public void None () 
		{
			FileDialogPermission p = new FileDialogPermission (FileDialogPermissionAccess.None);
			Assert.IsTrue (!p.IsUnrestricted (), "IsUnrestricted");
			SecurityElement se = p.ToXml ();
			Assert.IsNull ((se.Attributes ["Access"] as string), "ToXml-Access=None");
		}

		[Test]
		public void Open () 
		{
			FileDialogPermission p = new FileDialogPermission (FileDialogPermissionAccess.Open);
			Assert.IsTrue (!p.IsUnrestricted (), "IsUnrestricted");
			SecurityElement se = p.ToXml ();
			Assert.AreEqual ("Open", (se.Attributes ["Access"] as string), "ToXml-Access=Open");
		}

		[Test]
		public void Save () 
		{
			FileDialogPermission p = new FileDialogPermission (FileDialogPermissionAccess.Save);
			Assert.IsTrue (!p.IsUnrestricted (), "IsUnrestricted");
			SecurityElement se = p.ToXml ();
			Assert.AreEqual ("Save", (se.Attributes ["Access"] as string), "ToXml-Access=Save");
		}

		[Test]
		public void OpenSave () 
		{
			FileDialogPermission p = new FileDialogPermission (FileDialogPermissionAccess.OpenSave);
			Assert.IsTrue (p.IsUnrestricted (), "IsUnrestricted");
			SecurityElement se = p.ToXml ();
			Assert.AreEqual ("true", (se.Attributes ["Unrestricted"] as string), "ToXml-Unrestricted");
		}

		[Test]
		public void Access () 
		{
			FileDialogPermission p = new FileDialogPermission (PermissionState.None);
			Assert.IsTrue (!p.IsUnrestricted (), "Access(default).IsUnrestricted");
			p.Access = FileDialogPermissionAccess.None;
			Assert.IsTrue (!p.IsUnrestricted (), "Access(None).IsUnrestricted");
			p.Access = FileDialogPermissionAccess.Open;
			Assert.IsTrue (!p.IsUnrestricted (), "Access(Open).IsUnrestricted");
			p.Access = FileDialogPermissionAccess.Save;
			Assert.IsTrue (!p.IsUnrestricted (), "Access(Save).IsUnrestricted");
			p.Access = FileDialogPermissionAccess.OpenSave;
			Assert.IsTrue (p.IsUnrestricted (), "Access(OpenSave).IsUnrestricted");
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
			Assert.IsNotNull (se, "ToXml()");

			FileDialogPermission p2 = (FileDialogPermission) p.Copy ();
			p2.FromXml (se);
			Assert.AreEqual (FileDialogPermissionAccess.None, p2.Access, "FromXml-None");

			se.AddAttribute ("Access", "Open");
			p2.FromXml (se);
			Assert.AreEqual (FileDialogPermissionAccess.Open, p2.Access, "FromXml-Open");

			se = p.ToXml ();
			se.AddAttribute ("Access", "Save");
			p2.FromXml (se);
			Assert.AreEqual (FileDialogPermissionAccess.Save, p2.Access, "FromXml-Save");

			se = p.ToXml ();
			se.AddAttribute ("Unrestricted", "true");
			p2.FromXml (se);
			Assert.IsTrue (p2.IsUnrestricted (), "FromXml-Unrestricted");
		}

		[Test]
		public void UnionWithNull () 
		{
			FileDialogPermission p1 = new FileDialogPermission (FileDialogPermissionAccess.Open);
			FileDialogPermission p2 = null;
			FileDialogPermission p3 = (FileDialogPermission) p1.Union (p2);
			Assert.AreEqual (p1.ToXml ().ToString (), p3.ToXml ().ToString (), "P1 U null == P1");
		}

		[Test]
		public void UnionWithUnrestricted () 
		{
			FileDialogPermission p1 = new FileDialogPermission (PermissionState.Unrestricted);
			FileDialogPermission p2 = new FileDialogPermission (FileDialogPermissionAccess.Open);
			FileDialogPermission p3 = (FileDialogPermission) p1.Union (p2);
			Assert.IsTrue (p3.IsUnrestricted (), "Unrestricted U P2 == Unrestricted");
			p3 = (FileDialogPermission) p2.Union (p1);
			Assert.IsTrue (p3.IsUnrestricted (), "P2 U Unrestricted == Unrestricted");
		}

		[Test]
		public void Union () 
		{
			FileDialogPermission p1 = new FileDialogPermission (FileDialogPermissionAccess.Open);
			FileDialogPermission p2 = new FileDialogPermission (FileDialogPermissionAccess.Save);
			FileDialogPermission p3 = (FileDialogPermission) p1.Union (p2);
			FileDialogPermission p4 = new FileDialogPermission (FileDialogPermissionAccess.OpenSave);
			Assert.AreEqual (p3.ToXml ().ToString (), p4.ToXml ().ToString (), "P1 U P2 == P1+2");
			Assert.IsTrue (p3.IsUnrestricted (), "P1+2==Unrestricted");
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
			Assert.IsNull (p3, "P1 N null == null");
		}

		[Test]
		public void IntersectWithUnrestricted () 
		{
			FileDialogPermission p1 = new FileDialogPermission (PermissionState.Unrestricted);
			FileDialogPermission p2 = new FileDialogPermission (FileDialogPermissionAccess.Open);
			FileDialogPermission p3 = (FileDialogPermission) p1.Intersect (p2);
			Assert.IsTrue (!p3.IsUnrestricted (), "Unrestricted N P2 == P2");
			Assert.AreEqual (p2.ToXml ().ToString (), p3.ToXml ().ToString (), "Unrestricted N EP2 == EP2");
			p3 = (FileDialogPermission) p2.Intersect (p1);
			Assert.IsTrue (!p3.IsUnrestricted (), "P2 N Unrestricted == P2");
			Assert.AreEqual (p2.ToXml ().ToString (), p3.ToXml ().ToString (), "P2 N Unrestricted == P2");
		}

		[Test]
		public void Intersect () 
		{
			// no intersection
			FileDialogPermission p1 = new FileDialogPermission (FileDialogPermissionAccess.Open);
			FileDialogPermission p2 = new FileDialogPermission (FileDialogPermissionAccess.Save);
			FileDialogPermission p3 = (FileDialogPermission) p1.Intersect (p2);
			Assert.IsNull (p3, "EP1 N EP2 == null");
			// intersection in open
			FileDialogPermission p4 = new FileDialogPermission (FileDialogPermissionAccess.Open);
			p3 = (FileDialogPermission) p4.Intersect (p1);
			Assert.AreEqual (FileDialogPermissionAccess.Open, p3.Access, "Intersect-Open");
			// intersection in save
			FileDialogPermission p5 = new FileDialogPermission (FileDialogPermissionAccess.Save);		
			p3 = (FileDialogPermission) p5.Intersect (p2);
			Assert.AreEqual (FileDialogPermissionAccess.Save, p3.Access, "Intersect-Save");
			// intersection in open and save
			FileDialogPermission p6 = new FileDialogPermission (FileDialogPermissionAccess.OpenSave);
			FileDialogPermission p7 = new FileDialogPermission (FileDialogPermissionAccess.OpenSave);
			p3 = (FileDialogPermission) p6.Intersect (p7);
			Assert.AreEqual (FileDialogPermissionAccess.OpenSave, p3.Access, "Intersect-AllAccess-OpenSave");
			Assert.IsTrue (p3.IsUnrestricted (), "Intersect-OpenSave-Unrestricted");
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
			Assert.IsTrue (!p1.IsSubsetOf (null), "IsSubsetOf(null)");
		}

		[Test]
		public void IsSubsetOfUnrestricted () 
		{
			FileDialogPermission p1 = new FileDialogPermission (PermissionState.Unrestricted);
			FileDialogPermission p2 = new FileDialogPermission (FileDialogPermissionAccess.Open);
			FileDialogPermission p3 = new FileDialogPermission (PermissionState.Unrestricted);
			Assert.IsTrue (!p1.IsSubsetOf (p2), "Unrestricted.IsSubsetOf()");
			Assert.IsTrue (p2.IsSubsetOf (p1), "IsSubsetOf(Unrestricted)");
			Assert.IsTrue (p1.IsSubsetOf (p3), "Unrestricted.IsSubsetOf(Unrestricted)");
		}

		[Test]
		public void IsSubsetOf () 
		{
			FileDialogPermission p1 = new FileDialogPermission (FileDialogPermissionAccess.Open);
			FileDialogPermission p2 = new FileDialogPermission (FileDialogPermissionAccess.Save);
			Assert.IsTrue (!p1.IsSubsetOf (p2), "IsSubsetOf(nosubset1)");
			Assert.IsTrue (!p2.IsSubsetOf (p1), "IsSubsetOf(nosubset2)");
			FileDialogPermission p3 = new FileDialogPermission (FileDialogPermissionAccess.OpenSave);
			Assert.IsTrue (p1.IsSubsetOf (p3), "IsSubsetOf(OpenSave)");
			Assert.IsTrue (!p3.IsSubsetOf (p1), "OpenSave.IsSubsetOf()");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void IsSubsetOfBadPermission () 
		{
			FileDialogPermission p1 = new FileDialogPermission (FileDialogPermissionAccess.Open);
			EnvironmentPermission ep2 = new EnvironmentPermission (PermissionState.Unrestricted);
			Assert.IsTrue (p1.IsSubsetOf (ep2), "IsSubsetOf(EnvironmentPermission)");
		}
	}
}
