//
// ReflectionPermissionTest.cs - NUnit Test Cases for ReflectionPermission
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
	public class ReflectionPermissionTest : Assertion {

		private static string className = "System.Security.Permissions.ReflectionPermission, ";

		[Test]
		public void PermissionStateNone () 
		{
			ReflectionPermission p = new ReflectionPermission (PermissionState.None);
			AssertNotNull ("ReflectionPermission(PermissionState.None)", p);
			Assert ("IsUnrestricted", !p.IsUnrestricted ());
			ReflectionPermission copy = (ReflectionPermission) p.Copy ();
			AssertEquals ("Copy.IsUnrestricted", p.IsUnrestricted (), copy.IsUnrestricted ());
			SecurityElement se = p.ToXml ();
			Assert ("ToXml-class", (se.Attributes ["class"] as string).StartsWith (className));
			AssertEquals ("ToXml-version", "1", (se.Attributes ["version"] as string));
		}

		[Test]
		public void PermissionStateUnrestricted () 
		{
			ReflectionPermission p = new ReflectionPermission (PermissionState.Unrestricted);
			AssertNotNull ("ReflectionPermission(PermissionState.Unrestricted)", p);
			Assert ("IsUnrestricted", p.IsUnrestricted ());
			ReflectionPermission copy = (ReflectionPermission) p.Copy ();
			AssertEquals ("Copy.IsUnrestricted", p.IsUnrestricted (), copy.IsUnrestricted ());
			SecurityElement se = p.ToXml ();
			AssertEquals ("ToXml-Unrestricted", "true", (se.Attributes ["Unrestricted"] as string));
		}

		[Test]
		public void Derestricted () 
		{
			ReflectionPermission p = new ReflectionPermission (PermissionState.Unrestricted);
			Assert ("IsUnrestricted", p.IsUnrestricted ());
			p.Flags = ReflectionPermissionFlag.NoFlags;
			Assert ("IsUnrestricted", !p.IsUnrestricted ());
		}

		[Test]
		public void NoFlags () 
		{
			ReflectionPermission p = new ReflectionPermission (ReflectionPermissionFlag.NoFlags);
			Assert ("IsUnrestricted", !p.IsUnrestricted ());
			SecurityElement se = p.ToXml ();
			AssertEquals ("ToXml-Flags=NoFlags", "NoFlags", (se.Attributes ["Flags"] as string));
		}

		[Test]
		public void TypeInformation () 
		{
			ReflectionPermission p = new ReflectionPermission (ReflectionPermissionFlag.TypeInformation);
			Assert ("IsUnrestricted", !p.IsUnrestricted ());
			SecurityElement se = p.ToXml ();
			AssertEquals ("ToXml-Flags=TypeInformation", "TypeInformation", (se.Attributes ["Flags"] as string));
		}

		[Test]
		public void MemberAccess () 
		{
			ReflectionPermission p = new ReflectionPermission (ReflectionPermissionFlag.MemberAccess);
			Assert ("IsUnrestricted", !p.IsUnrestricted ());
			SecurityElement se = p.ToXml ();
			AssertEquals ("ToXml-Flags=MemberAccess", "MemberAccess", (se.Attributes ["Flags"] as string));
		}

		[Test]
		public void ReflectionEmit () 
		{
			ReflectionPermission p = new ReflectionPermission (ReflectionPermissionFlag.ReflectionEmit);
			Assert ("IsUnrestricted", !p.IsUnrestricted ());
			SecurityElement se = p.ToXml ();
			AssertEquals ("ToXml-Flags=ReflectionEmit", "ReflectionEmit", (se.Attributes ["Flags"] as string));
		}

		[Test]
		public void AllFlags () 
		{
			ReflectionPermission p = new ReflectionPermission (ReflectionPermissionFlag.AllFlags);
			Assert ("IsUnrestricted", p.IsUnrestricted ());
			SecurityElement se = p.ToXml ();
			AssertEquals ("ToXml-Unrestricted", "true", (se.Attributes ["Unrestricted"] as string));
		}

		[Test]
		public void Flags () 
		{
			ReflectionPermission p = new ReflectionPermission (PermissionState.None);
			Assert ("Flags(default).IsUnrestricted", !p.IsUnrestricted ());
			p.Flags = ReflectionPermissionFlag.NoFlags;
			Assert ("Flags(NoFlags).IsUnrestricted", !p.IsUnrestricted ());
			p.Flags = ReflectionPermissionFlag.TypeInformation;
			Assert ("Flags(TypeInformation).IsUnrestricted", !p.IsUnrestricted ());
			p.Flags = ReflectionPermissionFlag.MemberAccess;
			Assert ("Flags(MemberAccess).IsUnrestricted", !p.IsUnrestricted ());
			p.Flags = ReflectionPermissionFlag.ReflectionEmit;
			Assert ("Flags(ReflectionEmit).IsUnrestricted", !p.IsUnrestricted ());
			p.Flags = ReflectionPermissionFlag.AllFlags;
			Assert ("Flags(AllFlags).IsUnrestricted", p.IsUnrestricted ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromXmlNull () 
		{
			ReflectionPermission p = new ReflectionPermission (PermissionState.None);
			p.FromXml (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXmlInvalidPermission () 
		{
			ReflectionPermission p = new ReflectionPermission (PermissionState.None);
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
			ReflectionPermission p = new ReflectionPermission (PermissionState.None);
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
			ReflectionPermission p = new ReflectionPermission (PermissionState.None);
			SecurityElement se = p.ToXml ();
			AssertNotNull ("ToXml()", se);

			ReflectionPermission p2 = (ReflectionPermission) p.Copy ();
			p2.FromXml (se);
			AssertEquals ("FromXml-None", ReflectionPermissionFlag.NoFlags, p2.Flags);

			string className = (string) se.Attributes ["class"];
			string version = (string) se.Attributes ["version"];

			SecurityElement se2 = new SecurityElement (se.Tag);
			se2.AddAttribute ("class", className);
			se2.AddAttribute ("version", version);
			se2.AddAttribute ("Flags", "TypeInformation");
			p2.FromXml (se2);
			AssertEquals ("FromXml-TypeInformation", ReflectionPermissionFlag.TypeInformation, p2.Flags);

			se2 = new SecurityElement (se.Tag);
			se2.AddAttribute ("class", className);
			se2.AddAttribute ("version", version);
			se2.AddAttribute ("Flags", "MemberAccess");
			p2.FromXml (se2);
			AssertEquals ("FromXml-MemberAccess", ReflectionPermissionFlag.MemberAccess, p2.Flags);

			se2 = new SecurityElement (se.Tag);
			se2.AddAttribute ("class", className);
			se2.AddAttribute ("version", version);
			se2.AddAttribute ("Flags", "ReflectionEmit");
			p2.FromXml (se2);
			AssertEquals ("FromXml-ReflectionEmit", ReflectionPermissionFlag.ReflectionEmit, p2.Flags);

			se = p.ToXml ();
			se.AddAttribute ("Unrestricted", "true");
			p2.FromXml (se);
			Assert ("FromXml-Unrestricted", p2.IsUnrestricted ());
			AssertEquals ("FromXml-AllFlags", ReflectionPermissionFlag.AllFlags, p2.Flags);
		}

		[Test]
		public void UnionWithNull () 
		{
			ReflectionPermission p1 = new ReflectionPermission (ReflectionPermissionFlag.NoFlags);
			ReflectionPermission p2 = null;
			ReflectionPermission p3 = (ReflectionPermission) p1.Union (p2);
			AssertEquals ("P1 U null == P1", p1.ToXml ().ToString (), p3.ToXml ().ToString ());
		}

		[Test]
		public void UnionWithUnrestricted () 
		{
			ReflectionPermission p1 = new ReflectionPermission (PermissionState.Unrestricted);
			ReflectionPermission p2 = new ReflectionPermission (ReflectionPermissionFlag.NoFlags);
			ReflectionPermission p3 = (ReflectionPermission) p1.Union (p2);
			Assert ("Unrestricted U P2 == Unrestricted", p3.IsUnrestricted ());
			p3 = (ReflectionPermission) p2.Union (p1);
			Assert ("P2 U Unrestricted == Unrestricted", p3.IsUnrestricted ());
		}

		[Test]
		public void Union () 
		{
			ReflectionPermission p1 = new ReflectionPermission (ReflectionPermissionFlag.TypeInformation);
			ReflectionPermission p2 = new ReflectionPermission (ReflectionPermissionFlag.MemberAccess);
			ReflectionPermission p3 = (ReflectionPermission) p1.Union (p2);
			AssertEquals (ReflectionPermissionFlag.MemberAccess | ReflectionPermissionFlag.TypeInformation, p3.Flags);
			ReflectionPermission p4 = new ReflectionPermission (ReflectionPermissionFlag.ReflectionEmit);
			ReflectionPermission p5 = (ReflectionPermission) p4.Union (p3);
			Assert ("P3 U P4==Unrestricted", p5.IsUnrestricted ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void UnionWithBadPermission () 
		{
			ReflectionPermission p1 = new ReflectionPermission (ReflectionPermissionFlag.NoFlags);
			EnvironmentPermission ep2 = new EnvironmentPermission (PermissionState.Unrestricted);
			ReflectionPermission p3 = (ReflectionPermission) p1.Union (ep2);
		}

		[Test]
		public void IntersectWithNull () 
		{
			ReflectionPermission p1 = new ReflectionPermission (ReflectionPermissionFlag.NoFlags);
			ReflectionPermission p2 = null;
			ReflectionPermission p3 = (ReflectionPermission) p1.Intersect (p2);
			AssertNull ("P1 N null == null", p3);
		}

		[Test]
		public void IntersectWithUnrestricted () 
		{
			ReflectionPermission p1 = new ReflectionPermission (PermissionState.Unrestricted);
			ReflectionPermission p2 = new ReflectionPermission (ReflectionPermissionFlag.MemberAccess);
			ReflectionPermission p3 = (ReflectionPermission) p1.Intersect (p2);
			Assert ("Unrestricted N P2 == P2", !p3.IsUnrestricted ());
			AssertEquals ("Unrestricted N EP2 == EP2", p2.ToXml ().ToString (), p3.ToXml ().ToString ());
			p3 = (ReflectionPermission) p2.Intersect (p1);
			Assert ("P2 N Unrestricted == P2", !p3.IsUnrestricted ());
			AssertEquals ("P2 N Unrestricted == P2", p2.ToXml ().ToString (), p3.ToXml ().ToString ());

			p2 = new ReflectionPermission (ReflectionPermissionFlag.NoFlags);
			p3 = (ReflectionPermission) p1.Intersect (p2);
			AssertNull ("Unrestricted N None == null", p3);
		}

		[Test]
		public void Intersect () 
		{
			// no intersection
			ReflectionPermission p1 = new ReflectionPermission (ReflectionPermissionFlag.MemberAccess);
			ReflectionPermission p2 = new ReflectionPermission (ReflectionPermissionFlag.TypeInformation);
			ReflectionPermission p3 = (ReflectionPermission) p1.Intersect (p2);
			AssertNull ("EP1 N EP2 == null", p3);
			// intersection in MemberAccess
			ReflectionPermission p4 = new ReflectionPermission (ReflectionPermissionFlag.MemberAccess | ReflectionPermissionFlag.ReflectionEmit);
			p3 = (ReflectionPermission) p4.Intersect (p1);
			AssertEquals ("Intersect-MemberAccess", ReflectionPermissionFlag.MemberAccess, p3.Flags);
			// intersection in TypeInformation
			ReflectionPermission p5 = new ReflectionPermission (ReflectionPermissionFlag.TypeInformation | ReflectionPermissionFlag.ReflectionEmit);
			p3 = (ReflectionPermission) p5.Intersect (p2);
			AssertEquals ("Intersect-TypeInformation", ReflectionPermissionFlag.TypeInformation, p3.Flags);
			// intersection in AllFlags
			ReflectionPermission p6 = new ReflectionPermission (ReflectionPermissionFlag.AllFlags);
			ReflectionPermission p7 = new ReflectionPermission (ReflectionPermissionFlag.AllFlags);
			p3 = (ReflectionPermission) p6.Intersect (p7);
			AssertEquals ("Intersect-AllFlags", ReflectionPermissionFlag.AllFlags, p3.Flags);
			Assert ("Intersect-AllFlags-Unrestricted", p3.IsUnrestricted ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void IntersectWithBadPermission () 
		{
			ReflectionPermission p1 = new ReflectionPermission (ReflectionPermissionFlag.NoFlags);
			EnvironmentPermission ep2 = new EnvironmentPermission (PermissionState.Unrestricted);
			ReflectionPermission p3 = (ReflectionPermission) p1.Intersect (ep2);
		}

		[Test]
		public void IsSubsetOfNull () 
		{
			ReflectionPermission p = new ReflectionPermission (ReflectionPermissionFlag.NoFlags);
			Assert ("NoFlags.IsSubsetOf(null)", p.IsSubsetOf (null));
			p = new ReflectionPermission (ReflectionPermissionFlag.MemberAccess);
			Assert ("MemberAccess.IsSubsetOf(null)", !p.IsSubsetOf (null));
		}

		[Test]
		public void IsSubsetOfUnrestricted () 
		{
			ReflectionPermission p1 = new ReflectionPermission (PermissionState.Unrestricted);
			ReflectionPermission p2 = new ReflectionPermission (ReflectionPermissionFlag.NoFlags);
			ReflectionPermission p3 = new ReflectionPermission (PermissionState.Unrestricted);
			Assert ("Unrestricted.IsSubsetOf()", !p1.IsSubsetOf (p2));
			Assert ("IsSubsetOf(Unrestricted)", p2.IsSubsetOf (p1));
			Assert ("Unrestricted.IsSubsetOf(Unrestricted)", p1.IsSubsetOf (p3));
		}

		[Test]
		public void IsSubsetOf () 
		{
			ReflectionPermission p1 = new ReflectionPermission (ReflectionPermissionFlag.MemberAccess);
			ReflectionPermission p2 = new ReflectionPermission (ReflectionPermissionFlag.TypeInformation);
			ReflectionPermission p3 = new ReflectionPermission (ReflectionPermissionFlag.ReflectionEmit);
			Assert ("MemberAccess.IsSubsetOf(TypeInformation)", !p1.IsSubsetOf (p2));
			Assert ("MemberAccess.IsSubsetOf(ReflectionEmit)", !p1.IsSubsetOf (p3));
			Assert ("TypeInformation.IsSubsetOf(MemberAccess)", !p2.IsSubsetOf (p1));
			Assert ("TypeInformation.IsSubsetOf(ReflectionEmit)", !p2.IsSubsetOf (p3));
			Assert ("ReflectionEmit.IsSubsetOf(MemberAccess)", !p3.IsSubsetOf (p1));
			Assert ("ReflectionEmit.IsSubsetOf(TypeInformation)", !p3.IsSubsetOf (p2));
			ReflectionPermission p4 = new ReflectionPermission (ReflectionPermissionFlag.AllFlags);
			Assert ("MemberAccess.IsSubsetOf(AllFlags)", p1.IsSubsetOf (p4));
			Assert ("TypeInformation.IsSubsetOf(AllFlags)", p2.IsSubsetOf (p4));
			Assert ("ReflectionEmit.IsSubsetOf(AllFlags)", p3.IsSubsetOf (p4));
			Assert ("AllFlags.IsSubsetOf(MemberAccess)", !p4.IsSubsetOf (p1));
			Assert ("AllFlags.IsSubsetOf(TypeInformation)", !p4.IsSubsetOf (p2));
			Assert ("AllFlags.IsSubsetOf(ReflectionEmit)", !p4.IsSubsetOf (p3));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void IsSubsetOfBadPermission () 
		{
			ReflectionPermission p1 = new ReflectionPermission (ReflectionPermissionFlag.NoFlags);
			EnvironmentPermission ep2 = new EnvironmentPermission (PermissionState.Unrestricted);
			Assert ("IsSubsetOf(EnvironmentPermission)", p1.IsSubsetOf (ep2));
		}
	}
}
