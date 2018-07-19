//------------------------------------------------------------------------------
// <copyright file="DefaultSection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration {
    using System.Xml;

    public sealed class DefaultSection : ConfigurationSection {
        private static volatile ConfigurationPropertyCollection  s_properties;

        string  _rawXml = string.Empty;
        bool    _isModified;

        private static ConfigurationPropertyCollection EnsureStaticPropertyBag() {
            if (s_properties == null) {
                ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                s_properties = properties;
            }

            return s_properties;
        }

        public DefaultSection() {
            EnsureStaticPropertyBag();
        }

        protected internal override ConfigurationPropertyCollection Properties {
            get {
                return EnsureStaticPropertyBag();
            }
        }

        protected internal override bool IsModified() {
            return _isModified;
        }

        protected internal override void ResetModified() {
            _isModified = false;
        }

        protected internal override void Reset(ConfigurationElement parentSection) {
            _rawXml = string.Empty;
            _isModified = false;
        }

        protected internal override void DeserializeSection(XmlReader xmlReader) {
            if (!xmlReader.Read() || xmlReader.NodeType != XmlNodeType.Element) {
                throw new ConfigurationErrorsException(SR.GetString(SR.Config_base_expected_to_find_element), xmlReader);
            }
            _rawXml = xmlReader.ReadOuterXml();
            _isModified = true;
        }

        protected internal override string SerializeSection(ConfigurationElement parentSection, string name, ConfigurationSaveMode saveMode) {
            return _rawXml;
        }
    }
}

