//------------------------------------------------------------------------------
// <copyright file="SqlCacheDependencySection.cs" company="Microsoft">
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

    /*             <!-- sqlCacheDependency Attributes:
                enabled="[true|false]" - Enable or disable SQL cache dependency polling
                pollTime="poll time in milliseconds. Minimum is 500 ms."

              Child nodes:
                <databases>                             Database entries
                    <add                                Add a database entry
                        name="string"                   Name to identify this database entry
                        connectionStringName="string"   Connection string name to the SQL database
                        pollTime="int"                  (optional) Poll time in milliseconds

                    <remove                             Remove a database entry
                        name="string" />                Name of database entry to remove
                    <clear/>                            Remove all database entries
                </databases>

              Example:
                <databases>
                    <add name="Northwind_Local" connectionStringName="LocalNorthwind" />
                    <remove name="Pubs_Local">
                <databases>
            -->
            <sqlCacheDependency enabled="true" pollTime="60000">
                <databases>
                </databases>
            </sqlCacheDependency>
*/
    public sealed class SqlCacheDependencySection : ConfigurationSection {
        private static readonly ConfigurationElementProperty s_elemProperty = 
            new ConfigurationElementProperty(new CallbackValidator(typeof(SqlCacheDependencySection), Validate));

        private static ConfigurationPropertyCollection _properties;
        private static readonly ConfigurationProperty _propEnabled;
        private static readonly ConfigurationProperty _propPollTime;
        private static readonly ConfigurationProperty _propDatabases;

        static SqlCacheDependencySection() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();

            _propEnabled = new ConfigurationProperty("enabled", 
                                            typeof(bool), 
                                            true, 
                                            ConfigurationPropertyOptions.None);
            
            _propPollTime = new ConfigurationProperty("pollTime", 
                                            typeof(int), 
                                            60000, 
                                            ConfigurationPropertyOptions.None);

            _propDatabases = new ConfigurationProperty("databases", 
                                            typeof(SqlCacheDependencyDatabaseCollection), 
                                            null,
                                            ConfigurationPropertyOptions.IsDefaultCollection);

            _properties.Add(_propEnabled);
            _properties.Add(_propPollTime);
            _properties.Add(_propDatabases);
        }

        public SqlCacheDependencySection() {
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
                throw new ArgumentNullException("sqlCacheDependency");
            }
            Debug.Assert(value is SqlCacheDependencySection);

            SqlCacheDependencySection elem = (SqlCacheDependencySection)value;

            int defaultPollTime = elem.PollTime;

            if (defaultPollTime != 0 && defaultPollTime < 500) {
                throw new ConfigurationErrorsException(
                    SR.GetString(SR.Invalid_sql_cache_dep_polltime),
                    elem.ElementInformation.Properties["pollTime"].Source, 
                    elem.ElementInformation.Properties["pollTime"].LineNumber);
            }
        }
        
        protected override void PostDeserialize() {
            int defaultPollTime = PollTime;

            foreach (SqlCacheDependencyDatabase dbase in Databases) {
                dbase.CheckDefaultPollTime(defaultPollTime);
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

        [ConfigurationProperty("pollTime", DefaultValue = 60000)]
        public int PollTime {
            get {
                return (int)base[_propPollTime];
            }
            set {
                base[_propPollTime] = value;
            }
        }

        [ConfigurationProperty("databases")]
        public SqlCacheDependencyDatabaseCollection Databases {
            get {
                return (SqlCacheDependencyDatabaseCollection)base[_propDatabases];
            }
        }
    }
}
