//
// System.Web.Security.Roles
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
using System.Collections;
using System.Collections.Specialized;
using System.Text;

namespace System.Web.Security
{
	public sealed class Roles
	{
		public static void AddUsersToRole (string [] usernames, string rolename)
		{
			Provider.AddUsersToRoles (usernames, new string[] {rolename});
		}
		
		public static void AddUsersToRoles (string [] usernames, string [] rolenames)
		{
			Provider.AddUsersToRoles (usernames, rolenames);
		}
		
		public static void AddUserToRole (string username, string rolename)
		{
			Provider.AddUsersToRoles (new string[] {username}, new string[] {rolename});
		}
		
		public static void AddUserToRoles (string username, string [] rolenames)
		{
			Provider.AddUsersToRoles (new string[] {username}, rolenames);
		}
		
		public static void CreateRole (string rolename)
		{
			Provider.CreateRole (rolename);
		}
		
		[MonoTODO]
		public static void DeleteCookie ()
		{
			throw new NotImplementedException ();
		}
		
		public static bool DeleteRole (string rolename)
		{
			return Provider.DeleteRole (rolename, true);
		}
		
		public static bool DeleteRole (string rolename, bool throwOnPopulatedRole)
		{
			return Provider.DeleteRole (rolename, throwOnPopulatedRole);
		}
		
		public static string [] GetAllRoles ()
		{
			return Provider.GetAllRoles ();
		}
		
		public static string [] GetRolesForUser ()
		{
			return Provider.GetRolesForUser (CurrentUser);
		}
		
		static string CurrentUser {
			get {
				if (HttpContext.Current != null && HttpContext.Current.User != null)
					return HttpContext.Current.User.Identity.Name;
				else
					return System.Threading.Thread.CurrentPrincipal.Identity.Name;
			}
		}
		
		public static string [] GetRolesForUser (string username)
		{
			return Provider.GetRolesForUser (username);
		}
		
		public static string [] GetUsersInRole (string rolename)
		{
			return Provider.GetUsersInRole (rolename);
		}
		
		public static bool IsUserInRole (string rolename)
		{
			return Provider.IsUserInRole (CurrentUser, rolename);
		}
		
		public static bool IsUserInRole (string username, string rolename)
		{
			return Provider.IsUserInRole (username, rolename);
		}
		
		public static void RemoveUserFromRole (string username, string rolename)
		{
			Provider.RemoveUsersFromRoles (new string[] {username}, new string[] {rolename});
		}
		
		public static void RemoveUserFromRoles (string username, string [] rolenames)
		{
			Provider.RemoveUsersFromRoles (new string[] {username}, rolenames);
		}
		
		public static void RemoveUsersFromRole (string [] usernames, string rolename)
		{
			Provider.RemoveUsersFromRoles (usernames, new string[] {rolename});
		}
		
		public static void RemoveUsersFromRoles (string [] usernames, string [] rolenames)
		{
			Provider.RemoveUsersFromRoles (usernames, rolenames);
		}
		
		public static bool RoleExists (string rolename)
		{
			return Provider.RoleExists (rolename);
		}
		
		public static string[] FinsUsersInRole (string rolename, string usernameToMatch)
		{
			return Provider.FindUsersInRole (rolename, usernameToMatch);
		}
		
		public static string ApplicationName {
			get { return Provider.ApplicationName; }
			set { Provider.ApplicationName = value; }
		}
		
		[MonoTODO]
		public static bool CacheRolesInCookie {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public static string CookieName {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public static string CookiePath {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public static CookieProtection CookieProtectionValue {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public static bool CookieRequireSSL {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public static bool CookieSlidingExpiration {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public static int CookieTimeout {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public static bool Enabled {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public static RoleProvider Provider {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public static RoleProviderCollection Providers {
			get { throw new NotImplementedException (); }
		}
	}
}
#endif

