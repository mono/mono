//
// RolePrincipalTest.cs
//	- Unit tests for System.Web.Security.RolePrincipal
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
using System.Configuration.Provider;
using System.Security.Principal;
using System.Web.Security;

using NUnit.Framework;

namespace MonoTests.System.Web.Security {

	public class TestRoleProvider : RoleProvider {

		public override void AddUsersToRoles (string[] usernames, string[] roleNames)
		{
			throw new Exception ("The method or operation is not implemented.");
		}

		public override string ApplicationName
		{
			get
			{
				throw new Exception ("The method or operation is not implemented.");
			}
			set
			{
				throw new Exception ("The method or operation is not implemented.");
			}
		}

		public override void CreateRole (string roleName)
		{
			throw new Exception ("The method or operation is not implemented.");
		}

		public override bool DeleteRole (string roleName, bool throwOnPopulatedRole)
		{
			throw new Exception ("The method or operation is not implemented.");
		}

		public override string[] FindUsersInRole (string roleName, string usernameToMatch)
		{
			throw new Exception ("The method or operation is not implemented.");
		}

		public override string[] GetAllRoles ()
		{
			throw new Exception ("The method or operation is not implemented.");
		}

		public override string[] GetRolesForUser (string username)
		{
			throw new Exception ("The method or operation is not implemented.");
		}

		public override string[] GetUsersInRole (string roleName)
		{
			throw new Exception ("The method or operation is not implemented.");
		}

		public override bool IsUserInRole (string username, string roleName)
		{
			throw new Exception ("The method or operation is not implemented.");
		}

		public override void RemoveUsersFromRoles (string[] usernames, string[] roleNames)
		{
			throw new Exception ("The method or operation is not implemented.");
		}

		public override bool RoleExists (string roleName)
		{
			throw new Exception ("The method or operation is not implemented.");
		}
}

	[TestFixture]
	public class RolePrincipalTest {

		private IIdentity GetGenericIdentity (string name)
		{
			return new GenericIdentity (name);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Contructor_Identity_Null ()
		{
			RolePrincipal rp = new RolePrincipal (null);
		}

		[Test]
		[ExpectedException (typeof (ProviderException))]
		[Category ("NotWorking")]
		public void Contructor_Identity ()
		{
			RolePrincipal rp = new RolePrincipal (GetGenericIdentity ("me"));
		}
	}
}

#endif
