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
	public class PrincipalPermissionTest : Assertion {

		private static string className = "System.Security.Permissions.PrincipalPermission, ";

		[Test]
		public void PermissionStateNone () 
		{
			PrincipalPermission p = new PrincipalPermission (PermissionState.None);
			AssertNotNull ("PrincipalPermission(PermissionState.None)", p);
			Assert ("IsUnrestricted", !p.IsUnrestricted ());
			PrincipalPermission copy = (PrincipalPermission) p.Copy ();
			AssertEquals ("Copy.IsUnrestricted", p.IsUnrestricted (), copy.IsUnrestricted ());
			SecurityElement se = p.ToXml ();
			Assert ("ToXml-class", (se.Attributes ["class"] as string).StartsWith (className));
			AssertEquals ("ToXml-version", "1", (se.Attributes ["version"] as string));
		}

		[Test]
		public void PermissionStateUnrestricted () 
		{
			PrincipalPermission p = new PrincipalPermission (PermissionState.Unrestricted);
			AssertNotNull ("PrincipalPermission(PermissionState.Unrestricted)", p);
			Assert ("IsUnrestricted", p.IsUnrestricted ());
			PrincipalPermission copy = (PrincipalPermission) p.Copy ();
			AssertEquals ("Copy.IsUnrestricted", p.IsUnrestricted (), copy.IsUnrestricted ());
			// Note: Unrestricted isn't shown in XML
		}

		[Test]
		public void Name () 
		{
			PrincipalPermission p = new PrincipalPermission ("user", null);
			Assert("Name.IsUnrestricted", !p.IsUnrestricted ());
		}

		[Test]
		public void UnauthenticatedName () 
		{
			PrincipalPermission p = new PrincipalPermission ("user", null, false);
			Assert("UnauthenticatedName.IsUnrestricted", !p.IsUnrestricted ());
		}

		[Test]
		public void Role () 
		{
			PrincipalPermission p = new PrincipalPermission (null, "users");
			Assert("Role.IsUnrestricted", !p.IsUnrestricted ());
		}

		[Test]
		public void UnauthenticatedRole () 
		{
			PrincipalPermission p = new PrincipalPermission (null, "users", false);
			Assert("UnauthenticatedRole.IsUnrestricted", !p.IsUnrestricted ());
		}

		[Test]
		public void NameRole () 
		{
			PrincipalPermission p = new PrincipalPermission ("user", "users", true);
			Assert("NameRole.IsUnrestricted", !p.IsUnrestricted ());
		}

		[Test]
		public void UnauthenticatedNameRole () 
		{
			PrincipalPermission p = new PrincipalPermission ("user", "users", false);
			Assert("UnauthenticatedNameRole.IsUnrestricted", !p.IsUnrestricted ());
		}

		[Test]
		public void AuthenticatedNullNull () 
		{
			PrincipalPermission p = new PrincipalPermission (null, null, true);
			Assert("UnauthenticatedNameRole.IsUnrestricted", p.IsUnrestricted ());
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
			AssertNotNull ("ToXml()", se);

			PrincipalPermission p2 = (PrincipalPermission) p.Copy ();
			p2.FromXml (se);
			AssertEquals ("FromXml-Copy", p.ToString (), p2.ToString ());

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
			Assert ("FromXml-Unrestricted", p2.IsUnrestricted ());
		}

		[Test]
		public void UnionWithNull () 
		{
			PrincipalPermission p1 = new PrincipalPermission ("user", null);
			PrincipalPermission p2 = null;
			PrincipalPermission p3 = (PrincipalPermission) p1.Union (p2);
			AssertEquals ("P1 U null == P1", p1.ToXml ().ToString (), p3.ToXml ().ToString ());
		}

		[Test]
		public void UnionWithUnrestricted () 
		{
			PrincipalPermission p1 = new PrincipalPermission (PermissionState.Unrestricted);
			PrincipalPermission p2 = new PrincipalPermission ("user", "role");
			PrincipalPermission p3 = (PrincipalPermission) p1.Union (p2);
			Assert ("Unrestricted U P2 == Unrestricted", p3.IsUnrestricted ());
			p3 = (PrincipalPermission) p2.Union (p1);
			Assert ("P2 U Unrestricted == Unrestricted", p3.IsUnrestricted ());
		}

		[Test]
		public void Union () 
		{
			PrincipalPermission p1 = new PrincipalPermission ("user A", "role A");
			PrincipalPermission p2 = new PrincipalPermission ("user B", "role B", false);
			PrincipalPermission p3 = (PrincipalPermission) p1.Union (p2);
			Assert ("Union.UserA", p3.ToString ().IndexOf ("user A") >= 0);
			Assert ("Union.UserB", p3.ToString ().IndexOf ("user B") >= 0);
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
			AssertNull ("P1 N null == null", p3);
		}

		[Test]
		public void IntersectWithUnrestricted () 
		{
			PrincipalPermission p1 = new PrincipalPermission (PermissionState.Unrestricted);
			PrincipalPermission p2 = new PrincipalPermission ("user", "role");
			PrincipalPermission p3 = (PrincipalPermission) p1.Intersect (p2);
			Assert ("Unrestricted N P2 == P2", !p3.IsUnrestricted ());
			AssertEquals ("Unrestricted N EP2 == EP2", p2.ToXml ().ToString (), p3.ToXml ().ToString ());
			p3 = (PrincipalPermission) p2.Intersect (p1);
			Assert ("P2 N Unrestricted == P2", !p3.IsUnrestricted ());
			AssertEquals ("P2 N Unrestricted == P2", p2.ToXml ().ToString (), p3.ToXml ().ToString ());
		}

		[Test]
		public void Intersect () 
		{
			// no intersection
			PrincipalPermission p1 = new PrincipalPermission ("user A", "role 1");
			PrincipalPermission p2 = new PrincipalPermission ("user B", "role 2");
			PrincipalPermission p3 = (PrincipalPermission) p1.Intersect (p2);
			AssertNull ("EP1 N EP2 == null", p3);
			// intersection in role
			PrincipalPermission p4 = new PrincipalPermission ("user C", "role 1");
			p3 = (PrincipalPermission) p4.Intersect (p1);
			Assert ("Intersect (!user A)", p3.ToString ().IndexOf ("user A") < 0);
			Assert ("Intersect (!user C)", p3.ToString ().IndexOf ("user C") < 0);
			Assert ("Intersect (role 1)", p3.ToString ().IndexOf ("role 1") >= 0);
			// intersection in role without authentication
			PrincipalPermission p5 = new PrincipalPermission ("user C", "role 1", false);
			p3 = (PrincipalPermission) p5.Intersect (p1);
			AssertNull ("EP5 N EP1 == null", p3);
		}

		[Test]
		public void IntersectNullName ()
		{
			PrincipalPermission p1 = new PrincipalPermission ("user", "role");
			PrincipalPermission p2 = new PrincipalPermission (null, "role");
			PrincipalPermission p3 = (PrincipalPermission) p1.Intersect (p2);
			AssertEquals ("p1 N p2 == p1", p1.ToString (), p3.ToString ());
			p3 = (PrincipalPermission) p2.Intersect (p1);
			AssertEquals ("p2 N p1 == p1", p1.ToString (), p3.ToString ());
		}

		[Test]
		public void IntersectNullRole ()
		{
			PrincipalPermission p1 = new PrincipalPermission ("user", "role");
			PrincipalPermission p2 = new PrincipalPermission ("user", null);
			PrincipalPermission p3 = (PrincipalPermission) p1.Intersect (p2);
			AssertEquals ("p1 N p2 == p1", p1.ToString (), p3.ToString ());
			p3 = (PrincipalPermission) p2.Intersect (p1);
			AssertEquals ("p2 N p1 == p1", p1.ToString (), p3.ToString ());
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
			Assert ("User.IsSubsetOf(null)", !p.IsSubsetOf (null));
		}

		[Test]
		public void IsSubsetOfUnrestricted () 
		{
			PrincipalPermission p1 = new PrincipalPermission (PermissionState.Unrestricted);
			PrincipalPermission p2 = new PrincipalPermission ("user", "role", false);
			Assert ("Unrestricted.IsSubsetOf(user)", !p1.IsSubsetOf (p2));
			Assert ("user.IsSubsetOf(Unrestricted)", p2.IsSubsetOf (p1));
		}

		[Test]
		public void IsSubsetOf () 
		{
			PrincipalPermission p1 = new PrincipalPermission ("user A", "role 1");
			PrincipalPermission p2 = new PrincipalPermission (null, "role 1");
			Assert ("UserRole.IsSubsetOf(Role)", p1.IsSubsetOf (p2));
			Assert ("Role.IsSubsetOf(UserRole)", !p2.IsSubsetOf (p1));

			PrincipalPermission p3 = new PrincipalPermission ("user A", "role 1", false);
			Assert ("UserRoleAuth.IsSubsetOf(UserRoleNA)", !p3.IsSubsetOf (p1));
			Assert ("UserRoleNA.IsSubsetOf(UserRoleAuth)", !p1.IsSubsetOf (p3));

			PrincipalPermission p4 = new PrincipalPermission (null, null, true); // unrestricted
			Assert ("unrestricted.IsSubsetOf(UserRole)", !p4.IsSubsetOf (p1));
			Assert ("UserRole.IsSubsetOf(unrestricted)", p1.IsSubsetOf (p4));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void IsSubsetOfBadPermission () 
		{
			PrincipalPermission p1 = new PrincipalPermission ("user", null);
			EnvironmentPermission ep2 = new EnvironmentPermission (PermissionState.Unrestricted);
			Assert ("IsSubsetOf(EnvironmentPermission)", p1.IsSubsetOf (ep2));
		}
	}
}
