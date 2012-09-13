// DiscretionaryAclTest.cs - NUnit Test Cases for DiscretionaryAcl
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
	public class DiscretionaryAclTest
	{
		[Test]
		public void StartsEmpty ()
		{
			Assert.AreEqual (0, new DiscretionaryAcl (false, false, 0).Count);
			Assert.AreEqual (0, new DiscretionaryAcl (false, false, null).Count);

		}

		[Test]
		public void AddAccessCommonAce ()
		{
			SecurityIdentifier sid = new SecurityIdentifier ("BA");
			DiscretionaryAcl dacl = new DiscretionaryAcl (false, false, 0);

			dacl.AddAccess (AccessControlType.Allow, sid, 1, InheritanceFlags.None, PropagationFlags.None);
			Assert.AreEqual (1, dacl.Count);

			CommonAce ace = (CommonAce)dacl[0];
			Assert.AreEqual (1, ace.AccessMask);
			Assert.AreEqual ("S-1-5-32-544", ace.SecurityIdentifier.Value);
			Assert.IsFalse (ace.IsInherited);
		}

		[Test]
		public void AddAccessCommonAceUsingDSOverload ()
		{
			SecurityIdentifier sid = new SecurityIdentifier ("BA");
			DiscretionaryAcl dacl = new DiscretionaryAcl (false, true, 0);

			dacl.AddAccess (AccessControlType.Allow, sid, 1, InheritanceFlags.None, PropagationFlags.None,
			                ObjectAceFlags.None, Guid.NewGuid (), Guid.NewGuid ());
			Assert.AreEqual (1, dacl.Count);

			CommonAce ace = (CommonAce)dacl [0];
			Assert.AreEqual (1, ace.AccessMask);
			Assert.AreEqual ("S-1-5-32-544", ace.SecurityIdentifier.Value);
			Assert.IsFalse (ace.IsInherited);
		}

		[Test, ExpectedException (typeof (InvalidOperationException))]
		public void AddAccessObjectAceNonDSFailsEvenIfObjectAceFlagsNoneImplyingCommonAce ()
		{
			SecurityIdentifier sid = new SecurityIdentifier ("BA");
			DiscretionaryAcl dacl = new DiscretionaryAcl (false, false, 0);

			dacl.AddAccess (AccessControlType.Allow, sid, 1, InheritanceFlags.None, PropagationFlags.None,
			                ObjectAceFlags.None, Guid.Empty, Guid.Empty);
		}

		[Test, ExpectedException (typeof (InvalidOperationException))]
		public void AddAccessFailsOnNonCanonical ()
		{
			SecurityIdentifier sid = new SecurityIdentifier ("BU");

			RawAcl acl = new RawAcl (RawAcl.AclRevision, 0);
			acl.InsertAce (0, new CommonAce (AceFlags.None, AceQualifier.AccessAllowed, 1, sid, false, null));
			acl.InsertAce (1, new CommonAce (AceFlags.None, AceQualifier.AccessDenied, 1, sid, false, null));

			DiscretionaryAcl dacl = new DiscretionaryAcl (false, false, acl);
			Assert.IsFalse (dacl.IsCanonical);
			Assert.AreEqual (2, dacl.Count);

			dacl.AddAccess (AccessControlType.Allow, sid, 1, InheritanceFlags.None, PropagationFlags.None);
		}

		[Test, ExpectedException (typeof (ArgumentException))]
		public void InheritanceFlagsRequireContainer ()
		{
			SecurityIdentifier sid = new SecurityIdentifier ("BU");
			DiscretionaryAcl dacl = new DiscretionaryAcl (false, false, 0);
			dacl.AddAccess (AccessControlType.Allow, sid, 3, InheritanceFlags.ContainerInherit, PropagationFlags.None);
		}

		[Test, ExpectedException (typeof (ArgumentException))]
		public void PropagationFlagsRequireInheritanceFlagsForAdd ()
		{
			SecurityIdentifier sid = new SecurityIdentifier ("BU");
			DiscretionaryAcl dacl = new DiscretionaryAcl (true, false, 0);
			dacl.AddAccess (AccessControlType.Allow, sid, 3, InheritanceFlags.None, PropagationFlags.InheritOnly);
		}

		[Test]
		public void AddAccessObjectAceAndCommonAce ()
		{
			SecurityIdentifier sid = new SecurityIdentifier ("BA");
			DiscretionaryAcl dacl = new DiscretionaryAcl (false, true, 0);

			dacl.AddAccess (AccessControlType.Allow, sid, 1, InheritanceFlags.None, PropagationFlags.None,
			                ObjectAceFlags.ObjectAceTypePresent, Guid.NewGuid (), Guid.Empty);
			dacl.AddAccess (AccessControlType.Allow, sid, 1, InheritanceFlags.None, PropagationFlags.None,
					ObjectAceFlags.None, Guid.Empty, Guid.Empty);
			Assert.AreEqual (2, dacl.Count);

			CommonAce cace = (CommonAce)dacl [0];
			Assert.AreEqual (1, cace.AccessMask);
			Assert.AreEqual ("S-1-5-32-544", cace.SecurityIdentifier.Value);
			Assert.IsFalse (cace.IsCallback);
			Assert.IsFalse (cace.IsInherited);

			ObjectAce oace = (ObjectAce)dacl [1];
			Assert.AreEqual (1, oace.AccessMask);
			Assert.AreEqual ("S-1-5-32-544", oace.SecurityIdentifier.Value);
			Assert.IsFalse (oace.IsCallback);
			Assert.IsFalse (oace.IsInherited);

			dacl.AddAccess (AccessControlType.Allow, sid, 2, InheritanceFlags.None, PropagationFlags.None,
					ObjectAceFlags.None, Guid.Empty, Guid.Empty);
			Assert.AreEqual (2, dacl.Count);

			CommonAce cace2 = (CommonAce)dacl [0];
			Assert.AreEqual (3, cace2.AccessMask);
		}

		[Test, ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void InvalidAccessControlType ()
		{
			// This is also testing the fact that the AccessControlType is checked before the
			// InheritanceFlags are validated -- IsContainer is false here, so if the InheritanceFlags
			// were checked first, ArgumentException would be thrown instead.
			SecurityIdentifier sid = new SecurityIdentifier ("BA");
			DiscretionaryAcl dacl = new DiscretionaryAcl (false, false, 0);
			dacl.AddAccess ((AccessControlType)43210, sid, 1, InheritanceFlags.ContainerInherit, PropagationFlags.None);
		}

		[Test]
		public void RemoveSpecific ()
		{
			SecurityIdentifier sid = new SecurityIdentifier ("BA");
			DiscretionaryAcl dacl = new DiscretionaryAcl (false, false, 0);

			RemoveSpecificBegin (sid, dacl, InheritanceFlags.None);
			dacl.RemoveAccessSpecific (AccessControlType.Allow, sid, 3, InheritanceFlags.None, PropagationFlags.None);
			Assert.AreEqual (0, dacl.Count);
		}

		[Test]
		public void RemoveSpecificUsingDSOverload ()
		{
			SecurityIdentifier sid = new SecurityIdentifier ("BA");
			DiscretionaryAcl dacl = new DiscretionaryAcl (false, true, 0);

			RemoveSpecificBegin (sid, dacl, InheritanceFlags.None);
			dacl.RemoveAccessSpecific (AccessControlType.Allow, sid, 3, InheritanceFlags.None, PropagationFlags.None,
			                           ObjectAceFlags.ObjectAceTypePresent, Guid.Empty, Guid.Empty);
			Assert.AreEqual (1, dacl.Count);
			dacl.RemoveAccessSpecific (AccessControlType.Allow, sid, 3, InheritanceFlags.None, PropagationFlags.None,
			                           ObjectAceFlags.None, Guid.Empty, Guid.Empty);
			Assert.AreEqual (0, dacl.Count);
		}

		[Test]
		public void RemoveSpecificIsContainer ()
		{
			SecurityIdentifier sid = new SecurityIdentifier ("BA");
			DiscretionaryAcl dacl = new DiscretionaryAcl (true, false, 0);

			RemoveSpecificBegin (sid, dacl, InheritanceFlags.ObjectInherit);
			dacl.RemoveAccessSpecific (AccessControlType.Allow, sid, 3, InheritanceFlags.ObjectInherit, PropagationFlags.None);
			Assert.AreEqual (0, dacl.Count);
		}

		[Test]
		public void RemoveSpecificIgnoresPropagationFlagsWhenMatchingInheritanceFlagsNone()
		{
			SecurityIdentifier sid = new SecurityIdentifier ("BA");
			DiscretionaryAcl dacl = new DiscretionaryAcl (false, false, 0);

			RemoveSpecificBegin (sid, dacl, InheritanceFlags.None);
			dacl.RemoveAccessSpecific (AccessControlType.Allow, sid, 3,
			                           InheritanceFlags.None, PropagationFlags.InheritOnly);
			Assert.AreEqual (0, dacl.Count);
		}

		void RemoveSpecificBegin (SecurityIdentifier sid, DiscretionaryAcl dacl, InheritanceFlags inheritanceFlags)
		{
			SecurityIdentifier otherSid = new SecurityIdentifier ("BU");

			dacl.AddAccess (AccessControlType.Allow, sid, 3, inheritanceFlags, PropagationFlags.None);
			Assert.AreEqual (1, dacl.Count);
			dacl.RemoveAccessSpecific (AccessControlType.Deny, sid, 1, inheritanceFlags, PropagationFlags.None);
			Assert.AreEqual (1, dacl.Count);
			dacl.RemoveAccessSpecific (AccessControlType.Allow, otherSid, 1, inheritanceFlags, PropagationFlags.None);
			Assert.AreEqual (1, dacl.Count);
			dacl.RemoveAccessSpecific (AccessControlType.Allow, sid, 1, inheritanceFlags, PropagationFlags.None);
			Assert.AreEqual (1, dacl.Count);
			Assert.AreEqual (3, ((CommonAce)dacl [0]).AccessMask);
			dacl.RemoveAccessSpecific (AccessControlType.Allow, sid, 3,
			                           inheritanceFlags ^ InheritanceFlags.ContainerInherit,
			                           PropagationFlags.None);
			Assert.AreEqual (1, dacl.Count);
		}

		[Test]
		public void PropagationFlagsDoNotRequireInheritanceFlagsForRemoveSpecific ()
		{
			SecurityIdentifier sid = new SecurityIdentifier ("BU");
			DiscretionaryAcl dacl = new DiscretionaryAcl (false, false, 0);
			dacl.RemoveAccessSpecific (AccessControlType.Allow, sid, 3,
			                           InheritanceFlags.ContainerInherit, PropagationFlags.InheritOnly);
		}

		[Test]
		public void SetAccess ()
		{
			SecurityIdentifier adminSid = new SecurityIdentifier ("BA"); // S-1-5-32-544
			SecurityIdentifier userSid = new SecurityIdentifier ("BU"); // S-1-5-32-545

			DiscretionaryAcl dacl = new DiscretionaryAcl (true, false, 0);
			dacl.SetAccess (AccessControlType.Allow, adminSid, 1, InheritanceFlags.ObjectInherit, PropagationFlags.None);
			dacl.SetAccess (AccessControlType.Allow, userSid, 2, InheritanceFlags.None, PropagationFlags.None);
			Assert.AreEqual (2, dacl.Count);

			CommonAce ace = (CommonAce)dacl [0];
			Assert.AreEqual (adminSid, ace.SecurityIdentifier);
			Assert.AreEqual (1, ace.AccessMask);

			dacl.SetAccess (AccessControlType.Allow, adminSid, 4, InheritanceFlags.ObjectInherit, PropagationFlags.None);
			Assert.AreNotEqual (4, ace.AccessMask); // remove and add, not modify, despite AccessMask having a setter
			ace = (CommonAce)dacl [0];
			Assert.AreEqual (4, ace.AccessMask);

			dacl.SetAccess (AccessControlType.Deny, adminSid, 4, InheritanceFlags.ObjectInherit, PropagationFlags.None);
			Assert.AreEqual (3, dacl.Count);
			ace = (CommonAce)dacl [0];
			Assert.AreEqual (AceQualifier.AccessDenied, ace.AceQualifier);
			ace = (CommonAce)dacl [1];
			Assert.AreEqual (AceQualifier.AccessAllowed, ace.AceQualifier);
		}
	}
}

