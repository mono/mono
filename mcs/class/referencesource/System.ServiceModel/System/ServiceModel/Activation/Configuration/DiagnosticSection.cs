//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Activation.Configuration
{
    using System;
    using System.Configuration;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;

    public sealed partial class DiagnosticSection : ConfigurationSection
    {
        public DiagnosticSection()
            : base()
        {
        }

        static internal DiagnosticSection GetSection()
        {
            DiagnosticSection retval = (DiagnosticSection)ConfigurationManager.GetSection(ConfigurationStrings.DiagnosticSectionPath);
            if (retval == null)
            {
                retval = new DiagnosticSection();
            }
            return retval;
        }

        [ConfigurationProperty(ConfigurationStrings.PerformanceCountersEnabled, DefaultValue = ListenerConstants.DefaultPerformanceCountersEnabled)]
        public bool PerformanceCountersEnabled
        {
            get { return (bool)base[ConfigurationStrings.PerformanceCountersEnabled]; }
            set { base[ConfigurationStrings.PerformanceCountersEnabled] = value; }
        }
    }
}
