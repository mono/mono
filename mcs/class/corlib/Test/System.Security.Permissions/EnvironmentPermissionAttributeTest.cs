//
// EnvironmentPermissionTest.cs - NUnit Test Cases for EnvironmentPermission
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
	public class EnvironmentPermissionAttributeTest : Assertion {

		private static string envar = "TMP";

		[Test]
		public void All () 
		{
			EnvironmentPermissionAttribute attr = new EnvironmentPermissionAttribute (SecurityAction.Assert);
			attr.All = envar;
			AssertEquals ("All=Read", envar, attr.Read);
			AssertEquals ("All=Write", envar, attr.Write);
			EnvironmentPermission p = (EnvironmentPermission) attr.CreatePermission ();
			AssertEquals ("All=EnvironmentPermission-Read", envar, p.GetPathList (EnvironmentPermissionAccess.Read));
			AssertEquals ("All=EnvironmentPermission-Write", envar, p.GetPathList (EnvironmentPermissionAccess.Write));
		}

		[Test]
		public void Read () 
		{
			EnvironmentPermissionAttribute attr = new EnvironmentPermissionAttribute (SecurityAction.Assert);
			attr.Read = envar;
			AssertEquals ("Read=Read", envar, attr.Read);
			AssertNull ("Write=null", attr.Write);
			EnvironmentPermission p = (EnvironmentPermission) attr.CreatePermission ();
			AssertEquals ("Read=EnvironmentPermission-Read", envar, p.GetPathList (EnvironmentPermissionAccess.Read));
			AssertNull ("Read=EnvironmentPermission-Write", p.GetPathList (EnvironmentPermissionAccess.Write));
		}

		[Test]
		public void Write ()
		{
			EnvironmentPermissionAttribute attr = new EnvironmentPermissionAttribute (SecurityAction.Assert);
			attr.Write = envar;
			AssertNull ("Read=null", attr.Read);
			AssertEquals ("Write=Write", envar, attr.Write);
			EnvironmentPermission p = (EnvironmentPermission) attr.CreatePermission ();
			AssertNull ("Write=EnvironmentPermission-Read", p.GetPathList (EnvironmentPermissionAccess.Read));
			AssertEquals ("Write=EnvironmentPermission-Write", envar, p.GetPathList (EnvironmentPermissionAccess.Write));
		}
	}
}
