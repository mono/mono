//
// SecurityExceptionTest.cs - NUnit Test Cases for SecurityException
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2004 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using System;
using System.Security;
using System.Security.Permissions;

namespace MonoTests.System.Security {

	[TestFixture]
	public class SecurityExceptionTest : Assertion {

		[Test]
		public void Constructor_Empty () 
		{
			SecurityException se = new SecurityException ();
#if ! NET_1_0
			AssertNull ("GrantedSet", se.GrantedSet);
			AssertNull ("RefusedSet", se.RefusedSet);
#endif
			AssertNull ("PermissionState", se.PermissionState);
			AssertNull ("PermissionType", se.PermissionType);
			Assert ("ToString()", se.ToString ().StartsWith ("System.Security.SecurityException: "));
		}

		[Test]
		public void Constructor_Message () 
		{
			SecurityException se = new SecurityException ("message");
#if ! NET_1_0
			AssertNull ("GrantedSet", se.GrantedSet);
			AssertNull ("RefusedSet", se.RefusedSet);
#endif
			AssertNull ("PermissionState", se.PermissionState);
			AssertNull ("PermissionType", se.PermissionType);
			AssertEquals ("ToString()", "System.Security.SecurityException: message", se.ToString ());
		}

		[Test]
		public void Constructor_MessageInner () 
		{
			SecurityException se = new SecurityException ("message", new Exception ());
#if ! NET_1_0
			AssertNull ("GrantedSet", se.GrantedSet);
			AssertNull ("RefusedSet", se.RefusedSet);
#endif
			AssertNull ("PermissionState", se.PermissionState);
			AssertNull ("PermissionType", se.PermissionType);
			Assert ("ToString().Starts", se.ToString ().StartsWith ("System.Security.SecurityException: message"));
			Assert ("ToString().Include", (se.ToString ().IndexOf ("System.Exception") > 0));
		}

		[Test]
		public void Constructor_MessageType () 
		{
			SecurityException se = new SecurityException ("message", typeof (EnvironmentPermission));
#if ! NET_1_0
			AssertNull ("GrantedSet", se.GrantedSet);
			AssertNull ("RefusedSet", se.RefusedSet);
#endif
			AssertNull ("PermissionState", se.PermissionState);
			AssertEquals ("PermissionType", typeof (EnvironmentPermission), se.PermissionType);

			Assert ("ToString().Starts", se.ToString ().StartsWith ("System.Security.SecurityException: message"));
			// note: can't check for PermissionType as it's not shown with MS class lib
		}

		[Test]
		public void Constructor_MessageTypeState () 
		{
			SecurityException se = new SecurityException ("message", typeof (EnvironmentPermission), "mono");
#if ! NET_1_0
			AssertNull ("GrantedSet", se.GrantedSet);
			AssertNull ("RefusedSet", se.RefusedSet);
#endif
			AssertEquals ("PermissionState", "mono", se.PermissionState);
			AssertEquals ("PermissionType", typeof (EnvironmentPermission), se.PermissionType);

			Assert ("ToString().Include(mono)", (se.ToString ().IndexOf ("mono") > 0));
			// note: can't check for PermissionType as it's not shown with MS class lib
		}
	}
}
