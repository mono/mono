//
// GenericPrincipalTest.cs - NUnit Test Cases for GenericPrincipal
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using System;
using System.Security.Principal;

namespace MonoTests.System.Security.Principal {

	[TestFixture]
	public class GenericPrincipalTest : Assertion {

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void NullIdentity () 
		{
			GenericPrincipal gp = new GenericPrincipal (null, new string [5]);
		}

		[Test]
		public void NullRoles () 
		{
			GenericIdentity gi = new GenericIdentity ("user");
			GenericPrincipal gp = new GenericPrincipal (gi, null);
			AssertEquals ("Identity", "user", gp.Identity.Name);
			Assert ("NoRole.IsInRole(x)", !gp.IsInRole ("role 1"));
		}

		[Test]
		public void IsInRole () 
		{
			GenericIdentity gi = new GenericIdentity ("user");
			string[] roles = new string [5];
			roles [0] = "role 1";
			GenericPrincipal gp = new GenericPrincipal (gi, roles);
			roles [1] = "role 2";
			Assert ("IsInRole (role added before constructor)", gp.IsInRole ("role 1"));
			Assert ("IsInRole (role added after constructor)", !gp.IsInRole ("role 2"));
		}
	}
}
