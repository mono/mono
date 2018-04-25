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

    internal static class ConfigurationStrings
    {
        static string GetSectionPath(string sectionName)
        {
            return string.Format(CultureInfo.InvariantCulture, @"{0}/{1}", ConfigurationStrings.SectionGroupName, sectionName);
        }

        static internal string DiagnosticSectionPath
        {
            get { return ConfigurationStrings.GetSectionPath(ConfigurationStrings.DiagnosticSectionName); }
        }

        static internal string NetTcpSectionPath
        {
            get { return ConfigurationStrings.GetSectionPath(ConfigurationStrings.NetTcpSectionName); }
        }

        static internal string NetPipeSectionPath
        {
            get { return ConfigurationStrings.GetSectionPath(ConfigurationStrings.NetPipeSectionName); }
        }

        internal const string SectionGroupName = "system.serviceModel.activation";

        // Sid for the built-in group IIS_IUSRS for IIS7
        internal const string IIS_IUSRSSid = "S-1-5-32-568";

        internal const string DiagnosticSectionName = "diagnostics";
        internal const string NetTcpSectionName = "net.tcp";
        internal const string NetPipeSectionName = "net.pipe";

        internal const string AllowAccounts = "allowAccounts";
        internal const string Enabled = "enabled";
        internal const string ListenBacklog = "listenBacklog";
        internal const string MaxPendingAccepts = "maxPendingAccepts";
        internal const string MaxPendingConnections = "maxPendingConnections";
        internal const string PerformanceCountersEnabled = "performanceCountersEnabled";
        internal const string ReceiveTimeout = "receiveTimeout";
        internal const string SecurityIdentifier = "securityIdentifier";
        internal const string TeredoEnabled = "teredoEnabled";
        internal const string TimeSpanOneTick = "00:00:00.0000001";
        internal const string TimeSpanZero = "00:00:00";
    }
}
