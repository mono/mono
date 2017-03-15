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
using System.Configuration.Provider;
using System.Runtime.CompilerServices;

namespace System.Web.Security
{
	[TypeForwardedFrom ("System.Web, Version=2.0.0.0, Culture=Neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	public abstract class RoleProvider : ProviderBase
	{
		protected RoleProvider ()
		{
		}
		
		public abstract void AddUsersToRoles (string [] usernames, string [] roleNames);
		public abstract void CreateRole (string roleName);
		public abstract bool DeleteRole (string roleName, bool throwOnPopulatedRole);
		public abstract string [] FindUsersInRole (string roleName, string usernameToMatch);
		public abstract string [] GetAllRoles ();
		public abstract string [] GetRolesForUser (string username);
		public abstract string [] GetUsersInRole (string roleName);
		public abstract bool IsUserInRole (string username, string roleName);
		public abstract void RemoveUsersFromRoles (string [] usernames, string [] roleNames);
		public abstract bool RoleExists (string roleName);
		public abstract string ApplicationName { get; set; }
	}
}


