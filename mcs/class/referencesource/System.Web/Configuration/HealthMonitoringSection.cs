//------------------------------------------------------------------------------
// <copyright file="HealthMonitoringSection.cs" company="Microsoft">
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

    /*
            <!--
        healthMonitoring attributes:
            heartbeatInterval="[seconds]" - A non-negative integer in seconds that details how often the WebHeartbeatEvent is raised
                                            by each application domain. Zero means no heart beat event is fired.
        -->
        <healthMonitoring
            enabled="true"
            heartbeatInterval="0">

            <bufferModes>
                <add name="Critical Notification"
                    maxBufferSize="100"
                    maxFlushSize="20"
                    urgentFlushThreshold="1"
                    regularFlushInterval="Infinite"
                    urgentFlushInterval="00:01:00"
                    maxBufferThreads="1"
                 />

                <add name="Notification"
                    maxBufferSize="300"
                    maxFlushSize="20"
                    urgentFlushThreshold="1"
                    regularFlushInterval="Infinite"
                    urgentFlushInterval="00:01:00"
                    maxBufferThreads="1"
                 />

                <add name="Analysis"
                    maxBufferSize="1000"
                    maxFlushSize="100"
                    urgentFlushThreshold="100"
                    regularFlushInterval="00:05:00"
                    urgentFlushInterval="00:01:00"
                    maxBufferThreads="1"
                 />

                <add name="Logging"
                    maxBufferSize="1000"
                    maxFlushSize="200"
                    urgentFlushThreshold="800"
                    regularFlushInterval="00:30:00"
                    urgentFlushInterval="00:05:00"
                    maxBufferThreads="1"
                 />
            </bufferModes>

            <!--
            providers attributes:
                name - Friendly name of the provider.
                type - A class that implements IProvider. The value is a fully qualified reference to an assembly.

                Other name/value pairs - Additional name value pairs may be present. It is the responsibility of the provider to
                                         understand those values.
            -->
            <providers>
                <!--
                  <add name="SqlWebEventProvider"
                    type="System.Web.Management.SqlWebEventProvider,System.Web,Version=%ASSEMBLY_VERSION%,Culture=neutral,PublicKeyToken=%MICROSOFT_PUBLICKEY%"
                    connectionStringName="Name corresponding to the entry in <connectionStrings> section where the connection string for the provider is specified"
                    maxEventDetailsLength="Maximum number of characters allowed to be logged in the Details column in the SQL table.  Default is no limit."
                    buffer="true|false (default is false)"
                    bufferMode="name of the buffer mode to use if buffer is set to true"
                  />

                  <add name="SimpleMailWebEventProvider"
                    type="System.Web.Management.SimpleMailWebEventProvider,System.Web,Version=%ASSEMBLY_VERSION%,Culture=neutral,PublicKeyToken=%MICROSOFT_PUBLICKEY%"
                    from="sender address"
                    to="semi-colon separated to addresses"
                    cc="semi-colon separated cc addresses"
                    bcc="semi-colon separated bcc addresses"
                    priority="High|Normal|Low (default is Normal)"
                    bodyHeader="Text added at the top of a message (optional)"
                    bodyFooter="Text added at the bottom of a message (optional)"
                    subjectPrefix="Text added at the beginning of the subject (optional)"
                    buffer="true|false (default is true)"
                    bufferMode="name of the buffer mode to use if buffer is set to true"
                    maxEventLength="Maximum number of characters allowed for each event in a message (optional) (default is 8K characters)"
                    maxEventsPerMessage="Maximum number of events allowed for in each message (optional) (default is 50)"
                    maxMessagesPerNotification="Maximum number of messages allowed for each notification (optional) (default is 10)"
                  />

                  <add name="TemplatedMailWebEventProvider"
                    type="System.Web.Management.TemplatedMailWebEventProvider,System.Web,Version=%ASSEMBLY_VERSION%,Culture=neutral,PublicKeyToken=%MICROSOFT_PUBLICKEY%"
                    from="sender address"
                    to="semi-colon separated to addresses"
                    cc="semi-colon separated cc addresses"
                    bcc="semi-colon separated bcc addresses"
                    priority="High|Normal|Low (default is Normal)"
                    subjectPrefix="Text added at the beginning of the subject (optional)"
                    template="The template page (.aspx) that will be used to create the message body for each notification"
                    detailedTemplateErrors="true|false (default is false)"
                    buffer="true|false (default is true)"
                    bufferMode="name of the buffer mode to use if buffer is set to true"
                    maxEventsPerMessage="Maximum number of events allowed for in each message (optional) (default is 50)"
                    maxMessagesPerNotification="Maximum number of messages allowed for each notification (optional) (default is 100)"
                  />
                -->

                <add name="EventLogProvider"
                    type="System.Web.Management.EventLogWebEventProvider,System.Web,Version=%ASSEMBLY_VERSION%,Culture=neutral,PublicKeyToken=%MICROSOFT_PUBLICKEY%"
                />

                <add name="SqlWebEventProvider"
                    type="System.Web.Management.SqlWebEventProvider,System.Web,Version=%ASSEMBLY_VERSION%,Culture=neutral,PublicKeyToken=%MICROSOFT_PUBLICKEY%"
                    connectionStringName="LocalSqlServer"
                    maxEventDetailsLength="1073741823"
                    buffer="false"
                    bufferMode="Notification"
                />

                <add name="WmiWebEventProvider"
                    type="System.Web.Management.WmiWebEventProvider,System.Web,Version=%ASSEMBLY_VERSION%,Culture=neutral,PublicKeyToken=%MICROSOFT_PUBLICKEY%"
                />

            </providers>

            <!--
            eventMappings attributes:
                name - The friendly name of the event class.
                type - The type of the event class. This can be the type of a parent class.
                startEventCode - The starting event code range.  Default is 0.
                endEventCode - The ending event code range.  Default is Int32.MaxValue.
            -->
            <eventMappings>
                <add name="All Events"
                    type="System.Web.Management.WebBaseEvent,System.Web,Version=%ASSEMBLY_VERSION%,Culture=neutral,PublicKeyToken=%MICROSOFT_PUBLICKEY%" />

                <add name="Heartbeats"
                    type="System.Web.Management.WebHeartbeatEvent,System.Web,Version=%ASSEMBLY_VERSION%,Culture=neutral,PublicKeyToken=%MICROSOFT_PUBLICKEY%" />

                <add name="Application Lifetime Events"
                    type="System.Web.Management.WebApplicationLifetimeEvent,System.Web,Version=%ASSEMBLY_VERSION%,Culture=neutral,PublicKeyToken=%MICROSOFT_PUBLICKEY%" />

                <add name="Request Processing Events"
                    type="System.Web.Management.WebRequestEvent,System.Web,Version=%ASSEMBLY_VERSION%,Culture=neutral,PublicKeyToken=%MICROSOFT_PUBLICKEY%" />

                <add name="All Errors"
                    type="System.Web.Management.WebBaseErrorEvent,System.Web,Version=%ASSEMBLY_VERSION%,Culture=neutral,PublicKeyToken=%MICROSOFT_PUBLICKEY%" />

                <add name="Infrastructure Errors"
                    type="System.Web.Management.WebErrorEvent,System.Web,Version=%ASSEMBLY_VERSION%,Culture=neutral,PublicKeyToken=%MICROSOFT_PUBLICKEY%" />

                <add name="Request Processing Errors"
                    type="System.Web.Management.WebRequestErrorEvent,System.Web,Version=%ASSEMBLY_VERSION%,Culture=neutral,PublicKeyToken=%MICROSOFT_PUBLICKEY%" />

                <add name="All Audits"
                    type="System.Web.Management.WebAuditEvent,System.Web,Version=%ASSEMBLY_VERSION%,Culture=neutral,PublicKeyToken=%MICROSOFT_PUBLICKEY%" />

                <add name="Failure Audits"
                    type="System.Web.Management.WebFailureAuditEvent,System.Web,Version=%ASSEMBLY_VERSION%,Culture=neutral,PublicKeyToken=%MICROSOFT_PUBLICKEY%" />

                <add name="Success Audits"
                    type="System.Web.Management.WebSuccessAuditEvent,System.Web,Version=%ASSEMBLY_VERSION%,Culture=neutral,PublicKeyToken=%MICROSOFT_PUBLICKEY%" />

            </eventMappings>

            <!--
            profiles attributes:
                The scope of the following attributes is per application domain.

                minInstances="[number]" - It is the minimum number of occurences of each event before it's fired.
                                          E.g. a value of 5 means that ASP.NET will not fire the event until the 5th
                                          instance of the event is raised. A value of 0 is invalid. Default is 1.

                maxLimit="[Infinite|number]" - It is the threshold after which events stop being fired. E.g. a value
                                               of 10 means ASP.NET will stop firing the event after the 10th events
                                               have been raised. Default is Infinite.

                minInterval="[Infinite|HH:MM:SS]" - It is a time interval that details the minimum duration between firing two events
                                                    of the same type.  E.g. A value of "00:01:00" means at most one event of a given
                                                    type will be thrown per minute. 00:00:00 means there is no minimum interval.
                                                    Default is 00:00:00.

                custom="[type]" - It is the type of a custom class that implements System.Web.Management.IWebEventCustomEvaluator.

            -->
            <profiles>
                <add name="Default"
                    minInstances="1"
                    maxLimit="Infinite"
                    minInterval="00:01:00"
                />

                <add name="Critical"
                    minInstances="1"
                    maxLimit="Infinite"
                    minInterval="00:00:00"
                />
            </profiles>

            <!--
            rules attributes:
                <rules>
                    <add
                        name="stinrg"       The name of the rule.
                        eventName="string"  The name of the event type, as specified in <healthEventNames>.
                        profile="string"    (Optional) The name of the profile for the event type, as specified in <healthProfiles>.
                        provider="provider" The name of the provider to be used by the event type.

                        The same <healthProfiles> attributes can also be specified to override specific settings in the profile.

                        />

                    <remove              Remove an entry
                        name="string" /> Name of the entry
                    <clear/>             Remove all entries
                </rules>
            -->
            <rules>
                <add name="All Errors Default"
                    eventName="All Errors"
                    provider="EventLogProvider"
                    profile="Default"
                    minInterval="00:01:00" />

                <add name="Failure Audits Default"
                    eventName="Failure Audits"
                    provider="EventLogProvider"
                    profile="Default"
                    minInterval="00:01:00" />
            </rules>

        </healthMonitoring>

    */

    public sealed class HealthMonitoringSection : ConfigurationSection {
        const int MAX_HEARTBEAT_VALUE = Int32.MaxValue / 1000;      // in sec; this value will be converted to ms and passed to Timer ctor, which takes a ms param
        const bool DEFAULT_HEALTH_MONITORING_ENABLED = true;
        const int DEFAULT_HEARTBEATINTERVAL = 0;  // This was Zero in Machine.config and 60 in here

        private static ConfigurationPropertyCollection _properties;

        private static readonly ConfigurationProperty _propHeartbeatInterval =
            new ConfigurationProperty("heartbeatInterval",
                                        typeof(TimeSpan),
                                        TimeSpan.FromSeconds((long)DEFAULT_HEARTBEATINTERVAL),
                                        StdValidatorsAndConverters.TimeSpanSecondsConverter,
                                        new TimeSpanValidator(TimeSpan.Zero, TimeSpan.FromSeconds(MAX_HEARTBEAT_VALUE)),
                                        ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _propEnabled =
            new ConfigurationProperty("enabled", 
                                        typeof(bool), 
                                        DEFAULT_HEALTH_MONITORING_ENABLED, 
                                        ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _propBufferModes =
            new ConfigurationProperty("bufferModes", 
                                        typeof(BufferModesCollection), 
                                        null, 
                                        ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _propProviders =
            new ConfigurationProperty("providers", 
                                        typeof(ProviderSettingsCollection), 
                                        null, 
                                        ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _propProfileSettingsCollection =
            new ConfigurationProperty("profiles", 
                                        typeof(ProfileSettingsCollection), 
                                        null, 
                                        ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _propRuleSettingsCollection =
            new ConfigurationProperty("rules", 
                                        typeof(RuleSettingsCollection), 
                                        null, 
                                        ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _propEventMappingSettingsCollection =
            new ConfigurationProperty("eventMappings", 
                                        typeof(EventMappingSettingsCollection), 
                                        null, 
                                        ConfigurationPropertyOptions.None);

        static HealthMonitoringSection() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propHeartbeatInterval);
            _properties.Add(_propEnabled);
            _properties.Add(_propBufferModes);
            _properties.Add(_propProviders);
            _properties.Add(_propProfileSettingsCollection);
            _properties.Add(_propRuleSettingsCollection);
            _properties.Add(_propEventMappingSettingsCollection);
        }

        public HealthMonitoringSection() {
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("heartbeatInterval", DefaultValue = "00:00:00" /* DEFAULT_HEARTBEATINTERVAL */)]
        [TypeConverter(typeof(TimeSpanSecondsConverter))]
        [TimeSpanValidator(MinValueString = "00:00:00", MaxValueString = "24.20:31:23")]
        public TimeSpan HeartbeatInterval {
            get {
                return (TimeSpan)base[_propHeartbeatInterval];
            }
            set {
                base[_propHeartbeatInterval] = value;
            }
        }

        [ConfigurationProperty("enabled", DefaultValue = DEFAULT_HEALTH_MONITORING_ENABLED)]
        public bool Enabled {
            get {
                return (bool)base[_propEnabled];
            }
            set {
                base[_propEnabled] = value;
            }
        }

        [ConfigurationProperty("bufferModes")]
        public BufferModesCollection BufferModes {
            get {
                return (BufferModesCollection)base[_propBufferModes];
            }
        }

        [ConfigurationProperty("providers")]
        public ProviderSettingsCollection Providers {
            get {
                return (ProviderSettingsCollection)base[_propProviders];
            }
        }

        [ConfigurationProperty("profiles")]
        public ProfileSettingsCollection Profiles {
            get {
                return (ProfileSettingsCollection)base[_propProfileSettingsCollection];
            }
        }

        [ConfigurationProperty("rules")]
        public RuleSettingsCollection Rules {
            get {
                return (RuleSettingsCollection)base[_propRuleSettingsCollection];
            }
        }

        [ConfigurationProperty("eventMappings")]
        public EventMappingSettingsCollection EventMappings {
            get {
                return (EventMappingSettingsCollection)base[_propEventMappingSettingsCollection];
            }
        }
    } // class HealthMonitoringSection 
}
