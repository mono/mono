//
// FileIOPermissionAttributeTest.cs - NUnit Test Cases for FileIOPermissionAttribute
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
	public class FileIOPermissionAttributeTest : Assertion {

		private static string filename = @"c:\mono.txt";

		[Test]
		public void All () 
		{
			FileIOPermissionAttribute attr = new FileIOPermissionAttribute (SecurityAction.Assert);
			attr.All = filename;
			AssertEquals ("All=Append", filename, attr.Append);
			AssertEquals ("All=PathDiscovery", filename, attr.PathDiscovery);
			AssertEquals ("All=Read", filename, attr.Read);
			AssertEquals ("All=Write", filename, attr.Write);
			FileIOPermission p = (FileIOPermission) attr.CreatePermission ();
			AssertEquals ("All=FileIOPermissionAttribute-Append", filename, p.GetPathList (FileIOPermissionAccess.Append)[0]);
			AssertEquals ("All=FileIOPermissionAttribute-PathDiscovery", filename, p.GetPathList (FileIOPermissionAccess.PathDiscovery)[0]);
			AssertEquals ("All=FileIOPermissionAttribute-Read", filename, p.GetPathList (FileIOPermissionAccess.Read)[0]);
			AssertEquals ("All=FileIOPermissionAttribute-Write", filename, p.GetPathList (FileIOPermissionAccess.Write)[0]);
		}

		[Test]
		public void Append ()
		{
			FileIOPermissionAttribute attr = new FileIOPermissionAttribute (SecurityAction.Assert);
			attr.Append = filename;
			AssertEquals ("Append=Append", filename, attr.Append);
			AssertNull ("PathDiscovery=null", attr.PathDiscovery);
			AssertNull ("Read=null", attr.Read);
			AssertNull ("Write=null", attr.Write);
			FileIOPermission p = (FileIOPermission) attr.CreatePermission ();
			AssertEquals ("Append=FileIOPermissionAttribute-Append", filename, p.GetPathList (FileIOPermissionAccess.Append)[0]);
			AssertNull ("Append=FileIOPermissionAttribute-PathDiscovery", p.GetPathList (FileIOPermissionAccess.PathDiscovery));
			AssertNull ("Append=FileIOPermissionAttribute-Read", p.GetPathList (FileIOPermissionAccess.Read));
			AssertNull ("Append=FileIOPermissionAttribute-Write", p.GetPathList (FileIOPermissionAccess.Write));
		}

		[Test]
		public void PathDiscovery () 
		{
			FileIOPermissionAttribute attr = new FileIOPermissionAttribute (SecurityAction.Assert);
			attr.PathDiscovery = filename;
			AssertNull ("Append=null", attr.Append);
			AssertEquals ("PathDiscovery=PathDiscovery", filename, attr.PathDiscovery);
			AssertNull ("Read=null", attr.Read);
			AssertNull ("Write=null", attr.Write);
			FileIOPermission p = (FileIOPermission) attr.CreatePermission ();
			AssertNull ("PathDiscovery=FileIOPermissionAttribute-Append", p.GetPathList (FileIOPermissionAccess.Append));
			AssertEquals ("PathDiscovery=FileIOPermissionAttribute-PathDiscovery", filename, p.GetPathList (FileIOPermissionAccess.PathDiscovery)[0]);
			AssertNull ("PathDiscovery=FileIOPermissionAttribute-Read", p.GetPathList (FileIOPermissionAccess.Read));
			AssertNull ("PathDiscovery=FileIOPermissionAttribute-Write", p.GetPathList (FileIOPermissionAccess.Write));
		}

		[Test]
		public void Read () 
		{
			FileIOPermissionAttribute attr = new FileIOPermissionAttribute (SecurityAction.Assert);
			attr.Read = filename;
			AssertNull ("Append=null", attr.Append);
			AssertNull ("PathDiscovery=null", attr.PathDiscovery);
			AssertEquals ("Read=Read", filename, attr.Read);
			AssertNull ("Write=null", attr.Write);
			FileIOPermission p = (FileIOPermission) attr.CreatePermission ();
			AssertNull ("PathDiscovery=FileIOPermissionAttribute-Append", p.GetPathList (FileIOPermissionAccess.Append));
			AssertNull ("PathDiscovery=FileIOPermissionAttribute-PathDiscovery", p.GetPathList (FileIOPermissionAccess.PathDiscovery));
			AssertEquals ("PathDiscovery=FileIOPermissionAttribute-Read", filename, p.GetPathList (FileIOPermissionAccess.Read)[0]);
			AssertNull ("PathDiscovery=FileIOPermissionAttribute-Write", p.GetPathList (FileIOPermissionAccess.Write));
		}

		[Test]
		public void Write () 
		{
			FileIOPermissionAttribute attr = new FileIOPermissionAttribute (SecurityAction.Assert);
			attr.Write = filename;
			AssertNull ("Append=null", attr.Append);
			AssertNull ("PathDiscovery=null", attr.PathDiscovery);
			AssertNull ("Read=null", attr.Read);
			AssertEquals ("Write=Write", filename, attr.Write);
			FileIOPermission p = (FileIOPermission) attr.CreatePermission ();
			AssertNull ("PathDiscovery=FileIOPermissionAttribute-Append", p.GetPathList (FileIOPermissionAccess.Append));
			AssertNull ("PathDiscovery=FileIOPermissionAttribute-PathDiscovery", p.GetPathList (FileIOPermissionAccess.PathDiscovery));
			AssertNull ("PathDiscovery=FileIOPermissionAttribute-Read", p.GetPathList (FileIOPermissionAccess.Read));
			AssertEquals ("PathDiscovery=FileIOPermissionAttribute-Write", filename, p.GetPathList (FileIOPermissionAccess.Write)[0]);
		}
	}
}
