// AuthorizationRuleTest.cs - NUnit Test Cases for AuthorizationRule
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
	public class AuthorizationRuleTest
	{
		class TestRule : AuthorizationRule
		{
			public TestRule (IdentityReference identity,
					int accessMask, bool isInherited,
					InheritanceFlags inheritanceFlags,
					PropagationFlags propagationFlags)
				: base (identity, accessMask, isInherited, inheritanceFlags, propagationFlags)
			{

			}
		}

		[Test, ExpectedException (typeof (ArgumentException))]
		public void ThrowOnZeroAccessMask ()
		{
			new TestRule (new SecurityIdentifier (WellKnownSidType.WorldSid, null),
				0, false, InheritanceFlags.None, PropagationFlags.None);
		}

		[Test, ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ThrowOnBadInheritanceFlags ()
		{
			new TestRule (new SecurityIdentifier (WellKnownSidType.WorldSid, null),
				1, false, (InheritanceFlags)(-1), PropagationFlags.None);
		}

		// While InheritanceFlags.None makes PropagationFlags not *significant*,
		// my tests with MS.NET show that it is still *validated*. So, we'll use
		// that case with this test to make sure.
		[Test, ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ThrowOnBadPropagationFlags ()
		{
			new TestRule (new SecurityIdentifier (WellKnownSidType.WorldSid, null),
				1, false, InheritanceFlags.None, (PropagationFlags)(-1));
		}

		[Test]
		public void AcceptNTAccount ()
		{
			new TestRule (new NTAccount ("Test"), 1, false, InheritanceFlags.None, PropagationFlags.None);				
		}

		[Test]
		public void AcceptSecurityIdentifier ()
		{
			new TestRule (new SecurityIdentifier (WellKnownSidType.WorldSid, null),
				1, false, InheritanceFlags.None, PropagationFlags.None);
		}

		[Test]
		public void AcceptValidFlags ()
		{
			SecurityIdentifier id = new SecurityIdentifier (WellKnownSidType.WorldSid, null);
			new TestRule (id, 1, false, InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit);				
			new TestRule (id, 1, false, InheritanceFlags.ObjectInherit, PropagationFlags.InheritOnly);				
		}
	}
}

