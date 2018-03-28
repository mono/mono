//------------------------------------------------------------------------------
// <copyright file="EventMappingSettings.cs" company="Microsoft">
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
    using System.Web.Hosting;
    using System.Web.Util;
    using System.Web.Configuration;
    using System.Web.Management;
    using System.Web.Compilation;
    using System.Security.Permissions;

    public sealed class EventMappingSettings : ConfigurationElement {
        private static ConfigurationPropertyCollection _properties;

        private static readonly ConfigurationProperty _propName =
            new ConfigurationProperty("name",
                                        typeof(string),
                                        null,
                                        null,
                                        StdValidatorsAndConverters.NonEmptyStringValidator,
                                        ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);

        private static readonly ConfigurationProperty _propType = new ConfigurationProperty("type", typeof(string), String.Empty, ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsTypeStringTransformationRequired);

        private static readonly ConfigurationProperty _propStartEventCode =
            new ConfigurationProperty("startEventCode",
                                        typeof(int),
                                        0,
                                        null,
                                        StdValidatorsAndConverters.PositiveIntegerValidator,
                                        ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _propEndEventCode =
            new ConfigurationProperty("endEventCode",
                                        typeof(int),
                                        int.MaxValue,
                                        null,
                                        StdValidatorsAndConverters.PositiveIntegerValidator,
                                        ConfigurationPropertyOptions.None);

        Type _type;  // The real type

        internal Type RealType {
            get { Debug.Assert(_type != null, "_type != null"); return _type; }
            set { _type = value; }
        }

        static EventMappingSettings() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propName);
            _properties.Add(_propType);
            _properties.Add(_propStartEventCode);
            _properties.Add(_propEndEventCode);
        }

        internal EventMappingSettings() {
        }

        public EventMappingSettings(String name, String type, int startEventCode, int endEventCode)
            : this() {
            Name = name;
            Type = type;
            StartEventCode = startEventCode;
            EndEventCode = endEventCode;
        }


        public EventMappingSettings(String name, String type)
            : this() // Do not call the constructor which sets the event codes
        {                                                              // or the values will be persisted as it the user set them
            Name = name;
            Type = type;
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("name", IsRequired = true, IsKey = true, DefaultValue = "")]
        public String Name {
            get {
                return (string)base[_propName];
            }
            set {
                base[_propName] = value;
            }
        }

        [ConfigurationProperty("type", IsRequired = true, DefaultValue = "")]
        public String Type {
            get {
                return (string)base[_propType];
            }
            set {
                base[_propType] = value;
            }
        }

        [ConfigurationProperty("startEventCode", DefaultValue = 0)]
        [IntegerValidator(MinValue = 0)]
        public int StartEventCode {
            get {
                return (int)base[_propStartEventCode];
            }
            set {
                base[_propStartEventCode] = value;
            }
        }

        [ConfigurationProperty("endEventCode", DefaultValue = int.MaxValue)]
        [IntegerValidator(MinValue = 0)]
        public int EndEventCode {
            get {
                return (int)base[_propEndEventCode];
            }
            set {
                base[_propEndEventCode] = value;
            }
        }
    }
}
