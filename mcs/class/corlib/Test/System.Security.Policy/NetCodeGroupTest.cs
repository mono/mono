//
// MonoTests.System.Security.Policy.NetCodeGroupTest
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
	public class NetCodeGroupTest : Assertion {

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor_Null () 
		{
			NetCodeGroup cg = new NetCodeGroup ((IMembershipCondition)null);
		}

		[Test]
		public void Constructor () 
		{
			NetCodeGroup cg = new NetCodeGroup (new AllMembershipCondition ());
			AssertNotNull ("MembershipCondition", cg.MembershipCondition);
			AssertNull ("PolicyStatement", cg.PolicyStatement);
			// documented as always null
			AssertNull ("AttributeString", cg.AttributeString);
			// documented as always "Same site Web" but it's "Same site Web." (missing .)
			AssertEquals ("PermissionSetName", "Same site Web.", cg.PermissionSetName);
		}

		[Test]
		public void MergeLogic () 
		{
			NetCodeGroup cg = new NetCodeGroup (new AllMembershipCondition ());
			AssertEquals ("MergeLogic", "Union", cg.MergeLogic);
		}

		[Test]
		public void Copy () 
		{
			NetCodeGroup cg = new NetCodeGroup (new AllMembershipCondition ());
			NetCodeGroup cg2 = (NetCodeGroup) cg.Copy ();
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
			NetCodeGroup cgChild = new NetCodeGroup (new AllMembershipCondition ());
			NetCodeGroup cg = new NetCodeGroup (new AllMembershipCondition ());
			cg.AddChild (cgChild);
			NetCodeGroup cg2 = (NetCodeGroup) cg.Copy ();
			AssertEquals ("Children", cg.Children.Count, cg2.Children.Count);
			AssertEquals ("ToXml", cg.ToXml ().ToString (), cg2.ToXml ().ToString ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Resolve_Null () 
		{
			NetCodeGroup cg = new NetCodeGroup (new AllMembershipCondition ());
			cg.Resolve (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ResolveMatchingCodeGroups_Null () 
		{
			NetCodeGroup cg = new NetCodeGroup (new AllMembershipCondition ());
			cg.ResolveMatchingCodeGroups (null);
		}

		[Test]
		public void ToFromXmlRoundtrip () 
		{
			NetCodeGroup cg = new NetCodeGroup (new AllMembershipCondition ());
			cg.Name = "SomeName";
			cg.Description = "Some Description";
			Assert ("Equals (itself)", cg.Equals (cg));
			SecurityElement se = cg.ToXml ();

			NetCodeGroup cg2 = new NetCodeGroup (new AllMembershipCondition());
			cg2.Name = "SomeOtherName";
			cg2.Description = "Some Other Description";
			Assert ("Equals (another)", !cg.Equals (cg2));

			cg2.FromXml (se);
			Assert ("Equals (FromXml)", cg.Equals (cg2));
		}
	}
}
