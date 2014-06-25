//
// WindowsIdentityTest.cs - NUnit Test Cases for WindowsIdentity
//
// Author:
//	Sebastien Pouliot (sebastien@ximian.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
//
#if NET_4_5
using NUnit.Framework;
using System;
using System.Security.Claims;

namespace MonoTests.System.Security.Claims {

	[TestFixture]
	public class ClaimsIdentityTest {

		[Test]
		public void EmptyCtorWorks () {
			var id = new ClaimsIdentity ();
			Assert.IsNull (id.AuthenticationType, "#1");
			
		}
	}
}
#endif
