//
// MonoTests.System.Security.Policy.CodeGroupTest
//
// Author(s):
//   Nick Drochak (ndrochak@gol.com)
//
// (C) 2001 Nick Drochak, All rights reserved.

using NUnit.Framework;
using System;
using System.Security;
using System.Security.Policy;
using System.Security.Permissions;

namespace MonoTests.System.Security.Policy
{

	public class CodeGroupTest : TestCase 
	{
		
		protected override void SetUp()
		{
		}

		public class MyCodeGroup : CodeGroup
		{
			public MyCodeGroup(IMembershipCondition membershipCondition,
				PolicyStatement policy) :base(membershipCondition, policy)
			{
			}

			public override CodeGroup Copy()
			{
				return this;
			}

			public override string MergeLogic
			{
				get
				{
					return "";
				}
			}

			public override PolicyStatement Resolve(	Evidence evidence)
			{
				return (PolicyStatement)null;
			}

			public override CodeGroup ResolveMatchingCodeGroups(Evidence evidence)
			{
				return this;
			}
		}

		public void TestConstructorExceptions()
		{
			MyCodeGroup cg;
			try
			{
				cg = new MyCodeGroup(null, null);
				Fail("Constructor should throw exception on null paramters");
			}
			catch(Exception e)
			{
				Assert("Should have caught an ArgumentNull Exception", e is ArgumentNullException);
			}
		}

		public void TestConstructor()
		{
			MyCodeGroup cg = null;
			try
			{
				cg = new MyCodeGroup(new AllMembershipCondition(), new PolicyStatement(new PermissionSet(PermissionState.None)));
				Assert("PolicyStatement property not set correctly by constructor.", cg.PolicyStatement != null);
				Assert("MembershipCondition property not set correctly by constructor.", cg.MembershipCondition != null);
			}
			catch(Exception e)
			{
				Fail("Constructor failed. Exception caught was: " + e.ToString());
			}
		}

		public void TestDescriptionProperty()
		{
			MyCodeGroup cg = null;
			const string description = "Test Description";
			cg = new MyCodeGroup(new AllMembershipCondition(), new PolicyStatement(new PermissionSet(PermissionState.None)));
			cg.Description = description;
			Assert("Description not the expected value", cg.Description == description);
		}

		public void TestNameProperty()
		{
			MyCodeGroup cg = null;
			const string name = "Test Name";
			cg = new MyCodeGroup(new AllMembershipCondition(), new PolicyStatement(new PermissionSet(PermissionState.None)));
			cg.Name = name;
			Assert("Description not the expected value", cg.Name == name);
		}

		public void TestChildren()
		{
			MyCodeGroup cg = null;
			cg = new MyCodeGroup(new AllMembershipCondition(), new PolicyStatement(new PermissionSet(PermissionState.None)));
			cg.AddChild(new MyCodeGroup(new AllMembershipCondition(), new PolicyStatement(new PermissionSet(PermissionState.Unrestricted))));
			Assert("Unexpected number of children", cg.Children.Count == 1);
		}

		public void TestAttributeStringProperty()
		{
			MyCodeGroup cg = null;
			PolicyStatementAttribute psa = PolicyStatementAttribute.LevelFinal;
			PolicyStatement ps = new PolicyStatement(new PermissionSet(PermissionState.None));
			ps.Attributes = psa;
			cg = new MyCodeGroup(new AllMembershipCondition(), ps);
			AssertEquals("AttributeString", psa.ToString(), cg.AttributeString);
		}

		public void TestPermissionSetNameProperty()
		{
			MyCodeGroup cg = null;
			const string ps_Name = "TestName";
			PolicyStatement ps = new PolicyStatement(new NamedPermissionSet(ps_Name));
			cg = new MyCodeGroup(new AllMembershipCondition(), ps);
			AssertEquals("AttributeString", ps_Name, cg.PermissionSetName);
		}

		public void TestEquals()
		{
			MyCodeGroup cg = null;
			MyCodeGroup cg2 = null;
			const string ps_Name = "TestName";
			PolicyStatement ps = new PolicyStatement(new NamedPermissionSet(ps_Name));
			cg = new MyCodeGroup(new AllMembershipCondition(), ps);
			cg.Name = "SomeName";
			cg.Description = "Some Description";
			bool isEquals;
			isEquals = cg.Equals(cg);
			
			isEquals = cg.Equals("Not Equal to this");
			Assert("CodeGroup should not be equal to a non-CodeGroup type", !isEquals);

			cg2 = new MyCodeGroup(new AllMembershipCondition(), ps);
			cg2.Name = "SomeOtherName";
			cg2.Description = "Some Other Description";

			isEquals = cg.Equals(cg2);
			Assert("CodeGroup should not be equal when Name or Description is different", !isEquals);

			cg2 = new MyCodeGroup(new ApplicationDirectoryMembershipCondition(), ps);
			cg2.Name = cg.Name;
			cg2.Description = cg.Description;
			isEquals = cg.Equals(cg2);
			Assert("CodeGroup should not be equal when Membership Condition is different", !isEquals);
		}

		public void TestEqualsWithChildren()
		{
			MyCodeGroup cg = null;
			MyCodeGroup cg2 = null;
			MyCodeGroup cgChild = null;
			const string ps_Name = "TestName";
			bool isEquals;

			PolicyStatement ps = new PolicyStatement(new NamedPermissionSet(ps_Name));
			cg = new MyCodeGroup(new AllMembershipCondition(), ps);
			cg.Name = "SomeName";
			cg.Description = "Some Description";

			cgChild = new MyCodeGroup(new ApplicationDirectoryMembershipCondition(), ps);
			cgChild.Name = "ChildName";
			cgChild.Description = "Child Descripiton";

			cg.AddChild(cgChild);

			cg2 = new MyCodeGroup(cg.MembershipCondition, cg.PolicyStatement);
			cg2.Name = cg.Name;
			cg2.Description = cg.Description;

			isEquals = cg.Equals(cg2);
			Assert("Should be equal when Children are ignored", isEquals);

			isEquals = cg.Equals(cg2, true);
			Assert("Should not be equal when Child count is different", !isEquals);

			cg2.AddChild(cgChild);
			isEquals = cg2.Equals(cg, true);
			Assert("Should be equal when children are equal", isEquals);
		}

	
		public void TestRemoveChild()
		{
			MyCodeGroup cg = null;
			MyCodeGroup cgChild = null;
			MyCodeGroup cgChild2 = null;
			const string ps_Name = "TestName";

			PolicyStatement ps = new PolicyStatement(new NamedPermissionSet(ps_Name));
			cg = new MyCodeGroup(new AllMembershipCondition(), ps);
			cg.Name = "SomeName";
			cg.Description = "Some Description";

			cgChild = new MyCodeGroup(new ApplicationDirectoryMembershipCondition(), ps);
			cgChild.Name = "ChildName";
			cgChild.Description = "Child Descripiton";

			cg.AddChild(cgChild);

			cgChild2 = new MyCodeGroup(new ApplicationDirectoryMembershipCondition(), ps);
			cgChild2.Name = "ChildName2";
			cgChild2.Description = "Child Descripiton 2";

			cg.AddChild(cgChild2);

			AssertEquals("Should be two children before the call to Remove()", 2, cg.Children.Count);

			cg.RemoveChild(cgChild);

			AssertEquals("Remaing child does not have correct name", "ChildName2", ((CodeGroup)cg.Children[0]).Name);
			try
			{
				cg.RemoveChild(cgChild);
				Fail("Should have throw error on trying to remove non-existant child");
			}
			catch{}
		}
	}  // public class CodeGroupTest 

}  // namespace MonoTests.System.Security.Policy