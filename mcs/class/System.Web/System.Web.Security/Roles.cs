//
// System.Web.Security.Roles
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Ben Maurer
// Copyright (c) 2005 Novell, Inc (http://www.novell.com)
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

namespace System.Web.Security {

	[MonoTODO ("read infos from web.config")]
	public static class Roles {

		private static RoleProvider provider;
		private static bool cookie_cache_roles;
		private static string cookie_name;
		private static string cookie_path;
		private static CookieProtection cookie_protection;
		private static bool cookie_ssl;
		private static bool cookie_sliding;
		private static int cookie_timeout;
		private static bool cookie_persistent;
		private static string domain;
		private static int max_cached_result;

		static Roles ()
		{
			// default values (when not supplied in web.config)
			cookie_name = ".ASPXROLES";
			cookie_path = "/";
			cookie_protection = CookieProtection.All;
			cookie_sliding = true;
			cookie_timeout = 30;
			max_cached_result = 25;
		}


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
		
		public static string[] FindUsersInRole (string rolename, string usernameToMatch)
		{
			return Provider.FindUsersInRole (rolename, usernameToMatch);
		}
		
		public static string ApplicationName {
			get { return Provider.ApplicationName; }
			set { Provider.ApplicationName = value; }
		}
		
		public static bool CacheRolesInCookie {
			get { return cookie_cache_roles; }
		}
		
		public static string CookieName {
			get { return cookie_name; }
		}
		
		public static string CookiePath {
			get { return cookie_path; }
		}
		
		public static CookieProtection CookieProtectionValue {
			get { return cookie_protection; }
		}
		
		public static bool CookieRequireSSL {
			get { return cookie_ssl; }
		}
		
		public static bool CookieSlidingExpiration {
			get { return cookie_sliding; }
		}
		
		public static int CookieTimeout {
			get { return cookie_timeout; }
		}

		public static bool CreatePersistentCookie {
			get { return cookie_persistent; }
		}

		public static string Domain {
			get { return domain; }
		}

		public static bool Enabled {
			get { return (provider != null); }
		}

		public static int MaxCachedResults {
			get { return max_cached_result; }
		}
		
		[MonoTODO]
		public static RoleProvider Provider {
			get {
				CheckProvider ();
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public static RoleProviderCollection Providers {
			get {
				CheckProvider ();
				throw new NotImplementedException ();
			}
		}

		// private stuff

		private static void CheckProvider ()
		{
			if (!Enabled)
				throw new ProviderException ();
		}
	}
}

#endif
