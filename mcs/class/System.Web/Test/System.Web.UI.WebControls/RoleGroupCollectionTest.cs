//
// RoleGroupCollectionTest.cs 
//	- Unit tests for System.Web.UI.WebControls.RoleGroupCollection
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System;
using System.IO;
using System.Security.Principal;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using NUnit.Framework;

namespace MonoTests.System.Web.UI.WebControls {

	[TestFixture]
	public class RoleGroupCollectionTest {

		private IPrincipal GetPrincipal (string name)
		{
			return new GenericPrincipal (new GenericIdentity (name), null);
		}

		private IPrincipal GetPrincipal (string name, string role)
		{
			return new GenericPrincipal (new GenericIdentity (name), new string[1] { role });
		}

		private IPrincipal GetUnauthenticatedPrincipal (string name, string role)
		{
			return new GenericPrincipal (new UnauthenticatedIdentity (name), new string[1] { role });
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Add_Null ()
		{
			RoleGroupCollection rgc = new RoleGroupCollection ();
			rgc.Add (null);
		}

		[Test]
		public void Add ()
		{
			RoleGroupCollection rgc = new RoleGroupCollection ();
			Assert.AreEqual (0, rgc.Count, "0");
			RoleGroup rg1 = new RoleGroup ();
			rgc.Add (rg1);
			Assert.AreEqual (1, rgc.Count, "1");
			rgc.Add (rg1);
			Assert.AreEqual (2, rgc.Count, "2");
		}

		[Test]
		public void Contains ()
		{
			RoleGroupCollection rgc = new RoleGroupCollection ();
			Assert.IsFalse (rgc.Contains (null), "null");

			RoleGroup rg1 = new RoleGroup ();
			rgc.Add (rg1);
			Assert.IsTrue (rgc.Contains (rg1), "1a");

			RoleGroup rg2 = new RoleGroup ();
			Assert.IsFalse (rgc.Contains (rg2), "2a");
			rgc.Add (rg2);
			Assert.IsTrue (rgc.Contains (rg2), "2b");

			rgc.Remove (rg1);
			Assert.IsFalse (rgc.Contains (rg1), "1b");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetMatchingRoleGroup_Null ()
		{
			RoleGroupCollection rgc = new RoleGroupCollection ();
			rgc.GetMatchingRoleGroup (null);
		}

		[Test]
		public void GetMatchingRoleGroup_NoRoles ()
		{
			RoleGroup rg = new RoleGroup ();
			
			RoleGroupCollection rgc = new RoleGroupCollection ();
			rgc.Add (rg);
			Assert.AreEqual (1, rgc.Count, "Count");

			RoleGroup result = rgc.GetMatchingRoleGroup (GetPrincipal ("me"));
			Assert.IsNull (result, "me");

			result = rgc.GetMatchingRoleGroup (GetPrincipal ("me", "mono"));
			Assert.IsNull (result, "me+mono");
		}

		[Test]
		public void GetMatchingRoleGroup_In ()
		{
			RoleGroup rg = new RoleGroup ();
			rg.Roles = new string[2] { "mono", "hackers" };

			RoleGroupCollection rgc = new RoleGroupCollection ();
			rgc.Add (rg);
			Assert.AreEqual (1, rgc.Count, "Count");

			RoleGroup result = rgc.GetMatchingRoleGroup (GetPrincipal ("me", "mono"));
			Assert.IsNotNull (result, "me+mono");
			Assert.IsTrue (Object.ReferenceEquals (result, rg), "ref1");

			result = rgc.GetMatchingRoleGroup (GetPrincipal ("me", "hackers"));
			Assert.IsNotNull (result, "me+hackers");
			Assert.IsTrue (Object.ReferenceEquals (result, rg), "ref2");

			// works for unauthenticated principals too
			result = rgc.GetMatchingRoleGroup (GetUnauthenticatedPrincipal ("me", "mono"));
			Assert.IsNotNull (result, "unauthenticated+me+mono");
			Assert.IsTrue (Object.ReferenceEquals (result, rg), "ref3");

			result = rgc.GetMatchingRoleGroup (GetUnauthenticatedPrincipal ("me", "hackers"));
			Assert.IsNotNull (result, "unauthenticated+me+hackers");
			Assert.IsTrue (Object.ReferenceEquals (result, rg), "ref4");

			// case insensitive
			result = rgc.GetMatchingRoleGroup (GetPrincipal ("me", "MoNo"));
			Assert.IsNotNull (result, "unauthenticated+me+MoNo");
			Assert.IsTrue (Object.ReferenceEquals (result, rg), "ref5");

			result = rgc.GetMatchingRoleGroup (GetPrincipal ("me", "hAcKeRs"));
			Assert.IsNotNull (result, "unauthenticated+me+hAcKeRs");
			Assert.IsTrue (Object.ReferenceEquals (result, rg), "ref6");
		}

		[Test]
		public void ContainsUser_Out ()
		{
			RoleGroup rg = new RoleGroup ();
			rg.Roles = new string[2] { "mono", "hackers" };

			RoleGroupCollection rgc = new RoleGroupCollection ();
			rgc.Add (rg);
			Assert.AreEqual (1, rgc.Count, "Count");

			RoleGroup result = rgc.GetMatchingRoleGroup (GetPrincipal ("me", "m0n0"));
			Assert.IsNull (result, "me+MoNo");

			result = rgc.GetMatchingRoleGroup (GetPrincipal ("me", "h4ck"));
			Assert.IsNull (result, "me+h4ck");
		}

		[Test]
		public void IndexOf ()
		{
			RoleGroupCollection rgc = new RoleGroupCollection ();
			Assert.AreEqual (-1, rgc.IndexOf (null), "null");

			RoleGroup rg1 = new RoleGroup ();
			rgc.Add (rg1);
			Assert.AreEqual (0, rgc.IndexOf (rg1), "0");
			rgc.Add (rg1);
			Assert.AreEqual (0, rgc.IndexOf (rg1), "1");
		}

		[Test]
		public void Remove ()
		{
			RoleGroupCollection rgc = new RoleGroupCollection ();
			rgc.Remove (null);

			RoleGroup rg1 = new RoleGroup ();
			rgc.Remove (rg1);
			rgc.Add (rg1);
			rgc.Add (rg1);
			Assert.AreEqual (2, rgc.Count, "Count");
			rgc.Remove (rg1);
			Assert.IsTrue (rgc.Contains (rg1), "rg1-bis");

			RoleGroup rg2 = new RoleGroup ();
			rgc.Add (rg2);
			rgc.Remove (rg2);
			rgc.Remove (rg2);
		}

		[Test]
		public void ThisIndex ()
		{
			RoleGroupCollection rgc = new RoleGroupCollection ();
			RoleGroup rg = new RoleGroup ();
			rgc.Add (rg);
			Assert.IsTrue (Object.ReferenceEquals (rg, rgc [0]));
		}
	}
}

#endif
