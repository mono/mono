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
	public class FirstMatchCodeGroupTest : Assertion {

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
			AssertNull ("PolicyStatement", cg.PolicyStatement);
		}

		[Test]
		public void Constructor () 
		{
			FirstMatchCodeGroup cg = new FirstMatchCodeGroup (new AllMembershipCondition (), new PolicyStatement (new PermissionSet (PermissionState.None)));
			AssertNotNull ("PolicyStatement", cg.PolicyStatement);
			AssertNotNull ("MembershipCondition", cg.MembershipCondition);
		}

		[Test]
		public void MergeLogic () 
		{
			FirstMatchCodeGroup cg = new FirstMatchCodeGroup (new AllMembershipCondition (), new PolicyStatement (new PermissionSet (PermissionState.None)));
			AssertEquals ("MergeLogic", "First Match", cg.MergeLogic);
		}

		[Test]
		public void Copy () 
		{
			FirstMatchCodeGroup cg = new FirstMatchCodeGroup (new AllMembershipCondition (), new PolicyStatement (new PermissionSet (PermissionState.None)));
			FirstMatchCodeGroup cg2 = (FirstMatchCodeGroup) cg.Copy ();
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
			FirstMatchCodeGroup cgChild = new FirstMatchCodeGroup (new AllMembershipCondition (), new PolicyStatement (new PermissionSet (PermissionState.Unrestricted)));
			FirstMatchCodeGroup cg = new FirstMatchCodeGroup (new AllMembershipCondition (), new PolicyStatement (new PermissionSet (PermissionState.None)));
			cg.AddChild (cgChild);
			FirstMatchCodeGroup cg2 = (FirstMatchCodeGroup) cg.Copy ();
			AssertEquals ("Children", cg.Children.Count, cg2.Children.Count);
			AssertEquals ("ToXml", cg.ToXml ().ToString (), cg2.ToXml ().ToString ());
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
			Assert ("Equals (itself)", cg.Equals (cg));
			SecurityElement se = cg.ToXml ();

			FirstMatchCodeGroup cg2 = new FirstMatchCodeGroup (new AllMembershipCondition(), ps);
			cg2.Name = "SomeOtherName";
			cg2.Description = "Some Other Description";
			Assert ("Equals (another)", !cg.Equals (cg2));

			cg2.FromXml (se);
			Assert ("Equals (FromXml)", cg.Equals (cg2));
		}
	}
}
