// CryptoKeyAccessRuleTest.cs - NUnit Test Cases for CryptoKeyAccessRule
//
// Authors:
//	James Bellinger  <jfb@zer7.com>
//
// Copyright (C) 2012 James Bellinger

using System;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Security.Principal;
using NUnit.Framework;

namespace MonoTests.System.Security.AccessControl
{
	[TestFixture]
	public class CryptoKeyAccessRuleTest
	{
		[Test]
		public void StringOverloadIsNTAccount ()
		{
			CryptoKeyAccessRule rule;
			rule = new CryptoKeyAccessRule (@"BUILTIN\Users", CryptoKeyRights.FullControl, AccessControlType.Allow);
			Assert.AreNotEqual (new SecurityIdentifier ("BU"), rule.IdentityReference);
			Assert.AreEqual (new NTAccount (@"BUILTIN\Users"), rule.IdentityReference);
		}

		[Test]
		public void StringOverloadIsNotSID ()
		{
			CryptoKeyAccessRule rule;
			rule = new CryptoKeyAccessRule (@"S-1-5-32-545", CryptoKeyRights.FullControl, AccessControlType.Allow);
			Assert.AreNotEqual (new SecurityIdentifier ("S-1-5-32-545"), rule.IdentityReference);
			Assert.AreEqual (new NTAccount (@"S-1-5-32-545"), rule.IdentityReference);
		}
	}
}

