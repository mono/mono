//
// RoleGroupTest.cs - Unit tests for System.Web.UI.WebControls.RoleGroup
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
	public class RoleGroupTest : ITemplate {

		public void InstantiateIn (Control container)
		{
			// ITemplate
		}

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
		public void DefaultValues ()
		{
			RoleGroup rg = new RoleGroup ();
			Assert.IsNull (rg.ContentTemplate, "ContentTemplate");
			Assert.AreEqual (0, rg.Roles.Length, "Roles");
			Assert.AreEqual (String.Empty, rg.ToString ());
		}

		[Test]
		public void ContentTemplate ()
		{
			RoleGroup rg = new RoleGroup ();
			rg.ContentTemplate = this;
			Assert.IsNotNull (rg.ContentTemplate, "ContentTemplate");
		}

		[Test]
		public void Roles_Null ()
		{
			RoleGroup rg = new RoleGroup ();
			rg.Roles = null;
			Assert.AreEqual (String.Empty, rg.ToString ());
			Assert.AreEqual (0, rg.Roles.Length, "Roles");
		}

		[Test]
		public void Roles_One ()
		{
			RoleGroup rg = new RoleGroup ();
			rg.Roles = new string[1] { "mono" };
			Assert.AreEqual ("mono", rg.ToString ());
			Assert.AreEqual (1, rg.Roles.Length, "Roles");
		}

		[Test]
		public void Roles_Two ()
		{
			RoleGroup rg = new RoleGroup ();
			rg.Roles = new string[2] { "mono", "hackers" };
			Assert.AreEqual ("mono,hackers", rg.ToString ());
			Assert.AreEqual (2, rg.Roles.Length, "Roles");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ContainsUser_Null ()
		{
			RoleGroup rg = new RoleGroup ();
			Assert.IsFalse (rg.ContainsUser (null), "null");
		}

		[Test]
		public void ContainsUser_NoRoles ()
		{
			RoleGroup rg = new RoleGroup ();
			Assert.IsFalse (rg.ContainsUser (GetPrincipal ("me")), "me");
		}

		[Test]
		public void ContainsUser_In ()
		{
			RoleGroup rg = new RoleGroup ();
			rg.Roles = new string[2] { "mono", "hackers" };
			Assert.IsTrue (rg.ContainsUser (GetPrincipal ("me1", "mono")), "me+mono");
			Assert.IsTrue (rg.ContainsUser (GetPrincipal ("me2", "hackers")), "me+hackers");
			// works for unauthenticated principals too
			Assert.IsTrue (rg.ContainsUser (GetUnauthenticatedPrincipal ("me3", "mono")), "unauthenticated+me+mono");
			Assert.IsTrue (rg.ContainsUser (GetUnauthenticatedPrincipal ("me4", "hackers")), "unauthenticated+me+hackers");
			// case insensitive
			Assert.IsTrue (rg.ContainsUser (GetPrincipal ("me5", "MoNo")), "case+me+mono");
			Assert.IsTrue (rg.ContainsUser (GetPrincipal ("me6", "hAcKeRs")), "case+me+hackers");
		}

		[Test]
		public void ContainsUser_Out ()
		{
			RoleGroup rg = new RoleGroup ();
			rg.Roles = new string[2] { "mono", "hackers" };
			Assert.IsFalse (rg.ContainsUser (GetPrincipal ("me", "m0n0")), "m0n0");
			Assert.IsFalse (rg.ContainsUser (GetPrincipal ("me", "h4ck")), "h4ck");
		}
	}
}

#endif
