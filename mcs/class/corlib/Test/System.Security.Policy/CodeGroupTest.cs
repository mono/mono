//
// MonoTests.System.Security.Policy.CodeGroupTest
//
// Authors:
//	Nick Drochak (ndrochak@gol.com)
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2001 Nick Drochak, All rights reserved.
// Portions (C) 2004 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using System;
using System.Collections;
using System.Security;
using System.Security.Policy;
using System.Security.Permissions;

namespace MonoTests.System.Security.Policy {

	public class MyCodeGroup : CodeGroup {

		public MyCodeGroup (IMembershipCondition membershipCondition, PolicyStatement policy) 
			: base (membershipCondition, policy) {}

		public override CodeGroup Copy () 
		{
			return this;
		}

		public override string MergeLogic {
			get { return ""; }
		}

		public override PolicyStatement Resolve (Evidence evidence) 
		{
			return null;
		}

		public override CodeGroup ResolveMatchingCodeGroups (Evidence evidence) 
		{
			return this;
		}
	}

	// this version has a constructor with no parameters
	public class MySecondCodeGroup : CodeGroup {

		// must be public (else the ToFromXmlRoundtrip_WithChildren_Second will fail)
		public MySecondCodeGroup () : base (new AllMembershipCondition (), null) {}

		public MySecondCodeGroup (IMembershipCondition membershipCondition, PolicyStatement policy) 
			: base (membershipCondition, policy) {}

		public override CodeGroup Copy () {
			return this;
		}

		public override string MergeLogic {
			get { return ""; }
		}

		public override PolicyStatement Resolve (Evidence evidence) {
			return null;
		}

		public override CodeGroup ResolveMatchingCodeGroups (Evidence evidence) {
			return this;
		}
	}

	[TestFixture]
	public class CodeGroupTest : Assertion {

		private const string ps_Name = "TestName";
		
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor_MembershipConditionNullPolicyStatement ()
		{
			MyCodeGroup cg = new MyCodeGroup (null, new PolicyStatement(new PermissionSet(PermissionState.None)));
		}

		[Test]
		public void Constructor_MembershipConditionPolicyStatementNull () 
		{
			// legal
			MyCodeGroup cg = new MyCodeGroup (new AllMembershipCondition(), null);
			AssertNull ("PolicyStatement", cg.PolicyStatement);
		}

		[Test]
		public void Constructor ()
		{
			MyCodeGroup cg = new MyCodeGroup (new AllMembershipCondition(), new PolicyStatement(new PermissionSet(PermissionState.None)));
			AssertNotNull ("PolicyStatement property not set correctly by constructor.", cg.PolicyStatement);
			AssertNotNull ("MembershipCondition property not set correctly by constructor.", cg.MembershipCondition);
		}

		[Test]
		public void Description ()
		{
			const string description = "Test Description";
			MyCodeGroup cg = new MyCodeGroup (new AllMembershipCondition(), new PolicyStatement(new PermissionSet(PermissionState.None)));
			cg.Description = description;
			AssertEquals ("Description not the expected value", description, cg.Description);
		}

		[Test]
		public void Name ()
		{
			const string name = "Test Name";
			MyCodeGroup cg = new MyCodeGroup (new AllMembershipCondition(), new PolicyStatement(new PermissionSet(PermissionState.None)));
			cg.Name = name;
			AssertEquals ("Description not the expected value", name, cg.Name);
		}

		[Test]
		public void AddChild ()
		{
			MyCodeGroup cg = new MyCodeGroup(new AllMembershipCondition (), new PolicyStatement(new PermissionSet (PermissionState.None)));
			AssertEquals ("Unexpected number of children (before add)", 0, cg.Children.Count);
			cg.AddChild (new MyCodeGroup (new AllMembershipCondition (), new PolicyStatement(new PermissionSet (PermissionState.Unrestricted))));
			AssertEquals ("Unexpected number of children (after add)", 1, cg.Children.Count);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddChild_Null () 
		{
			MyCodeGroup cg = new MyCodeGroup (new AllMembershipCondition (), new PolicyStatement(new PermissionSet (PermissionState.None)));
			cg.AddChild (null);
		}

		[Test]
		public void AttributeString ()
		{
			PolicyStatementAttribute psa = PolicyStatementAttribute.LevelFinal;
			PolicyStatement ps = new PolicyStatement (new PermissionSet (PermissionState.None));
			ps.Attributes = psa;
			MyCodeGroup cg = new MyCodeGroup(new AllMembershipCondition (), ps);
			AssertEquals ("AttributeString", psa.ToString(), cg.AttributeString);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Children_Null () 
		{
			MyCodeGroup cg = new MyCodeGroup (new AllMembershipCondition (), new PolicyStatement(new PermissionSet (PermissionState.None)));
			cg.Children = null;
		}

		[Test]
		public void PermissionSetName ()
		{
			PolicyStatement ps = new PolicyStatement(new NamedPermissionSet (ps_Name));
			MyCodeGroup cg = new MyCodeGroup (new AllMembershipCondition (), ps);
			AssertEquals ("PermissionSetName", ps_Name, cg.PermissionSetName);
		}

		[Test]
		public void Equals ()
		{
			PolicyStatement ps = new PolicyStatement (new NamedPermissionSet (ps_Name));
			MyCodeGroup cg = new MyCodeGroup (new AllMembershipCondition (), ps);
			cg.Name = "SomeName";
			cg.Description = "Some Description";
			Assert ("Equals (itself)", cg.Equals (cg));
			Assert ("Equals (string)", !cg.Equals ("Not Equal to this"));

			MyCodeGroup cg2 = new MyCodeGroup(new AllMembershipCondition(), ps);
			cg2.Name = "SomeOtherName";
			cg2.Description = "Some Other Description";
			Assert ("Equals (another)", !cg.Equals (cg2));

			cg2 = new MyCodeGroup (new ApplicationDirectoryMembershipCondition(), ps);
			cg2.Name = cg.Name;
			cg2.Description = cg.Description;
			Assert ("Equals (different Membership Condition)", !cg.Equals (cg2));
		}

		[Test]
		public void EqualsWithChildren ()
		{
			PolicyStatement ps = new PolicyStatement (new NamedPermissionSet (ps_Name));
			
			MyCodeGroup cgChild = new MyCodeGroup(new ApplicationDirectoryMembershipCondition(), ps);
			cgChild.Name = "ChildName";
			cgChild.Description = "Child Descripiton";

			MyCodeGroup cg = new MyCodeGroup (new AllMembershipCondition (), ps);
			cg.Name = "SomeName";
			cg.Description = "Some Description";
			cg.AddChild (cgChild);

			MyCodeGroup cg2 = new MyCodeGroup (cg.MembershipCondition, cg.PolicyStatement);
			cg2.Name = cg.Name;
			cg2.Description = cg.Description;

			Assert ("Should be equal when Children are ignored", cg.Equals (cg2));
			Assert ("Should not be equal when Child count is different", !cg.Equals(cg2, true));

			cg2.AddChild(cgChild);
			Assert ("Should be equal when children are equal", cg2.Equals(cg, true));
		}

		[Test]
		public void RemoveChild ()
		{
			PolicyStatement ps = new PolicyStatement (new NamedPermissionSet (ps_Name));
			MyCodeGroup cg = new MyCodeGroup (new AllMembershipCondition (), ps);
			cg.Name = "SomeName";
			cg.Description = "Some Description";

			MyCodeGroup cgChild = new MyCodeGroup (new ApplicationDirectoryMembershipCondition (), ps);
			cgChild.Name = "ChildName";
			cgChild.Description = "Child Descripiton";
			cg.AddChild (cgChild);

			MyCodeGroup cgChild2 = new MyCodeGroup (new ApplicationDirectoryMembershipCondition (), ps);
			cgChild2.Name = "ChildName2";
			cgChild2.Description = "Child Descripiton 2";
			cg.AddChild (cgChild2);

			AssertEquals ("Should be two children before the call to Remove()", 2, cg.Children.Count);
			cg.RemoveChild(cgChild);
			AssertEquals ("Should be one children after the call to Remove()", 1, cg.Children.Count);
			AssertEquals("Remaining child does not have correct name", "ChildName2", ((CodeGroup)cg.Children[0]).Name);
		}

		[Test]
		public void RemoveChild_NonExistant () 
		{
			PolicyStatement ps = new PolicyStatement (new NamedPermissionSet (ps_Name));

			MyCodeGroup cgChild = new MyCodeGroup (new ApplicationDirectoryMembershipCondition (), ps);
			cgChild.Name = "ChildName";
			cgChild.Description = "Child Descripiton";

			MyCodeGroup cg = new MyCodeGroup (new AllMembershipCondition (), ps);
			cg.AddChild (cgChild);
			cg.RemoveChild (cgChild);
			cg.RemoveChild (cgChild);
			// no exception
		}

		[Test]
		public void RemoveChild_Null () 
		{
			MyCodeGroup cg = new MyCodeGroup (new AllMembershipCondition (), new PolicyStatement(new PermissionSet (PermissionState.None)));
			cg.RemoveChild (null);
			// no exception
		}

		[Test]
		public void ToXml () 
		{
			MyCodeGroup cg = new MyCodeGroup (new AllMembershipCondition (), new PolicyStatement(new PermissionSet (PermissionState.None)));
			SecurityElement se = cg.ToXml ();
			string s = se.ToString ();
			Assert ("ToXml-Starts", s.StartsWith ("<CodeGroup class=\"MonoTests.System.Security.Policy.MyCodeGroup,"));
			Assert ("ToXml-Ends", s.EndsWith ("version=\"1\"/>" + Environment.NewLine + "</CodeGroup>" + Environment.NewLine));

			cg.AddChild (new MyCodeGroup (new AllMembershipCondition (), new PolicyStatement(new PermissionSet (PermissionState.Unrestricted))));
			se = cg.ToXml ();
			s = se.ToString ();
			Assert ("ToXml-Child", s.IndexOf ("<CodeGroup class=\"MonoTests.System.Security.Policy.MyCodeGroup,", 1) > 0);
		}

		[Test]
		public void ToFromXmlRoundtrip () 
		{
			PolicyStatement ps = new PolicyStatement (new NamedPermissionSet (ps_Name));
			MyCodeGroup cg = new MyCodeGroup (new AllMembershipCondition (), ps);
			cg.Name = "SomeName";
			cg.Description = "Some Description";
			Assert ("Equals (itself)", cg.Equals (cg));
			SecurityElement se = cg.ToXml ();

			MyCodeGroup cg2 = new MyCodeGroup (new AllMembershipCondition(), ps);
			cg2.Name = "SomeOtherName";
			cg2.Description = "Some Other Description";
			Assert ("Equals (another)", !cg.Equals (cg2));

			cg2.FromXml (se);
			Assert ("Equals (FromXml)", cg.Equals (cg2));
		}

		[Test]
		[ExpectedException (typeof (MissingMethodException))]
		public void ToFromXmlRoundtrip_WithChildren () 
		{
			PolicyStatement ps = new PolicyStatement (new NamedPermissionSet (ps_Name));

			MyCodeGroup cgChild = new MyCodeGroup (new ApplicationDirectoryMembershipCondition (), ps);
			cgChild.Name = "ChildName";
			cgChild.Description = "Child Descripiton";

			MyCodeGroup cg = new MyCodeGroup (new AllMembershipCondition (), ps);
			cg.Name = "SomeName";
			cg.Description = "Some Description";
			cg.AddChild (cgChild);
			cg.AddChild (cgChild);
			Assert ("Equals (itself)", cg.Equals (cg));
			SecurityElement se = cg.ToXml ();

			MyCodeGroup cg2 = (MyCodeGroup) cg.Copy ();
			cg2.FromXml (se);
			// MissingMethodException down here (stangely not up here ?!? delayed ?)
			Assert ("Equals (FromXml)", cg.Equals (cg2, true));
		}

		[Test]
		public void ToFromXmlRoundtrip_WithChildren_Second () 
		{
			PolicyStatement ps = new PolicyStatement (new NamedPermissionSet (ps_Name));

			// only the child is MySecondCodeGroup
			MySecondCodeGroup cgChild = new MySecondCodeGroup (new ApplicationDirectoryMembershipCondition (), ps);
			cgChild.Name = "ChildName";
			cgChild.Description = "Child Descripiton";

			MyCodeGroup cg = new MyCodeGroup (new AllMembershipCondition (), ps);
			cg.Name = "SomeName";
			cg.Description = "Some Description";
			cg.AddChild (cgChild);
			Assert ("Equals (itself)", cg.Equals (cg));
			SecurityElement se = cg.ToXml ();

			MyCodeGroup cg2 = (MyCodeGroup) cg.Copy ();
			cg2.FromXml (se);
			Assert ("Equals (FromXml)", cg.Equals (cg2, true));
		}

		[Test]
		public void FromXml_Bad () 
		{
			MyCodeGroup cg = new MyCodeGroup (new AllMembershipCondition (), new PolicyStatement(new PermissionSet (PermissionState.None)));
			SecurityElement se = cg.ToXml ();
			se.Tag = "Mono";
			// strangely this works :(
			cg.FromXml (se);
			// let's get weirder :)
			foreach (SecurityElement child in se.Children) {
				child.Tag = "Mono";
			}
			cg.FromXml (se);
			// it's not enough :(( - very relax parsing
			se.Attributes = new Hashtable ();
			cg.FromXml (se);
			// arghh - I will prevail!
			foreach (SecurityElement child in se.Children) {
				child.Attributes = new Hashtable ();
			}
			cg.FromXml (se);
			// huh ? well maybe not (but this is both cruel and injust)
		}
	}  // public class CodeGroupTest 
}  // namespace MonoTests.System.Security.Policy