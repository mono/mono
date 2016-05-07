//------------------------------------------------------------------------------
// <copyright file="TraceSection.cs" company="Microsoft">
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
    using System.Security.Permissions;

    /*         <!--
        trace Attributes:
          enabled="[true|false]" - Enable application tracing
          localOnly="[true|false]" - View trace results from localhost only
          pageOutput="[true|false]" - Display trace ouput on individual pages
          requestLimit="[number]" - Number of trace results available in trace.axd
          traceMode="[SortByTime|SortByCategory]" - Sorts trace result displays based on Time or Category
        -->
        <trace
            enabled="false"
            localOnly="true"
            pageOutput="false"
            requestLimit="10"
            traceMode="SortByTime"
        />
 */

    public sealed class TraceSection : ConfigurationSection {
        private static ConfigurationPropertyCollection _properties;

        private static readonly ConfigurationProperty _propEnabled =
            new ConfigurationProperty("enabled", 
                                        typeof(bool), 
                                        false, 
                                        ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _propLocalOnly =
            new ConfigurationProperty("localOnly", 
                                        typeof(bool), 
                                        true, 
                                        ConfigurationPropertyOptions.None);
        
        private static readonly ConfigurationProperty _propMostRecent =
            new ConfigurationProperty("mostRecent", 
                                        typeof(bool), 
                                        false, 
                                        ConfigurationPropertyOptions.None);
        
        private static readonly ConfigurationProperty _propPageOutput =
            new ConfigurationProperty("pageOutput", 
                                        typeof(bool), 
                                        false, 
                                        ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _propRequestLimit =
            new ConfigurationProperty("requestLimit",
                                        typeof(int),
                                        10,
                                        null,
                                        StdValidatorsAndConverters.PositiveIntegerValidator,
                                        ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _propMode =
            new ConfigurationProperty("traceMode", 
                                        typeof(TraceDisplayMode), 
                                        TraceDisplayMode.SortByTime, 
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _writeToDiagnosticTrace =
            new ConfigurationProperty("writeToDiagnosticsTrace", 
                                        typeof(bool), 
                                        false, 
                                        ConfigurationPropertyOptions.None);

        static TraceSection() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propEnabled);
            _properties.Add(_propLocalOnly);
            _properties.Add(_propMostRecent);
            _properties.Add(_propPageOutput);
            _properties.Add(_propRequestLimit);
            _properties.Add(_propMode);
            _properties.Add(_writeToDiagnosticTrace);
        }

        public TraceSection() {
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("enabled", DefaultValue = false)]
        public bool Enabled {
            get {
                return (bool)base[_propEnabled];
            }
            set {
                base[_propEnabled] = value;
            }
        }

        [ConfigurationProperty("mostRecent", DefaultValue = false)]
        public bool MostRecent {
            get {
                return (bool)base[_propMostRecent];
            }
            set {
                base[_propMostRecent] = value;
            }
        }

        [ConfigurationProperty("localOnly", DefaultValue = true)]
        public bool LocalOnly {
            get {
                return (bool)base[_propLocalOnly];
            }
            set {
                base[_propLocalOnly] = value;
            }
        }

        [ConfigurationProperty("pageOutput", DefaultValue = false)]
        public bool PageOutput {
            get {
                return (bool)base[_propPageOutput];
            }
            set {
                base[_propPageOutput] = value;
            }
        }

        [ConfigurationProperty("requestLimit", DefaultValue = 10)]
        [IntegerValidator(MinValue = 0)]
        public int RequestLimit {
            get {
                return (int)base[_propRequestLimit];
            }
            set {
                base[_propRequestLimit] = value;
            }
        }

        [ConfigurationProperty("traceMode", DefaultValue = TraceDisplayMode.SortByTime)]
        public TraceDisplayMode TraceMode {
            get {
                return (TraceDisplayMode)base[_propMode];
            }
            set {
                base[_propMode] = value;
            }
        }

        [ConfigurationProperty("writeToDiagnosticsTrace", DefaultValue = false)]
        public bool WriteToDiagnosticsTrace {
            get {
                return (bool)base[_writeToDiagnosticTrace];
            }
            set {
                base[_writeToDiagnosticTrace] = value;
            }
        }
    }
}
