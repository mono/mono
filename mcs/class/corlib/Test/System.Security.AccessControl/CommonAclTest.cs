// CommonAclTest.cs - NUnit Test Cases for CommonAcl
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
	public class CommonAclTest
	{
		[Test]
		public void RevisionOK ()
		{
			DiscretionaryAcl dacl = new DiscretionaryAcl(false, false, 0);
			Assert.AreEqual (2, dacl.Revision);
		}

		[Test]
		public void RevisionDSOK ()
		{
			DiscretionaryAcl dacl = new DiscretionaryAcl(false, true, 0);
			Assert.AreEqual (4, dacl.Revision);
		}

		[Test]
		public void NullRawAclRevisionOK ()
		{
			DiscretionaryAcl dacl1 = new DiscretionaryAcl (false, false, null);
			Assert.AreEqual (2, dacl1.Revision);

			DiscretionaryAcl dacl2 = new DiscretionaryAcl (false, true, null);
			Assert.AreEqual (4, dacl2.Revision);
		}

		[Test]
		public void UsesRawAclRevision ()
		{
			RawAcl acl1 = new RawAcl (RawAcl.AclRevisionDS, 0);
			DiscretionaryAcl dacl1 = new DiscretionaryAcl (false, false, acl1);
			Assert.AreEqual (4, dacl1.Revision);

			RawAcl acl2 = new RawAcl (RawAcl.AclRevision, 0);
			DiscretionaryAcl dacl2 = new DiscretionaryAcl (false, true, acl2);
			Assert.AreEqual (2, dacl2.Revision);
		}

		[Test]
		public void IndexerMakesCopies ()
		{
			// This behavior is mentioned in the DiscretionaryAcl RawAcl constructor overload.
			// Turns out it applies to more than just the constructor.
			SecurityIdentifier worldSid = new SecurityIdentifier ("WD");

			// RawAcl does not make copies.
			RawAcl acl = new RawAcl (RawAcl.AclRevision, 1);
			CommonAce ace = new CommonAce (AceFlags.SuccessfulAccess, AceQualifier.SystemAudit, 1, worldSid, false, null);
			acl.InsertAce (0, ace);
			Assert.AreSame (acl [0], acl [0]);

			// CommonAcl does.
			SystemAcl sacl = new SystemAcl (false, false, acl);
			Assert.AreNotSame (sacl [0], sacl [0]);

			// Make sure the copying occurs in the constructor as well as the indexer.
			ace.AceFlags = AceFlags.FailedAccess;
			Assert.AreEqual (AceFlags.SuccessfulAccess, sacl [0].AceFlags);
		}

		[Test]
		public void EmptyBinaryLengthOK()
		{
			DiscretionaryAcl dacl = new DiscretionaryAcl (false, false, 0);
			Assert.AreEqual (8, dacl.BinaryLength);
		}

		[Test]
		public void EmptyBinaryFormOK()
		{
			DiscretionaryAcl dacl = new DiscretionaryAcl(false, false, 0);
			byte[] buffer = new byte[8];
			dacl.GetBinaryForm (buffer, 0);

			Assert.AreEqual (2, buffer [0]); // Revision
			Assert.AreEqual (8, ToUInt16 (buffer, 2)); // ACL Size
			Assert.AreEqual (0, ToUInt16 (buffer, 4)); // ACE Count
		}

		[Test]
		public void EmptyBinaryFormDSOK()
		{
			DiscretionaryAcl dacl = new DiscretionaryAcl (false, true, 0);
			byte[] buffer = new byte[8];
			dacl.GetBinaryForm (buffer, 0);

			Assert.AreEqual (4, buffer [0]); // Revision
			Assert.AreEqual (8, ToUInt16 (buffer, 2)); // ACL Size
			Assert.AreEqual (0, ToUInt16 (buffer, 4)); // ACE Count
		}

		[Test] // ... stumbled upon this by choosing Guid.Empty when needing an arbitrary GUID ...
		public void GuidEmptyMergesRegardlessOfFlagsAndOpaqueDataIsNotConsidered ()
		{
			SecurityIdentifier sid = new SecurityIdentifier (WellKnownSidType.BuiltinUsersSid, null);
			
			RawAcl acl = MakeRawAcl (new GenericAce[] {
				new CommonAce (AceFlags.None, AceQualifier.AccessAllowed, 1, sid, true, new byte[12]),
				new CommonAce (AceFlags.None, AceQualifier.AccessAllowed, 4, sid, false, new byte[8]), // gets merged
				new ObjectAce (AceFlags.None, AceQualifier.AccessAllowed, 1, sid,
				               ObjectAceFlags.ObjectAceTypePresent, Guid.Empty, Guid.Empty, false, new byte[8]),
				new ObjectAce (AceFlags.None, AceQualifier.AccessAllowed, 2, sid,
				               ObjectAceFlags.InheritedObjectAceTypePresent, Guid.Empty, Guid.Empty, true, new byte[16]), // gets merged
				new ObjectAce (AceFlags.None, AceQualifier.AccessAllowed, 4, sid,
				               ObjectAceFlags.InheritedObjectAceTypePresent, Guid.Empty, Guid.NewGuid (), true, new byte[4])
			});
			Assert.AreEqual (236, acl.BinaryLength);

			DiscretionaryAcl dacl = new DiscretionaryAcl (false, false, acl);
			Assert.IsTrue (dacl.IsCanonical);
			Assert.AreEqual (3, dacl.Count);

			CommonAce cace = (CommonAce)dacl [0];
			Assert.AreEqual (12, cace.OpaqueLength);
			Assert.AreEqual (5, cace.AccessMask);
			Assert.IsTrue (cace.IsCallback);

			ObjectAce oace = (ObjectAce)dacl [1];
			Assert.AreEqual (8, oace.OpaqueLength);
			Assert.AreEqual (3, oace.AccessMask);
			Assert.AreEqual (ObjectAceFlags.ObjectAceTypePresent, oace.ObjectAceFlags);
			Assert.AreEqual (Guid.Empty, oace.ObjectAceType);
			Assert.IsFalse (oace.IsCallback);
		}

		[Test]
		public void DetectsCanonicalMergesAndRemovesInheritedAces ()
		{
			SecurityIdentifier sid = new SecurityIdentifier (WellKnownSidType.BuiltinUsersSid, null);

			RawAcl acl = MakeRawAcl(new GenericAce[] {
				new CommonAce (AceFlags.None, AceQualifier.AccessDenied, 4, sid, false, null),
				new CommonAce (AceFlags.None, AceQualifier.AccessDenied, 8, sid, false, null),
				new CommonAce (AceFlags.None, AceQualifier.AccessAllowed, 1, sid, false, null),
				new CommonAce (AceFlags.None, AceQualifier.AccessAllowed, 2, sid, false, null),
				new CommonAce (AceFlags.Inherited, AceQualifier.AccessAllowed, 4, sid, false, null),
				new CommonAce (AceFlags.Inherited, AceQualifier.AccessAllowed, 4, sid, false, null)
			});
			Assert.AreEqual (6, acl.Count);

			DiscretionaryAcl dacl = new DiscretionaryAcl (false, false, acl);
			Assert.IsTrue (dacl.IsCanonical);
			Assert.AreEqual (4, dacl.Count);

			Assert.AreEqual (AceFlags.None, ((CommonAce)dacl [0]).AceFlags);
			Assert.AreEqual (AceFlags.None, ((CommonAce)dacl [1]).AceFlags);
			Assert.AreEqual (AceFlags.Inherited, ((CommonAce)dacl [2]).AceFlags);
			Assert.AreEqual (AceFlags.Inherited, ((CommonAce)dacl [3]).AceFlags);
			Assert.AreEqual (AceQualifier.AccessDenied, ((CommonAce)dacl [0]).AceQualifier);
			Assert.AreEqual (AceQualifier.AccessAllowed, ((CommonAce)dacl [1]).AceQualifier);
			Assert.AreEqual (AceQualifier.AccessAllowed, ((CommonAce)dacl [2]).AceQualifier);
			Assert.AreEqual (AceQualifier.AccessAllowed, ((CommonAce)dacl [3]).AceQualifier);
			GenericAce ace7 = dacl[0];
			Assert.IsInstanceOfType (typeof (CommonAce), ace7);

			dacl.RemoveInheritedAces ();
			Assert.AreEqual (2, dacl.Count);

			dacl.Purge (sid);
			Assert.AreEqual (0, dacl.Count);
		}

		[Test]
		public void MergesAfterSortingForMultipleSids ()
		{
			SecurityIdentifier adminSid = new SecurityIdentifier
				(WellKnownSidType.BuiltinAdministratorsSid, null); // S-1-5-32-544

			SecurityIdentifier userSid = new SecurityIdentifier
				(WellKnownSidType.BuiltinUsersSid, null); // S-1-5-32-545

			RawAcl acl = MakeRawAcl(new GenericAce[] {
				new CommonAce (AceFlags.None, AceQualifier.AccessDenied, 1, userSid, false, null),
				new CommonAce (AceFlags.None, AceQualifier.AccessDenied, 2, adminSid, false, null),
				new CommonAce (AceFlags.None, AceQualifier.AccessDenied, 4, userSid, false, null),
				new CommonAce (AceFlags.None, AceQualifier.AccessDenied, 8, adminSid, false, null),
			});

			DiscretionaryAcl dacl = new DiscretionaryAcl (false, false, acl);
			Assert.IsTrue (dacl.IsCanonical);
			Assert.AreEqual (2, dacl.Count);

			CommonAce adminAce = (CommonAce)dacl [0];
			Assert.AreEqual (adminSid, adminAce.SecurityIdentifier);
			Assert.AreEqual (10, adminAce.AccessMask);

			CommonAce userAce = (CommonAce)dacl [1];
			Assert.AreEqual (userSid, userAce.SecurityIdentifier);
			Assert.AreEqual (5, userAce.AccessMask);
		}

		[Test]
		public void DetectsNonCanonicalAndDoesNotMerge ()
		{
			SecurityIdentifier sid = new SecurityIdentifier (WellKnownSidType.BuiltinUsersSid, null);

			RawAcl acl = MakeRawAcl(new GenericAce[] {
				new CommonAce (AceFlags.None, AceQualifier.AccessAllowed, 1, sid, false, null),
				new CommonAce (AceFlags.None, AceQualifier.AccessAllowed, 2, sid, false, null),
				new CommonAce (AceFlags.None, AceQualifier.AccessDenied, 4, sid, false, null),
				new CommonAce (AceFlags.None, AceQualifier.AccessDenied, 8, sid, false, null)
			});

			DiscretionaryAcl dacl = new DiscretionaryAcl (false, false, acl);
			Assert.IsFalse (dacl.IsCanonical);
			Assert.AreEqual (4, dacl.Count);
		}

		[Test]
		public void DoesNotMergeOrEvaluateOrderingForInherited ()
		{
			SecurityIdentifier sid = new SecurityIdentifier (WellKnownSidType.BuiltinUsersSid, null);

			RawAcl acl = MakeRawAcl(new GenericAce[] {
				new CommonAce (AceFlags.Inherited, AceQualifier.AccessDenied, 1, sid, false, null),
				new CommonAce (AceFlags.Inherited, AceQualifier.AccessAllowed, 2, sid, false, null),
				new CommonAce (AceFlags.Inherited, AceQualifier.AccessAllowed, 4, sid, false, null),
				new CommonAce (AceFlags.Inherited, AceQualifier.AccessDenied, 8, sid, false, null)
			});

			DiscretionaryAcl dacl = new DiscretionaryAcl (false, false, acl);
			Assert.IsTrue (dacl.IsCanonical);
			Assert.AreEqual (4, dacl.Count);
		}

		[Test]
		public void SetterNotSupported ()
		{
			SecurityIdentifier sid = new SecurityIdentifier (WellKnownSidType.BuiltinUsersSid, null);

			RawAcl acl = MakeRawAcl(new GenericAce[] {
				new CommonAce (AceFlags.None, AceQualifier.AccessDenied, 1, sid, false, null),
				new CommonAce (AceFlags.Inherited, AceQualifier.AccessDenied, 2, sid, false, null),
				new CommonAce (AceFlags.Inherited, AceQualifier.AccessAllowed, 4, sid, false, null),
				new CommonAce (AceFlags.Inherited, AceQualifier.AccessAllowed, 4, sid, false, null)
			});

			DiscretionaryAcl dacl = new DiscretionaryAcl (false, false, acl);
			Assert.IsTrue (dacl.IsCanonical);
			Assert.AreEqual (4, dacl.Count);

			bool throws1 = false;
			try { dacl[0] = acl[0]; } catch (NotSupportedException) { throws1 = true; }
			Assert.IsTrue (throws1);

			bool throws2 = false;
			try { dacl[0] = acl[2]; } catch (NotSupportedException) { throws2 = true; }
			Assert.IsTrue (throws2);
		}

		// FIXME: Uncomment this once CompoundAce is implemented on Mono.
		/*
		[Test]
		public void CompoundAcesAreNotCanonical ()
		{
			SecurityIdentifier sid = new SecurityIdentifier (WellKnownSidType.BuiltinUsersSid, null);

			RawAcl acl = MakeRawAcl(new GenericAce[] {
				new CompoundAce (AceFlags.None, 1, CompoundAceType.Impersonation, sid)
			});

			DiscretionaryAcl dacl = new DiscretionaryAcl (false, false, acl);
			Assert.IsFalse (dacl.IsCanonical);
		}
		*/

		[Test]
		public void RemovesMeaninglessAces ()
		{
			RawAcl acl = GetRemovesMeaninglessAcesAcl ();

			DiscretionaryAcl dacl1 = new DiscretionaryAcl (false, false, acl);
			Assert.IsTrue (dacl1.IsCanonical);
			Assert.AreEqual (7, dacl1.Count);
			Assert.AreEqual (12, ((KnownAce)dacl1 [0]).AccessMask);

			DiscretionaryAcl dacl2 = new DiscretionaryAcl (true, false, acl);
			Assert.IsTrue (dacl2.IsCanonical);
			Assert.AreEqual (8, dacl2.Count);
			Assert.AreEqual (12, ((KnownAce)dacl1 [0]).AccessMask);
		}

		// shared with BinaryRoundtrip as well
		static RawAcl GetRemovesMeaninglessAcesAcl ()
		{
			SecurityIdentifier sid = new SecurityIdentifier (WellKnownSidType.BuiltinUsersSid, null);

			return MakeRawAcl(new GenericAce[] {
				new CommonAce (AceFlags.None, AceQualifier.AccessDenied, 4, sid, false, null),
				new CommonAce (AceFlags.None, AceQualifier.AccessDenied, 8, sid, false, null), // merged
				new CommonAce (AceFlags.InheritOnly|AceFlags.ObjectInherit,
				               AceQualifier.AccessDenied, 42, sid, false, null), // removed ONLY if !IsContainer
				new CommonAce (AceFlags.None, AceQualifier.AccessAllowed, 0, sid, false, null), // removed
				new CommonAce (AceFlags.None, AceQualifier.AccessAllowed, 1, sid, false, null),
				new CommonAce (AceFlags.None, AceQualifier.AccessAllowed, 2, sid, false, null), // merged
				new CommonAce (AceFlags.Inherited, AceQualifier.AccessAllowed, 1, sid, false, null),
				new CommonAce (AceFlags.Inherited, AceQualifier.AccessAllowed, 2, sid, false, null),
				new ObjectAce (AceFlags.None, AceQualifier.AccessAllowed, 0, sid,
				               ObjectAceFlags.None, Guid.NewGuid (), Guid.NewGuid (), false, null), // removed
				new ObjectAce (AceFlags.InheritOnly, AceQualifier.AccessAllowed, 1, sid,
				               ObjectAceFlags.ObjectAceTypePresent, Guid.NewGuid (), Guid.NewGuid (), false, null), // removed
				new ObjectAce (AceFlags.Inherited, AceQualifier.AccessAllowed, 1, sid,
				               ObjectAceFlags.None, Guid.NewGuid (), Guid.NewGuid (), false, null),
				new CommonAce (AceFlags.Inherited, AceQualifier.AccessAllowed, 0, sid, false, null), // removed
				new CommonAce (AceFlags.Inherited, AceQualifier.AccessDenied, 4, sid, false, null),
				new CommonAce (AceFlags.Inherited, AceQualifier.AccessDenied, 4, sid, false, null),
				new CommonAce (AceFlags.Inherited, AceQualifier.SystemAlarm, 4, sid, false, null), // removed
				new CommonAce (AceFlags.Inherited, AceQualifier.SystemAudit, 4, sid, false, null) // removed
			});
		}

		[Test]
		public void BinaryRoundtrip ()
		{
			RawAcl acl = GetRemovesMeaninglessAcesAcl ();

			DiscretionaryAcl dacl1 = new DiscretionaryAcl (false, false, acl);
			byte[] binaryForm1 = new byte[dacl1.BinaryLength];
			dacl1.GetBinaryForm (binaryForm1, 0);

			DiscretionaryAcl dacl2 = new DiscretionaryAcl (false, false, new RawAcl (binaryForm1, 0));
			byte[] binaryForm2 = new byte[dacl2.BinaryLength];
			dacl2.GetBinaryForm (binaryForm2, 0);

			Assert.AreEqual (binaryForm1.Length, binaryForm2.Length);
			for (int i = 0; i < binaryForm1.Length; i ++)
				Assert.AreEqual (binaryForm1 [i], binaryForm2 [i]);
		}

		[Test]
		public void ContiguousRangeSorting ()
		{
			SecurityIdentifier[] sids = new SecurityIdentifier[] {
				new SecurityIdentifier (WellKnownSidType.BuiltinAdministratorsSid, null), // S-1-5-32-544
				new SecurityIdentifier (WellKnownSidType.BuiltinUsersSid, null),	  // S-1-5-32-545
				new SecurityIdentifier (WellKnownSidType.WorldSid, null), 		  // S-1-1-0
				new SecurityIdentifier ("S-1-5-40"),
				new SecurityIdentifier ("S-1-5-30-123"),
				new SecurityIdentifier ("S-1-5-32-99"),
				new SecurityIdentifier ("S-1-5-23-45-67"),
				new SecurityIdentifier ("S-1-5-32-5432"),
				new SecurityIdentifier ("S-1-0-2"),
				new SecurityIdentifier ("S-1-6-0")
			};

			GenericAce[] aces = new GenericAce[sids.Length];
			for (int i = 0; i < aces.Length; i ++)
				aces [i] = new CommonAce (AceFlags.None, AceQualifier.AccessAllowed, 1, sids[i], false, null);
			RawAcl acl = MakeRawAcl (aces);

			DiscretionaryAcl dacl = new DiscretionaryAcl (false, false, acl);
			Assert.IsTrue (dacl.IsCanonical);
			Assert.AreEqual (sids[8], ((CommonAce)dacl [0]).SecurityIdentifier); // S-1-0-2
			Assert.AreEqual (sids[2], ((CommonAce)dacl [1]).SecurityIdentifier); // S-1-1-0
			Assert.AreEqual (sids[3], ((CommonAce)dacl [2]).SecurityIdentifier); // S-1-5-40
			Assert.AreEqual (sids[4], ((CommonAce)dacl [3]).SecurityIdentifier); // S-1-5-30-123
			Assert.AreEqual (sids[5], ((CommonAce)dacl [4]).SecurityIdentifier); // S-1-5-32-99
			Assert.AreEqual (sids[0], ((CommonAce)dacl [5]).SecurityIdentifier); // S-1-5-32-544
			Assert.AreEqual (sids[1], ((CommonAce)dacl [6]).SecurityIdentifier); // S-1-5-32-545
			Assert.AreEqual (sids[7], ((CommonAce)dacl [7]).SecurityIdentifier); // S-1-5-32-5432
			Assert.AreEqual (sids[6], ((CommonAce)dacl [8]).SecurityIdentifier); // S-1-5-23-45-67
			Assert.AreEqual (sids[9], ((CommonAce)dacl [9]).SecurityIdentifier); // S-1-6-0
		}

		static RawAcl MakeRawAcl (GenericAce[] aces)
		{
			RawAcl acl = new RawAcl (RawAcl.AclRevision, 0);
			for (int i = 0; i < aces.Length; i ++) { acl.InsertAce (i, aces [i]); }
			return acl;
		}

		static ushort ToUInt16 (byte[] buffer, int offset)
		{
			return (ushort)(buffer [offset] | buffer [offset + 1]);
		}

		[Test]
		public void InheritanceFlagsMergeForAccessMasksThatMatch ()
		{
			SecurityIdentifier sid = new SecurityIdentifier ("BU");

			RawAcl acl = MakeRawAcl (new GenericAce[] {
				new CommonAce (AceFlags.ContainerInherit, AceQualifier.AccessAllowed, 1, sid, false, null),
				new CommonAce (AceFlags.ObjectInherit, AceQualifier.AccessAllowed, 1, sid, false, null)
			});

			DiscretionaryAcl dacl = new DiscretionaryAcl (true, false, acl);
			Assert.AreEqual (1, dacl.Count);

			CommonAce ace = (CommonAce) dacl [0];
			Assert.AreEqual (InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, ace.InheritanceFlags);
		}

		[Test]
		public void InheritanceFlagsDoNotMergeForAccessMasksThatAND ()
		{
			SecurityIdentifier sid = new SecurityIdentifier ("BU");

			RawAcl acl = MakeRawAcl (new GenericAce[] {
				new CommonAce (AceFlags.ContainerInherit, AceQualifier.AccessAllowed, 1, sid, false, null),
				new CommonAce (AceFlags.ObjectInherit, AceQualifier.AccessAllowed, 3, sid, false, null)
			});

			DiscretionaryAcl dacl = new DiscretionaryAcl (true, false, acl);
			Assert.AreEqual (2, dacl.Count);
		}

		[Test]
		public void InheritanceFlagsAreClearedBeforeMergeCheckingWhenNotContainer ()
		{
			SecurityIdentifier sid = new SecurityIdentifier ("BU");

			RawAcl acl = MakeRawAcl (new GenericAce[] {
				new CommonAce (AceFlags.ContainerInherit, AceQualifier.AccessAllowed, 1, sid, false, null),
				new CommonAce (AceFlags.ObjectInherit, AceQualifier.AccessAllowed, 2, sid, false, null)
			});

			DiscretionaryAcl dacl = new DiscretionaryAcl (false, false, acl);
			Assert.AreEqual (1, dacl.Count);

			CommonAce ace = (CommonAce) dacl [0];
			Assert.AreEqual (3, ace.AccessMask);
			Assert.AreEqual (InheritanceFlags.None, ace.InheritanceFlags);
		}
	}
}

