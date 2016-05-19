//------------------------------------------------------------------------------
// <copyright file="ProfileManager.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
/*
 * ProfileManager
 *
 * Copyright (c) 2002 Microsoft Corporation
 */

namespace System.Web.Profile
{
    using  System.Security.Principal;
    using  System.Security.Permissions;
    using  System.Collections;
    using  System.Collections.Specialized;
    using  System.Web.Configuration;
    using  System.Web.Util;
    using  System.Web.Security;
    using  System.Web.Compilation;
    using  System.Configuration;
    using  System.Configuration.Provider;
    using  System.Reflection;
    using  System.CodeDom;
    using System.Web.Hosting;

    public static class ProfileManager
    {
        private static ProfilePropertySettingsCollection s_dynamicProperties = new ProfilePropertySettingsCollection();
        internal static ProfilePropertySettingsCollection DynamicProfileProperties {
            get {
                return s_dynamicProperties;
            }
        }

        public static void AddDynamicProfileProperty(ProfilePropertySettings property) {
            BuildManager.ThrowIfPreAppStartNotRunning();
            s_dynamicProperties.Add(property);
        }

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        public static bool DeleteProfile(string username)
        {
            SecUtility.CheckParameter( ref username, true, true, true, 0, "username" );
            return (Provider.DeleteProfiles(new string [] {username}) != 0);
        }

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        public static int DeleteProfiles(ProfileInfoCollection profiles)
        {
            if( profiles == null )
            {
                throw new ArgumentNullException( "profiles" );
            }

            if ( profiles.Count < 1 )
            {
                throw new ArgumentException(
                    SR.GetString(SR.Parameter_collection_empty,
                        "profiles" ),
                    "profiles" );
            }

            foreach (ProfileInfo pi in profiles) {
                string username = pi.UserName;
                SecUtility.CheckParameter(ref username, true, true, true, 0, "UserName");
            }
            return Provider.DeleteProfiles(profiles);
        }

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        public static int DeleteProfiles(string[] usernames)
        {
            SecUtility.CheckArrayParameter( ref usernames,
                                            true,
                                            true,
                                            true,
                                            0,
                                            "usernames");


            return Provider.DeleteProfiles(usernames);
        }

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        public static int DeleteInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
        {
            return Provider.DeleteInactiveProfiles(authenticationOption, userInactiveSinceDate);
        }

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        public static int GetNumberOfProfiles(ProfileAuthenticationOption authenticationOption)
        {
            return Provider.GetNumberOfInactiveProfiles(authenticationOption, DateTime.Now.AddDays(1)); // 
        }

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        public static int GetNumberOfInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
        {
            return Provider.GetNumberOfInactiveProfiles(authenticationOption, userInactiveSinceDate);
        }


        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        public static ProfileInfoCollection GetAllProfiles(ProfileAuthenticationOption authenticationOption)
        {
            int totalRecords;
            return Provider.GetAllProfiles(authenticationOption, 0, Int32.MaxValue, out totalRecords);
        }


        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        public static ProfileInfoCollection GetAllProfiles(ProfileAuthenticationOption authenticationOption,
                                                           int pageIndex,
                                                           int pageSize,
                                                           out int totalRecords)
        {
            return Provider.GetAllProfiles(authenticationOption, pageIndex, pageSize, out totalRecords);
        }


        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        public static ProfileInfoCollection GetAllInactiveProfiles(ProfileAuthenticationOption authenticationOption,
                                                                   DateTime userInactiveSinceDate)
        {
            int totalRecords;
            return Provider.GetAllInactiveProfiles(authenticationOption, userInactiveSinceDate, 0, Int32.MaxValue, out totalRecords);
        }


        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        public static ProfileInfoCollection GetAllInactiveProfiles(ProfileAuthenticationOption authenticationOption,
                                                                   DateTime userInactiveSinceDate,
                                                                   int pageIndex,
                                                                   int pageSize,
                                                                   out int totalRecords)
        {
            return Provider.GetAllInactiveProfiles(authenticationOption, userInactiveSinceDate, pageIndex, pageSize, out totalRecords);
        }

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        public static ProfileInfoCollection FindProfilesByUserName(ProfileAuthenticationOption authenticationOption,
                                                                   string usernameToMatch)
        {
            SecUtility.CheckParameter( ref usernameToMatch,
                                       true,
                                       true,
                                       false,
                                       0,
                                       "usernameToMatch" );

            int totalRecords;
            return Provider.FindProfilesByUserName(authenticationOption, usernameToMatch, 0, Int32.MaxValue, out totalRecords);
        }

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        public static ProfileInfoCollection FindProfilesByUserName (ProfileAuthenticationOption authenticationOption,
                                                                    string usernameToMatch,
                                                                    int pageIndex,
                                                                    int pageSize,
                                                                    out int totalRecords)
        {
            if ( pageIndex < 0 )
            {
                throw new ArgumentException(SR.GetString(SR.PageIndex_bad), "pageIndex");
            }

            if ( pageSize < 1 )
            {
                throw new ArgumentException(SR.GetString(SR.PageSize_bad), "pageSize");
            }

            SecUtility.CheckParameter( ref usernameToMatch,
                                       true,
                                       true,
                                       false,
                                       0,
                                       "usernameToMatch" );

            return Provider.FindProfilesByUserName(authenticationOption, usernameToMatch, pageIndex, pageSize, out totalRecords);
        }


        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        public static ProfileInfoCollection FindInactiveProfilesByUserName(ProfileAuthenticationOption authenticationOption,
                                                                           string usernameToMatch,
                                                                           DateTime userInactiveSinceDate)
        {
            SecUtility.CheckParameter( ref usernameToMatch,
                                       true,
                                       true,
                                       false,
                                       0,
                                       "usernameToMatch" );

            int totalRecords;
            return Provider.FindInactiveProfilesByUserName(authenticationOption, usernameToMatch, userInactiveSinceDate, 0, Int32.MaxValue, out totalRecords);
        }

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        public static ProfileInfoCollection FindInactiveProfilesByUserName(ProfileAuthenticationOption authenticationOption,
                                                                           string usernameToMatch,
                                                                           DateTime userInactiveSinceDate,
                                                                           int pageIndex,
                                                                           int pageSize,
                                                                           out int totalRecords)
        {
            if ( pageIndex < 0 )
            {
                throw new ArgumentException(SR.GetString(SR.PageIndex_bad), "pageIndex");
            }

            if ( pageSize < 1 )
            {
                throw new ArgumentException(SR.GetString(SR.PageSize_bad), "pageSize");
            }

            SecUtility.CheckParameter( ref usernameToMatch,
                                       true,
                                       true,
                                       false,
                                       0,
                                       "usernameToMatch" );

            return Provider.FindInactiveProfilesByUserName(authenticationOption, usernameToMatch, userInactiveSinceDate, pageIndex, pageSize, out totalRecords);
        }



        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        // Properties

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        public static bool Enabled {
            get {
                // 
                if (!s_Initialized && !s_InitializedEnabled) {
                    InitializeEnabled(false);
                }

                return s_Enabled;
            }
        }

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        public static string ApplicationName {
            get {
                return Provider.ApplicationName;
            }
            set {
                Provider.ApplicationName = value;
            }
        }

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        public static bool AutomaticSaveEnabled {
            get {
                HttpRuntime.CheckAspNetHostingPermission(AspNetHostingPermissionLevel.Low, SR.Feature_not_supported_at_this_level);
                // WOS #1544130: Don't initialize providers when getting this property, because it is called in ProfileModule.Init
                InitializeEnabled(false);
                return s_AutomaticSaveEnabled;
            }
        }
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        public static ProfileProvider Provider {
            get {
                HttpRuntime.CheckAspNetHostingPermission(AspNetHostingPermissionLevel.Low, SR.Feature_not_supported_at_this_level);
                Initialize(true);
                if (s_Provider == null) {
                    throw new InvalidOperationException(SR.GetString(SR.Profile_default_provider_not_found));
                }
                return s_Provider;
            }
        }

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        public static ProfileProviderCollection Providers {
            get {
                HttpRuntime.CheckAspNetHostingPermission(AspNetHostingPermissionLevel.Low, SR.Feature_not_supported_at_this_level);
                Initialize(true);
                return s_Providers;
            }
        }

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        // Private stuff

        private static void InitializeEnabled(bool initProviders) {
            if (!s_Initialized || !s_InitializedProviders || !s_InitializeDefaultProvider) {
                lock (s_Lock) {
                    if (!s_Initialized || !s_InitializedProviders || !s_InitializeDefaultProvider) {
                        try {
                            ProfileSection config = MTConfigUtil.GetProfileAppConfig();
                            if (!s_InitializedEnabled) {
                                s_Enabled = config.Enabled && HttpRuntime.HasAspNetHostingPermission(AspNetHostingPermissionLevel.Low);
                                s_AutomaticSaveEnabled = s_Enabled && config.AutomaticSaveEnabled;
                                s_InitializedEnabled = true;
                            }
                            if (initProviders && s_Enabled && (!s_InitializedProviders || !s_InitializeDefaultProvider)) {
                                InitProviders(config);
                            }
                        }
                        catch (Exception e) {
                            s_InitException = e;
                        }

                        s_Initialized = true;
                    }
                }
            }
        }

        private static void Initialize(bool throwIfNotEnabled)
        {
            InitializeEnabled(true);
            if (s_InitException != null)
                throw s_InitException;
            if (throwIfNotEnabled && !s_Enabled)
                throw new ProviderException(SR.GetString(SR.Profile_not_enabled));
        }

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        static private void InitProviders(ProfileSection config)
        {
            if (!s_InitializedProviders) {
                s_Providers = new ProfileProviderCollection();
                if (config.Providers != null) {
                    ProvidersHelper.InstantiateProviders(config.Providers, s_Providers, typeof(ProfileProvider));
                }
                s_InitializedProviders = true;
            }

            bool canInitializeDefaultProvider = (!HostingEnvironment.IsHosted || BuildManager.PreStartInitStage == PreStartInitStage.AfterPreStartInit);
            if (!s_InitializeDefaultProvider && canInitializeDefaultProvider) {
                s_Providers.SetReadOnly();

                if (config.DefaultProvider == null)
                    throw new ProviderException(SR.GetString(SR.Profile_default_provider_not_specified));

                s_Provider = (ProfileProvider)s_Providers[config.DefaultProvider];
                if (s_Provider == null)
                    throw new ConfigurationErrorsException(SR.GetString(SR.Profile_default_provider_not_found), config.ElementInformation.Properties["providers"].Source, config.ElementInformation.Properties["providers"].LineNumber);

                s_InitializeDefaultProvider = true;
            }
        }

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        private static ProfileProvider             s_Provider;
        private static ProfileProviderCollection   s_Providers;
        private static bool                        s_Enabled;
        private static bool                        s_Initialized;
        private static bool                        s_InitializedProviders;
        private static bool                        s_InitializeDefaultProvider;
        private static object                      s_Lock = new object();
        private static Exception                   s_InitException;
        private static bool                        s_InitializedEnabled;
        private static bool                        s_AutomaticSaveEnabled;
    }
}
