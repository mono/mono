//
// RawAclTest.cs - NUnit Test Cases for RawAclTest
//
// Author:
//	Kenneth Bell
//

using System.Security.AccessControl;
using System.Security.Principal;
using NUnit.Framework;

namespace MonoTests.System.Security.AccessControl {

	[TestFixture]
	public class AclTest {

		[Test]
		public void GetBinaryForm ()
		{
			RawAcl acl = new RawAcl (1, 0);
			
			byte[] buffer = new byte[acl.BinaryLength];
			acl.GetBinaryForm (buffer, 0);
			byte[] sdBinary = new byte[] { 0x01, 0x00, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00 };
			Assert.AreEqual (sdBinary, buffer);
			
			
			SecurityIdentifier builtInAdmins = new SecurityIdentifier (WellKnownSidType.BuiltinAdministratorsSid, null);
			CommonAce ace = new CommonAce (AceFlags.None, AceQualifier.AccessAllowed, 0x7FFFFFFF, builtInAdmins, false, null);
			acl.InsertAce (0, ace);
			buffer = new byte[acl.BinaryLength];
			acl.GetBinaryForm (buffer, 0);
			sdBinary = new byte[] {
				0x01, 0x00, 0x20, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00,
				0x18, 0x00, 0xFF, 0xFF, 0xFF, 0x7F, 0x01, 0x02, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x05, 0x20, 0x00, 0x00, 0x00, 0x20, 0x02,
				0x00, 0x00 };
			Assert.AreEqual (sdBinary, buffer);
		}
	}
}
