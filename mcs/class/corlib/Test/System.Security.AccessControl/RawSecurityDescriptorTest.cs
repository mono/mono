//
// RawSecurityDescriptorTest.cs - NUnit Test Cases for RawSecurityDescriptor
//
// Author:
//	Kenneth Bell
//

using System;
using System.Security.AccessControl;
using System.Security.Principal;
using NUnit.Framework;

namespace MonoTests.System.Security.AccessControl {

	[TestFixture]
	public class RawSecurityDescriptorTest {
		private void CheckSddlConstructor (string sddl, byte[] expectedBinary)
		{
			RawSecurityDescriptor sd = new RawSecurityDescriptor (sddl);
			
			Assert.That (sd.BinaryLength, Is.GreaterThanOrEqualTo (0));
			byte[] buffer = new byte[sd.BinaryLength];
			
			sd.GetBinaryForm (buffer, 0);
			Assert.AreEqual (expectedBinary, buffer);
		}

		private void CheckBinaryConstructor (string expectedSddl, byte[] binary)
		{
			RawSecurityDescriptor sd = new RawSecurityDescriptor (binary, 0);
			
			Assert.AreEqual (sd.BinaryLength, binary.Length);
			Assert.AreEqual (expectedSddl, sd.GetSddlForm (AccessControlSections.All));
		}

		private void CheckRoundTrip (string sddl)
		{
			RawSecurityDescriptor sd = new RawSecurityDescriptor (sddl);
			
			byte[] buffer = new byte[sd.BinaryLength];
			sd.GetBinaryForm (buffer, 0);
			
			sd = new RawSecurityDescriptor (buffer, 0);
			Assert.AreEqual (sddl, sd.GetSddlForm (AccessControlSections.All));
		}

		[Test]
		public void ConstructorEmptyString ()
		{
			byte[] sdBinary = new byte[] {
				0x01, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
			CheckSddlConstructor ("", sdBinary);
		}

		[Test]
		public void ConstructorString ()
		{
			byte[] sdBinary = new byte[] {
				0x01, 0x00, 0x04, 0x80, 0x14, 0x00, 0x00, 0x00, 0x24, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x34, 0x00, 0x00, 0x00,
				0x01, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x05, 0x20, 0x00,
				0x00, 0x00, 0x21, 0x02, 0x00, 0x00, 0x01, 0x02, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x05, 0x20, 0x00, 0x00, 0x00, 0x20, 0x02,
				0x00, 0x00, 0x02, 0x00, 0x1C, 0x00, 0x01, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x14, 0x00, 0x3F, 0x00, 0x0E, 0x10, 0x01, 0x01,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
			CheckSddlConstructor ("O:BUG:BAD:(A;;RPWPCCDCLCSWRCWDWOGA;;;S-1-0-0)", sdBinary);
			CheckSddlConstructor ("G:BAO:BUD:(A;;RPWPCCDCLCSWRCWDWOGA;;;S-1-0-0)", sdBinary);
			CheckSddlConstructor ("G:BAD:(A; ;RPWPCCDCLCSWRCWDWOGA;;;S-1-0-0)O:BU", sdBinary);
			CheckSddlConstructor ("O:buG:baD:(a;;rpwpccdclcswrcwdwoga;;;s-1-0-0)", sdBinary);
			
			sdBinary = new byte[] {
				0x01, 0x00, 0x00, 0x80, 0x14, 0x00, 0x00, 0x00, 0x24, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
				0x01, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x05, 0x20, 0x00,
				0x00, 0x00, 0x21, 0x02, 0x00, 0x00, 0x01, 0x02, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x05, 0x20, 0x00, 0x00, 0x00, 0x20, 0x02,
				0x00, 0x00 };
			CheckSddlConstructor ("O:BUG:BA", sdBinary);
			
			sdBinary = new byte[] {
				0x01, 0x00, 0x04, 0x80, 0x14, 0x00, 0x00, 0x00, 0x24, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x34, 0x00, 0x00, 0x00,
				0x01, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x05, 0x20, 0x00,
				0x00, 0x00, 0x21, 0x02, 0x00, 0x00, 0x01, 0x02, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x05, 0x20, 0x00, 0x00, 0x00, 0x20, 0x02,
				0x00, 0x00, 0x04, 0x00, 0x40, 0x00, 0x01, 0x00, 0x00, 0x00,
				0x05, 0x00, 0x38, 0x00, 0x3F, 0x00, 0x0E, 0x10, 0x03, 0x00,
				0x00, 0x00, 0x53, 0x1A, 0x72, 0xAB, 0x2F, 0x1E, 0xD0, 0x11,
				0x98, 0x19, 0x00, 0xAA, 0x00, 0x40, 0x52, 0x9B, 0x53, 0x1A,
				0x72, 0xAB, 0x2F, 0x1E, 0xD0, 0x11, 0x98, 0x19, 0x00, 0xAA,
				0x00, 0x40, 0x52, 0x9B, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
			CheckSddlConstructor ("O:BUG:BAD:(OA;;RPWPCCDCLCSWRCWDWOGA;ab721a53-1e2f-11d0-9819-00aa0040529b;ab721a53-1e2f-11d0-9819-00aa0040529b;S-1-0-0)", sdBinary);
		}

		[Test]
		public void ConstructorBinary ()
		{
			byte[] sdBinary = new byte[] {
				0x01, 0x00, 0x04, 0x80, 0x14, 0x00, 0x00, 0x00, 0x24, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x34, 0x00, 0x00, 0x00,
				0x01, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x05, 0x20, 0x00,
				0x00, 0x00, 0x21, 0x02, 0x00, 0x00, 0x01, 0x02, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x05, 0x20, 0x00, 0x00, 0x00, 0x20, 0x02,
				0x00, 0x00, 0x02, 0x00, 0x1C, 0x00, 0x01, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x14, 0x00, 0x3F, 0x00, 0x0E, 0x10, 0x01, 0x01,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
			CheckBinaryConstructor ("O:BUG:BAD:(A;;CCDCLCSWRPWPRCWDWOGA;;;S-1-0-0)", sdBinary);
			sdBinary = new byte[] {
				0x01, 0x00, 0x00, 0x80, 0x14, 0x00, 0x00, 0x00, 0x24, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
				0x01, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x05, 0x20, 0x00,
				0x00, 0x00, 0x21, 0x02, 0x00, 0x00, 0x01, 0x02, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x05, 0x20, 0x00, 0x00, 0x00, 0x20, 0x02,
				0x00, 0x00 };
			CheckBinaryConstructor ("O:BUG:BA", sdBinary);
			
			sdBinary = new byte[] {
				0x01, 0x00, 0x04, 0x80, 0x14, 0x00, 0x00, 0x00, 0x24, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x34, 0x00, 0x00, 0x00,
				0x01, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x05, 0x20, 0x00,
				0x00, 0x00, 0x21, 0x02, 0x00, 0x00, 0x01, 0x02, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x05, 0x20, 0x00, 0x00, 0x00, 0x20, 0x02,
				0x00, 0x00, 0x04, 0x00, 0x40, 0x00, 0x01, 0x00, 0x00, 0x00,
				0x05, 0x00, 0x38, 0x00, 0x3F, 0x00, 0x0E, 0x10, 0x03, 0x00,
				0x00, 0x00, 0x53, 0x1A, 0x72, 0xAB, 0x2F, 0x1E, 0xD0, 0x11,
				0x98, 0x19, 0x00, 0xAA, 0x00, 0x40, 0x52, 0x9B, 0x53, 0x1A,
				0x72, 0xAB, 0x2F, 0x1E, 0xD0, 0x11, 0x98, 0x19, 0x00, 0xAA,
				0x00, 0x40, 0x52, 0x9B, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
			CheckBinaryConstructor ("O:BUG:BAD:(OA;;CCDCLCSWRPWPRCWDWOGA;ab721a53-1e2f-11d0-9819-00aa0040529b;ab721a53-1e2f-11d0-9819-00aa0040529b;S-1-0-0)", sdBinary);
		}

		[Test]
		public void FlagMismatch ()
		{
			// Check setting DACL-present flag on empty SD
			RawSecurityDescriptor sd = new RawSecurityDescriptor ("");
			Assert.AreEqual (20, sd.BinaryLength);
			sd.SetFlags (ControlFlags.DiscretionaryAclPresent);
			Assert.AreEqual (20, sd.BinaryLength);
			byte[] buffer = new byte[sd.BinaryLength];
			sd.GetBinaryForm (buffer, 0);
			byte[] sdBinary = new byte[] {
				0x01, 0x00, 0x04, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
			Assert.AreEqual (sdBinary, buffer);
			
			// Check unsetting DACL-present flag on SD with DACL
			sd = new RawSecurityDescriptor ("O:BUG:BAD:(A;;RPWPCCDCLCSWRCWDWOGA;;;S-1-0-0)");
			Assert.AreEqual (80, sd.BinaryLength);
			sd.SetFlags (sd.ControlFlags & ~ControlFlags.DiscretionaryAclPresent);
			Assert.AreEqual (ControlFlags.SelfRelative, sd.ControlFlags);
			Assert.AreEqual (52, sd.BinaryLength);
			buffer = new byte[sd.BinaryLength];
			sd.GetBinaryForm (buffer, 0);
			sdBinary = new byte[] {
				0x01, 0x00, 0x00, 0x80, 0x14, 0x00, 0x00, 0x00, 0x24, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
				0x01, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x05, 0x20, 0x00,
				0x00, 0x00, 0x21, 0x02, 0x00, 0x00, 0x01, 0x02, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x05, 0x20, 0x00, 0x00, 0x00, 0x20, 0x02,
				0x00, 0x00 };
			Assert.AreEqual (sdBinary, buffer);
		}

		[Test]
		public void GetBinaryForm ()
		{
			RawSecurityDescriptor sd = new RawSecurityDescriptor ("");
			sd.Owner = new SecurityIdentifier (WellKnownSidType.BuiltinUsersSid, null);
			sd.Group = new SecurityIdentifier (WellKnownSidType.BuiltinAdministratorsSid, null);
			sd.DiscretionaryAcl = new RawAcl (1, 0);
			sd.SystemAcl = new RawAcl (1, 0);
			sd.SetFlags (sd.ControlFlags | ControlFlags.DiscretionaryAclPresent | ControlFlags.SystemAclPresent);
			
			// Empty ACL form
			byte[] buffer = new byte[sd.BinaryLength];
			sd.GetBinaryForm (buffer, 0);
			byte[] sdBinary = new byte[] {
				0x01, 0x00, 0x14, 0x80, 0x14, 0x00, 0x00, 0x00, 0x24, 0x00,
				0x00, 0x00, 0x34, 0x00, 0x00, 0x00, 0x3C, 0x00, 0x00, 0x00,
				0x01, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x05, 0x20, 0x00,
				0x00, 0x00, 0x21, 0x02, 0x00, 0x00, 0x01, 0x02, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x05, 0x20, 0x00, 0x00, 0x00, 0x20, 0x02,
				0x00, 0x00, 0x01, 0x00, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00,
				0x01, 0x00, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00 };
			Assert.AreEqual (sdBinary, buffer);
			
			// Add an ACE to the DACL
			SecurityIdentifier builtInAdmins = new SecurityIdentifier (WellKnownSidType.BuiltinAdministratorsSid, null);
			CommonAce ace = new CommonAce (AceFlags.None, AceQualifier.AccessAllowed, 0x7FFFFFFF, builtInAdmins, false, null);
			sd.DiscretionaryAcl.InsertAce (0, ace);
			buffer = new byte[sd.BinaryLength];
			sd.GetBinaryForm (buffer, 0);
			sdBinary = new byte[] {
				0x01, 0x00, 0x14, 0x80, 0x14, 0x00, 0x00, 0x00, 0x24, 0x00,
				0x00, 0x00, 0x34, 0x00, 0x00, 0x00, 0x3C, 0x00, 0x00, 0x00,
				0x01, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x05, 0x20, 0x00,
				0x00, 0x00, 0x21, 0x02, 0x00, 0x00, 0x01, 0x02, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x05, 0x20, 0x00, 0x00, 0x00, 0x20, 0x02,
				0x00, 0x00, 0x01, 0x00, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00,
				0x01, 0x00, 0x20, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00,
				0x18, 0x00, 0xFF, 0xFF, 0xFF, 0x7F, 0x01, 0x02, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x05, 0x20, 0x00, 0x00, 0x00, 0x20, 0x02,
				0x00, 0x00 };
			Assert.AreEqual (sdBinary, buffer);
			
			// This time with an Object ACE
			ObjectAce objectAce = new ObjectAce (AceFlags.Inherited, AceQualifier.AccessAllowed, 0x12345678, builtInAdmins, ObjectAceFlags.ObjectAceTypePresent | ObjectAceFlags.InheritedObjectAceTypePresent, new Guid ("189c0dc7-b849-4dea-93a5-6d4cb8857a5c"), new Guid ("53b4a3d4-fe39-468b-bc60-b4fcba772fa5"), false, null);
			sd.DiscretionaryAcl = new RawAcl (2, 0);
			sd.DiscretionaryAcl.InsertAce (0, objectAce);
			buffer = new byte[sd.BinaryLength];
			sd.GetBinaryForm (buffer, 0);
			sdBinary = new byte[] {
				0x01, 0x00, 0x14, 0x80, 0x14, 0x00, 0x00, 0x00, 0x24, 0x00,
				0x00, 0x00, 0x34, 0x00, 0x00, 0x00, 0x3C, 0x00, 0x00, 0x00,
				0x01, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x05, 0x20, 0x00,
				0x00, 0x00, 0x21, 0x02, 0x00, 0x00, 0x01, 0x02, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x05, 0x20, 0x00, 0x00, 0x00, 0x20, 0x02,
				0x00, 0x00, 0x01, 0x00, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00,
				0x02, 0x00, 0x44, 0x00, 0x01, 0x00, 0x00, 0x00, 0x05, 0x10,
				0x3C, 0x00, 0x78, 0x56, 0x34, 0x12, 0x03, 0x00, 0x00, 0x00,
				0xC7, 0x0D, 0x9C, 0x18, 0x49, 0xB8, 0xEA, 0x4D, 0x93, 0xA5,
				0x6D, 0x4C, 0xB8, 0x85, 0x7A, 0x5C, 0xD4, 0xA3, 0xB4, 0x53,
				0x39, 0xFE, 0x8B, 0x46, 0xBC, 0x60, 0xB4, 0xFC, 0xBA, 0x77,
				0x2F, 0xA5, 0x01, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x05,
				0x20, 0x00, 0x00, 0x00, 0x20, 0x02, 0x00, 0x00 };
			Assert.AreEqual (sdBinary, buffer);
		}

		[Test]
		public void GetSddlForm ()
		{
			RawSecurityDescriptor sd = new RawSecurityDescriptor ("");
			Assert.AreEqual ("", sd.GetSddlForm (AccessControlSections.All));
			
			// Ask for part of SD that isn't represented
			sd.Owner = new SecurityIdentifier (WellKnownSidType.BuiltinUsersSid, null);
			sd.Group = new SecurityIdentifier (WellKnownSidType.BuiltinAdministratorsSid, null);
			Assert.AreEqual ("", sd.GetSddlForm (AccessControlSections.Access));
			
			// Empty ACL form
			sd.DiscretionaryAcl = new RawAcl (2, 0);
			sd.SystemAcl = new RawAcl (1, 0);
			sd.SetFlags (sd.ControlFlags | ControlFlags.DiscretionaryAclPresent | ControlFlags.SystemAclPresent);
			Assert.AreEqual ("O:BUG:BAD:S:", sd.GetSddlForm (AccessControlSections.All));
			
			// Add an ACE to the DACL
			SecurityIdentifier builtInAdmins = new SecurityIdentifier (WellKnownSidType.BuiltinAdministratorsSid, null);
			CommonAce ace = new CommonAce (AceFlags.None, AceQualifier.AccessAllowed, 0x7FFFFFFF, builtInAdmins, false, null);
			sd.DiscretionaryAcl.InsertAce (0, ace);
			Assert.AreEqual ("O:BUG:BAD:(A;;0x7fffffff;;;BA)S:", sd.GetSddlForm (AccessControlSections.All));
			
			// Add second ACE to the DACL
			SecurityIdentifier randomUser = new SecurityIdentifier ("S-1-5-21-324-23423-234-334");
			ace = new CommonAce (AceFlags.Inherited | AceFlags.ContainerInherit, AceQualifier.AccessDenied, 0x12345678, randomUser, true, null);
			sd.DiscretionaryAcl.InsertAce (0, ace);
			Assert.AreEqual ("O:BUD:(XD;CIID;0x12345678;;;S-1-5-21-324-23423-234-334)(A;;0x7fffffff;;;BA)", sd.GetSddlForm (AccessControlSections.Owner | AccessControlSections.Access));
			
			// DACL & SACL flags
			sd.SetFlags (sd.ControlFlags | ControlFlags.DiscretionaryAclProtected | ControlFlags.DiscretionaryAclAutoInherited | ControlFlags.DiscretionaryAclAutoInheritRequired | ControlFlags.SystemAclAutoInherited);
			sd.DiscretionaryAcl = new RawAcl (1, 0);
			ace = new CommonAce (AceFlags.None, AceQualifier.AccessAllowed, 0x7FFFFFFF, builtInAdmins, false, null);
			sd.DiscretionaryAcl.InsertAce (0, ace);
			Assert.AreEqual ("O:BUG:BAD:PARAI(A;;0x7fffffff;;;BA)S:AI", sd.GetSddlForm (AccessControlSections.All));
			
			sd.SetFlags (sd.ControlFlags | ControlFlags.ServerSecurity | ControlFlags.DiscretionaryAclDefaulted);
			Assert.AreEqual ("O:BUG:BAD:PARAI(A;;0x7fffffff;;;BA)S:AI", sd.GetSddlForm (AccessControlSections.All));
		}

		[Test]
		public void RoundTrip ()
		{
			CheckRoundTrip ("O:BUG:BAD:(A;;CCDCLCSWRPWPRCWDWOGA;;;S-1-0-0)");
			CheckRoundTrip ("O:BUG:BAD:(A;;KR;;;S-1-0-0)");
			CheckRoundTrip ("O:BUG:BAD:(OA;;CCDCLCSWRPWPRCWDWOGA;ab721a53-1e2f-11d0-9819-00aa0040529b;ab721a53-1e2f-11d0-9819-00aa0040529b;S-1-0-0)");
			CheckRoundTrip ("O:BUG:BAD:(A;;CCDCLCSWRPRC;;;S-1-0-0)");
			CheckRoundTrip ("O:SYG:BAD:(A;;0x12019f;;;SY)(A;;0x12019f;;;BA)");
			CheckRoundTrip ("O:SYG:BAD:(A;OICINPIOID;0x12019f;;;SY)");
			CheckRoundTrip ("O:SYG:BAS:(AU;SAFA;0x12019f;;;SY)");
		}
	}
}
