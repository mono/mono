//
// GenericIdentityTest.cs - NUnit Test Cases for GenericIdentity
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
	public class GenericIdentityTest : Assertion {

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void NullName () 
		{
			GenericIdentity gi = new GenericIdentity (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void NullAuthenticationType () 
		{
			GenericIdentity gi = new GenericIdentity ("user", null);
		}

		[Test]
		public void Name () 
		{
			GenericIdentity gi = new GenericIdentity ("user");
			AssertEquals ("Name.Name", "user", gi.Name);
			AssertEquals ("Name.AuthenticationType", "", gi.AuthenticationType);
			Assert ("Name.IsAuthenticated", gi.IsAuthenticated);
		}

		[Test]
		public void NameAuthenticationType () 
		{
			GenericIdentity gi = new GenericIdentity ("user", "blood oath");
			AssertEquals ("NameAuthenticationType.Name", "user", gi.Name);
			AssertEquals ("NameAuthenticationType.AuthenticationType", "blood oath", gi.AuthenticationType);
			Assert ("NameAuthenticationType.IsAuthenticated", gi.IsAuthenticated);
		}

		[Test]
		public void EmptyName () 
		{
			GenericIdentity gi = new GenericIdentity ("");
			AssertEquals ("EmptyName.Name", "", gi.Name);
			AssertEquals ("EmptyName.AuthenticationType", "", gi.AuthenticationType);
			Assert ("EmptyName.IsAuthenticated", !gi.IsAuthenticated);
		}
	}
}
