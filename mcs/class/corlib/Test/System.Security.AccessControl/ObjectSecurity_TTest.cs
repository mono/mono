// ObjectSecurity_TTest.cs - NUnit Test Cases for ObjectSecurity<T>
//
// Authors:
//	James Bellinger (jfb@zer7.com)

#if NET_4_0

using System;
using System.Security.AccessControl;
using System.Security.Principal;
using NUnit.Framework;

namespace MonoTests.System.Security.AccessControl
{
	[TestFixture]
	public class ObjectSecurity_TTest
	{
		enum WillWorkRights
		{
			Value = 1
		}

		class WillWorkSecurity : ObjectSecurity<WillWorkRights>
		{
			public WillWorkSecurity ()
				: base (false, ResourceType.Unknown)
			{

			}
		}

		struct WillFailRights
		{

		}

		class WillFailSecurity : ObjectSecurity<WillFailRights>
		{
			public WillFailSecurity ()
				: base (false, ResourceType.Unknown)
			{

			}
		}

		[Test]
		public void TypesAreCorrect ()
		{
			WillWorkSecurity security = new WillWorkSecurity ();
			Assert.AreEqual (security.AccessRightType, typeof (WillWorkRights));
			Assert.AreEqual (security.AccessRuleType, typeof (AccessRule<WillWorkRights>));
			Assert.AreEqual (security.AuditRuleType, typeof (AuditRule<WillWorkRights>));
		}

		[Test]
		public void WillWorkOKUsingAccessFactory ()
		{
			WillWorkSecurity security = new WillWorkSecurity ();
			SecurityIdentifier id = new SecurityIdentifier (WellKnownSidType.WorldSid, null);
			AccessRule<WillWorkRights> rule = (AccessRule<WillWorkRights>)
				security.AccessRuleFactory (id, 1, false,
					InheritanceFlags.None, PropagationFlags.None,
					AccessControlType.Allow);
			Assert.AreEqual (rule.AccessControlType, AccessControlType.Allow);
			Assert.AreEqual (rule.IdentityReference, id);
			Assert.AreEqual (rule.InheritanceFlags, InheritanceFlags.None);
			Assert.AreEqual (rule.PropagationFlags, PropagationFlags.None);
			Assert.AreEqual (rule.Rights, WillWorkRights.Value);
		}

		[Test]
		public void WillWorkOKUsingConstructor()
		{
			SecurityIdentifier id = new SecurityIdentifier (WellKnownSidType.WorldSid, null);
			AccessRule<WillWorkRights> rule = new AccessRule<WillWorkRights> (id, WillWorkRights.Value,
											  AccessControlType.Allow);
			Assert.AreEqual (rule.AccessControlType, AccessControlType.Allow);
			Assert.AreEqual (rule.IdentityReference, id);
			Assert.AreEqual (rule.Rights, WillWorkRights.Value);
		}

		[Test, ExpectedException (typeof (InvalidCastException))]
		public void WillFailFailsUsingFactoryOnGetter()
		{
			WillFailSecurity security = new WillFailSecurity ();
			SecurityIdentifier id = new SecurityIdentifier (WellKnownSidType.WorldSid, null);
			AccessRule<WillFailRights> rule = (AccessRule<WillFailRights>)
				security.AccessRuleFactory (id, 1, false,
					InheritanceFlags.None, PropagationFlags.None,
					AccessControlType.Allow);
			WillFailRights rights = rule.Rights;
		}

		[Test, ExpectedException (typeof (InvalidCastException))]
		public void WillFailFailsUsingConstructor()
		{
			SecurityIdentifier id = new SecurityIdentifier (WellKnownSidType.WorldSid, null);
			AccessRule<WillFailRights> rule = new AccessRule<WillFailRights> (id, new WillFailRights(),
											  AccessControlType.Allow);
		}
	}
}

#endif

