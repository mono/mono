//
// MonoTests.System.Security.Policy.CodeGroupTest
//
// Author: Nick Drochak (ndrochak@gol.com)
//
// Copyright (C) 2002  Nick Drochak

using NUnit.Framework;
using System;
using System.Security;
using System.Security.Policy;
using System.Security.Permissions;

namespace MonoTests.System.Security.Policy
{

	public class CodeGroupTest : TestCase 
	{
		
		public CodeGroupTest( string name ): base(name)
		{
		}
		
		public CodeGroupTest() : base("CodeGroupTest")
		{
		}

		public static ITest Suite
		{
			get
			{
				return new TestSuite(typeof(CodeGroupTest));
			}
		}

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

	}  // public class CodeGroupTest 

}  // namespace MonoTests.System.Security.Policy