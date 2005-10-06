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

namespace System.Web.Security {
	public sealed class Roles {
		
		[MonoTODO]
		public static void AddUsersToRole (string [] usernames, string rolename)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static void AddUsersToRoles (string [] usernames, string [] rolenames)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static void AddUserToRole (string username, string rolename)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static void AddUserToRoles (string username, string [] rolenames)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static void CreateRole (string rolename)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static void DeleteCookie ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static void DeleteRole (string rolename)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static string [] GetAllRoles ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static string [] GetRolesForUser ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static string [] GetRolesForUser (string username)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static string [] GetUsersInRole (string rolename)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static bool IsUserInRole (string rolename)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static bool IsUserInRole (string username, string rolename)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static void RemoveUserFromRole (string username, string rolename)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static void RemoveUserFromRoles (string username, string [] rolenames)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static void RemoveUsersFromRole (string [] usernames, string rolename)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static void RemoveUsersFromRoles (string [] usernames, string [] rolenames)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static bool RoleExists (string rolename)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static string ApplicationName {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
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

