//------------------------------------------------------------------------------
// <copyright file="AppSettingsSection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration {
    using System;
    using System.Xml;
    using System.Configuration;
    using System.Collections.Specialized;
    using System.Collections;
    using System.IO;
    using System.Text;

    public sealed class AppSettingsSection : ConfigurationSection {
        private volatile static ConfigurationPropertyCollection  s_properties;
        private volatile static ConfigurationProperty            s_propAppSettings;
        private volatile static ConfigurationProperty            s_propFile;

        private KeyValueInternalCollection _KeyValueCollection = null;

        private static ConfigurationPropertyCollection EnsureStaticPropertyBag() {
            if (s_properties == null) {
                ConfigurationProperty propAppSettings = new ConfigurationProperty(null, typeof(KeyValueConfigurationCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);
                ConfigurationProperty propFile = new ConfigurationProperty("file", typeof(string), String.Empty, ConfigurationPropertyOptions.None);

                ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                properties.Add(propAppSettings);
                properties.Add(propFile);

                s_propAppSettings = propAppSettings;
                s_propFile = propFile;
                s_properties = properties;
            }

            return s_properties;
        }

        public AppSettingsSection() {
            EnsureStaticPropertyBag();
        }

        protected internal override ConfigurationPropertyCollection Properties {
            get {
                return EnsureStaticPropertyBag();
            }
        }

        protected internal override object GetRuntimeObject() {
            SetReadOnly();
            return this.InternalSettings;            // return the read only object
        }

        internal NameValueCollection InternalSettings {
            get {
                if (_KeyValueCollection == null) {
                    _KeyValueCollection = new KeyValueInternalCollection(this);
                }
                return (NameValueCollection)_KeyValueCollection;
            }
        }

        [ConfigurationProperty("", IsDefaultCollection = true)]
        public KeyValueConfigurationCollection Settings {
            get {
                return (KeyValueConfigurationCollection)base[s_propAppSettings];
            }
        }

        [ConfigurationProperty("file", DefaultValue = "")]
        public string File {
            get {
                string fileValue = (string)base[s_propFile];
                if (fileValue == null) {
                    return String.Empty;
                }
                return fileValue;
            }
            set {
                base[s_propFile] = value;
            }
        }
        protected internal override void Reset(ConfigurationElement parentSection) {
            _KeyValueCollection = null;
            base.Reset(parentSection);
            if (!String.IsNullOrEmpty((string)base[s_propFile])) { // don't inherit from the parent
                SetPropertyValue(s_propFile,null,true); // ignore the lock to prevent inheritence
            }
        }


        protected internal override bool IsModified() {
            return base.IsModified();
        }

        protected internal override string SerializeSection(ConfigurationElement parentElement, string name, ConfigurationSaveMode saveMode) {
            return base.SerializeSection(parentElement, name, saveMode);
        }

        protected internal override void DeserializeElement(XmlReader reader, bool serializeCollectionKey) {
            string ElementName = reader.Name;

            base.DeserializeElement(reader, serializeCollectionKey);
            if ((File != null) && (File.Length > 0)) {
                string sourceFileFullPath;
                string configFileDirectory;
                string configFile;

                // Determine file location
                configFile = ElementInformation.Source;

                if (String.IsNullOrEmpty(configFile)) {
                    sourceFileFullPath = File;
                }
                else {
                    configFileDirectory = System.IO.Path.GetDirectoryName(configFile);
                    sourceFileFullPath = System.IO.Path.Combine(configFileDirectory, File);
                }

                if (System.IO.File.Exists(sourceFileFullPath)) {
                    int lineOffset = 0;
                    string rawXml = null;

                    using (Stream sourceFileStream = new FileStream(sourceFileFullPath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                        using (XmlUtil xmlUtil = new XmlUtil(sourceFileStream, sourceFileFullPath, true)) {
                            if (xmlUtil.Reader.Name != ElementName) {
                                throw new ConfigurationErrorsException(
                                        SR.GetString(SR.Config_name_value_file_section_file_invalid_root, ElementName),
                                        xmlUtil);
                            }

                            lineOffset = xmlUtil.Reader.LineNumber;
                            rawXml = xmlUtil.CopySection();

                            // Detect if there is any XML left over after the section
                            while (!xmlUtil.Reader.EOF) {
                                XmlNodeType t = xmlUtil.Reader.NodeType;
                                if (t != XmlNodeType.Comment) {
                                    throw new ConfigurationErrorsException(SR.GetString(SR.Config_source_file_format), xmlUtil);
                                }

                                xmlUtil.Reader.Read();
                            }
                        }
                    }

                    ConfigXmlReader internalReader = new ConfigXmlReader(rawXml, sourceFileFullPath, lineOffset);
                    internalReader.Read();
                    if (internalReader.MoveToNextAttribute()) {
                        throw new ConfigurationErrorsException(SR.GetString(SR.Config_base_unrecognized_attribute, internalReader.Name), (XmlReader)internalReader);
                    }

                    internalReader.MoveToElement();

                    base.DeserializeElement(internalReader, serializeCollectionKey);
                }
            }
        }
    }
}

