//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.Runtime;
    using System.Security;
    using System.ServiceModel.Diagnostics;

    public sealed partial class DiagnosticSection : ConfigurationSection
    {
        // These three constructors are used by the configuration system. 
        public DiagnosticSection()
            : base()
        {
        }

        [ConfigurationProperty(ConfigurationStrings.WmiProviderEnabled, DefaultValue = false)]
        public bool WmiProviderEnabled
        {
            get { return (bool)base[ConfigurationStrings.WmiProviderEnabled]; }
            set { base[ConfigurationStrings.WmiProviderEnabled] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.MessageLogging, Options = ConfigurationPropertyOptions.None)]
        public MessageLoggingElement MessageLogging
        {
            get { return (MessageLoggingElement)base[ConfigurationStrings.MessageLogging]; }
        }

        [ConfigurationProperty(ConfigurationStrings.EndToEndTracing, Options = ConfigurationPropertyOptions.None)]
        public EndToEndTracingElement EndToEndTracing
        {
            get { return (EndToEndTracingElement)base[ConfigurationStrings.EndToEndTracing]; }
        }

        [ConfigurationProperty(ConfigurationStrings.PerformanceCounters, DefaultValue = PerformanceCounterScope.Default)]
        [ServiceModelEnumValidator(typeof(PerformanceCounterScopeHelper))]
        public PerformanceCounterScope PerformanceCounters
        {
            get { return (PerformanceCounterScope)base[ConfigurationStrings.PerformanceCounters]; }
            set { base[ConfigurationStrings.PerformanceCounters] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.EtwProviderId, DefaultValue = "{c651f5f6-1c0d-492e-8ae1-b4efd7c9d503}")]
        [StringValidator(MinLength = 32)]
        public string EtwProviderId
        {
            get { return (string)base[ConfigurationStrings.EtwProviderId]; }
            set { base[ConfigurationStrings.EtwProviderId] = value; }
        }

        internal static DiagnosticSection GetSection()
        {
            return (DiagnosticSection)ConfigurationHelpers.GetSection(ConfigurationStrings.DiagnosticSectionPath);
        }

        [Fx.Tag.SecurityNote(Critical = "Calls Critical method UnsafeGetSection which elevates in order to fetch config."
            + "Caller must guard access to resultant config section.")]
        [SecurityCritical]
        internal static DiagnosticSection UnsafeGetSection()
        {
            return (DiagnosticSection)ConfigurationHelpers.UnsafeGetSection(ConfigurationStrings.DiagnosticSectionPath);
        }

        [Fx.Tag.SecurityNote(Critical = "Calls Critical method UnsafeGetSection which elevates in order to fetch config."
            + "Caller must guard access to resultant config section.")]
        [SecurityCritical]
        internal static DiagnosticSection UnsafeGetSectionNoTrace()
        {
            return (DiagnosticSection)ConfigurationHelpers.UnsafeGetSectionNoTrace(ConfigurationStrings.DiagnosticSectionPath);
        }

        internal bool IsEtwProviderIdFromConfigFile()
        {
            return PropertyValueOrigin.Default != this.ElementInformation.Properties[ConfigurationStrings.EtwProviderId].ValueOrigin;
        }

    }
}



