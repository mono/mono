//
// System.Web.Profile.ProfileManager.cs
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Web;

namespace System.Web.Profile
{
	public static class ProfileManager
	{
		[MonoTODO]
		public static int DeleteInactiveProfiles (ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static bool DeleteProfile (string username)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static int DeleteProfiles (string[] usernames)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static int DeleteProfiles (ProfileInfoCollection profiles)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static ProfileInfoCollection FindInactiveProfilesByUserName (ProfileAuthenticationOption authenticationOption,
										    string usernameToMatch, DateTime userInactiveSinceDate)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static ProfileInfoCollection FindInactiveProfilesByUserName (ProfileAuthenticationOption authenticationOption,
										    string usernameToMatch, DateTime userInactiveSinceDate,
										    int pageIndex, int pageSize, out int totalRecords)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static ProfileInfoCollection FindProfilesByUserName (ProfileAuthenticationOption authenticationOption,
									    string usernameToMatch)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static ProfileInfoCollection FindProfilesByUserName (ProfileAuthenticationOption authenticationOption,
									    string usernameToMatch, int pageIndex, int pageSize,
									    out int totalRecords)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static ProfileInfoCollection GetAllInactiveProfiles (ProfileAuthenticationOption authenticationOption,
									    DateTime userInactiveSinceDate)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static ProfileInfoCollection GetAllInactiveProfiles (ProfileAuthenticationOption authenticationOption,
									    DateTime userInactiveSinceDate, int pageIndex, int pageSize,
									    out int totalRecords)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static ProfileInfoCollection GetAllProfiles (ProfileAuthenticationOption authenticationOption)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static ProfileInfoCollection GetAllProfiles (ProfileAuthenticationOption authenticationOption,
								    int pageIndex, int pageSize, out int totalRecords)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static int GetNumberOfInactiveProfiles (ProfileAuthenticationOption authenticationOption,
							       DateTime userInactiveSinceDate)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static int GetNumberOfProfiles (ProfileAuthenticationOption authenticationOption)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static string ApplicationName {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public static bool AutomaticSaveEnabled {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public static bool Enabled {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public static ProfileProvider Provider {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public static ProfileProviderCollection Providers {
			get {
				throw new NotImplementedException ();
			}
		}
	}
}

#endif
