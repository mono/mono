//
// MonoTests.System.Security.Policy.FileCodeGroupTest
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
	public class FileCodeGroupTest {

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
			Assert.IsNotNull (cg.MembershipCondition, "MembershipCondition");
			Assert.IsNull (cg.PolicyStatement, "PolicyStatement");
			// documented as always null
			Assert.IsNull (cg.AttributeString, "AttributeString");
			Assert.IsNotNull (cg.PermissionSetName, "PermissionSetName");
#if NET_2_0
			Assert.AreEqual (CodeGroupGrantScope.Assembly, cg.Scope, "Scope");
#endif
		}

		[Test]
		public void Constructor_Append () 
		{
			FileCodeGroup cg = new FileCodeGroup (new AllMembershipCondition (), FileIOPermissionAccess.Append);
			Assert.IsNotNull (cg.MembershipCondition, "MembershipCondition");
			Assert.IsNull (cg.PolicyStatement, "PolicyStatement");
			// documented as always null
			Assert.IsNull (cg.AttributeString, "AttributeString");
			Assert.IsNotNull (cg.PermissionSetName, "PermissionSetName");
#if NET_2_0
			Assert.AreEqual (CodeGroupGrantScope.Assembly, cg.Scope, "Scope");
#endif
		}

		[Test]
		public void Constructor_NoAccess () 
		{
			FileCodeGroup cg = new FileCodeGroup (new AllMembershipCondition (), FileIOPermissionAccess.NoAccess);
			Assert.IsNotNull (cg.MembershipCondition, "MembershipCondition");
			Assert.IsNull (cg.PolicyStatement, "PolicyStatement");
			// documented as always null
			Assert.IsNull (cg.AttributeString, "AttributeString");
			Assert.IsNotNull (cg.PermissionSetName, "PermissionSetName");
#if NET_2_0
			Assert.AreEqual (CodeGroupGrantScope.Assembly, cg.Scope, "Scope");
#endif
		}

		[Test]
		public void Constructor_PathDiscovery () 
		{
			FileCodeGroup cg = new FileCodeGroup (new AllMembershipCondition (), FileIOPermissionAccess.PathDiscovery);
			Assert.IsNotNull (cg.MembershipCondition, "MembershipCondition");
			Assert.IsNull (cg.PolicyStatement, "PolicyStatement");
			// documented as always null
			Assert.IsNull (cg.AttributeString, "AttributeString");
			Assert.IsNotNull (cg.PermissionSetName, "PermissionSetName");
#if NET_2_0
			Assert.AreEqual (CodeGroupGrantScope.Assembly, cg.Scope, "Scope");
#endif
		}

		[Test]
		public void Constructor_Read () 
		{
			FileCodeGroup cg = new FileCodeGroup (new AllMembershipCondition (), FileIOPermissionAccess.Read);
			Assert.IsNotNull (cg.MembershipCondition, "MembershipCondition");
			Assert.IsNull (cg.PolicyStatement, "PolicyStatement");
			// documented as always null
			Assert.IsNull (cg.AttributeString, "AttributeString");
			Assert.IsNotNull (cg.PermissionSetName, "PermissionSetName");
#if NET_2_0
			Assert.AreEqual (CodeGroupGrantScope.Assembly, cg.Scope, "Scope");
#endif
		}

		[Test]
		public void Constructor_Write () 
		{
			FileCodeGroup cg = new FileCodeGroup (new AllMembershipCondition (), FileIOPermissionAccess.Write);
			Assert.IsNotNull (cg.MembershipCondition, "MembershipCondition");
			Assert.IsNull (cg.PolicyStatement, "PolicyStatement");
			// documented as always null
			Assert.IsNull (cg.AttributeString, "AttributeString");
			Assert.IsNotNull (cg.PermissionSetName, "PermissionSetName");
#if NET_2_0
			Assert.AreEqual (CodeGroupGrantScope.Assembly, cg.Scope, "Scope");
#endif
		}

		[Test]
		public void MergeLogic () 
		{
			FileCodeGroup cg = new FileCodeGroup (new AllMembershipCondition (), FileIOPermissionAccess.AllAccess);
			Assert.AreEqual ("Union", cg.MergeLogic, "MergeLogic");
		}

		[Test]
		public void Copy () 
		{
			FileCodeGroup cg = new FileCodeGroup (new AllMembershipCondition (), FileIOPermissionAccess.AllAccess);
			FileCodeGroup cg2 = (FileCodeGroup) cg.Copy ();
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
			FileCodeGroup cgChild = new FileCodeGroup (new AllMembershipCondition (), FileIOPermissionAccess.AllAccess);
			FileCodeGroup cg = new FileCodeGroup (new AllMembershipCondition (), FileIOPermissionAccess.AllAccess);
			cg.AddChild (cgChild);
			FileCodeGroup cg2 = (FileCodeGroup) cg.Copy ();
			Assert.AreEqual (cg.Children.Count, cg2.Children.Count, "Children");
			Assert.AreEqual (cg.ToXml ().ToString (), cg2.ToXml ().ToString (), "ToXml");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Resolve_Null () 
		{
			FileCodeGroup cg = new FileCodeGroup (new AllMembershipCondition (), FileIOPermissionAccess.AllAccess);
			cg.Resolve (null);
		}

		[Test]
		public void Resolve_NoMatch ()
		{
			FileCodeGroup cg = new FileCodeGroup (new ZoneMembershipCondition (SecurityZone.Untrusted), FileIOPermissionAccess.AllAccess);
			Assert.IsNull (cg.Resolve (new Evidence ()));
		}

		[Test]
		public void Resolve_AllMembershipCondition_NoAccess ()
		{
			FileCodeGroup cg = new FileCodeGroup (new AllMembershipCondition (), FileIOPermissionAccess.NoAccess);
			PolicyStatement result = cg.Resolve (new Evidence ());
			Assert.AreEqual (PolicyStatementAttribute.Nothing, result.Attributes, "Attributes");
			Assert.AreEqual (String.Empty, result.AttributeString, "AttributeString");
			Assert.IsFalse (result.PermissionSet.IsUnrestricted (), "IsUnrestricted");
			Assert.AreEqual (0, result.PermissionSet.Count, "Count");
#if NET_2_0
			Assert.AreEqual (CodeGroupGrantScope.Assembly, cg.Scope, "Scope");
#endif
		}

		[Test]
		public void Resolve_AllMembershipCondition_AllAccess ()
		{
			FileCodeGroup cg = new FileCodeGroup (new AllMembershipCondition (), FileIOPermissionAccess.AllAccess);
			PolicyStatement result = cg.Resolve (new Evidence ());
			Assert.AreEqual (PolicyStatementAttribute.Nothing, result.Attributes, "Attributes");
			Assert.AreEqual (String.Empty, result.AttributeString, "AttributeString");
			Assert.IsFalse (result.PermissionSet.IsUnrestricted (), "IsUnrestricted");
			Assert.AreEqual (0, result.PermissionSet.Count, "Count");
#if NET_2_0
			Assert.AreEqual (CodeGroupGrantScope.Assembly, cg.Scope, "Scope");
#endif
		}

		[Test]
		public void Resolve_ZoneMembershipCondition_Internet ()
		{
			IMembershipCondition mc = new ZoneMembershipCondition (SecurityZone.Internet);
			PermissionSet pset = new PermissionSet (PermissionState.Unrestricted);
			FileCodeGroup cg = new FileCodeGroup (mc, FileIOPermissionAccess.AllAccess);

			Evidence e = new Evidence ();
			e.AddHost (new Zone (SecurityZone.Internet));
			PolicyStatement result = cg.Resolve (e);
			Assert.AreEqual (PolicyStatementAttribute.Nothing, result.Attributes, "Internet-Attributes");
			Assert.AreEqual (String.Empty, result.AttributeString, "Internet-AttributeString");
			Assert.IsFalse (result.PermissionSet.IsUnrestricted (), "Internet-IsUnrestricted");
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
			FileCodeGroup cg = new FileCodeGroup (mc, FileIOPermissionAccess.AllAccess);

			Evidence e = new Evidence ();
			e.AddHost (new Zone (SecurityZone.Intranet));
			PolicyStatement result = cg.Resolve (e);
			Assert.AreEqual (PolicyStatementAttribute.Nothing, result.Attributes, "Internet-Attributes");
			Assert.AreEqual (String.Empty, result.AttributeString, "Internet-AttributeString");
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
			FileCodeGroup cg = new FileCodeGroup (mc, FileIOPermissionAccess.AllAccess);

			Evidence e = new Evidence ();
			e.AddHost (new Zone (SecurityZone.MyComputer));
			PolicyStatement result = cg.Resolve (e);
			Assert.AreEqual (PolicyStatementAttribute.Nothing, result.Attributes, "Internet-Attributes");
			Assert.AreEqual (String.Empty, result.AttributeString, "Internet-AttributeString");
			Assert.IsFalse (result.PermissionSet.IsUnrestricted (), "MyComputer-IsUnrestricted");
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
			FileCodeGroup cg = new FileCodeGroup (mc, FileIOPermissionAccess.AllAccess);

			Evidence e = new Evidence ();
			e.AddHost (new Zone (SecurityZone.Trusted));
			PolicyStatement result = cg.Resolve (e);
			Assert.AreEqual (PolicyStatementAttribute.Nothing, result.Attributes, "Internet-Attributes");
			Assert.AreEqual (String.Empty, result.AttributeString, "Internet-AttributeString");
			Assert.IsFalse (result.PermissionSet.IsUnrestricted (), "Trusted-IsUnrestricted");
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
			FileCodeGroup cg = new FileCodeGroup (mc, FileIOPermissionAccess.AllAccess);

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
			FileCodeGroup cg = new FileCodeGroup (new AllMembershipCondition (), FileIOPermissionAccess.AllAccess);
			cg.ResolveMatchingCodeGroups (null);
		}

		[Test]
		public void ResolveMatchingCodeGroups_NoMatch ()
		{
			FileCodeGroup cg = new FileCodeGroup (new ZoneMembershipCondition (SecurityZone.Untrusted), FileIOPermissionAccess.AllAccess);
			Assert.IsNull (cg.ResolveMatchingCodeGroups (new Evidence ()));
		}

		[Test]
		public void ResolveMatchingCodeGroups_OneLevel ()
		{
			FileCodeGroup level1 = new FileCodeGroup (new AllMembershipCondition (), FileIOPermissionAccess.AllAccess);
			CodeGroup match = level1.ResolveMatchingCodeGroups (new Evidence ());
			Assert.IsNotNull (match, "Match");
			Assert.IsTrue (match.Equals (level1, false), "Equals(false)");
			Assert.IsTrue (match.Equals (level1, true), "Equals(true)");
		}

		[Test]
		public void ResolveMatchingCodeGroups_TwoLevel ()
		{
			FileCodeGroup level1 = new FileCodeGroup (new AllMembershipCondition (), FileIOPermissionAccess.AllAccess);
			CodeGroup level2 = level1.Copy ();
			level1.AddChild (level2);

			CodeGroup match = level1.ResolveMatchingCodeGroups (new Evidence ());
			Assert.IsNotNull (match, "Match");
			Assert.IsTrue (match.Equals (level1, false), "Equals(false)");
			Assert.IsTrue (match.Equals (level1, true), "Equals(true)");

			FileCodeGroup level2b = new FileCodeGroup (new ZoneMembershipCondition (SecurityZone.Untrusted), FileIOPermissionAccess.AllAccess);
			level1.AddChild (level2b);
			CodeGroup match2 = level1.ResolveMatchingCodeGroups (new Evidence ());
			Assert.IsNotNull (match2, "Match2");
			Assert.IsTrue (match2.Equals (level1, false), "Equals(false)");
			Assert.IsTrue (!match2.Equals (level1, true), "Equals(true)");
		}

		[Test]
		public void ResolveMatchingCodeGroups_ThreeLevel ()
		{
			FileCodeGroup level1 = new FileCodeGroup (new AllMembershipCondition (), FileIOPermissionAccess.AllAccess);
			CodeGroup level2 = level1.Copy ();
			level1.AddChild (level2);
			FileCodeGroup level3 = new FileCodeGroup (new ZoneMembershipCondition (SecurityZone.Untrusted), FileIOPermissionAccess.AllAccess);
			level2.AddChild (level3);

			CodeGroup match = level1.ResolveMatchingCodeGroups (new Evidence ());
			Assert.IsNotNull (match, "Match");
			Assert.IsTrue (match.Equals (level1, false), "Equals(false)");
			// Equals (true) isn't a deep compare (just one level)
			Assert.IsTrue (match.Equals (level1, true), "Equals(true)");
		}

		[Test]
		public void ToXml () 
		{
			FileIOPermissionAccess access = FileIOPermissionAccess.Read | FileIOPermissionAccess.Write;
			FileCodeGroup cg = new FileCodeGroup (new AllMembershipCondition (), access);
			string s = cg.ToXml ().ToString ();
			Assert.IsTrue (s.IndexOf ("Access=\"Read, Write\"") > 0, "Access='Read, Write'");
		}

		[Test]
		public void ToFromXmlRoundtrip () 
		{
			FileCodeGroup cg = new FileCodeGroup (new AllMembershipCondition (), FileIOPermissionAccess.AllAccess);
			cg.Name = "SomeName";
			cg.Description = "Some Description";
			Assert.IsTrue (cg.Equals (cg), "Equals (itself)");
			SecurityElement se = cg.ToXml ();

			FileCodeGroup cg2 = new FileCodeGroup (new AllMembershipCondition (), FileIOPermissionAccess.NoAccess);
			cg2.Name = "SomeOtherName";
			cg2.Description = "Some Other Description";
			Assert.IsFalse (cg.Equals (cg2), "Equals (another)");

			cg2.FromXml (se);
			Assert.IsTrue (cg.Equals (cg2), "Equals (FromXml)");
		}
	}
}
