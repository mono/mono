//
// GenericPrincipalTest.cs - NUnit Test Cases for GenericPrincipal
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//

using NUnit.Framework;
using System;
using System.Security.Principal;

namespace MonoTests.System.Security.Principal {

	[TestFixture]
	public class GenericPrincipalTest {

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
			Assert.AreEqual ("user", gp.Identity.Name, "Identity");
			Assert.IsFalse (gp.IsInRole ("role 1"), "NoRole.IsInRole(x)");
		}

		[Test]
		public void IsInRole () 
		{
			GenericIdentity gi = new GenericIdentity ("user");
			string[] roles = new string [5];
			roles [0] = "role 1";
			GenericPrincipal gp = new GenericPrincipal (gi, roles);
			roles [1] = "role 2";
			Assert.IsTrue (gp.IsInRole ("role 1"), "IsInRole (role added before constructor)");
			Assert.IsFalse (gp.IsInRole ("role 2"), "IsInRole (role added after constructor)");
		}

		[Test]
		public void IsInRole_CaseInsensitive ()
		{
			GenericIdentity gi = new GenericIdentity ("user");
			GenericPrincipal gp = new GenericPrincipal (gi, new string[2] { "mono", "hackers" });
			Assert.IsTrue (gp.IsInRole ("MoNo"), "MoNo");
			Assert.IsTrue (gp.IsInRole ("hAcKeRs"), "hAcKeRs");
		}
	}
}
