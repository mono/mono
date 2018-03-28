//------------------------------------------------------------------------------
// <copyright file="ProfileGroupSettings.cs" company="Microsoft">
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

    public sealed class ProfileGroupSettings : ConfigurationElement {
        private static ConfigurationPropertyCollection _properties;
        private static readonly ConfigurationProperty _propName =
            new ConfigurationProperty("name",
                                        typeof(string),
                                        null,
                                        null,
                                        ProfilePropertyNameValidator.SingletonInstance,
                                        ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);
        private static readonly ConfigurationProperty _propProperties =
            new ConfigurationProperty(null, 
                                        typeof(ProfilePropertySettingsCollection), 
                                        null, 
                                        ConfigurationPropertyOptions.IsDefaultCollection);

        static ProfileGroupSettings() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propName);
            _properties.Add(_propProperties);
        }

        internal void InternalDeserialize(XmlReader reader, bool serializeCollectionKey) {
            DeserializeElement(reader, serializeCollectionKey);
        }

        internal ProfileGroupSettings() {
        }

        public ProfileGroupSettings(string name) {
            base[_propName] = name;
        }

        public override bool Equals(object obj) {
            ProfileGroupSettings o = obj as ProfileGroupSettings;
            return (o != null && Name == o.Name && Object.Equals(PropertySettings, o.PropertySettings));
        }

        public override int GetHashCode() {
            return Name.GetHashCode() ^ PropertySettings.GetHashCode();
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public string Name {
            get {
                return (string)base[_propName];
            }
        }

        [ConfigurationProperty("", IsDefaultCollection = true)]
        public ProfilePropertySettingsCollection PropertySettings {
            get {
                return (ProfilePropertySettingsCollection)base[_propProperties];
            }
        }

        internal void InternalReset(ProfileGroupSettings parentSettings) {
            base.Reset(parentSettings);
        }

        internal void InternalUnmerge(ConfigurationElement sourceElement, ConfigurationElement parentElement,
                                ConfigurationSaveMode saveMode) {
            base.Unmerge(sourceElement, parentElement, saveMode); // Base merge to get locks merged
        }

    } // class ProfileGroupSettings
}
