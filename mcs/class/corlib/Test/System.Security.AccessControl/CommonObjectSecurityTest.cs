// CommonObjectSecurityTest.cs - NUnit Test Cases for CommonObjectSecurity
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
	public class CommonObjectSecurityTest
	{
		[Test]
		public void Defaults ()
		{
			TestSecurity security;

			security = new TestSecurity (false);
			Assert.IsFalse (security.IsContainerTest);
			Assert.IsFalse (security.IsDSTest);

			security = new TestSecurity (true);
			Assert.IsTrue (security.IsContainerTest);
			Assert.IsFalse (security.IsDSTest);
		}

		[Test]
		public void AddAndGetAccessRulesWorkAndMergeCorrectly ()
		{
			var security = new TestSecurity (false);

			// CommonObjectSecurity does not appear to care at all about types on MS.NET.
			// It just uses AccessMask, and then GetAccessRules uses the factory methods.
			// So, the whole API is a mess of strong typing and repeated code backed by nothing.
			Assert.IsFalse (security.modify_access_called);

			SecurityIdentifier sid = new SecurityIdentifier (WellKnownSidType.WorldSid, null);
			security.AddAccessRuleTest (new TestAccessRule<int> (sid, 2, AccessControlType.Allow));
			security.AddAccessRuleTest (new TestAccessRule<TestRights> (sid, TestRights.One, AccessControlType.Allow));
			security.AddAccessRuleTest (new TestAccessRule<int> (sid, 4, AccessControlType.Allow));

			Assert.IsTrue (security.modify_access_called);
			Assert.IsFalse (security.modify_access_rule_called);
			Assert.IsFalse (security.modify_audit_called);

			Assert.IsFalse (security.access_rule_factory_called);
			AuthorizationRuleCollection rules1 = security.GetAccessRules (false, true, typeof (SecurityIdentifier));
			Assert.IsFalse (security.access_rule_factory_called);
			Assert.AreEqual (0, rules1.Count);

			Assert.IsFalse (security.access_rule_factory_called);
			AuthorizationRuleCollection rules2 = security.GetAccessRules (true, true, typeof (SecurityIdentifier));
			Assert.IsTrue (security.access_rule_factory_called);
			Assert.AreEqual (1, rules2.Count);

			Assert.IsInstanceOfType (typeof (TestAccessRule<TestRights>), rules2[0]);
			TestAccessRule<TestRights> rule = (TestAccessRule<TestRights>)rules2[0];
			Assert.AreEqual ((TestRights)7, rule.Rights);
		}

		[Test]
		public void AddAndPurgeWorks ()
		{
			TestSecurity security = new TestSecurity (false);
 
			NTAccount nta1 = new NTAccount(@"BUILTIN\Users");
			NTAccount nta2 = new NTAccount(@"BUILTIN\Administrators");
			security.AddAccessRuleTest (new TestAccessRule<TestRights> (nta1, TestRights.One,
			                                                            AccessControlType.Allow));
			security.AddAccessRuleTest (new TestAccessRule<TestRights> (nta2, TestRights.One,
			                                                            AccessControlType.Allow));

			AuthorizationRuleCollection rules1 = security.GetAccessRules (true, true, typeof (NTAccount));
			Assert.AreEqual (2, rules1.Count);

			security.PurgeAccessRules (nta1);
			AuthorizationRuleCollection rules2 = security.GetAccessRules (true, true, typeof (NTAccount));
			Assert.AreEqual (1, rules2.Count);
			Assert.IsInstanceOfType (typeof (TestAccessRule<TestRights>), rules2[0]);
			TestAccessRule<TestRights> rule = (TestAccessRule<TestRights>)rules2[0];
			Assert.AreEqual (nta2, rule.IdentityReference);
		}

		[Test]
		public void ResetAccessRuleCausesExactlyOneModifyAccessCall ()
		{
			TestSecurity security = new TestSecurity (false);
			SecurityIdentifier sid = new SecurityIdentifier ("WD");
			security.ResetAccessRuleTest (new TestAccessRule<TestRights> (sid, TestRights.One,
			                                                              AccessControlType.Allow));
			Assert.AreEqual (1, security.modify_access_called_count);
		}

		class TestAccessRule<T> : AccessRule
		{
			public TestAccessRule (IdentityReference identity, T rules,
			                       AccessControlType type)
				: this (identity, rules, InheritanceFlags.None, PropagationFlags.None, type)
			{
			}

			public TestAccessRule (IdentityReference identity, T rules,
			                       InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags,
			                       AccessControlType type)
				: base (identity, (int)(object)rules, false, inheritanceFlags, propagationFlags, type)
			{
			}

			public T Rights {
				get { return (T)(object)AccessMask; }
			}
		}

		class TestAuditRule<T> : AuditRule
		{
			public TestAuditRule (IdentityReference identity, T rules,
			                      InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags,
			                      AuditFlags auditFlags)
				: base (identity, (int)(object)rules, false, inheritanceFlags, propagationFlags, auditFlags)
			{
			}
		}

		enum TestRights
		{
			One = 1
		}

		class TestSecurity : CommonObjectSecurity
		{
			public bool access_rule_factory_called;
			public bool audit_rule_factory_called;
			public bool modify_access_called;
			public int modify_access_called_count;
			public bool modify_access_rule_called;
			public bool modify_audit_called;
			public bool modify_audit_rule_called;

			public TestSecurity (bool isContainer)
				: base (isContainer)
			{
			}

			public bool IsContainerTest {
				get { return IsContainer; }
			}

			public bool IsDSTest {
				get { return IsDS; }
			}

			public void AddAccessRuleTest (AccessRule rule)
			{
				AddAccessRule (rule);
			}

			public void AddAuditRuleTest (AuditRule rule)
			{
				AddAuditRule (rule);
			}

			public bool RemoveAccessRuleTest (AccessRule rule)
			{
				return RemoveAccessRule (rule);
			}

			public void RemoveAccessRuleAllTest (AccessRule rule)
			{
				RemoveAccessRuleAll (rule);
			}

			public void RemoveAccessRuleSpecificTest (AccessRule rule)
			{
				RemoveAccessRuleSpecific (rule);
			}

			public void ResetAccessRuleTest (AccessRule rule)
			{
				ResetAccessRule (rule);
			}

			public override AccessRule AccessRuleFactory (IdentityReference identityReference,
			                                              int accessMask, bool isInherited,
			                                              InheritanceFlags inheritanceFlags,
			                                              PropagationFlags propagationFlags,
			                                              AccessControlType type)
			{
				access_rule_factory_called = true;
				return new TestAccessRule<TestRights> (identityReference, (TestRights)accessMask,
								       inheritanceFlags, propagationFlags, type);
			}

			public override AuditRule AuditRuleFactory (IdentityReference identityReference,
				                                    int accessMask, bool isInherited,
				                                    InheritanceFlags inheritanceFlags,
			                                            PropagationFlags propagationFlags,
			                                            AuditFlags flags)
			{
				audit_rule_factory_called = true;
				return new TestAuditRule<TestRights> (identityReference, (TestRights)accessMask,
				                                      inheritanceFlags, propagationFlags, flags);
			}

			public override bool ModifyAccessRule (AccessControlModification modification,
			                                       AccessRule rule, out bool modified)
			{
				modify_access_rule_called = true;
				return base.ModifyAccessRule (modification, rule, out modified);
			}

			protected override bool ModifyAccess (AccessControlModification modification, 
			                                      AccessRule rule, out bool modified)
			{
				modify_access_called = true;
				modify_access_called_count ++;
				return base.ModifyAccess (modification, rule, out modified);
			}

			public override bool ModifyAuditRule (AccessControlModification modification,
			                                      AuditRule rule, out bool modified)
			{
				modify_audit_rule_called = true;
				return base.ModifyAuditRule (modification, rule, out modified);
			}

			protected override bool ModifyAudit (AccessControlModification modification,
			                                     AuditRule rule, out bool modified)
			{
				modify_audit_called = true;
				return base.ModifyAudit (modification, rule, out modified);
			}

			public override Type AccessRightType {
				get { return typeof (TestRights); }
			}

			public override Type AccessRuleType {
				get { return typeof (TestAccessRule<TestRights>); }
			}

			public override Type AuditRuleType {
				get { return typeof (TestAuditRule<TestRights>); }
			}
		}
	}
}

