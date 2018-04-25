//------------------------------------------------------------------------------
// <copyright file="XhtmlConformanceSection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {

    using System.Configuration;
    using System.Security.Permissions;

/*
        <!--
        xhtmlConformance Attributes:
            mode="[Transitional|Legacy|Strict]"
                            - Supports switching between Xhtml 1.0 transitional standard,
                              Xhtml 1.0 strict standard and legacy rendering which reverts
                              significant rendering changes made to conform to the Xhtml 1.1
                              standards to their version 1.1 rendering. Default is "Transitional"
                              to ensure conformance with the Xhtml 1.0 transitional standards
                              while continue to render the name attribute on the form element.
                              Note: Only rendering changes likely to break existing applications
                              will be reverted to their old behavior. There is no mechanism to
                              revert all changes made to conform to the Xhtml specification.
        -->
*/
    public sealed class XhtmlConformanceSection : ConfigurationSection {
        private static ConfigurationPropertyCollection _properties;

        internal const XhtmlConformanceMode DefaultMode = XhtmlConformanceMode.Transitional;

        private static readonly ConfigurationProperty _propMode = 
            new ConfigurationProperty("mode", 
                                      typeof(XhtmlConformanceMode), 
                                      DefaultMode, 
                                      ConfigurationPropertyOptions.None);

        static XhtmlConformanceSection() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propMode);
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("mode", DefaultValue=DefaultMode)]
        public XhtmlConformanceMode Mode {
            get {
                return (XhtmlConformanceMode)base[_propMode];
            }
            set {
                base[_propMode] = value;
            }
        }
    }
}
