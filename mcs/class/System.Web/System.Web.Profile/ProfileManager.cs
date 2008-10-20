//
// System.Web.Profile.ProfileManager.cs
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//	Vladimir Krasnov (vladimirk@mainsoft.com)
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
using System.Web.Configuration;
using System.Configuration;

namespace System.Web.Profile
{
	public static class ProfileManager
	{
#if TARGET_J2EE
		const string Profiles_config = "Profiles.config";
		const string Profiles_ProfileProviderCollection = "Profiles.ProfileProviderCollection";
		static ProfileSection config
		{
			get
			{
				object o = AppDomain.CurrentDomain.GetData (Profiles_config);
				if (o == null) {
					config = (ProfileSection) WebConfigurationManager.GetSection ("system.web/profile");
					return (ProfileSection) config;
				}

				return (ProfileSection) o;
			}
			set
			{
				AppDomain.CurrentDomain.SetData (Profiles_config, value);
			}
		}
		static ProfileProviderCollection providersCollection
		{
			get
			{
				object o = AppDomain.CurrentDomain.GetData (Profiles_ProfileProviderCollection);
				return (ProfileProviderCollection) o;
			}
			set
			{
				AppDomain.CurrentDomain.SetData (Profiles_ProfileProviderCollection, value);
			}
		}
#else
		static ProfileSection config;
		static ProfileProviderCollection providersCollection;

		static ProfileManager ()
		{
			config = (ProfileSection) WebConfigurationManager.GetSection ("system.web/profile");
		}
#endif

		public static int DeleteInactiveProfiles (ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
		{
			return Provider.DeleteInactiveProfiles (authenticationOption, userInactiveSinceDate);
		}

		public static bool DeleteProfile (string username)
		{
			return Provider.DeleteProfiles (new string [] { username }) > 0;
		}

		public static int DeleteProfiles (string[] usernames)
		{
			return Provider.DeleteProfiles (usernames);
		}

		public static int DeleteProfiles (ProfileInfoCollection profiles)
		{
			return Provider.DeleteProfiles (profiles);
		}

		public static ProfileInfoCollection FindInactiveProfilesByUserName (ProfileAuthenticationOption authenticationOption,
										    string usernameToMatch, DateTime userInactiveSinceDate)
		{
			int totalRecords = 0;
			return Provider.FindInactiveProfilesByUserName (authenticationOption, usernameToMatch, userInactiveSinceDate, 0, int.MaxValue, out totalRecords);
		}

		public static ProfileInfoCollection FindInactiveProfilesByUserName (ProfileAuthenticationOption authenticationOption,
										    string usernameToMatch, DateTime userInactiveSinceDate,
										    int pageIndex, int pageSize, out int totalRecords)
		{
			return Provider.FindInactiveProfilesByUserName (authenticationOption, usernameToMatch, userInactiveSinceDate, pageIndex, pageSize, out totalRecords);
		}

		public static ProfileInfoCollection FindProfilesByUserName (ProfileAuthenticationOption authenticationOption, string usernameToMatch)
		{
			int totalRecords = 0;
			return Provider.FindProfilesByUserName (authenticationOption, usernameToMatch, 0, int.MaxValue, out totalRecords);
		}

		public static ProfileInfoCollection FindProfilesByUserName (ProfileAuthenticationOption authenticationOption, string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
		{
			return Provider.FindProfilesByUserName (authenticationOption, usernameToMatch, pageIndex, pageSize, out totalRecords);
		}

		public static ProfileInfoCollection GetAllInactiveProfiles (ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
		{
			int totalRecords = 0;
			return Provider.GetAllInactiveProfiles (authenticationOption, userInactiveSinceDate, 0, int.MaxValue, out totalRecords);
		}

		public static ProfileInfoCollection GetAllInactiveProfiles (ProfileAuthenticationOption authenticationOption,
									    DateTime userInactiveSinceDate, int pageIndex, int pageSize,
									    out int totalRecords)
		{
			return Provider.GetAllInactiveProfiles (authenticationOption, userInactiveSinceDate, pageIndex, pageSize, out totalRecords);
		}

		public static ProfileInfoCollection GetAllProfiles (ProfileAuthenticationOption authenticationOption)
		{
			int totalRecords = 0;
			return Provider.GetAllProfiles (authenticationOption, 0, int.MaxValue, out totalRecords);
		}

		public static ProfileInfoCollection GetAllProfiles (ProfileAuthenticationOption authenticationOption, int pageIndex, int pageSize, out int totalRecords)
		{
			return Provider.GetAllProfiles (authenticationOption, pageIndex, pageSize, out totalRecords);
		}

		public static int GetNumberOfInactiveProfiles (ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
		{
			return Provider.GetNumberOfInactiveProfiles (authenticationOption, userInactiveSinceDate);
		}

		public static int GetNumberOfProfiles (ProfileAuthenticationOption authenticationOption)
		{
			int totalRecords = 0;
			Provider.GetAllProfiles (authenticationOption, 0, 1, out totalRecords);
			return totalRecords;
		}

		public static string ApplicationName {
			get {
				return Provider.ApplicationName;
			}
			set {
				Provider.ApplicationName = value;
			}
		}

		public static bool AutomaticSaveEnabled {
			get {
				return config.AutomaticSaveEnabled;
			}
		}

		public static bool Enabled {
			get {
				return config.Enabled;
			}
		}

		[MonoTODO ("check AspNetHostingPermissionLevel")]
		public static ProfileProvider Provider {
			get	{
				ProfileProvider p = Providers [config.DefaultProvider];
				if (p == null)
					throw new ConfigurationErrorsException ("Provider '" + config.DefaultProvider + "' was not found");
				return p;
			}
		}

		public static ProfileProviderCollection Providers {
			get {
				CheckEnabled ();
				if (providersCollection == null) {
					ProfileProviderCollection providersCollectionTmp = new ProfileProviderCollection ();
					ProvidersHelper.InstantiateProviders (config.Providers, providersCollectionTmp, typeof (ProfileProvider));
					providersCollection = providersCollectionTmp;
				}
				return providersCollection;
			}
		}

		static void CheckEnabled ()
		{
			if (!Enabled)
				throw new Exception ("This feature is not enabled.  To enable it, add <profile enabled=\"true\"> to your configuration file.");
		}

	}
}

#endif
