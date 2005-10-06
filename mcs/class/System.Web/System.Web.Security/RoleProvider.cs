//
// System.Web.Security.IRoleProvider
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
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

#if NET_2_0
using System.Configuration.Provider;

namespace System.Web.Security
{
	public abstract class RoleProvider : ProviderBase
	{
		protected RoleProvider ()
		{
		}
		
		public abstract void AddUsersToRoles (string [] usernames, string [] rolenames);
		public abstract void CreateRole (string rolename);
		public abstract void DeleteRole (string rolename, bool throwOnPopulatedRole);
		public abstract void FindUsersInRole (string roleName, string usernameToMatch);
		public abstract string [] GetAllRoles ();
		public abstract string [] GetRolesForUser (string username);
		public abstract string [] GetUsersInRole (string rolename);
		public abstract bool IsUserInRole (string username, string rolename);
		public abstract void RemoveUsersFromRoles (string [] usernames, string [] rolenames);
		public abstract bool RoleExists (string rolename);
		public abstract string ApplicationName { get; set; }
	}
}
#endif

