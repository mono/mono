// MutexAccessRuleTest - NUnit Test Cases for MutexAccessRule
//
// Authors:
//	James Bellinger (jfb@zer7.com)

using System;
using System.Security.AccessControl;
using System.Security.Principal;
using NUnit.Framework;

namespace MonoTests.System.Security.AccessControl
{
	[TestFixture]
	public class MutexAccessRuleTest
	{
		[Test]
		public void ConstructsWithoutCrashingAndRemembersRights ()
		{
			SecurityIdentifier id = new SecurityIdentifier (WellKnownSidType.WorldSid, null);
			MutexAccessRule rule = new MutexAccessRule (id, MutexRights.FullControl, AccessControlType.Allow);
			Assert.AreEqual (rule.MutexRights, MutexRights.FullControl);
		}
	}
}

