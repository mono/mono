//------------------------------------------------------------------------------
// <copyright file="MembershipSection.cs" company="Microsoft">
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
    using System.Security.Permissions;

    /*         <!-- membership config:
              Attributes:
                  defaultProvider="string"                    Name of provider to use by default
                  userIsOnlineTimeWindow="int"                Time window (in minutes) to consider a User as being Online after since last activity
                  hashAlgorithmType="[SHA1|SHA512|MD5|...]"   Any valid hash algorithm supported by .NET framework, default is SHA1
              Child nodes:
                <providers>              Providers (class must inherit from MembershipProvider)
                    <add                 Add a provider
                        name="string"    Name to identify this provider instance by
                        type="string"    Class that implements MembershipProvider
                        provider-specific-configuration />

                    <remove              Remove a provider
                        name="string" /> Name of provider to remove
                    <clear/>             Remove all providers
                </providers>

                Configuration for SqlMembershipProvider and AccessMembershipProvider:
                   connectionStringName="string"              Name corresponding to the entry in <connectionStrings> section where the connection string for the provider is specified
                   maxInvalidPasswordAttempts="int"           A user's account is locked out when the number of failed password answer attempts matches the value of the configuration setting
                   passwordAttemptWindow="int"                The time window, in minutes, during which failed password attempts and failed password answer attempts are tracked
                   enablePasswordRetrieval="[true|false]"     Should the provider support password retrievals
                   enablePasswordReset="[true|false]"         Should the provider support password resets
                   requiresQuestionAndAnswer="[true|false]"   Should the provider require Q & A, the default is true
                   applicationName="string"                   Optional string to identity the application: defaults to Application Metabase path
                   requiresUniqueEmail="[true|false]"         Should the provider require a unique email to be specified
                   passwordFormat="[Clear|Hashed|Encrypted]"  Storage format for the password: Hashed (SHA1), Clear or Encrypted (Triple-DES)
                   description="string"                       Description of what the provider does
                   commandTimeout="int"                       Command timeout value for SQL command
                   minRequiredPasswordLength="int"            The minimum number of characters required in a password
                   minRequiredNonAlphanumericCharacters="int" The minimum number of non-alphanumeric characters that are required in a password
                   passwordStrengthRegularExpression="string" The regular expression used to test the password strength
                   passwordStrengthRegexTimeout="int"         The timeout in milliseconds for the regex we use to check password strength 
        -->

        <membership defaultProvider="AspNetSqlMembershipProvider" userIsOnlineTimeWindow="15" >
            <providers>
                <add name="AspNetSqlMembershipProvider"
                    type="System.Web.Security.SqlMembershipProvider, System.Web, Version=%ASSEMBLY_VERSION%, Culture=neutral, PublicKeyToken=%MICROSOFT_PUBLICKEY%"
                    connectionStringName="LocalSqlServer"
                    maxInvalidPasswordAttempts="5"
                    passwordAttemptWindow="10"
                    minRequiredPasswordLength="7"
                    minRequireNonAlphanumericCharacters="1"
                    passwordStrengthRegularExpression=""
                    passwordStrengthRegexTimeout="2000"
                    enablePasswordRetrieval="false"
                    enablePasswordReset="true"
                    requiresQuestionAndAnswer="true"
                    applicationName="/"
                    requiresUniqueEmail="false"
                    passwordFormat="Hashed"
                    description="Stores and retrieves membership data from the local Microsoft SQL Server database"
                />
            </providers>
        </membership>
 */

    public sealed class MembershipSection : ConfigurationSection {
        private static ConfigurationPropertyCollection _properties;
        private static readonly ConfigurationProperty _propProviders;
        private static readonly ConfigurationProperty _propDefaultProvider;
        private static readonly ConfigurationProperty _propUserIsOnlineTimeWindow;
        private static readonly ConfigurationProperty _propHashAlgorithmType;

        static MembershipSection() {
            // Property initialization
            _propProviders = new ConfigurationProperty("providers", typeof(ProviderSettingsCollection), null, ConfigurationPropertyOptions.None);
            _propDefaultProvider =
                new ConfigurationProperty("defaultProvider",
                                            typeof(string),
                                            "AspNetSqlMembershipProvider",
                                            null,
                                            StdValidatorsAndConverters.NonEmptyStringValidator,
                                            ConfigurationPropertyOptions.None);
            _propUserIsOnlineTimeWindow =
                new ConfigurationProperty("userIsOnlineTimeWindow",
                                            typeof(TimeSpan),
                                            TimeSpan.FromMinutes(15.0),
                                            StdValidatorsAndConverters.TimeSpanMinutesConverter,
                                            new TimeSpanValidator(TimeSpan.FromMinutes(1), TimeSpan.MaxValue),
                                            ConfigurationPropertyOptions.None);
            _propHashAlgorithmType = new ConfigurationProperty("hashAlgorithmType", typeof(string), string.Empty, ConfigurationPropertyOptions.None);

            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propProviders);
            _properties.Add(_propDefaultProvider);
            _properties.Add(_propUserIsOnlineTimeWindow);
            _properties.Add(_propHashAlgorithmType);
        }

        public MembershipSection() {
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("providers")]
        public ProviderSettingsCollection Providers {
            get {
                return (ProviderSettingsCollection)base[_propProviders];
            }
        }

        [ConfigurationProperty("defaultProvider", DefaultValue = "AspNetSqlMembershipProvider")]
        [StringValidator(MinLength = 1)]
        public string DefaultProvider {
            get {
                return (string)base[_propDefaultProvider];
            }
            set {
                base[_propDefaultProvider] = value;
            }
        }

        [ConfigurationProperty("hashAlgorithmType", DefaultValue = "")]
        public string HashAlgorithmType {
            get {
                return (string)base[_propHashAlgorithmType];
            }
            set {
                base[_propHashAlgorithmType] = value;
            }
        }

        internal void ThrowHashAlgorithmException() {
            throw new ConfigurationErrorsException(SR.GetString(SR.Invalid_hash_algorithm_type, HashAlgorithmType), ElementInformation.Properties["hashAlgorithmType"].Source, ElementInformation.Properties["hashAlgorithmType"].LineNumber);
        }

        [ConfigurationProperty("userIsOnlineTimeWindow", DefaultValue = "00:15:00")]
        [TypeConverter(typeof(TimeSpanMinutesConverter))]
        [TimeSpanValidator(MinValueString = "00:01:00", MaxValueString = TimeSpanValidatorAttribute.TimeSpanMaxValue)]
        public TimeSpan UserIsOnlineTimeWindow {
            get {
                return (TimeSpan)base[_propUserIsOnlineTimeWindow];
            }
            set {
                base[_propUserIsOnlineTimeWindow] = value;
            }
        }
    } // class MembershipSection
}
