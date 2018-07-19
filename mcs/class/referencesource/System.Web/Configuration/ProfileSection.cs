//------------------------------------------------------------------------------
// <copyright file="ProfileSection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System;
    using System.Xml;
    using System.Configuration;
    using System.Collections.Specialized;
    using System.Collections;
    using System.IO;
    using System.Text;
    using System.Web.Util;
    using System.Security.Permissions;

    /*          <!--
            Configuration for profile:
                enabled="[true|false]"   Feature is enabled?
                defaultProvider="string" Name of provider to use by default

                <providers>              Providers (class must inherit from ProfileProvider)

                    <add                 Add a provider
                        name="string"    Name to identify this provider instance by
                        type="string"   Class that implements ProfileProvider
                        provider-specific-configuration />

                    <remove              Remove a provider
                        name="string" /> Name of provider to remove
                    <clear/>             Remove all providers


                <properties>                Optional element. List of properties in the Profile system
                    <property            Properties in the profile
                            name="string"                 Name of the property
                            type="string"                 Optional. Type of the property. Default: string.
                            readOnly="[true|false]"       Optional. Is Value read-only. Default: false.
                            defaultValue="string"         Optional. Default Value. Default: Empty string.
                            allowAnonymous="[true|false]" Optional. Allow storing values for anonymous users. Default: false.
                            provider="string              Optional. Name of provider. Default: Default provider.
                            serializeAs=["String|Xml|Binary|ProviderSpecific"] Optional. How to serialize the type. Default: ProviderSpecific.
                        />


                    <group              Optional element. Group of properties: Note: groups can not nested
                        name="string"   Name of the group

                        <property       Property in the group
                            name="string"                 Name of the property
                            type="type-name"              Optional. Type of the property. Default: "string".
                            readOnly="[true|false]"       Optional. Is Value read-only. Default: false.
                            defaultValue="string"         Optional. Default Value. Default: Empty string.
                            allowAnonymous="[true|false]" Optional. Allow storing values for anonymous users. Default: false.
                            provider="string              Optional. Name of provider. Default: Default provider.
                            serializeAs=["String|Xml|Binary|ProviderSpecific"] Optional. How to serialize the type. Default: ProviderSpecific.
                        />
                    </group>
                </properties>

             Configuration for SqlProfileProvider:
                   connectionStringName="string"  Name corresponding to the entry in <connectionStrings> section where the connection string for the provider is specified
                   description="string"           Description of what the provider does
                   commandTimeout="int"           Command timeout value for SQL command

        -->


        <profile enabled="true" defaultProvider="AspNetSqlProfileProvider" inherits="System.Web.Profile.ProfileBase, System.Web, Version=%ASSEMBLY_VERSION%, Culture=neutral, PublicKeyToken=%MICROSOFT_PUBLICKEY%" >
           <providers>
              <add  name="AspNetSqlProfileProvider"
                    type="System.Web.Profile.SqlProfileProvider, System.Web, Version=%ASSEMBLY_VERSION%, Culture=neutral, PublicKeyToken=%MICROSOFT_PUBLICKEY%"
                    connectionStringName="LocalSqlServer"
                    applicationName="/"
                    description="Stores and retrieves profile data from the local Microsoft SQL Server database" />
           </providers>

           <properties>
                <!-- Add profile properties here. Example:
                       <property name="FriendlyName" type="string" />
                       <property name="Height"       type="int"    />
                       <property name="Weight"       type="int"    />
                -->
           </properties>
        </profile>
*/
    public sealed class ProfileSection : ConfigurationSection {
        private static ConfigurationPropertyCollection _properties;
        private static readonly ConfigurationProperty _propEnabled =
            new ConfigurationProperty("enabled",
                                        typeof(bool),
                                        true,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propDefaultProvider =
            new ConfigurationProperty("defaultProvider",
                                        typeof(string),
                                        "AspNetSqlProfileProvider",
                                        null,
                                        StdValidatorsAndConverters.NonEmptyStringValidator,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propProviders =
            new ConfigurationProperty("providers",
                                        typeof(ProviderSettingsCollection),
                                        null,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propProfile =
            new ConfigurationProperty("properties",
                                        typeof(RootProfilePropertySettingsCollection),
                                        null,
                                        ConfigurationPropertyOptions.IsDefaultCollection);
        private static readonly ConfigurationProperty _propInherits =
            new ConfigurationProperty("inherits",
                                        typeof(string),
                                        String.Empty,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propAutomaticSaveEnabled =
            new ConfigurationProperty("automaticSaveEnabled",
                                        typeof(bool),
                                        true,
                                        ConfigurationPropertyOptions.None);

        static ProfileSection() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propEnabled);
            _properties.Add(_propDefaultProvider);
            _properties.Add(_propProviders);
            _properties.Add(_propProfile);
            _properties.Add(_propInherits);
            _properties.Add(_propAutomaticSaveEnabled);
        }

        private long _recompilationHash;

        private bool _recompilationHashCached;

        internal long RecompilationHash {
            get {
                if (!_recompilationHashCached) {
                    _recompilationHash = CalculateHash();
                    _recompilationHashCached = true;
                }

                return _recompilationHash;
            }
        }

        private long CalculateHash() {
            HashCodeCombiner hashCombiner = new HashCodeCombiner();

            CalculateProfilePropertySettingsHash(PropertySettings, hashCombiner);

            if (PropertySettings != null) {
                foreach (ProfileGroupSettings pgs in PropertySettings.GroupSettings) {
                    hashCombiner.AddObject(pgs.Name);
                    CalculateProfilePropertySettingsHash(pgs.PropertySettings, hashCombiner);
                }
            }

            return hashCombiner.CombinedHash;
        }

        private void CalculateProfilePropertySettingsHash(
            ProfilePropertySettingsCollection settings,
            HashCodeCombiner hashCombiner) {
            foreach (ProfilePropertySettings pps in settings) {
                hashCombiner.AddObject(pps.Name);
                hashCombiner.AddObject(pps.Type);
            }
        }

        public ProfileSection() {
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("automaticSaveEnabled", DefaultValue = true)]
        public bool AutomaticSaveEnabled {
            get {
                return (bool)base[_propAutomaticSaveEnabled];
            }
            set {
                base[_propAutomaticSaveEnabled] = value;
            }
        }

        [ConfigurationProperty("enabled", DefaultValue = true)]
        public bool Enabled {
            get {
                return (bool)base[_propEnabled];
            }
            set {
                base[_propEnabled] = value;
            }
        }

        [ConfigurationProperty("defaultProvider", DefaultValue = "AspNetSqlProfileProvider")]
        [StringValidator(MinLength = 1)]
        public string DefaultProvider {
            get {
                return (string)base[_propDefaultProvider];
            }
            set {
                base[_propDefaultProvider] = value;
            }
        }

        [ConfigurationProperty("inherits", DefaultValue = "")]
        public string Inherits {
            get {
                return (string)base[_propInherits];
            }
            set {
                base[_propInherits] = value;
            }
        }

                [ConfigurationProperty("providers")]
                public ProviderSettingsCollection Providers                 {
                    get                     {
                        return (ProviderSettingsCollection)base[_propProviders];
                    }
                }

        // not exposed to the API
        [ConfigurationProperty("properties")]
        public RootProfilePropertySettingsCollection PropertySettings {
            get {
                return (RootProfilePropertySettingsCollection)base[_propProfile];
            }
        }
    }
}


