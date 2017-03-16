//
// System.Web.Security.Roles
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	Sebastien Pouliot  <sebastien@ximian.com>
//	Chris Toshok  <toshok@ximian.com>
//
// (C) 2003 Ben Maurer
// Copyright (c) 2005,2006 Novell, Inc (http://www.novell.com)
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
using System.Web.Configuration;
using System.Configuration;

namespace System.Web.Security {

	public static class Roles {
		static RoleManagerSection config;
		static RoleProviderCollection providersCollection;

		static Roles ()
		{
			config = (RoleManagerSection)WebConfigurationManager.GetSection ("system.web/roleManager");
		}


		public static void AddUsersToRole (string [] usernames, string roleName)
		{
			Provider.AddUsersToRoles (usernames, new string[] {roleName});
		}
		
		public static void AddUsersToRoles (string [] usernames, string [] roleNames)
		{
			Provider.AddUsersToRoles (usernames, roleNames);
		}
		
		public static void AddUserToRole (string username, string roleName)
		{
			Provider.AddUsersToRoles (new string[] {username}, new string[] {roleName});
		}
		
		public static void AddUserToRoles (string username, string [] roleNames)
		{
			Provider.AddUsersToRoles (new string[] {username}, roleNames);
		}
		
		public static void CreateRole (string roleName)
		{
			Provider.CreateRole (roleName);
		}
		
		public static void DeleteCookie ()
		{
			if (CacheRolesInCookie) {
				HttpContext context = HttpContext.Current;
				if (context == null)
					throw new HttpException ("Context is null.");

				HttpResponse response = context.Response;
				if (response == null)
					throw new HttpException ("Response is null.");

				HttpCookieCollection cc = response.Cookies;
				cc.Remove (CookieName);
				HttpCookie expiration_cookie = new HttpCookie (CookieName, "");
				expiration_cookie.Expires = new DateTime (1999, 10, 12);
				expiration_cookie.Path = CookiePath;
				cc.Add (expiration_cookie);
			}
		}
		
		public static bool DeleteRole (string roleName)
		{
			return Provider.DeleteRole (roleName, true);
		}
		
		public static bool DeleteRole (string roleName, bool throwOnPopulatedRole)
		{
			return Provider.DeleteRole (roleName, throwOnPopulatedRole);
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
		
		public static string [] GetUsersInRole (string roleName)
		{
			return Provider.GetUsersInRole (roleName);
		}
		
		public static bool IsUserInRole (string roleName)
		{
			return IsUserInRole (CurrentUser, roleName);
		}
		
		public static bool IsUserInRole (string username, string roleName)
		{
			if (String.IsNullOrEmpty (username))
				return false;
			return Provider.IsUserInRole (username, roleName);
		}
		
		public static void RemoveUserFromRole (string username, string roleName)
		{
			Provider.RemoveUsersFromRoles (new string[] {username}, new string[] {roleName});
		}
		
		public static void RemoveUserFromRoles (string username, string [] roleNames)
		{
			Provider.RemoveUsersFromRoles (new string[] {username}, roleNames);
		}
		
		public static void RemoveUsersFromRole (string [] usernames, string roleName)
		{
			Provider.RemoveUsersFromRoles (usernames, new string[] {roleName});
		}
		
		public static void RemoveUsersFromRoles (string [] usernames, string [] roleNames)
		{
			Provider.RemoveUsersFromRoles (usernames, roleNames);
		}
		
		public static bool RoleExists (string roleName)
		{
			return Provider.RoleExists (roleName);
		}
		
		public static string[] FindUsersInRole (string roleName, string usernameToMatch)
		{
			return Provider.FindUsersInRole (roleName, usernameToMatch);
		}
		
		public static string ApplicationName {
			get { return Provider.ApplicationName; }
			set { Provider.ApplicationName = value; }
		}
		
		public static bool CacheRolesInCookie {
			get { return config.CacheRolesInCookie; }
		}
		
		public static string CookieName {
			get { return config.CookieName; }
		}
		
		public static string CookiePath {
			get { return config.CookiePath; }
		}
		
		public static CookieProtection CookieProtectionValue {
			get { return config.CookieProtection; }
		}
		
		public static bool CookieRequireSSL {
			get { return config.CookieRequireSSL; }
		}
		
		public static bool CookieSlidingExpiration {
			get { return config.CookieSlidingExpiration; }
		}
		
		public static int CookieTimeout {
			get { return (int)config.CookieTimeout.TotalMinutes; }
		}

		public static bool CreatePersistentCookie {
			get { return config.CreatePersistentCookie; }
		}

		public static string Domain {
			get { return config.Domain; }
		}

		public static bool Enabled {
			get { return config.Enabled; }
			set { config.Enabled = value; }
		}

		public static int MaxCachedResults {
			get { return config.MaxCachedResults; }
		}
		
		public static RoleProvider Provider {
			get {
				RoleProvider p = Providers [config.DefaultProvider];
				if (p == null)
					throw new ConfigurationErrorsException ("Default Role Provider could not be found: Cannot instantiate provider: '" + config.DefaultProvider + "'.");
				return p;
			}
		}
		
		public static RoleProviderCollection Providers {
			get {
				CheckEnabled ();
				if (providersCollection == null) {
					RoleProviderCollection providersCollectionTmp = new RoleProviderCollection ();
					ProvidersHelper.InstantiateProviders (config.Providers, providersCollectionTmp, typeof (RoleProvider));
					providersCollection = providersCollectionTmp;
				}
				return providersCollection;
			}
		}

		// private stuff
		static void CheckEnabled ()
		{
			if (!Enabled)
				throw new ProviderException ("This feature is not enabled.  To enable it, add <roleManager enabled=\"true\"> to your configuration file.");
		}
	}
}

