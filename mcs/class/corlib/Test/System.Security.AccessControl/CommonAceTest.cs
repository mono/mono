//
// CommonAceTest.cs - NUnit Test Cases for CommonAce
//
// Author:
//	Kenneth Bell
//

using System.Security.AccessControl;
using System.Security.Principal;
using NUnit.Framework;

namespace MonoTests.System.Security.AccessControl {
	[TestFixture]
	public class CommonAceTest {
		[Test]
		public void GetBinaryForm ()
		{
			SecurityIdentifier builtInAdmins = new SecurityIdentifier (WellKnownSidType.BuiltinAdministratorsSid, null);
			CommonAce ace = new CommonAce (AceFlags.None, AceQualifier.AccessAllowed, 0x7FFFFFFF, builtInAdmins, false, null);
			
			byte[] buffer = new byte[ace.BinaryLength];
			ace.GetBinaryForm (buffer, 0);
			byte[] aceBinary = new byte[] {
				0x00, 0x00, 0x18, 0x00, 0xFF, 0xFF, 0xFF, 0x7F, 0x01, 0x02,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x05, 0x20, 0x00, 0x00, 0x00,
				0x20, 0x02, 0x00, 0x00 };
			Assert.AreEqual (aceBinary, buffer);
		}

		[Test]
		public void MaxOpaqueLength ()
		{
			Assert.AreEqual (65459, CommonAce.MaxOpaqueLength (true));
			Assert.AreEqual (65459, CommonAce.MaxOpaqueLength (false));
			Assert.AreEqual (65423, ObjectAce.MaxOpaqueLength (true));
			Assert.AreEqual (65423, ObjectAce.MaxOpaqueLength (false));
		}
	}
}
