//------------------------------------------------------------------------------
// <copyright file="RoleManagerSection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System;
    using System.Xml;
    using System.Configuration;
    using System.Collections.Specialized;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.ComponentModel;
    using System.Web.Security; // for CookieProtection Enum
    using System.Security.Permissions;

    /*         <!-- Configuration for roleManager:
                enabled="[true|false]"                            Feature is enabled?
                cacheRolesInCookie="[true|false]"                 Cache roles in cookie?
                cookieName=".ASPXROLES"                           Cookie Name
                createPersistentCookie="[true|false]"             Creates a persistent cookie or session cookie?
                cookieTimeout="30"                                Cookie Timeout
                cookiePath="/"                                    Cookie Path
                cookieRequireSSL="[true|false]"                   Set Secure bit in Cookie
                cookieSlidingExpiration="[true|false]"          Reissue expiring cookies?
                cookieProtection="[None|Validation|Encryption|All]"    How to protect cookies from being read/tampered
                defaultProvider="string"                          Name of provider to use by default
                domain="[domain]"                                 Enables output of the "domain" cookie attribute set to the specified value
                maxCachedResults="int"                            Maximum number of roles to cache in cookie
               Child nodes:
                <providers>              Providers (class must inherit from RoleProvider)

                    <add                 Add a provider
                        name="string"    Name to identify this provider instance by
                        type="string"   Class that implements RoleProvider
                        provider-specific-configuration />

                    <remove              Remove a provider
                        name="string" /> Name of provider to remove
                    <clear/>             Remove all providers
                </providers>


                <providers> type="TypeName"                        Class that inherits from System.Web.Security.RoleProvider
                    providerSpecificConfig                        Config for the provider


           Configuration for SqlRoleProvider and AccessRoleProvider:
                   connectionStringName="string"  Name corresponding to the entry in <connectionStrings> section where the connection string for the provider is specified
                   description="string"           Description of what the provider does
                   commandTimeout="int"           Command timeout value for SQL command

           Configuration for AuthorizationStoreProvider:
                   connectionStringName="string"  Name corresponding to the entry in <connectionStrings> section where the connection string for the provider is specified
                   description="string"           Description of what the provider does
                   cacheRefreshInterval="int"     The number of minutes between forced refreshes of the cached policy store data

           Configuration for WindowsTokenRoleProvider:
                 description="string"           Description of what the provider does
        -->


        <roleManager
                enabled="false" cacheRolesInCookie="false" cookieName=".ASPXROLES" cookieTimeout="30"
                cookiePath="/" cookieRequireSSL="false" cookieSlidingExpiration="true" createPersistentCookie="false"
                cookieProtection="All" defaultProvider="AspNetSqlRoleProvider" domain=""  >
           <providers>
               <add  name="AspNetSqlRoleProvider" type="System.Web.Security.SqlRoleProvider, System.Web, Version=%ASSEMBLY_VERSION%, Culture=neutral, PublicKeyToken=%MICROSOFT_PUBLICKEY%"
                     connectionStringName="LocalSqlServer"
                     applicationName="/"
                     description="Stores and retrieves roles data from the local Microsoft SQL Server database" />

               <add name="AspNetWindowsTokenRoleProvider"
                    type="System.Web.Security.WindowsTokenRoleProvider, System.Web, Version=%ASSEMBLY_VERSION%, Culture=neutral, PublicKeyToken=%MICROSOFT_PUBLICKEY%"
                    description="Retrieves roles data from the Windows authenticated token for the request" />


                <add name="AspNetAuthorizationStoreRoleProvider"
                    type="System.Web.Security.AuthorizationRoleProvider, System.Web, Version=%ASSEMBLY_VERSION%, Culture=neutral, PublicKeyToken=%MICROSOFT_PUBLICKEY%"
                    connectionStringName="AuthorizationStore"
                    cacheRefreshInterval="60"
                    applicationName="MyApplication"
                    scopeName="MyScope"
                    description="Stores and retrieves roles data from the authorization store" />
           </providers>

        </roleManager>
 */
    public sealed class RoleManagerSection : ConfigurationSection {
        private static ConfigurationPropertyCollection _properties;
        private static readonly ConfigurationProperty _propEnabled =
            new ConfigurationProperty("enabled",
                                        typeof(bool),
                                        false,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propUseCookies =
            new ConfigurationProperty("cacheRolesInCookie",
                                        typeof(bool),
                                        false,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propCookieName =
            new ConfigurationProperty("cookieName",
                                        typeof(string),
                                        ".ASPXROLES",
                                        StdValidatorsAndConverters.WhiteSpaceTrimStringConverter,
                                        StdValidatorsAndConverters.NonEmptyStringValidator,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propCookieTimeout =
            new ConfigurationProperty("cookieTimeout",
                                        typeof(TimeSpan),
                                        TimeSpan.FromMinutes(30.0),
                                        StdValidatorsAndConverters.TimeSpanMinutesOrInfiniteConverter,
                                        StdValidatorsAndConverters.PositiveTimeSpanValidator,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propCookiePath =
            new ConfigurationProperty("cookiePath",
                                        typeof(string),
                                        "/",
                                        StdValidatorsAndConverters.WhiteSpaceTrimStringConverter,
                                        StdValidatorsAndConverters.NonEmptyStringValidator,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propCookieRequireSSL =
            new ConfigurationProperty("cookieRequireSSL",
                                        typeof(bool),
                                        false,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propCookieSlidingExpiration =
            new ConfigurationProperty("cookieSlidingExpiration",
                                        typeof(bool),
                                        true,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propCookieProtection =
            new ConfigurationProperty("cookieProtection",
                                        typeof(CookieProtection),
                                        CookieProtection.All,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propDefaultProvider =
            new ConfigurationProperty("defaultProvider",
                                        typeof(string),
                                        "AspNetSqlRoleProvider",
                                        null,
                                        StdValidatorsAndConverters.NonEmptyStringValidator,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propProviders =
            new ConfigurationProperty("providers",
                                        typeof(ProviderSettingsCollection),
                                        null,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propCreatePersistentCookie =
            new ConfigurationProperty("createPersistentCookie",
                                        typeof(bool),
                                        false,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propDomain =
            new ConfigurationProperty("domain",
                                        typeof(string),
                                        null,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propMaxCachedResults =
            new ConfigurationProperty("maxCachedResults",
                                        typeof(int),
                                        25,
                                        ConfigurationPropertyOptions.None);

        private enum InheritedType {
            inNeither = 0,
            inParent = 1,
            inSelf = 2,
            inBothSame = 3,
            inBothDiff = 4,
        }

        static RoleManagerSection() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propEnabled);
            _properties.Add(_propUseCookies);
            _properties.Add(_propCookieName);
            _properties.Add(_propCookieTimeout);
            _properties.Add(_propCookiePath);
            _properties.Add(_propCookieRequireSSL);
            _properties.Add(_propCookieSlidingExpiration);
            _properties.Add(_propCookieProtection);
            _properties.Add(_propDefaultProvider);
            _properties.Add(_propProviders);
            _properties.Add(_propCreatePersistentCookie);
            _properties.Add(_propDomain);
            _properties.Add(_propMaxCachedResults);
        }

        public RoleManagerSection() {
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("enabled", DefaultValue = false)]
        public bool Enabled {
            get {
                return (bool)base[_propEnabled];
            }
            set {
                base[_propEnabled] = value;
            }
        }

        [ConfigurationProperty("createPersistentCookie", DefaultValue = false)]
        public bool CreatePersistentCookie {
            get {
                return (bool)base[_propCreatePersistentCookie];
            }
            set {
                base[_propCreatePersistentCookie] = value;
            }
        }

        [ConfigurationProperty("cacheRolesInCookie", DefaultValue = false)]
        public bool CacheRolesInCookie {
            get {
                return (bool)base[_propUseCookies];
            }
            set {
                base[_propUseCookies] = value;
            }
        }

        [ConfigurationProperty("cookieName", DefaultValue = ".ASPXROLES")]
        [TypeConverter(typeof(WhiteSpaceTrimStringConverter))]
        [StringValidator(MinLength = 1)]
        public string CookieName {
            get {
                return (string)base[_propCookieName];
            }
            set {
                base[_propCookieName] = value;
            }
        }

        [ConfigurationProperty("cookieTimeout", DefaultValue = "00:30:00")]
        [TypeConverter(typeof(TimeSpanMinutesOrInfiniteConverter))]
        [TimeSpanValidator(MinValueString="00:00:00", MaxValueString=TimeSpanValidatorAttribute.TimeSpanMaxValue)]
        public TimeSpan CookieTimeout {
            get {
                return (TimeSpan)base[_propCookieTimeout];
            }
            set {
                base[_propCookieTimeout] = value;
            }
        }

        [ConfigurationProperty("cookiePath", DefaultValue = "/")]
        [TypeConverter(typeof(WhiteSpaceTrimStringConverter))]
        [StringValidator(MinLength = 1)]
        public string CookiePath {
            get {
                return (string)base[_propCookiePath];
            }
            set {
                base[_propCookiePath] = value;
            }
        }

        [ConfigurationProperty("cookieRequireSSL", DefaultValue = false)]
        public bool CookieRequireSSL {
            get {
                return (bool)base[_propCookieRequireSSL];
            }
            set {
                base[_propCookieRequireSSL] = value;
            }
        }

        [ConfigurationProperty("cookieSlidingExpiration", DefaultValue = true)]
        public bool CookieSlidingExpiration {
            get {
                return (bool)base[_propCookieSlidingExpiration];
            }
            set {
                base[_propCookieSlidingExpiration] = value;
            }
        }

        [ConfigurationProperty("cookieProtection", DefaultValue = CookieProtection.All)]
        public CookieProtection CookieProtection {
            get {
                return (CookieProtection)base[_propCookieProtection];
            }
            set {
                base[_propCookieProtection] = value;
            }
        }

        [ConfigurationProperty("defaultProvider", DefaultValue = "AspNetSqlRoleProvider")]
        [TypeConverter(typeof(WhiteSpaceTrimStringConverter))]
        [StringValidator(MinLength = 1)]
        public string DefaultProvider {
            get {
                return (string)base[_propDefaultProvider];
            }
            set                     {
                        base[_propDefaultProvider] = value;
                    }
                }

                [ConfigurationProperty("providers")]
                public ProviderSettingsCollection Providers                 {
                    get                     {
                        return (ProviderSettingsCollection)base[_propProviders];
            }
        }

        [ConfigurationProperty("domain")]
        public string Domain {
            get {
                return (string)base[_propDomain];
            }
            set {
                base[_propDomain] = value;
            }
        }

        [ConfigurationProperty("maxCachedResults", DefaultValue = 25)]
        public int MaxCachedResults {
            get {
                return (int)base[_propMaxCachedResults];
            }
            set {
                base[_propMaxCachedResults] = value;
            }
        }
    } // class RoleManagerSection
}
