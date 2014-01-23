// ObjectAceTest.cs - NUnit Test Cases for ObjectAce
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
	public class ObjectAceTest
	{
		static RawAcl CreateRoundtripRawAcl ()
		{
			SecurityIdentifier sid = new SecurityIdentifier (WellKnownSidType.BuiltinUsersSid, null);
			Assert.AreEqual (16, sid.BinaryLength);
			
			GenericAce[] aces = new GenericAce[] {
				new ObjectAce (AceFlags.None, AceQualifier.AccessAllowed, 1, sid,
				               ObjectAceFlags.ObjectAceTypePresent,
				               Guid.Empty, Guid.Empty, false, new byte[8]),
				new ObjectAce (AceFlags.None, AceQualifier.AccessAllowed, 2, sid,
				               ObjectAceFlags.InheritedObjectAceTypePresent,
				               Guid.Empty, Guid.Empty, true, new byte[16]),
				new ObjectAce (AceFlags.None, AceQualifier.AccessAllowed, 4, sid,
				               ObjectAceFlags.InheritedObjectAceTypePresent,
				               Guid.Empty, new Guid ("{8865FB90-A9EB-422F-A8BA-07ECA611D699}"), true, new byte[4]),
				new ObjectAce (AceFlags.None, AceQualifier.AccessAllowed, 4, sid,
				               ObjectAceFlags.ObjectAceTypePresent|ObjectAceFlags.InheritedObjectAceTypePresent,
				               Guid.Empty, new Guid ("{B893007C-38D5-4827-A698-BA25F1E30BAC}"), true, new byte[4]),
				new ObjectAce (AceFlags.None, AceQualifier.AccessAllowed, 4, sid,
				               ObjectAceFlags.None,
				               Guid.Empty, new Guid ("{C0F9DF22-C320-4400-B41F-754F69668640}"), true, new byte[4])
			};
			
			// Make sure this created right, first of all.
			Assert.AreEqual (AceType.AccessAllowedObject, aces [0].AceType);
			Assert.AreEqual (AceType.AccessAllowedCallbackObject, aces [1].AceType);
			Assert.AreEqual (AceType.AccessAllowedCallbackObject, aces [2].AceType);
			Assert.AreEqual (AceType.AccessAllowedCallbackObject, aces [3].AceType);
			Assert.AreEqual (AceType.AccessAllowedCallbackObject, aces [4].AceType);
			Assert.AreEqual (52, aces [0].BinaryLength);
			Assert.AreEqual (60, aces [1].BinaryLength);
			Assert.AreEqual (48, aces [2].BinaryLength);
			Assert.AreEqual (64, aces [3].BinaryLength);
			Assert.AreEqual (32, aces [4].BinaryLength);

			RawAcl acl = new RawAcl (RawAcl.AclRevision, 0);
			for (int i = 0; i < aces.Length; i ++)
				acl.InsertAce (i, aces[i]);
			return acl;
		}
		
		void CompareBinaryForms (byte[] binaryFormExpected, byte[] binaryFormActual)
		{
			Assert.AreEqual (binaryFormExpected.Length, binaryFormActual.Length);
			for (int i = 0; i < binaryFormExpected.Length; i ++)
				Assert.AreEqual (binaryFormExpected [i], binaryFormActual [i], "Mismatch at position " + i.ToString ());
		}
		
		[Test]
		public void BinaryRoundtrip ()
		{
			RawAcl acl = CreateRoundtripRawAcl ();
			byte[] binaryForm1 = new byte[acl.BinaryLength];
			acl.GetBinaryForm (binaryForm1, 0);

			RawAcl acl2 = new RawAcl (binaryForm1, 0);
			byte[] binaryForm2 = new byte[acl2.BinaryLength];
			acl2.GetBinaryForm (binaryForm2, 0);

			CompareBinaryForms (binaryForm1, binaryForm2);
		}
		
		[Test] // This blob produced by binaryForm1 from the BinaryRoundtrip test...
		public void BlobMatchesMSNet ()
		{
			RawAcl acl = CreateRoundtripRawAcl ();
			byte[] binaryForm1 = new byte[acl.BinaryLength];
			acl.GetBinaryForm (binaryForm1, 0);

			byte[] binaryForm2 = new byte[] { // 11 per line
(byte)0x02, (byte)0x00, (byte)0x08, (byte)0x01, (byte)0x05, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x05, (byte)0x00, (byte)0x34,
(byte)0x00, (byte)0x01, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x01, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00,
(byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00,
(byte)0x00, (byte)0x00, (byte)0x00, (byte)0x01, (byte)0x02, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x05,
(byte)0x20, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x21, (byte)0x02, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00,
(byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x0B, (byte)0x00, (byte)0x3C, (byte)0x00, (byte)0x02, (byte)0x00,
(byte)0x00, (byte)0x00, (byte)0x02, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00,
(byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00,
(byte)0x01, (byte)0x02, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x05, (byte)0x20, (byte)0x00, (byte)0x00,
(byte)0x00, (byte)0x21, (byte)0x02, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00,
(byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x0B,
(byte)0x00, (byte)0x30, (byte)0x00, (byte)0x04, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x02, (byte)0x00, (byte)0x00, (byte)0x00,
(byte)0x90, (byte)0xFB, (byte)0x65, (byte)0x88, (byte)0xEB, (byte)0xA9, (byte)0x2F, (byte)0x42, (byte)0xA8, (byte)0xBA, (byte)0x07,
(byte)0xEC, (byte)0xA6, (byte)0x11, (byte)0xD6, (byte)0x99, (byte)0x01, (byte)0x02, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00,
(byte)0x00, (byte)0x05, (byte)0x20, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x21, (byte)0x02, (byte)0x00, (byte)0x00, (byte)0x00,
(byte)0x00, (byte)0x00, (byte)0x00, (byte)0x0B, (byte)0x00, (byte)0x40, (byte)0x00, (byte)0x04, (byte)0x00, (byte)0x00, (byte)0x00,
(byte)0x03, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00,
(byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x7C, (byte)0x00,
(byte)0x93, (byte)0xB8, (byte)0xD5, (byte)0x38, (byte)0x27, (byte)0x48, (byte)0xA6, (byte)0x98, (byte)0xBA, (byte)0x25, (byte)0xF1,
(byte)0xE3, (byte)0x0B, (byte)0xAC, (byte)0x01, (byte)0x02, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x05,
(byte)0x20, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x21, (byte)0x02, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00,
(byte)0x00, (byte)0x0B, (byte)0x00, (byte)0x20, (byte)0x00, (byte)0x04, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00,
(byte)0x00, (byte)0x00, (byte)0x01, (byte)0x02, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x05, (byte)0x20,
(byte)0x00, (byte)0x00, (byte)0x00, (byte)0x21, (byte)0x02, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00
			};
			
			CompareBinaryForms (binaryForm2, binaryForm1);
		}
	}
}

