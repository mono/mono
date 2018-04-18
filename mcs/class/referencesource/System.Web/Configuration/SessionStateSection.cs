//------------------------------------------------------------------------------
// <copyright file="SessionStateSection.cs" company="Microsoft">
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
    using System.Web.SessionState;
    using System.Diagnostics;
    using System.Security.Permissions;

    /*         <!-- sessionState Attributes:
                mode="[Off|InProc|StateServer|SQLServer|Custom]"
                stateConnectionString="tcpip=server:port"
                stateNetworkTimeout="timeout for network operations with State Server, in seconds"
                sqlConnectionString="valid System.Data.SqlClient.SqlConnection string, minus Initial Catalog"
                sqlCommandTimeout="timeout for SQL commands sent to SQL Server, in seconds"
                sqlConnectionRetryInterval="the interval the SQL State provider will retry opening connections and executing SQL commands when fatal errors occur, in seconds"
                customProvider="name of the custom provider"
                cookieless="[true|false|UseCookies|UseUri|AutoDetect|UseDeviceProfile]"
                cookieName="To override the default cookie name used for storing session ID"
                allowCustomSqlDatabase="[true|false]" - If true, the user can specify the Initial Catalog value in sqlConnectionString
                compressionEnabled="[true|false]"
                timeout="timeout in minutes"
                partitionResolverType="[fully qualified type of partition resolver]"
                useHostingIdentity="[true|false]"
                sessionIDManagerType="[fully qualified type of session ID Manager]"

              Child nodes:
                <providers>              Custom store providers (class must inherit SessionStateStoreProviderBase)
                    <add                 Add a provider
                        name="string"    Name to identify this provider instance by
                        type="string"    Class that implements ISessionStateStore
                        provider-specific-configuration />

                    <remove              Remove a provider
                        name="string" /> Name of provider to remove
                    <clear/>             Remove all providers
                </providers>
        -->
        <sessionState
            mode="InProc"
            stateConnectionString="tcpip=loopback:42424"
            stateNetworkTimeout="10"
            sqlConnectionString="data source=localhost;Integrated Security=SSPI"
            sqlCommandTimeout="30"
            customProvider=""
            cookieless="false"
            allowCustomSqlDatabase="false"
            compressionEnabled="false"
            regenerateExpiredSessionId="false"
            timeout="20"
        >
            <providers>
            </providers>
        </sessionState>

 */
    public sealed class SessionStateSection : ConfigurationSection {
        private static readonly ConfigurationElementProperty s_elemProperty =
            new ConfigurationElementProperty(new CallbackValidator(typeof(SessionStateSection), Validate));


        private static ConfigurationPropertyCollection _properties;

        private static readonly ConfigurationProperty _propMode =
            new ConfigurationProperty("mode",
                                        typeof(SessionStateMode),
                                        SessionStateModule.MODE_DEFAULT,
                                        ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _propStateConnectionString =
            new ConfigurationProperty("stateConnectionString",
                                        typeof(string),
                                        SessionStateModule.STATE_CONNECTION_STRING_DEFAULT,
                                        ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _propStateNetworkTimeout =
            new ConfigurationProperty("stateNetworkTimeout",
                                        typeof(TimeSpan),
#if FEATURE_PAL // FEATURE_PAL does not enable OutOfProcSessionStore
                                        TimeSpan.FromSeconds(600),
#else // FEATURE_PAL
                                        TimeSpan.FromSeconds((long)
                                            OutOfProcSessionStateStore.STATE_NETWORK_TIMEOUT_DEFAULT),
#endif // FEATURE_PAL
                                        StdValidatorsAndConverters.TimeSpanSecondsOrInfiniteConverter,
                                        StdValidatorsAndConverters.PositiveTimeSpanValidator,
                                        ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _propSqlConnectionString =
            new ConfigurationProperty("sqlConnectionString",
                                        typeof(string),
#if FEATURE_PAL // FEATURE_PAL does not enable SessionStateModule
                                        "data source=localhost;Integrated Security=SSPI",
#else // FEATURE_PAL
                                        SessionStateModule.SQL_CONNECTION_STRING_DEFAULT,
#endif // FEATURE_PAL
                                        ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _propSqlCommandTimeout =
            new ConfigurationProperty("sqlCommandTimeout",
                                        typeof(TimeSpan),
#if FEATURE_PAL // FEATURE_PAL does not enable SqlSessionStateStore
                                        TimeSpan.FromSeconds(1800),
#else // FEATURE_PAL
                                        TimeSpan.FromSeconds((long)
                                            SqlSessionStateStore.SQL_COMMAND_TIMEOUT_DEFAULT),
#endif // FEATURE_PAL
                                        StdValidatorsAndConverters.TimeSpanSecondsOrInfiniteConverter,
                                        null,
                                        ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _propSqlConnectionRetryInterval =
                    new ConfigurationProperty("sqlConnectionRetryInterval",
                                                typeof(TimeSpan),
                                                TimeSpan.FromSeconds(0),
                                                StdValidatorsAndConverters.TimeSpanSecondsOrInfiniteConverter,
                                                null,
                                                ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _propCustomProvider =
            new ConfigurationProperty("customProvider",
                                        typeof(string),
                                        String.Empty,
                                        ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _propCookieless =
            new ConfigurationProperty("cookieless",
                                        typeof(string),
                                        SessionIDManager.COOKIEMODE_DEFAULT.ToString(),
                                        ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _propCookieName =
            new ConfigurationProperty("cookieName",
                                        typeof(string),
                                        SessionIDManager.SESSION_COOKIE_DEFAULT,
                                        ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _propTimeout =
            new ConfigurationProperty("timeout",
                                        typeof(TimeSpan),
                                        TimeSpan.FromMinutes((long)SessionStateModule.TIMEOUT_DEFAULT),
                                        StdValidatorsAndConverters.TimeSpanMinutesOrInfiniteConverter,
                                        new TimeSpanValidator(TimeSpan.FromMinutes(1), TimeSpan.MaxValue),
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propAllowCustomSqlDatabase =
            new ConfigurationProperty("allowCustomSqlDatabase",
                                        typeof(bool),
                                        false,
                                        ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _propCompressionEnabled =
            new ConfigurationProperty("compressionEnabled",
                                        typeof(bool),
                                        false,
                                        ConfigurationPropertyOptions.None);


        //        private static readonly ConfigurationProperty _propLockAttributes =
        //            new ConfigurationProperty("lockAttributes",
        //                                    typeof(string),
        //                                    "",
        //                                    ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _propProviders =
            new ConfigurationProperty("providers",
                                        typeof(ProviderSettingsCollection),
                                        null,
                                        ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _propRegenerateExpiredSessionId =
            new ConfigurationProperty("regenerateExpiredSessionId",
                                        typeof(bool),
                                        true,
                                        ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _propPartitionResolverType =
            new ConfigurationProperty("partitionResolverType",
                                        typeof(string),
                                        String.Empty,
                                        ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _propUseHostingIdentity =
            new ConfigurationProperty("useHostingIdentity",
                                        typeof(bool),
                                        true,
                                        ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _propSessionIDManagerType =
            new ConfigurationProperty("sessionIDManagerType",
                                        typeof(string),
                                        String.Empty,
                                        ConfigurationPropertyOptions.None);

        private HttpCookieMode cookielessCache = SessionIDManager.COOKIEMODE_DEFAULT;
        private bool cookielessCached = false;
        private bool regenerateExpiredSessionIdCache = false;
        private bool regenerateExpiredSessionIdCached = false;
        static SessionStateSection() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propMode);
            _properties.Add(_propStateConnectionString);
            _properties.Add(_propStateNetworkTimeout);
            _properties.Add(_propSqlConnectionString);
            _properties.Add(_propSqlCommandTimeout);
            _properties.Add(_propSqlConnectionRetryInterval);
            _properties.Add(_propCustomProvider);
            _properties.Add(_propCookieless);
            _properties.Add(_propCookieName);
            _properties.Add(_propTimeout);
            _properties.Add(_propAllowCustomSqlDatabase);
            _properties.Add(_propCompressionEnabled);
            //            _properties.Add(_propLockAttributes);
            _properties.Add(_propProviders);
            _properties.Add(_propRegenerateExpiredSessionId);
            _properties.Add(_propPartitionResolverType);
            _properties.Add(_propUseHostingIdentity);
            _properties.Add(_propSessionIDManagerType);
        }

        public SessionStateSection() {
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("mode", DefaultValue = SessionStateModule.MODE_DEFAULT)]
        public SessionStateMode Mode {
            get {
                return (SessionStateMode)base[_propMode];
            }
            set {
                base[_propMode] = value;
            }
        }

        [ConfigurationProperty("stateConnectionString", DefaultValue = SessionStateModule.STATE_CONNECTION_STRING_DEFAULT)]
        public string StateConnectionString {
            get {
                return (string)base[_propStateConnectionString];
            }
            set {
                base[_propStateConnectionString] = value;
            }
        }

        [ConfigurationProperty("stateNetworkTimeout", DefaultValue = "00:00:10")]
        [TypeConverter(typeof(TimeSpanSecondsOrInfiniteConverter))]
        public TimeSpan StateNetworkTimeout {
            get {
                return (TimeSpan)base[_propStateNetworkTimeout];
            }
            set {
                base[_propStateNetworkTimeout] = value;
            }
        }

        [ConfigurationProperty("sqlConnectionString", DefaultValue = SessionStateModule.SQL_CONNECTION_STRING_DEFAULT)]
        public string SqlConnectionString {
            get {
                return (string)base[_propSqlConnectionString];
            }
            set {
                base[_propSqlConnectionString] = value;
            }
        }

        [ConfigurationProperty("sqlCommandTimeout", DefaultValue = "00:00:30")]
        [TypeConverter(typeof(TimeSpanSecondsOrInfiniteConverter))]
        public TimeSpan SqlCommandTimeout {
            get {
                return (TimeSpan)base[_propSqlCommandTimeout];
            }
            set {
                base[_propSqlCommandTimeout] = value;
            }
        }

        [ConfigurationProperty("sqlConnectionRetryInterval", DefaultValue = "00:00:00")]
        [TypeConverter(typeof(TimeSpanSecondsOrInfiniteConverter))]
        public TimeSpan SqlConnectionRetryInterval {
            get {
                return (TimeSpan)base[_propSqlConnectionRetryInterval];
            }
            set {
                base[_propSqlConnectionRetryInterval] = value;
            }
        }


        [ConfigurationProperty("customProvider", DefaultValue = "")]
        public string CustomProvider {
            get {
                return (string)base[_propCustomProvider];
            }
            set {
                base[_propCustomProvider] = value;
            }
        }

        [ConfigurationProperty("cookieless")]
        public HttpCookieMode Cookieless {
            get {
                if (cookielessCached == false) {
                    cookielessCache = ConvertToCookieMode((string)base[_propCookieless]);
                    cookielessCached = true;
                }
                return cookielessCache;
            }
            set {
                base[_propCookieless] = value.ToString();
                cookielessCache = value;
            }
        }

        [ConfigurationProperty("cookieName", DefaultValue = SessionIDManager.SESSION_COOKIE_DEFAULT)]
        public string CookieName {
            get {
                return (string)base[_propCookieName];
            }
            set {
                base[_propCookieName] = value;
            }
        }

        [ConfigurationProperty("timeout", DefaultValue = "00:20:00")]
        [TypeConverter(typeof(TimeSpanMinutesOrInfiniteConverter))]
        [TimeSpanValidator(MinValueString = "00:01:00", MaxValueString = TimeSpanValidatorAttribute.TimeSpanMaxValue)]
        public TimeSpan Timeout {
            get {
                return (TimeSpan)base[_propTimeout];
            }
            set {
                base[_propTimeout] = value;
            }
        }

        [ConfigurationProperty("allowCustomSqlDatabase", DefaultValue = false)]
        public bool AllowCustomSqlDatabase {
            get {
                return (bool)base[_propAllowCustomSqlDatabase];
            }
            set {
                base[_propAllowCustomSqlDatabase] = value;
            }
        }

        [ConfigurationProperty("compressionEnabled", DefaultValue = false)]
        public bool CompressionEnabled{
            get {
                return (bool)base[_propCompressionEnabled];
            }
            set {
                base[_propCompressionEnabled] = value;
            }
        }

        [ConfigurationProperty("regenerateExpiredSessionId", DefaultValue = true)]
        public bool RegenerateExpiredSessionId {
            get {
                if (regenerateExpiredSessionIdCached == false) {
                    regenerateExpiredSessionIdCache = (bool)base[_propRegenerateExpiredSessionId];
                    regenerateExpiredSessionIdCached = true;
                }
                return regenerateExpiredSessionIdCache;
            }
            set {
                base[_propRegenerateExpiredSessionId] = value;
                regenerateExpiredSessionIdCache = value;
            }
        }


#if DONTCOMPILE
        [ConfigurationProperty("lockAttributes", DefaultValue = "")]
        public string LockAttributes {
            get {
                return (string)base[_propLockAttributes];
            }
            set {
                // base.LockedAttributes.SetFromList(value); // keep the internal list in sync
                base[_propLockAttributes] = value;
            }
        }
#endif

        [ConfigurationProperty("providers")]
        public ProviderSettingsCollection Providers {
            get {
                return (ProviderSettingsCollection)base[_propProviders];
            }
        }

        [ConfigurationProperty("partitionResolverType", DefaultValue = "")]
        public string PartitionResolverType {
            get {
                return (string)base[_propPartitionResolverType];
            }
            set {
                base[_propPartitionResolverType] = value;
            }
        }

        [ConfigurationProperty("useHostingIdentity", DefaultValue = true)]
        public bool UseHostingIdentity {
            get {
                return (bool)base[_propUseHostingIdentity];
            }
            set {
                base[_propUseHostingIdentity] = value;
            }
        }

        [ConfigurationProperty("sessionIDManagerType", DefaultValue = "")]
        public string SessionIDManagerType {
            get {
                return (string)base[_propSessionIDManagerType];
            }
            set {
                base[_propSessionIDManagerType] = value;
            }
        }


        HttpCookieMode ConvertToCookieMode(string s) {
            if (s == "true") {
                return HttpCookieMode.UseUri;
            }
            else if (s == "false") {
                return HttpCookieMode.UseCookies;
            }
            else {
                int iTemp = 0;
                Type enumType = typeof(HttpCookieMode);

                if (Enum.IsDefined(enumType, s)) {
                    iTemp = (int)Enum.Parse(enumType, s);
                }
                else {
                    // if not null and not defined throw error
                    string names = "true, false";
                    foreach (string name in Enum.GetNames(enumType)) {
                        if (names == null) {
                            names = name;
                        }
                        else {
                            names += ", " + name;
                        }
                    }

                    throw new ConfigurationErrorsException(
                        SR.GetString(SR.Invalid_enum_attribute, "cookieless", names),
                        ElementInformation.Properties["cookieless"].Source,
                        ElementInformation.Properties["cookieless"].LineNumber);
                }

                return (HttpCookieMode)iTemp;
            }
        }

        protected override void PostDeserialize() {
            ConvertToCookieMode((string)base[_propCookieless]);
        }

        protected override ConfigurationElementProperty ElementProperty {
            get {
                return s_elemProperty;
            }
        }

        private static void Validate(object value) {
            if (value == null) {
                throw new ArgumentNullException("sessionState");
            }
            Debug.Assert(value is SessionStateSection);

            SessionStateSection elem = (SessionStateSection)value;

            if (elem.Timeout.TotalMinutes > SessionStateModule.MAX_CACHE_BASED_TIMEOUT_MINUTES &&
                (elem.Mode == SessionStateMode.InProc ||
                elem.Mode == SessionStateMode.StateServer)) {
                throw new ConfigurationErrorsException(
                    SR.GetString(SR.Invalid_cache_based_session_timeout),
                    elem.ElementInformation.Properties["timeout"].Source,
                    elem.ElementInformation.Properties["timeout"].LineNumber);
            }
        }
    }
}
