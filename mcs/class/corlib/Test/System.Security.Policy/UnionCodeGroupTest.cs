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
	public class UnionCodeGroupTest {

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
			Assert.IsNull (cg.AttributeString, "AttributeString");
			Assert.IsNull (cg.Description, "Description");
			Assert.IsNotNull (cg.MembershipCondition, "MembershipCondition");
			Assert.IsNull (cg.Name, "Name");
			Assert.IsNull (cg.PermissionSetName, "PermissionSetName");
			Assert.IsNull (cg.PolicyStatement, "PolicyStatement");
		}

		[Test]
		public void Constructor () 
		{
			UnionCodeGroup cg = new UnionCodeGroup (new AllMembershipCondition (), new PolicyStatement (new PermissionSet (PermissionState.None)));
			Assert.AreEqual (String.Empty, cg.AttributeString, "AttributeString");
			Assert.IsNull (cg.Description, "Description");
			Assert.IsNotNull (cg.MembershipCondition, "MembershipCondition");
			Assert.IsNull (cg.Name, "Name");
			Assert.IsNull (cg.PermissionSetName, "PermissionSetName");
			Assert.IsNotNull (cg.PolicyStatement, "PolicyStatement");
		}

		[Test]
		public void MergeLogic () 
		{
			UnionCodeGroup cg = new UnionCodeGroup (new AllMembershipCondition (), new PolicyStatement (new PermissionSet (PermissionState.None)));
			Assert.AreEqual ("Union", cg.MergeLogic, "MergeLogic");
		}

		[Test]
		public void Copy () 
		{
			UnionCodeGroup cg = new UnionCodeGroup (new AllMembershipCondition (), new PolicyStatement (new PermissionSet (PermissionState.None)));
			UnionCodeGroup cg2 = (UnionCodeGroup) cg.Copy ();
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
			UnionCodeGroup cgChild = new UnionCodeGroup (new AllMembershipCondition (), new PolicyStatement (new PermissionSet (PermissionState.Unrestricted)));
			UnionCodeGroup cg = new UnionCodeGroup (new AllMembershipCondition (), new PolicyStatement (new PermissionSet (PermissionState.None)));
			cg.AddChild (cgChild);
			UnionCodeGroup cg2 = (UnionCodeGroup) cg.Copy ();
			Assert.AreEqual (cg.Children.Count, cg2.Children.Count, "Children");
			Assert.AreEqual (cg.ToXml ().ToString (), cg2.ToXml ().ToString (), "ToXml");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Resolve_Null () 
		{
			UnionCodeGroup cg = new UnionCodeGroup (new AllMembershipCondition (), new PolicyStatement (new PermissionSet (PermissionState.None)));
			cg.Resolve (null);
		}

		[Test]
		public void Resolve_NoMatch ()
		{
			UnionCodeGroup cg = new UnionCodeGroup (new ZoneMembershipCondition (SecurityZone.Untrusted), new PolicyStatement (new PermissionSet (PermissionState.Unrestricted)));
			Assert.IsNull (cg.Resolve (new Evidence ()));
		}

		[Test]
		public void Resolve_AllMembershipCondition_None ()
		{
			UnionCodeGroup cg = new UnionCodeGroup (new AllMembershipCondition (), new PolicyStatement (new PermissionSet (PermissionState.None)));
			PolicyStatement result = cg.Resolve (new Evidence ());
			Assert.AreEqual (PolicyStatementAttribute.Nothing, result.Attributes, "Attributes");
			Assert.AreEqual (String.Empty, result.AttributeString, "AttributeString");
			Assert.IsFalse (result.PermissionSet.IsUnrestricted (), "IsUnrestricted");
			Assert.AreEqual (0, result.PermissionSet.Count, "Count");
		}

		[Test]
		public void Resolve_AllMembershipCondition_Unrestricted ()
		{
			UnionCodeGroup cg = new UnionCodeGroup (new AllMembershipCondition (), new PolicyStatement (new PermissionSet (PermissionState.Unrestricted)));
			PolicyStatement result = cg.Resolve (new Evidence ());
			Assert.AreEqual (PolicyStatementAttribute.Nothing, result.Attributes, "Attributes");
			Assert.AreEqual (String.Empty, result.AttributeString, "AttributeString");
			Assert.IsTrue (result.PermissionSet.IsUnrestricted (), "IsUnrestricted");
			Assert.AreEqual (0, result.PermissionSet.Count, "Count");
		}

		[Test]
		public void Resolve_ZoneMembershipCondition_Internet ()
		{
			IMembershipCondition mc = new ZoneMembershipCondition (SecurityZone.Internet);
			PermissionSet pset = new PermissionSet (PermissionState.Unrestricted);
			UnionCodeGroup cg = new UnionCodeGroup (mc, new PolicyStatement (pset, PolicyStatementAttribute.Nothing));

			Evidence e = new Evidence ();
			e.AddHost (new Zone (SecurityZone.Internet));
			PolicyStatement result = cg.Resolve (e);
			Assert.AreEqual (PolicyStatementAttribute.Nothing, result.Attributes, "Internet-Attributes");
			Assert.AreEqual (String.Empty, result.AttributeString, "Internet-AttributeString");
			Assert.IsTrue (result.PermissionSet.IsUnrestricted (),"Internet-IsUnrestricted");
			Assert.AreEqual (0, result.PermissionSet.Count, "Internet-Count");

			e = new Evidence ();
			e.AddHost (new Zone (SecurityZone.Intranet));
			Assert.IsNull (cg.Resolve (e), "Intranet");

			e = new Evidence ();
			e.AddHost (new Zone (SecurityZone.MyComputer));
			Assert.IsNull (cg.Resolve (e), "MyComputer");

			e = new Evidence ();
			e.AddHost (new Zone (SecurityZone.NoZone));
			Assert.IsNull (cg.Resolve (e), "NoZone");

			e = new Evidence ();
			e.AddHost (new Zone (SecurityZone.Trusted));
			Assert.IsNull (cg.Resolve (e), "Trusted");

			e = new Evidence ();
			e.AddHost (new Zone (SecurityZone.Untrusted));
			Assert.IsNull (cg.Resolve (e), "Untrusted");
		}

		[Test]
		public void Resolve_ZoneMembershipCondition_Intranet ()
		{
			IMembershipCondition mc = new ZoneMembershipCondition (SecurityZone.Intranet);
			PermissionSet pset = new PermissionSet (PermissionState.None);
			UnionCodeGroup cg = new UnionCodeGroup (mc, new PolicyStatement (pset, PolicyStatementAttribute.Exclusive));

			Evidence e = new Evidence ();
			e.AddHost (new Zone (SecurityZone.Intranet));
			PolicyStatement result = cg.Resolve (e);
			Assert.AreEqual (PolicyStatementAttribute.Exclusive, result.Attributes, "Intranet-Attributes");
			Assert.AreEqual ("Exclusive", result.AttributeString, "Intranet-AttributeString");
			Assert.IsFalse (result.PermissionSet.IsUnrestricted (), "Intranet-IsUnrestricted");
			Assert.AreEqual (0, result.PermissionSet.Count, "Intranet-Count");

			e = new Evidence ();
			e.AddHost (new Zone (SecurityZone.Internet));
			Assert.IsNull (cg.Resolve (e), "Internet");

			e = new Evidence ();
			e.AddHost (new Zone (SecurityZone.MyComputer));
			Assert.IsNull (cg.Resolve (e), "MyComputer");

			e = new Evidence ();
			e.AddHost (new Zone (SecurityZone.NoZone));
			Assert.IsNull (cg.Resolve (e), "NoZone");

			e = new Evidence ();
			e.AddHost (new Zone (SecurityZone.Trusted));
			Assert.IsNull (cg.Resolve (e), "Trusted");

			e = new Evidence ();
			e.AddHost (new Zone (SecurityZone.Untrusted));
			Assert.IsNull (cg.Resolve (e), "Untrusted");
		}

		[Test]
		public void Resolve_ZoneMembershipCondition_MyComputer ()
		{
			IMembershipCondition mc = new ZoneMembershipCondition (SecurityZone.MyComputer);
			PermissionSet pset = new PermissionSet (PermissionState.Unrestricted);
			UnionCodeGroup cg = new UnionCodeGroup (mc, new PolicyStatement (pset, PolicyStatementAttribute.LevelFinal));

			Evidence e = new Evidence ();
			e.AddHost (new Zone (SecurityZone.MyComputer));
			PolicyStatement result = cg.Resolve (e);
			Assert.AreEqual (PolicyStatementAttribute.LevelFinal, result.Attributes, "MyComputer-Attributes");
			Assert.AreEqual ("LevelFinal", result.AttributeString, "MyComputer-AttributeString");
			Assert.IsTrue (result.PermissionSet.IsUnrestricted (), "MyComputer-IsUnrestricted");
			Assert.AreEqual (0, result.PermissionSet.Count, "MyComputer-Count");

			e = new Evidence ();
			e.AddHost (new Zone (SecurityZone.Internet));
			Assert.IsNull (cg.Resolve (e), "Internet");

			e = new Evidence ();
			e.AddHost (new Zone (SecurityZone.Intranet));
			Assert.IsNull (cg.Resolve (e), "Intranet");

			e = new Evidence ();
			e.AddHost (new Zone (SecurityZone.NoZone));
			Assert.IsNull (cg.Resolve (e), "NoZone");

			e = new Evidence ();
			e.AddHost (new Zone (SecurityZone.Trusted));
			Assert.IsNull (cg.Resolve (e), "Trusted");

			e = new Evidence ();
			e.AddHost (new Zone (SecurityZone.Untrusted));
			Assert.IsNull (cg.Resolve (e), "Untrusted");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Resolve_ZoneMembershipCondition_NoZone ()
		{
			IMembershipCondition mc = new ZoneMembershipCondition (SecurityZone.NoZone);
		}

		[Test]
		public void Resolve_ZoneMembershipCondition_Trusted ()
		{
			IMembershipCondition mc = new ZoneMembershipCondition (SecurityZone.Trusted);
			PermissionSet pset = new PermissionSet (PermissionState.Unrestricted);
			UnionCodeGroup cg = new UnionCodeGroup (mc, new PolicyStatement (pset, PolicyStatementAttribute.All));

			Evidence e = new Evidence ();
			e.AddHost (new Zone (SecurityZone.Trusted));
			PolicyStatement result = cg.Resolve (e);
			Assert.AreEqual (PolicyStatementAttribute.All, result.Attributes, "Trusted-Attributes");
			Assert.AreEqual ("Exclusive LevelFinal", result.AttributeString, "Trusted-AttributeString");
			Assert.IsTrue (result.PermissionSet.IsUnrestricted (), "Trusted-IsUnrestricted");
			Assert.AreEqual (0, result.PermissionSet.Count, "Trusted-Count");

			e = new Evidence ();
			e.AddHost (new Zone (SecurityZone.Internet));
			Assert.IsNull (cg.Resolve (e), "Internet");

			e = new Evidence ();
			e.AddHost (new Zone (SecurityZone.Intranet));
			Assert.IsNull (cg.Resolve (e), "Intranet");

			e = new Evidence ();
			e.AddHost (new Zone (SecurityZone.MyComputer));
			Assert.IsNull (cg.Resolve (e), "MyComputer");

			e = new Evidence ();
			e.AddHost (new Zone (SecurityZone.NoZone));
			Assert.IsNull (cg.Resolve (e), "NoZone");

			e = new Evidence ();
			e.AddHost (new Zone (SecurityZone.Untrusted));
			Assert.IsNull (cg.Resolve (e), "Untrusted");
		}

		[Test]
		public void Resolve_ZoneMembershipCondition_Untrusted ()
		{
			IMembershipCondition mc = new ZoneMembershipCondition (SecurityZone.Untrusted);
			PermissionSet pset = new PermissionSet (PermissionState.None);
			UnionCodeGroup cg = new UnionCodeGroup (mc, new PolicyStatement (pset, PolicyStatementAttribute.Nothing));

			Evidence e = new Evidence ();
			e.AddHost (new Zone (SecurityZone.Untrusted));
			PolicyStatement result = cg.Resolve (e);
			Assert.AreEqual (PolicyStatementAttribute.Nothing, result.Attributes, "Untrusted-Attributes");
			Assert.AreEqual (String.Empty, result.AttributeString, "Untrusted-AttributeString");
			Assert.IsFalse (result.PermissionSet.IsUnrestricted (), "Untrusted-IsUnrestricted");
			Assert.AreEqual (0, result.PermissionSet.Count, "Untrusted-Count");

			e = new Evidence ();
			e.AddHost (new Zone (SecurityZone.Internet));
			Assert.IsNull (cg.Resolve (e), "Internet");

			e = new Evidence ();
			e.AddHost (new Zone (SecurityZone.Intranet));
			Assert.IsNull (cg.Resolve (e), "Intranet");

			e = new Evidence ();
			e.AddHost (new Zone (SecurityZone.MyComputer));
			Assert.IsNull (cg.Resolve (e), "MyComputer");

			e = new Evidence ();
			e.AddHost (new Zone (SecurityZone.NoZone));
			Assert.IsNull (cg.Resolve (e), "NoZone");

			e = new Evidence ();
			e.AddHost (new Zone (SecurityZone.Trusted));
			Assert.IsNull (cg.Resolve (e), "Trusted");
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
			Assert.IsNull (cg.ResolveMatchingCodeGroups (new Evidence ()));
		}

		[Test]
		public void ResolveMatchingCodeGroups_OneLevel ()
		{
			UnionCodeGroup level1 = new UnionCodeGroup (new AllMembershipCondition (), new PolicyStatement (new PermissionSet (PermissionState.None)));
			CodeGroup match = level1.ResolveMatchingCodeGroups (new Evidence ());
			Assert.IsNotNull (match, "Match");
			Assert.IsTrue (match.Equals (level1, false), "Equals(false)");
			Assert.IsTrue (match.Equals (level1, true), "Equals(true)");
		}

		[Test]
		public void ResolveMatchingCodeGroups_TwoLevel ()
		{
			UnionCodeGroup level1 = new UnionCodeGroup (new AllMembershipCondition (), new PolicyStatement (new PermissionSet (PermissionState.None)));
			CodeGroup level2 = level1.Copy ();
			level1.AddChild (level2);

			CodeGroup match = level1.ResolveMatchingCodeGroups (new Evidence ());
			Assert.IsNotNull (match, "Match");
			Assert.IsTrue (match.Equals (level1, false), "Equals(false)");
			Assert.IsTrue (match.Equals (level1, true), "Equals(true)");

			UnionCodeGroup level2b = new UnionCodeGroup (new ZoneMembershipCondition (SecurityZone.Untrusted), new PolicyStatement (new PermissionSet (PermissionState.Unrestricted)));
			level1.AddChild (level2b);
			CodeGroup match2 = level1.ResolveMatchingCodeGroups (new Evidence ());
			Assert.IsNotNull (match2, "Match2");
			Assert.IsTrue (match2.Equals (level1, false), "Equals(false)");
			Assert.IsTrue (!match2.Equals (level1, true), "Equals(true)");
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
			Assert.IsNotNull (match, "Match");
			Assert.IsTrue (match.Equals (level1, false), "Equals(false)");
			// Equals (true) isn't a deep compare (just one level)
			Assert.IsTrue (match.Equals (level1, true), "Equals(true)");
		}

		[Test]
		public void ToFromXmlRoundtrip () 
		{
			const string ps_Name = "TestName";
			PolicyStatement ps = new PolicyStatement (new NamedPermissionSet (ps_Name));
			UnionCodeGroup cg = new UnionCodeGroup (new AllMembershipCondition (), ps);
			cg.Name = "SomeName";
			cg.Description = "Some Description";
			Assert.IsTrue (cg.Equals (cg), "Equals (itself)");
			SecurityElement se = cg.ToXml ();

			UnionCodeGroup cg2 = new UnionCodeGroup (new AllMembershipCondition(), ps);
			cg2.Name = "SomeOtherName";
			cg2.Description = "Some Other Description";
			Assert.IsTrue (!cg.Equals (cg2), "Equals (another)");

			cg2.FromXml (se);
			Assert.IsTrue (cg.Equals (cg2), "Equals (FromXml)");
		}
	}
}
