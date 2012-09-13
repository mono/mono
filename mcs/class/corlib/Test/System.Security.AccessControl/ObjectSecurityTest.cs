// ObjectSecurityTest.cs - NUnit Test Cases for ObjectSecurity
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
	public class ObjectSecurityTest
	{
		[Test]
		public void Defaults ()
		{
			TestSecurity security = new TestSecurity ();
			Assert.IsTrue (security.AreAccessRulesCanonical);
			Assert.IsTrue (security.AreAuditRulesCanonical);
			Assert.IsFalse (security.AreAccessRulesProtected);
			Assert.IsFalse (security.AreAuditRulesProtected);
			Assert.IsNull (security.GetGroup (typeof (SecurityIdentifier)));
			Assert.IsNull (security.GetOwner (typeof (SecurityIdentifier)));
		}

		[Test]
		public void DefaultsForSddlAndBinary ()
		{
			TestSecurity security = new TestSecurity ();
			Assert.AreEqual ("D:", security.GetSecurityDescriptorSddlForm (AccessControlSections.All));
			Assert.AreEqual (28, security.GetSecurityDescriptorBinaryForm ().Length);
		}

		[Test]
		public void SetSddlForm ()
		{
			TestSecurity security = new TestSecurity ();

			SecurityIdentifier groupSid = new SecurityIdentifier ("WD");
			SecurityIdentifier userSid = new SecurityIdentifier ("SY");

			security.SetGroup (groupSid);
			security.SetOwner (userSid);
			Assert.AreEqual ("G:WD", security.GetSecurityDescriptorSddlForm (AccessControlSections.Group));
			Assert.AreEqual ("O:SY", security.GetSecurityDescriptorSddlForm (AccessControlSections.Owner));
			security.SetSecurityDescriptorSddlForm ("O:BG", AccessControlSections.Owner);
			Assert.AreEqual ("O:BG", security.GetSecurityDescriptorSddlForm (AccessControlSections.Owner));
			Assert.AreEqual (new SecurityIdentifier ("BG"), security.GetOwner (typeof (SecurityIdentifier)));
		}

		[Test]
		public void SetSddlFormAllowsFlags ()
		{
			TestSecurity security = new TestSecurity ();
			security.SetSecurityDescriptorSddlForm ("G:BA", AccessControlSections.Group | AccessControlSections.Owner);
			Assert.AreEqual ("", security.GetSecurityDescriptorSddlForm (AccessControlSections.Owner));
			Assert.AreEqual ("G:BA", security.GetSecurityDescriptorSddlForm (AccessControlSections.Group));
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void SetGroupThrowsOnNull ()
		{
			TestSecurity security = new TestSecurity ();
			security.SetGroup (null);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void SetOwnerThrowsOnNull ()
		{
			TestSecurity security = new TestSecurity ();
			security.SetOwner (null);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void PurgeThrowsOnNull ()
		{
			TestSecurity security = new TestSecurity ();
			security.PurgeAccessRules (null);
		}

		[Test]
		public void AllTypesAcceptedOnGetGroupOwnerUntilTheyAreSet ()
		{
			TestSecurity security = new TestSecurity ();
			Assert.IsNull (security.GetGroup (typeof (void)));
			Assert.IsNull (security.GetOwner (typeof (int)));

			SecurityIdentifier everyoneSid = new SecurityIdentifier ("WD");
			security.SetOwner (everyoneSid);

			bool throwsOnInt = false;
			try { security.GetOwner (typeof (int)); } catch (ArgumentException) { throwsOnInt = true; }
			Assert.IsTrue (throwsOnInt);

			bool throwsOnSuperclass = false;
			try { security.GetOwner (typeof (IdentityReference)); } catch (ArgumentException) { throwsOnSuperclass = true; }
			Assert.IsTrue (throwsOnSuperclass);

			Assert.IsNull (security.GetGroup (typeof (void)));
			Assert.IsInstanceOfType (typeof (SecurityIdentifier), security.GetOwner (typeof (SecurityIdentifier)));
		}

		[Test]
		public void ModifyAccessRuleAllowsDerivedTypeAndCallsModifyAccessButNothingChanges ()
		{
			bool modifiedRet, modifiedOut;
			SecurityIdentifier everyoneSid = new SecurityIdentifier ("WD");
			TestSecurity security = new TestSecurity ();

			DerivedAccessRule rule = new DerivedAccessRule (everyoneSid, TestRights.One, AccessControlType.Allow);

			modifiedRet = security.ModifyAccessRule (AccessControlModification.Add, rule, out modifiedOut);
			Assert.AreEqual (modifiedRet, modifiedOut);
			Assert.IsTrue (modifiedRet);

			Assert.IsTrue (security.modify_access_called);
			Assert.AreEqual ("D:", security.GetSecurityDescriptorSddlForm (AccessControlSections.All));

			// (1) There is no external abstract/virtual 'get collection',
			// (2) The overrides in this test call this base class, which does not change it, and
			// (3) There are methods based on the collection value such as GetSecurityDescriptorSddlForm.
			// Conclusion: Collection is internal and manipulated by derived classes.
		}

		[Test, ExpectedException (typeof (ArgumentException))]
		public void ModifyAccessRuleThrowsOnWrongType ()
		{
			bool modified;
			SecurityIdentifier everyoneSid = new SecurityIdentifier ("WD");
			TestSecurity security = new TestSecurity ();

			FileSystemAccessRule rule = new FileSystemAccessRule
				(everyoneSid, FileSystemRights.FullControl, AccessControlType.Allow);

			security.ModifyAccessRule (AccessControlModification.Add, rule, out modified);
		}

		[Test]
		public void Reset ()
		{
			bool modifiedRet, modifiedOut;
			SecurityIdentifier everyoneSid = new SecurityIdentifier ("WD");
			TestSecurity security = new TestSecurity ();

			TestAccessRule rule = new TestAccessRule
				(everyoneSid, TestRights.One, AccessControlType.Allow);

			modifiedRet = security.ModifyAccessRule (AccessControlModification.Reset, rule, out modifiedOut);
		}

		[Test]
		public void Protection ()
		{
			TestSecurity security = new TestSecurity ();

			security.SetAccessRuleProtection (true, true);
			Assert.IsTrue (security.AreAccessRulesProtected);
			Assert.IsFalse (security.AreAuditRulesProtected);

			security.SetAuditRuleProtection (true, false);
			Assert.IsTrue (security.AreAccessRulesProtected);
			Assert.IsTrue (security.AreAuditRulesProtected);

			security.SetAccessRuleProtection (false, false);
			Assert.IsFalse (security.AreAccessRulesProtected);
			Assert.IsTrue (security.AreAuditRulesProtected);

			security.SetAuditRuleProtection (false, true);
			Assert.IsFalse (security.AreAccessRulesProtected);
			Assert.IsFalse (security.AreAuditRulesProtected);
		}

		enum TestRights
		{
			One = 1
		}

		class DerivedAccessRule : TestAccessRule
		{
			public DerivedAccessRule (IdentityReference identity, TestRights rights, AccessControlType type)
				: base (identity, rights, type)
			{
			}
		}

		class TestAccessRule : AccessRule
		{
			public TestAccessRule (IdentityReference identity, TestRights rights, AccessControlType type)
				: this (identity, rights, false, InheritanceFlags.None, PropagationFlags.None, type)
			{
			}

			public TestAccessRule (IdentityReference identity,
			                       TestRights rights, bool isInherited,
			                       InheritanceFlags inheritanceFlags,
			                       PropagationFlags propagationFlags,
			                       AccessControlType type)
				: base (identity, (int)rights, isInherited, inheritanceFlags, propagationFlags, type)
			{
			}
		}

		class TestAuditRule : AuditRule
		{
			public TestAuditRule (IdentityReference identity,
				              TestRights rights, bool isInherited,
				              InheritanceFlags inheritanceFlags,
			                      PropagationFlags propagationFlags,
			                      AuditFlags flags)
				: base (identity, (int)rights, isInherited, inheritanceFlags, propagationFlags, flags)
			{
			}
		}

		class TestSecurity : ObjectSecurity
		{
			internal bool modify_access_called;

			public TestSecurity () : base (false, false)
			{
			}

			public override AccessRule AccessRuleFactory (IdentityReference identityReference,
			                                              int accessMask, bool isInherited,
			                                              InheritanceFlags inheritanceFlags,
			                                              PropagationFlags propagationFlags,
			                                              AccessControlType type)
			{
				return new TestAccessRule (identityReference, (TestRights)accessMask, isInherited,
				                           inheritanceFlags, propagationFlags, type);
			}

			public override AuditRule AuditRuleFactory (IdentityReference identityReference,
				                                    int accessMask, bool isInherited,
				                                    InheritanceFlags inheritanceFlags,
			                                            PropagationFlags propagationFlags,
			                                            AuditFlags flags)
			{
				return new TestAuditRule (identityReference, (TestRights)accessMask, isInherited,
				                          inheritanceFlags, propagationFlags, flags);
			}

			protected override bool ModifyAccess (AccessControlModification modification, 
			                                      AccessRule rule, out bool modified)
			{
				modify_access_called = true;
				modified = true; return modified;
			}

			protected override bool ModifyAudit (AccessControlModification modification,
			                                     AuditRule rule, out bool modified)
			{
				modified = false; return modified;
			}

			public override Type AccessRightType {
				get { return typeof (TestRights); }
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


