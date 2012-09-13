// SystemAclTest.cs - NUnit Test Cases for SystemAcl
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
	public class SystemAclTest
	{
		[Test]
		public void StartsEmpty ()
		{
			Assert.AreEqual (0, new SystemAcl (false, false, 0).Count);
			//Assert.AreEqual (0, new SystemAcl (false, false, null).Count);
			// ^ MS.NET has a bug here and throws, contrary to their own documentation.
		}

		[Test]
		public void AddAuditMergesFlags ()
		{
			SecurityIdentifier sid = new SecurityIdentifier ("BA");
			SystemAcl sacl = new SystemAcl (false, false, 0);

			sacl.AddAudit (AuditFlags.Success, sid, 1, InheritanceFlags.None, PropagationFlags.None);
			sacl.AddAudit (AuditFlags.Failure, sid, 1, InheritanceFlags.None, PropagationFlags.None);
			Assert.AreEqual (1, sacl.Count);

			CommonAce ace = (CommonAce)sacl [0];
			Assert.AreEqual (AuditFlags.Success|AuditFlags.Failure, ace.AuditFlags);
		}

		[Test]
		public void AddAuditCommonAce ()
		{
			SecurityIdentifier sid = new SecurityIdentifier ("BA");
			SystemAcl sacl = new SystemAcl (false, false, 0);

			sacl.AddAudit (AuditFlags.Success, sid, 1, InheritanceFlags.None, PropagationFlags.None);
			Assert.AreEqual (1, sacl.Count);

			CommonAce ace = (CommonAce)sacl [0];
			Assert.AreEqual (AuditFlags.Success, ace.AuditFlags);
			Assert.AreEqual (1, ace.AccessMask);
			Assert.AreEqual ("S-1-5-32-544", ace.SecurityIdentifier.Value);
			Assert.IsFalse (ace.IsInherited);
		}

		[Test]
		public void AddAuditCommonAceUsingDSOverload ()
		{
			SecurityIdentifier sid = new SecurityIdentifier ("BA");
			SystemAcl sacl = new SystemAcl (false, true, 0);

			sacl.AddAudit (AuditFlags.Failure, sid, 1, InheritanceFlags.None, PropagationFlags.None,
			               ObjectAceFlags.None, Guid.NewGuid (), Guid.NewGuid ());
			Assert.AreEqual (1, sacl.Count);

			CommonAce ace = (CommonAce)sacl [0];
			Assert.AreEqual (AuditFlags.Failure, ace.AuditFlags);
			Assert.AreEqual (1, ace.AccessMask);
			Assert.AreEqual ("S-1-5-32-544", ace.SecurityIdentifier.Value);
			Assert.IsFalse (ace.IsInherited);
		}

		[Test]
		public void AddAuditObjectAceAndCommonAce ()
		{
			SecurityIdentifier sid = new SecurityIdentifier ("BA");
			SystemAcl sacl = new SystemAcl (false, true, 0);

			sacl.AddAudit (AuditFlags.Success, sid, 1, InheritanceFlags.None, PropagationFlags.None,
			               ObjectAceFlags.ObjectAceTypePresent, Guid.NewGuid (), Guid.Empty);
			sacl.AddAudit (AuditFlags.Success, sid, 1, InheritanceFlags.None, PropagationFlags.None,
				       ObjectAceFlags.None, Guid.Empty, Guid.Empty);
			Assert.AreEqual (2, sacl.Count);

			CommonAce cace = (CommonAce)sacl [0];
			Assert.AreEqual (1, cace.AccessMask);
			Assert.AreEqual ("S-1-5-32-544", cace.SecurityIdentifier.Value);
			Assert.IsFalse (cace.IsCallback);
			Assert.IsFalse (cace.IsInherited);

			ObjectAce oace = (ObjectAce)sacl [1];
			Assert.AreEqual (1, oace.AccessMask);
			Assert.AreEqual ("S-1-5-32-544", oace.SecurityIdentifier.Value);
			Assert.IsFalse (oace.IsCallback);
			Assert.IsFalse (oace.IsInherited);

			sacl.AddAudit (AuditFlags.Success, sid, 2, InheritanceFlags.None, PropagationFlags.None,
				       ObjectAceFlags.None, Guid.Empty, Guid.Empty);
			Assert.AreEqual (2, sacl.Count);

			CommonAce cace2 = (CommonAce)sacl [0];
			Assert.AreEqual (3, cace2.AccessMask);
		}

		[Test]
		public void RemoveSpecific ()
		{
			SecurityIdentifier sid = new SecurityIdentifier ("BA");
			SystemAcl sacl = new SystemAcl (false, false, 0);

			RemoveSpecificBegin (sid, sacl, InheritanceFlags.None);
			sacl.RemoveAuditSpecific (AuditFlags.Success, sid, 3, InheritanceFlags.None, PropagationFlags.None);
			Assert.AreEqual (0, sacl.Count);
		}

		[Test]
		public void RemoveSpecificUsingDSOverload ()
		{
			SecurityIdentifier sid = new SecurityIdentifier ("BA");
			SystemAcl sacl = new SystemAcl (false, true, 0);

			RemoveSpecificBegin (sid, sacl, InheritanceFlags.None);
			sacl.RemoveAuditSpecific (AuditFlags.Success, sid, 3, InheritanceFlags.None, PropagationFlags.None,
			                          ObjectAceFlags.ObjectAceTypePresent, Guid.Empty, Guid.Empty);
			Assert.AreEqual (1, sacl.Count);
			sacl.RemoveAuditSpecific (AuditFlags.Success, sid, 3, InheritanceFlags.None, PropagationFlags.None,
			                          ObjectAceFlags.None, Guid.Empty, Guid.Empty);
			Assert.AreEqual (0, sacl.Count);
		}

		void RemoveSpecificBegin (SecurityIdentifier sid, SystemAcl sacl, InheritanceFlags inheritanceFlags)
		{
			SecurityIdentifier otherSid = new SecurityIdentifier ("BU");

			sacl.AddAudit (AuditFlags.Success, sid, 3, inheritanceFlags, PropagationFlags.None);
			Assert.AreEqual (1, sacl.Count);
			sacl.RemoveAuditSpecific (AuditFlags.Failure, sid, 1, inheritanceFlags, PropagationFlags.None);
			Assert.AreEqual (1, sacl.Count);
			sacl.RemoveAuditSpecific (AuditFlags.Success, otherSid, 1, inheritanceFlags, PropagationFlags.None);
			Assert.AreEqual (1, sacl.Count);
			sacl.RemoveAuditSpecific (AuditFlags.Success, sid, 1, inheritanceFlags, PropagationFlags.None);
			Assert.AreEqual (1, sacl.Count);
			Assert.AreEqual (3, ((CommonAce)sacl [0]).AccessMask);
			sacl.RemoveAuditSpecific (AuditFlags.Success, sid, 3,
			                          inheritanceFlags ^ InheritanceFlags.ContainerInherit,
			                          PropagationFlags.None);
			Assert.AreEqual (1, sacl.Count);
		}

		[Test]
		public void SetAudit ()
		{
			SecurityIdentifier adminSid = new SecurityIdentifier ("BA"); // S-1-5-32-544
			SecurityIdentifier userSid = new SecurityIdentifier ("BU"); // S-1-5-32-545

			SystemAcl sacl = new SystemAcl (true, false, 0);
			sacl.SetAudit (AuditFlags.Success, adminSid, 1, InheritanceFlags.ObjectInherit, PropagationFlags.None);
			sacl.SetAudit (AuditFlags.Success, userSid, 2, InheritanceFlags.None, PropagationFlags.None);
			Assert.AreEqual (2, sacl.Count);

			CommonAce ace = (CommonAce)sacl [0];
			Assert.AreEqual (adminSid, ace.SecurityIdentifier);
			Assert.AreEqual (1, ace.AccessMask);

			sacl.SetAudit (AuditFlags.Success, adminSid, 4, InheritanceFlags.ObjectInherit, PropagationFlags.None);
			Assert.AreNotEqual (4, ace.AccessMask);
			ace = (CommonAce)sacl [0];
			Assert.AreEqual (4, ace.AccessMask);

			sacl.SetAudit (AuditFlags.Failure, adminSid, 4, InheritanceFlags.ObjectInherit, PropagationFlags.None);
			Assert.AreEqual (2, sacl.Count);
			ace = (CommonAce)sacl [0];
			Assert.AreEqual (AuditFlags.Failure, ace.AuditFlags);
			Assert.AreEqual (adminSid, ace.SecurityIdentifier);
			ace = (CommonAce)sacl [1];
			Assert.AreEqual (AuditFlags.Success, ace.AuditFlags);
			Assert.AreEqual (userSid, ace.SecurityIdentifier);
		}
	}
}

