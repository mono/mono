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
	public class ReflectionPermissionTest {

		private static string className = "System.Security.Permissions.ReflectionPermission, ";

		[Test]
		public void PermissionStateNone ()
		{
			ReflectionPermission p = new ReflectionPermission (PermissionState.None);
			Assert.IsNotNull (p, "ReflectionPermission(PermissionState.None)");
			Assert.IsTrue (!p.IsUnrestricted (), "IsUnrestricted");
			ReflectionPermission copy = (ReflectionPermission) p.Copy ();
			Assert.AreEqual (p.IsUnrestricted (), copy.IsUnrestricted (), "Copy.IsUnrestricted");
			SecurityElement se = p.ToXml ();
			Assert.IsTrue ((se.Attributes ["class"] as string).StartsWith (className), "ToXml-class");
			Assert.AreEqual ("1", (se.Attributes ["version"] as string), "ToXml-version");
		}

		[Test]
		public void PermissionStateUnrestricted () 
		{
			ReflectionPermission p = new ReflectionPermission (PermissionState.Unrestricted);
			Assert.IsNotNull (p, "ReflectionPermission(PermissionState.Unrestricted)");
			Assert.IsTrue (p.IsUnrestricted (), "IsUnrestricted");
			ReflectionPermission copy = (ReflectionPermission) p.Copy ();
			Assert.AreEqual (p.IsUnrestricted (), copy.IsUnrestricted (), "Copy.IsUnrestricted");
			SecurityElement se = p.ToXml ();
			Assert.AreEqual ("true", (se.Attributes ["Unrestricted"] as string), "ToXml-Unrestricted");
		}

		[Test]
		public void Derestricted () 
		{
			ReflectionPermission p = new ReflectionPermission (PermissionState.Unrestricted);
			Assert.IsTrue (p.IsUnrestricted (), "IsUnrestricted");
			p.Flags = ReflectionPermissionFlag.NoFlags;
			Assert.IsTrue (!p.IsUnrestricted (), "IsUnrestricted");
		}

		[Test]
		public void NoFlags () 
		{
			ReflectionPermission p = new ReflectionPermission (ReflectionPermissionFlag.NoFlags);
			Assert.IsTrue (!p.IsUnrestricted (), "IsUnrestricted");
			SecurityElement se = p.ToXml ();
			Assert.AreEqual ("NoFlags", (se.Attributes ["Flags"] as string), "ToXml-Flags=NoFlags");
		}

		[Test]
		public void TypeInformation () 
		{
			ReflectionPermission p = new ReflectionPermission (ReflectionPermissionFlag.TypeInformation);
			Assert.IsTrue (!p.IsUnrestricted (), "IsUnrestricted");
			SecurityElement se = p.ToXml ();
			Assert.AreEqual ("TypeInformation", (se.Attributes ["Flags"] as string), "ToXml-Flags=TypeInformation");
		}

		[Test]
		public void MemberAccess () 
		{
			ReflectionPermission p = new ReflectionPermission (ReflectionPermissionFlag.MemberAccess);
			Assert.IsTrue (!p.IsUnrestricted (), "IsUnrestricted");
			SecurityElement se = p.ToXml ();
			Assert.AreEqual ("MemberAccess", (se.Attributes ["Flags"] as string), "ToXml-Flags=MemberAccess");
		}

		[Test]
		public void ReflectionEmit () 
		{
			ReflectionPermission p = new ReflectionPermission (ReflectionPermissionFlag.ReflectionEmit);
			Assert.IsTrue (!p.IsUnrestricted (), "IsUnrestricted");
			SecurityElement se = p.ToXml ();
			Assert.AreEqual ("ReflectionEmit", (se.Attributes ["Flags"] as string), "ToXml-Flags=ReflectionEmit");
		}

		[Test]
		public void AllFlags () 
		{
			ReflectionPermission p = new ReflectionPermission (ReflectionPermissionFlag.AllFlags);
			Assert.IsTrue (p.IsUnrestricted (), "IsUnrestricted");
			SecurityElement se = p.ToXml ();
			Assert.AreEqual ("true", (se.Attributes ["Unrestricted"] as string), "ToXml-Unrestricted");
		}

		[Test]
		public void Flags () 
		{
			ReflectionPermission p = new ReflectionPermission (PermissionState.None);
			Assert.IsTrue (!p.IsUnrestricted (), "Flags(default).IsUnrestricted");
			p.Flags = ReflectionPermissionFlag.NoFlags;
			Assert.IsTrue (!p.IsUnrestricted (), "Flags(NoFlags).IsUnrestricted");
			p.Flags = ReflectionPermissionFlag.TypeInformation;
			Assert.IsTrue (!p.IsUnrestricted (), "Flags(TypeInformation).IsUnrestricted");
			p.Flags = ReflectionPermissionFlag.MemberAccess;
			Assert.IsTrue (!p.IsUnrestricted (), "Flags(MemberAccess).IsUnrestricted");
			p.Flags = ReflectionPermissionFlag.ReflectionEmit;
			Assert.IsTrue (!p.IsUnrestricted (), "Flags(ReflectionEmit).IsUnrestricted");
			p.Flags = ReflectionPermissionFlag.AllFlags;
			Assert.IsTrue (p.IsUnrestricted (), "Flags(AllFlags).IsUnrestricted");
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
			Assert.IsNotNull (se, "ToXml()");

			ReflectionPermission p2 = (ReflectionPermission) p.Copy ();
			p2.FromXml (se);
			Assert.AreEqual (ReflectionPermissionFlag.NoFlags, p2.Flags, "FromXml-None");

			string className = (string) se.Attributes ["class"];
			string version = (string) se.Attributes ["version"];

			SecurityElement se2 = new SecurityElement (se.Tag);
			se2.AddAttribute ("class", className);
			se2.AddAttribute ("version", version);
			se2.AddAttribute ("Flags", "TypeInformation");
			p2.FromXml (se2);
			Assert.AreEqual (ReflectionPermissionFlag.TypeInformation, p2.Flags, "FromXml-TypeInformation");

			se2 = new SecurityElement (se.Tag);
			se2.AddAttribute ("class", className);
			se2.AddAttribute ("version", version);
			se2.AddAttribute ("Flags", "MemberAccess");
			p2.FromXml (se2);
			Assert.AreEqual (ReflectionPermissionFlag.MemberAccess, p2.Flags, "FromXml-MemberAccess");

			se2 = new SecurityElement (se.Tag);
			se2.AddAttribute ("class", className);
			se2.AddAttribute ("version", version);
			se2.AddAttribute ("Flags", "ReflectionEmit");
			p2.FromXml (se2);
			Assert.AreEqual (ReflectionPermissionFlag.ReflectionEmit, p2.Flags, "FromXml-ReflectionEmit");

			se = p.ToXml ();
			se.AddAttribute ("Unrestricted", "true");
			p2.FromXml (se);
			Assert.IsTrue (p2.IsUnrestricted (), "FromXml-Unrestricted");
			Assert.AreEqual (ReflectionPermissionFlag.AllFlags, p2.Flags, "FromXml-AllFlags");
		}

		[Test]
		public void UnionWithNull () 
		{
			ReflectionPermission p1 = new ReflectionPermission (ReflectionPermissionFlag.NoFlags);
			ReflectionPermission p2 = null;
			ReflectionPermission p3 = (ReflectionPermission) p1.Union (p2);
			Assert.AreEqual (p1.ToXml ().ToString (), p3.ToXml ().ToString (), "P1 U null == P1");
		}

		[Test]
		public void UnionWithUnrestricted () 
		{
			ReflectionPermission p1 = new ReflectionPermission (PermissionState.Unrestricted);
			ReflectionPermission p2 = new ReflectionPermission (ReflectionPermissionFlag.NoFlags);
			ReflectionPermission p3 = (ReflectionPermission) p1.Union (p2);
			Assert.IsTrue (p3.IsUnrestricted (), "Unrestricted U P2 == Unrestricted");
			p3 = (ReflectionPermission) p2.Union (p1);
			Assert.IsTrue (p3.IsUnrestricted (), "P2 U Unrestricted == Unrestricted");
		}

		[Test]
		public void Union () 
		{
			ReflectionPermission p1 = new ReflectionPermission (ReflectionPermissionFlag.TypeInformation);
			ReflectionPermission p2 = new ReflectionPermission (ReflectionPermissionFlag.MemberAccess);
			ReflectionPermission p3 = (ReflectionPermission) p1.Union (p2);
			Assert.AreEqual (ReflectionPermissionFlag.MemberAccess | ReflectionPermissionFlag.TypeInformation, p3.Flags);
			ReflectionPermission p4 = new ReflectionPermission (ReflectionPermissionFlag.ReflectionEmit);
			ReflectionPermission p5 = (ReflectionPermission) p4.Union (p3);
			Assert.IsTrue (p5.IsUnrestricted (), "P3 U P4==Unrestricted");
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
			Assert.IsNull (p3, "P1 N null == null");
		}

		[Test]
		public void IntersectWithUnrestricted () 
		{
			ReflectionPermission p1 = new ReflectionPermission (PermissionState.Unrestricted);
			ReflectionPermission p2 = new ReflectionPermission (ReflectionPermissionFlag.MemberAccess);
			ReflectionPermission p3 = (ReflectionPermission) p1.Intersect (p2);
			Assert.IsTrue (!p3.IsUnrestricted (), "Unrestricted N P2 == P2");
			Assert.AreEqual (p2.ToXml ().ToString (), p3.ToXml ().ToString (), "Unrestricted N EP2 == EP2");
			p3 = (ReflectionPermission) p2.Intersect (p1);
			Assert.IsTrue (!p3.IsUnrestricted (), "P2 N Unrestricted == P2");
			Assert.AreEqual (p2.ToXml ().ToString (), p3.ToXml ().ToString (), "P2 N Unrestricted == P2");

			p2 = new ReflectionPermission (ReflectionPermissionFlag.NoFlags);
			p3 = (ReflectionPermission) p1.Intersect (p2);
			Assert.IsNull (p3, "Unrestricted N None == null");
		}

		[Test]
		public void Intersect () 
		{
			// no intersection
			ReflectionPermission p1 = new ReflectionPermission (ReflectionPermissionFlag.MemberAccess);
			ReflectionPermission p2 = new ReflectionPermission (ReflectionPermissionFlag.TypeInformation);
			ReflectionPermission p3 = (ReflectionPermission) p1.Intersect (p2);
			Assert.IsNull (p3, "EP1 N EP2 == null");
			// intersection in MemberAccess
			ReflectionPermission p4 = new ReflectionPermission (ReflectionPermissionFlag.MemberAccess | ReflectionPermissionFlag.ReflectionEmit);
			p3 = (ReflectionPermission) p4.Intersect (p1);
			Assert.AreEqual (ReflectionPermissionFlag.MemberAccess, p3.Flags, "Intersect-MemberAccess");
			// intersection in TypeInformation
			ReflectionPermission p5 = new ReflectionPermission (ReflectionPermissionFlag.TypeInformation | ReflectionPermissionFlag.ReflectionEmit);
			p3 = (ReflectionPermission) p5.Intersect (p2);
			Assert.AreEqual (ReflectionPermissionFlag.TypeInformation, p3.Flags, "Intersect-TypeInformation");
			// intersection in AllFlags
			ReflectionPermission p6 = new ReflectionPermission (ReflectionPermissionFlag.AllFlags);
			ReflectionPermission p7 = new ReflectionPermission (ReflectionPermissionFlag.AllFlags);
			p3 = (ReflectionPermission) p6.Intersect (p7);
			Assert.AreEqual (ReflectionPermissionFlag.AllFlags, p3.Flags, "Intersect-AllFlags");
			Assert.IsTrue (p3.IsUnrestricted (), "Intersect-AllFlags-Unrestricted");
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
			Assert.IsTrue (p.IsSubsetOf (null), "NoFlags.IsSubsetOf(null)");
			p = new ReflectionPermission (ReflectionPermissionFlag.MemberAccess);
			Assert.IsTrue (!p.IsSubsetOf (null), "MemberAccess.IsSubsetOf(null)");
		}

		[Test]
		public void IsSubsetOfUnrestricted () 
		{
			ReflectionPermission p1 = new ReflectionPermission (PermissionState.Unrestricted);
			ReflectionPermission p2 = new ReflectionPermission (ReflectionPermissionFlag.NoFlags);
			ReflectionPermission p3 = new ReflectionPermission (PermissionState.Unrestricted);
			Assert.IsTrue (!p1.IsSubsetOf (p2), "Unrestricted.IsSubsetOf()");
			Assert.IsTrue (p2.IsSubsetOf (p1), "IsSubsetOf(Unrestricted)");
			Assert.IsTrue (p1.IsSubsetOf (p3), "Unrestricted.IsSubsetOf(Unrestricted)");
		}

		[Test]
		public void IsSubsetOf () 
		{
			ReflectionPermission p1 = new ReflectionPermission (ReflectionPermissionFlag.MemberAccess);
			ReflectionPermission p2 = new ReflectionPermission (ReflectionPermissionFlag.TypeInformation);
			ReflectionPermission p3 = new ReflectionPermission (ReflectionPermissionFlag.ReflectionEmit);
			Assert.IsTrue (!p1.IsSubsetOf (p2), "MemberAccess.IsSubsetOf(TypeInformation)");
			Assert.IsTrue (!p1.IsSubsetOf (p3), "MemberAccess.IsSubsetOf(ReflectionEmit)");
			Assert.IsTrue (!p2.IsSubsetOf (p1), "TypeInformation.IsSubsetOf(MemberAccess)");
			Assert.IsTrue (!p2.IsSubsetOf (p3), "TypeInformation.IsSubsetOf(ReflectionEmit)");
			Assert.IsTrue (!p3.IsSubsetOf (p1), "ReflectionEmit.IsSubsetOf(MemberAccess)");
			Assert.IsTrue (!p3.IsSubsetOf (p2), "ReflectionEmit.IsSubsetOf(TypeInformation)");
			ReflectionPermission p4 = new ReflectionPermission (ReflectionPermissionFlag.AllFlags);
			Assert.IsTrue (p1.IsSubsetOf (p4), "MemberAccess.IsSubsetOf(AllFlags)");
			Assert.IsTrue (p2.IsSubsetOf (p4), "TypeInformation.IsSubsetOf(AllFlags)");
			Assert.IsTrue (p3.IsSubsetOf (p4), "ReflectionEmit.IsSubsetOf(AllFlags)");
			Assert.IsTrue (!p4.IsSubsetOf (p1), "AllFlags.IsSubsetOf(MemberAccess)");
			Assert.IsTrue (!p4.IsSubsetOf (p2), "AllFlags.IsSubsetOf(TypeInformation)");
			Assert.IsTrue (!p4.IsSubsetOf (p3), "AllFlags.IsSubsetOf(ReflectionEmit)");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void IsSubsetOfBadPermission () 
		{
			ReflectionPermission p1 = new ReflectionPermission (ReflectionPermissionFlag.NoFlags);
			EnvironmentPermission ep2 = new EnvironmentPermission (PermissionState.Unrestricted);
			Assert.IsTrue (p1.IsSubsetOf (ep2), "IsSubsetOf(EnvironmentPermission)");
		}
	}
}
