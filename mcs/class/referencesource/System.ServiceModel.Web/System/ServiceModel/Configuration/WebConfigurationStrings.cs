//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------
namespace System.ServiceModel.Configuration
{
    using System;
    using System.ServiceModel.Channels;
    using System.ServiceModel;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Description;
    using System.Configuration;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.ServiceModel.Diagnostics;

    internal static class WebConfigurationStrings
    {
        internal const string DefaultBodyStyle = "defaultBodyStyle";
        internal const string HelpEnabled = "helpEnabled";
        internal const string DefaultOutgoingResponseFormat = "defaultOutgoingResponseFormat";
        internal const string AutomaticFormatSelectionEnabled = "automaticFormatSelectionEnabled";
        internal const string ContentTypeMapper = "contentTypeMapper";
        internal const string CrossDomainScriptAccessEnabled = "crossDomainScriptAccessEnabled";
        internal const string FaultExceptionEnabled = "faultExceptionEnabled";
    }
}
