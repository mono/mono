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
	public class SecurityExceptionTest {

		[Test]
		public void Constructor_Empty () 
		{
			SecurityException se = new SecurityException ();
#if ! NET_1_0
			Assert.IsNull (se.GrantedSet, "GrantedSet");
			Assert.IsNull (se.RefusedSet, "RefusedSet");
#endif
			Assert.IsNull (se.PermissionState, "PermissionState");
			Assert.IsNull (se.PermissionType, "PermissionType");
			Assert.IsTrue (se.ToString ().StartsWith ("System.Security.SecurityException: "), "ToString()");
		}

		[Test]
		public void Constructor_Message () 
		{
			SecurityException se = new SecurityException ("message");
#if ! NET_1_0
			Assert.IsNull (se.GrantedSet, "GrantedSet");
			Assert.IsNull (se.RefusedSet, "RefusedSet");
#endif
			Assert.IsNull (se.PermissionState, "PermissionState");
			Assert.IsNull (se.PermissionType, "PermissionType");
			Assert.AreEqual ("System.Security.SecurityException: message", se.ToString (), "ToString()");
		}

		[Test]
		public void Constructor_MessageInner () 
		{
			SecurityException se = new SecurityException ("message", new Exception ());
#if ! NET_1_0
			Assert.IsNull (se.GrantedSet, "GrantedSet");
			Assert.IsNull (se.RefusedSet, "RefusedSet");
#endif
			Assert.IsNull (se.PermissionState, "PermissionState");
			Assert.IsNull (se.PermissionType, "PermissionType");
			Assert.IsTrue (se.ToString ().StartsWith ("System.Security.SecurityException: message"), "ToString().Starts");
			Assert.IsTrue ((se.ToString ().IndexOf ("System.Exception") > 0), "ToString().Include");
		}

		[Test]
		public void Constructor_MessageType () 
		{
			SecurityException se = new SecurityException ("message", typeof (EnvironmentPermission));
#if ! NET_1_0
			Assert.IsNull (se.GrantedSet, "GrantedSet");
			Assert.IsNull (se.RefusedSet, "RefusedSet");
#endif
			Assert.IsNull (se.PermissionState, "PermissionState");
			Assert.AreEqual (typeof (EnvironmentPermission), se.PermissionType, "PermissionType");

			Assert.IsTrue (se.ToString ().StartsWith ("System.Security.SecurityException: message"), "ToString().Starts");
			// note: can't check for PermissionType as it's not shown with MS class lib
		}

		[Test]
		public void Constructor_MessageTypeState () 
		{
			SecurityException se = new SecurityException ("message", typeof (EnvironmentPermission), "mono");
			Assert.IsNull (se.GrantedSet, "GrantedSet");
			Assert.IsNull (se.RefusedSet, "RefusedSet");
			Assert.AreEqual ("mono", se.PermissionState, "PermissionState");
			Assert.AreEqual (typeof (EnvironmentPermission), se.PermissionType, "PermissionType");

#if !MOBILE
			Assert.IsTrue ((se.ToString ().IndexOf ("mono") > 0), "ToString().Include(mono)");
#endif
			// note: can't check for PermissionType as it's not shown with MS class lib
		}
	}
}
