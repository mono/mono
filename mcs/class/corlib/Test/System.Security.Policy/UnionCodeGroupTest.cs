//
// MonoTests.System.Security.Policy.UnionCodeGroupTest
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
	public class UnionCodeGroupTest : Assertion {

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor_MembershipConditionNullPolicyStatement () 
		{
			UnionCodeGroup cg = new UnionCodeGroup (null, new PolicyStatement (new PermissionSet (PermissionState.None)));
		}

		[Test]
		public void Constructor_MembershipConditionPolicyStatementNull () 
		{
			// legal
			UnionCodeGroup cg = new UnionCodeGroup (new AllMembershipCondition (), null);
			AssertNull ("PolicyStatement", cg.PolicyStatement);
		}

		[Test]
		public void Constructor () 
		{
			UnionCodeGroup cg = new UnionCodeGroup (new AllMembershipCondition (), new PolicyStatement (new PermissionSet (PermissionState.None)));
			AssertNotNull ("PolicyStatement", cg.PolicyStatement);
			AssertNotNull ("MembershipCondition", cg.MembershipCondition);
		}

		[Test]
		public void MergeLogic () 
		{
			UnionCodeGroup cg = new UnionCodeGroup (new AllMembershipCondition (), new PolicyStatement (new PermissionSet (PermissionState.None)));
			AssertEquals ("MergeLogic", "Union", cg.MergeLogic);
		}

		[Test]
		public void Copy () 
		{
			UnionCodeGroup cg = new UnionCodeGroup (new AllMembershipCondition (), new PolicyStatement (new PermissionSet (PermissionState.None)));
			UnionCodeGroup cg2 = (UnionCodeGroup) cg.Copy ();
			AssertEquals ("AttributeString", cg.AttributeString, cg2.AttributeString);
			AssertEquals ("Children", cg.Children.Count, cg2.Children.Count);
			AssertEquals ("Description", cg.Description, cg2.Description);
			AssertEquals ("MergeLogic", cg.MergeLogic, cg2.MergeLogic);
			AssertEquals ("Name", cg.Name, cg2.Name);
			AssertEquals ("PermissionSetName", cg.PermissionSetName, cg2.PermissionSetName);
			AssertEquals ("ToXml", cg.ToXml ().ToString (), cg2.ToXml ().ToString ());
		}

		[Test]
		public void CopyWithChildren () 
		{
			UnionCodeGroup cgChild = new UnionCodeGroup (new AllMembershipCondition (), new PolicyStatement (new PermissionSet (PermissionState.Unrestricted)));
			UnionCodeGroup cg = new UnionCodeGroup (new AllMembershipCondition (), new PolicyStatement (new PermissionSet (PermissionState.None)));
			cg.AddChild (cgChild);
			UnionCodeGroup cg2 = (UnionCodeGroup) cg.Copy ();
			AssertEquals ("Children", cg.Children.Count, cg2.Children.Count);
			AssertEquals ("ToXml", cg.ToXml ().ToString (), cg2.ToXml ().ToString ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Resolve_Null () 
		{
			UnionCodeGroup cg = new UnionCodeGroup (new AllMembershipCondition (), new PolicyStatement (new PermissionSet (PermissionState.None)));
			cg.Resolve (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ResolveMatchingCodeGroups_Null () 
		{
			UnionCodeGroup cg = new UnionCodeGroup (new AllMembershipCondition (), new PolicyStatement (new PermissionSet (PermissionState.None)));
			cg.ResolveMatchingCodeGroups (null);
		}

		[Test]
		public void ToFromXmlRoundtrip () 
		{
			const string ps_Name = "TestName";
			PolicyStatement ps = new PolicyStatement (new NamedPermissionSet (ps_Name));
			UnionCodeGroup cg = new UnionCodeGroup (new AllMembershipCondition (), ps);
			cg.Name = "SomeName";
			cg.Description = "Some Description";
			Assert ("Equals (itself)", cg.Equals (cg));
			SecurityElement se = cg.ToXml ();

			UnionCodeGroup cg2 = new UnionCodeGroup (new AllMembershipCondition(), ps);
			cg2.Name = "SomeOtherName";
			cg2.Description = "Some Other Description";
			Assert ("Equals (another)", !cg.Equals (cg2));

			cg2.FromXml (se);
			Assert ("Equals (FromXml)", cg.Equals (cg2));
		}
	}
}
