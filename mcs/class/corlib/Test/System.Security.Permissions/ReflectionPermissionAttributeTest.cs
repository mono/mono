//
// ReflectionPermissionAttributeTest.cs - NUnit Test Cases for ReflectionPermissionAttribute
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
	public class ReflectionPermissionAttributeTest : Assertion {

		[Test]
		public void Flags () 
		{
			ReflectionPermissionAttribute attr = new ReflectionPermissionAttribute (SecurityAction.Assert);
			attr.Flags = ReflectionPermissionFlag.MemberAccess;
			Assert ("Flags/MemberAccess=MemberAccess", attr.MemberAccess);
			Assert ("Flags/MemberAccess=ReflectionEmit", !attr.ReflectionEmit);
			Assert ("Flags/MemberAccess=TypeInformation", !attr.TypeInformation);
			attr.Flags |= ReflectionPermissionFlag.ReflectionEmit;
			Assert ("Flags/ReflectionEmit=MemberAccess", attr.MemberAccess);
			Assert ("Flags/ReflectionEmit=ReflectionEmit", attr.ReflectionEmit);
			Assert ("Flags/ReflectionEmit=TypeInformation", !attr.TypeInformation);
			attr.Flags |= ReflectionPermissionFlag.TypeInformation;
			Assert ("Flags/TypeInformation=MemberAccess", attr.MemberAccess);
			Assert ("Flags/TypeInformation=ReflectionEmit", attr.ReflectionEmit);
			Assert ("Flags/TypeInformation=TypeInformation", attr.TypeInformation);
			attr.Flags = ReflectionPermissionFlag.NoFlags;
			Assert ("Flags/NoFlags=MemberAccess", !attr.MemberAccess);
			Assert ("Flags/NoFlags=ReflectionEmit", !attr.ReflectionEmit);
			Assert ("Flags/NoFlags=TypeInformation", !attr.TypeInformation);
		}

		[Test]
		public void NoFlags () 
		{
			ReflectionPermissionAttribute attr = new ReflectionPermissionAttribute (SecurityAction.Assert);
			AssertEquals ("NoFlags.Flags", ReflectionPermissionFlag.NoFlags, attr.Flags);
			Assert ("NoFlags.Unrestricted", !attr.Unrestricted);
			Assert ("NoFlags=MemberAccess", !attr.MemberAccess);
			Assert ("NoFlags=ReflectionEmit", !attr.ReflectionEmit);
			Assert ("NoFlags=TypeInformation", !attr.TypeInformation);
			ReflectionPermission p = (ReflectionPermission) attr.CreatePermission ();
			AssertEquals ("NoFlags=ReflectionPermission", ReflectionPermissionFlag.NoFlags, p.Flags);
		}

		[Test]
		public void MemberAccess () 
		{
			ReflectionPermissionAttribute attr = new ReflectionPermissionAttribute (SecurityAction.Assert);
			attr.MemberAccess = true;
			AssertEquals ("MemberAccess.Flags", ReflectionPermissionFlag.MemberAccess, attr.Flags);
			Assert ("MemberAccess.Unrestricted", !attr.Unrestricted);
			Assert ("MemberAccess=MemberAccess", attr.MemberAccess);
			Assert ("MemberAccess=ReflectionEmit", !attr.ReflectionEmit);
			Assert ("MemberAccess=TypeInformation", !attr.TypeInformation);
			ReflectionPermission p = (ReflectionPermission) attr.CreatePermission ();
			AssertEquals ("MemberAccess=ReflectionPermission", ReflectionPermissionFlag.MemberAccess, p.Flags);
		}

		[Test]
		public void ReflectionEmit () 
		{
			ReflectionPermissionAttribute attr = new ReflectionPermissionAttribute (SecurityAction.Assert);
			attr.ReflectionEmit = true;
			AssertEquals ("ReflectionEmit.Flags", ReflectionPermissionFlag.ReflectionEmit, attr.Flags);
			Assert ("ReflectionEmit.Unrestricted", !attr.Unrestricted);
			Assert ("ReflectionEmit=MemberAccess", !attr.MemberAccess);
			Assert ("ReflectionEmit=ReflectionEmit", attr.ReflectionEmit);
			Assert ("ReflectionEmit=TypeInformation", !attr.TypeInformation);
			ReflectionPermission p = (ReflectionPermission) attr.CreatePermission ();
			AssertEquals ("ReflectionEmit=ReflectionPermission", ReflectionPermissionFlag.ReflectionEmit, p.Flags);
		}

		[Test]
		public void TypeInformation () 
		{
			ReflectionPermissionAttribute attr = new ReflectionPermissionAttribute (SecurityAction.Assert);
			attr.TypeInformation = true;
			AssertEquals ("TypeInformation.Flags", ReflectionPermissionFlag.TypeInformation, attr.Flags);
			Assert ("TypeInformation.Unrestricted", !attr.Unrestricted);
			Assert ("TypeInformation=MemberAccess", !attr.MemberAccess);
			Assert ("TypeInformation=ReflectionEmit", !attr.ReflectionEmit);
			Assert ("TypeInformation=TypeInformation", attr.TypeInformation);
			ReflectionPermission p = (ReflectionPermission) attr.CreatePermission ();
			AssertEquals ("TypeInformation=ReflectionPermission", ReflectionPermissionFlag.TypeInformation, p.Flags);
		}

		[Test]
		public void AllFlags () 
		{
			ReflectionPermissionAttribute attr = new ReflectionPermissionAttribute (SecurityAction.Assert);
			attr.MemberAccess = true;
			attr.ReflectionEmit = true;
			attr.TypeInformation = true;
			AssertEquals ("AllFlags.Flags", ReflectionPermissionFlag.AllFlags, attr.Flags);
			// attribute isn't unrestricted but the created permission is !!!
			Assert ("AllFlags.Unrestricted", !attr.Unrestricted);
			Assert ("AllFlags=MemberAccess", attr.MemberAccess);
			Assert ("AllFlags=ReflectionEmit", attr.ReflectionEmit);
			Assert ("AllFlags=TypeInformation", attr.TypeInformation);
			ReflectionPermission p = (ReflectionPermission) attr.CreatePermission ();
			AssertEquals ("AllFlags=ReflectionPermission", ReflectionPermissionFlag.AllFlags, p.Flags);
			Assert ("AllFlags=Unrestricted", p.IsUnrestricted ());
		}
	}
}
