//
// MonoTests.System.Security.Policy.UnionCodeGroupTest
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2004 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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
		public void ResolveMatchingCodeGroups_NoMatch ()
		{
			UnionCodeGroup cg = new UnionCodeGroup (new ZoneMembershipCondition (SecurityZone.Untrusted), new PolicyStatement (new PermissionSet (PermissionState.Unrestricted)));
			AssertNull (cg.ResolveMatchingCodeGroups (new Evidence ()));
		}

		[Test]
		public void ResolveMatchingCodeGroups_OneLevel ()
		{
			UnionCodeGroup level1 = new UnionCodeGroup (new AllMembershipCondition (), new PolicyStatement (new PermissionSet (PermissionState.None)));
			CodeGroup match = level1.ResolveMatchingCodeGroups (new Evidence ());
			AssertNotNull ("Match", match);
			Assert ("Equals(false)", match.Equals (level1, false));
			Assert ("Equals(true)", match.Equals (level1, true));
		}

		[Test]
		public void ResolveMatchingCodeGroups_TwoLevel ()
		{
			UnionCodeGroup level1 = new UnionCodeGroup (new AllMembershipCondition (), new PolicyStatement (new PermissionSet (PermissionState.None)));
			CodeGroup level2 = level1.Copy ();
			level1.AddChild (level2);

			CodeGroup match = level1.ResolveMatchingCodeGroups (new Evidence ());
			AssertNotNull ("Match", match);
			Assert ("Equals(false)", match.Equals (level1, false));
			Assert ("Equals(true)", match.Equals (level1, true));

			UnionCodeGroup level2b = new UnionCodeGroup (new ZoneMembershipCondition (SecurityZone.Untrusted), new PolicyStatement (new PermissionSet (PermissionState.Unrestricted)));
			level1.AddChild (level2b);
			CodeGroup match2 = level1.ResolveMatchingCodeGroups (new Evidence ());
			AssertNotNull ("Match2", match2);
			Assert ("Equals(false)2", match2.Equals (level1, false));
			Assert ("Equals(true)2", !match2.Equals (level1, true));
		}

		[Test]
		public void ResolveMatchingCodeGroups_ThreeLevel ()
		{
			UnionCodeGroup level1 = new UnionCodeGroup (new AllMembershipCondition (), new PolicyStatement (new PermissionSet (PermissionState.None)));
			CodeGroup level2 = level1.Copy ();
			level1.AddChild (level2);
			UnionCodeGroup level3 = new UnionCodeGroup (new ZoneMembershipCondition (SecurityZone.Untrusted), new PolicyStatement (new PermissionSet (PermissionState.Unrestricted)));
			level2.AddChild (level3);

			CodeGroup match = level1.ResolveMatchingCodeGroups (new Evidence ());
			AssertNotNull ("Match", match);
			Assert ("Equals(false)", match.Equals (level1, false));
			// Equals (true) isn't a deep compare (just one level)
			Assert ("Equals(true)", match.Equals (level1, true));
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
