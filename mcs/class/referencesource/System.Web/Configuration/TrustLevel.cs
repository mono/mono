//------------------------------------------------------------------------------
// <copyright file="TrustLevel.cs" company="Microsoft">
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
    using System.ComponentModel;
    using System.Security.Permissions;

    // class SecurityPolicySection

    public sealed class TrustLevel : ConfigurationElement {
        private static ConfigurationPropertyCollection _properties;

        private static readonly ConfigurationProperty _propName =
            new ConfigurationProperty("name",
                                        typeof(string),
                                        "Full",
                                        null,
                                        StdValidatorsAndConverters.NonEmptyStringValidator,
                                        ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);
        
        private static readonly ConfigurationProperty _propPolicyFile =
            new ConfigurationProperty("policyFile", 
                                        typeof(string), 
                                        "internal", 
                                        ConfigurationPropertyOptions.IsRequired);

        private string _PolicyFileExpanded = null;
        private string _LegacyPolicyFileExpanded = null;

        static TrustLevel() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propName);
            _properties.Add(_propPolicyFile);
        }

        internal TrustLevel() {
        }

        public TrustLevel(String name, String policyFile) {
            Name = name;
            PolicyFile = policyFile;
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("name", IsRequired = true, DefaultValue = "Full", IsKey = true)]
        [StringValidator(MinLength = 1)]
        public string Name {
            get {
                return (string)base[_propName];
            }
            set {
                base[_propName] = value;
            }
        }

        [ConfigurationProperty("policyFile", IsRequired = true, DefaultValue = "internal")]
        public string PolicyFile {
            get {
                return (string)base[_propPolicyFile];
            }
            set {
                base[_propPolicyFile] = value;
            }
        }

        internal string PolicyFileExpanded {
            get {
                if (_PolicyFileExpanded == null) {
                    // 
                    string filename = ElementInformation.Properties["policyFile"].Source;
                    String strDir = filename.Substring(0, filename.LastIndexOf('\\') + 1);
                    bool fAppend = true; // Append filename to directory else return just filename
                    if (PolicyFile.Length > 1) {
                        char c1 = PolicyFile[1];
                        char c0 = PolicyFile[0];

                        if (c1 == ':') { // Absolute file path
                            fAppend = false;
                        }
                        else if (c0 == '\\' && c1 == '\\') { // UNC file path
                            fAppend = false;
                        }
                    }

                    if (fAppend) {
                        _PolicyFileExpanded = strDir + PolicyFile;
                    }
                    else {
                        _PolicyFileExpanded = PolicyFile;
                    }
                }
                return _PolicyFileExpanded;
            }
        }

        internal string LegacyPolicyFileExpanded {
            get {
                if (_LegacyPolicyFileExpanded == null) {
                    // 
                    string filename = ElementInformation.Properties["policyFile"].Source;
                    String strDir = filename.Substring(0, filename.LastIndexOf('\\') + 1);
                    bool fAppend = true; // Append filename to directory else return just filename
                    if (PolicyFile.Length > 1) {
                        char c1 = PolicyFile[1];
                        char c0 = PolicyFile[0];

                        if (c1 == ':') { // Absolute file path
                            fAppend = false;
                        }
                        else if (c0 == '\\' && c1 == '\\') { // UNC file path
                            fAppend = false;
                        }
                    }

                    if (fAppend) {
                        _LegacyPolicyFileExpanded = strDir + "legacy." + PolicyFile;
                    }
                    else {
                        _LegacyPolicyFileExpanded = PolicyFile;
                    }
                }
                return _LegacyPolicyFileExpanded;
            }
        }
    } // class TrustLevel
}
