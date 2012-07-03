// CommonSecurityDescriptorTest.cs - NUnit Test Cases for CommonSecurityDescriptor
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
	public class CommonSecurityDescriptorTest
	{
		[Test]
		public void DefaultOwnerAndGroup ()
		{
			CommonSecurityDescriptor csd = new CommonSecurityDescriptor
				(false, false, ControlFlags.None, null, null, null, null);
			Assert.IsNull (csd.Owner);
			Assert.IsNull (csd.Group);
			Assert.AreEqual (ControlFlags.DiscretionaryAclPresent
			                 | ControlFlags.SelfRelative, csd.ControlFlags);
		}

		[Test, ExpectedException (typeof (ArgumentException))]
		public void ContainerAndDSConsistencyEnforcedA ()
		{
			SecurityIdentifier userSid = new SecurityIdentifier (WellKnownSidType.LocalSystemSid, null);
			SecurityIdentifier groupSid = new SecurityIdentifier (WellKnownSidType.BuiltinAdministratorsSid, null);

			DiscretionaryAcl dacl = new DiscretionaryAcl (true, true, 0);
			new CommonSecurityDescriptor (true, false, ControlFlags.None, userSid, groupSid, null, dacl);
		}

		[Test, ExpectedException (typeof (ArgumentException))]
		public void ContainerAndDSConsistencyEnforcedB ()
		{
			SecurityIdentifier userSid = new SecurityIdentifier (WellKnownSidType.LocalSystemSid, null);
			SecurityIdentifier groupSid = new SecurityIdentifier (WellKnownSidType.BuiltinAdministratorsSid, null);

			SystemAcl sacl = new SystemAcl (false, false, 0);
			new CommonSecurityDescriptor (true, false, ControlFlags.None, userSid, groupSid, sacl, null);
		}

		[Test, ExpectedException (typeof (ArgumentException))]
		public void ContainerAndDSConsistencyEnforcedInSetter ()
		{
			SecurityIdentifier userSid = new SecurityIdentifier (WellKnownSidType.LocalSystemSid, null);
			SecurityIdentifier groupSid = new SecurityIdentifier (WellKnownSidType.BuiltinAdministratorsSid, null);

			CommonSecurityDescriptor csd = new CommonSecurityDescriptor
				(true, false, ControlFlags.None, userSid, groupSid, null, null);
			csd.DiscretionaryAcl = new DiscretionaryAcl (true, true, 0);
		}

		[Test]
		public void DefaultDaclIsAllowEveryoneFullAccess ()
		{
			SecurityIdentifier userSid = new SecurityIdentifier ("SY");
			SecurityIdentifier groupSid = new SecurityIdentifier ("BA");
			SecurityIdentifier everyoneSid = new SecurityIdentifier ("WD");

			CommonSecurityDescriptor csd = new CommonSecurityDescriptor
				(false, false, ControlFlags.None, userSid, groupSid, null, null);

			DiscretionaryAcl dacl = csd.DiscretionaryAcl;
			Assert.AreEqual (1, dacl.Count);

			CommonAce ace = (CommonAce)dacl [0];
			Assert.AreEqual (-1, ace.AccessMask);
			Assert.AreEqual (AceFlags.None, ace.AceFlags);
			Assert.AreEqual (AceType.AccessAllowed, ace.AceType);
			Assert.AreEqual (20, ace.BinaryLength);
			Assert.IsFalse (ace.IsCallback);
			Assert.IsFalse (ace.IsInherited);
			Assert.AreEqual (0, ace.OpaqueLength);
			Assert.AreEqual (ace.SecurityIdentifier, everyoneSid);
		}

		[Test]
		public void PurgeDefaultDacl ()
		{
			SecurityIdentifier userSid = new SecurityIdentifier ("SY");
			SecurityIdentifier groupSid = new SecurityIdentifier ("BA");
			SecurityIdentifier everyoneSid = new SecurityIdentifier ("WD");

			CommonSecurityDescriptor csd = new CommonSecurityDescriptor
				(false, false, ControlFlags.None, userSid, groupSid, null, null);

			DiscretionaryAcl dacl = csd.DiscretionaryAcl;
			Assert.AreEqual (1, dacl.Count);

			csd.PurgeAccessControl (userSid);
			Assert.AreEqual (1, dacl.Count);

			csd.PurgeAccessControl (everyoneSid);
			Assert.AreEqual (0, dacl.Count);
		}

		[Test]
		public void PurgeNullSaclWithoutError ()
		{
			SecurityIdentifier userSid = new SecurityIdentifier ("SY");
			SecurityIdentifier groupSid = new SecurityIdentifier ("BA");

			CommonSecurityDescriptor csd = new CommonSecurityDescriptor
				(false, false, ControlFlags.None, userSid, groupSid, null, null);
			csd.PurgeAudit (userSid);
			Assert.IsNull (csd.SystemAcl);
		}

		[Test]
		public void OwnerAndGroupAreReferences ()
		{
			SecurityIdentifier userSid = new SecurityIdentifier (WellKnownSidType.LocalSystemSid, null);
			SecurityIdentifier groupSid = new SecurityIdentifier (WellKnownSidType.BuiltinAdministratorsSid, null);
			CommonSecurityDescriptor csd;

			csd = new CommonSecurityDescriptor
				(false, false, ControlFlags.None, userSid, groupSid, null, null);
			Assert.AreSame (groupSid, csd.Group);
			Assert.AreSame (userSid, csd.Owner);
		}

		[Test]
		public void ProtectionChangesFlags ()
		{
			SecurityIdentifier userSid = new SecurityIdentifier (WellKnownSidType.LocalSystemSid, null);
			SecurityIdentifier groupSid = new SecurityIdentifier (WellKnownSidType.BuiltinAdministratorsSid, null);
			CommonSecurityDescriptor csd;

			csd = new CommonSecurityDescriptor
				(false, false, ControlFlags.None, userSid, groupSid, null, null);
			Assert.AreEqual (ControlFlags.DiscretionaryAclPresent
			                 | ControlFlags.SelfRelative, csd.ControlFlags);

			csd.SetDiscretionaryAclProtection (true, false);
			Assert.AreEqual (ControlFlags.DiscretionaryAclPresent
			                 | ControlFlags.DiscretionaryAclProtected
			                 | ControlFlags.SelfRelative, csd.ControlFlags);

			csd.SetSystemAclProtection (true, false); // despite not being *present*
			Assert.AreEqual (ControlFlags.DiscretionaryAclPresent
			                 | ControlFlags.DiscretionaryAclProtected
			                 | ControlFlags.SystemAclProtected
			                 | ControlFlags.SelfRelative, csd.ControlFlags);
		}

		[Test]
		public void DaclPresent ()
		{
			SecurityIdentifier userSid = new SecurityIdentifier (WellKnownSidType.LocalSystemSid, null);
			SecurityIdentifier groupSid = new SecurityIdentifier (WellKnownSidType.BuiltinAdministratorsSid, null);
			CommonSecurityDescriptor csd;

			csd = new CommonSecurityDescriptor
				(false, false, ControlFlags.None, userSid, groupSid, null, null);
			Assert.IsNotNull (csd.DiscretionaryAcl);
			Assert.IsTrue (csd.IsDiscretionaryAclCanonical);
			Assert.AreEqual (ControlFlags.DiscretionaryAclPresent | ControlFlags.SelfRelative, csd.ControlFlags);
			Assert.AreEqual (1, csd.DiscretionaryAcl.Count);

			csd = new CommonSecurityDescriptor
				(false, false, ControlFlags.DiscretionaryAclPresent, userSid, groupSid, null, null);

			Assert.IsNotNull (csd.DiscretionaryAcl);
			Assert.IsTrue (csd.IsDiscretionaryAclCanonical);
			Assert.AreEqual (ControlFlags.DiscretionaryAclPresent | ControlFlags.SelfRelative, csd.ControlFlags);

			DiscretionaryAcl dacl = new DiscretionaryAcl (false, false, 0);

			csd = new CommonSecurityDescriptor
				(false, false, ControlFlags.None, userSid, groupSid, null, dacl);
			Assert.AreSame (dacl, csd.DiscretionaryAcl);
			Assert.AreEqual (ControlFlags.DiscretionaryAclPresent | ControlFlags.SelfRelative, csd.ControlFlags);

			csd = new CommonSecurityDescriptor
				(false, false, ControlFlags.DiscretionaryAclPresent, userSid, groupSid, null, dacl);
			Assert.AreSame (dacl, csd.DiscretionaryAcl);
			Assert.AreEqual (ControlFlags.DiscretionaryAclPresent | ControlFlags.SelfRelative, csd.ControlFlags);
		}

		[Test]
		public void SaclPresent ()
		{
			SecurityIdentifier userSid = new SecurityIdentifier (WellKnownSidType.LocalSystemSid, null);
			SecurityIdentifier groupSid = new SecurityIdentifier (WellKnownSidType.BuiltinAdministratorsSid, null);
			SystemAcl sacl = new SystemAcl (false, false, 0);
			CommonSecurityDescriptor csd;

			csd = new CommonSecurityDescriptor
				(false, false, ControlFlags.None, userSid, groupSid, null, null);
			Assert.IsNull (csd.SystemAcl);
			Assert.IsTrue (csd.IsSystemAclCanonical);

			csd = new CommonSecurityDescriptor
				(false, false, ControlFlags.SystemAclPresent, userSid, groupSid, null, null);
			Assert.IsNull (csd.SystemAcl);
			Assert.IsTrue (csd.IsSystemAclCanonical);
			Assert.AreEqual (ControlFlags.DiscretionaryAclPresent | ControlFlags.SelfRelative, csd.ControlFlags);

			csd = new CommonSecurityDescriptor
				(false, false, ControlFlags.None, userSid, groupSid, sacl, null);
			Assert.AreSame (sacl, csd.SystemAcl);
			Assert.IsTrue (csd.IsSystemAclCanonical);
			Assert.AreEqual (0, csd.SystemAcl.Count);
			Assert.AreEqual (ControlFlags.DiscretionaryAclPresent
			                 | ControlFlags.SystemAclPresent
			                 | ControlFlags.SelfRelative, csd.ControlFlags);

			csd.SystemAcl = null;
			Assert.AreEqual (ControlFlags.DiscretionaryAclPresent
			                 | ControlFlags.SelfRelative, csd.ControlFlags);
		}
	}
}

