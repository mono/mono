//
// MonoTests.System.Security.Policy.FileCodeGroupTest
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
	public class FileCodeGroupTest : Assertion {

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor_MembershipConditionNullFileIOPermissionAccess () 
		{
			FileCodeGroup cg = new FileCodeGroup (null, FileIOPermissionAccess.AllAccess);
		}

		[Test]
		public void Constructor_AllAccess () 
		{
			FileCodeGroup cg = new FileCodeGroup (new AllMembershipCondition (), FileIOPermissionAccess.AllAccess);
			AssertNotNull ("MembershipCondition", cg.MembershipCondition);
			AssertNull ("PolicyStatement", cg.PolicyStatement);
			// documented as always null
			AssertNull ("AttributeString", cg.AttributeString);
		}

		[Test]
		public void Constructor_Append () 
		{
			FileCodeGroup cg = new FileCodeGroup (new AllMembershipCondition (), FileIOPermissionAccess.Append);
			AssertNotNull ("MembershipCondition", cg.MembershipCondition);
			AssertNull ("PolicyStatement", cg.PolicyStatement);
			// documented as always null
			AssertNull ("AttributeString", cg.AttributeString);
		}

		[Test]
		public void Constructor_NoAccess () 
		{
			FileCodeGroup cg = new FileCodeGroup (new AllMembershipCondition (), FileIOPermissionAccess.NoAccess);
			AssertNotNull ("MembershipCondition", cg.MembershipCondition);
			AssertNull ("PolicyStatement", cg.PolicyStatement);
			// documented as always null
			AssertNull ("AttributeString", cg.AttributeString);
		}

		[Test]
		public void Constructor_PathDiscovery () 
		{
			FileCodeGroup cg = new FileCodeGroup (new AllMembershipCondition (), FileIOPermissionAccess.PathDiscovery);
			AssertNotNull ("MembershipCondition", cg.MembershipCondition);
			AssertNull ("PolicyStatement", cg.PolicyStatement);
			// documented as always null
			AssertNull ("AttributeString", cg.AttributeString);
		}

		[Test]
		public void Constructor_Read () 
		{
			FileCodeGroup cg = new FileCodeGroup (new AllMembershipCondition (), FileIOPermissionAccess.Read);
			AssertNotNull ("MembershipCondition", cg.MembershipCondition);
			AssertNull ("PolicyStatement", cg.PolicyStatement);
			// documented as always null
			AssertNull ("AttributeString", cg.AttributeString);
		}

		[Test]
		public void Constructor_Write () 
		{
			FileCodeGroup cg = new FileCodeGroup (new AllMembershipCondition (), FileIOPermissionAccess.Write);
			AssertNotNull ("MembershipCondition", cg.MembershipCondition);
			AssertNull ("PolicyStatement", cg.PolicyStatement);
			// documented as always null
			AssertNull ("AttributeString", cg.AttributeString);
		}

		[Test]
		public void MergeLogic () 
		{
			FileCodeGroup cg = new FileCodeGroup (new AllMembershipCondition (), FileIOPermissionAccess.AllAccess);
			AssertEquals ("MergeLogic", "Union", cg.MergeLogic);
		}

		[Test]
		public void Copy () 
		{
			FileCodeGroup cg = new FileCodeGroup (new AllMembershipCondition (), FileIOPermissionAccess.AllAccess);
			FileCodeGroup cg2 = (FileCodeGroup) cg.Copy ();
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
			FileCodeGroup cgChild = new FileCodeGroup (new AllMembershipCondition (), FileIOPermissionAccess.AllAccess);
			FileCodeGroup cg = new FileCodeGroup (new AllMembershipCondition (), FileIOPermissionAccess.AllAccess);
			cg.AddChild (cgChild);
			FileCodeGroup cg2 = (FileCodeGroup) cg.Copy ();
			AssertEquals ("Children", cg.Children.Count, cg2.Children.Count);
			AssertEquals ("ToXml", cg.ToXml ().ToString (), cg2.ToXml ().ToString ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Resolve_Null () 
		{
			FileCodeGroup cg = new FileCodeGroup (new AllMembershipCondition (), FileIOPermissionAccess.AllAccess);
			cg.Resolve (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ResolveMatchingCodeGroups_Null () 
		{
			FileCodeGroup cg = new FileCodeGroup (new AllMembershipCondition (), FileIOPermissionAccess.AllAccess);
			cg.ResolveMatchingCodeGroups (null);
		}

		[Test]
		public void ToXml () 
		{
			FileIOPermissionAccess access = FileIOPermissionAccess.Read | FileIOPermissionAccess.Write;
			FileCodeGroup cg = new FileCodeGroup (new AllMembershipCondition (), access);
			string s = cg.ToXml ().ToString ();
			Assert ("Access='Read, Write'", s.IndexOf ("Access=\"Read, Write\"") > 0);
		}

		[Test]
		public void ToFromXmlRoundtrip () 
		{
			FileCodeGroup cg = new FileCodeGroup (new AllMembershipCondition (), FileIOPermissionAccess.AllAccess);
			cg.Name = "SomeName";
			cg.Description = "Some Description";
			Assert ("Equals (itself)", cg.Equals (cg));
			SecurityElement se = cg.ToXml ();

			FileCodeGroup cg2 = new FileCodeGroup (new AllMembershipCondition (), FileIOPermissionAccess.NoAccess);
			cg2.Name = "SomeOtherName";
			cg2.Description = "Some Other Description";
			Assert ("Equals (another)", !cg.Equals (cg2));

			cg2.FromXml (se);
			Assert ("Equals (FromXml)", cg.Equals (cg2));
		}
	}
}
