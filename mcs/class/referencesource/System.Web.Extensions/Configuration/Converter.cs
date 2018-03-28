//------------------------------------------------------------------------------
// <copyright file="Converter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.Configuration {
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Web;

    public class Converter : ConfigurationElement {
        private static TypeConverter _whiteSpaceTrimStringConverter =
            new WhiteSpaceTrimStringConverter();

        private static ConfigurationValidatorBase _nonEmptyStringValidator =
            new StringValidator(1);

        private static readonly ConfigurationProperty _propType =
            new ConfigurationProperty("type",
                                        typeof(string),
                                        null,
                                        _whiteSpaceTrimStringConverter,
                                        _nonEmptyStringValidator,
                                        ConfigurationPropertyOptions.IsRequired);


        private static readonly ConfigurationProperty _propName =
            new ConfigurationProperty("name",
                                        typeof(string),
                                        null,
                                        _whiteSpaceTrimStringConverter,
                                        _nonEmptyStringValidator,
                                        ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);

        private static ConfigurationPropertyCollection _properties = BuildProperties();


        private static ConfigurationPropertyCollection BuildProperties() {
            ConfigurationPropertyCollection props = new ConfigurationPropertyCollection();
            props.Add(_propType);
            props.Add(_propName);
            return props;
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("type", IsRequired = true, DefaultValue = "")]
        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods",
                          Justification = "Refers to a script element, not Object.GetType()")]
        [StringValidator(MinLength = 1)]
        public string Type {
            get {
                return (string)base[_propType];
            }
            set {
                base[_propType] = value;
            }
        }

        [ConfigurationProperty("name", IsRequired = true, IsKey = true, DefaultValue = "")]
        [StringValidator(MinLength = 1)]
        public string Name {
            get {
                return (string)base[_propName];
            }
            set {
                base[_propName] = value;
            }
        }
    }
}
