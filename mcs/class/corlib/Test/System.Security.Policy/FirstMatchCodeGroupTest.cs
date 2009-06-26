//
// MonoTests.System.Security.Policy.FirstMatchCodeGroupTest
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2004 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using System;
using System.Collections;
using System.Security;
using System.Security.Policy;
using System.Security.Permissions;

namespace MonoTests.System.Security.Policy {

	[TestFixture]
	public class FirstMatchCodeGroupTest  {

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor_MembershipConditionNullPolicyStatement () 
		{
			FirstMatchCodeGroup cg = new FirstMatchCodeGroup (null, new PolicyStatement (new PermissionSet (PermissionState.None)));
		}

		[Test]
		public void Constructor_MembershipConditionPolicyStatementNull () 
		{
			// legal
			FirstMatchCodeGroup cg = new FirstMatchCodeGroup (new AllMembershipCondition (), null);
			Assert.IsNull (cg.PolicyStatement, "PolicyStatement");
		}

		[Test]
		public void Constructor () 
		{
			FirstMatchCodeGroup cg = new FirstMatchCodeGroup (new AllMembershipCondition (), new PolicyStatement (new PermissionSet (PermissionState.None)));
			Assert.IsNotNull (cg.PolicyStatement, "PolicyStatement");
			Assert.IsNotNull (cg.MembershipCondition, "MembershipCondition");
		}

		[Test]
		public void MergeLogic () 
		{
			FirstMatchCodeGroup cg = new FirstMatchCodeGroup (new AllMembershipCondition (), new PolicyStatement (new PermissionSet (PermissionState.None)));
			Assert.AreEqual ("First Match", cg.MergeLogic, "MergeLogic");
		}

		[Test]
		public void Copy () 
		{
			FirstMatchCodeGroup cg = new FirstMatchCodeGroup (new AllMembershipCondition (), new PolicyStatement (new PermissionSet (PermissionState.None)));
			FirstMatchCodeGroup cg2 = (FirstMatchCodeGroup) cg.Copy ();
			Assert.AreEqual (cg.AttributeString, cg2.AttributeString, "AttributeString");
			Assert.AreEqual (cg.Children.Count, cg2.Children.Count, "Children");
			Assert.AreEqual (cg.Description, cg2.Description, "Description");
			Assert.AreEqual (cg.MergeLogic, cg2.MergeLogic, "MergeLogic");
			Assert.AreEqual (cg.Name, cg2.Name, "Name");
			Assert.AreEqual (cg.PermissionSetName, cg2.PermissionSetName, "PermissionSetName");
			Assert.AreEqual (cg.ToXml ().ToString (), cg2.ToXml ().ToString (), "ToXml");
		}

		[Test]
		public void CopyWithChildren () 
		{
			FirstMatchCodeGroup cgChild = new FirstMatchCodeGroup (new AllMembershipCondition (), new PolicyStatement (new PermissionSet (PermissionState.Unrestricted)));
			FirstMatchCodeGroup cg = new FirstMatchCodeGroup (new AllMembershipCondition (), new PolicyStatement (new PermissionSet (PermissionState.None)));
			cg.AddChild (cgChild);
			FirstMatchCodeGroup cg2 = (FirstMatchCodeGroup) cg.Copy ();
			Assert.AreEqual (cg.Children.Count, cg2.Children.Count, "Children");
			Assert.AreEqual (cg.ToXml ().ToString (), cg2.ToXml ().ToString (), "ToXml");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Resolve_Null ()
		{
			FirstMatchCodeGroup cg = new FirstMatchCodeGroup (new AllMembershipCondition (), new PolicyStatement (new PermissionSet (PermissionState.None)));
			cg.Resolve (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ResolveMatchingCodeGroups_Null () 
		{
			FirstMatchCodeGroup cg = new FirstMatchCodeGroup (new AllMembershipCondition (), new PolicyStatement (new PermissionSet (PermissionState.None)));
			cg.ResolveMatchingCodeGroups (null);
		}

		[Test]
		public void ToFromXmlRoundtrip () 
		{
			const string ps_Name = "TestName";
			PolicyStatement ps = new PolicyStatement (new NamedPermissionSet (ps_Name));
			FirstMatchCodeGroup cg = new FirstMatchCodeGroup (new AllMembershipCondition (), ps);
			cg.Name = "SomeName";
			cg.Description = "Some Description";
			Assert.IsTrue (cg.Equals (cg), "Equals (itself)");
			SecurityElement se = cg.ToXml ();

			FirstMatchCodeGroup cg2 = new FirstMatchCodeGroup (new AllMembershipCondition(), ps);
			cg2.Name = "SomeOtherName";
			cg2.Description = "Some Other Description";
			Assert.IsTrue (!cg.Equals (cg2), "Equals (another)");

			cg2.FromXml (se);
			Assert.IsTrue (cg.Equals (cg2), "Equals (FromXml)");
		}
	}
}
