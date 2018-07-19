//------------------------------------------------------------------------------
// <copyright file="SqlCacheDependencyDatabase.cs" company="Microsoft">
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
    using System.Diagnostics;
    using System.Security.Permissions;

    // class SqlCacheDependencySection

    public sealed class SqlCacheDependencyDatabase : ConfigurationElement {
        private static readonly ConfigurationElementProperty s_elemProperty = new ConfigurationElementProperty(new CallbackValidator(typeof(SqlCacheDependencyDatabase), Validate));

        private static ConfigurationPropertyCollection _properties;
        private static readonly ConfigurationProperty _propName;
        private static readonly ConfigurationProperty _propConnectionStringName;
        private static readonly ConfigurationProperty _propPollTime;

        static SqlCacheDependencyDatabase() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();

            _propName =
                new ConfigurationProperty("name",
                                            typeof(string),
                                            null,
                                            null,
                                            StdValidatorsAndConverters.NonEmptyStringValidator,
                                            ConfigurationPropertyOptions.IsRequired | 
                                            ConfigurationPropertyOptions.IsKey);
            
            _propConnectionStringName =
                new ConfigurationProperty("connectionStringName",
                                            typeof(string),
                                            null,
                                            null,
                                            StdValidatorsAndConverters.NonEmptyStringValidator,
                                            ConfigurationPropertyOptions.IsRequired);

            _propPollTime = new ConfigurationProperty("pollTime", 
                                            typeof(int), 
                                            60000, 
                                            ConfigurationPropertyOptions.None);

            _properties.Add(_propName);
            _properties.Add(_propConnectionStringName);
            _properties.Add(_propPollTime);
        }

        private int defaultPollTime;    // This may be set by the outer node to specify the default poll time (i.e. not specified on this node)

        public SqlCacheDependencyDatabase(string name, string connectionStringName, int pollTime) {
            Name = name;
            ConnectionStringName = connectionStringName;
            PollTime = pollTime;
        }

        public SqlCacheDependencyDatabase(string name, string connectionStringName) {
            Name = name;
            ConnectionStringName = connectionStringName;
        }

        internal SqlCacheDependencyDatabase() {
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }
        protected override ConfigurationElementProperty ElementProperty {
            get {
                return s_elemProperty;
            }
        }
        private static void Validate(object value) {
            if (value == null) {
                throw new ArgumentNullException("sqlCacheDependencyDatabase");
            }
            Debug.Assert(value is SqlCacheDependencyDatabase);

            SqlCacheDependencyDatabase elem = (SqlCacheDependencyDatabase)value;

            if (elem.PollTime != 0 && elem.PollTime < 500) {
                throw new ConfigurationErrorsException(
                    SR.GetString(SR.Invalid_sql_cache_dep_polltime),
                    elem.ElementInformation.Properties["pollTime"].Source, 
                    elem.ElementInformation.Properties["pollTime"].LineNumber);
            }
        }


        internal void CheckDefaultPollTime(int value) {
            // This method will be called by the outer node.
            // If the poolTime property is not specified in the node, then grab the one
            // from above.
            if (ElementInformation.Properties["pollTime"].ValueOrigin == PropertyValueOrigin.Default) {
                defaultPollTime = value;
            }
        }

        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        [StringValidator(MinLength = 1)]
        public string Name {
            get {
                return (string)base[_propName];
            }
            set {
                base[_propName] = value;
            }
        }

        [ConfigurationProperty("connectionStringName", IsRequired = true)]
        [StringValidator(MinLength = 1)]
        public string ConnectionStringName {
            get {
                return (string)base[_propConnectionStringName];
            }
            set {
                base[_propConnectionStringName] = value;
            }
        }

        [ConfigurationProperty("pollTime", DefaultValue = 60000)]
        public int PollTime {
            get {
                if (ElementInformation.Properties["pollTime"].ValueOrigin == PropertyValueOrigin.Default) {
                    return defaultPollTime;   // Return the default value from outer node
                }
                else {
                    return (int)base[_propPollTime];
                }
            }
            set {
                base[_propPollTime] = value;
            }
        }
    }
}
