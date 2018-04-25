//------------------------------------------------------------------------------
// <copyright file="ApplicationServiceHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.ApplicationServices {
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Configuration;
    using System.Globalization;
    using System.Security.Principal;
    using System.Threading;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.Profile;
    using System.Web.Resources;

    internal static class ApplicationServiceHelper {
        // store profile properties allowed for get/set over the webservice
        // a dictionary is used for perf, as .ContainsKey is called often
        // These dictionaries are used for concurrent reads, but all writes are done on a new instance one per thread
        // and isn't available for reading from other threads until the operation is complete.
        // So it is safe to use Dictionary<K,V> in this case.
        // We use Dictionary<string, object> instead of Dictionary<string, bool> to avoid violating
        // FxCop Rule CA908: UseApprovedGenericsForPrecompiledAssemblies.
        private static Dictionary<string, object> _profileAllowedGet;
        private static Dictionary<string, object> _profileAllowedSet;
        private static bool? _profileServiceEnabled;
        private static bool? _roleServiceEnabled;
        private static bool? _authServiceEnabled;
        private static bool _authRequiresSSL;

        internal static Dictionary<string, object> ProfileAllowedGet {
            get {
                EnsureProfileConfigLoaded();
                return _profileAllowedGet;
            }
        }

        internal static Dictionary<string, object> ProfileAllowedSet {
            get {
                EnsureProfileConfigLoaded();
                return _profileAllowedSet;
            }
        }

        internal static bool AuthenticationServiceEnabled {
            get {
                EnsureAuthenticationConfigLoaded();
                return _authServiceEnabled.Value;
            }
        }
        
        internal static bool ProfileServiceEnabled {
            get {
                EnsureProfileConfigLoaded();
                return _profileServiceEnabled.Value;
            }
        }

        internal static bool RoleServiceEnabled {
            get {
                // Get the flag on demand from config
                if (_roleServiceEnabled == null) {
                    ScriptingRoleServiceSection roleServiceSection = ScriptingRoleServiceSection.GetConfigurationSection();
                    _roleServiceEnabled = (roleServiceSection != null) && roleServiceSection.Enabled;
                }

                return _roleServiceEnabled.Value;
            }
        }

        internal static void EnsureAuthenticated(HttpContext context) {
            // 

            bool authenticated = false;
            IPrincipal user = GetCurrentUser(context);

            if (user != null) {
                IIdentity userIdentity = user.Identity;
                if (userIdentity != null) {
                    authenticated = userIdentity.IsAuthenticated;
                }
            }

            if (!authenticated) {
                throw new HttpException(AtlasWeb.UserIsNotAuthenticated);
            }
        }

        private static void EnsureAuthenticationConfigLoaded() {
            // DevDiv 52730: drop the unnecessary double checked lock
            if (_authServiceEnabled == null) {
                ScriptingAuthenticationServiceSection authServicesSection = ScriptingAuthenticationServiceSection.GetConfigurationSection();

                if (authServicesSection != null) {
                    _authRequiresSSL = authServicesSection.RequireSSL;
                    _authServiceEnabled = authServicesSection.Enabled;
                }
                else {
                    _authServiceEnabled = false;
                }
            }
        }

        // Fail if the Authentication Service is disabled or this is a non-ssl request and ssl is required
        internal static void EnsureAuthenticationServiceEnabled(HttpContext context, bool enforceSSL) {
            if (!AuthenticationServiceEnabled) {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, AtlasWeb.AppService_Disabled, "AuthenticationService"));
            }

            if (enforceSSL && _authRequiresSSL && !context.Request.IsSecureConnection) {
                throw new HttpException(403, AtlasWeb.AppService_RequiredSSL);
            }
        }

        private static void EnsureProfileConfigLoaded() {
            if (_profileServiceEnabled == null) {
#pragma warning disable 0436
                ScriptingProfileServiceSection profileServiceSection = ScriptingProfileServiceSection.GetConfigurationSection();
#pragma warning restore 0436
                Dictionary<string, object> readAccessProperties = null;
                Dictionary<string, object> writeAccessProperties = null;

                bool enabled = (profileServiceSection != null) && profileServiceSection.Enabled;

                if (enabled) {
                    string[] enabledForRead = profileServiceSection.ReadAccessProperties;

                    if (enabledForRead != null && enabledForRead.Length > 0) {
                        readAccessProperties = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                        ParseProfilePropertyList(readAccessProperties, enabledForRead);
                    }

                    string[] enabledForWriting = profileServiceSection.WriteAccessProperties;

                    if (enabledForWriting != null && enabledForWriting.Length > 0) {
                        writeAccessProperties = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                        ParseProfilePropertyList(writeAccessProperties, enabledForWriting);
                    }
                }

                _profileAllowedGet = readAccessProperties;
                _profileAllowedSet = writeAccessProperties;
                _profileServiceEnabled = enabled;
            }
        }

        // Fail if the Profile Service is disabled
        internal static void EnsureProfileServiceEnabled() {
            if (!ProfileServiceEnabled) {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, AtlasWeb.AppService_Disabled, "ProfileService"));
            }
        }

        // Fail if the Role Service is disabled
        internal static void EnsureRoleServiceEnabled() {
            if (!RoleServiceEnabled) {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, AtlasWeb.AppService_Disabled, "RoleService"));
            }
        }

        internal static IPrincipal GetCurrentUser(HttpContext context) {
            return (context != null) ? context.User : Thread.CurrentPrincipal;
        }

        internal static Collection<ProfilePropertyMetadata> GetProfilePropertiesMetadata() {
            EnsureProfileConfigLoaded();

            if (ProfileBase.Properties == null) {
                return new Collection<ProfilePropertyMetadata>();
            }

            Collection<ProfilePropertyMetadata> metadatas = new Collection<ProfilePropertyMetadata>();

            foreach (SettingsProperty property in ProfileBase.Properties) {
                string propertyName = property.Name;

                // only return property metadata for properties that are allowed for Reading and/or Writing
                bool allowedReadOrWrite = _profileAllowedGet.ContainsKey(propertyName) || _profileAllowedSet.ContainsKey(propertyName);
                if (!allowedReadOrWrite) {
                    continue;
                }

                string defaultValue = null;

                if (property.DefaultValue != null) {
                    if (property.DefaultValue is string) {
                        defaultValue = (string)property.DefaultValue;
                    }
                    else {
                        defaultValue = Convert.ToBase64String((byte[])property.DefaultValue);
                    }
                }

                ProfilePropertyMetadata metadata = new ProfilePropertyMetadata();
                metadata.PropertyName = propertyName;
                metadata.DefaultValue = defaultValue;
                metadata.TypeName = property.PropertyType.AssemblyQualifiedName;
                metadata.AllowAnonymousAccess = (bool)property.Attributes["AllowAnonymous"];
                metadata.SerializeAs = (int)property.SerializeAs;
                metadata.IsReadOnly = property.IsReadOnly;

                metadatas.Add(metadata);
            }

            return metadatas;
        }

        internal static string GetUserName(IPrincipal user) {
            if (user == null || user.Identity == null) {
                return String.Empty;
            }
            else {
                return user.Identity.Name;
            }
        }

        private static void ParseProfilePropertyList(Dictionary<string, object> dictionary, string[] properties) {
            foreach (string property in properties) {
                string trimmed = property == null ? String.Empty : property.Trim();
                if (property.Length > 0) {
                    dictionary[trimmed] = true;
                }
            }
        }

    }
}
