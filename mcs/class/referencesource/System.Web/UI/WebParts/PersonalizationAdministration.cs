//------------------------------------------------------------------------------
// <copyright file="PersonalizationStateInfoCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Globalization;
    using System.Web.Configuration;
    using System.Web.Hosting;
    using System.Web.Util;

    public static class PersonalizationAdministration {

        private static readonly object _initializationLock = new object();

        private static bool _initialized;
        private static PersonalizationProvider _provider;
        private static PersonalizationProviderCollection _providers;

        internal static readonly DateTime DefaultInactiveSinceDate = DateTime.MaxValue;
        private const int _defaultPageIndex = 0;
        private const int _defaultPageSize = Int32.MaxValue;

        public static string ApplicationName {
            get {
                return Provider.ApplicationName;
            }
            set {
                Provider.ApplicationName = value;
            }
        }

        public static PersonalizationProvider Provider {
            get {
                Initialize();
                return _provider;
            }
        }

        public static PersonalizationProviderCollection Providers {
            get {
                Initialize();
                return _providers;
            }
        }

        private static void Initialize() {
            HttpRuntime.CheckAspNetHostingPermission(AspNetHostingPermissionLevel.Low, SR.Feature_not_supported_at_this_level);

            if (_initialized) {
                return;
            }

            lock (_initializationLock) {
                if (_initialized) {
                    return;
                }

                WebPartsSection webPartsSection = RuntimeConfig.GetAppConfig().WebParts;
                WebPartsPersonalization personalization = webPartsSection.Personalization;

                Debug.Assert(_providers == null);
                _providers = new PersonalizationProviderCollection();

                ProvidersHelper.InstantiateProviders(personalization.Providers, _providers, typeof(PersonalizationProvider));
                _providers.SetReadOnly();
                _provider = _providers[personalization.DefaultProvider];
                if (_provider == null) {
                    throw new ConfigurationErrorsException(
                        SR.GetString(SR.Config_provider_must_exist, personalization.DefaultProvider),
                        personalization.ElementInformation.Properties["defaultProvider"].Source,
                        personalization.ElementInformation.Properties["defaultProvider"].LineNumber);
                }

                _initialized = true;
            }
        }

        public static int ResetAllState(PersonalizationScope scope) {
            PersonalizationProviderHelper.CheckPersonalizationScope(scope);
            return ResetStatePrivate(scope, null, null);
        }

        public static int ResetState(PersonalizationStateInfoCollection data) {
            int count = 0;
            PersonalizationProviderHelper.CheckNullEntries(data, "data");

            StringCollection sharedPaths = null;
            foreach (PersonalizationStateInfo stateInfo in data) {
                UserPersonalizationStateInfo userStateInfo = stateInfo as UserPersonalizationStateInfo;
                if (userStateInfo != null) {
                    if (ResetUserState(userStateInfo.Path, userStateInfo.Username)) {
                        count += 1;
                    }
                }
                else {
                    if (sharedPaths == null) {
                         sharedPaths = new StringCollection();
                    }
                    sharedPaths.Add(stateInfo.Path);
                }
            }

            if (sharedPaths != null) {
                string [] sharedPathsArray = new string [sharedPaths.Count];
                sharedPaths.CopyTo(sharedPathsArray, 0);
                count += ResetStatePrivate(PersonalizationScope.Shared, sharedPathsArray, null);
            }
            return count;
        }

        public static bool ResetSharedState(string path) {
            path = StringUtil.CheckAndTrimString(path, "path");
            string [] paths = new string[] {path};
            int count = ResetStatePrivate(PersonalizationScope.Shared, paths, null);
            Debug.Assert(count >= 0);
            if (count > 1) {
                throw new HttpException(SR.GetString(SR.PersonalizationAdmin_UnexpectedResetSharedStateReturnValue, count.ToString(CultureInfo.CurrentCulture)));
            }
            return (count == 1);
        }

        public static int ResetSharedState(string[] paths) {
            paths = PersonalizationProviderHelper.CheckAndTrimNonEmptyStringEntries(paths, "paths", true, false, -1);
            return ResetStatePrivate(PersonalizationScope.Shared, paths, null);
        }

        public static int ResetUserState(string path) {
            path = StringUtil.CheckAndTrimString(path, "path");
            string [] paths = new string [] {path};
            return ResetStatePrivate(PersonalizationScope.User, paths, null);
        }

        public static int ResetUserState(string[] usernames) {
            usernames = PersonalizationProviderHelper.CheckAndTrimNonEmptyStringEntries(usernames, "usernames", true, true, -1);
            return ResetStatePrivate(PersonalizationScope.User, null, usernames);
        }

        public static bool ResetUserState(string path, string username) {
            path = StringUtil.CheckAndTrimString(path, "path");
            username = PersonalizationProviderHelper.CheckAndTrimStringWithoutCommas(username, "username");
            string [] paths = new string [] {path};
            string [] usernames = new string[] {username};
            int count = ResetStatePrivate(PersonalizationScope.User, paths, usernames);
            Debug.Assert(count >= 0);
            if (count > 1) {
                throw new HttpException(SR.GetString(SR.PersonalizationAdmin_UnexpectedResetUserStateReturnValue, count.ToString(CultureInfo.CurrentCulture)));
            }
            return (count == 1);
        }

        public static int ResetUserState(string path, string[] usernames) {
            path = StringUtil.CheckAndTrimString(path, "path");
            usernames = PersonalizationProviderHelper.CheckAndTrimNonEmptyStringEntries(usernames, "usernames", true, true, -1);
            string [] paths = new string [] {path};
            return ResetStatePrivate(PersonalizationScope.User, paths, usernames);
        }

        // This private method assumes input parameters have been checked
        private static int ResetStatePrivate(PersonalizationScope scope, string[] paths, string[] usernames) {
            Initialize();
            int count = _provider.ResetState(scope, paths, usernames);
            PersonalizationProviderHelper.CheckNegativeReturnedInteger(count, "ResetState");
            return count;
        }

        public static int ResetInactiveUserState(DateTime userInactiveSinceDate) {
            return ResetInactiveUserStatePrivate(null, userInactiveSinceDate);
        }

        public static int ResetInactiveUserState(string path,
                                                 DateTime userInactiveSinceDate) {
            path = StringUtil.CheckAndTrimString(path, "path");
            return ResetInactiveUserStatePrivate(path, userInactiveSinceDate);
        }

        // This private method assumes input parameters have been checked
        private static int ResetInactiveUserStatePrivate(string path, DateTime userInactiveSinceDate) {
            Initialize();
            int count = _provider.ResetUserState(path, userInactiveSinceDate);
            PersonalizationProviderHelper.CheckNegativeReturnedInteger(count, "ResetUserState");
            return count;
        }

        public static int GetCountOfState(PersonalizationScope scope) {
            return GetCountOfState(scope, null);
        }

        public static int GetCountOfState(PersonalizationScope scope, string pathToMatch) {
            PersonalizationProviderHelper.CheckPersonalizationScope(scope);
            pathToMatch = StringUtil.CheckAndTrimString(pathToMatch, "pathToMatch", false);
            PersonalizationStateQuery stateQuery = new PersonalizationStateQuery();
            stateQuery.PathToMatch = pathToMatch;
            return GetCountOfStatePrivate(scope, stateQuery);
        }

        // This private method assumes input parameters have been checked
        private static int GetCountOfStatePrivate(PersonalizationScope scope,
                                                  PersonalizationStateQuery stateQuery) {
            Initialize();            
            int count = _provider.GetCountOfState(scope, stateQuery);
            PersonalizationProviderHelper.CheckNegativeReturnedInteger(count, "GetCountOfState");
            return count;
        }

        public static int GetCountOfUserState(string usernameToMatch) {
            usernameToMatch = StringUtil.CheckAndTrimString(usernameToMatch, "usernameToMatch", false);
            PersonalizationStateQuery stateQuery = new PersonalizationStateQuery();
            stateQuery.UsernameToMatch = usernameToMatch;
            return GetCountOfStatePrivate(PersonalizationScope.User, stateQuery);
        }

        public static int GetCountOfInactiveUserState(DateTime userInactiveSinceDate) {
            return GetCountOfInactiveUserState(null, userInactiveSinceDate);
        }

        public static int GetCountOfInactiveUserState(string pathToMatch,
                                                      DateTime userInactiveSinceDate) {
            pathToMatch = StringUtil.CheckAndTrimString(pathToMatch, "pathToMatch", false);
            PersonalizationStateQuery stateQuery = new PersonalizationStateQuery();
            stateQuery.PathToMatch = pathToMatch;
            stateQuery.UserInactiveSinceDate = userInactiveSinceDate;
            return GetCountOfStatePrivate(PersonalizationScope.User, stateQuery);
        }

        // This private method assumes input parameters have been checked
        private static PersonalizationStateInfoCollection FindStatePrivate(
                                                    PersonalizationScope scope,
                                                    PersonalizationStateQuery stateQuery,
                                                    int pageIndex,
                                                    int pageSize,
                                                    out int totalRecords) {
            Initialize();
            return _provider.FindState(scope, stateQuery, pageIndex, pageSize, out totalRecords);
        }

        public static PersonalizationStateInfoCollection GetAllState(PersonalizationScope scope) {
            int totalRecords;
            return GetAllState(scope, _defaultPageIndex, _defaultPageSize, out totalRecords);
        }

        public static PersonalizationStateInfoCollection GetAllState(PersonalizationScope scope,
                                                                     int pageIndex, int pageSize,
                                                                     out int totalRecords) {
            PersonalizationProviderHelper.CheckPersonalizationScope(scope);
            PersonalizationProviderHelper.CheckPageIndexAndSize(pageIndex, pageSize);
            return FindStatePrivate(scope, null, pageIndex, pageSize, out totalRecords);
        }

        public static PersonalizationStateInfoCollection GetAllInactiveUserState(DateTime userInactiveSinceDate) {
            int totalRecords;
            return GetAllInactiveUserState(userInactiveSinceDate, _defaultPageIndex, _defaultPageSize, out totalRecords);
        }

        public static PersonalizationStateInfoCollection GetAllInactiveUserState(DateTime userInactiveSinceDate,
                                                                                 int pageIndex, int pageSize,
                                                                                 out int totalRecords) {
            PersonalizationProviderHelper.CheckPageIndexAndSize(pageIndex, pageSize);
            PersonalizationStateQuery stateQuery = new PersonalizationStateQuery();
            stateQuery.UserInactiveSinceDate = userInactiveSinceDate;
            return FindStatePrivate(PersonalizationScope.User, stateQuery, pageIndex, pageSize, out totalRecords);
        }

        public static PersonalizationStateInfoCollection FindSharedState(string pathToMatch) {
            int totalRecords;
            return FindSharedState(pathToMatch, _defaultPageIndex, _defaultPageSize, out totalRecords);
        }

        public static PersonalizationStateInfoCollection FindSharedState(string pathToMatch,
                                                                         int pageIndex, int pageSize,
                                                                         out int totalRecords) {
            pathToMatch = StringUtil.CheckAndTrimString(pathToMatch, "pathToMatch", false);
            PersonalizationProviderHelper.CheckPageIndexAndSize(pageIndex, pageSize);
            PersonalizationStateQuery stateQuery = new PersonalizationStateQuery();
            stateQuery.PathToMatch= pathToMatch;
            return FindStatePrivate(PersonalizationScope.Shared, stateQuery, pageIndex, pageSize, out totalRecords);
        }

        public static PersonalizationStateInfoCollection FindUserState(string pathToMatch,
                                                                       string usernameToMatch) {
            int totalRecords;
            return FindUserState(pathToMatch, usernameToMatch, _defaultPageIndex,
                                 _defaultPageSize, out totalRecords);
        }

        public static PersonalizationStateInfoCollection FindUserState(string pathToMatch,
                                                                       string usernameToMatch,
                                                                       int pageIndex, int pageSize,
                                                                       out int totalRecords) {
            pathToMatch = StringUtil.CheckAndTrimString(pathToMatch, "pathToMatch", false);
            usernameToMatch = StringUtil.CheckAndTrimString(usernameToMatch, "usernameToMatch", false);
            PersonalizationProviderHelper.CheckPageIndexAndSize(pageIndex, pageSize);
            PersonalizationStateQuery stateQuery = new PersonalizationStateQuery();
            stateQuery.PathToMatch= pathToMatch;
            stateQuery.UsernameToMatch = usernameToMatch;
            return FindStatePrivate(PersonalizationScope.User, stateQuery, pageIndex,
                                    pageSize, out totalRecords);
        }

        public static PersonalizationStateInfoCollection FindInactiveUserState(string pathToMatch,
                                                                               string usernameToMatch,
                                                                               DateTime userInactiveSinceDate) {
            int totalRecords;
            return FindInactiveUserState(pathToMatch, usernameToMatch, userInactiveSinceDate,
                                         _defaultPageIndex, _defaultPageSize, out totalRecords);
        }

        public static PersonalizationStateInfoCollection FindInactiveUserState(string pathToMatch,
                                                                               string usernameToMatch,
                                                                               DateTime userInactiveSinceDate,
                                                                               int pageIndex, int pageSize,
                                                                               out int totalRecords) {
            pathToMatch = StringUtil.CheckAndTrimString(pathToMatch, "pathToMatch", false);
            usernameToMatch = StringUtil.CheckAndTrimString(usernameToMatch, "usernameToMatch", false);
            PersonalizationProviderHelper.CheckPageIndexAndSize(pageIndex, pageSize);
            PersonalizationStateQuery stateQuery = new PersonalizationStateQuery();
            stateQuery.PathToMatch= pathToMatch;
            stateQuery.UsernameToMatch = usernameToMatch;
            stateQuery.UserInactiveSinceDate = userInactiveSinceDate;
            return FindStatePrivate(PersonalizationScope.User, stateQuery, pageIndex,
                                    pageSize, out totalRecords);
        }
    }
}

