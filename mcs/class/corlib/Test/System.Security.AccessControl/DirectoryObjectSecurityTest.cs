// DirectoryObjectSecurityTest.cs - NUnit Test Cases for DirectoryObjectSecurity
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
	public class DirectoryObjectSecurityTest
	{
		[Test]
		public void Defaults ()
		{
			TestSecurity security = new TestSecurity ();
			Assert.IsTrue (security.IsContainerTest);
			Assert.IsTrue (security.IsDSTest);
		}

		[Test, ExpectedExceptionAttribute (typeof (ArgumentOutOfRangeException))]
		public void ChecksAccessControlModificationRange ()
		{
			bool modifiedRet, modifiedOut;
			TestSecurity security = new TestSecurity ();

			SecurityIdentifier sid = new SecurityIdentifier ("WD");
			TestAccessRule rule = new TestAccessRule
				(sid, 1, false, InheritanceFlags.None, PropagationFlags.None,
				 Guid.Empty, Guid.Empty, AccessControlType.Allow);

			modifiedRet = security.ModifyAccessRule ((AccessControlModification)43210,
			                           		 rule, out modifiedOut);
		}

		[Test]
		public void IgnoresResetOnAuditAndReturnsTrue ()
		{
			bool modifiedRet, modifiedOut;
			TestSecurity security = new TestSecurity ();

			SecurityIdentifier sid = new SecurityIdentifier ("WD");
			TestAuditRule rule = new TestAuditRule
				(sid, 1, false, InheritanceFlags.None, PropagationFlags.None,
				 Guid.Empty, Guid.Empty, AuditFlags.Success);

			modifiedRet = security.ModifyAuditRule (AccessControlModification.Reset,
			                           		rule, out modifiedOut);
			Assert.IsTrue (modifiedRet);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorFailsOnNullDescriptor ()
		{
			new TestSecurity (null);
		}

		[Test]
		public void ConstructorLetsFalseDSThrough ()
		{
			CommonSecurityDescriptor descriptor = new CommonSecurityDescriptor
				(false, false, ControlFlags.None, null, null, null, null);

			TestSecurity security = new TestSecurity (descriptor);
			Assert.IsFalse (security.IsContainerTest);
			Assert.IsFalse (security.IsDSTest);
		}

		[Test]
		public void ObjectSecurityJustWrapsCommonSecurityDescriptor ()
		{
			CommonSecurityDescriptor descriptor = new CommonSecurityDescriptor
				(false, false, ControlFlags.None, null, null, null, null);

			TestSecurity security = new TestSecurity (descriptor);
			Assert.IsNull (security.GetOwner (typeof(SecurityIdentifier)));
			SecurityIdentifier sid = new SecurityIdentifier ("WD");

			descriptor.Owner = sid; // Not virtual, so the conclusion in the test's title.
			Assert.IsFalse (security.OwnerModifiedTest);
			Assert.AreSame (sid, security.GetOwner (typeof(SecurityIdentifier)));

			security.SetOwner (sid);
			Assert.IsTrue (security.OwnerModifiedTest);
			Assert.AreSame (sid, security.GetOwner (typeof(SecurityIdentifier)));
		}

		[Test, ExpectedExceptionAttribute (typeof (InvalidOperationException))]
		public void LocksAreEnforced ()
		{
			TestSecurity security = new TestSecurity ();
			bool value = security.OwnerModifiedTestWithoutLock;
		}

		[Test]
		[Category ("NotWorking")] // Mono does not have a working CustomAce implementation yet.
		public void ObjectSecurityRemovesWhatItCannotCreate ()
		{
			RawAcl acl = new RawAcl (GenericAcl.AclRevision, 1);
			acl.InsertAce (0, new CustomAce ((AceType)255, AceFlags.None, new byte[4]));

			DiscretionaryAcl dacl = new DiscretionaryAcl (true, true, acl);
			Assert.AreEqual (1, dacl.Count);

			CommonSecurityDescriptor descriptor = new CommonSecurityDescriptor
				(true, true, ControlFlags.None, null, null, null, dacl);

			TestSecurity security = new TestSecurity (descriptor);
			AuthorizationRuleCollection rules = security.GetAccessRules (true, true, typeof (SecurityIdentifier));
			Assert.AreEqual (0, rules.Count);
		}

		[Test]
		public void FactoryWithoutGuidsCalledWhenNotObjectAce ()
		{
			TestSecurity security = FactoryCallTest (false);
			Assert.IsTrue (security.access_factory_called);
		}

		[Test, ExpectedExceptionAttribute (typeof (NotImplementedException))]
		public void FactoryWithGuidsThrowsNotImplementedByDefault ()
		{
			FactoryCallTest (true);
		}

		TestSecurity FactoryCallTest (bool objectAce)
		{
			SecurityIdentifier sid = new SecurityIdentifier ("WD");
			DiscretionaryAcl dacl = new DiscretionaryAcl (true, true, 1);
			dacl.AddAccess (AccessControlType.Allow, sid, 1,
			                InheritanceFlags.None, PropagationFlags.None,
			                objectAce ? ObjectAceFlags.ObjectAceTypePresent : ObjectAceFlags.None,
			                Guid.NewGuid (), Guid.Empty);

			CommonSecurityDescriptor descriptor = new CommonSecurityDescriptor
				(true, true, ControlFlags.None, null, null, null, dacl);

			TestSecurity security = new TestSecurity (descriptor);
			security.GetAccessRules (true, true, typeof (SecurityIdentifier));
			return security;
		}

		class TestAccessRule : ObjectAccessRule
		{
			public TestAccessRule(IdentityReference identity, int accessMask, bool isInherited,
			                      InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags,
			                      Guid objectType, Guid inheritedObjectType,
			                      AccessControlType type)
				: base(identity, accessMask, isInherited, inheritanceFlags, propagationFlags,
				       objectType, inheritedObjectType, type)
			{
			}
		}

		class TestAuditRule : ObjectAuditRule
		{
			public TestAuditRule(IdentityReference identity, int accessMask, bool isInherited,
			                     InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags,
			                     Guid objectType, Guid inheritedObjectType,
			                     AuditFlags flags)
				: base(identity, accessMask, isInherited, inheritanceFlags, propagationFlags,
				       objectType, inheritedObjectType, flags)
			{
			}
		}

		class TestSecurity : DirectoryObjectSecurity
		{
			internal bool access_factory_called;

			public TestSecurity ()
			{
			}

			public TestSecurity (CommonSecurityDescriptor descriptor)
				: base (descriptor)
			{
			}

			public bool IsContainerTest {
				get { return IsContainer; }
			}

			public bool IsDSTest {
				get { return IsDS; }
			}

			public bool OwnerModifiedTest {
				get { ReadLock (); bool value = OwnerModified; ReadUnlock (); return value; }
				set { WriteLock (); OwnerModified = value; WriteUnlock (); }
			}

			public bool OwnerModifiedTestWithoutLock {
				get { return OwnerModified; }
			}

			public override AccessRule AccessRuleFactory (IdentityReference identityReference,
			                                              int accessMask, bool isInherited,
			                                              InheritanceFlags inheritanceFlags,
			                                              PropagationFlags propagationFlags,
			                                              AccessControlType type)
			{
				access_factory_called = true;
				return new TestAccessRule (identityReference, accessMask,
				                           isInherited, inheritanceFlags, propagationFlags,
				                           Guid.Empty, Guid.Empty, type);
			}

			public override AuditRule AuditRuleFactory (IdentityReference identityReference,
				                                    int accessMask, bool isInherited,
				                                    InheritanceFlags inheritanceFlags,
			                                            PropagationFlags propagationFlags,
			                                            AuditFlags flags)
			{
				return new TestAuditRule (identityReference, accessMask,
				                          isInherited, inheritanceFlags, propagationFlags,
				                          Guid.Empty, Guid.Empty, flags);
			}

			public override Type AccessRightType {
				get { return typeof (int); }
			}

			public override Type AccessRuleType {
				get { return typeof (TestAccessRule); }
			}

			public override Type AuditRuleType {
				get { return typeof (TestAuditRule); }
			}
		}
	}
}

