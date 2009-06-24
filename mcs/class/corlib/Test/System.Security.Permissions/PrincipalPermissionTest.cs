//
// PrincipalPermissionTest.cs - NUnit Test Cases for PrincipalPermission
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
	public class PrincipalPermissionTest {

		private static string className = "System.Security.Permissions.PrincipalPermission, ";

		[Test]
		public void PermissionStateNone () 
		{
			PrincipalPermission p = new PrincipalPermission (PermissionState.None);
			Assert.IsNotNull (p, "PrincipalPermission(PermissionState.None)");
			Assert.IsTrue (!p.IsUnrestricted (), "IsUnrestricted");
			PrincipalPermission copy = (PrincipalPermission) p.Copy ();
			Assert.AreEqual (p.IsUnrestricted (), copy.IsUnrestricted (), "Copy.IsUnrestricted");
			SecurityElement se = p.ToXml ();
			Assert.IsTrue ((se.Attributes ["class"] as string).StartsWith (className), "ToXml-class");
			Assert.AreEqual ("1", (se.Attributes ["version"] as string), "ToXml-version");
		}

		[Test]
		public void PermissionStateUnrestricted () 
		{
			PrincipalPermission p = new PrincipalPermission (PermissionState.Unrestricted);
			Assert.IsNotNull (p, "PrincipalPermission(PermissionState.Unrestricted)");
			Assert.IsTrue (p.IsUnrestricted (), "IsUnrestricted");
			PrincipalPermission copy = (PrincipalPermission) p.Copy ();
			Assert.AreEqual (p.IsUnrestricted (), copy.IsUnrestricted (), "Copy.IsUnrestricted");
			// Note: Unrestricted isn't shown in XML
		}

		[Test]
		public void Name () 
		{
			PrincipalPermission p = new PrincipalPermission ("user", null);
			Assert.IsTrue(!p.IsUnrestricted (), "Name.IsUnrestricted");
		}

		[Test]
		public void UnauthenticatedName () 
		{
			PrincipalPermission p = new PrincipalPermission ("user", null, false);
			Assert.IsTrue(!p.IsUnrestricted (), "UnauthenticatedName.IsUnrestricted");
		}

		[Test]
		public void Role () 
		{
			PrincipalPermission p = new PrincipalPermission (null, "users");
			Assert.IsTrue(!p.IsUnrestricted (), "Role.IsUnrestricted");
		}

		[Test]
		public void UnauthenticatedRole () 
		{
			PrincipalPermission p = new PrincipalPermission (null, "users", false);
			Assert.IsTrue(!p.IsUnrestricted (), "UnauthenticatedRole.IsUnrestricted");
		}

		[Test]
		public void NameRole () 
		{
			PrincipalPermission p = new PrincipalPermission ("user", "users", true);
			Assert.IsTrue(!p.IsUnrestricted (), "NameRole.IsUnrestricted");
		}

		[Test]
		public void UnauthenticatedNameRole () 
		{
			PrincipalPermission p = new PrincipalPermission ("user", "users", false);
			Assert.IsTrue(!p.IsUnrestricted (), "UnauthenticatedNameRole.IsUnrestricted");
		}

		[Test]
		public void AuthenticatedNullNull () 
		{
			PrincipalPermission p = new PrincipalPermission (null, null, true);
			Assert.IsTrue(p.IsUnrestricted (), "UnauthenticatedNameRole.IsUnrestricted");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromXmlNull () 
		{
			PrincipalPermission p = new PrincipalPermission (PermissionState.None);
			p.FromXml (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXmlInvalidPermission () 
		{
			PrincipalPermission p = new PrincipalPermission (PermissionState.None);
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
			PrincipalPermission p = new PrincipalPermission (PermissionState.None);
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
			PrincipalPermission p = new PrincipalPermission (PermissionState.None);
			SecurityElement se = p.ToXml ();
			Assert.IsNotNull (se, "ToXml()");

			PrincipalPermission p2 = (PrincipalPermission) p.Copy ();
			p2.FromXml (se);
			Assert.AreEqual (p.ToString (), p2.ToString (), "FromXml-Copy");

			string className = (string) se.Attributes ["class"];
			string version = (string) se.Attributes ["version"];

			SecurityElement se2 = new SecurityElement (se.Tag);
			se2.AddAttribute ("class", className);
			se2.AddAttribute ("version", version);
			p2.FromXml (se2);

			SecurityElement sec = new SecurityElement ("Identity");
			sec.AddAttribute ("Authenticated", "true");
			se2.AddChild (sec);
			p2.FromXml (se2);
			Assert.IsTrue (p2.IsUnrestricted (), "FromXml-Unrestricted");
		}

		[Test]
		public void UnionWithNull () 
		{
			PrincipalPermission p1 = new PrincipalPermission ("user", null);
			PrincipalPermission p2 = null;
			PrincipalPermission p3 = (PrincipalPermission) p1.Union (p2);
			Assert.AreEqual (p1.ToXml ().ToString (), p3.ToXml ().ToString (), "P1 U null == P1");
		}

		[Test]
		public void UnionWithUnrestricted () 
		{
			PrincipalPermission p1 = new PrincipalPermission (PermissionState.Unrestricted);
			PrincipalPermission p2 = new PrincipalPermission ("user", "role");
			PrincipalPermission p3 = (PrincipalPermission) p1.Union (p2);
			Assert.IsTrue (p3.IsUnrestricted (), "Unrestricted U P2 == Unrestricted");
			p3 = (PrincipalPermission) p2.Union (p1);
			Assert.IsTrue (p3.IsUnrestricted (), "P2 U Unrestricted == Unrestricted");
		}

		[Test]
		public void Union () 
		{
			PrincipalPermission p1 = new PrincipalPermission ("user A", "role A");
			PrincipalPermission p2 = new PrincipalPermission ("user B", "role B", false);
			PrincipalPermission p3 = (PrincipalPermission) p1.Union (p2);
			Assert.IsTrue (p3.ToString ().IndexOf ("user A") >= 0, "Union.UserA");
			Assert.IsTrue (p3.ToString ().IndexOf ("user B") >= 0, "Union.UserB");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void UnionWithBadPermission () 
		{
			PrincipalPermission p1 = new PrincipalPermission ("user", null);
			EnvironmentPermission ep2 = new EnvironmentPermission (PermissionState.Unrestricted);
			PrincipalPermission p3 = (PrincipalPermission) p1.Union (ep2);
		}

		[Test]
		public void IntersectWithNull () 
		{
			PrincipalPermission p1 = new PrincipalPermission ("user", "role");
			PrincipalPermission p2 = null;
			PrincipalPermission p3 = (PrincipalPermission) p1.Intersect (p2);
			Assert.IsNull (p3, "P1 N null == null");
		}

		[Test]
		public void IntersectWithUnrestricted () 
		{
			PrincipalPermission p1 = new PrincipalPermission (PermissionState.Unrestricted);
			PrincipalPermission p2 = new PrincipalPermission ("user", "role");
			PrincipalPermission p3 = (PrincipalPermission) p1.Intersect (p2);
			Assert.IsTrue (!p3.IsUnrestricted (), "Unrestricted N P2 == P2");
			Assert.AreEqual (p2.ToXml ().ToString (), p3.ToXml ().ToString (), "Unrestricted N EP2 == EP2");
			p3 = (PrincipalPermission) p2.Intersect (p1);
			Assert.IsTrue (!p3.IsUnrestricted (), "P2 N Unrestricted == P2");
			Assert.AreEqual (p2.ToXml ().ToString (), p3.ToXml ().ToString (), "P2 N Unrestricted == P2");
		}

		[Test]
		public void Intersect () 
		{
			// no intersection
			PrincipalPermission p1 = new PrincipalPermission ("user A", "role 1");
			PrincipalPermission p2 = new PrincipalPermission ("user B", "role 2");
			PrincipalPermission p3 = (PrincipalPermission) p1.Intersect (p2);
			Assert.IsNull (p3, "EP1 N EP2 == null");
			// intersection in role
			PrincipalPermission p4 = new PrincipalPermission ("user C", "role 1");
			p3 = (PrincipalPermission) p4.Intersect (p1);
			Assert.IsTrue (p3.ToString ().IndexOf ("user A") < 0, "Intersect (!user A)");
			Assert.IsTrue (p3.ToString ().IndexOf ("user C") < 0, "Intersect (!user C)");
			Assert.IsTrue (p3.ToString ().IndexOf ("role 1") >= 0, "Intersect (role 1)");
			// intersection in role without authentication
			PrincipalPermission p5 = new PrincipalPermission ("user C", "role 1", false);
			p3 = (PrincipalPermission) p5.Intersect (p1);
			Assert.IsNull (p3, "EP5 N EP1 == null");
		}

		[Test]
		public void IntersectNullName ()
		{
			PrincipalPermission p1 = new PrincipalPermission ("user", "role");
			PrincipalPermission p2 = new PrincipalPermission (null, "role");
			PrincipalPermission p3 = (PrincipalPermission) p1.Intersect (p2);
			Assert.AreEqual (p1.ToString (), p3.ToString (), "p1 N p2 == p1");
			p3 = (PrincipalPermission) p2.Intersect (p1);
			Assert.AreEqual (p1.ToString (), p3.ToString (), "p2 N p1 == p1");
		}

		[Test]
		public void IntersectNullRole ()
		{
			PrincipalPermission p1 = new PrincipalPermission ("user", "role");
			PrincipalPermission p2 = new PrincipalPermission ("user", null);
			PrincipalPermission p3 = (PrincipalPermission) p1.Intersect (p2);
			Assert.AreEqual (p1.ToString (), p3.ToString (), "p1 N p2 == p1");
			p3 = (PrincipalPermission) p2.Intersect (p1);
			Assert.AreEqual (p1.ToString (), p3.ToString (), "p2 N p1 == p1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void IntersectWithBadPermission () 
		{
			PrincipalPermission p1 = new PrincipalPermission ("user", null);
			EnvironmentPermission ep2 = new EnvironmentPermission (PermissionState.Unrestricted);
			PrincipalPermission p3 = (PrincipalPermission) p1.Intersect (ep2);
		}

		[Test]
		public void IsSubsetOfNull () 
		{
			PrincipalPermission p = new PrincipalPermission ("user", null);
			Assert.IsTrue (!p.IsSubsetOf (null), "User.IsSubsetOf(null)");

			p = new PrincipalPermission (PermissionState.None);
			Assert.IsTrue (p.IsSubsetOf (null), "None.IsSubsetOf(null)");

			p = new PrincipalPermission (PermissionState.Unrestricted);
			Assert.IsTrue (!p.IsSubsetOf (null), "Unrestricted.IsSubsetOf(null)");
		}

		[Test]
		public void IsSubsetOfNone ()
		{
			PrincipalPermission none = new PrincipalPermission (PermissionState.None);
			PrincipalPermission p = new PrincipalPermission ("user", null);
			Assert.IsTrue (!p.IsSubsetOf (none), "User.IsSubsetOf(null)");

			p = new PrincipalPermission (PermissionState.None);
			Assert.IsTrue (p.IsSubsetOf (none), "None.IsSubsetOf(null)");

			p = new PrincipalPermission (PermissionState.Unrestricted);
			Assert.IsTrue (!p.IsSubsetOf (none), "Unrestricted.IsSubsetOf(null)");
		}

		[Test]
		public void IsSubsetOfUnrestricted () 
		{
			PrincipalPermission p1 = new PrincipalPermission (PermissionState.Unrestricted);
			PrincipalPermission p2 = new PrincipalPermission ("user", "role", false);
			Assert.IsTrue (!p1.IsSubsetOf (p2), "Unrestricted.IsSubsetOf(user)");
			Assert.IsTrue (p2.IsSubsetOf (p1), "user.IsSubsetOf(Unrestricted)");
		}

		[Test]
		public void IsSubsetOf () 
		{
			PrincipalPermission p1 = new PrincipalPermission ("user A", "role 1");
			PrincipalPermission p2 = new PrincipalPermission (null, "role 1");
			Assert.IsTrue (p1.IsSubsetOf (p2), "UserRole.IsSubsetOf(Role)");
			Assert.IsTrue (!p2.IsSubsetOf (p1), "Role.IsSubsetOf(UserRole)");

			PrincipalPermission p3 = new PrincipalPermission ("user A", "role 1", false);
			Assert.IsTrue (!p3.IsSubsetOf (p1), "UserRoleAuth.IsSubsetOf(UserRoleNA)");
			Assert.IsTrue (!p1.IsSubsetOf (p3), "UserRoleNA.IsSubsetOf(UserRoleAuth)");

			PrincipalPermission p4 = new PrincipalPermission (null, null, true); // unrestricted
			Assert.IsTrue (!p4.IsSubsetOf (p1), "unrestricted.IsSubsetOf(UserRole)");
			Assert.IsTrue (p1.IsSubsetOf (p4), "UserRole.IsSubsetOf(unrestricted)");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void IsSubsetOfBadPermission () 
		{
			PrincipalPermission p1 = new PrincipalPermission ("user", null);
			EnvironmentPermission ep2 = new EnvironmentPermission (PermissionState.Unrestricted);
			Assert.IsTrue (p1.IsSubsetOf (ep2), "IsSubsetOf(EnvironmentPermission)");
		}
	}
}
