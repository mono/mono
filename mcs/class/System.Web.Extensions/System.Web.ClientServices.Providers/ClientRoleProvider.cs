//
// System.Web.ClientServices.Providers.ClientRoleProvider
//
// Authors:
//   Marek Habersack (mhabersack@novell.com)
//
// (C) 2008 Novell, Inc
//

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

#if NET_3_5
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Net;
using System.Security;
using System.Security.Principal;
using System.Web;
using System.Web.Security;
using System.Web.UI;

namespace System.Web.ClientServices.Providers
{
	public class ClientRoleProvider : RoleProvider
	{
		public override string ApplicationName {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		public string ServiceUri {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		public ClientRoleProvider ()
		{
			throw new NotImplementedException ();
		}
		
		public override void AddUsersToRoles (string [] usernames, string [] roleNames)
		{
			throw new NotImplementedException ();
		}
		
		public override void CreateRole (string roleName)
		{
			throw new NotImplementedException ();
		}
		
		public override bool DeleteRole (string roleName, bool throwOnPopulatedRole)
		{
			throw new NotImplementedException ();
		}
		
		public override string [] FindUsersInRole (string roleName, string usernameToMatch)
		{
			throw new NotImplementedException ();
		}
		
		public override string [] GetAllRoles ()
		{
			throw new NotImplementedException ();
		}
		
		public override string [] GetRolesForUser (string username)
		{
			throw new NotImplementedException ();
		}
		
		public override string [] GetUsersInRole (string roleName)
		{
			throw new NotImplementedException ();
		}
		
		public override void Initialize (string name, NameValueCollection config)
		{
			throw new NotImplementedException ();
		}
		
		public override bool IsUserInRole (string username, string roleName)
		{
			throw new NotImplementedException ();
		}
		
		public override void RemoveUsersFromRoles (string [] usernames, string [] roleNames)
		{
			throw new NotImplementedException ();
		}
		
		public void ResetCache ()
		{
			throw new NotImplementedException ();
		}
		
		public override bool RoleExists (string roleName)
		{
			throw new NotImplementedException ();
		}
	}
}
#endif
