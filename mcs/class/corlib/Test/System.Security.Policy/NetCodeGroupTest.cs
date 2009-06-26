//
// MonoTests.System.Security.Policy.NetCodeGroupTest
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
	public class NetCodeGroupTest  {

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
			Assert.IsNotNull (cg.MembershipCondition, "MembershipCondition");
			Assert.IsNull (cg.PolicyStatement, "PolicyStatement");
			// documented as always null
			Assert.IsNull (cg.AttributeString, "AttributeString");
#if NET_2_0
			// seems it's easier to change code than to change code ;)
			Assert.AreEqual ("Same site Web", cg.PermissionSetName, "PermissionSetName");
#else
			// documented as always "Same site Web" but it's "Same site Web." (missing .)
			Assert.AreEqual ("Same site Web.", cg.PermissionSetName, "PermissionSetName");
#endif
		}

		[Test]
		public void MergeLogic () 
		{
			NetCodeGroup cg = new NetCodeGroup (new AllMembershipCondition ());
			Assert.AreEqual ("Union", cg.MergeLogic, "MergeLogic");
		}

		[Test]
		public void Copy () 
		{
			NetCodeGroup cg = new NetCodeGroup (new AllMembershipCondition ());
			NetCodeGroup cg2 = (NetCodeGroup) cg.Copy ();
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
			NetCodeGroup cgChild = new NetCodeGroup (new AllMembershipCondition ());
			NetCodeGroup cg = new NetCodeGroup (new AllMembershipCondition ());
			cg.AddChild (cgChild);
			NetCodeGroup cg2 = (NetCodeGroup) cg.Copy ();
			Assert.AreEqual (cg.Children.Count, cg2.Children.Count, "Children");
			Assert.AreEqual (cg.ToXml ().ToString (), cg2.ToXml ().ToString (), "ToXml");
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
			Assert.IsTrue (cg.Equals (cg), "Equals (itself)");
			SecurityElement se = cg.ToXml ();

			NetCodeGroup cg2 = new NetCodeGroup (new AllMembershipCondition());
			cg2.Name = "SomeOtherName";
			cg2.Description = "Some Other Description";
			Assert.IsTrue (!cg.Equals (cg2), "Equals (another)");

			cg2.FromXml (se);
			Assert.IsTrue (cg.Equals (cg2), "Equals (FromXml)");
		}
	}
}
