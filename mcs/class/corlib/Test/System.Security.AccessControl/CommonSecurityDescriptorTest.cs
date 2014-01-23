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

		[Test]
		public void GetBinaryForm ()
		{
			CommonSecurityDescriptor csd = new CommonSecurityDescriptor
				(false, false, ControlFlags.None, null, null, null, null);

			Assert.AreEqual (20, csd.BinaryLength);
			byte[] binaryForm = new byte[csd.BinaryLength];
			csd.GetBinaryForm (binaryForm, 0);

			Assert.AreEqual (ControlFlags.DiscretionaryAclPresent | ControlFlags.SelfRelative,
			                 csd.ControlFlags);

			// The default 'Allow Everyone Full Access' serializes as NOT having a
			// DiscretionaryAcl, as the above demonstrates (byte 3 is 0 not 4).
			Assert.AreEqual (new byte[20] {
				1, 0, 0, 128, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
			}, binaryForm);

			// Changing SystemAcl protection does nothing special.
			csd.SetSystemAclProtection (true, true);
			Assert.AreEqual (20, csd.BinaryLength);

			// Modifying the DiscretionaryAcl (even effective no-ops like this) causes serialization.
			csd.SetDiscretionaryAclProtection (false, true);
			Assert.AreEqual (48, csd.BinaryLength);
		}

		[Test, ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void GetBinaryFormOffset ()
		{
			CommonSecurityDescriptor csd = new CommonSecurityDescriptor
				(false, false, ControlFlags.None, null, null, null, null);
			csd.GetBinaryForm (new byte[csd.BinaryLength], 1);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void GetBinaryFormNull ()
		{
			CommonSecurityDescriptor csd = new CommonSecurityDescriptor
				(false, false, ControlFlags.None, null, null, null, null);
			csd.GetBinaryForm (null, 0);
		}

		[Test]
		public void AefaModifiedFlagIsStoredOnDiscretionaryAcl ()
		{
			CommonSecurityDescriptor csd1, csd2;

			// Incidentally this shows the DiscretionaryAcl is NOT cloned.
			csd1 = new CommonSecurityDescriptor (false, false, ControlFlags.None, null, null, null, null);
			csd2 = new CommonSecurityDescriptor (false, false, ControlFlags.None, null, null, null, csd1.DiscretionaryAcl);
			Assert.AreSame (csd1.DiscretionaryAcl, csd2.DiscretionaryAcl);

			Assert.AreEqual ("", csd1.GetSddlForm (AccessControlSections.Access));
			csd2.SetDiscretionaryAclProtection (false, true);
			Assert.AreEqual ("D:(A;;0xffffffff;;;WD)", csd1.GetSddlForm (AccessControlSections.Access));
			Assert.AreEqual ("D:(A;;0xffffffff;;;WD)", csd2.GetSddlForm (AccessControlSections.Access));
		}

		[Test]
		public void AefaRoundtrip ()
		{
			CommonSecurityDescriptor csd;

			csd = new CommonSecurityDescriptor (false, false, ControlFlags.None, null, null, null, null);
			Assert.AreEqual (20, csd.BinaryLength);

			byte[] binaryForm1 = new byte[csd.BinaryLength];
			csd.GetBinaryForm (binaryForm1, 0);

			csd = new CommonSecurityDescriptor (false, false, new RawSecurityDescriptor (binaryForm1, 0));

			byte[] binaryForm2 = new byte[csd.BinaryLength];
			csd.GetBinaryForm (binaryForm2, 0);

			Assert.AreEqual (binaryForm1, binaryForm2);
		}

		[Test]
		public void GetSddlFormAefaRemovesDacl ()
		{
			CommonSecurityDescriptor csd = new CommonSecurityDescriptor
				(false, false, ControlFlags.None, null, null, null, null);

			Assert.AreEqual (1, csd.DiscretionaryAcl.Count);
			Assert.AreEqual ("", csd.GetSddlForm (AccessControlSections.Access));
			Assert.AreEqual (ControlFlags.DiscretionaryAclPresent
			                 | ControlFlags.SelfRelative,
			                 csd.ControlFlags);

			Assert.AreSame (csd.DiscretionaryAcl, csd.DiscretionaryAcl);
			Assert.AreNotSame (csd.DiscretionaryAcl[0], csd.DiscretionaryAcl[0]);
			Assert.AreEqual ("", csd.GetSddlForm (AccessControlSections.Access));

			csd.SetDiscretionaryAclProtection (false, true);
			Assert.AreEqual ("D:(A;;0xffffffff;;;WD)", csd.GetSddlForm (AccessControlSections.Access));
			Assert.AreSame (csd.DiscretionaryAcl, csd.DiscretionaryAcl);
			Assert.AreNotSame (csd.DiscretionaryAcl[0], csd.DiscretionaryAcl[0]);
			Assert.AreEqual (ControlFlags.DiscretionaryAclPresent
			                 | ControlFlags.SelfRelative,
			                 csd.ControlFlags);

			csd.SetDiscretionaryAclProtection (true, true);
			Assert.AreEqual (1, csd.DiscretionaryAcl.Count);
			Assert.AreEqual ("D:P(A;;0xffffffff;;;WD)", csd.GetSddlForm (AccessControlSections.Access));
			Assert.AreEqual (ControlFlags.DiscretionaryAclPresent
			                 | ControlFlags.DiscretionaryAclProtected
			                 | ControlFlags.SelfRelative,
			                 csd.ControlFlags);

			csd.SetDiscretionaryAclProtection (false, false);
			Assert.AreEqual (1, csd.DiscretionaryAcl.Count);
			Assert.AreEqual ("D:(A;;0xffffffff;;;WD)", csd.GetSddlForm (AccessControlSections.Access));
			Assert.AreEqual (ControlFlags.DiscretionaryAclPresent
			                 | ControlFlags.SelfRelative,
			                 csd.ControlFlags);
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
			CommonSecurityDescriptor csd; DiscretionaryAcl dacl; CommonAce ace;

			csd = new CommonSecurityDescriptor (false, false, ControlFlags.None, userSid, groupSid, null, null);
			dacl = csd.DiscretionaryAcl;
			Assert.AreEqual (1, dacl.Count);

			ace = (CommonAce)dacl [0];
			Assert.AreEqual (-1, ace.AccessMask);
			Assert.AreEqual (AceFlags.None, ace.AceFlags);
			Assert.AreEqual (AceType.AccessAllowed, ace.AceType);
			Assert.AreEqual (20, ace.BinaryLength);
			Assert.IsFalse (ace.IsCallback);
			Assert.IsFalse (ace.IsInherited);
			Assert.AreEqual (0, ace.OpaqueLength);
			Assert.AreEqual (ace.SecurityIdentifier, everyoneSid);

			csd = new CommonSecurityDescriptor (true, false, ControlFlags.None, userSid, groupSid, null, null);
			dacl = csd.DiscretionaryAcl;
			Assert.AreEqual (1, dacl.Count);

			ace = (CommonAce)dacl [0];
			Assert.AreEqual (-1, ace.AccessMask);
			Assert.AreEqual (AceFlags.ObjectInherit | AceFlags.ContainerInherit, ace.AceFlags);
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
		public void ProtectionPreserveInheritanceIgnoredUnlessProtectedTrue ()
		{
			CommonSecurityDescriptor descriptor;

			descriptor = ProtectionPreserveInheritanceIgnoredUnlessProtectedTrueDescriptor();
			Assert.AreEqual (2, descriptor.DiscretionaryAcl.Count);

			descriptor = ProtectionPreserveInheritanceIgnoredUnlessProtectedTrueDescriptor();
			descriptor.SetDiscretionaryAclProtection (true, false);
			Assert.AreEqual (1, descriptor.DiscretionaryAcl.Count);

			descriptor = ProtectionPreserveInheritanceIgnoredUnlessProtectedTrueDescriptor();
			descriptor.SetDiscretionaryAclProtection (false, false);
			Assert.AreEqual (2, descriptor.DiscretionaryAcl.Count);

			descriptor = ProtectionPreserveInheritanceIgnoredUnlessProtectedTrueDescriptor();
			descriptor.SetDiscretionaryAclProtection (true, true);
			Assert.AreEqual (2, descriptor.DiscretionaryAcl.Count);
			descriptor.SetDiscretionaryAclProtection (false, false);
			Assert.AreEqual (2, descriptor.DiscretionaryAcl.Count);
			descriptor.SetDiscretionaryAclProtection (false, true);
			Assert.AreEqual (2, descriptor.DiscretionaryAcl.Count);
			descriptor.SetDiscretionaryAclProtection (true, false);
			Assert.AreEqual (1, descriptor.DiscretionaryAcl.Count);
		}

		static CommonSecurityDescriptor ProtectionPreserveInheritanceIgnoredUnlessProtectedTrueDescriptor()
		{
			SecurityIdentifier sid = new SecurityIdentifier ("WD");

			RawAcl acl = new RawAcl (GenericAcl.AclRevision, 1);
			acl.InsertAce (0, new CommonAce (AceFlags.None, AceQualifier.AccessDenied, 1, sid, false, null));
			               acl.InsertAce (1, new CommonAce (AceFlags.Inherited, AceQualifier.AccessAllowed, 1, sid, false, null));

			CommonSecurityDescriptor descriptor = new CommonSecurityDescriptor
				(false, false, ControlFlags.None, null, null, null, null);
			descriptor.DiscretionaryAcl = new DiscretionaryAcl (false, false, acl);
			return descriptor;
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

